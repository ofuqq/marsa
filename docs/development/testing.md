# Marsa — Testing Strategy

> Status: Proposed for 0.1. Last reviewed: 2026-10-22.
> The strategy is designed for a privileged, single-host deployment product where a failed health gate or a route mistake is more costly than a failed build.

## 1. Testing principles

1. **Test invariants, not implementation details.** The state machine, tenancy, route safety, and secret redaction matter more than private methods.
2. **Use real PostgreSQL.** EF Core's in-memory provider hides SQL, constraints, indexes, and concurrency behavior.
3. **Docker and Caddy tests are explicit.** They are opt-in integration tests and required in the dedicated E2E lane.
4. **Every bug gets a regression test.** A fix without a failing test is incomplete.
5. **A flaky test is a defect.** Quarantine it with an owner and expiry; do not retry it into passing.
6. **Tests clean up after themselves.** Containers, networks, routes, databases, and workspaces are labelled and removed.
7. **Determinism is injected.** Use `TimeProvider`, `IIdGenerator`, and fake ports; never depend on wall-clock ordering.
8. **Security is tested as behavior.** Cross-tenant probes, redaction canaries, CSRF, SSRF, and webhook replay are first-class suites.

## 2. Test layers

| Layer | Scope | I/O | Speed | Required |
|---|---|---|---|---|
| Domain unit | Entities, value objects, state machine, invariants | None | Milliseconds | Every PR |
| Application | Handlers, validators, authorization, orchestration with fake ports | None | Fast | Every PR |
| Infrastructure integration | EF mappings, repositories, migrations, crypto, Docker/Caddy adapters | PostgreSQL; Docker/Caddy opt-in | Seconds | Every PR for PostgreSQL; E2E lane for Docker/Caddy |
| Server contract | Endpoints, auth matrix, ProblemDetails, OpenAPI, SignalR | `WebApplicationFactory` + PostgreSQL | Seconds | Every PR |
| CLI | Parsing, output, exit codes, HTTP behavior | WireMock | Fast | Every PR |
| Contract | OpenAPI snapshot, generated client compilation, breaking changes | Build artifacts | Fast | Every PR |
| E2E | Real Docker, Caddy, PostgreSQL, sample apps | Full stack | Minutes | Before release, nightly |
| Security | Authorization, tenancy, redaction, SSRF, CSRF, Docker guard | Mixed | Mixed | Every PR (fast) + release |

## 3. Test projects

| Project | Responsibilities |
|---|---|
| `Marsa.Domain.Tests` | State transitions, invalid transitions, secret version rules, route/domain validation, membership invariants |
| `Marsa.Application.Tests` | Deployment admission, cancellation, rollback, preparation, authorization, validation, idempotency |
| `Marsa.Infrastructure.Tests` | EF configurations, migrations, tenant filters, concurrency, outbox, secret protector, Docker/Caddy adapters |
| `Marsa.Server.Tests` | Endpoint contracts, cookies, antiforgery, rate limits, ProblemDetails, OpenAPI, SignalR authorization |
| `Marsa.Cli.Tests` | Commands, JSON output, exit codes, token handling, log following |
| `Marsa.E2E.Tests` | Push-to-deploy, failed build, failed health, rollback, cancellation, route conflict, worker recovery |
| `Marsa.Testing` | Builders, fixtures, fake ports, Testcontainers helpers, canary helpers |

Production projects never reference test projects.

## 4. Domain tests

Required cases:

- Every valid state transition.
- Every invalid transition is rejected.
- Terminal states are immutable.
- Cancellation is allowed only through `health_checking`.
- `routing` is not cancellable.
- A failed health check with a previous release becomes `action_required`.
- A failed health check with no previous release becomes `failed`.
- A route with a pending certificate becomes `succeeded_with_warnings`.
- Rollback creates a new deployment and never mutates history.
- The last owner cannot be removed.
- Secret versions are immutable.
- Snapshot content cannot be modified after admission.
- Hostname normalization rejects wildcards, ports, paths, and IP literals.
- Environment keys and path prefixes reject traversal.

## 5. Application tests

