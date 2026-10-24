# ADR 0007 — ASP.NET Core Identity plus personal access tokens

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Marsa needs local authentication for a single-operator install, roles for a small team, and non-interactive access for the CLI. OIDC/SSO would add provider configuration and is not needed for 0.1.

## Decision

Use **ASP.NET Core Identity** for local users and **personal access tokens** for CLI/API access.

- Local email/password login.
- Identity password hasher with current defaults.
- Cookie sessions for the dashboard.
- Roles are organization memberships: owner, admin, developer, viewer.
- Identity roles are not used for authorization.
- PATs are prefixed `marsa_pat_`, hashed with SHA-256, scoped `read`/`deploy`/`admin`, expiring, revocable.
- PATs cannot exceed the creator's role.
- OIDC/SSO is post-MVP.

## Consequences

### Positive

- No external identity dependency.
- Standard ASP.NET Core security primitives.
- CLI and dashboard share the same authorization model.
- PATs are easy to audit and revoke.

### Negative

- Password reset and email delivery are not solved in 0.1.
- No enterprise SSO.
- PATs require careful storage and scope design.
- Identity tables add schema surface.

### Follow-ups

- Define password reset after email exists.
- Add OIDC only with a reviewed provider.
- Test token revocation and role changes.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| OIDC-only | Requires an external provider; blocks first-run simplicity |
| Custom auth | Unnecessary risk and maintenance |
| Passkeys-only | Not enough browser/device coverage for 0.1 |
| Long-lived API keys without scopes | Too coarse and hard to revoke safely |

## References

- `07-security.md`
- `06-api-contracts.md`
