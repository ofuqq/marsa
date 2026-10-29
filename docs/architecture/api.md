# Marsa — API Contracts

> Status: Proposed for 0.1. Last reviewed: 2026-10-22.
> Base path: `/api/v1`. The implementation is the source of OpenAPI; this document is the human contract. Breaking changes require `/api/v2` and an ADR.

## 1. Conventions

| Concern | Rule |
|---|---|
| Format | JSON, UTF-8, `application/json` |
| Versioning | URL path, `/api/v1`; additive changes within v1 |
| IDs | UUIDv7 as lowercase strings |
| Timestamps | ISO 8601 UTC, e.g. `2026-10-22T12:00:00Z` |
| Enums | Lowercase strings; unknown values are a contract change |
| Field names | `camelCase` |
| Nulls | Explicit `null`; omitted only when the field is not applicable |
| Pagination | Cursor-based, `limit` 1–100, default 50 |
| Errors | RFC 9457 ProblemDetails with stable `code` |
| Idempotency | `Idempotency-Key` on retryable mutations |
| Request ID | `X-Request-Id` accepted or generated and returned |
| Trace | W3C `traceparent` accepted and propagated |
| OpenAPI | `GET /openapi/v1.json`; interactive docs only in Development |

### 1.1 Authentication

| Client | Mechanism |
|---|---|
| Dashboard | ASP.NET Core Identity cookie: `HttpOnly`, `Secure` in production, `SameSite=Lax` |
| CLI/API | `Authorization: Bearer marsa_pat_...` |
| Webhook | Provider signature or HMAC; no user session |
| Setup | One-time setup token from install output |

Cookie-authenticated unsafe methods require an antiforgery token. Bearer requests are exempt from CSRF but rate-limited per token.

### 1.2 Standard headers

- `X-Request-Id`
- `traceparent`
- `RateLimit-Limit`, `RateLimit-Remaining`, `RateLimit-Reset`
- `ETag` and `If-Match` on mutable resources where lost updates matter
- `Idempotent-Replay: true` on a replayed response

### 1.3 Pagination

```http
GET /api/v1/projects?limit=50&cursor=eyJjcmVhdGVkQXQiOiIuLi4ifQ
```

```json
{
  "items": [],
  "nextCursor": "eyJjcmVhdGVkQXQiOiIuLi4ifQ",
  "total": 12
}
```

- `cursor` is opaque base64url; clients must not parse it.
- `total` is omitted when the count is expensive, such as old audit rows or deployment logs.
- Sorting is fixed per endpoint in 0.1.

### 1.4 Idempotency

Required on:

- `POST /projects/{projectId}/deployments`
- `POST /deployments/{deploymentId}/rollback`
- `POST /services/{serviceId}/domains`
- `POST /projects/{projectId}/secrets`

Rules:

- 8–128 visible ASCII characters.
- Scoped to the organization and operation.
- Same key and same request: return the original response with `Idempotent-Replay: true`.
- Same key and different request: `409 idempotency.conflict`.
- If the original request is still running, return `409 idempotency.in_progress` with `Retry-After`.

### 1.5 Errors

```json
{
  "type": "urn:marsa:error:deployment-health-timeout",
  "title": "Release did not become healthy",
  "status": 422,
  "detail": "Health check /health did not pass within 60 seconds.",
  "instance": "/api/v1/deployments/0192f5b4-6d1a-7c3e-9f00-1a2b3c4d5e6f",
  "code": "deployment.health_timeout",
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "errors": {}
}
```

`type` uses `urn:marsa:error:<code>` until a public documentation domain is owned. Do not invent a public `marsa.dev` URL.

### 1.6 Error codes

