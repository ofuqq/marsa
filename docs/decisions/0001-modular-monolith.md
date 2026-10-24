# ADR 0001 — Modular monolith with one deployable server

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Marsa must be installable on one Linux host, understandable by a small team, and operable by one person. The product has distinct concerns: API, dashboard, realtime updates, deployment orchestration, Docker, Caddy, Git, and persistence. Splitting them into services would add network boundaries, deployment sequencing, and failure modes before the product has proven demand.

## Decision

Build Marsa as a **modular monolith** packaged as one ASP.NET Core process.

- API endpoints, Razor Components, SignalR hubs, and the deployment worker run in `Marsa.Server`.
- Modules are separated by projects and application boundaries, not network calls.
- The worker is a hosted service. It can be split into a separate process later without changing domain or application contracts.
- The database is shared by modules, but modules do not write directly to another module's tables.

## Consequences

### Positive

- One artifact to install, upgrade, and debug.
- No distributed transactions, service discovery, or inter-service authentication.
- A single process can share authorization, logging, and configuration.
- The worker can be split later because orchestration uses ports and persisted state.

### Negative

- A crash affects API, UI, and worker together.
- A CPU-heavy build step could affect request latency if not isolated; builds run through Docker, not in-process.
- Module boundaries must be enforced by review and architecture tests.

### Follow-ups

- Enforce project references and namespace bans in CI.
- Keep the worker free of HTTP-specific dependencies.
- Revisit a worker split when concurrency or availability requires it.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Microservices from day one | Too much operational surface for one host and a small team |
| Modular monolith with a separate worker immediately | Adds packaging and deployment complexity before scale demands it |
| Single project without module boundaries | Fast initially, but auth, tenancy, and deployment logic would become entangled |

## References

- `03-architecture.md`
- `04-dotnet-solution-structure.md`
