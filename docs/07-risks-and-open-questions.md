# Marsa — Risks, Decisions, and Open Questions

> Status: **LIVING DOCUMENT**. Last updated: 2026-10-22.
> Security controls are in [`07-security.md`](07-security.md). Decisions here are **PROPOSED** until the owner approves them; entries marked **DECIDED by owner** are already approved. Unapproved decisions are still binding for the current baseline so that later phases do not drift.

## 1. How to use this document

- A **decision** is a choice already reflected in the design. It records the alternative that was rejected and why.
- An **open question** is unresolved. The current documents use the recommended default; changing it requires updating the affected docs and this log.
- A **risk** is an accepted uncertainty with a mitigation and a trigger for revisiting it.
- Do not silently change a decision. Add a new entry and link the affected document.

## 2. Decision log for Phases 3–8

| ID | Decision | Status | Alternatives rejected | Why |
|---|---|---|---|---|
| D-011 | Use the Phase 2 canonical deployment states: `queued`, `preparing`, `building`, `deploying`, `health_checking`, `routing`, `succeeded`, `succeeded_with_warnings`, `action_required`, `failed`, `cancelled`. | **PROPOSED** | Prior analysis `Starting`/`Activating`; larger state set with separate `NoChanges` state. | Phase 2 already approved these names; changing them would invalidate requirement IDs and UI copy. `NoChanges` is an admission result, not a state. |
| D-012 | Caddy receives the complete Marsa-managed route set, including the static control-plane route, applied transactionally over a Unix socket. | **PROPOSED** | Patch only the changed vhost; keep a hand-edited Caddyfile; expose the admin API on localhost TCP. | A full managed set prevents drift and lockout; per-route patching is harder to reconcile. TCP admin is reachable from tenant networks. |
| D-013 | Persist runtime application logs in `service_log_chunks` for 14 days alongside 30-day deployment logs. | **PROPOSED** | Rely on `docker logs` only; store logs only in the deployment table; ship to an external log system. | `docker logs` cannot enforce retention, search, or post-hoc redaction. An external log system violates the minimal-dependency promise. |
| D-014 | Use a generic `idempotency_records` table for retryable mutations. | **PROPOSED** | Per-resource idempotency columns only; no idempotency outside deployments. | Deployments, rollbacks, domains, and secrets all need replay behavior; one table avoids duplicated logic. |
| D-015 | Default to Caddy ACME HTTP-01 for custom hostnames; DNS-01 and wildcards are post-MVP. | **PROPOSED** | DNS-01 in 0.1; wildcard certificates; external certificate upload. | HTTP-01 needs only an A/AAAA record and works with the single-host model. DNS-01 requires provider integrations and broader secrets. |
| D-016 | One verified custom hostname per service in 0.1; no aliases or wildcards. | **PROPOSED** | Multiple aliases; wildcard host; path-based multi-tenant routing. | Keeps route uniqueness, DNS instructions, and certificate tests small. Aliases are a likely 0.2 feature. |
| D-017 | Caddy admin socket is `/run/marsa/caddy/admin.sock`, directory `0700`, socket `0660`, shared only by server and edge. | **PROPOSED** | `localhost:2019`; Docker network TCP; mounting the Docker socket into Caddy. | The edge is attached to tenant networks, so a TCP admin port would be reachable from workloads. |
| D-018 | Enforce concurrency with PostgreSQL row locks plus partial unique indexes; use `xmin` for optimistic concurrency. | **PROPOSED** | Advisory locks only; Redis locks; in-process semaphores. | Database constraints survive crashes and multiple workers. Advisory locks are supplementary, not the invariant. |
| D-019 | Encrypt secrets with AES-256-GCM using a master key file outside the database and image. | **PROPOSED** | ASP.NET Data Protection keyring in the database; KMS/HSM; plaintext with filesystem permissions. | Offline operation, no cloud dependency, and a clear backup boundary. KMS is a post-MVP option for regulated deployments. |
| D-020 | Default health gate: 60-second total timeout, 5-second interval, 3-second request timeout, 12 retries, 10-second start period, 3 consecutive successes. | **PROPOSED** | Docker health status only; fixed 30 seconds; immediate first success. | The values fit small apps on a cold host. Docker health status alone hides the probe contract and is harder to test. |
| D-021 | Produce local images; do not push application images to an external registry in 0.1. | **PROPOSED** | Push every image to a configured registry; use only prebuilt images. | Local images keep the dependency footprint small and avoid registry credentials. Rollback requires retaining local image digests. |
| D-022 | Retain the active image, the previous image, and the last known-good image; cap managed image retention at five releases per service. | **PROPOSED** | Keep all images; keep only the active image; retain for a fixed 30 days. | Rollback must work without a rebuild. A cap prevents unbounded disk growth. |
| D-023 | Deployment worker maximum attempts: 3, with a 60-second lease and 15-second heartbeat. | **PROPOSED** | Unlimited retries; one attempt; 5-minute lease. | Three attempts cover transient Docker/Caddy failures without hiding a deterministic bug. A short lease recovers quickly. |
| D-024 | API error `type` values use `urn:marsa:error:<code>` until a public documentation domain exists. | **PROPOSED** | `https://marsa.dev/errors/...`; no `type` field. | The domains are not owned and must not be presented as public endpoints. URNs are stable and honest. |
| D-025 | Dashboard and API share an origin; no production CORS policy is enabled in 0.1. | **PROPOSED** | Separate SPA origin; permissive CORS; wildcard origins. | One origin removes CORS and CSRF complexity. A separate frontend is explicitly out of scope. |
| D-026 | PATs are hashed with SHA-256, not a password hash. | **PROPOSED** | Argon2id/bcrypt for PATs; encrypted PATs. | Tokens have 256 bits of entropy, so a fast one-way hash is appropriate and lookup is simpler. User passwords still use Identity's password hasher. |
| D-027 | Production migrations run through an explicit migration bundle/command, never automatically on web startup. | **PROPOSED** | Auto-migrate at startup; manual SQL only. | Auto-migration races with rollouts and can break the previous binary. A bundle is reviewable and repeatable. |
| D-028 | CLI token storage defaults to `~/.config/marsa/config.json` with mode `0600`; OS keyring is optional. | **PROPOSED** | Project-local `.env`; keyring required; plaintext in home directory without permissions. | One predictable location works on Linux/macOS and can be secured. A required keyring adds platform dependencies. |
| D-029 | Use SemVer with `0.1.0` as the first public release; pre-1.0 breaking changes require a migration note. | **PROPOSED** | Calendar versioning; `1.0.0` at first release. | SemVer matches .NET/NuGet expectations. `1.0.0` would overstate stability. |
| D-030 | License first-party code under Apache-2.0 and documentation under Apache-2.0 as well. | **PROPOSED** | CC BY 4.0 for docs; MIT for code; dual license. | One license is simpler for contributors and redistributors. CC BY is friendlier for prose but adds a second policy to maintain. Revisit if docs are published separately. |
| D-031 | Use the official Caddy 2.x image pinned by digest; do not build a custom Caddy in 0.1. | **PROPOSED** | Custom Caddy with plugins; another reverse proxy. | Official image reduces supply-chain surface. Plugins can be added later behind the edge adapter. |
| D-032 | No auto-rollback in 0.1; a failed release leaves the old release serving and requires an explicit rollback. | **PROPOSED** | Auto-rollback on health failure; auto-rollback on route failure. | Explicit rollback avoids surprising data/schema effects. The old release is preserved, so recovery is one action. |
| D-033 | Supported stack detection is limited to ASP.NET Core, Node/Next.js, static, Dockerfile, and prebuilt image. | **PROPOSED** | Broader language detection; execute build scripts to detect. | Detection must be deterministic and must not execute repository code. Unknown stacks get actionable guidance. |
| D-034 | The server never deploys its own control-plane project or writes routes for its own control hostname through the dynamic path. | **PROPOSED** | Allow self-deployment; treat the control plane as a normal project. | Self-deployment can take down the operator's only recovery path. The control route stays static. |
| D-035 | The backlog follows the PRD requirement milestones. Four roadmap rows were corrected without deleting content: E2.1–E2.4 → M2, E0.9 → M6, E5.7 → M5, E9.9 → M5. | **PROPOSED** | Keep the roadmap milestones and split conflicting issues; move the PRD requirements instead. | The PRD is the requirement source of truth; the roadmap rows were stale. The correction is recorded in `docs/backlog/README.md` §7. |
| D-036 | Use 39 vertically sliced issues for the MVP; allow an `xl` issue when it is a cohesive, testable slice, and split it before parallel work. | **PROPOSED** | One issue per roadmap story (104); one issue per requirement; split every XL by layer. | 39 issues keep dependencies and review overhead manageable while every MUST, NFR, and roadmap story remains mapped. Layer splits would create untestable handoffs. |
| D-037 | Use a short mandatory `AGENTS.md` contract plus on-demand `docs/ai/` guides and five project skills. | **PROPOSED** | One oversized instruction file; no project skills; docs-only guidance without permissions. | Keeps the always-loaded context small while preserving depth and owner control over GitHub writes. |
| D-038 | Treat the Arabic explainer as a companion narrative, with the Arabic glossary as canonical terminology. | **PROPOSED** | A full Arabic translation of every specification; English-only onboarding; a second normative contract in Arabic. | The explainer teaches the mental model without creating a second source of truth for changing contracts. |
| D-039 | The 0.1 gate is all 132 mandatory requirements via 39 issues; a smaller external release is a private preview, not 0.1. | **PROPOSED** | Require all 151 rows including SHOULD/COULD; trim individual MUST acceptance criteria under schedule pressure; call an M0–M5 slice 0.1. | 151 rows are an inventory, not 151 independent features. The cut line preserves the approved PRD while giving a coherent, honestly labeled fallback. |
| D-040 | Keep the owner’s progress, Arabic learning files, prompts, handoff, notes, costs, and scratch files in a private `.local/` workspace excluded through `.git/info/exclude`. | **DECIDED by owner** | Store them as public docs; track them but rely on review; add them to `.gitignore`. | The public repository keeps only product documentation; local exclusion avoids publishing even the filenames. Public docs must never link into `.local/`. |
| D-041 | Require a beginner Arabic explanation for every code change before the change is considered done. | **DECIDED by owner** | Write explanations only when asked; rely on English docs; translate the entire specification. | The owner must be able to understand and maintain the system. The file lives at `.local/learn/NNN-<issue-or-topic>.ar.md`, is never committed, and is enforced by `AGENTS.md`, `docs/ai/definition-of-done.md`, the PR template, and the `explain-as-you-build` skill. |
| D-042 | Keep the current public history and accept the low-risk exposure of old private intermediate paths in it, instead of rewriting or recreating the repository. | **DECIDED by owner** | Rewrite history with git-filter-repo or delete and recreate the GitHub repository. | Security, license, secret, and privacy scans found no tokens, keys, emails, or Openship source; the exposed items are low-risk project paths and local username/path references. Removing them would require high-risk or manual repository mutation. |

