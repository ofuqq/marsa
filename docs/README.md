# Marsa Documentation Index

> Status: documentation-only repository. No Marsa product release exists.
> Last updated: 2026-10-22.
> This index lists every Markdown file under `docs/`. Root project files are listed separately.

## Start here

| If you want to… | Read |
|---|---|
| Understand why Marsa exists | [`../vision.md`](vision.md) |
| Know the canonical names and URLs | [`../identity.md`](identity.md) |
| See what 0.1 must do | [`../product-requirements.md`](product-requirements.md) |
| Understand the design | [`../architecture/overview.md`](architecture/overview.md) |
| Review the security model | [`../architecture/security.md`](architecture/security.md) |
| See the construction order | [`../roadmap.md`](roadmap.md) |
| Pick up an implementation task | [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) |
| Contributor workflow | [`../CONTRIBUTING.md`](../CONTRIBUTING.md) |
| Know when a change is done | [`../CONTRIBUTING.md`](../CONTRIBUTING.md) |

## Core documents

| File | Purpose | Status |
|---|---|---|
| [`../vision.md`](vision.md) | Problem, users, principles, non-goals, naming evidence | Decided |
| [`../identity.md`](identity.md) | Canonical product name, brand, handle, URLs, prefixes | Decided |
| [`../product-requirements.md`](product-requirements.md) | 151 requirement IDs, acceptance criteria, metrics | approved approved |
| [`../positioning.md`](positioning.md) | Honest competitor comparison, differentiation bets, validation plan | approved approved |
| [`../architecture/overview.md`](architecture/overview.md) | Modular monolith, components, state machine, trust boundaries | Proposed for 0.1 |
| [`../architecture/solution-structure.md`](architecture/solution-structure.md) | Projects, references, repository layout, conventions | Proposed for 0.1 |
| [`../architecture/data-model.md`](architecture/data-model.md) | Tables, constraints, indexes, invariants, retention | Proposed for 0.1 |
| [`../architecture/api.md`](architecture/api.md) | `/api/v1` behavior, errors, webhooks, SignalR | Proposed for 0.1 |
| [`../architecture/security.md`](architecture/security.md) | Threat model, auth, secrets, Docker/Caddy boundaries | Proposed for 0.1 |
| [`../risks-and-open-questions.md`](risks-and-open-questions.md) | Decision register, open questions, assumptions, risks | Living register |
| [`../roadmap.md`](roadmap.md) | Foundation–Release candidate milestones, epics, MVP cut line, release plan | approved approved |
| [`../development/testing.md`](development/testing.md) | Test layers, required cases, CI and E2E lanes | Proposed for 0.1 |
| [`../development/getting-started.md`](development/getting-started.md) | Prerequisites, local stack, database and test workflow | Proposed for 0.1 |
| [`../development/releasing.md`](development/releasing.md) | SemVer, artifacts, upgrade, backup, release gates | Proposed for 0.1 |
| [`../license-and-provenance.md`](license-and-provenance.md) | Apache-2.0 proposal, dependency policy, clean-room evidence | Proposed for 0.1 |

## Architecture decision records

The ADR index contains the status and summary of each decision. Every ADR below is linked from it.

| ADR | Topic |
|---|---|
| [`decisions/README.md`](decisions/README.md) | ADR index and status |
| [`decisions/0001-modular-monolith.md`](decisions/0001-modular-monolith.md) | Modular monolith |
| [`decisions/0002-dotnet-10-lts.md`](decisions/0002-dotnet-10-lts.md) | .NET 10 LTS |
| [`decisions/0003-postgresql-only-stateful-dependency.md`](decisions/0003-postgresql-only-stateful-dependency.md) | PostgreSQL as the only stateful dependency |
| [`decisions/0004-caddy-edge.md`](decisions/0004-caddy-edge.md) | Caddy edge and Unix admin socket |
| [`decisions/0005-docker-engine-socket.md`](decisions/0005-docker-engine-socket.md) | Docker Engine socket boundary |
| [`decisions/0006-razor-components.md`](decisions/0006-razor-components.md) | Razor Components, no SPA |
| [`decisions/0007-local-identity-and-pats.md`](decisions/0007-local-identity-and-pats.md) | Local identity and PATs |
| [`decisions/0008-postgresql-lease-worker-outbox.md`](decisions/0008-postgresql-lease-worker-outbox.md) | PostgreSQL lease, worker, outbox |
| [`decisions/0009-signalr-realtime.md`](decisions/0009-signalr-realtime.md) | SignalR realtime updates |
| [`decisions/0010-single-host-mvp.md`](decisions/0010-single-host-mvp.md) | Single-host MVP |
| [`decisions/0011-deployment-state-machine.md`](decisions/0011-deployment-state-machine.md) | Canonical deployment state machine |
| [`decisions/0012-apache-2-license.md`](decisions/0012-apache-2-license.md) | Apache-2.0 outbound license proposal |
| [`decisions/0013-no-desktop-app-mvp.md`](decisions/0013-no-desktop-app-mvp.md) | No desktop app in the MVP |
| [`decisions/0014-plugin-model-deferred.md`](decisions/0014-plugin-model-deferred.md) | Plugin model deferred |
| [`decisions/0015-openapi-contract-first.md`](decisions/0015-openapi-contract-first.md) | OpenAPI contract-first |
| [`decisions/0016-uuidv7-identifiers.md`](decisions/0016-uuidv7-identifiers.md) | UUIDv7 identifiers |
| [`decisions/0017-envelope-encryption-master-key.md`](decisions/0017-envelope-encryption-master-key.md) | Envelope encryption and master key |
| [`decisions/0018-git-cli-process-boundary.md`](decisions/0018-git-cli-process-boundary.md) | Git CLI process boundary |
| [`decisions/0019-no-auto-rollback.md`](decisions/0019-no-auto-rollback.md) | No auto-rollback |
| [`decisions/0020-explicit-migrations.md`](decisions/0020-explicit-migrations.md) | Explicit migrations |

