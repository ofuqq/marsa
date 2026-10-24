# Marsa — Product Requirements

> Status: **APPROVED** (approved, 2026-10-22)
> Date: 2026-10-22
> Depends on: [`../vision.md`](vision.md), [`../positioning.md`](positioning.md)
> Implementation sequence: [`../roadmap.md`](roadmap.md)
> Canonical names: [`../identity.md`](identity.md)

## 1. Purpose and scope

This document defines **what Marsa 0.1 must do**, for whom, and how completion is judged. It is the requirements baseline for architecture, backlog, tests, and the Arabic explainer.

It is deliberately narrower than a general-purpose PaaS. Marsa 0.1 is:

- a **self-hosted deployment control plane** for **one Linux host**;
- **Docker-first**, with **one service per deployment**;
- reachable through a **server-rendered dashboard**, a **CLI**, and a **versioned HTTP API**;
- safe by default: no default credentials, no secret in logs, no traffic to an unhealthy release, no silent host mutation;
- **.NET-first** in its best-supported path and **Arabic/RTL-first** in its user experience.

**Out of scope for this document:** detailed schema, endpoint payloads, class design, and infrastructure diagrams. Those belong in `03`–`06`. This document states behavior and acceptance criteria; later documents may refine implementation without weakening these requirements.

### 1.1 Requirement language

- **MUST** — required for Marsa 0.1. A release without it is not Marsa 0.1.
- **SHOULD** — expected, but may be deferred with a recorded decision if it threatens the release.
- **COULD** — useful, explicitly optional for 0.1.
- **OUT** — excluded from 0.1; see §10.

Every requirement has a stable ID, a milestone, and acceptance criteria. IDs are never reused. A requirement can move milestone only through the decision log in [`../risks-and-open-questions.md`](risks-and-open-questions.md).

---

## 2. Product promise

> On one Linux host, I can install Marsa, connect a repository, deploy it behind HTTPS, see exactly what happened, and roll back to a known-good release — without SSH-ing into the box or hand-editing a reverse proxy.

The promise has four parts, and all four are requirements:

1. **Install and first deploy are guided.** The user is never asked to infer a hidden step.
2. **The deployment is observable.** Phases, logs, health evidence, and a stable failure code are visible after a refresh.
3. **The cutover is safe.** The previous release keeps serving until the new one passes health checks.
4. **Recovery is explicit.** Rollback is a visible action, not a hidden side effect.

---

## 3. Users and jobs

The canonical personas and non-users are in [`../vision.md`](vision.md) §3. This document adds the jobs that drive requirements.

| Persona | Primary job | Requirements that matter most |
|---|---|---|
| Solo .NET builder | “Put my repository online safely and let me undo a bad release.” | Setup, detection, build, health gate, rollback, logs, secrets, domain/TLS |
| Small team operator | “Give the team one auditable deployment path.” | Roles, PATs, audit, concurrency guard, CLI/API |
| Arabic-first learner | “Teach me in Arabic without a broken RTL experience.” | Arabic UI, RTL, error copy, Arabic explainer, accessibility |
| Platform-minded developer | “Give me a stable API and a state machine I can automate.” | OpenAPI, idempotency, error taxonomy, CLI `--json`, webhooks |

**Non-users for 0.1:** Kubernetes operators, multi-host fleets, Windows-container teams, no-code builders, public multi-tenant SaaS operators, mail-hosting teams, plugin-marketplace buyers.

---

## 4. Supported environments and stacks

### 4.1 Host requirements

| Item | Requirement |
|---|---|
| OS | Linux x86_64 or arm64 |
| Container runtime | Docker Engine 27+ with BuildKit |
| CPU / RAM | 2 vCPU / 4 GB minimum for the control plane plus one small app; 4 vCPU / 8 GB recommended |
| Disk | 20 GB free minimum; image/build/log growth monitored |
| Network | Outbound HTTPS for Git, registries, and ACME; inbound 80/443 for the edge |
| Database | PostgreSQL 17, containerized or external |
| Edge | Caddy managed by Marsa over a permissioned Unix socket |
| Rootless Docker | Best-effort, not a launch requirement |
| Windows/macOS hosts | OUT for 0.1 |
| Kubernetes / Nomad | OUT for 0.1 |

### 4.2 Source and workload support

| Source / stack | 0.1 status | Detection / build expectation |
|---|---|---|
| ASP.NET Core | **MUST** | Detect `.sln`, `.csproj`, `global.json`, or `Program.cs`; build with the .NET SDK; run on the ASP.NET Core runtime; default HTTP port and `/health` guidance |
| Node.js / Next.js | **MUST** | Detect `package.json` and lockfile; identify package manager, build, start, and declared port; support standalone Next.js output where documented |
| Static site | **MUST** | Detect an output directory with `index.html`; serve through a small static server image |
| Dockerfile | **MUST** | Build with BuildKit; require an explicit port if the image does not declare one |
| Prebuilt OCI image | **MUST** | Pull by digest or tag; never build; require an explicit port and health check |
| Git source | **MUST** | GitHub public/private PAT, GitLab PAT, generic HTTPS Git |
| SSH Git URLs | OUT | Rejected with a clear error |
| Local paths / `file://` | OUT | Rejected |
| Docker Compose | OUT | Import and multi-service orchestration deferred |
| Python, Go, Rust, PHP, Ruby, Java | COULD via Dockerfile | Not detected natively in 0.1 |

**One service per deployment.** A project may have one environment and one service in 0.1. “Multi-service” means a later release, not an implicit Compose import.

---

## 5. Functional requirements

