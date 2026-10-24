# ADR 0019 — No automatic rollback in 0.1

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

A failed deployment should never take down the serving release. It is tempting to auto-rollback on health or route failure. Automatic rollback can also hide a persistent failure, repeat a destructive data migration, or surprise an operator.

## Decision

No automatic rollback in 0.1.

- A failed health or route step leaves the previous release serving.
- The deployment ends in `action_required` when a previous release exists.
- The operator explicitly starts a rollback, which creates a new deployment pinned to a prior image digest and snapshot.
- Rollback passes the same health and route gates.
- History is never mutated.

## Consequences

### Positive

- No hidden state changes.
- The operator sees the failure and chooses recovery.
- Avoids repeated rollback loops.
- Audit remains simple.

### Negative

- Recovery needs a human action.
- A production incident may last longer.
- The UI must make rollback obvious.

### Follow-ups

- Show `action_required` with a clear rollback action.
- Test rollback after failed health and failed route.
- Revisit auto-rollback only with a safe policy and data-migration guard.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Auto-rollback on health failure | Can loop and hide the real cause |
| Auto-rollback on route failure | Same, plus route churn |
| Auto-rollback with a cooldown | Still surprising; more policy surface |
| No rollback support | Unacceptable; explicit rollback is required |

## References

- `01-product-requirements.md`
- `03-architecture.md`