| Code | HTTP | Meaning |
|---|---|---|
| `auth.required` | 401 | Missing or invalid credentials |
| `auth.invalid_token` | 401 | PAT malformed, revoked, or expired |
| `auth.forbidden` | 403 | Authenticated but not allowed |
| `setup.required` | 409 | Setup not completed |
| `setup.already_complete` | 409 | Setup already completed |
| `setup.token_invalid` | 401 | Setup token wrong or expired |
| `project.not_found` | 404 | Project not found in organization |
| `project.slug_conflict` | 409 | Slug already exists |
| `source.invalid_url` | 422 | Repository URL rejected |
| `source.auth_failed` | 422 | Provider rejected credentials |
| `source.repo_too_large` | 413 | Repository exceeds limit |
| `detection.unsupported` | 422 | No supported runtime detected |
| `deployment.active_exists` | 409 | Another deployment is active |
| `deployment.invalid_transition` | 409 | Transition not allowed |
| `deployment.idempotency_conflict` | 409 | Same key, different request |
| `deployment.build_failed` | 422 | Build failed |
| `deployment.health_timeout` | 422 | Health gate timed out |
| `deployment.route_conflict` | 409 | Hostname/path conflict |
| `deployment.attempts_exhausted` | 409 | Retry limit reached |
| `domain.conflict` | 409 | Hostname already registered |
| `domain.not_verified` | 422 | Domain not verified |
| `domain.invalid_hostname` | 422 | Hostname invalid |
| `secret.invalid` | 422 | Secret key or value invalid |
| `secret.reference_missing` | 422 | `secret://` reference unresolved |
| `server.unavailable` | 503 | Docker or Caddy unavailable |
| `rate_limited` | 429 | Rate limit exceeded |
| `idempotency.conflict` | 409 | Key reused with different body |
| `idempotency.in_progress` | 409 | Original request still running |
| `internal.error` | 500 | Unexpected server error |

## 2. System and setup

| Method | Path | Auth | Request | Response |
|---|---|---|---|---|
| `GET` | `/system/info` | none | — | Instance name, version, setup state, auth modes |
| `GET` | `/system/setup` (and `/api/v1/system/setup`) | none | — | `{required, completed, tokenRequired}` |
| `POST` | `/system/setup` (and `/api/v1/system/setup`) | setup token | Owner email, password, display name, org name, org slug | `201` session + organization |
| `GET` | `/system/version` | read | — | Version, commit, build date, schema version |
| `GET` | `/system/diagnostics` | admin | — | Redacted host, database, Docker, Caddy, disk, version report |
| `GET` | `/health/live` | none | — | Liveness |
| `GET` | `/health/ready` | none | — | Readiness with component status |
| `GET` | `/health/degraded` | none | — | Warnings and optional-component state |

### 2.1 Setup

`POST /system/setup` (and `/api/v1/system/setup`) is allowed once. It creates the organization, owner, and membership in one transaction, marks setup complete, and returns a session cookie. The setup token is consumed. Once complete, all setup endpoints return `404 Not Found` to mask instance state. Repeated calls return `404`.

### 2.2 Diagnostics

The diagnostics response is redacted and includes:

```json
{
  "version": "0.1.0",
  "commit": "abc1234",
  "schemaVersion": 12,
  "host": {"os": "linux", "architecture": "x64", "diskFreeBytes": 0},
  "database": {"status": "online", "latencyMs": 2},
  "docker": {"status": "online", "version": "27.0.0", "buildkit": true},
  "caddy": {"status": "online", "adminSocket": true},
  "warnings": []
}
```

No environment values, tokens, secrets, or absolute host paths beyond documented roots.

## 3. Authentication and tokens

| Method | Path | Auth | Request | Response |
|---|---|---|---|---|
| `POST` | `/auth/login` | none | `{email, password, rememberMe}` | `204` + session cookie |
| `POST` | `/auth/logout` | session | — | `204` |
| `GET` | `/auth/me` | read | — | Current user, memberships, scopes |
| `GET` | `/auth/sessions` | read | — | Session metadata |
| `DELETE` | `/auth/sessions/{id}` | owner | — | `204` |
| `GET` | `/auth/tokens` | read | — | Token metadata, never values |
| `POST` | `/auth/tokens` | self, or admin for others | `{name, scopes, expiresAt?}` | `201` plaintext once |
| `DELETE` | `/auth/tokens/{id}` | owner/admin or self | — | `204` |
| `POST` | `/auth/tokens/{id}/rotate` | owner/admin or self | — | `201` new plaintext; old revoked |

Token creation response includes the plaintext exactly once:

```json
{
  "id": "0192f5b4-...",
  "name": "ci",
  "scopes": ["read", "deploy"],
  "token": "marsa_pat_...",
  "expiresAt": "2027-01-01T00:00:00Z"
}
```

## 4. Projects