### 5.1 Instance setup and configuration

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-SETUP-01 | First-run setup requires a one-time setup token generated by the installer and shown only on the host console/file with restrictive permissions. | MUST | Setup and identity | A request without the token is rejected; a used token cannot be replayed; no default password exists. |
| FR-SETUP-02 | The first user creates the organization and becomes its owner. | MUST | Setup and identity | Setup completes once; a second setup attempt returns `setup.already_complete`; refreshing does not create a second organization. |
| FR-SETUP-03 | Startup validates required configuration before serving traffic. | MUST | Foundation | Missing database connection, master key, Caddy socket, or invalid URL fails startup with an actionable message and no secret value. |
| FR-SETUP-04 | Host preflight reports Docker, BuildKit, PostgreSQL, Caddy, disk, architecture, and ports 80/443. | MUST | Setup and identity | The dashboard shows pass/fail per check with a fix hint; unsupported hosts can be diagnosed before setup. |
| FR-SETUP-05 | The instance has one organization in 0.1, while every tenant-owned row still carries `organization_id`. | MUST | Setup and identity | Cross-organization access fails closed; a future multi-organization migration does not require dropping tenant columns. |
| FR-SETUP-06 | Setup and login are rate-limited and audited. | MUST | Setup and identity | Repeated failures are throttled; success and failure events appear in audit without passwords or tokens. |
| FR-SETUP-07 | Public base URL and trusted proxy settings are explicit configuration. | MUST | Setup and identity | Cookies, redirects, and absolute links use the configured URL; an invalid URL fails validation. |

### 5.2 Identity, sessions, roles, and tokens

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-AUTH-01 | Local email/password login uses ASP.NET Core Identity with a modern password hash. | MUST | Setup and identity | Valid credentials create a session; invalid credentials return a safe error; passwords are never logged. |
| FR-AUTH-02 | Session cookies are `HttpOnly`, `Secure` in production, and `SameSite=Lax`; logout invalidates the session server-side. | MUST | Setup and identity | Cookie flags are asserted by tests; a logged-out cookie cannot access a protected page or API. |
| FR-AUTH-03 | Roles are owner, admin, developer, and viewer with an explicit permission matrix. | MUST | Setup and identity | Every endpoint is tested for each role; the last owner cannot be removed; a developer cannot perform admin-only actions. |
| FR-AUTH-04 | Personal access tokens can be created, listed, rotated, and revoked. | MUST | Setup and identity | Plaintext is shown once; only a hash is stored; scopes cannot exceed the user's role; revocation takes effect immediately. |
| FR-AUTH-05 | Cookie-authenticated mutations require antiforgery protection; bearer requests are exempt. | MUST | Setup and identity | A cookie mutation without a token fails; a bearer mutation does not require a CSRF token. |
| FR-AUTH-06 | Security headers are set: CSP, HSTS in production, frame denial, `X-Content-Type-Options`, and a referrer policy. | MUST | Setup and identity | Header tests pass; the dashboard works under the CSP without unsafe inline script where avoidable. |
| FR-AUTH-07 | OIDC/SSO is out of scope. | OUT | — | No OIDC endpoints or configuration in 0.1. |
| FR-AUTH-08 | API tokens are prefixed `marsa_pat_` and are recognizable by secret scanners. | SHOULD | Setup and identity | Generated tokens match the prefix; redaction tests recognize the prefix. |

### 5.3 Projects and source connections

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-SRC-01 | A user can create, list, read, update, archive, and restore a project. | MUST | Projects, detection, and builds | Slugs are unique per organization; archived projects are hidden by default; restore returns the project with history intact. |
| FR-SRC-02 | A project connects to a GitHub repository using a public URL or a PAT for private repositories. | MUST | Projects, detection, and builds | Public clone works without a credential; private clone works with a valid PAT; invalid credentials return `source.auth_failed` without storing the credential. |
| FR-SRC-03 | GitLab PAT and generic HTTPS Git sources are supported. | MUST | Projects, detection, and builds | Source test returns the default branch; GitLab token auth works; generic HTTPS requires explicit credentials when private. |
| FR-SRC-04 | Unsupported source schemes are rejected: SSH, `file://`, local paths, and non-HTTPS URLs. | MUST | Projects, detection, and builds | Validation returns a stable code and a helpful message; no outbound request is made to a rejected URL. |
| FR-SRC-05 | Source testing lists the default branch and up to 100 branches. | MUST | Projects, detection, and builds | A successful test returns branch metadata; a failed test does not persist partial credentials. |
| FR-SRC-06 | Repository size, file count, and submodule limits are enforced before a build. | MUST | Projects, detection, and builds | Oversized repositories fail with `source.repo_too_large`; submodules fail with a clear message; limits are configurable. |
| FR-SRC-07 | Push webhooks for GitHub, GitLab, and generic Git verify signatures, deduplicate deliveries, filter branches, and enqueue at most one deployment. | MUST | Projects, detection, and builds | A forged signature is rejected; a duplicate delivery ID is ignored; a push to an unwatched branch does nothing. |
| FR-SRC-08 | Webhook secrets can be rotated and are shown once. | SHOULD | Projects, detection, and builds | The old secret stops working immediately after rotation; audit records the rotation without the secret. |
| FR-SRC-09 | Source credentials are encrypted at rest and never returned by the API. | MUST | Projects, detection, and builds | API projections contain metadata only; database inspection shows ciphertext; logs contain no token. |
| FR-SRC-10 | Repository URLs are checked for SSRF and unsafe redirects. | MUST | Projects, detection, and builds | Private/link-local targets and unsafe redirects are blocked; DNS re-resolution is considered. |

