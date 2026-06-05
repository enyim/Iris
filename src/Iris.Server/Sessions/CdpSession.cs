using System.Buffers;
using System.Threading.Channels;

using Enyim.Iris.Server.Contracts;
using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Events;
using Enyim.Iris.Server.Protocol;
using Enyim.Iris.Server.Transport;

using ChromeProtocol.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Enyim.Iris.Server.Sessions;

/// <summary>
/// Drives one CDP client connection: a read loop that parses and dispatches incoming commands, an
/// outbound <see cref="Channel{T}"/> drained by a single writer task (so all socket writes happen
/// on one thread), and per-command DI scopes. Commands are dispatched concurrently; responses and
/// events are serialized through the writer.
/// </summary>
public sealed class CdpSession : ICdpClientConnection, IAsyncDisposable
{
	private readonly ICdpConnection _connection;
	private readonly ICdpDispatcher _dispatcher;
	private readonly CdpContractIndex _index;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger _logger;
	private readonly Channel<CdpOutgoingMessage> _outbound;
	private readonly CdpDomainState _domainState = new();
	private readonly CdpEventEmitter _emitter;
	private readonly ICdpSessionHub? _hub;
	private readonly CancellationTokenSource _cts = new();

	private readonly object _inflightGate = new();
	private readonly HashSet<Task> _inflight = [];

	public string ConnectionId { get; }
	public string? SessionId { get; }

	public CdpSession(
		ICdpConnection connection,
		ICdpDispatcher dispatcher,
		CdpContractIndex index,
		IServiceScopeFactory scopeFactory,
		ILogger<CdpSession> logger,
		string connectionId,
		string? sessionId = null,
		int outboundCapacity = 1024,
		ICdpSessionHub? hub = null)
	{
		_connection = connection;
		_dispatcher = dispatcher;
		_index = index;
		_scopeFactory = scopeFactory;
		_logger = logger;
		_hub = hub;
		ConnectionId = connectionId;
		SessionId = sessionId;
		_outbound = Channel.CreateBounded<CdpOutgoingMessage>(new BoundedChannelOptions(outboundCapacity)
		{
			SingleReader = true,
			SingleWriter = false,
			FullMode = BoundedChannelFullMode.DropNewest,
		});
		_emitter = new CdpEventEmitter(this, _domainState, _index);
	}

	/// <summary>
	/// Best-effort event delivery from the broadcast hub. Uses <c>TryWrite</c> so a full outbound
	/// buffer drops the event rather than blocking the hub (and other sessions or the app thread).
	/// Domain gating is still enforced: suppressed events return <c>true</c>.
	/// </summary>
	internal bool TryEmit(IEvent evt)
	{
		var method = _index.GetMethodName(evt.GetType());
		var (domain, _) = CdpContractIndex.SplitMethod(method);
		if (_index.IsGatedDomain(domain) && !_domainState.IsEnabled(domain))
			return true;
		return _outbound.Writer.TryWrite(new CdpEventMessage(method, evt, SessionId));
	}

	public ValueTask EnqueueAsync(CdpOutgoingMessage message, CancellationToken cancellationToken = default) =>
		_outbound.Writer.WriteAsync(message, cancellationToken);

	/// <summary>Runs the session until the client disconnects or <paramref name="cancellationToken"/> fires.</summary>
	public async Task RunAsync(CancellationToken cancellationToken)
	{
		_hub?.Register(this);
		using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
		var token = linked.Token;

		var writer = Task.Run(() => WriteLoopAsync(token), CancellationToken.None);
		try
		{
			await ReadLoopAsync(token).ConfigureAwait(false);
		}
		finally
		{
			_hub?.Unregister(this);
			await DrainInflightAsync().ConfigureAwait(false);
			_outbound.Writer.TryComplete();
			await writer.ConfigureAwait(false);
		}
	}

