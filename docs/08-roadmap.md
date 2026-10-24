# Marsa — Roadmap and MVP

> Status: **APPROVED** (Phase 2, 2026-10-22); MVP cut line added in Phase 7
> Date: 2026-10-22
> Depends on: [`00-vision.md`](00-vision.md), [`01-product-requirements.md`](01-product-requirements.md), [`02-differentiation.md`](02-differentiation.md)
> Architecture baseline: prior analysis notes on architecture, solution structure, data model, and API (kept outside this repository)

## 1. Product frame

Marsa is a self-hosted deployment control plane for **one Linux Docker host**. The MVP lets a small team connect a Git repository or image, detect or declare how to build it, deploy it, route a hostname to it over TLS, observe the deployment, and roll back safely.

### 1.1 MVP promise

> On one Linux host, I can install Marsa, connect a repository, deploy it behind HTTPS, see exactly what happened, and roll back to a known-good release — without SSH-ing into the box or hand-editing a reverse proxy.

### 1.2 MVP boundary

In scope:

- one Linux host, one Docker daemon, one organization per install;
- Git sources (GitHub, GitLab, generic HTTPS) and prebuilt OCI images;
- ASP.NET Core, Node/Next.js, static, Dockerfile, and prebuilt image stacks;
- one service per deployment;
- build, deploy, health gate, route, TLS, logs, secrets, audit;
- explicit rollback;
- local auth with roles and PATs;
- server-rendered dashboard, CLI, and `/api/v1`;
- Arabic/English UI and docs;
- install, upgrade, backup/restore drill, diagnostics.

Out of scope:

- multi-host, SSH targets, Kubernetes, clustering;
- Compose import, multi-service projects;
- app catalog, plugin marketplace;
- managed databases, scheduled backups UI;
- mail hosting;
- MCP/AI mutation, desktop app, SPA;
- PR previews, approvals, OIDC/SSO;
- wildcard DNS/TLS, multiple hostnames;
- automatic rollback;
- public SaaS, billing, telemetry-by-default.

The full requirement list is in [`01-product-requirements.md`](01-product-requirements.md). This roadmap does not add scope to that document.

### 1.3 Milestone map

```text
M0 Foundation
  -> M1 Setup, identity, and projects
  -> M2 Detect and build
  -> M3 Deploy and observe
  -> M4 Route and secure
  -> M5 Operate and recover
  -> M6 Release candidate
```

Each milestone is a **vertical, demonstrable slice**, not a layer. A milestone is complete only when its exit criterion passes on a clean Linux host, not only on a developer machine.

Sizes are relative (S/M/L/XL), not calendar estimates. The project assumes one primary maintainer with AI assistance; a larger team can parallelize the web/CLI and platform tracks without changing the dependency order.

---

## 2. Milestones

| Milestone | Theme | Exit criterion | Primary epics |
|---|---|---|---|
| **M0** | Walking skeleton | A clean checkout builds, tests, starts with PostgreSQL and Caddy, serves `/health/live` and `/health/ready`, and produces a runnable server image. | E0 |
| **M1** | Setup and identity | An owner completes setup, signs in, and the tenant/role invariants pass. | E0, E1 |
| **M2** | Projects, detect, and build | A project connects to a supported repository, detection explains the plan, and a build produces an immutable image with redacted logs. | E2, E3, E4 |
| **M3** | Deploy and observe | A release starts, passes health checks, and is visible in realtime and after refresh; a failed health check leaves the old release serving. | E4, E5, E7 |
| **M4** | Route and secure | A verified hostname serves the healthy release over HTTPS; a failed release never receives traffic; secrets are encrypted and redacted. | E5, E6, E7, E8 |
| **M5** | Operate and recover | A user can roll back, rotate a secret, inspect audit history, and complete the core workflow from the CLI and the Arabic dashboard. | E5, E8, E9 |
| **M6** | Release candidate | Clean-host install, upgrade from the previous release, backup/restore drill, E2E suite, license/SBOM review, and runbooks pass. | E0, E9, E10 |

### 2.1 Why this order

- **M0 first:** a broken build, migration, or health endpoint invalidates every later demo.
- **M1 before source work:** tenancy, authorization, and the setup token are boundaries; retrofitting them is expensive.
- **M2 before routing:** a route to an image that cannot be built or started is not a deployment.
- **M3 before TLS:** health and cutover correctness matter more than the certificate story; TLS is added only after the release can be proven healthy.
- **M4 before rollback UI:** rollback depends on a prior healthy release and a stable route.
- **M5 before release candidate:** rollback, audit, CLI, and Arabic completeness are part of the MVP, not polish.
- **M6 is a gate, not a feature phase:** hardening and runbooks run continuously from M3.

### 2.2 MVP cut line

The PRD has **151 requirement rows**. That is not 151 independent features:

| Category | Count | In the 0.1 gate? |
|---|---:|---|
| Functional MUST | 102 | Yes |
| Functional SHOULD | 11 | No; may slip only by recorded decision |
| Functional COULD | 1 | No |
| Functional OUT | 7 | No |
| Non-functional requirements (NFRs) | 30 | Yes |
| **Total** | **151** | **132 mandatory** |

**Verdict:** 151 is too many to manage as a single checklist, but the committed obligation is **132 mandatory requirements delivered through 39 vertical issues**. That is still large for a one-maintainer 0.1. The risk is breadth, integration, and polish — not arithmetic. Do not add features to compensate.

**Cut line for Marsa 0.1:** the 39-issue backlog, every one of the 132 mandatory requirements, and all seven milestones (M0–M6). The 11 SHOULD items may be deferred only by a recorded decision; the M6 release gates cannot be skipped. A build that stops at M0–M4 (86 functional MUST) is an internal demo, not a releasable product: rollback, audit, CLI, Arabic completion, install/upgrade, backup/restore, and the release E2E gates are still missing.

**If schedule pressure forces a smaller external release:** ship a **private preview**, not Marsa 0.1. The largest coherent preview is the M0–M5 vertical slice (97 functional MUST plus the applicable NFRs), with no public upgrade promise, no general-availability claim, and an explicit security warning. Record the scope change and the missing requirements; never relabel the preview as 0.1.

**Trim order:** defer SHOULD items first. If that is not enough, remove a whole post-MVP capability, not an individual MUST inside a core flow. A MUST can leave 0.1 only by changing the PRD, this roadmap, backlog coverage, and the decision log in the same change.

**Outside the line:** the 7 OUT items, the single COULD item, and everything in §8. This line does not authorize Compose, multi-host, Kubernetes, an app catalog, managed databases, MCP mutation, or a second stateful dependency.

---

## 3. Epics and stories

Every story has acceptance criteria and a relative size. The backlog in Phase 4 turns these into issues; IDs are stable and should be referenced in commits and PRs.

### E0 — Foundation and engineering system

**Outcome:** the repository is buildable, testable, and deployable before product features accumulate.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E0.1 | Solution skeleton and project boundaries | `dotnet build` succeeds with zero warnings; architecture tests reject forbidden references; layout matches the architecture document. | S | M0 |
| E0.2 | Central package management and reproducible restore | Versions are pinned; lock files are committed; CI restores in locked mode. | S | M0 |
| E0.3 | CI for format, build, unit, and integration tests | A pull request runs all gates on Linux; failures block merge; test results are retained. | M | M0 |
| E0.4 | Local development stack | `docker compose -f deploy/compose.dev.yaml up -d postgres caddy` starts dependencies; `dotnet run` serves `/health/live`. | S | M0 |
| E0.5 | Configuration and options validation | Missing database, master key, Caddy socket, or public URL fails startup with an actionable message; no secret is printed. | S | M0 |
| E0.6 | Logging, correlation IDs, and ProblemDetails | Every response has `X-Request-Id`; errors use the shared shape; production logs are structured JSON. | S | M0 |
| E0.7 | Migration workflow | Initial migration applies to an empty PostgreSQL 17 database; a migration bundle builds; startup refuses an incompatible schema. | M | M0 |
| E0.8 | Container images and compose stack | The server runs as non-root with a read-only root; the compose stack starts PostgreSQL, Caddy, and the server; health checks pass. | M | M0 |
| E0.9 | License, SBOM, and provenance gate | CI generates a dependency inventory and fails on unapproved licenses; `LICENSE` and `NOTICE` exist; no GPL/unknown code in the default image. | S | M6 |
| E0.10 | ADR and decision-log process | A template exists; every significant architectural choice has an ADR; the decision log is updated in the same PR. | S | M0 |
| E0.11 | Clean-room evidence ledger | Every non-trivial design derived from prior analysis has a provenance note; no copied prose/code/assets. | S | M0 |

**Exit:** M0 criterion passes on a clean Linux host.

---

### E1 — Setup, identity, and tenancy

**Outcome:** a secure first-run path and a correct organization/role model.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E1.1 | First-run setup | One-time setup token required; owner creates the organization; setup cannot be replayed; `setup.already_complete` is returned afterward. | M | M1 |
| E1.2 | Local login and sessions | Email/password login; `HttpOnly`, `Secure` in production, `SameSite=Lax`; logout invalidates the session; login is rate-limited. | M | M1 |
| E1.3 | Memberships and roles | Owner/admin/developer/viewer permissions match the matrix; the last owner cannot be removed; cross-organization access fails without leaking existence. | M | M1 |
| E1.4 | Personal access tokens | Create/list/rotate/revoke; plaintext shown once; hash stored; scopes cannot exceed the user's role; revocation is immediate. | M | M1 |
| E1.5 | CSRF and security headers | Cookie mutations require antiforgery; bearer requests are exempt; CSP, HSTS, frame, content-type, and referrer headers are set. | S | M1 |
| E1.6 | Tenant isolation tests | Every repository query is organization-scoped; an automated matrix attempts cross-tenant reads/writes and all fail. | M | M1 |
| E1.7 | Host preflight | Docker, BuildKit, PostgreSQL, Caddy, disk, architecture, and 80/443 are checked; failures show a fix hint. | M | M1 |
| E1.8 | Setup and auth audit | Setup, login success/failure, role change, and token changes are audited without secrets. | S | M1 |

