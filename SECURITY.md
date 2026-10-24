# Security Policy

## Current status

Marsa is currently a **documentation and design repository**. There is no released server, CLI, container image, or installer to test. Do not treat the design documents as a hardened product.

This policy covers the repository and future Marsa releases. It does not cover third-party applications deployed by Marsa users.

## Supported versions

| Version | Supported |
|---|---|
| No released version | Not applicable |
| 0.1.x after release | Security fixes at the maintainer’s discretion |
| Older pre-1.0 versions | Not supported by default; upgrade guidance will be published with releases |

There is no security SLA and no bug bounty. The project is maintained on a best-effort basis.

## Reporting a vulnerability

**Do not open a public issue with exploit details, credentials, or private data.**

Preferred path:

1. Use GitHub’s **private vulnerability reporting** for this repository if it is enabled: open the repository’s **Security** tab and choose **Report a vulnerability**.
2. If private reporting is unavailable, contact the repository owner through the contact method on their GitHub profile and ask for a private channel. Send only a minimal note first; do not include the full exploit or any secret.

A useful report includes:

- affected commit, version, or document;
- the component and trust boundary involved;
- reproduction steps or a minimal proof of concept;
- expected and observed behavior;
- security impact and who is exposed;
- whether the issue is already public;
- a safe way to contact you;
- any suggested fix or mitigation, if you have one.

Never include live credentials, private repository URLs, customer data, or a working exploit against a system you do not own or have permission to test.

## What to expect

- We aim to acknowledge a complete report within **3 business days**.
- We aim to provide an initial assessment within **10 business days**.
- Timelines may slip for complex issues or maintainer availability. We will tell you if that happens.
- We will coordinate disclosure and credit with you when practical.
- We may publish a GitHub security advisory after a fix or mitigation is available.

These are targets, not contractual commitments. Do not disclose a vulnerability publicly before a fix or coordinated disclosure date is agreed.

## Scope

In scope for the design and future releases:

- authentication, sessions, CSRF, personal access tokens, and authorization;
- tenant isolation and cross-organization access;
- secret storage, envelope encryption, master-key handling, and redaction;
- Docker socket access, container isolation, build isolation, and host-mount rejection;
- Caddy admin socket, route compilation, hostname validation, and TLS handling;
- webhook signature verification, replay prevention, and deduplication;
- SSRF, path traversal, symlink escape, archive limits, and input validation;
- deployment admission, worker leases, state transitions, rollback, and reconciliation;
- supply-chain integrity, SBOM, dependency licenses, and release artifacts;
- documentation that could cause an operator to create an unsafe deployment.

Out of scope:

- vulnerabilities in applications deployed by users;
- vulnerabilities in third-party dependencies that have no Marsa-specific impact (report them upstream; we still want to know if Marsa exposes them);
- social engineering, physical access, or denial of service against a host you do not own;
- missing hardening features that are explicitly documented as post-MVP, unless they create an unexpected exposure;
- the fact that Docker socket access is root-equivalent; this is documented residual risk, not a hidden vulnerability.

## Safe harbor

We support good-faith security research that:

- tests only systems you own or have explicit permission to test;
- avoids privacy violations, data destruction, persistence, and service disruption;
- does not access, modify, or exfiltrate data that is not yours;
- gives us reasonable time to respond before public disclosure;
- follows applicable law.

We will not pursue legal action for good-faith research that follows these rules. This statement is not a waiver of any third party’s rights and is not legal advice.

## Hardening expectations for operators

When Marsa is released:

- use a dedicated Linux host, not a shared personal server;
- keep the host kernel, Docker Engine, Caddy, PostgreSQL, and Marsa patched;
- restrict inbound traffic to `80` and `443`;
- keep the Caddy admin socket local and permissioned;
- never expose the Docker socket over TCP;
- protect the master key separately from database backups;
- test restore, key rotation, and upgrade procedures;
- review the audit log and deployment history;
- do not run untrusted images or Dockerfiles on a host that holds unrelated secrets.

The full threat model is in [`docs/architecture/security.md`](docs/architecture/security.md). The single-host and Docker-socket limits are in [`docs/roadmap.md`](docs/roadmap.md) and [`docs/risks-and-open-questions.md`](docs/risks-and-open-questions.md).

## Dependency and supply-chain reports

Reports about a dependency’s license, provenance, or known vulnerability are welcome. Include the package, version, affected release artifact, and whether the default distribution is exposed. License policy is in [`docs/license-and-provenance.md`](docs/license-and-provenance.md).
