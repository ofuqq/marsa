# ADR 0012 — Apache-2.0 outbound license

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Marsa is a self-hosted product that should be broadly adoptable, friendly to commercial use, and clear about patents and contributions. Some prior analysis material has GPL boundaries that must not leak into the default distribution.

## Decision

License first-party code and documentation under **Apache-2.0**.

- Include the full `LICENSE`.
- Include a `NOTICE` with copyright and required attributions.
- Maintain `THIRD-PARTY-NOTICES.md`.
- Use DCO sign-off for contributions; no CLA in 0.1.
- Exclude GPL/AGPL/unknown-license material from the default distribution.
- Record exceptions with owner and expiry.

## Consequences

### Positive

- Patent grant and contributor clarity.
- Compatible with common .NET, PostgreSQL, and Caddy dependencies.
- Permits private and commercial self-hosting.
- One license for code and docs.

### Negative

- Legal review is still required before public distribution.
- Documentation is less prose-friendly than CC BY.
- Some users prefer MIT for simplicity.
- A future commercial edition could require a different model.

### Follow-ups

- Add `LICENSE` and `NOTICE` in approved.
- Add an SBOM and notices gate.
- Revisit the docs license if they are published separately.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| MIT | No express patent grant |
| AGPL | Network-copyleft conflicts with adoption goals |
| BSL/SSPL | Not OSI open source |
| Dual license | Premature complexity |
| CC BY for docs | A second policy to maintain; revisit later |

## References

- `12-license-and-clean-room.md`
- `00-identity.md`
