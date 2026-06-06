using Enyim.Iris.Server.Contracts;
using Enyim.Iris.Server.Events;
using Enyim.Iris.Server.Protocol;
using Enyim.Iris.Server.Sessions;

using Enyim.Iris.Protocol;

namespace Enyim.Iris.Server.Tests;

public class EventEmitterTests
{
	private static Runtime.ExecutionContextCreated SampleEvent() =>
		new(new Runtime.ExecutionContextDescriptionType(
			new Runtime.ExecutionContextIdType(1), "://", "ctx", "u1"));

	[Fact]
	public async Task Gated_event_is_suppressed_until_domain_enabled()
	{
		var connection = new TestConnection();
		var state = new CdpDomainState();
		var emitter = new CdpEventEmitter(connection, state, CdpContractIndex.Default);

		await emitter.EmitAsync(SampleEvent(), cancellationToken: Xunit.TestContext.Current.CancellationToken);
		Assert.Empty(connection.Sent);

		state.Enable("Runtime");
		await emitter.EmitAsync(SampleEvent(), cancellationToken: Xunit.TestContext.Current.CancellationToken);

		var message = Assert.IsType<CdpEventMessage>(Assert.Single(connection.Sent));
		Assert.Equal("Runtime.executionContextCreated", message.Method);
	}

	[Fact]
	public async Task Ungated_domain_events_are_always_delivered()
	{
		var connection = new TestConnection();
		var emitter = new CdpEventEmitter(connection, new CdpDomainState(), CdpContractIndex.Default);

		// Target is not gated (no Target.enable command).
		await emitter.EmitAsync(new Target.TargetCreated(
			new Target.TargetInfoType(new Target.TargetIDType("t1"), "page", "", "", true, false)), cancellationToken: Xunit.TestContext.Current.CancellationToken);

		Assert.Single(connection.Sent);
	}

	[Fact]
	public async Task Session_id_falls_back_to_connection_session()
	{
		var connection = new TestConnection(sessionId: "SES-1");
		var state = new CdpDomainState();
		state.Enable("Runtime");
		var emitter = new CdpEventEmitter(connection, state, CdpContractIndex.Default);

		await emitter.EmitAsync(SampleEvent(), cancellationToken: Xunit.TestContext.Current.CancellationToken);

		var message = Assert.IsType<CdpEventMessage>(Assert.Single(connection.Sent));
		Assert.Equal("SES-1", message.SessionId);
	}
}