### 5.4 Detection and configuration

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-DET-01 | Detection proposes a build and runtime plan for ASP.NET Core, Node/Next.js, static, Dockerfile, and prebuilt images. | MUST | Projects, detection, and builds | Each supported sample produces the expected plan; the plan names the evidence used. |
| FR-DET-02 | Detection is deterministic for the same commit and detector version. | MUST | Projects, detection, and builds | Repeated detection returns the same plan; results are cached by commit; the detector version is recorded. |
| FR-DET-03 | Detection never executes repository scripts on the host. | MUST | Projects, detection, and builds | A repository containing a malicious script cannot cause host execution during detection; only declared build steps run inside Docker. |
| FR-DET-04 | Detection reports confidence and evidence, not just a guess. | MUST | Projects, detection, and builds | The UI/API shows the detected stack, files, port, and health path; low-confidence results ask for confirmation. |
| FR-DET-05 | A `marsa.json` file can declare or override build, start, port, health, and static output settings. | MUST | Projects, detection, and builds | Schema validation rejects unknown fields, invalid hostnames, unsafe paths, and inline secrets; errors include a JSON path. |
| FR-DET-06 | Precedence is API override > `marsa.json` > detection. | MUST | Projects, detection, and builds | The effective configuration is stored per commit and shown in the UI/CLI; changing it creates a new plan hash. |
| FR-DET-07 | Unsupported repositories receive actionable guidance. | MUST | Projects, detection, and builds | `detection.unsupported` names the missing evidence and suggests a Dockerfile or `marsa.json`. |
| FR-DET-08 | `marsa.json` is versioned and forward-incompatible fields are rejected in v1. | MUST | Projects, detection, and builds | Unknown fields fail validation; a new schema version is required to add fields. |
| FR-DET-09 | Detection supports monorepos only through an explicit project root or Dockerfile. | SHOULD | Projects, detection, and builds | A monorepo without an explicit root fails with guidance; no recursive guessing across packages. |

### 5.5 Build and image creation

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-BUILD-01 | The server detects Docker/BuildKit and reports capability without crashing when Docker is unavailable. | MUST | Foundation | Health shows a degraded Docker component; startup does not crash; the UI explains the missing capability. |
| FR-BUILD-02 | The selected commit is exported to a private build workspace without `.git` or unsafe paths. | MUST | Projects, detection, and builds | `.git` is absent; path traversal and symlink escapes are blocked; archive bombs and size limits are enforced. |
| FR-BUILD-03 | A build produces an immutable image tagged by commit and content hash, with a recorded digest. | MUST | Projects, detection, and builds | Rebuilding the same commit and plan reuses the image unless forced; the digest is stored on the deployment. |
| FR-BUILD-04 | Build logs stream with phases and timestamps, are redacted, and are persisted. | MUST | Projects, detection, and builds | Logs survive a page refresh; secrets are absent; build timeout produces a stable failure. |
| FR-BUILD-05 | Build failures return a stable code and a redacted diagnostic. | MUST | Projects, detection, and builds | Common failures (restore, compile, missing file, timeout) map to documented codes; raw secrets are not shown. |
| FR-BUILD-06 | Build workspaces and old managed images are cleaned by policy. | MUST | Projects, detection, and builds | Failed workspaces are removed; image pruning respects retention and never removes an image referenced by a retained release. |
| FR-BUILD-07 | A prebuilt image source is pulled and pinned by digest when possible. | MUST | Projects, detection, and builds | A tag is resolved to a digest at deploy time; the deployment records the digest; pull failures return a stable code. |
| FR-BUILD-08 | Untrusted Dockerfiles are treated as code execution. | MUST | Projects, detection, and builds | Documentation states the boundary; builds run with the documented isolation; no privileged build flags are exposed to users. |

### 5.6 Runtime and container lifecycle

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-RUN-01 | Marsa creates containers with ownership labels `io.marsa.*` and refuses to mutate unmanaged containers. | MUST | Deployment and observation | Label guards are tested; an unmanaged container with a conflicting name is not stopped or removed. |
| FR-RUN-02 | Workload containers run without privileged mode or host mounts. | MUST | Deployment and observation | Privileged, host-network, and host-path requests are rejected; documentation states that Docker socket access is root-equivalent. |
| FR-RUN-03 | Containers use resource limits, a non-root user where the image permits, a read-only root where possible, and dropped capabilities. | MUST | Deployment and observation | Defaults are applied; unsafe overrides are rejected or require an explicit admin action. |
| FR-RUN-04 | Environment variables come from the deployment snapshot, not live project edits. | MUST | Deployment and observation | Editing a project after admission does not change an in-flight deployment; the snapshot is visible in the deployment detail. |
| FR-RUN-05 | Health checks support HTTP and TCP with configurable path, port, interval, timeout, retries, and start period. | MUST | Deployment and observation | A failing check never activates the release; timeout and crash-loop cases produce distinct codes. |
| FR-RUN-06 | Container start, inspect, stop, remove, and log operations are idempotent. | MUST | Deployment and observation | Repeating a start/stop does not create duplicates or errors; a missing container is handled safely. |
| FR-RUN-07 | Application logs are captured with stream separation and bounded buffering. | MUST | Deployment and observation | `stdout` and `stderr` are distinguishable; a noisy container cannot exhaust memory; logs are persisted. |
| FR-RUN-08 | The previous release is retained after a successful cutover for rollback, subject to retention policy. | MUST | Deployment and observation | The old container/image remains available until retention prunes it; rollback can use it without a rebuild. |