| Method | Path | Auth | Request | Response |
|---|---|---|---|---|
| `GET` | `/projects` | read | `limit`, `cursor`, `q`, `archived` | Page |
| `POST` | `/projects` | developer | `{name, slug?, description?, defaultBranch?}` | `201` project |
| `GET` | `/projects/{projectId}` | read | — | Project |
| `PATCH` | `/projects/{projectId}` | developer | `{name?, description?, defaultBranch?}` | Updated project |
| `POST` | `/projects/{projectId}/archive` | admin | — | `204` |
| `POST` | `/projects/{projectId}/restore` | admin | — | `200` |
| `GET` | `/projects/{projectId}/config` | read | `commit?` | Effective config and source |
| `PUT` | `/projects/{projectId}/config` | developer | Manual override document | `200` effective config |
| `POST` | `/projects/{projectId}/detect` | developer | `{branch?, commit?}` | `200` detection result |
| `GET` | `/projects/{projectId}/detection` | read | — | Latest detection |
| `GET` | `/projects/{projectId}/overview` | read | — | Environments, services, latest deployments, domains |

### 4.1 Project representation

```json
{
  "id": "0192f5a0-1111-7abc-8def-0123456789ab",
  "name": "Acme API",
  "slug": "acme-api",
  "description": "Customer-facing API",
  "defaultBranch": "main",
  "configSource": "marsa_json",
  "detectedAt": "2026-10-22T11:58:00Z",
  "createdAt": "2026-10-20T09:00:00Z",
  "updatedAt": "2026-10-22T11:58:00Z",
  "environments": [
    {
      "id": "0192f5a1-2222-7abc-8def-0123456789ab",
      "name": "production",
      "slug": "production",
      "isDefault": true
    }
  ]
}
```

## 5. Sources and detection

| Method | Path | Auth | Request | Response |
|---|---|---|---|---|
| `GET` | `/projects/{projectId}/sources` | read | — | Source connections |
| `POST` | `/projects/{projectId}/sources` | developer | `{provider, repoUrl, authType, credential?, webhookSecret?}` | `201` |
| `PATCH` | `/sources/{sourceId}` | developer | `{defaultBranch?, credential?, webhookSecret?}` | Updated |
| `DELETE` | `/sources/{sourceId}` | admin | — | `204` |
| `POST` | `/sources/{sourceId}/test` | developer | — | `{ok, branches, defaultBranch, error?}` |
| `GET` | `/sources/{sourceId}/branches` | read | `q` | Branch list, max 100 |
| `POST` | `/sources/{sourceId}/webhook-secret` | admin | — | New secret once |

Source credentials and webhook secrets are write-only. Responses return `credentialSet`, `credentialHint`, and `webhookSecretPrefix`, never the values.

## 6. Environments and services

| Method | Path | Auth | Request | Response |
|---|---|---|---|---|
| `GET` | `/projects/{projectId}/environments` | read | — | Environments |
| `POST` | `/projects/{projectId}/environments` | developer | `{name, slug, isDefault?}` | `201` |
| `PATCH` | `/environments/{environmentId}` | developer | `{name?, isDefault?}` | Updated |
| `DELETE` | `/environments/{environmentId}` | admin | — | `204`; refuses if active |
| `GET` | `/projects/{projectId}/services` | read | `environmentId?` | Services |
| `POST` | `/projects/{projectId}/services` | developer | `{environmentId, name, kind, plan}` | `201` |
| `GET` | `/services/{serviceId}` | read | — | Service |
| `PATCH` | `/services/{serviceId}` | developer | `{plan?, port?}` | Updated |
| `DELETE` | `/services/{serviceId}` | admin | — | `204`; archives |
| `GET` | `/services/{serviceId}/env` | read | — | Keys, masked values, secret refs |
| `PUT` | `/services/{serviceId}/env` | developer | Array of `{key, value?, secretId?, isSecret}` | Metadata |
| `DELETE` | `/services/{serviceId}/env/{key}` | developer | — | `204` |
| `POST` | `/services/{serviceId}/env/{key}/reveal` | admin | — | Plaintext once; audited |
| `GET` | `/services/{serviceId}/logs` | read | `after`, `limit`, `stream` | Runtime log chunks |
| `GET` | `/services/{serviceId}/logs/stream` | read | `after`, `stream` | Server-sent events of runtime logs |
| `GET` | `/services/{serviceId}/logs/download` | read | — | Redacted text archive |