**Exit:** an owner completes setup, signs in, and the tenant/role invariants pass.

---

### E2 — Projects and source connections

**Outcome:** projects exist, repositories are connected safely, and pushes can be received.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E2.1 | Project CRUD | Create/list/read/update/archive/restore; slugs unique per organization; archived projects hidden by default. | M | M2 |
| E2.2 | Connect a GitHub repository | Public repos work without a credential; private repos use a PAT; invalid credentials return `source.auth_failed`; the token is encrypted. | M | M2 |
| E2.3 | Connect GitLab and generic Git | GitLab PAT and generic HTTPS work; SSH, `file://`, and local paths are rejected. | M | M2 |
| E2.4 | Test source and list branches | Source test returns the default branch and up to 100 branches; failure does not persist a partial credential. | S | M2 |
| E2.5 | Repository size and file guards | Oversized repositories fail with `source.repo_too_large`; submodules are rejected with guidance; limits are configurable. | S | M2 |
| E2.6 | Webhook ingestion | GitHub/GitLab/generic push webhooks verify signatures, deduplicate deliveries, filter branches, and enqueue at most one deployment. | M | M2 |
| E2.7 | Webhook secret rotation | Admin rotates the secret; old secret stops immediately; new secret is shown once; audit records the rotation. | S | M2 |
| E2.8 | Source SSRF and redirect guards | Private/link-local targets and unsafe redirects are blocked; DNS re-resolution is considered. | M | M2 |
| E2.9 | Source credential encryption | Credentials are envelope-encrypted at rest and never returned by the API. | M | M2 |

**Exit:** a project connects to a supported repository and a signed push can enqueue a deployment.

---

### E3 — Detection and configuration

**Outcome:** Marsa can propose a correct plan, and users can pin it with `marsa.json`.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E3.1 | Detect ASP.NET Core | `.sln`/`.csproj`/`global.json`/`Program.cs` produces a web plan with port and health path; evidence is shown. | M | M2 |
| E3.2 | Detect Node/Next.js | `package.json` plus lockfile detects the package manager, build/start commands, and declared port; unsupported frameworks fail with guidance. | M | M2 |
| E3.3 | Detect static sites | A directory with `index.html` produces a static plan with the publish directory. | S | M2 |
| E3.4 | Detect Dockerfile | A Dockerfile produces a Dockerfile plan; a missing exposed port requires an explicit override. | S | M2 |
| E3.5 | Prebuilt image source | An OCI reference is accepted; a tag resolves to a digest; a missing port/health check requires explicit configuration. | M | M2 |
| E3.6 | Deterministic detection | Same commit + detector version = same plan; detection is cached by commit; detector version is recorded. | M | M2 |
| E3.7 | `marsa.json` schema v1 | Unknown fields, invalid hostnames, unsafe paths, and inline secrets are rejected with a JSON path; valid overrides apply. | M | M2 |
| E3.8 | Configuration precedence | API override > `marsa.json` > detection; effective config is stored per commit and visible in UI/CLI. | M | M2 |
| E3.9 | Unsupported guidance | `detection.unsupported` names missing evidence and suggests a Dockerfile or `marsa.json`. | S | M2 |
| E3.10 | Detection never executes repo scripts | A malicious repository cannot cause host execution; only declared build steps run inside Docker. | M | M2 |

**Exit:** supported sample repositories produce correct, reproducible plans.

---

### E4 — Build and runtime

**Outcome:** Marsa builds an immutable image and runs it with guardrails.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E4.1 | Docker connectivity and capabilities | Server detects Docker version, BuildKit, architecture, and cgroup version; missing Docker degrades health without crashing. | M | M0 |
| E4.2 | Source materialization | Selected commit exported to a private workspace; `.git` excluded; path traversal/symlink escape blocked; archive limits enforced. | M | M2 |
| E4.3 | Image build | Build produces an image tagged by commit/content hash; logs stream; timeout enforced; failures use stable codes. | L | M2 |
| E4.4 | Build cache and cleanup | Same commit reuses the image unless forced; failed workspaces cleaned; managed images pruned by policy without removing retained releases. | M | M2 |
| E4.5 | Container creation | Managed labels, read-only root where possible, resource limits, env snapshot, loopback host port, no privileged/host mounts. | L | M3 |
| E4.6 | Container lifecycle | Start/inspect/stop/remove/log are idempotent and refuse unmanaged containers. | M | M3 |
| E4.7 | Build/runtime failure taxonomy | Failures map to `build_failed`, `start_failed`, `health_timeout`, `route_conflict`, or `internal` with redacted diagnostics. | M | M3 |
| E4.8 | Health checks | HTTP/TCP with path, port, interval, timeout, retries, start period; crash-loop detection. | M | M3 |
| E4.9 | Image pull | Prebuilt images resolve to a digest and are pinned; pull failures return a stable code. | M | M2 |