### 5.7 Deployment orchestration

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-DEP-01 | A deployment is persisted as a durable row with an immutable snapshot before any work starts. | MUST | Deployment and observation | A server restart after admission resumes or safely fails the deployment; the snapshot cannot be edited. |
| FR-DEP-02 | At most one deployment is active per environment, enforced by a database constraint and a worker lease. | MUST | Deployment and observation | Concurrent deploy requests return `deployment.active_exists`; a race test cannot create two active deployments. |
| FR-DEP-03 | The deployment state machine is explicit and rejects invalid transitions. | MUST | Deployment and observation | States: `queued`, `preparing`, `building`, `deploying`, `health_checking`, `routing`, `succeeded`, `succeeded_with_warnings`, `action_required`, `failed`, `cancelled`; invalid transitions are rejected and tested. |
| FR-DEP-04 | Each step records start/end time, status, a stable code, and redacted evidence. | MUST | Deployment and observation | The deployment page can render a timeline after a refresh; a failed step names the next action. |
| FR-DEP-05 | A worker claims work with a lease and heartbeat; a dead worker's lease expires and another attempt can resume or fail deterministically. | MUST | Deployment and observation | Kill -9 during build/health is tested; no duplicate activation occurs; attempt limits are enforced. |
| FR-DEP-06 | A queued or running deployment can be cancelled at a safe boundary without taking down the serving release. | MUST | Deployment and observation | Cancel from `queued` through `health_checking` works; the old route remains; cleanup is attempted and recorded. |
| FR-DEP-07 | A release is activated only after health checks and a stabilization window pass. | MUST | Deployment and observation | A failed health check never changes the active route; the old release keeps serving. |
| FR-DEP-08 | Idempotency keys prevent duplicate deployments; the same commit and plan returns the existing result unless forced. | MUST | Deployment and observation | Retrying a request with the same key returns the original deployment; a forced redeploy creates a new one. |
| FR-DEP-09 | Rollback is a new deployment pinned to a previous image digest and snapshot. | MUST | Operations and recovery | Rollback passes the same health gate; history is not mutated; the failed deployment remains visible. |
| FR-DEP-10 | Automatic rollback is out of scope. | OUT | — | A failed release leaves the old release serving and requires an explicit rollback action. |
| FR-DEP-11 | A reconciler detects drift between the database, containers, and routes and repairs only Marsa-managed resources. | MUST | Deployment and observation | A deleted container or stale route is detected; unrelated containers/routes are untouched; the server reports degraded when repair is unsafe. |
| FR-DEP-12 | Deployment retention keeps enough history for audit and rollback without unbounded disk growth. | SHOULD | Operations and recovery | Retention is configurable; pruning never removes the active release or the last known-good release. |

### 5.8 Domains, DNS, and TLS

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-EDGE-01 | Marsa controls Caddy only over a permissioned Unix socket, never a public admin port. | MUST | Routing and security | The admin endpoint is not reachable from the public interface; socket permissions are documented and tested. |
| FR-EDGE-02 | A service can attach one verified custom hostname in 0.1. | MUST | Routing and security | DNS TXT/CNAME instructions are shown exactly; verification is idempotent; an unverified hostname cannot be attached. |
| FR-EDGE-03 | Hostnames are validated and normalized; wildcards, IP literals, and unsafe values are rejected. | MUST | Routing and security | Negative tests cover trailing dots, IDN, uppercase, ports, paths, and wildcard attempts. |
| FR-EDGE-04 | Caddy obtains and renews TLS certificates for verified hostnames. | MUST | Routing and security | Certificate status is visible; a failed issuance does not break other routes; staging CA is used in tests. |
| FR-EDGE-05 | Route changes are serialized, preserve unrelated Caddy configuration, and use stable route IDs. | MUST | Routing and security | Concurrent add/remove tests pass; unknown config is preserved; duplicate host/path conflicts are rejected. |
| FR-EDGE-06 | The route switches only after the new release is healthy; a failed activation leaves the old route serving. | MUST | Routing and security | Failure injection proves no traffic reaches an unhealthy container; rollback re-points after the same gate. |
| FR-EDGE-07 | Route reconciliation on startup and periodically repairs only Marsa-managed routes. | MUST | Routing and security | A Caddy restart or manual drift is reconciled; foreign routes are untouched. |
| FR-EDGE-08 | HTTP redirects to HTTPS for verified hostnames. | SHOULD | Routing and security | Plain HTTP requests redirect with the correct host; ACME challenges are not broken. |
| FR-EDGE-09 | Multiple aliases, wildcard DNS/TLS, and external CDNs are out of scope. | OUT | — | No wildcard certificate or CDN integration in 0.1. |

### 5.9 Logs, health, and observability

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-OBS-01 | Build and runtime logs are captured, redacted, chunked, sequenced, and persisted. | MUST | Deployment and observation | Logs can be read after a sequence; a refresh does not lose them; retention is enforced. |
| FR-OBS-02 | Logs can be filtered by stream and downloaded as redacted text. | MUST | Deployment and observation | Download contains no secret canary; filters are stable in the API. |
| FR-OBS-03 | Realtime deployment updates use SignalR with ordered events and replay after reconnect. | MUST | Deployment and observation | A reconnect after the last sequence receives missed events; a slow client does not block the worker. |
| FR-OBS-04 | `/health/live`, `/health/ready`, and `/health/degraded` report component state. | MUST | Foundation | Readiness fails when PostgreSQL, schema, master key, or Caddy is unavailable; liveness does not depend on optional components. |
| FR-OBS-05 | Every request has a correlation ID; logs and deployment traces include it. | MUST | Foundation | `X-Request-Id` is returned; logs are structured JSON in production. |
| FR-OBS-06 | Deployment metrics include phase duration, queue age, success/failure counts, lease expiry, and route failures. | SHOULD | Operations and recovery | Metrics are exposed in a documented format; telemetry export is off by default. |
| FR-OBS-07 | OpenTelemetry export is opt-in and never required for a healthy install. | SHOULD | Operations and recovery | An install without a collector works; enabling export requires explicit configuration. |
| FR-OBS-08 | Default retention is 30 days for deployment logs, 14 days for application logs, and 1 year for audit. | SHOULD | Operations and recovery | Defaults are configurable; disk-pressure behavior is documented and tested. |

