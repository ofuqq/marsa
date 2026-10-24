# ADR 0006 — Razor Components, server-rendered dashboard

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

The dashboard must support setup, projects, deployments, logs, domains, secrets, audit, Arabic RTL, and accessibility. A separate SPA would add a Node build, a second auth model, and another deployment artifact.

## Decision

Use **Razor Components** hosted in `Marsa.Server`, server-rendered by default.

- Interactive islands are small and optional.
- SignalR handles live deployment updates and logs.
- No SPA, Node build pipeline, or separate frontend host.
- Dashboard actions call application services or the same typed API contract.
- The dashboard never accesses `DbContext` directly.
- Layout uses logical CSS properties for RTL.
- Code and logs stay LTR inside Arabic pages.

## Consequences

### Positive

- One runtime, one auth model, one deployable.
- No JavaScript bundler or frontend supply chain.
- Server-side authorization is straightforward.
- Shared localization and layout components.
- Fast initial render on a local network.

### Negative

- Rich interactions require explicit islands or small scripts.
- The dashboard and API share a process, so a UI bug can affect the API.
- A future separate frontend would need a migration plan.
- Razor Components antiforgery and auth need early validation.

### Follow-ups

- Spike cookie auth, interactive components, and antiforgery in Setup and identity.
- Keep interactive islands small and independently testable.
- Test RTL at 360 px and 1280 px.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| React/Vue SPA | Node build, duplicate auth, CORS, more deployment surface |
| MVC views only | Workable, but weaker component model for live updates |
| Blazor WebAssembly | Large download, duplicate auth, not needed |
| Separate static admin app | Additional host and CORS complexity |

## References

- `04-dotnet-solution-structure.md`
- `09-testing-strategy.md`