**Exit:** a supported sample produces a runnable, health-checkable container.

---

### E5 — Deployment orchestration

**Outcome:** one active deployment per environment, a durable state machine, and safe recovery.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E5.1 | Deployment admission | Deployment row created only when the environment has no active deployment; duplicate idempotency keys return the original; environment row is locked. | M | M3 |
| E5.2 | State machine and step journal | States in the PRD are enforced; invalid transitions rejected; each step records start/end, status, code, and redacted evidence. | L | M3 |
| E5.3 | Worker lease and heartbeat | Claim with `SKIP LOCKED`; heartbeat renews; a dead worker's lease expires and another attempt resumes or fails deterministically. | L | M3 |
| E5.4 | Cancellation | Queued/running deployment cancelled at a safe boundary; old release stays serving; cleanup recorded. | M | M3 |
| E5.5 | Health gate | Release activates only after health + stabilization pass; timeout/crash-loop fails without route changes. | M | M3 |
| E5.6 | Idempotent recovery | Re-running prepare/build/start/activate is safe; the orchestrator inspects existing resources before creating new ones. | L | M3 |
| E5.7 | Rollback backend | New deployment pinned to a prior image digest/snapshot; same health gate; failed deployment remains in history; UI/CLI rollback in M5. | M | M5 |
| E5.8 | Reconciliation | Periodic reconciler detects container/route drift and repairs only managed resources; unsafe drift marks the server degraded. | M | M3 |
| E5.9 | Deployment retention | Configurable history; pruning never removes active or last known-good release. | S | M5 |
| E5.10 | No auto-rollback | A failed release leaves the old release serving and requires an explicit rollback action. | S | M3 |

**Exit:** a release activates only after health, a failed release does not receive traffic, and a killed worker recovers.

---

### E6 — Edge routing, domains, and TLS

**Outcome:** a healthy release is reachable on a verified hostname over HTTPS, and failures do not receive traffic.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E6.1 | Caddy admin adapter | Server reaches Caddy only over a Unix socket; ping/read/write works; the socket is not publicly reachable. | M | M4 |
| E6.2 | Route management | Add/replace/remove by stable route ID; unrelated config preserved; duplicate host/path conflicts rejected. | L | M4 |
| E6.3 | Domain verification | UI/CLI shows exact TXT/CNAME records; verification is idempotent; unverified hostname cannot be attached. | M | M4 |
| E6.4 | TLS issuance | Caddy obtains a certificate for a verified hostname; status is surfaced; failure does not break other routes; staging CA in tests. | M | M4 |
| E6.5 | Health-gated cutover | Route switches only after health passes; old route remains until success; failed activation leaves the old release serving. | M | M4 |
| E6.6 | Rollback routing primitive | A prior release can be re-pointed after its health gate; certificate state unchanged; end-to-end rollback UX is completed in M5. | M | M4 |
| E6.7 | Route reconciliation | Startup/periodic reconciliation removes only orphaned managed routes; foreign routes untouched. | M | M4 |
| E6.8 | HTTP to HTTPS redirect | Verified hostnames redirect HTTP to HTTPS without breaking ACME challenges. | S | M4 |

**Exit:** a verified hostname serves the healthy release over HTTPS; an unhealthy release never receives traffic.

---

### E7 — Logs, realtime, and observability

**Outcome:** an operator can see what is happening now and what happened after the fact.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E7.1 | Deployment log capture | Build/runtime logs captured, redacted, chunked, sequenced; bounded buffering. | M | M3 |
| E7.2 | Log query and download | Read after a sequence; filter by stream; download redacted text; retention enforced. | M | M3 |
| E7.3 | SignalR updates | Project/deployment groups; ordered events; reconnect replays after last sequence; slow clients do not block the worker. | M | M3 |
| E7.4 | Health endpoints | `/health/live`, `/health/ready`, `/health/degraded` report correct components; readiness fails when PostgreSQL/schema/master key/Caddy unavailable. | S | M0 |
| E7.5 | Correlation and tracing | Every request has an ID; deployment ID appears in logs/traces; OpenTelemetry export is opt-in. | M | M5 |
| E7.6 | Metrics | Deploy success rate, phase duration, queue age, lease expiry, route failures; documented format. | M | M5 |
| E7.7 | Log redaction | Secrets, tokens, credentials, and full env maps never appear in app/deployment/audit/telemetry; canary suite passes. | M | M4 |
| E7.8 | Log retention | Defaults: 30d deployment, 14d app, 1y audit; configurable; disk pressure handled. | S | M5 |