- Deployment admission locks the environment and rejects a second active deployment.
- Same idempotency key and same request returns the original result.
- Same key and different body returns `idempotency.conflict`.
- Same commit and plan returns the existing deployment unless forced.
- Cancellation persists a flag and does not stop the serving release.
- Rollback pins the prior image digest and snapshot.
- Missing secret references fail at `prepare` with a stable code.
- Authorization is enforced for owner, admin, developer, viewer, and PAT scopes.
- A project from another organization returns not found without existence leakage.
- Worker orchestration is idempotent when a step is retried.
- Artifact missing on rollback triggers the documented rebuild path.

## 6. Infrastructure tests

### 6.1 PostgreSQL

- Every EF configuration maps to the expected table and column names.
- Every tenant-owned entity has a global query filter.
- Partial unique indexes enforce:
  - one active deployment per environment,
  - one active release per service,
  - one owner per organization,
  - one default environment per project,
  - one primary domain per service.
- `xmin` concurrency rejects a stale update.
- Migrations apply from an empty database and from the previous release schema.
- Expand/contract migrations keep the previous binary working.
- Outbox dispatch is at-least-once and handlers are idempotent.
- Cursor pagination is stable under inserts.
- Log chunk sequence is unique and ordered.

### 6.2 Docker adapter

Opt-in with `Category=RequiresDocker`:

- Build a small image with BuildKit.
- Detect missing BuildKit without crashing the server.
- Create, start, inspect, stop, and remove a managed container.
- Refuse to mutate an unmanaged container.
- Reject privileged mode, host network, and host mounts.
- Apply resource limits and labels.
- Capture stdout/stderr separately.
- Recover an existing container by matching the config hash.
- Clean up only labeled resources.

### 6.3 Caddy adapter

Opt-in with `Category=RequiresCaddy`:

- Compile the control-plane route plus application routes.
- Apply the full managed set and preserve foreign configuration.
- Reject duplicate host/path.
- Reconcile after a Caddy restart.
- Verify the admin socket is not reachable over TCP.
- Preserve the static control route if a dynamic route fails.
- Surface certificate status without exposing private keys.
- Use the ACME staging CA in tests.

### 6.4 Crypto

- AES-GCM round trip.
- Wrong key version fails closed.
- Tampered ciphertext/tag fails.
- Nonce uniqueness under concurrent writes.
- Value hash equality does not reveal plaintext.
- Master key missing fails startup for secret-dependent operations.
- Key rotation re-encrypts lazily and preserves old versions.

## 7. Server and API tests

- Every endpoint has an authorization matrix:
  anonymous, viewer, developer, admin, owner, cross-tenant, revoked token.
- Cookie mutations without antiforgery return the expected error.
- Bearer requests are not CSRF-checked but are rate-limited.
- ProblemDetails has `code`, `status`, `traceId`, and no secret values.
- `X-Request-Id` is accepted or generated.
- OpenAPI examples validate against schemas.
- List endpoints enforce bounded pagination.
- Idempotency replay headers are correct.
- SignalR subscription rejects unauthorized projects and deployments.
- Reconnect replays events after the last sequence.
- Rate limits return `429` with `Retry-After`.
- Health endpoints do not depend on optional telemetry.
- Setup is one-time and rejects replayed setup tokens.

## 8. CLI tests

- `marsa login`, `logout`, `whoami`.
- `marsa projects list`, `deploy`, `status`, `logs --follow`.
- `marsa env`, `domains`, `secrets`.
- `--json` output is machine-readable and stable.
- Exit codes: `0` success, `1` user error, `2` auth, `3` server/network, `4` deployment failed, `5` validation.
- Token is stored with `0600`.
- Token is never accepted as a positional argument.
- `--yes` is required for destructive non-interactive actions.
- Logs resume after a dropped connection.
- Generated client compiles from the current OpenAPI document.

## 9. E2E scenarios

| Scenario | Assertions |
|---|---|
| Happy path | Push, build, health, route, HTTPS response, logs visible, old release retained |
| Failed build | Stable code, redacted logs, old release still serving |
| Failed health | No route change, `action_required` when a previous release exists |
| First deployment failure | `failed`, no route, clear next action |
| Cancellation | Old release serving, no orphan managed container/route |
| Rollback | New deployment, pinned digest, same health gate, history intact |
| Route conflict | `deployment.route_conflict`, no partial config |
| Worker kill | Lease recovered, no duplicate activation |
| Caddy restart | Routes reconciled, control plane reachable |
| Docker unavailable | Readiness degraded, deployment fails clearly |
| Disk pressure | Warning/degraded, no corruption, managed cleanup only |
| Secret rotation | Next deployment uses new version; active release unchanged |
| Webhook replay | Same delivery is ignored |
| Cross-tenant probe | No existence leak |
| Arabic RTL | 360 px and 1280 px, logs LTR, no clipped controls |