## 3. Open questions

| ID | Question | Current default | Decision needed by | If changed |
|---|---|---|---|---|
| Q-01 | Must a Docker socket proxy or host agent ship before the first public release? | Ship with a documented direct-socket risk; evaluate a proxy before public launch. | M6 | Runtime adapter, packaging, and threat model change. |
| Q-02 | Is runtime application-log browsing required in 0.1, or are deployment logs enough? | Include a read-only runtime log view and 14-day retention. | M3 | Remove `service_log_chunks`, endpoints, and retention; lower storage. |
| Q-03 | What is the minimum supported host? | Linux x86_64/arm64, Docker 27+, 2 vCPU, 4 GB RAM, 20 GB free disk. | M0 | Install checks, resource defaults, and docs change. |
| Q-04 | Is rootless Docker supported at launch? | Best-effort, not a launch gate. | M6 | Socket path and permission assumptions; test matrix. |
| Q-05 | Should application images ever be pushed to a registry in 0.1? | No; local image store only. | M2 | Registry credentials, retention, and rollback behavior change. |
| Q-06 | How many previous releases should be retained by default? | Active + previous + last known-good, capped at five per service. | M3 | Disk usage and rollback window change. |
| Q-07 | What is the default build concurrency? | Four per host. | M0 | Worker leases, disk, and CPU pressure. |
| Q-08 | Should `marsa.json` allow more than one service in 0.1? | No; one service per file, matching one service per deployment. | M2 | Snapshot, route, and issue scope expand. |
| Q-09 | Should the dashboard expose the environment concept when only one exists? | Hide it until a second environment is created. | M1 | Navigation and URL structure change. |
| Q-10 | What is the initial docs license? | Apache-2.0 for code and docs. | M6 | Add a separate docs license and notices. |
| Q-11 | Is a public documentation domain secured before launch? | No; use relative links and `urn:marsa:*` identifiers. | M6 | Error `type` URLs, schema IDs, and docs links can become HTTPS. |
| Q-12 | Should the CLI support OS keyring storage in 0.1? | Optional; file with `0600` is the baseline. | M5 | Add platform-specific dependencies and tests. |
| Q-13 | Should there be a separate worker process before 0.1? | No; one hosted worker, split post-MVP. | M3 | Packaging, leases, and deployment topology change. |
| Q-14 | Should Caddy route changes use a full config replace or a per-route transaction inside the managed set? | Full managed-set replace, serialized. | M4 | Reconciliation and rollback tests change. |
| Q-15 | Do we need a product UI for backups in 0.1? | No; script + restore drill. | M6 | Adds scheduler, storage target, and restore UX. |