### 5.10 Secrets, environment, and audit

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-SEC-01 | Secrets are envelope-encrypted at rest with a master key outside the database and image. | MUST | Routing and security | Database inspection shows ciphertext; a missing master key fails closed; key versions are recorded. |
| FR-SEC-02 | Secret writes are versioned and immutable; the API returns metadata only. | MUST | Routing and security | A new version does not mutate old deployments; the API never lists plaintext. |
| FR-SEC-03 | Secret reveal is admin-only, feature-flagged, default off, and audited. | SHOULD | Routing and security | When disabled, no endpoint returns plaintext; when enabled, every reveal is audited. |
| FR-SEC-04 | Deployment snapshots carry encrypted secret references; plaintext exists only at container creation. | MUST | Routing and security | Logs, events, audit, and API projections contain no plaintext; a canary test passes. |
| FR-SEC-05 | Environment keys are validated; missing secret references fail at prepare with a clear code. | MUST | Routing and security | Invalid keys are rejected; a missing reference does not start a container. |
| FR-SEC-06 | Secret rotation affects the next deployment only; old deployments keep their snapshot. | MUST | Routing and security | Rotating a secret writes an audit event and does not alter active releases. |
| FR-SEC-07 | Audit events cover login, setup, project/source changes, deploys, rollbacks, secrets, domains, and tokens. | MUST | Routing and security | Audit is append-only, filterable, and contains actor/source/resource but no secret values. |
| FR-SEC-08 | A developer sees their own actions; admins see all; viewers see no sensitive metadata. | MUST | Routing and security | Permission tests pass for each role. |
| FR-SEC-09 | A general-purpose secrets manager, arbitrary host shell, and secret export are out of scope. | OUT | — | No endpoint executes host commands or exports all secrets. |

### 5.11 CLI

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-CLI-01 | `marsa login`, `marsa logout`, and `marsa whoami` work against the API. | MUST | Operations and recovery | Token is stored with `0600`; `--json` works; auth errors use a documented exit code. |
| FR-CLI-02 | `marsa projects list`, `marsa deploy`, `marsa status`, and `marsa logs --follow` work end to end. | MUST | Operations and recovery | Deploy returns non-zero on failure; logs follow and resume; status is machine-readable with `--json`. |
| FR-CLI-03 | `marsa env`, `marsa domains`, and `marsa secrets` manage metadata within role limits. | MUST | Operations and recovery | Secret values are never printed except an explicit write confirmation; destructive actions require `--yes` in non-interactive mode. |
| FR-CLI-04 | The CLI uses a generated client from the OpenAPI contract. | MUST | Operations and recovery | A contract change fails client generation; no hand-written duplicate models. |
| FR-CLI-05 | The CLI never accepts a token as a positional argument. | MUST | Operations and recovery | Token values in argv are rejected; configuration and environment input are supported. |
| FR-CLI-06 | Shell completion is a COULD. | COULD | Post-MVP | Not required for 0.1. |

### 5.12 Dashboard, Arabic, and accessibility

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-UI-01 | A new instance redirects to setup; after setup, login/logout/current-user state works. | MUST | Setup and identity | No dead end; unauthorized pages return a safe error. |
| FR-UI-02 | A user can create a project, connect a source, review detection, deploy, watch phases/logs, and roll back. | MUST | Operations and recovery | The primary flow works without the CLI; a failed deploy shows the failing step and next action. |
| FR-UI-03 | A user can add a domain, copy DNS instructions, verify, and see certificate status. | MUST | Routing and security | DNS values are copyable; verification state is clear; errors explain the next step. |
| FR-UI-04 | A user can manage members, tokens, secrets metadata, and audit within role limits. | MUST | Routing and security | No secret plaintext is rendered; the last owner cannot be removed. |
| FR-UI-05 | The dashboard is server-rendered Razor Components; no SPA or Node build pipeline is required. | MUST | Setup and identity | A clean checkout builds and runs without a JavaScript bundler; interactive islands degrade gracefully. |
| FR-UI-06 | Arabic and English are selectable per user; the default follows the browser with an explicit override. | MUST | Operations and recovery | The choice persists; switching language does not lose session or form state. |
| FR-UI-07 | Arabic is written as Arabic, with correct RTL layout, numerals, code blocks, and mixed-direction content. | MUST | Operations and recovery | RTL tests pass at 360 px and 1280 px; code/log blocks remain LTR; no clipped controls. |
| FR-UI-08 | Primary flows are keyboard-usable with labels, focus states, and an error summary. | SHOULD | Operations and recovery | Automated and manual accessibility checks pass for setup, login, project, deploy, domain, and secrets pages. |
| FR-UI-09 | Destructive actions require confirmation and explain impact. | MUST | Projects, detection, and builds | Delete/archive/rollback/cancel dialogs state what changes and what is retained. |
| FR-UI-10 | A public marketing site, desktop app, and mobile app are out of scope. | OUT | — | No separate frontend product in 0.1. |

### 5.13 Operations and recovery

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-OPS-01 | A documented clean-host install starts PostgreSQL, Caddy, and the Marsa server with one procedure. | MUST | Release candidate | A fresh Linux VM installs from the release artifact; the setup page is reachable over HTTPS or localhost. |
| FR-OPS-02 | Upgrade applies migrations without breaking the running version during rollout. | MUST | Release candidate | Expand/contract migration; upgrade from the previous release passes with data preserved; downgrade requires a documented restore. |
| FR-OPS-03 | A backup/restore drill restores database, deployment state, and secrets with the master key. | MUST | Release candidate | The restored instance passes readiness; active release history is visible; no secret canary leaks. |
| FR-OPS-04 | A diagnostic command or page reports host, database, Docker, Caddy, disk, and version health. | MUST | Operations and recovery | The report is redacted; a failed component has a next action. |
| FR-OPS-05 | Install, upgrade, backup, restore, rotate master key, troubleshoot, and uninstall runbooks exist and are tested. | MUST | Release candidate | A person who did not write the feature can follow the runbook; commands are copy-paste safe. |
| FR-OPS-06 | Disk-pressure behavior is defined: warning, degraded state, and safe cleanup. | SHOULD | Operations and recovery | A full disk does not corrupt state; the UI warns before builds fail; cleanup removes only managed artifacts. |
| FR-OPS-07 | The release includes an SBOM, checksums, and a license inventory. | MUST | Release candidate | No GPL/unknown component in the default distribution; CI gates the inventory. |
| FR-OPS-08 | A product UI for scheduled backups and restore is out of scope; the script and drill are in scope. | OUT | — | No backup scheduler in 0.1. |