Fixtures:

- `aspnet-minimal`
- `node-express`
- `static-site`
- `dockerfile-app`
- `bad-health`
- `crash-loop`
- `prebuilt-image`

Fixtures are original and carry the repository license.

## 10. Security tests

- Cross-tenant access for every resource.
- PAT scope escalation attempts.
- Revoked token and expired token.
- Session fixation and logout invalidation.
- CSRF on cookie mutations.
- Open redirect and unsafe return URL.
- Host header and forwarded-header spoofing.
- SSRF through repository URL, redirect, and DNS rebinding.
- Path traversal, symlink escape, zip bomb, and oversized repository.
- Docker label spoofing and unmanaged container mutation.
- Privileged container, host mount, host network, and device requests.
- Caddy admin socket permissions and no TCP listener.
- Webhook signature, timestamp tolerance, replay, and delivery dedupe.
- Secret canaries in logs, events, audit, errors, telemetry, and downloads.
- Rate limits and request body limits.

## 11. Contract tests

- Generate OpenAPI from the implementation.
- Compare against `docs/api/openapi.v1.json`; fail on unapproved diff.
- Removed fields, narrowed types, changed enum values, and new required request fields fail.
- Generate the CLI/dashboard client; compile it.
- Validate examples against schemas.
- Ensure error codes used by the server exist in the documented catalog.
- Ensure `marsa.json` schema rejects unknown fields and accepts documented examples.

## 12. Determinism and isolation

- `FakeTimeProvider` for time.
- `IIdGenerator` for IDs.
- Unique database names per test run; drop in teardown.
- Unique project prefixes for E2E resources.
- Container/network labels for cleanup.
- No shared mutable static state.
- No test depends on execution order.
- No test calls the real internet; use WireMock or local fixtures.
- ACME tests use the staging directory, never production.

## 13. Coverage and quality gates

| Gate | Threshold |
|---|---|
| Overall line coverage | 70% |
| Domain line coverage | 85% |
| New code coverage | No hard gate in 0.1; reviewers judge risk |
| Nullable warnings | Zero |
| Analyzer warnings | Zero |
| Formatting | `dotnet format --verify-no-changes` |
| Architecture boundaries | Pass |
| Secret scan | Pass |
| License/SBOM scan | Pass |
| Vulnerability scan | No unapproved High/Critical |
| OpenAPI diff | Reviewed and additive |

Coverage is a floor, not a goal. A high-coverage test that asserts nothing is a defect.

## 14. CI and release lanes

### Pull request

```text
locked restore
format
build
unit tests
PostgreSQL integration tests
API contract tests
architecture tests
secret scan
license/SBOM scan
container build without push
```

### Main

- Multi-arch image build.
- SBOM and checksums.
- OpenAPI artifact.
- E2E smoke on a dedicated Linux runner.

### Release

- Full E2E suite.
- Clean-host install.
- Upgrade from previous release.
- Backup/restore drill with master key.
- Security checklist sign-off.
- License inventory sign-off.
- Runbooks updated.

## 15. Test data and secrets

- Never use real production secrets.
- Canary values are generated per test and unique.
- Fixtures use disposable credentials.
- Tokens and master keys are created in a temporary directory and removed in teardown.
- Database snapshots contain no real user data.
- Logs from failed tests must not contain canaries.

## 16. Definition of done for a change

- [ ] Behavior matches a requirement or accepts a documented exception.
- [ ] Domain/application tests cover the happy path and failure path.
- [ ] Integration test covers the database or adapter boundary.
- [ ] Authorization matrix updated if a new endpoint or role rule exists.
- [ ] Secret redaction verified if input can contain a secret.
- [ ] Migration is expand/contract and tested if schema changes.
- [ ] OpenAPI and clients updated if the public contract changes.
- [ ] Runbook updated if operations change.
- [ ] No new warning, flaky test, or unapproved dependency.
- [ ] The change stays inside the 0.1 scope.
