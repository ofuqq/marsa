# ADR 0020 — Explicit migration bundle, no startup migration

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Automatic startup migrations are convenient but risky. Multiple server instances, a failed migration, or a rollback can leave the schema incompatible with the running binary. The product needs a reviewable upgrade path.

## Decision

Use an **explicit migration bundle** or command, never automatic migration on web startup.

- Migrations are generated from `Marsa.Infrastructure` and reviewed as code.
- The release ships a self-contained migration bundle.
- The upgrade script runs the bundle before starting the new server.
- Migrations are expand/contract; the new binary runs against the previous schema during rollout.
- Column drops happen in a later release.
- The server checks schema version at startup and refuses to serve if the database is newer than the binary.
- Downgrade requires a database restore.

## Consequences

### Positive

- The upgrade is a deliberate, logged step.
- A failed migration does not run repeatedly on every restart.
- Backward compatibility is testable.
- Rollback planning is explicit.

### Negative

- The operator must run an extra step.
- The install/upgrade script must handle failures clearly.
- Migration bundles add a release artifact.

### Follow-ups

- Test upgrade from the previous release.
- Document the backup before migration.
- Add schema-version checks and a clear error.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Auto-migrate at startup | Races, repeated failures, hidden changes |
| Manual SQL only | Error-prone and not repeatable |
| No migrations, recreate the database | Data loss |
| Migration sidecar | More moving parts on one host |

## References

- `05-data-model.md`
- `11-release-and-versioning.md`