**Exit:** logs survive a refresh and a restart; a failed deploy has a readable timeline; no secret canary leaks.

---

### E8 — Secrets, environment, and audit

**Outcome:** configuration is safe by default and every sensitive action is attributable.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E8.1 | Secret storage | Envelope encryption at rest; immutable versions; API returns metadata only; reveal is admin-only, feature-flagged, audited. | L | M4 |
| E8.2 | Secret injection | Deployment snapshot carries ciphertext + key versions; plaintext only in container-create; logs/events never contain it. | M | M4 |
| E8.3 | Environment management | Env keys validated; secret references resolve at deployment; missing references fail at prepare. | M | M4 |
| E8.4 | Secret rotation | New version used by next deployment; old deployments keep snapshots; audit event written. | M | M5 |
| E8.5 | Audit trail | Login, setup, project/source changes, deploys, rollbacks, secrets, domains, tokens are recorded; append-only and filterable. | M | M4 |
| E8.6 | Audit visibility | Developer sees own actions; admins see all; viewers see no sensitive metadata; no secret values. | S | M4 |
| E8.7 | Master key handling | Key outside DB/image; missing key fails closed; backup/restore and rotation runbook exist. | M | M6 |

**Exit:** a secret can be set, used, rotated, and never appears in logs/API/audit; sensitive actions are attributable.

---

### E9 — CLI, dashboard, Arabic, and accessibility

**Outcome:** the same workflows are usable from a terminal and a browser, in Arabic and English.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E9.1 | CLI authentication | `marsa login`, `logout`, `whoami`; token stored `0600`; `--json`; documented auth exit code. | M | M5 |
| E9.2 | CLI project/deploy workflow | `projects list`, `deploy`, `status`, `logs --follow`; non-zero on failure; logs resume. | L | M5 |
| E9.3 | CLI env/domain/secrets metadata | `env`, `domains`, `secrets` within role limits; secret values never printed except explicit write confirmation. | M | M5 |
| E9.4 | CLI generated client | Client generated from OpenAPI; contract change fails generation; no duplicate models. | M | M5 |
| E9.5 | Dashboard setup and auth | Setup redirect, login/logout/current user; unauthorized pages return a safe error. | M | M1 |
| E9.6 | Dashboard deployment detail | Phases, step timeline, logs, and failure evidence remain visible after refresh; a failed deploy shows the failing step and next action. | M | M3 |
| E9.7 | Dashboard domains and settings | Add/verify hostname, copy DNS, certificate status, members/tokens/secrets metadata/audit. | M | M4 |
| E9.8 | Server-rendered UI | Razor Components; no SPA/Node build; interactive islands degrade gracefully. | M | M1 |
| E9.9 | Bilingual shell and strings | English/Arabic resource pipeline; language switch persists; layout direction flips; code/log blocks stay LTR. | M | M5 |
| E9.10 | Arabic UX completion | Onboarding, deploy pages, errors, domains, secrets, and audit copy reviewed by a native writer. | L | M5 |
| E9.11 | RTL and responsive tests | 360 px and 1280 px; forms, tables, logs, code blocks, navigation, dialogs. | M | M5 |
| E9.12 | Accessibility | Keyboard, focus, labels, error summaries; primary flows pass manual + automated checks. | M | M5 |
| E9.13 | Destructive-action confirmation | Archive/delete/rollback/cancel explain impact and what is retained. | S | M2 |
| E9.14 | Dashboard project and rollback flow | Create/connect/review detection/deploy/rollback end to end from the dashboard; role limits enforced; no dead ends. | L | M5 |

**Exit:** a user completes the core flow from the CLI and from the Arabic dashboard without switching to English.

---

### E10 — Hardening and release

**Outcome:** the MVP is safe enough to expose to a small team and recoverable by an operator.