In 0.1 a service has one environment and one replica. A deployment targets exactly one service. The UI may hide the environment concept when only the default exists.

## 7. Deployments

| Method | Path | Auth | Request | Response |
|---|---|---|---|---|
| `POST` | `/projects/{projectId}/deployments` | deploy | `{environmentId, commit?, branch?, serviceId, force?}` + `Idempotency-Key` | `202` |
| `GET` | `/projects/{projectId}/deployments` | read | `environmentId?`, `limit`, `cursor`, `status` | Page |
| `GET` | `/deployments/{deploymentId}` | read | — | Deployment detail |
| `POST` | `/deployments/{deploymentId}/cancel` | deploy | — | `202` |
| `POST` | `/deployments/{deploymentId}/rollback` | deploy | `{targetDeploymentId?}` + `Idempotency-Key` | `202` new deployment |
| `POST` | `/deployments/{deploymentId}/redeploy` | deploy | — | `202` new deployment |
| `GET` | `/deployments/{deploymentId}/steps` | read | — | Steps in order |
| `GET` | `/deployments/{deploymentId}/events` | read | `after`, `limit` | Events in sequence order |
| `GET` | `/deployments/{deploymentId}/logs` | read | `after`, `limit`, `stream` | Log chunks |
| `GET` | `/deployments/{deploymentId}/logs/stream` | read | `after`, `stream` | Server-sent events of deployment logs |
| `GET` | `/deployments/{deploymentId}/logs/download` | read | — | Redacted text archive |

### 7.1 Admission response

```json
{
  "id": "0192f5b4-6d1a-7c3e-9f00-1a2b3c4d5e6f",
  "status": "queued",
  "projectId": "0192f5a0-1111-7abc-8def-0123456789ab",
  "environmentId": "0192f5a1-2222-7abc-8def-0123456789ab",
  "serviceId": "0192f5a2-3333-7abc-8def-0123456789ab",
  "commitSha": "8f3c2d1e0a9b8c7d6e5f4a3b2c1d0e9f8a7b6c5d",
  "branch": "main",
  "trigger": "manual",
  "createdAt": "2026-10-22T12:00:00Z",
  "links": {
    "self": "/api/v1/deployments/0192f5b4-6d1a-7c3e-9f00-1a2b3c4d5e6f",
    "logs": "/api/v1/deployments/0192f5b4-6d1a-7c3e-9f00-1a2b3c4d5e6f/logs"
  }
}
```

### 7.2 Deployment detail

```json
{
  "id": "0192f5b4-6d1a-7c3e-9f00-1a2b3c4d5e6f",
  "status": "health_checking",
  "trigger": "git_push",
  "commitSha": "8f3c2d1e0a9b8c7d6e5f4a3b2c1d0e9f8a7b6c5d",
  "branch": "main",
  "image": {
    "ref": "marsa/acme-api:8f3c2d1e",
    "digest": "sha256:3b1f..."
  },
  "failure": null,
  "steps": [
    {"sequence": 1, "name": "prepare", "status": "succeeded"},
    {"sequence": 2, "name": "build", "status": "succeeded"},
    {"sequence": 3, "name": "release", "status": "succeeded"},
    {"sequence": 4, "name": "start", "status": "succeeded"},
    {"sequence": 5, "name": "health", "status": "running"},
    {"sequence": 6, "name": "route", "status": "pending"}
  ],
  "rollbackOf": null,
  "createdAt": "2026-10-22T12:00:00Z",
  "startedAt": "2026-10-22T12:00:02Z",
  "finishedAt": null
}
```

### 7.3 Cancellation rules

- Allowed from `queued` through `health_checking`.
- Not allowed during `routing`; returns `409 deployment.invalid_transition`.
- The serving release is never stopped by cancellation.
- Cleanup is attempted and recorded; orphaned resources are reconciled.

## 8. Domains and certificates

