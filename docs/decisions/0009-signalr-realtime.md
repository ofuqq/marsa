# ADR 0009 — SignalR for deployment events and logs

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

The dashboard needs live deployment phases and logs. Polling works but is wasteful and makes the experience feel slower. The server already runs ASP.NET Core and has cookie/PAT authentication.

## Decision

Use **SignalR** for realtime deployment events and log chunks.

- Hub path: `/hubs/deployments`.
- Clients subscribe to a project or deployment after authorization.
- Events are ordered and sequence-numbered.
- Reconnect replays events after the last sequence.
- SignalR is not the source of truth; clients refetch state.
- Slow clients may receive coalesced or dropped log chunks.
- Payloads never contain secrets.
- The server publishes after commit through the outbox.

## Consequences

### Positive

- No additional service or protocol.
- Uses the same auth and authorization model.
- Ordered events and replay cover reconnects.
- Log streaming feels immediate on a local network.

### Negative

- WebSocket or long-polling support must be configured correctly behind Caddy.
- Reconnection and backpressure need tests.
- A future multi-replica server needs a backplane or sticky sessions.
- SignalR events are not a durable log.

## Follow-ups

- Test reconnect and slow clients.
- Document proxy timeouts.
- Revisit a backplane only if the server is split.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Polling only | Simpler, but slower and more requests |
| Server-sent events only | Works for logs, but not bidirectional subscriptions |
| WebSocket custom protocol | Reinvents SignalR |
| External pub/sub | Additional dependency for one process |

## References

- `06-api-contracts.md`
- `03-architecture.md`
