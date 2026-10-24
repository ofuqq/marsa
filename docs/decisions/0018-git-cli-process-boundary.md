# ADR 0018 — Git CLI process boundary

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Marsa must clone and fetch public and private HTTPS repositories. A native Git library would avoid a process boundary but adds native dependencies, platform packaging, and its own security surface. The Git CLI is GPL-2.0, which must not contaminate the default distribution.

## Decision

Use the **Git CLI as a separate process** in the builder/source-fetch path.

- The runtime server image does not contain Git.
- Source fetching happens in a controlled builder environment that includes Git.
- Arguments are passed as an argument list, never a shell string.
- Repository URLs are validated HTTPS; credentials are passed through a constrained mechanism, not logged.
- The source is materialized into a private workspace; `.git` is excluded from the build context.
- No `LibGit2Sharp` in 0.1.
- If a distributed builder image includes Git, its GPL obligations and notices are documented and satisfied.

## Consequences

### Positive

- No native library coupling.
- Predictable Git behavior and protocol support.
- Keeps the runtime image free of GPL tooling.
- Process isolation for source fetch.

### Negative

- Process spawning and output capture add complexity.
- Git must be present in the builder image.
- Performance is slightly worse than a linked library.
- License compliance is required if the builder image is distributed.

### Follow-ups

- Test argument injection and credential redaction.
- Document the builder image license.
- Revisit a permissive source fetcher if distribution becomes difficult.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| LibGit2Sharp | Native dependency and license/maintenance review |
| JGit/.NET port | Immature or unavailable |
| GitHub API only | Does not cover GitLab or generic Git |
| Shell command string | Injection risk |

## References

- `12-license-and-clean-room.md`
- `07-security.md`
