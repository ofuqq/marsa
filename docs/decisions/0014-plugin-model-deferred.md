# ADR 0014 — No plugin loader in 0.1

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

A plugin model could support new runtimes, registries, DNS providers, and notification channels. It would also introduce arbitrary code loading, version compatibility, permissions, and supply-chain risk before the core deployment path is stable.

## Decision

Do not ship a plugin loader or marketplace in 0.1.

- Use narrow application ports (`IRuntimeProvider`, `IEdgeRouter`, `ISourceProvider`, `ISecretProtector`).
- Implement adapters inside the repository.
- Do not load assemblies from user-writable paths.
- Do not expose a plugin API as a public contract yet.
- Revisit after the MVP adapters and invariants are stable.

## Consequences

### Positive

- Smaller attack surface.
- No version-matrix or sandbox design.
- Internal ports still allow future extension.
- Faster core delivery.

### Negative

- Users cannot add integrations without a code change.
- A future plugin API may require refactoring.
- Provider breadth lags behind plugin-based competitors.

### Follow-ups

- Keep interfaces narrow and documented.
- Avoid leaking internal types into a future plugin contract.
- Revisit with a threat model and an ADR.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Load .NET assemblies from a folder | Arbitrary code execution and version chaos |
| Script plugins | Sandbox and dependency complexity |
| Out-of-process plugin protocol | Useful later; too much scope now |
| Marketplace | Requires governance and supply-chain controls |

## References

- `04-dotnet-solution-structure.md`
- `07-security.md`
