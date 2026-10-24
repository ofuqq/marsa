# ADR 0003 — PostgreSQL as the only required stateful dependency

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Marsa needs durable configuration, deployment state, a work queue, leases, audit, encrypted secrets, and log metadata. Adding Redis, a broker, or an object store would increase install, backup, and failure-recovery burden.

## Decision

Use **PostgreSQL 17** as the system of record and the only required stateful service.

- EF Core with Npgsql for persistence.
- PostgreSQL partial unique indexes and row locks for concurrency invariants.
- `FOR UPDATE SKIP LOCKED` for worker and outbox claims.
- A transactional outbox for side effects.
- JSONB only for versioned snapshots, plans, and provider metadata.
- No Redis, broker, object store, or external scheduler in 0.1.

## Consequences

### Positive

- One database to install, back up, restore, and secure.
- Transactional invariants across deployment admission, snapshots, audit, and outbox.
- No additional network service or credential set.
- Logs and state can be restored together (except the separate master key).

### Negative

- PostgreSQL is a single point of failure on one host.
- The queue is modest; very high throughput would need a broker.
- Large log volumes require retention and partitioning discipline.
- Caddy certificates and images still live outside the database.

### Follow-ups

- Define backup/restore and WAL archiving later.
- Monitor queue depth, lock waits, and table growth.
- Revisit a broker only if measured throughput demands it.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Redis + PostgreSQL | Extra service, backup, and failure mode |
| RabbitMQ/Kafka | Operational overkill for one host |
| SQLite | Weaker concurrency and operational story for a server |
| PostgreSQL + S3 for artifacts | Extra dependency; local image store is enough for 0.1 |

## References

- `05-data-model.md`
- `03-architecture.md`
