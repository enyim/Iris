# DebugServer

A **server** implementation of the [Chrome DevTools Protocol](https://chromedevtools.github.io/devtools-protocol/)
in .NET 10 / C# latest. It provides the plumbing — transport, JSON-RPC dispatch, sessions, events,
target discovery — that a CDP client (DevTools, Puppeteer, Playwright, `chrome-remote-interface`,
the `ChromeProtocol` client library) connects to. Domain *behavior* is left to handlers you supply.

Strongly-typed contracts (615 commands, 198 events) are **reused** from
[`seclerp/dotnet-chrome-protocol`](https://github.com/seclerp/dotnet-chrome-protocol)
(`ChromeProtocol.Domains` + `ChromeProtocol.Core`); only the server half is implemented here.

## Projects

| Project | Role |
| --- | --- |
| `src/Cdp.Server` | Core framework: wire protocol, dispatch, sessions, events, targets, transport abstraction. No ASP.NET dependency. |
| `src/Cdp.Server.AspNetCore` | Kestrel hosting: `AddCdpServer()`, `MapCdpServer()`, the `/json/*` discovery endpoints, and the WebSocket transport adapter. |
| `samples/Cdp.Sample.BrowserEmulator` | A runnable host with stub handlers a real CDP client can connect to. |
| `tests/Cdp.Server.Tests` | Unit tests + an in-memory WebSocket integration test. |

## How it works

```
WebSocket ─▶ WebSocketCdpConnection ─▶ CdpSession ─▶ CdpDispatcher ─▶ handler
                                          │ read loop parses frames (CdpWireParser)
                                          │ concurrent dispatch, per-command DI scope
                                          ▼ single writer task drains an outbound Channel
                                       ICdpConnection.SendAsync
```

- **Contracts.** `CdpContractIndex` reflects `ChromeProtocol.Domains`: commands carry
  `[MethodName("Domain.command")]` and implement `ICommand<TResult>`, so the method name and result
  type are discovered without instantiation. The records already have camelCase `[JsonPropertyName]`,
  so `System.Text.Json` (de)serializes them as-is.
- **Dispatch.** `ICdpCommandRegistry` maps method → handler. Register inline
  (`MapCommand<TParams, TResult>(...)`) or as DI classes (`AddCommandHandler<THandler>()`).
- **Events & gating.** `ICdpEventEmitter` reads the event's `[MethodName]` and suppresses events for
  a *gated* domain (one that defines `enable`) until that domain is enabled on the connection. The
  session enables a domain optimistically before its `enable` handler runs, so an `enable` handler
  can emit immediately.
- **Sessions.** One `CdpSession` per connection: a read loop, concurrent command dispatch, and a
  single-writer outbound `Channel` (WebSocket allows one concurrent send). `sessionId` is threaded
  through end to end; flatten/multi-target routing is a future addition.

## Build

The solution file is `src/Enyim.Iris.slnx`.

```pwsh
dotnet build src/Enyim.Iris.slnx
```

## Run the sample

```pwsh
dotnet run --project samples/Cdp.Sample.BrowserEmulator
# then browse the discovery endpoints:
#   GET /json/version
#   GET /json/list      -> a page target with a webSocketDebuggerUrl
# connect a CDP client to that webSocketDebuggerUrl and call e.g. Browser.getVersion
```

## Test

```pwsh
dotnet test src/Enyim.Iris.slnx
```
