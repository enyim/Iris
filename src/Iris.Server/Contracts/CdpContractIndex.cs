using System.Collections.Frozen;
using System.Reflection;

using ChromeProtocol.Core;

namespace Enyim.Iris.Server.Contracts;

/// <summary>Metadata about a CDP command derived from the generated <c>ChromeProtocol.Domains</c> types.</summary>
/// <param name="Method">The wire method name, e.g. <c>"Browser.getVersion"</c>.</param>
/// <param name="ParamsType">The command request record (also the params shape).</param>
/// <param name="ResultType">The command result record (the <c>ICommand&lt;T&gt;</c> generic argument).</param>
public sealed record CdpCommandDescriptor(string Method, Type ParamsType, Type ResultType);

/// <summary>
/// Reflects the generated <c>ChromeProtocol.Domains</c> assembly to map CDP method names to the
/// CLR types that represent their params/results (commands) and payloads (events).
/// </summary>
/// <remarks>
/// Generated command records carry <c>[MethodName("Domain.command")]</c> and implement
/// <see cref="ICommand{TResponse}"/>; events implement <see cref="IEvent"/> with the same attribute.
/// This index is built once and is safe to share.
/// </remarks>
public sealed class CdpContractIndex
{
	private readonly FrozenDictionary<string, CdpCommandDescriptor> _commandsByMethod;
	private readonly FrozenDictionary<Type, string> _methodByType;
	private readonly FrozenSet<string> _gatedDomains;

	private CdpContractIndex(
		FrozenDictionary<string, CdpCommandDescriptor> commandsByMethod,
		FrozenDictionary<Type, string> methodByType,
		FrozenSet<string> gatedDomains)
	{
		_commandsByMethod = commandsByMethod;
		_methodByType = methodByType;
		_gatedDomains = gatedDomains;
	}

	/// <summary>
	/// Domains that define an <c>enable</c> command, and therefore gate their events behind it.
	/// Events for these domains are only delivered to connections that have enabled them.
	/// </summary>
	public IReadOnlySet<string> GatedDomains => _gatedDomains;

	/// <summary>True if events for <paramref name="domain"/> require the domain to be enabled.</summary>
	public bool IsGatedDomain(string domain) => _gatedDomains.Contains(domain);

	/// <summary>Splits a CDP method like <c>"Runtime.enable"</c> into (<c>"Runtime"</c>, <c>"enable"</c>).</summary>
	public static (string Domain, string Command) SplitMethod(string method)
	{
		var dot = method.IndexOf('.');
		return dot < 0 ? (method, String.Empty) : (method[..dot], method[(dot + 1)..]);
	}

	/// <summary>The index built from the official <c>ChromeProtocol.Domains</c> assembly.</summary>
	public static CdpContractIndex Default { get; } =
		FromAssembly(typeof(ChromeProtocol.Domains.Browser).Assembly);

	public IReadOnlyCollection<CdpCommandDescriptor> Commands => _commandsByMethod.Values;

	public bool TryGetCommand(string method, out CdpCommandDescriptor descriptor) =>
		_commandsByMethod.TryGetValue(method, out descriptor!);

	/// <summary>Returns the wire method name for a generated command or event type.</summary>
	public string GetMethodName(Type type)
	{
		if (_methodByType.TryGetValue(type, out var method))
			return method;
		return ReadMethodNameAttribute(type)
			   ?? throw new InvalidOperationException(
				   $"Type '{type}' is not a known CDP command/event and has no [MethodName] attribute.");
	}

	/// <summary>Reads the <c>[MethodName]</c> attribute directly off a type, without the index.</summary>
	public static string? ReadMethodNameAttribute(Type type) =>
		type.GetCustomAttribute<MethodNameAttribute>()?.MethodName;

	public static CdpContractIndex FromAssembly(Assembly assembly)
	{
		var commands = new Dictionary<string, CdpCommandDescriptor>(StringComparer.Ordinal);
		var methodByType = new Dictionary<Type, string>();
		var gatedDomains = new HashSet<string>(StringComparer.Ordinal);

		foreach (var type in assembly.GetTypes())
		{
			if (type.IsInterface || type.IsAbstract)
				continue;

			var method = ReadMethodNameAttribute(type);
			if (method is null)
				continue;

			if (typeof(ICommand).IsAssignableFrom(type))
			{
				var resultType = ResolveResultType(type);
				commands[method] = new CdpCommandDescriptor(method, type, resultType);
				methodByType[type] = method;

				var (domain, command) = SplitMethod(method);
				if (command.Equals("enable", StringComparison.Ordinal))
					gatedDomains.Add(domain);
			}
			else if (typeof(IEvent).IsAssignableFrom(type))
			{
				methodByType[type] = method;
			}
		}

		return new CdpContractIndex(
			commands.ToFrozenDictionary(StringComparer.Ordinal),
			methodByType.ToFrozenDictionary(),
			gatedDomains.ToFrozenSet(StringComparer.Ordinal));
	}

	private static Type ResolveResultType(Type commandType)
	{
		foreach (var iface in commandType.GetInterfaces())
		{
			if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ICommand<>))
				return iface.GetGenericArguments()[0];
		}
		return typeof(object);
	}
}