## 4. Risk register

| ID | Risk | Likelihood | Impact | Mitigation | Trigger |
|---|---|---|---|---|---|
| R-01 | Docker socket compromise gives host root | High | Critical | Dedicated host, no untrusted users, label guard, no privileged/host mounts, socket proxy review | Any user-controlled Docker parameter or host mount |
| R-02 | Secret plaintext leaks via logs/errors/telemetry | Medium | Critical | Encryption, write-only API, central redaction, canary tests, diagnostics off | Secret-looking value in any output |
| R-03 | Master key loss makes secrets unrecoverable | Medium | High | Separate key backup, fail-closed startup, rotation runbook | Restore without key |
| R-04 | Cross-tenant data access | Medium | Critical | Organization filters, resource authorization, matrix tests | Query without tenant predicate |
| R-05 | Forged or replayed webhook deploys | Medium | High | HMAC, constant-time compare, replay window, delivery dedupe, branch filter | Duplicate delivery or signature failure |
| R-06 | SSRF through repo URLs or redirects | Medium | High | HTTPS allowlist, DNS re-check, block private/link-local/metadata addresses | User URL resolving internally |
| R-07 | Path traversal or symlink escape | Medium | High | Private workspace, canonicalization, no escaping symlinks, archive limits | Unexpected path in build context |
| R-08 | Malicious image/build compromises host | Medium | Critical | No socket, no host mounts, limits, no privileged mode, registry allowlist | Privileged or host-mount request |
| R-09 | Two deployments race and both activate | Medium | Critical | Environment lock, partial unique index, lease, concurrency test | Missing `deployment.active_exists` |
| R-10 | Failed release receives traffic | Medium | High | Health gate before route, old route retained, failure injection | Route points to unhealthy container |
| R-11 | Worker crash leaves deployment stuck | Medium | High | Lease, heartbeat, attempt limit, reclaim, reconciler | Expired lease with no progress |
| R-12 | Caddy partial apply/corruption | Medium | High | Serialized full managed-set apply, validation, reconcile, static control route | Config hash drift |
| R-13 | Rollback uses mutable tag or wrong image | Medium | High | Pin digest, immutable releases, record image digest | Tag resolves differently |
| R-14 | Upgrade migration breaks running binary | Medium | High | Expand/contract, migration bundle, upgrade test | Old binary fails after migration |
| R-15 | Disk fills from images/logs/builds | High | High | Retention, image cap, workspace cleanup, disk warning/degraded | Disk > 80% |
| R-16 | Scope creeps into Compose/multi-host/catalog | High | High | Explicit non-goals, one-service invariant, change control | Story needs a new runtime abstraction |
| R-17 | Detection is wrong for real repositories | High | Medium | Four supported stacks, evidence/confidence, overrides, failure taxonomy | High `detection.unsupported` rate |
| R-18 | Domain/TLS setup blocked by DNS or ACME | Medium | Medium | Exact DNS instructions, staging tests, retries, status | Repeated ACME failures |
| R-19 | GPL/unknown code enters the distribution | Medium | Critical | Clean-room policy, allowlist, SBOM, review | Unknown license or GPL header |
| R-20 | “Clean room” overclaimed | Medium | High | Document source-informed process honestly, evidence ledger, independent review | Structural similarity |
| R-21 | Arabic UI is a translation, not an experience | Medium | High | Native copy, RTL tests, Arabic usability test, glossary | RTL/layout defects or mixed-language errors |
| R-22 | Documentation drifts from implementation | High | Medium | Docs and runbooks in same PR, generated OpenAPI, ADR updates | Runbook step fails |
| R-23 | Single host is mistaken for HA | High | Medium | Explicit limits, no HA claims, backup/restore | User asks for failover |
| R-24 | No operator can debug at 3 a.m. | Medium | High | Stable codes, timeline, redacted logs, diagnostics, runbooks | Support request lacks evidence |

