# Marsa Documentation Index

> Status: documentation-only repository. No Marsa product release exists.
> Last updated: 2026-10-22.
> This index lists every Markdown file under `docs/`. Root project files are listed separately.

## Start here

| If you want to… | Read |
|---|---|
| Understand why Marsa exists | [`00-vision.md`](00-vision.md) |
| Know the canonical names and URLs | [`00-identity.md`](00-identity.md) |
| See what 0.1 must do | [`01-product-requirements.md`](01-product-requirements.md) |
| Understand the design | [`03-architecture.md`](03-architecture.md) |
| Review the security model | [`07-security.md`](07-security.md) |
| See the construction order | [`08-roadmap.md`](08-roadmap.md) |
| Pick up an implementation task | [`backlog/README.md`](backlog/README.md) |
| Work with an AI agent | [`../AGENTS.md`](../AGENTS.md) and [`ai/README.md`](ai/README.md) |
| Know when a change is done | [`ai/definition-of-done.md`](ai/definition-of-done.md) |

## Core documents

| File | Purpose | Status |
|---|---|---|
| [`00-vision.md`](00-vision.md) | Problem, users, principles, non-goals, naming evidence | Decided |
| [`00-identity.md`](00-identity.md) | Canonical product name, brand, handle, URLs, prefixes | Decided |
| [`01-product-requirements.md`](01-product-requirements.md) | 151 requirement IDs, acceptance criteria, metrics | Phase 2 approved |
| [`02-differentiation.md`](02-differentiation.md) | Honest competitor comparison, differentiation bets, validation plan | Phase 2 approved |
| [`03-architecture.md`](03-architecture.md) | Modular monolith, components, state machine, trust boundaries | Proposed for 0.1 |
| [`04-dotnet-solution-structure.md`](04-dotnet-solution-structure.md) | Projects, references, repository layout, conventions | Proposed for 0.1 |
| [`05-data-model.md`](05-data-model.md) | Tables, constraints, indexes, invariants, retention | Proposed for 0.1 |
| [`06-api-contracts.md`](06-api-contracts.md) | `/api/v1` behavior, errors, webhooks, SignalR | Proposed for 0.1 |
| [`07-security.md`](07-security.md) | Threat model, auth, secrets, Docker/Caddy boundaries | Proposed for 0.1 |
| [`07-risks-and-open-questions.md`](07-risks-and-open-questions.md) | Decision register, open questions, assumptions, risks | Living register |
| [`08-roadmap.md`](08-roadmap.md) | M0–M6 milestones, epics, MVP cut line, release plan | Phase 2 approved |
| [`09-testing-strategy.md`](09-testing-strategy.md) | Test layers, required cases, CI and E2E lanes | Proposed for 0.1 |
| [`10-dev-environment.md`](10-dev-environment.md) | Prerequisites, local stack, database and test workflow | Proposed for 0.1 |
| [`11-release-and-versioning.md`](11-release-and-versioning.md) | SemVer, artifacts, upgrade, backup, release gates | Proposed for 0.1 |
| [`12-license-and-clean-room.md`](12-license-and-clean-room.md) | Apache-2.0 proposal, dependency policy, clean-room evidence | Proposed for 0.1 |

## Architecture decision records

The ADR index contains the status and summary of each decision. Every ADR below is linked from it.