| ID | Story | Acceptance criteria | Size | Milestone |
|---|---|---|---|---|
| E10.1 | Authorization matrix tests | Every endpoint tested for anonymous/viewer/developer/admin/owner/cross-tenant/revoked token. | M | M5 |
| E10.2 | Input and transport hardening | Hostname, path, repo URL, branch, env key, image reference validators have negative tests; SSRF/path traversal tests pass. | M | M5 |
| E10.3 | Docker socket guardrails | Server refuses unmanaged containers; label spoofing bounded; socket mount documented as root-equivalent. | S | M5 |
| E10.4 | Failure injection | Docker down, Caddy down, PostgreSQL restart, worker kill, full disk, slow health; behavior matches runbook. | L | M5 |
| E10.5 | Backup and restore drill | Documented `pg_dump`/restore restores projects/deployments/secrets with the master key; readiness green. | M | M6 |
| E10.6 | Clean-host install and upgrade | Fresh Linux host installs from the artifact; upgrade from previous release applies migrations and preserves data. | L | M6 |
| E10.7 | E2E suite | Happy path, failed build, failed health, rollback, cancellation, route conflict on a disposable Docker runner. | L | M6 |
| E10.8 | Runbooks | Install, upgrade, backup, restore, rotate master key, troubleshoot, uninstall; tested by someone who did not write them. | M | M6 |
| E10.9 | License and SBOM review | No GPL/unknown in default distribution; notices complete; CI gate blocks regressions. | M | M6 |
| E10.10 | Release artifacts | Multi-arch server image, CLI binaries, migration bundle, compose/Caddy/systemd templates, checksums, SBOM, notes. | M | M6 |

**Exit:** M6 criterion and §9.2 security/reliability gates pass.

---

## 4. Cross-cutting acceptance criteria

### 4.1 Security

- No default credentials; setup requires a one-time token.
- Passwords hashed by ASP.NET Core Identity; PATs stored only as hashes.
- All tenant queries organization-scoped; cross-tenant probes return no existence signal.
- Cookie mutations require antiforgery; cookie flags correct in production.
- Secrets encrypted at rest and redacted everywhere.
- Docker operations label-guarded; Caddy admin Unix-socket only.
- Hostnames, paths, repo URLs, and image references validated.
- Dependency/container scans have no unapproved High/Critical findings at release.
- Rate limits cover login, setup, deploy, webhooks, and log streaming.

### 4.2 Reliability

- At most one active deployment per environment, enforced by a partial unique index.
- Worker crash recovery completes within 2 × lease duration.
- A failed deployment never changes the active route.
- A Caddy restart is reconciled without manual database edits.
- A server restart resumes queued work and preserves logs.
- Shutdown is graceful; in-flight steps finish within the timeout or are recoverable.
- All external calls have timeouts and bounded retries; retries are idempotent.

### 4.3 Observability

- Every deployment has a timeline, step states, failure code, and redacted logs.
- Every request has a correlation ID; every deployment ID appears in logs/traces.
- Metrics cover deploy success rate, phase duration, queue age, lease expiry, route failures.
- Health endpoints distinguish liveness, readiness, and degraded capability.

### 4.4 Performance (design targets)

| Operation | Target |
|---|---|
| API read p95 | < 200 ms excluding cold start |
| API write p95 | < 500 ms excluding deployment work |
| Deployment admission | < 1 s |
| Small app deploy with cached base image | < 3 minutes admission to active |
| Health detection | Within configured interval + timeout |
| Log delivery | < 1 s write to subscriber on a healthy connection |
| Dashboard initial render | < 2 s on a local network |

### 4.5 UX and language

- Setup has no dead ends and explains where the setup token came from.
- A failed deployment always shows the failing step and a next action.
- Domain setup shows copyable DNS records and a verification state.
- Destructive actions require confirmation and explain impact.
- The CLI works non-interactively with `--json` and `--yes`.
- Arabic is reviewed by a native writer; RTL passes at 360 px and 1280 px.
- Primary flows are keyboard-usable with labels and error summaries.

---

## 5. Dependencies and critical path

```text
E0 -> E1 -> E2 -> E3 -> E4 -> E5 -> E6 -> E7 -> E9 -> E10
                         \-> E8 --------/
```

- E3 needs E2 for a resolved commit.
- E4 needs E3 for the effective plan and E0 for Docker connectivity.
- E5 needs E4 for image/container operations.
- E6 needs E5 for a healthy release and E1 for authorization.
- E7 can start once E5 emits events; it does not block E5.
- E8 can run in parallel with E5/E6 after the schema is stable.
- E9 can start after the M1 contracts freeze; the CLI can be built against a stub API.
- E10 starts at M3 and runs continuously; it is not a final cleanup phase.

### 5.1 Critical path

`source connection -> detection -> build -> container start -> health gate -> route -> rollback proof`

Any delay in the health gate or route reconciliation delays the release, because both are required for the core promise. Arabic/RTL and CLI are parallel workstreams, not afterthoughts, but they cannot compensate for a broken deployment engine.

### 5.2 Parallel workstreams

| Track | Work | Can start |
|---|---|---|
| Backend core | E3, E4, E5, E6, E8 | M1 |
| Web/CLI | E9 (Arabic shell from M1; full flow after M1 contracts) | M1 |
| Platform/ops | E0, E7, E10 | M0 |
| Documentation | ADRs, runbooks, Arabic explainer | Continuous |
| Security | Model, tests, redaction, review | M1 |

**WIP limit:** one active epic per track unless a second maintainer is available. The critical path wins conflicts.