## 5. Assumptions

| ID | Assumption | If false |
|---|---|---|
| A-01 | The host is Linux with Docker and outbound HTTPS. | Install/runtime design changes; offline mode unsupported. |
| A-02 | The operator controls DNS for custom hostnames. | Domain verification needs provider APIs or delegation. |
| A-03 | Caddy can bind 80/443 and obtain ACME certificates. | TLS/edge design changes. |
| A-04 | PostgreSQL 17 is available as a container or external service. | Packaging and migration support change. |
| A-05 | The server can access the Docker socket. | Runtime moves to a host agent/proxy. |
| A-06 | One organization per install is enough for 0.1. | Membership and UI scope expand. |
| A-07 | The server process can handle four concurrent deployments. | Worker split or external queue needed earlier. |
| A-08 | Detection can be deterministic without executing repository code. | Detection becomes sandboxed execution, expanding scope. |
| A-09 | Apache-2.0 is acceptable for first-party code and docs. | License and contribution model change. |
| A-10 | GPL components can be excluded from the default distribution. | Distribution and legal review change. |
| A-11 | .NET 10 LTS remains supported for the 0.1 lifetime. | Runtime upgrade or fallback plan needed. |

## 6. Go/no-go gates

Before a public 0.1 release:

- [ ] R-01 has a written acceptance or a socket proxy/agent is added.
- [ ] Cross-tenant isolation tests pass for every endpoint.
- [ ] Secret-canary tests pass across logs, events, audit, errors, and telemetry.
- [ ] One-active-deployment concurrency test passes.
- [ ] Failed health never changes the active route.
- [ ] Worker kill/lease recovery test passes.
- [ ] Clean-host install, upgrade, backup, and restore drills pass.
- [ ] No GPL/unknown component in the default distribution.
- [ ] OpenAPI diff reviewed; no breaking change without a version plan.
- [ ] Arabic RTL review and beginner usability test completed.
- [ ] Runbooks tested by someone who did not write them.
- [ ] No unresolved High/Critical vulnerability without an approved exception.
- [ ] The 0.1 scope is still one host, one service per deployment, Docker/PostgreSQL/Caddy.