| ADR | Topic |
|---|---|
| [`adr/README.md`](adr/README.md) | ADR index and status |
| [`adr/0001-modular-monolith.md`](adr/0001-modular-monolith.md) | Modular monolith |
| [`adr/0002-dotnet-10-lts.md`](adr/0002-dotnet-10-lts.md) | .NET 10 LTS |
| [`adr/0003-postgresql-only-stateful-dependency.md`](adr/0003-postgresql-only-stateful-dependency.md) | PostgreSQL as the only stateful dependency |
| [`adr/0004-caddy-edge.md`](adr/0004-caddy-edge.md) | Caddy edge and Unix admin socket |
| [`adr/0005-docker-engine-socket.md`](adr/0005-docker-engine-socket.md) | Docker Engine socket boundary |
| [`adr/0006-razor-components.md`](adr/0006-razor-components.md) | Razor Components, no SPA |
| [`adr/0007-local-identity-and-pats.md`](adr/0007-local-identity-and-pats.md) | Local identity and PATs |
| [`adr/0008-postgresql-lease-worker-outbox.md`](adr/0008-postgresql-lease-worker-outbox.md) | PostgreSQL lease, worker, outbox |
| [`adr/0009-signalr-realtime.md`](adr/0009-signalr-realtime.md) | SignalR realtime updates |
| [`adr/0010-single-host-mvp.md`](adr/0010-single-host-mvp.md) | Single-host MVP |
| [`adr/0011-deployment-state-machine.md`](adr/0011-deployment-state-machine.md) | Canonical deployment state machine |
| [`adr/0012-apache-2-license.md`](adr/0012-apache-2-license.md) | Apache-2.0 outbound license proposal |
| [`adr/0013-no-desktop-app-mvp.md`](adr/0013-no-desktop-app-mvp.md) | No desktop app in the MVP |
| [`adr/0014-plugin-model-deferred.md`](adr/0014-plugin-model-deferred.md) | Plugin model deferred |
| [`adr/0015-openapi-contract-first.md`](adr/0015-openapi-contract-first.md) | OpenAPI contract-first |
| [`adr/0016-uuidv7-identifiers.md`](adr/0016-uuidv7-identifiers.md) | UUIDv7 identifiers |
| [`adr/0017-envelope-encryption-master-key.md`](adr/0017-envelope-encryption-master-key.md) | Envelope encryption and master key |
| [`adr/0018-git-cli-process-boundary.md`](adr/0018-git-cli-process-boundary.md) | Git CLI process boundary |
| [`adr/0019-no-auto-rollback.md`](adr/0019-no-auto-rollback.md) | No auto-rollback |
| [`adr/0020-explicit-migrations.md`](adr/0020-explicit-migrations.md) | Explicit migrations |

## AI-assisted development

| File | Purpose |
|---|---|
| [`ai/README.md`](ai/README.md) | Agent reading order, allowed actions, restart note |
| [`ai/agent-guide.md`](ai/agent-guide.md) | Task classification, planning, uncertainty, evidence |
| [`ai/architecture-context.md`](ai/architecture-context.md) | Boundaries, invariants, states, health, routing |
| [`ai/coding-standards.md`](ai/coding-standards.md) | .NET, API, EF Core, Docker/Caddy, UI rules |
| [`ai/testing-guide.md`](ai/testing-guide.md) | Test layers, fixtures, commands, required coverage |
| [`ai/workflows.md`](ai/workflows.md) | Issue, bug, docs, ADR, migration, API, release workflows |
| [`ai/known-pitfalls.md`](ai/known-pitfalls.md) | Common failure modes and guardrails |
| [`ai/review-checklist.md`](ai/review-checklist.md) | Evidence-based self-review checklist |
| [`ai/definition-of-done.md`](ai/definition-of-done.md) | Completion gate, Arabic explanation rule, handoff update |

## Backlog

| File | Purpose |
|---|---|
| [`backlog/README.md`](backlog/README.md) | Issue format, workflow, coverage rules, sizing, milestone corrections |
| [`backlog/coverage.md`](backlog/coverage.md) | Requirement-to-issue and story-to-issue coverage |
| [`backlog/milestones.md`](backlog/milestones.md) | Issue counts and dependency order by milestone |
| [`backlog/epics/`](backlog/epics/) | Epic index files generated from issue frontmatter |
| [`backlog/epics/E0.md`](backlog/epics/E0.md) | Epic E0 issue grouping |
| [`backlog/epics/E1.md`](backlog/epics/E1.md) | Epic E1 issue grouping |
| [`backlog/epics/E2.md`](backlog/epics/E2.md) | Epic E2 issue grouping |
| [`backlog/epics/E3.md`](backlog/epics/E3.md) | Epic E3 issue grouping |
| [`backlog/epics/E4.md`](backlog/epics/E4.md) | Epic E4 issue grouping |
| [`backlog/epics/E5.md`](backlog/epics/E5.md) | Epic E5 issue grouping |
| [`backlog/epics/E6.md`](backlog/epics/E6.md) | Epic E6 issue grouping |
| [`backlog/epics/E7.md`](backlog/epics/E7.md) | Epic E7 issue grouping |
| [`backlog/epics/E8.md`](backlog/epics/E8.md) | Epic E8 issue grouping |
| [`backlog/epics/E9.md`](backlog/epics/E9.md) | Epic E9 issue grouping |
| [`backlog/epics/E10.md`](backlog/epics/E10.md) | Epic E10 issue grouping |

### M0 — walking skeleton

| Issue | Title |
|---|---|
| [`backlog/issues/m0-01-solution-skeleton.md`](backlog/issues/m0-01-solution-skeleton.md) | Create the solution skeleton and validated configuration |
| [`backlog/issues/m0-02-health-errors-docker.md`](backlog/issues/m0-02-health-errors-docker.md) | Add health endpoints, ProblemDetails, request IDs, Docker capability detection |
| [`backlog/issues/m0-03-local-stack-images-ci.md`](backlog/issues/m0-03-local-stack-images-ci.md) | Local stack, migration bundle, server image, CI gates |

