# Architecture Decision Records

An ADR records a decision that is expensive to reverse or that shapes several parts of Marsa. ADRs are immutable; a later ADR supersedes an earlier one.

## Format

Each ADR uses:

- **Status:** proposed, accepted, superseded, or rejected.
- **Context:** the forces and constraints.
- **Decision:** what Marsa does.
- **Consequences:** benefits, costs, and follow-ups.
- **Alternatives:** what was rejected and why.

## Index

| ADR | Decision | Status |
|---|---|---|
| [0001](0001-modular-monolith.md) | Modular monolith with one deployable server | Proposed |
| [0002](0002-dotnet-10-lts.md) | .NET 10 LTS and C# 14 | Proposed |
| [0003](0003-postgresql-only-stateful-dependency.md) | PostgreSQL as the only required stateful dependency | Proposed |
| [0004](0004-caddy-edge.md) | Caddy as the edge and TLS terminator | Proposed |
| [0005](0005-docker-engine-socket.md) | Docker Engine through the local socket | Proposed |
| [0006](0006-razor-components.md) | Razor Components, server-rendered dashboard | Proposed |
| [0007](0007-local-identity-and-pats.md) | ASP.NET Core Identity plus PATs | Proposed |
| [0008](0008-postgresql-lease-worker-outbox.md) | PostgreSQL leases, outbox, no broker | Proposed |
| [0009](0009-signalr-realtime.md) | SignalR for deployment events and logs | Proposed |
| [0010](0010-single-host-mvp.md) | Single Linux host for 0.1 | Proposed |
| [0011](0011-deployment-state-machine.md) | Canonical deployment state machine | Proposed |
| [0012](0012-apache-2-license.md) | Apache-2.0 outbound license | Proposed |
| [0013](0013-no-desktop-app-mvp.md) | No desktop application in 0.1 | Proposed |
| [0014](0014-plugin-model-deferred.md) | No plugin loader in 0.1 | Proposed |
| [0015](0015-openapi-contract-first.md) | OpenAPI-first `/api/v1` contract | Proposed |
| [0016](0016-uuidv7-identifiers.md) | UUIDv7 identifiers | Proposed |
| [0017](0017-envelope-encryption-master-key.md) | AES-GCM envelope encryption with a master key | Proposed |
| [0018](0018-git-cli-process-boundary.md) | Git CLI process boundary | Proposed |
| [0019](0019-no-auto-rollback.md) | No automatic rollback in 0.1 | Proposed |
| [0020](0020-explicit-migrations.md) | Explicit migration bundle, no startup migration | Proposed |

## Creating an ADR

1. Copy the structure from an existing ADR.
2. Use the next four-digit number.
3. Link it from this index and from the affected design document.
4. If it supersedes an ADR, update both statuses.
5. Do not edit an accepted ADR to change its decision; write a new one.

## Template

```markdown
# ADR NNNN — Title

- Status: Proposed
- Date: YYYY-MM-DD
- Supersedes: none
- Superseded by: none

## Context

## Decision

## Consequences

### Positive

### Negative

### Follow-ups

## Alternatives considered

## References
```