### 5.14 API contract

| ID | Requirement | Priority | Milestone | Acceptance criteria |
|---|---|---|---|---|
| FR-API-01 | The public API is versioned under `/api/v1`. | MUST | Setup and identity | Breaking changes require `/api/v2`; additive changes are documented. |
| FR-API-02 | OpenAPI is generated from the implementation, not hand-maintained. | MUST | Setup and identity | The generated document is checked in or published; the CLI client is generated from it. |
| FR-API-03 | Errors use a consistent ProblemDetails shape with a stable machine-readable code. | MUST | Foundation | Every error has `type`, `title`, `status`, `code`, `traceId`, and a safe message; no stack trace or secret. |
| FR-API-04 | List endpoints paginate and have bounded page sizes. | MUST | Projects, detection, and builds | Default and maximum page sizes are enforced; cursors or stable ordering are documented. |
| FR-API-05 | Mutating endpoints accept an idempotency key where retries are expected. | MUST | Deployment and observation | Duplicate keys return the original result; conflicting payloads return a documented error. |
| FR-API-06 | Authentication supports session cookies for the dashboard and bearer PATs for CLI/API. | MUST | Setup and identity | Both paths enforce the same authorization; PAT scopes are checked. |
| FR-API-07 | Rate limits apply to login, setup, deploy, webhooks, and log streaming. | MUST | Operations and recovery | Limits return a documented error and `Retry-After`; normal use is not throttled. |
| FR-API-08 | The dashboard does not use a private business-logic layer. | MUST | Setup and identity | Dashboard server-rendered pages call application services/API equivalents; no direct database access. |
| FR-API-09 | A public SDK is out of scope. | OUT | — | No separate SDK in 0.1. |

---

## 6. Non-functional requirements

### 6.1 Security

| ID | Requirement |
|---|---|
| NFR-SEC-01 | No default credentials; setup requires a one-time token. |
| NFR-SEC-02 | Every tenant-owned query is organization-scoped; cross-tenant probes fail closed without existence leaks. |
| NFR-SEC-03 | Docker socket access is treated as root-equivalent and documented; workloads never receive the socket. |
| NFR-SEC-04 | Secrets are encrypted at rest, redacted in logs/events/audit/telemetry, and never returned by list APIs. |
| NFR-SEC-05 | Webhooks use HMAC verification with constant-time comparison, timestamp tolerance, delivery dedupe, and branch filters. |
| NFR-SEC-06 | Repository URLs, health URLs, hostnames, env keys, paths, and image references are validated against explicit allowlists. |
| NFR-SEC-07 | Build/source export blocks path traversal, symlink escape, archive bombs, and oversized repositories. |
| NFR-SEC-08 | Cookie mutations require antiforgery; cookie flags are correct in production. |
| NFR-SEC-09 | Dependency and container scans have no unapproved High/Critical findings at release. |
| NFR-SEC-10 | The default distribution contains no GPL or unknown-license component. |
| NFR-SEC-11 | Audit is append-only and covers every sensitive action. |
| NFR-SEC-12 | Rate limits and bounded resources protect login, setup, builds, logs, and webhooks from trivial denial of service. |

### 6.2 Reliability and recovery

| ID | Requirement |
|---|---|
| NFR-REL-01 | At most one active deployment per environment under concurrency. |
| NFR-REL-02 | A worker crash recovers within 2 × lease duration; no duplicate activation. |
| NFR-REL-03 | A failed deployment never changes the active route. |
| NFR-REL-04 | A Caddy restart is reconciled without manual database edits. |
| NFR-REL-05 | A server restart preserves queued work and deployment logs. |
| NFR-REL-06 | Shutdown is graceful; in-flight steps finish within a timeout or are recoverable. |
| NFR-REL-07 | All external calls have timeouts and bounded retries; retries are idempotent. |
| NFR-REL-08 | Backups can restore the instance and secrets with the master key; the drill is tested before release. |
| NFR-REL-09 | The active release and the last known-good release are never pruned by routine cleanup. |

### 6.3 Performance (design targets, not SLAs)

| Operation | Target |
|---|---|
| API read p95 | < 200 ms excluding cold start |
| API write p95 | < 500 ms excluding deployment work |
| Deployment admission | < 1 s |
| Small app deploy with cached base image | < 3 minutes from admission to active |
| Health check detection | Within configured interval + timeout |
| Log event delivery | < 1 s from write to subscribed client on a healthy connection |
| Dashboard initial render | < 2 s on a local network |

### 6.4 Accessibility and localization

| ID | Requirement |
|---|---|
| NFR-UX-01 | Primary flows are keyboard-operable with visible focus and labeled controls. |
| NFR-UX-02 | Arabic RTL is correct at 360 px and 1280 px; code and logs remain LTR. |
| NFR-UX-03 | Error pages state what failed, what was preserved, and what to do next. |
| NFR-UX-04 | Dates, numbers, and durations are localized without breaking machine-readable API values. |
| NFR-UX-05 | The UI does not rely on color alone to communicate deployment state. |

### 6.5 Compatibility

| ID | Requirement |
|---|---|
| NFR-COMP-01 | Supported control-plane hosts are Linux x86_64 and arm64. |
| NFR-COMP-02 | Supported browsers are the current and previous major versions of Chrome, Firefox, Safari, and Edge. |
| NFR-COMP-03 | PostgreSQL 17 is the supported database; other versions are not claimed. |
| NFR-COMP-04 | Docker Engine 27+ with BuildKit is the supported runtime. |

---

## 7. Data and state requirements

These are product-level invariants; the schema is defined in `05`.