### M1 — setup, identity, and projects

| Issue | Title |
|---|---|
| [`backlog/issues/m1-01-setup-and-login.md`](backlog/issues/m1-01-setup-and-login.md) | First-run setup, local login, sessions, auth shell |
| [`backlog/issues/m1-02-rbac-pats-tenant-isolation.md`](backlog/issues/m1-02-rbac-pats-tenant-isolation.md) | Roles, personal access tokens, tenant isolation |
| [`backlog/issues/m1-03-preflight-and-public-url.md`](backlog/issues/m1-03-preflight-and-public-url.md) | Host preflight and explicit public URL/trusted proxy |
| [`backlog/issues/m1-04-api-contract.md`](backlog/issues/m1-04-api-contract.md) | Versioned OpenAPI contract and dashboard boundaries |

### M2 — projects, detection, and build

| Issue | Title |
|---|---|
| [`backlog/issues/m2-01-project-crud.md`](backlog/issues/m2-01-project-crud.md) | Project CRUD, archive/restore, bounded pagination |
| [`backlog/issues/m2-02-source-connections.md`](backlog/issues/m2-02-source-connections.md) | Connect GitHub, GitLab, and generic HTTPS sources safely |
| [`backlog/issues/m2-03-repo-limits-webhooks.md`](backlog/issues/m2-03-repo-limits-webhooks.md) | Repository limits and signed, deduplicated webhooks |
| [`backlog/issues/m2-04-detection-engine.md`](backlog/issues/m2-04-detection-engine.md) | Detect supported stacks and produce build plans |
| [`backlog/issues/m2-05-marsa-json.md`](backlog/issues/m2-05-marsa-json.md) | Versioned `marsa.json` schema and configuration precedence |
| [`backlog/issues/m2-06-source-export.md`](backlog/issues/m2-06-source-export.md) | Harden the private build workspace |
| [`backlog/issues/m2-07-build-pipeline.md`](backlog/issues/m2-07-build-pipeline.md) | Build and pin immutable images with redacted logs |
| [`backlog/issues/m2-08-project-source-ui.md`](backlog/issues/m2-08-project-source-ui.md) | Project/source screens and destructive-action confirmations |

### M3 — deploy, observe, and recover

| Issue | Title |
|---|---|
| [`backlog/issues/m3-01-runtime-container.md`](backlog/issues/m3-01-runtime-container.md) | Runtime containers, labels, limits, no privileged access |
| [`backlog/issues/m3-02-health-and-release-retention.md`](backlog/issues/m3-02-health-and-release-retention.md) | Health-gated release and previous-release retention |
| [`backlog/issues/m3-03-deployment-admission.md`](backlog/issues/m3-03-deployment-admission.md) | Durable, single-flight, idempotent admission |
| [`backlog/issues/m3-04-worker-lease-recovery.md`](backlog/issues/m3-04-worker-lease-recovery.md) | Leases and deterministic worker-crash recovery |
| [`backlog/issues/m3-05-cancel-activate-reconcile.md`](backlog/issues/m3-05-cancel-activate-reconcile.md) | Cancellation, health-gated activation, reconciliation |
| [`backlog/issues/m3-06-logs.md`](backlog/issues/m3-06-logs.md) | Capture, redact, persist, filter, and page logs |
| [`backlog/issues/m3-07-realtime-deploy-ui.md`](backlog/issues/m3-07-realtime-deploy-ui.md) | SignalR events and the live deployment page |

### M4 — route and secure

| Issue | Title |
|---|---|
| [`backlog/issues/m4-01-caddy-route-adapter.md`](backlog/issues/m4-01-caddy-route-adapter.md) | Caddy Unix-socket adapter and managed route compilation |
| [`backlog/issues/m4-02-domains-and-tls.md`](backlog/issues/m4-02-domains-and-tls.md) | Hostname verification and certificate status |
| [`backlog/issues/m4-03-health-gated-cutover.md`](backlog/issues/m4-03-health-gated-cutover.md) | Health-gated cutover and old-route preservation |
| [`backlog/issues/m4-04-secrets-envelope.md`](backlog/issues/m4-04-secrets-envelope.md) | Envelope-encrypted secret versions and injection |
| [`backlog/issues/m4-05-audit-trail.md`](backlog/issues/m4-05-audit-trail.md) | Append-only audit trail and scoped visibility |
| [`backlog/issues/m4-06-domains-members-settings-ui.md`](backlog/issues/m4-06-domains-members-settings-ui.md) | Domain, member/token, secret metadata, and audit screens |