## 7. Validation spikes

| Spike | Question | Exit criterion |
|---|---|---|
| Docker socket threat model | Can the direct socket be accepted? | Written attack surface, label guard test, no privileged/host mounts, proxy decision |
| Caddy managed-set apply | Can routes be replaced atomically and reconciled? | Concurrent add/remove and Caddy restart tests pass; control route survives |
| .NET 10 package matrix | Do EF Core, Identity, Docker.DotNet, SignalR, and Testcontainers work? | Clean restore/build/test on Linux; fallbacks identified |
| Worker recovery | Does a killed worker resume without duplicate activation? | Kill during build/health; one active release; no duplicate route |
| Razor auth/antiforgery | Do cookie auth, interactive components, and CSRF coexist? | Login, mutation, logout, and CSRF tests pass |
| ACME staging | Can Caddy issue a certificate in staging? | Certificate issued; failure isolated; status surfaced |
| Source export safety | Can a malicious repo escape or exhaust the host? | Traversal, symlink, archive bomb, and size tests pass |
| Secret redaction | Can canaries be kept out of every sink? | Zero canary findings |
| OpenAPI generation | Does the generated document drive both clients? | Snapshot stable; generated clients compile; breaking check works |
| Clean-host install | Does the release artifact work without a dev environment? | Fresh host install, setup, deploy, domain, rollback |
| Backup/restore | Can data and secrets be restored? | Restore drill passes with master key |
| License provenance | Is every distributed component accounted for? | SBOM + license inventory; no unknown/GPL code |

## 8. Change control

If a later phase needs to change a decision:

1. Add a new decision row with the old ID in the rationale.
2. Update every affected document, requirement, issue, and ADR.
3. Mark the superseded decision `SUPERSEDED` rather than deleting it.
4. Re-run the link and naming checks in Phase 7.
