# ADR 0002 — .NET 10 LTS and C# 14

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Marsa is a .NET-native product. The runtime must be supported for the life of 0.1, available on Linux amd64/arm64, and compatible with ASP.NET Core, EF Core, Identity, SignalR, OpenAPI, Docker.DotNet, and Testcontainers.

## Decision

Target **.NET 10 LTS**, `net10.0`, with C# 14 and SDK `10.0.100`.

Pin the SDK in `global.json` with `rollForward: latestFeature`. Pin package versions centrally and commit lockfiles. Build and test on Linux in CI.

## Consequences

### Positive

- LTS support window.
- Current ASP.NET Core, EF Core, and Identity features.
- Modern C# syntax without preview features.
- Clear runtime image tags for amd64 and arm64.

### Negative

- Some third-party packages may lag a new LTS.
- Developers need the pinned SDK.
- Trimming and AOT are not reliable for ASP.NET Core/EF Core in 0.1; publish untrimmed.

### Follow-ups

- Validate the package matrix in Foundation.
- Keep a fallback plan if a critical dependency does not support .NET 10.
- Revisit the runtime at the next LTS.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| .NET 9 | Short support window for a new product |
| .NET 8 LTS | Older runtime and language features |
| .NET Framework | Not Linux-native or cross-platform |
| AOT/trimmed publish | EF Core, Razor, and reflection-heavy libraries are not safely trim-safe in 0.1 |

## References

- `04-dotnet-solution-structure.md`
- `11-release-and-versioning.md`