1. **Immutable deployment snapshot.** A deployment executes a frozen plan, source commit, config hash, secret references, and image reference. Editing a project does not change an in-flight or historical deployment.
2. **One active deployment per environment.** Enforced by a partial unique index and a worker lease.
3. **Immutable releases.** An image is referenced by digest; a successful release is never mutated in place.
4. **Rollback is a new deployment.** It references a prior image digest/snapshot and passes the same health gate.
5. **Retained history.** Deployment steps, logs, audit, and release metadata survive restarts and are retained per policy.
6. **Organization scoping.** Every tenant-owned row carries `organization_id`; repository queries apply the tenant filter.
7. **Append-only audit.** Audit rows are not updated or deleted by the application; redaction happens before write.
8. **Secret separation.** Secret plaintext is never stored in deployment logs, audit, or events; snapshots carry encrypted references and key versions.
9. **One organization per installation in 0.1.** The schema remains compatible with future multi-organization support.
10. **Time is UTC.** All persisted timestamps are UTC; lease checks prefer database time.

---

## 8. Constraints and assumptions

### 8.1 Constraints

- Single Linux Docker host; no agent, SSH target, Kubernetes, or cluster in 0.1.
- One service per deployment; no Compose import or multi-service orchestration.
- PostgreSQL is the only required stateful dependency; no Redis, broker, or object store.
- Caddy is the only edge in 0.1; no external reverse proxy is supported in front of it.
- The control plane has Docker socket access; this is a documented root-equivalent trust boundary.
- The dashboard is server-rendered; no SPA, Node build pipeline, desktop app, or mobile app.
- Local accounts only; OIDC/SSO and SCIM are post-MVP.
- Apache-2.0 is the Proposed license; no GPL component in the default distribution.
- The public domain and trademark clearance are unresolved release gates.

### 8.2 Assumptions

| ID | Assumption | If false |
|---|---|---|
| A-01 | The operator controls a Linux host with Docker and outbound HTTPS. | Install/runtime design changes; offline mode is unsupported. |
| A-02 | The operator controls DNS for the custom hostname. | Domain verification needs a provider integration or manual delegation. |
| A-03 | Caddy can bind 80/443 and obtain ACME certificates. | TLS mode or edge choice changes. |
| A-04 | PostgreSQL 17 is available as a container or external service. | Packaging and migration support change. |
| A-05 | The server can access the Docker socket. | A socket proxy/agent becomes an MVP requirement. |
| A-06 | One organization per install is enough. | Membership, billing, and UI scope expand. |
| A-07 | Users have a browser and a terminal. | Prioritize one client. |
| A-08 | Supported frameworks can be detected without executing repository code. | Detection becomes sandboxed execution with a larger security scope. |
| A-09 | A single server process handles the MVP's deployment concurrency. | Worker split or external queue becomes necessary earlier. |
| A-10 | Builds can run on the same host as the runtime. | Remote builders or a build service are needed. |
| A-11 | A failed release can be diagnosed without shell access. | Interactive debug/exec enters the threat model. |
| A-12 | Apache-2.0 is acceptable. | License and contribution model change. |

---

## 9. MVP acceptance criteria

Marsa 0.1 is not releasable unless all of these pass on a clean Linux host.

### 9.1 User outcomes

1. A developer can set up a fresh instance and deploy a supported ASP.NET Core repository to HTTPS in under 10 minutes, following the docs without SSH.
2. A failed build or failed health check never replaces the working release.
3. A rollback restores the previous release without a rebuild when the artifact exists.
4. A user can explain a failed deployment from the deployment page: phase, failing step, stable code, and next action.
5. A secret can be set, used, rotated, and never appears in logs, API projections, audit, or telemetry.
6. An Arabic-speaking beginner can complete the core flow in Arabic with correct RTL at 360 px and 1280 px.
7. One operator can install, upgrade, back up, restore, and diagnose the instance using the runbooks.

### 9.2 Security and reliability gates

- Cross-tenant isolation tests pass for every endpoint.
- No default credentials; setup cannot be replayed.
- No secret canary appears in logs, events, audit, or telemetry.
- At most one active deployment per environment under concurrency.
- A killed worker recovers without duplicate activation.
- A failed health check never changes the active route.
- Docker operations refuse unmanaged containers; workloads cannot access the Docker socket.
- Caddy admin is reachable only over the Unix socket.
- No unapproved High/Critical dependency or container finding.
- SBOM and license inventory have no unresolved default-distribution issue.

### 9.3 Operational gates

- Clean-host install and upgrade from the previous release pass.
- Backup/restore drill passes, including secrets and the master key.
- E2E suite passes on a disposable Linux runner.
- Runbooks cover Docker down, Caddy down, database down, disk full, and worker crash.
- The release artifact includes checksums, SBOM, and upgrade notes.

### 9.4 Scope gate

The final MVP still matches: one host, one service per deployment, Docker, PostgreSQL, Caddy, local auth, dashboard + CLI + API. If a feature requires violating this, it is post-MVP.

---

## 10. Out of scope for 0.1

The following are **OUT** for Marsa 0.1. They may appear in the post-MVP roadmap; listing them here is not a promise to build them.

- multi-host scheduling, SSH targets, Kubernetes, Nomad, or clustering;
- Docker Compose import, multi-service projects, or shared networking between services;
- one-click application catalog or plugin marketplace;
- managed database provisioning, failover, scaling, or upgrades;
- scheduled application-data backups or a restore UI (operator script and drill are in scope);
- public SaaS, billing, usage metering, or a mandatory cloud account;
- Windows/macOS hosts and Windows containers;
- desktop app, mobile app, or SPA;
- built-in mail hosting or mail DNS;
- MCP server, AI agent that can mutate production, or chat integrations;
- PR preview environments, approvals, and advanced team workflows;
- OIDC/SSO, SCIM, audit export, or compliance reporting;
- a general-purpose secrets manager beyond deployment needs;
- wildcard DNS/TLS, multiple hostnames, or external CDN support;
- automatic rollback;
- an in-browser terminal or arbitrary host shell;
- a public SDK, public container registry, or general-purpose CI system;
- Windows or non-Docker build agents.

---

