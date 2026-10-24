# ADR 0004 — Caddy as the edge and TLS terminator

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Marsa needs automatic TLS, HTTP routing, redirects, and a way to apply application routes without exposing an admin API to tenant networks. The edge must be reliable, small, and configurable from the control plane.

## Decision

Use **Caddy 2.x** as the only supported edge in 0.1.

- Caddy terminates TLS and obtains/renews certificates with ACME HTTP-01.
- The control-plane hostname uses a **static** route.
- Application routes are managed dynamically.
- The admin API listens only on `/run/marsa/caddy/admin.sock`.
- Marsa compiles the complete Marsa-managed route set and applies it transactionally.
- Foreign Caddy configuration is preserved.
- Caddy is attached to project networks only to reach workloads.
- No external reverse proxy is supported in the default topology.

## Consequences

### Positive

- Automatic certificates with a small configuration surface.
- No custom TLS or ACME implementation in Marsa.
- Unix-socket admin is not reachable from workloads.
- A static control route prevents operator lockout.

### Negative

- Caddy becomes a security-critical component attached to tenant networks.
- The admin API must be handled carefully; a full-config replace can be disruptive.
- Certificate issuance depends on DNS and ACME availability.
- Caddy is an additional image to pin and patch.

### Follow-ups

- Test route application under concurrency and after restart.
- Keep the admin socket permissions at `0700`/`0660`.
- Evaluate DNS-01 only when wildcard or private DNS support is required.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| YARP inside ASP.NET Core | No built-in ACME; more custom TLS work |
| Traefik | Larger configuration surface; label-driven model is less explicit |
| Nginx | Manual certificate management; no first-class admin API |
| Cloud load balancer | Breaks the self-hosted single-host promise |

## References

- `03-architecture.md`
- `07-security.md`
