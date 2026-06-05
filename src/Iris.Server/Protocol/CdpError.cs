namespace Enyim.Iris.Server.Protocol;

/// <summary>Standard JSON-RPC / CDP error codes.</summary>
public enum CdpErrorCode
{
	/// <summary>Invalid JSON was received.</summary>
	ParseError = -32700,

	/// <summary>The JSON sent is not a valid request object.</summary>
	InvalidRequest = -32600,

	/// <summary>The method does not exist or is not available.</summary>
	MethodNotFound = -32601,

	/// <summary>Invalid method parameters.</summary>
	InvalidParams = -32602,

	/// <summary>Internal error while handling the request.</summary>
	InternalError = -32603,

	/// <summary>Generic server error (CDP uses this for most domain failures).</summary>
	ServerError = -32000,
}

/// <summary>
/// A CDP error object, serialized as <c>{ "code": n, "message": "...", "data": "..." }</c>.
/// </summary>
public sealed record CdpError(int Code, string Message, string? Data = null)
{
	public static CdpError ParseError(string? data = null) =>
		new((int)CdpErrorCode.ParseError, "Message must be a valid JSON", data);

	public static CdpError InvalidRequest(string message, string? data = null) =>
		new((int)CdpErrorCode.InvalidRequest, message, data);

	public static CdpError MethodNotFound(string method) =>
		new((int)CdpErrorCode.MethodNotFound, $"'{method}' wasn't found");

	public static CdpError InvalidParams(string message, string? data = null) =>
		new((int)CdpErrorCode.InvalidParams, $"Invalid parameters", data ?? message);

	public static CdpError InternalError(string message) =>
		new((int)CdpErrorCode.InternalError, message);

	public static CdpError ServerError(string message, string? data = null) =>
		new((int)CdpErrorCode.ServerError, message, data);
}

/// <summary>
/// Thrown by command handlers to return a structured CDP error to the client.
/// Any other exception is mapped to <see cref="CdpErrorCode.ServerError"/> by the dispatcher.
/// </summary>
public sealed class CdpProtocolException : Exception
{
	public CdpError Error { get; }

	public CdpProtocolException(CdpError error) : base(error.Message) => Error = error;

	public CdpProtocolException(string message, CdpErrorCode code = CdpErrorCode.ServerError)
		: this(new CdpError((int)code, message)) { }
}
