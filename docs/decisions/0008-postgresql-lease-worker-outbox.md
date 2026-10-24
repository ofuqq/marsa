# ADR 0008 — PostgreSQL leases, outbox, and no message broker

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Deployments and webhooks need durable asynchronous work. A broker would add another service to install, secure, and back up. The expected concurrency is low: four deployments per host and one server process.

## Decision

Use PostgreSQL as the work queue.

- Deployments are claimed with `FOR UPDATE SKIP LOCKED`.
- A 60-second lease and 15-second heartbeat protect against worker crashes.
- A transactional outbox carries side effects such as webhook processing and SignalR notifications after commit.
- At-least-once delivery is expected; handlers are idempotent.
- Maximum deployment attempts: 3.
- No Redis, RabbitMQ, Kafka, or external scheduler in 0.1.
- The worker is a hosted service; it can be split later without changing the queue contract.

## Consequences

### Positive

- One transactional store for state, queue, and audit.
- No extra service or credential.
- Crashes recover through lease expiry.
- Outbox prevents lost events between commit and broadcast.

### Negative

- Polling and lock contention need monitoring.
- Very high throughput would require a broker.
- Long-running steps need heartbeats and idempotency.
- PostgreSQL becomes both the state store and the queue.

### Follow-ups

- Test kill/restart during build and health.
- Add queue-age and lease-expiry metrics.
- Revisit only with measured throughput or multi-process needs.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Hangfire with PostgreSQL | A framework owning state duplicates the deployment state machine |
| Redis queue | Extra stateful service and backup concern |
| RabbitMQ/Kafka | Operational overkill |
| In-memory queue | Loses work on restart |
| Cron + database polling only | Weaker lease and recovery semantics |

## References

- `05-data-model.md`
- `03-architecture.md`
