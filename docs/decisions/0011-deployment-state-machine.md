# ADR 0011 — Canonical deployment state machine

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Deployment state appears in the database, API, UI, CLI, audit, and tests. The approved requirements already named the states. Earlier analysis used slightly different names, which would create mapping bugs and ambiguous UI copy.

## Decision

Use the approved canonical states everywhere:

```text
queued -> preparing -> building -> deploying -> health_checking -> routing
       -> succeeded | succeeded_with_warnings | action_required | failed | cancelled
```

Rules:

- No backward transitions.
- Terminal states are immutable.
- Cancellation is allowed from `queued` through `health_checking`.
- `routing` is atomic and not cancellable.
- A failed health check with a previous serving release becomes `action_required`; with no previous release it becomes `failed`.
- A working route with pending TLS is `succeeded_with_warnings`.
- Rollback is a new deployment, not a reverse transition.
- `NoChanges` is an admission result, not a state.

## Consequences

### Positive

- One vocabulary across docs, API, UI, tests, and audit.
- Clear distinction between failure and operator action.
- No ambiguity about whether TLS is part of success.

### Negative

- `deploying` covers both container creation and start; steps add detail.
- `action_required` is terminal and needs a retry/rollback action.
- New states require a migration and contract review.

### Follow-ups

- Enforce transitions in the domain model.
- Add a state diagram to the UI.
- Test every invalid transition.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| `Starting`/`Activating` | Conflicts with approved approved names and requirement IDs |
| Separate `NoChanges` state | It is an admission result, not execution state |
| Auto-rollback on failure | Out of scope; explicit rollback is safer |
| One `failed` state for all outcomes | Loses the “old release still serving” distinction |

## References

- `01-product-requirements.md`
- `03-architecture.md`
- `05-data-model.md`
