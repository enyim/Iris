namespace Enyim.Iris.Server.Protocol;

/// <summary>
/// The outcome of dispatching a command: either a result payload (which may be <c>null</c> for
/// void commands, serialized as <c>{}</c>) or a <see cref="CdpError"/>.
/// </summary>
public readonly record struct CdpResult(object? Result, CdpError? Error)
{
	public bool IsError => Error is not null;

	public static CdpResult Ok(object? result = null) => new(result, null);
	public static CdpResult Fail(CdpError error) => new(null, error);
}