| Method | Path | Auth | Request | Response |
|---|---|---|---|---|
| `GET` | `/services/{serviceId}/domains` | read | — | Domains and DNS instructions |
| `POST` | `/services/{serviceId}/domains` | developer | `{hostname, pathPrefix?, isPrimary?}` + `Idempotency-Key` | `201` |
| `GET` | `/domains/{domainId}` | read | — | Domain and certificate status |
| `DELETE` | `/domains/{domainId}` | admin | — | `204`; removes route |
| `POST` | `/domains/{domainId}/verify` | developer | — | `202` |
| `POST` | `/domains/{domainId}/retry-certificate` | admin | — | `202` |
| `GET` | `/domains/{domainId}/dns-instructions` | read | — | Exact TXT/CNAME records |

DNS instructions include the record type, name, value, TTL, and whether the record is a verification TXT or traffic CNAME. A domain cannot be attached to a service until verified.

## 9. Secrets

| Method | Path | Auth | Request | Response |
|---|---|---|---|---|
| `GET` | `/projects/{projectId}/secrets` | read | `scope`, `scopeId` | Metadata only |
| `POST` | `/projects/{projectId}/secrets` | developer | `{key, description?, scope, scopeId?, value}` + `Idempotency-Key` | `201` metadata |
| `GET` | `/secrets/{secretId}` | read | — | Metadata and versions |
| `POST` | `/secrets/{secretId}/versions` | developer | `{value}` | `201` metadata |
| `GET` | `/secrets/{secretId}/versions` | read | — | Version metadata |
| `DELETE` | `/secrets/{secretId}` | admin | — | `204`; archives |

No endpoint lists plaintext values. The optional reveal endpoint for environment variables is admin-only, disabled by default, and audited.

## 10. Servers

| Method | Path | Auth | Request | Response |
|---|---|---|---|---|
| `GET` | `/servers` | read | — | Servers |
| `GET` | `/servers/local` | read | — | Local server |
| `GET` | `/servers/{serverId}` | read | — | Server and capabilities |
| `PATCH` | `/servers/{serverId}` | admin | `{name}` | Updated |
| `POST` | `/servers/{serverId}/health-check` | admin | — | `202` |

## 11. Audit and webhooks

| Method | Path | Auth | Request | Response |
|---|---|---|---|---|
| `GET` | `/audit` | read | `actor`, `action`, `targetType`, `targetId`, `from`, `to`, `cursor` | Page |
| `GET` | `/webhook-deliveries` | admin | `projectId`, `status`, `cursor` | Page |
| `POST` | `/webhooks/github` | signature | GitHub push | `202` or `204` ignored |
| `POST` | `/webhooks/gitlab` | signature | GitLab push | `202` or `204` ignored |
| `POST` | `/webhooks/generic/{sourceId}` | HMAC | `{ref, after, repository}` | `202` or `204` ignored |

### 11.1 GitHub verification

Headers: `X-GitHub-Event`, `X-GitHub-Delivery`, `X-Hub-Signature-256`.

1. Read the raw body with a 1 MiB limit.
2. Compute HMAC-SHA256 with the project webhook secret.
3. Compare in constant time.
4. Deduplicate on `(github, X-GitHub-Delivery)`.
5. Accept only `push`.
6. Ignore deletions, tags, and unconfigured branches.
7. Enqueue a deployment for the pushed commit when configured.
8. Return quickly.

GitHub App installation and pull-request previews are post-MVP.

### 11.2 Generic webhook

```http
POST /api/v1/webhooks/generic/{sourceId}
X-Marsa-Signature: sha256=<hex>
X-Marsa-Delivery: <unique-id>
```

```json
{
  "ref": "refs/heads/main",
  "after": "8f3c2d1e0a9b8c7d6e5f4a3b2c1d0e9f8a7b6c5d",
  "repository": "acme/api"
}
```

## 12. Realtime contract

Hub: `/hubs/deployments`.

### 12.1 Client methods

| Method | Arguments | Semantics |
|---|---|---|
| `SubscribeProject` | `projectId` | Join a project group after authorization |
| `UnsubscribeProject` | `projectId` | Leave the group |
| `SubscribeDeployment` | `deploymentId`, `afterSequence?` | Join and replay missed events |
| `UnsubscribeDeployment` | `deploymentId` | Leave the group |

### 12.2 Server events

