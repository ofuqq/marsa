# ADR 0015 — OpenAPI-first `/api/v1` contract

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

The dashboard, CLI, and integrations must agree on request and response shapes. Hand-written duplicate DTOs drift and hide breaking changes.

## Decision

Treat the HTTP API as a versioned, generated contract.

- Base path `/api/v1`.
- OpenAPI is generated from endpoint metadata and `Marsa.Contracts`.
- CI stores `docs/api/openapi.v1.json` and fails on unapproved diffs.
- The dashboard and CLI use generated clients; no duplicate wire models.
- Additive changes are allowed in v1; breaking changes require `/api/v2`.
- Error shapes, enum values, and idempotency behavior are part of the contract.
- Examples are authored with the contract and included in the document.

## Consequences

### Positive

- One source of truth for clients.
- Breaking changes are detected in CI.
- Generated clients reduce drift.
- The API is usable by third parties without a private SDK.

### Negative

- Generated clients can be awkward for UI-specific needs.
- Contract changes require regeneration and review.
- OpenAPI generation must be deterministic.
- A public SDK remains out of scope.

### Follow-ups

- Add OpenAPI diff to CI.
- Test generated clients compile.
- Document compatibility policy.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Hand-written DTOs per client | Drift and duplicate validation |
| gRPC | Browser and CLI complexity; not needed |
| GraphQL | Overkill for a small resource API |
| No versioning | Breakage without warning |

## References

- `06-api-contracts.md`
- `09-testing-strategy.md`