## Backlog

| File | Purpose |
|---|---|
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Issue format, workflow, coverage rules, sizing, milestone corrections |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Requirement-to-issue and story-to-issue coverage |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Issue counts and dependency order by milestone |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic index files generated from issue frontmatter |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Epic epic issue grouping |

### Foundation — walking skeleton

| Issue | Title |
|---|---|
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Create the solution skeleton and validated configuration |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Add health endpoints, ProblemDetails, request IDs, Docker capability detection |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Local stack, migration bundle, server image, CI gates |

### Setup and identity — setup, identity, and projects

| Issue | Title |
|---|---|
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | First-run setup, local login, sessions, auth shell |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Roles, personal access tokens, tenant isolation |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Host preflight and explicit public URL/trusted proxy |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Versioned OpenAPI contract and dashboard boundaries |

### Projects, detection, and builds — projects, detection, and build

| Issue | Title |
|---|---|
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Project CRUD, archive/restore, bounded pagination |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Connect GitHub, GitLab, and generic HTTPS sources safely |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Repository limits and signed, deduplicated webhooks |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Detect supported stacks and produce build plans |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Versioned `marsa.json` schema and configuration precedence |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Harden the private build workspace |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Build and pin immutable images with redacted logs |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Project/source screens and destructive-action confirmations |

### Deployment and observation — deploy, observe, and recover

| Issue | Title |
|---|---|
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Runtime containers, labels, limits, no privileged access |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Health-gated release and previous-release retention |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Durable, single-flight, idempotent admission |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Leases and deterministic worker-crash recovery |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Cancellation, health-gated activation, reconciliation |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Capture, redact, persist, filter, and page logs |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | SignalR events and the live deployment page |

### Routing and security — route and secure

| Issue | Title |
|---|---|
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Caddy Unix-socket adapter and managed route compilation |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Hostname verification and certificate status |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Health-gated cutover and old-route preservation |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Envelope-encrypted secret versions and injection |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Append-only audit trail and scoped visibility |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Domain, member/token, secret metadata, and audit screens |

### Operations and recovery — operate and recover

| Issue | Title |
|---|---|
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Explicit rollback and deployment-history retention |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Marsa CLI for auth, deploy, status, logs, env, domains, secrets |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Dashboard create-to-rollback workflow |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Arabic/English localization, RTL, accessibility |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Redacted diagnostics and API rate limits |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Opt-in metrics/tracing, retention, disk pressure |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Authorization, input, Docker guardrail, failure-injection matrix |

### Release candidate — release candidate

| Issue | Title |
|---|---|
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Clean-host install and expand/contract upgrade |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Backup/restore proof and operator runbooks |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | SBOM, checksums, license inventory, release artifacts |
| [`https://github.com/ofuqq/marsa/issues`](https://github.com/ofuqq/marsa/issues) | Release E2E suite across browsers and failure scenarios |

## Root project files

| File | Purpose |
|---|---|
| [`../README.md`](../README.md) | Project overview and current status |
| [`../AGENTS.md`](../AGENTS.md) | Operating contract for contributors |
| [`../CONTRIBUTING.md`](../CONTRIBUTING.md) | Contribution workflow, DCO, clean-room and naming rules |
| [`../CODE_OF_CONDUCT.md`](../CODE_OF_CONDUCT.md) | Community behavior and enforcement |
| [`../SECURITY.md`](../SECURITY.md) | Vulnerability reporting and security expectations |
| [`../LICENSE`](../LICENSE) | Apache License 2.0 text |
| [`../NOTICE`](../NOTICE) | Copyright and project notice |
| [`../THIRD-PARTY-NOTICES.md`](../THIRD-PARTY-NOTICES.md) | Third-party inventory policy and current status |
| `../labels.yml` | 42 GitHub labels |
| `../opencode.json` | opencode configuration and write permissions |
| [`../CODEOWNERS`](../CODEOWNERS) | Default code ownership |

## Documentation conventions

- `00-identity.md` is canonical for names and identifiers.
- `01-product-requirements.md` is canonical for requirement IDs and priorities.
- `03-architecture.md`, `05-data-model.md`, and `06-api-contracts.md` are canonical for implementation contracts.
- `07-security.md` is canonical for the threat model; `07-risks-and-open-questions.md` is canonical for decisions, questions, and assumptions.
- `08-roadmap.md` is canonical for construction order and the MVP cut line.
- Arabic user-facing copy follows the documented conventions.
- If a document and an ADR disagree, the ADR’s status and the decision log determine which is current. Record the conflict rather than silently choosing one.

## Validation

```bash
python3 scripts/check-docs.py
python3 scripts/validate-backlog.py
./scripts/sync-labels.sh --dry-run
./scripts/create-issues.sh --dry-run
```

`check-docs.py` verifies required files and relative links. `validate-backlog.py` verifies that every MUST functional requirement and every NFR maps to at least one issue. The GitHub scripts default to dry-run; never run `--apply` without owner intent.