## 11. Metrics

These are design targets measured in tests, usability sessions, and opt-in feedback; they are not contractual SLAs and do not depend on silent telemetry.

| Metric | Target | Evidence |
|---|---|---|
| Setup completion | ≥ 80% of started installs in usability tests | Facilitated funnel |
| First-deploy success on supported stacks | ≥ 70% of first attempts | E2E + usability |
| Deploy success excluding user build errors | ≥ 95% | Deployment outcomes |
| Median sample-app deploy | < 3 minutes with cached base image | Phase durations |
| Rollback success | 100% in E2E; no route regression | E2E + audit |
| Time to diagnose a failed deploy | < 5 minutes | Usability test |
| Secret leakage | 0 canary findings | Redaction suite |
| Unmanaged container operations | 0 | Adapter guard tests |
| Active deployments per environment | ≤ 1 at all times | Database invariant + reconciliation |
| Arabic flow completion without switching to English | ≥ 80% in usability tests | Facilitated session |
| Accessibility | Primary flows keyboard-usable; labels and error summaries present | Manual + automated |

---

## 12. Open questions

| ID | Question | Recommendation | Needed by | Impact if unresolved |
|---|---|---|---|---|
| Q-01 | Is the MVP strictly single-host Docker? | Yes. Defer SSH/multi-host. | Foundation | Scope explosion; adapter interfaces become speculative. |
| Q-02 | Is Caddy the only edge? | Yes. Keep an `IEdgeRouter` port. | Deployment and observation | Route/TLS tests churn. |
| Q-03 | Is the dashboard Razor Components only? | Yes. No SPA/Node build. | Setup and identity | Build/auth complexity. |
| Q-04 | Is one organization per install enough? | Yes for 0.1; keep `organization_id`. | Setup and identity | Tenant/UI schema churn. |
| Q-05 | Is PostgreSQL the only required stateful dependency? | Yes. No Redis/broker/object store. | Foundation | Queue/lease design changes. |
| Q-06 | Which stacks are supported at launch? | ASP.NET Core, Node/Next.js, static, Dockerfile, prebuilt image. | Projects, detection, and builds | Detection/support burden. |
| Q-07 | Is one service per deployment enough? | Yes. Multi-service is post-MVP. | Deployment and observation | Snapshot/route model complexity. |
| Q-08 | Does a failed deploy auto-rollback? | No. Explicit rollback only. | Routing and security | Hidden automation and surprising state. |
| Q-09 | Is rollback a new deployment? | Yes. Pinned digest; same health gate. | Deployment and observation | Audit/state ambiguity. |
| Q-10 | Is Docker socket access acceptable? | Yes for 0.1 with guardrails and documentation; evaluate a proxy/agent before a security-sensitive release. | Foundation | Runtime/threat model changes. |
| Q-11 | Is a GitHub webhook secret enough? | Yes for 0.1; GitHub App post-MVP. | Projects, detection, and builds | Provider auth and repo-picker scope. |
| Q-12 | One hostname per service? | Yes for 0.1; multiple aliases/wildcards post-MVP. | Routing and security | Route and certificate complexity. |
| Q-13 | Is secret reveal allowed? | Admin-only, feature-flagged, default off, audited. | Routing and security | Secret threat model. |
| Q-14 | Is a separate worker process required? | No in 0.1; durable hosted worker; split post-MVP. | Deployment and observation | Deployment topology/packaging. |
| Q-15 | What is the minimum host? | Linux x86_64/arm64, Docker 27+, 2 vCPU, 4 GB RAM, 20 GB disk. | Release candidate | Install docs and resource limits. |
| Q-16 | Is rootless Docker supported? | Best-effort; not a launch requirement. | Release candidate | Socket assumptions. |
| Q-17 | What is the default retention? | 30d deployment logs, 14d app logs, 1y audit; configurable. | Operations and recovery | Disk usage and expectations. |
| Q-18 | Is a backup UI in scope? | No. Script + drill only. | Release candidate | Data-loss expectations. |
| Q-19 | Are unknown `marsa.json` fields rejected? | Yes in v1; new schema version to add fields. | Projects, detection, and builds | Silent typos vs forward compatibility. |
| Q-20 | Is the API stable at v1? | Documented endpoints are additive-only pre-1.0. | Release candidate | Client compatibility. |
| Q-21 | What is the final license? | Apache-2.0 proposed; legal review before release. | Foundation, Release candidate | Distribution/contribution model. |
| Q-22 | What is the public domain and trademark status? | Resolve before public launch; no domain is owned. | Release candidate | Branding and release. |

---

## 13. Traceability

| Requirement area | Roadmap epics | Architecture baseline |
|---|---|---|
| Setup, auth, tenancy | epic, epic, epic | `03-architecture.md` §10 |
| Projects and source | epic | §6–7 |
| Detection and `marsa.json` | epic | §7 |
| Build and runtime | epic | §8–9 |
| Deployment orchestration | epic | §8.2–8.3 |
| Domains and TLS | epic | §9 |
| Logs and observability | epic | §11–12 |
| Secrets and audit | epic | §10 |
| CLI and dashboard | epic | §13 |
| Ops and release | epic | §16–19 |
| Arabic/RTL and accessibility | epic, epic | UI requirements in this document |

The epics and milestones are defined in [`../roadmap.md`](roadmap.md). This PRD does not invent calendar dates; sequencing is milestone-based.

---

## 14. Review and approval

| Question | Answer |
|---|---|
| Is the MVP boundary small enough to finish? | **PROPOSED yes**, if the single-service and single-host limits hold. |
| Is every requirement testable? | Every MUST has acceptance criteria; approved adds API/schema detail and approved turns them into issues. |
| What would invalidate this PRD? | A validated user need that requires multi-service, multi-host, Compose, or a different trust model before first release. |
| Who confirms? | The project owner. approved approved this baseline on 2026-10-22; changes to MUST scope require a recorded decision. |