---

## 6. Definition of Done

A story is done only when:

1. The behavior matches the documented contract.
2. Unit tests cover domain rules and failure paths.
3. Integration tests cover persistence, auth, or adapter behavior where applicable.
4. The OpenAPI diff is additive and documented.
5. Logs are structured and redacted; new error codes are added to the catalog.
6. Authorization is enforced and covered by a matrix test.
7. Metrics/traces exist for new long-running operations.
8. Dashboard and CLI paths are updated if the story is user-facing.
9. Arabic/RTL impact is addressed for user-facing strings and layout.
10. Documentation/runbook impact is addressed.
11. No new dependency lacks a license entry.
12. CI gates pass and a reviewer checklist is complete.
13. The feature works on a clean host, not only on the developer's machine.

A milestone is done only when its exit criterion is demonstrated in the E2E environment and the runbook covers the new failure modes.

---

## 7. Metrics

| Metric | Target | How measured |
|---|---|---|
| Setup completion | ≥ 80% of started installs in usability tests | Facilitated funnel |
| First-deploy success on supported stacks | ≥ 70% of first attempts | E2E + usability |
| Deploy success excluding user build errors | ≥ 95% | Deployment outcomes |
| Median sample-app deploy | < 3 minutes with cached base image | Phase durations |
| Rollback success | 100% in E2E; no route regression | E2E + audit |
| Time to diagnose a failed deploy | < 5 minutes | Usability test |
| Secret leakage | 0 canary findings | Redaction suite |
| Unmanaged container operations | 0 | Adapter guard tests |
| Active deployments per environment | ≤ 1 at all times | DB invariant + reconciliation |
| Arabic flow completion without English fallback | ≥ 80% in usability tests | Facilitated session |
| Accessibility | Primary flows keyboard-usable; labels/error summaries present | Manual + automated |

These are design targets, not contractual SLAs. They are measured with consent, not silent telemetry.

---

## 8. Post-MVP roadmap

The MVP is the only milestone that should be in the initial backlog. Everything below is a direction, not a commitment.

### Phase 2 — Team and multi-service depth

| Epic | Outcome | Why now |
|---|---|---|
| Multi-service projects | Web + worker + database services from one plan | The most likely first expansion request |
| Compose import | Supported `docker-compose.yml` subset with warnings | Meets existing project layouts |
| GitHub App | Installation tokens, repo picker, automatic webhook registration | Reduces onboarding friction |
| OIDC/SSO | Organization-level OIDC with role mapping | Small-team adoption |
| Preview environments | PR previews with automatic expiry | Review workflow |
| Approvals | Required approval before production | Team safety |
| Backup UI | Scheduled logical backups and restore workflow for Marsa data | Data-safety expectation |
| Private registries | Credentials and digest pinning | Enterprise/private images |
| Notifications | Email/webhook/Slack deploy events | Operational awareness |
| Worker split | Dedicated worker process with the same PostgreSQL queue | Scale/reliability |
| Remote servers | SSH or mTLS agent for a small number of hosts | Multi-host demand |

### Phase 3 — Platform breadth

| Epic | Outcome | Risk |
|---|---|---|
| Multi-host scheduling | Placement, capacity, failover | Different product; high complexity |
| Kubernetes target | Optional runtime adapter | Conflicts with “boring” promise if default |
| Application catalog | Curated templates with pinned images | Maintenance burden |
| Managed resources | PostgreSQL/Redis provisioning and lifecycle | Data-loss responsibility |
| MCP server | Read-only and deploy-scoped agent tools | Security boundary |
| Mail | Optional mail integration with explicit GPL boundary and packaging | License/operations |
| Plugin model | Signed, sandboxed extensions, not arbitrary assembly loading | Security |
| Enterprise controls | SCIM, audit export, retention, compliance reports | Support burden |

### Phase 4 — Community and sustainability

| Epic | Outcome |
|---|---|
| Arabic documentation expansion | More tutorials, video scripts, and examples written in Arabic |
| Migration guides | From manual Docker, Dokku, Coolify, and VPS scripts |
| Hosting/support offering | A sustainable business model without closing the core |
| Public roadmap and RFC process | Community contribution without scope chaos |
| Design partner program | Five to ten operators using Marsa on real projects |

### 8.1 Explicitly deferred or rejected

- Running arbitrary user shell commands on the host.
- A general-purpose CI system.
- A public container registry.
- A general-purpose secrets manager beyond deployment needs.
- Multi-region control planes.
- Windows containers.
- Silent telemetry or a mandatory cloud account.
- A plugin marketplace before the API and authorization model are stable.
- GPL code in the default distribution.

---

## 9. Release plan

### 9.1 Release artifacts

