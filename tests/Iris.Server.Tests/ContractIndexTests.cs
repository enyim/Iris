using Enyim.Iris.Server.Contracts;

using Enyim.Iris.Protocol;

namespace Enyim.Iris.Server.Tests;

public class ContractIndexTests
{
	private readonly CdpContractIndex index = CdpContractIndex.Default;

	[Fact]
	public void Maps_command_type_to_method_name()
	{
		Assert.Equal("Browser.getVersion", index.GetMethodName(typeof(Browser.GetVersionRequest)));
	}

	[Fact]
	public void Maps_event_type_to_method_name()
	{
		Assert.Equal("Runtime.executionContextCreated",
			index.GetMethodName(typeof(Runtime.ExecutionContextCreated)));
	}

	[Fact]
	public void Resolves_command_descriptor_with_result_type()
	{
		Assert.True(index.TryGetCommand("Browser.getVersion", out var descriptor));
		Assert.Equal(typeof(Browser.GetVersionRequest), descriptor.ParamsType);
		Assert.Equal(typeof(Browser.GetVersionRequestResult), descriptor.ResultType);
	}

	[Fact]
	public void Domains_with_enable_command_are_gated()
	{
		Assert.True(index.IsGatedDomain("Runtime"));
		Assert.True(index.IsGatedDomain("Page"));
		// Target has no enable command, so its events are not gated.
		Assert.False(index.IsGatedDomain("Target"));
	}

	[Theory]
	[InlineData("Runtime.enable", "Runtime", "enable")]
	[InlineData("Page.getFrameTree", "Page", "getFrameTree")]
	[InlineData("NoDot", "NoDot", "")]
	public void SplitMethod_splits_on_first_dot(string method, string domain, string command)
	{
		var (d, c) = CdpContractIndex.SplitMethod(method);
		Assert.Equal(domain, d);
		Assert.Equal(command, c);
	}
}
