# ADR 0005 — Docker Engine through the local socket

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Marsa must build images, create containers, run health checks, capture logs, and retire releases on one Linux host. The runtime must support BuildKit and be available without a Kubernetes cluster or a remote helper process.

## Decision

Use **Docker Engine 27+** through the local Unix socket.

- Docker.DotNet talks to `/var/run/docker.sock` by default.
- Builds use BuildKit when available.
- The server never accepts raw Docker operations from users; it accepts a constrained plan.
- Workloads never receive the socket.
- Builds never receive the socket.
- No TCP Docker endpoint is supported.
- Managed resources carry `io.marsa.managed=true` and organization/project/service/deployment labels.
- Privileged mode, host network/PID/IPC, devices, and host bind mounts are rejected.

## Consequences

### Positive

- Native containers and BuildKit without an orchestrator.
- Simple single-host install.
- Direct access to logs, inspect, and health.
- No helper process protocol to design in 0.1.

### Negative

- The socket is root-equivalent; a server compromise can become host compromise.
- Docker API version compatibility must be managed.
- Rootless Docker is best-effort.
- A socket proxy may be required for stronger isolation later.

### Follow-ups

- Keep the socket local and permissioned.
- Add a label guard and unmanaged-resource refusal tests.
- Evaluate a socket proxy or host helper process before public release.
- Document the residual risk in the security review.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Kubernetes | Far too heavy for one host and a small operator |
| Podman | Not the primary supported runtime; compatibility risk |
| Docker over TCP | Expands the attack surface |
| Host helper process with mTLS now | Useful later, but too much scope for 0.1 |
| systemd + native processes | Loses container portability and image workflow |

## References

- `07-security.md`
- `03-architecture.md`