### M5 — operate and recover

| Issue | Title |
|---|---|
| [`backlog/issues/m5-01-rollback-and-retention.md`](backlog/issues/m5-01-rollback-and-retention.md) | Explicit rollback and deployment-history retention |
| [`backlog/issues/m5-02-cli.md`](backlog/issues/m5-02-cli.md) | Marsa CLI for auth, deploy, status, logs, env, domains, secrets |
| [`backlog/issues/m5-03-dashboard-primary-flow.md`](backlog/issues/m5-03-dashboard-primary-flow.md) | Dashboard create-to-rollback workflow |
| [`backlog/issues/m5-04-i18n-rtl-accessibility.md`](backlog/issues/m5-04-i18n-rtl-accessibility.md) | Arabic/English localization, RTL, accessibility |
| [`backlog/issues/m5-05-diagnostics-and-rate-limits.md`](backlog/issues/m5-05-diagnostics-and-rate-limits.md) | Redacted diagnostics and API rate limits |
| [`backlog/issues/m5-06-metrics-retention-disk.md`](backlog/issues/m5-06-metrics-retention-disk.md) | Opt-in metrics/tracing, retention, disk pressure |
| [`backlog/issues/m5-07-hardening-matrix.md`](backlog/issues/m5-07-hardening-matrix.md) | Authorization, input, Docker guardrail, failure-injection matrix |

### M6 — release candidate

| Issue | Title |
|---|---|
| [`backlog/issues/m6-01-install-and-upgrade.md`](backlog/issues/m6-01-install-and-upgrade.md) | Clean-host install and expand/contract upgrade |
| [`backlog/issues/m6-02-backup-restore-runbooks.md`](backlog/issues/m6-02-backup-restore-runbooks.md) | Backup/restore proof and operator runbooks |
| [`backlog/issues/m6-03-supply-chain-and-release.md`](backlog/issues/m6-03-supply-chain-and-release.md) | SBOM, checksums, license inventory, release artifacts |
| [`backlog/issues/m6-04-e2e-release-gates.md`](backlog/issues/m6-04-e2e-release-gates.md) | Release E2E suite across browsers and failure scenarios |

## Root project files

| File | Purpose |
|---|---|
| [`../README.md`](../README.md) | Project overview and current status |
| [`../AGENTS.md`](../AGENTS.md) | Operating contract for contributors and agents |
| [`../CONTRIBUTING.md`](../CONTRIBUTING.md) | Contribution workflow, DCO, clean-room and naming rules |
| [`../CODE_OF_CONDUCT.md`](../CODE_OF_CONDUCT.md) | Community behavior and enforcement |
| [`../SECURITY.md`](../SECURITY.md) | Vulnerability reporting and security expectations |
| [`../LICENSE`](../LICENSE) | Apache License 2.0 text |
| [`../NOTICE`](../NOTICE) | Copyright and project notice |
| [`../THIRD-PARTY-NOTICES.md`](../THIRD-PARTY-NOTICES.md) | Third-party inventory policy and current status |
| [`../labels.yml`](../labels.yml) | 42 GitHub labels |
| [`../opencode.json`](../opencode.json) | opencode configuration and write permissions |
| [`../CODEOWNERS`](../CODEOWNERS) | Default code ownership |

## Documentation conventions

- `00-identity.md` is canonical for names and identifiers.
- `01-product-requirements.md` is canonical for requirement IDs and priorities.
- `03-architecture.md`, `05-data-model.md`, and `06-api-contracts.md` are canonical for implementation contracts.
- `07-security.md` is canonical for the threat model; `07-risks-and-open-questions.md` is canonical for decisions, questions, and assumptions.
- `08-roadmap.md` is canonical for construction order and the MVP cut line.
- Arabic user-facing copy follows `docs/ai/coding-standards.md`; the owner may keep a private glossary that is not part of this repository.
- If a document and an ADR disagree, the ADR’s status and the decision log determine which is current. Record the conflict rather than silently choosing one.

## Validation

```bash
python3 scripts/check-docs.py
python3 scripts/validate-backlog.py
./scripts/sync-labels.sh --dry-run
./scripts/create-issues.sh --dry-run
```

`check-docs.py` verifies required files and relative links. `validate-backlog.py` verifies that every MUST functional requirement and every NFR maps to at least one issue. The GitHub scripts default to dry-run; never run `--apply` without owner intent.