- `ghcr.io/ofuqq/marsa-server` OCI image, pinned by digest, `linux/amd64` and `linux/arm64`.
- `marsa` CLI binaries for Linux and macOS (amd64/arm64).
- Migration bundle.
- `compose.yaml`, Caddy template, systemd unit.
- SBOM and checksums.
- Release notes with breaking changes and upgrade steps.
- `LICENSE` and `NOTICE` with the reviewed dependency inventory.

### 9.2 Release checklist

- [ ] All CI gates pass on the release commit.
- [ ] E2E suite passes on a clean Linux runner.
- [ ] Clean-host install passes.
- [ ] Upgrade from the previous release passes with a database snapshot.
- [ ] Backup/restore drill passes, including secrets and master key.
- [ ] Security review checklist is signed off.
- [ ] License/SBOM review is complete.
- [ ] OpenAPI diff is reviewed and documented.
- [ ] Arabic and RTL checks pass for primary flows.
- [ ] Runbooks are updated.
- [ ] Rollback of Marsa itself is documented (previous image + compatible schema).
- [ ] Trademark/domain release gate is resolved.

### 9.3 Versioning and upgrade policy

- Semantic Versioning; pre-1.0 releases may add features but must document breaking changes.
- Migrations use expand/contract; a new binary must run against the previous schema during rollout.
- The previous release remains deployable for at least one minor version.
- A downgrade requires a database restore; it is documented, not automatic.
- Breaking API changes require `/api/v2` and a deprecation window.
- Published package identifiers use the `ofuqq` handle; product-facing names use Marsa.

### 9.4 Support envelope

- Supported host: Linux x86_64/arm64, Docker 27+, PostgreSQL 17, Caddy bundled.
- Supported browsers: current and previous major versions of Chrome, Firefox, Safari, Edge.
- No SLA for 0.1. The README states the single-host limits and the Docker socket trust boundary.
- Security reports follow `SECURITY.md` (Phase 4).

---

## 10. Risks and kill criteria

| Risk | Why it matters | Mitigation | Trigger |
|---|---|---|---|
| Docker socket compromise | Root-equivalent host access | Guardrails, no privileged workloads, label checks, documented boundary, evaluate proxy/agent | Any endpoint accepts arbitrary Docker parameters |
| Failed release receives traffic | Destroys trust | Health gate, old route retained, failure injection | Route points to unhealthy container |
| Secret leak | Irreversible exposure | Envelope encryption, redaction suite, write-only API | Canary appears in logs/events/audit |
| Scope creep into Coolify | A worse broad product | Non-goals, one-service invariant, ADR for dependencies | A story requires a new runtime abstraction |
| Detection is wrong | First deploy fails | Four stacks, evidence, overrides, guidance | High `detection.unsupported` rate |
| Arabic is cosmetic | Damages the core promise | Native copy, RTL tests, Arabic explainer | Users switch to English to finish |
| Single-host ceiling | Users leave | Honest limits, adapter seams, post-MVP path | More than half of interviews need multi-host |
| Operator cannot restore | Data-loss blame | Backup/restore drill, runbooks, master-key docs | Restore drill fails |
| Clean-room drift | Legal exposure | Provenance ledger, no copy, license gate | Unknown/GPL file in default image |
| Solo-maintainer burnout | Roadmap stalls | Vertical milestones, WIP limit, explicit deferrals | Two milestones slip without scope cuts |
| Trademark/domain conflict | Rebrand after release is expensive | Naming centralized; clearance/domain before public launch | Legal review blocks the name |

### 10.1 Kill or re-scope criteria

Re-scope before launch if:

- Docker socket access cannot be made acceptable with guardrails; replace with an agent/socket proxy first.
- Caddy admin mutations cannot be made reconciliation-safe; switch to a config-file + reload model.
- .NET 10 or a core package is not viable; move to the latest supported LTS before building features.
- The MVP requires multi-service orchestration to be useful; re-scope the milestone rather than bolting Compose onto a one-service model.
- The Arabic experience cannot meet the quality bar; lead with “legible single-host” and delay the Arabic-first claim.
- The clean-room process cannot be evidenced; do not ship the affected module.

---

## 11. Traceability

| PRD area | Epics | Milestone |
|---|---|---|
| Setup, auth, tenancy | E0, E1 | M0–M1 |
| Projects and source | E2 | M1–M2 |
| Detection and `marsa.json` | E3 | M2 |
| Build and runtime | E4 | M0, M2–M3 |
| Deployment orchestration | E5 | M3, M5 |
| Domains and TLS | E6 | M4 |
| Logs and observability | E7 | M0, M3–M5 |
| Secrets and audit | E8 | M4–M6 |
| CLI, dashboard, Arabic, accessibility | E9 | M1–M5 |
| Hardening and release | E10 | M3–M6 |

The Phase 4 backlog will use these epic and story IDs. Post-MVP epics are intentionally excluded from the MVP backlog so the milestone stays finishable.
