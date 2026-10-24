# ADR 0010 — Single Linux host for 0.1

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

The target user has one server, wants a clear deployment path, and does not want to operate a cluster. Multi-host scheduling, SSH helper processes, and Kubernetes would add abstractions that are not needed to prove the core experience.

## Decision

0.1 supports **one Linux Docker host** per installation.

- One local Docker server.
- One organization per installation.
- One service per deployment and one replica per service.
- One Caddy edge.
- One PostgreSQL database.
- No multi-host, SSH targets, Kubernetes, Nomad, or Compose import.
- The `servers` table reserves a future `kind`, but only `local` is active.
- Interfaces are kept narrow so a host helper process can be added later.

## Consequences

### Positive

- Understandable install and failure model.
- No scheduler or distributed-state design.
- Clear resource and backup boundary.
- The product can compete on comprehension, not breadth.

### Negative

- No HA or failover.
- One host's disk/CPU is the ceiling.
- Users with multiple hosts must wait or use separate installs.
- A future multi-host feature may require new invariants and UI.

### Follow-ups

- Document single-host limits honestly.
- Add backup/restore and degraded-state handling.
- Revisit multi-host only after the single-host experience is proven.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Kubernetes | Too heavy for the target user |
| Docker Swarm | Unclear long-term ecosystem; still a cluster |
| SSH multi-host | New credential, networking, and failure model |
| Nomad | Extra infrastructure |
| Compose import | Different product scope and data model |

## References

- `03-architecture.md`
- `08-roadmap.md`
