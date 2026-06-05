using System.Threading.Channels;

using Enyim.Iris.Server.Contracts;
using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Protocol;
using Enyim.Iris.Server.Sessions;
using Enyim.Iris.Server.Transport;

using ChromeProtocol.Domains;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Enyim.Iris.Server.Tests;

public class SessionHubTests
{
	private static CdpSession MakeSession(ICdpSessionHub hub, string id = "test")
	{
		var registry = new CdpCommandRegistry();
		registry.Fallback = _ => new ValueTask<CdpResult>(CdpResult.Ok());
		var dispatcher = new CdpDispatcher(registry);
		var services = new ServiceCollection().BuildServiceProvider();
		var connection = new NullCdpConnection();

		return new CdpSession(
			connection: connection,
			dispatcher: dispatcher,
			index: CdpContractIndex.Default,
			scopeFactory: services.GetRequiredService<IServiceScopeFactory>(),
			logger: NullLogger<CdpSession>.Instance,
			connectionId: id,
			hub: hub);
	}

	[Fact]
	public async Task Broadcast_delivers_to_all_registered_sessions()
	{
		var hub = new CdpSessionHub();

		var s1 = MakeSession(hub, "s1");
		var s2 = MakeSession(hub, "s2");

		// Manually register (normally done in RunAsync).
		hub.Register(s1);
		hub.Register(s2);

		// Runtime is gated; enable it on both sessions so the event isn't suppressed.
		s1.EnableDomainForTest("Runtime");
		s2.EnableDomainForTest("Runtime");

		var evt = new Runtime.ExecutionContextCreated(
			new Runtime.ExecutionContextDescriptionType(
				new Runtime.ExecutionContextIdType(1), "://", "ctx", "u1"));

		await hub.BroadcastAsync(evt);

		Assert.True(s1.HasPendingOutput(), "s1 should have received the broadcast");
		Assert.True(s2.HasPendingOutput(), "s2 should have received the broadcast");
	}

	[Fact]
	public async Task Broadcast_respects_domain_gating()
	{
		var hub = new CdpSessionHub();
		var s1 = MakeSession(hub, "s1");
		var s2 = MakeSession(hub, "s2");

		hub.Register(s1);
		hub.Register(s2);

		// Only s1 enables Runtime; s2 should not receive the event.
		s1.EnableDomainForTest("Runtime");

		var evt = new Runtime.ExecutionContextCreated(
			new Runtime.ExecutionContextDescriptionType(
				new Runtime.ExecutionContextIdType(1), "://", "ctx", "u1"));

		await hub.BroadcastAsync(evt);

		Assert.True(s1.HasPendingOutput(), "s1 (enabled) should receive the event");
		Assert.False(s2.HasPendingOutput(), "s2 (not enabled) should be gated");
	}

	[Fact]
	public async Task Unregistered_session_receives_nothing()
	{
		var hub = new CdpSessionHub();
		var s1 = MakeSession(hub, "s1");

		hub.Register(s1);
		hub.Unregister(s1);

		s1.EnableDomainForTest("Runtime");

		await hub.BroadcastAsync(new Runtime.ExecutionContextCreated(
			new Runtime.ExecutionContextDescriptionType(
				new Runtime.ExecutionContextIdType(1), "://", "ctx", "u1")));

		Assert.False(s1.HasPendingOutput(), "unregistered session should not receive events");
	}

	// --- helpers ---

	private sealed class NullCdpConnection : ICdpConnection
	{
		public ValueTask<CdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken) =>
			new(new CdpReceiveResult(default, IsClosed: true));

		public ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken) =>
			ValueTask.CompletedTask;

		public ValueTask CloseAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;
	}
}

/// <summary>Test helpers that expose internal session state without making it public API.</summary>
internal static class CdpSessionTestExtensions
{
	public static void EnableDomainForTest(this CdpSession session, string domain)
	{
		// Access the domain state via the internal field reflection to avoid polluting public API.
		var field = typeof(CdpSession).GetField("_domainState",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
		var state = (CdpDomainState)field.GetValue(session)!;
		state.Enable(domain);
	}

	public static bool HasPendingOutput(this CdpSession session)
	{
		var field = typeof(CdpSession).GetField("_outbound",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
		var channel = (Channel<Iris.Server.Protocol.CdpOutgoingMessage>)field.GetValue(session)!;
		return channel.Reader.Count > 0;
	}
}
