using System.Text.Json;

using ChromeProtocol.Core;

using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Events;
using Enyim.Iris.Server.Sessions;

namespace Enyim.Iris.Server.Tests;

internal sealed class NullEmitter : ICdpEventEmitter
{
	public static readonly NullEmitter Instance = new();

	public ValueTask EmitAsync(IEvent evt, string? sessionId = null, CancellationToken cancellationToken = default) =>
		ValueTask.CompletedTask;
}

internal sealed class EmptyServiceProvider : IServiceProvider
{
	public static readonly EmptyServiceProvider Instance = new();

	public object? GetService(Type serviceType) => null;
}

internal static class TestContext
{
	public static JsonElement Json(string raw) => JsonDocument.Parse(raw).RootElement.Clone();

	public static JsonElement NoParams => default;

	public static CdpCommandContext Create(
		string method,
		JsonElement @params = default,
		ICdpClientConnection? connection = null,
		ICdpEventEmitter? events = null)
	{
		connection ??= new TestConnection();
		return new CdpCommandContext
		{
			Method = method,
			Params = @params,
			Connection = connection,
			Events = events ?? NullEmitter.Instance,
			Services = EmptyServiceProvider.Instance,
			CancellationToken = CancellationToken.None,
			SessionId = connection.SessionId,
		};
	}
}
