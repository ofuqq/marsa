# ADR 0013 — No desktop application in 0.1

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

The prior analysis covers a desktop application and local-first workflows. Marsa targets a Linux server and a browser/CLI operator. A desktop app would add packaging, signing, auto-update, and a second auth/security model.

## Decision

No desktop or mobile application in 0.1. The dashboard and CLI are the only clients.

- No Electron, MAUI, Tauri, or native shell.
- No local-only mode with zero-auth loopback.
- No embedded server in a desktop package.
- The API remains suitable for a future client.

## Consequences

### Positive

- One UI runtime and one auth model.
- No code-signing or auto-update pipeline.
- Smaller security surface.
- The team focuses on the server experience.

### Negative

- No offline/local-first experience.
- Users must run a server even for local experiments.
- A future desktop client needs a separate design and threat model.

### Follow-ups

- Design the API so a client can be added later.
- Revisit only after the server workflow is proven.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Electron shell | Large surface, duplicate auth, auto-update burden |
| Tauri shell | Still a second product and packaging pipeline |
| Local desktop mode | Conflicts with the server/operator model |
| Mobile app | Out of scope and low value for deployment operations |

## References

- `01-product-requirements.md`
- `03-architecture.md`