| Event | Payload |
|---|---|
| `deployment.updated` | `{deploymentId, status, step, failure?, updatedAt}` |
| `deployment.step.updated` | `{deploymentId, sequence, name, status, error?}` |
| `deployment.log.appended` | `{deploymentId, sequence, stream, content, createdAt}` |
| `deployment.finished` | `{deploymentId, status, finishedAt}` |
| `route.updated` | `{domainId, hostname, status, certificateStatus}` |
| `server.updated` | `{serverId, status, capabilities}` |

Rules:

- Logs are chunked at 16 KiB and sequence-numbered.
- Slow clients may receive coalesced or dropped log chunks; the worker never blocks.
- SignalR is not the source of truth; clients refetch after reconnect.
- Authorization is checked at subscription and on event delivery.
- Payloads never contain secret values or full environment maps.

## 13. `marsa.json`

`marsa.json` is an optional repository-root file. It overrides detection for the fields it declares. API overrides win last.

```json
{
  "$schema": "urn:marsa:schema:marsa.json:v1",
  "version": 1,
  "services": [
    {
      "name": "web",
      "kind": "web",
      "build": {
        "type": "auto",
        "context": ".",
        "dockerfile": "Dockerfile",
        "args": {}
      },
      "run": {
        "port": 8080,
        "command": null
      },
      "healthCheck": {
        "path": "/health",
        "intervalSeconds": 5,
        "timeoutSeconds": 3,
        "retries": 12,
        "startPeriodSeconds": 10
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "DATABASE_URL": "secret://DATABASE_URL"
      }
    }
  ],
  "domains": [
    {
      "hostname": "api.example.com",
      "service": "web",
      "pathPrefix": "/",
      "primary": true
    }
  ]
}
```

### 13.1 Field rules

| Field | Required | Rule |
|---|---|---|
| `version` | yes | Must be `1` in 0.1 |
| `services` | no | One service if omitted; max 1 in 0.1 |
| `services[].name` | yes | DNS-label safe; unique |
| `services[].kind` | no | `web` default; `worker`, `static` |
| `build.type` | no | `auto`, `dockerfile`, `static`, `image` |
| `build.context` | no | Relative path under root; no `..` |
| `build.dockerfile` | no | Relative path |
| `build.args` | no | String map; `secret://NAME` allowed |
| `run.port` | web only | 1–65535 |
| `run.command` | no | Array of strings preferred; string form rejected |
| `healthCheck.path` | no | Default `/`; must start with `/` |
| `healthCheck.intervalSeconds` | no | 1–60; default 5 |
| `healthCheck.timeoutSeconds` | no | 1–30; default 3 |
| `healthCheck.retries` | no | 1–60; default 12 |
| `healthCheck.startPeriodSeconds` | no | 0–300; default 10 |
| `env` | no | String map; `secret://NAME` references a project secret |
| `domains[]` | no | Max 1 in 0.1 |
| `domains[].hostname` | yes | FQDN; no wildcard |
| `domains[].service` | yes | Must match a service name |
| `domains[].pathPrefix` | no | Default `/`; no traversal or encoded separators |
| `domains[].primary` | no | At most one per service |

Rules:

- Unknown fields are rejected in schema version 1 (`additionalProperties: false`).
- Environment keys match `^[A-Z_][A-Z0-9_]*$`.
- Plain env values may not contain newlines; secrets must use `secret://`.
- Effective config is detection, then `marsa.json`, then explicit API override.
- A malformed file fails at `prepare` with a precise validation error; there is no silent fallback.

## 14. OpenAPI and clients

- OpenAPI is generated at build time from endpoint metadata and contracts.
- CI stores `docs/api/openapi.v1.json` and fails on an unapproved diff.
- The dashboard and CLI consume generated clients; no hand-written duplicate DTOs.
- Breaking-change detection fails on removed fields, narrowed types, changed enum values, and new required request fields.
- Examples in contracts are part of the OpenAPI document.
- `GET /openapi/v1.json` is available in production for integrations.
- Interactive docs are Development-only unless explicitly enabled.

## 15. Compatibility policy

- `/api/v1` remains additive for the life of 0.1.
- A field removal or semantic change requires `/api/v2` and at least one minor release of overlap.
- Unknown response fields must be ignored by clients.
- The CLI and server publish a compatibility matrix.
- API errors, enum values, and idempotency behavior are part of the public contract.