	private async Task ReadLoopAsync(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			CdpReceiveResult received;
			try
			{
				received = await _connection.ReceiveAsync(token).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				break;
			}

			if (received.IsClosed)
				break;

			var parse = CdpWireParser.Parse(received.Payload);
			if (parse.IsError)
			{
				if (parse.Id is long id)
					await EnqueueAsync(new CdpErrorMessage(id, parse.Error!), token).ConfigureAwait(false);
				else
					_logger.LogWarning("[{ConnectionId}] Dropping unparseable message: {Error}",
						ConnectionId, parse.Error!.Message);
				continue;
			}

			DispatchInBackground(parse.Message, token);
		}
	}

	private void DispatchInBackground(CdpIncomingMessage message, CancellationToken token)
	{
		var task = ProcessAsync(message, token);
		if (task.IsCompleted)
		{
			ObserveCompletion(task, message.Method);
			return;
		}

		lock (_inflightGate)
			_inflight.Add(task);

		_ = task.ContinueWith(
			(t, state) =>
			{
				var (session, method) = ((CdpSession, string))state!;
				lock (session._inflightGate)
					session._inflight.Remove(t);
				session.ObserveCompletion(t, method);
			},
			(this, message.Method),
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.Default);
	}

	private void ObserveCompletion(Task task, string method)
	{
		if (task.IsFaulted)
			_logger.LogError(task.Exception, "[{ConnectionId}] Command processing failed for {Method}",
				ConnectionId, method);
	}

	private async Task ProcessAsync(CdpIncomingMessage message, CancellationToken token)
	{
		await using var scope = _scopeFactory.CreateAsyncScope();

		var context = new CdpCommandContext
		{
			Method = message.Method,
			Params = message.Params,
			Connection = this,
			Events = _emitter,
			Services = scope.ServiceProvider,
			CancellationToken = token,
			SessionId = message.SessionId,
		};

		// Enable optimistically *before* dispatch so an enable handler can immediately emit events
		// (which are gated on the domain being enabled); revert if the handler fails.
		var (domain, command) = CdpContractIndex.SplitMethod(message.Method);
		var isEnable = command.Equals("enable", StringComparison.Ordinal);
		if (isEnable)
			_domainState.Enable(domain);

		var result = await _dispatcher.DispatchAsync(context).ConfigureAwait(false);

		if (result.IsError)
		{
			if (isEnable)
				_domainState.Disable(domain);
		}
		else if (command.Equals("disable", StringComparison.Ordinal))
		{
			_domainState.Disable(domain);
		}

		if (result.IsError)
			_logger.LogInformation("[{ConnectionId}] CMD {Method} -> error {Code}: {Message}",
				ConnectionId, message.Method, result.Error!.Code, result.Error.Message);
		else
			_logger.LogInformation("[{ConnectionId}] CMD {Method} -> ok", ConnectionId, message.Method);

		if (message.Id is not long id)
			return;

		CdpOutgoingMessage response = result.IsError
			? new CdpErrorMessage(id, result.Error!, message.SessionId)
			: new CdpResultMessage(id, result.Result, message.SessionId);

		await EnqueueAsync(response, token).ConfigureAwait(false);
	}

	private async Task WriteLoopAsync(CancellationToken token)
	{
		var buffer = new ArrayBufferWriter<byte>(initialCapacity: 4096);
		try
		{
			await foreach (var message in _outbound.Reader.ReadAllAsync(token).ConfigureAwait(false))
			{
				buffer.ResetWrittenCount();
				message.WriteTo(buffer, CdpJson.Payload);
				await _connection.SendAsync(buffer.WrittenMemory, token).ConfigureAwait(false);
			}
		}
		catch (OperationCanceledException)
		{
			// Shutting down.
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[{ConnectionId}] Outbound write loop terminated unexpectedly", ConnectionId);
		}
	}

	private async Task DrainInflightAsync()
	{
		Task[] pending;
		lock (_inflightGate)
			pending = [.. _inflight];

		if (pending.Length == 0)
			return;

		try
		{
			await Task.WhenAll(pending).ConfigureAwait(false);
		}
		catch
		{
			// Individual failures are already logged by ObserveCompletion.
		}
	}

	public async ValueTask DisposeAsync()
	{
		await _cts.CancelAsync().ConfigureAwait(false);
		_cts.Dispose();
		await _connection.DisposeAsync().ConfigureAwait(false);
	}
}
