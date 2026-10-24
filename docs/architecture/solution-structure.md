# Marsa — .NET Solution Structure

> Status: Proposed for 0.1. Last reviewed: 2026-10-22.
> This document defines the solution shape, project boundaries, and conventions. It is a contract for implementation; deviations require an ADR and an update here.

## 1. Principles

1. **One deployable server.** API, dashboard, SignalR, and worker ship in `Marsa.Server`. The worker is a hosted service, not a separate process in 0.1.
2. **Dependencies point inward.** Domain knows nothing about infrastructure. Application knows Domain and Contracts. Infrastructure implements Application ports.
3. **Contracts are the wire boundary.** Dashboard and CLI consume the same versioned contracts.
4. **Small projects, clear ownership.** A new project needs a demonstrated boundary, not a folder preference.
5. **Tests are production assets.** They live in dedicated projects and never leak into production references.
6. **Build reproducibility.** SDK, packages, and analyzers are pinned; CI restores locked and treats warnings as errors.

## 2. Repository layout

```text
marsa/
├── .config/
│   └── dotnet-tools.json
├── .editorconfig
├── .github/
│   ├── ISSUE_TEMPLATE/
│   ├── workflows/
│   └── PULL_REQUEST_TEMPLATE.md
├── AGENTS.md
├── CODEOWNERS
├── CODE_OF_CONDUCT.md
├── CONTRIBUTING.md
├── Directory.Build.props
├── Directory.Build.targets
├── Directory.Packages.props
├── LICENSE
├── Marsa.sln
├── NOTICE
├── NuGet.config
├── README.md
├── SECURITY.md
├── deploy/
│   ├── caddy/
│   │   ├── Caddyfile.template
│   │   └── admin-policy.json
│   ├── compose.yaml
│   ├── compose.dev.yaml
│   ├── scripts/
│   │   ├── backup-postgres.sh
│   │   ├── install.sh
│   │   └── upgrade.sh
│   ├── server.Dockerfile
│   └── systemd/
│       └── marsa.service
├── docs/
├── eng/
│   ├── build.sh
│   ├── publish.sh
│   ├── smoke-e2e.sh
│   ├── test.sh
│   └── verify.sh
├── global.json
├── src/
│   ├── Marsa.Application/
│   ├── Marsa.Cli/
│   ├── Marsa.Contracts/
│   ├── Marsa.Domain/
│   ├── Marsa.Infrastructure/
│   └── Marsa.Server/
└── tests/
    ├── Marsa.Application.Tests/
    ├── Marsa.Cli.Tests/
    ├── Marsa.Domain.Tests/
    ├── Marsa.E2E.Tests/
    ├── Marsa.Infrastructure.Tests/
    ├── Marsa.Server.Tests/
    └── Marsa.Testing/
```

## 3. Projects and references

| Project | Type | Responsibility | References |
|---|---|---|---|
| `Marsa.Domain` | Class library | Aggregates, entities, value objects, domain events, state machine, invariants | None |
| `Marsa.Contracts` | Class library | DTOs, wire enums, error codes, SignalR payloads, webhook payloads, `marsa.json` schema types | None |
| `Marsa.Application` | Class library | Use cases, validation, authorization, ports, orchestration | Domain, Contracts |
| `Marsa.Infrastructure` | Class library | EF Core, Docker, Caddy, Git, crypto, telemetry adapters | Application, Domain, Contracts |
| `Marsa.Server` | Executable | Composition root, Minimal APIs, Razor Components, SignalR, worker, health, OpenAPI | Application, Infrastructure, Contracts |
| `Marsa.Cli` | Executable | API client and command line output | Contracts |
| `Marsa.Testing` | Class library | Builders, fixtures, fakes, Testcontainers helpers | Production projects as needed |
| `Marsa.*.Tests` | Test projects | Unit, integration, contract, E2E tests | Production projects and Testing |

### 3.1 Forbidden references

- Domain must not reference EF Core, ASP.NET Core, Docker, Caddy, HTTP, configuration, logging, or any other Marsa project.
- Contracts must not reference Domain, persistence types, or infrastructure types.
- Application must not reference EF Core, `Docker.DotNet`, `Microsoft.AspNetCore.*`, or Infrastructure.
- Infrastructure must not contain HTTP endpoints or Razor components.
- Server must not reference a test project.
- CLI must not access the database, Docker socket, Caddy socket, or server business rules.
- Production projects must never reference `Marsa.Testing`.

CI enforces namespace bans with architecture tests and a source scan.

## 4. Project folder structure

### 4.1 Marsa.Domain

```text
Marsa.Domain/
├── Common/
│   ├── Entity.cs
│   ├── AggregateRoot.cs
│   ├── ValueObject.cs
│   ├── DomainEvent.cs
│   └── Result.cs
├── Organizations/
├── Identity/
├── Projects/
├── Environments/
├── Services/
├── Deployments/
│   ├── Deployment.cs
│   ├── DeploymentSnapshot.cs
│   ├── DeploymentState.cs
│   ├── DeploymentStateMachine.cs
│   └── Events/
├── Releases/
├── Routing/
├── Secrets/
├── Servers/
├── Auditing/
└── Settings/
```

Rules:

- One public type per file; file name matches the type.
- Aggregates expose behavior, not public setters.
- State transitions go through `DeploymentStateMachine`; callers cannot assign `Status` directly.
- Domain events describe facts in the past tense.
- Domain validation throws a specific domain exception or returns a `Result`; no infrastructure exceptions.

### 4.2 Marsa.Application

```text
Marsa.Application/
├── Abstractions/
│   ├── IClock.cs
│   ├── IIdGenerator.cs
│   ├── IUnitOfWork.cs
│   └── ITenantContext.cs
├── Common/
│   ├── Behaviors/
│   ├── Result.cs
│   └── PagedResult.cs
├── Identity/
├── Organizations/
├── Projects/
├── Sources/
├── Detection/
├── Deployments/
│   ├── Admission/
│   ├── Commands/
│   ├── Queries/
│   ├── Orchestration/
│   │   └── Steps/
│   └── Ports/
├── Build/
├── Runtime/
├── Routing/
├── Secrets/
├── Servers/
├── Webhooks/
├── Auditing/
├── Setup/
└── DependencyInjection.cs
```

Each use case gets one folder containing its command/query, handler, validator, and result. A handler owns one transaction. Handlers do not call other handlers.

### 4.3 Marsa.Infrastructure

```text
Marsa.Infrastructure/
├── Persistence/
│   ├── MarsaDbContext.cs
│   ├── Configurations/
│   ├── Migrations/
│   ├── Repositories/
│   ├── Outbox/
│   ├── Interceptors/
│   └── Transactions/
├── Docker/
│   ├── DockerRuntime.cs
│   ├── DockerConnectionFactory.cs
│   ├── DockerBuildService.cs
│   ├── DockerContainerService.cs
│   ├── DockerLogSource.cs
│   └── DockerOptions.cs
├── Edge/
│   ├── CaddyAdminClient.cs
│   ├── CaddyRouteCompiler.cs
│   ├── CaddyHealthProbe.cs
│   └── CaddyOptions.cs
├── Git/
│   ├── GitCliSource.cs
│   ├── GitCommandRunner.cs
│   ├── GitHubSource.cs
│   └── SourceMaterializer.cs
├── Providers/
│   ├── GitHub/
│   ├── GitLab/
│   └── GenericGit/
├── Security/
│   ├── SecretProtector.cs
│   ├── MasterKeyStore.cs
│   ├── ApiTokenHasher.cs
│   └── Redaction.cs
├── Telemetry/
├── Time/
├── Auditing/
└── DependencyInjection.cs
```

### 4.4 Marsa.Server

```text
Marsa.Server/
├── Program.cs
├── Endpoints/
│   ├── SystemEndpoints.cs
│   ├── AuthEndpoints.cs
│   ├── ProjectEndpoints.cs
│   ├── DeploymentEndpoints.cs
│   ├── DomainEndpoints.cs
│   ├── SecretEndpoints.cs
│   ├── AuditEndpoints.cs
│   └── WebhookEndpoints.cs
├── Auth/
├── Dashboard/
│   ├── Components/
│   ├── Pages/
│   ├── Layout/
│   └── Resources/
├── Realtime/
│   ├── DeploymentHub.cs
│   └── RealtimePublisher.cs
├── Worker/
│   ├── DeploymentWorker.cs
│   ├── OutboxDispatcher.cs
│   └── Reconciler.cs
├── Middleware/
├── OpenApi/
├── Health/
└── wwwroot/
```

The server is the only composition root. Endpoints do not use `MarsaDbContext` directly. Razor Components call application handlers or a typed API client, never repositories.

### 4.5 Marsa.Cli

```text
Marsa.Cli/
├── Program.cs
├── Commands/
├── Configuration/
├── Http/
└── Output/
```

The CLI stores tokens at `~/.config/marsa/config.json` with mode `0600` or in the OS keyring when available. Token values are never accepted as positional arguments.

## 5. Build configuration

| Setting | Value |
|---|---|
| SDK | .NET SDK `10.0.100`, `rollForward: latestFeature` |
| Target framework | `net10.0` |
| Language | C# 14 (`LangVersion=latest`) |
| Nullable | Enabled |
| Warnings | `TreatWarningsAsErrors=true` |
| Analyzers | `AnalysisLevel=latest-recommended`, `EnableNETAnalyzers=true` |
| Style | `EnforceCodeStyleInBuild=true`, `.editorconfig` enforced |
| Determinism | `Deterministic=true`, `ContinuousIntegrationBuild=true` in CI |
| Docs | `GenerateDocumentationFile=true` for libraries |
| Globalization | `InvariantGlobalization=false` |
| Trimming | `PublishTrimmed=false` for 0.1 |
| Package versions | Central Package Management; no floating versions |
| Restore | `packages.lock.json` committed; locked mode in CI |
| NuGet source | `nuget.org` only, with source mapping |
| Tests | xUnit v3; coverlet; Testcontainers for PostgreSQL |

### 5.1 Core commands

```bash
dotnet restore --locked-mode
dotnet tool restore
dotnet format --verify-no-changes
dotnet build Marsa.sln -c Release --no-restore
dotnet test Marsa.sln -c Release --no-build
```

### 5.2 Local services

```bash
docker compose -f deploy/compose.dev.yaml up -d postgres caddy
dotnet run --project src/Marsa.Server
```

## 6. Package policy

| Project | Allowed package surface |
|---|---|
| Domain | BCL only; no third-party runtime package without an ADR |
| Contracts | `System.Text.Json` attributes and BCL only |
| Application | DI/logging abstractions, FluentValidation, rate-limiting abstractions |
| Infrastructure | EF Core, Npgsql, EFCore.NamingConventions, Docker.DotNet, OpenTelemetry, provider SDKs, crypto |
| Server | ASP.NET Core shared framework, OpenAPI, SignalR, health checks, Identity |
| CLI | `System.CommandLine`, optional `Spectre.Console`, `System.Net.Http.Json` |

Rules:

- A new package must have a permissive license, an active maintenance record, and a documented purpose.
- No GPL, AGPL, SSPL, BUSL, Commons Clause, or unknown-license dependency in the default distribution.
- No shell-execution helper for user input.
- No reflection-based object mapping in request paths.
- No second JSON serializer without an ADR.
- No EF Core in-memory provider for tests.

## 7. Code conventions

### 7.1 General

- File-scoped namespaces.
- Four-space indentation; max line length 120.
- One public type per file; `sealed` by default.
- `record` for immutable DTOs, events, and value objects; classes for entities.
- Async methods end in `Async`; `CancellationToken` is the last parameter.
- No `.Result`, `.Wait()`, `async void`, or fire-and-forget tasks.
- Expected failures use a typed `Result<T>`; exceptions are for bugs and infrastructure faults.
- Inject `TimeProvider`; never call `DateTime.UtcNow` in domain or application code.
- Inject `IIdGenerator`; UUIDv7 creation is centralized.
- No static mutable state, service locator, or ambient `HttpContext` in application code.
- Structured logging only; never interpolate secrets, tokens, env values, or credentialed URLs.

### 7.2 Domain and application

- One use case per folder.
- Command handlers own one transaction.
- Cross-aggregate effects use domain events and the outbox.
- Authorization is enforced at the endpoint and reinforced in the application handler.
- Every external call is cancellable and time-bounded.
- Deployment steps are idempotent and safe to retry.
- The state machine is authoritative; no ad hoc status writes.
- Rollback creates a new deployment; it never mutates a prior release.

### 7.3 EF Core

- Configurations live in `Infrastructure/Persistence/Configurations`.
- Table and column names use PostgreSQL `snake_case`.
- Every tenant-owned row has `organization_id` and a global query filter.
- Concurrency uses PostgreSQL `xmin` mapped to a row version.
- JSONB is limited to versioned snapshots, plan documents, and provider metadata.
- Partial unique indexes enforce concurrency invariants.
- Migrations are generated from Infrastructure and reviewed as code.
- No `EnsureCreated`, lazy loading, or automatic production migration.
- Integration tests use PostgreSQL 17 through Testcontainers.

### 7.4 API and UI

- Minimal APIs with endpoint filters for validation, authorization, and ProblemDetails.
- One endpoint calls one application handler.
- `/api/v1` is additive; breaking changes require `/api/v2` and a deprecation window.
- OpenAPI is generated from the implementation; examples live in contracts.
- The dashboard uses the same application services and does not query the database.
- Razor Components remain server-rendered; interactive islands are small and optional.
- All user-facing strings are localizable; no concatenated sentences.
- Log and code blocks remain LTR in Arabic layouts.

## 8. Dependency injection and composition

`Program.cs` composes modules explicitly:

```text
AddMarsaDomain()
AddMarsaApplication()
AddMarsaInfrastructure(configuration)
AddMarsaServer(configuration)
AddMarsaAuthentication()
AddMarsaOpenApi()
AddMarsaRealtime()
AddMarsaWorker()
AddMarsaHealthChecks()
```

Rules:

- Each project exposes one `Add...` extension.
- No assembly scanning for business handlers; registration is explicit or source-generated.
- Options are validated on startup with `ValidateOnStart`.
- The server fails startup if required configuration, master key, database schema, or Caddy socket is invalid.
- Optional integrations are registered only when configured.
- Feature flags default off for deferred features.

## 9. Error handling

- Domain throws specific exceptions for invariant violations that indicate bugs.
- Application returns `Result<T>` for expected failures with a stable code.
- Infrastructure maps provider errors to stable application errors.
- The API maps errors to RFC 9457 ProblemDetails with `code`, `traceId`, and no secret values.
- The worker persists a stable `failure_code` and a redacted `failure_message`.
- Unexpected exceptions are logged with a correlation ID and returned as `internal.error`.

## 10. Testing layout

| Project | Scope |
|---|---|
| `Marsa.Domain.Tests` | Invariants, state transitions, value objects, domain events |
| `Marsa.Application.Tests` | Handlers, validators, authorization, orchestration with fake ports |
| `Marsa.Infrastructure.Tests` | EF mappings, repositories, migrations, crypto, Docker/Caddy adapters |
| `Marsa.Server.Tests` | Endpoint contracts, auth matrix, ProblemDetails, OpenAPI, SignalR |
| `Marsa.Cli.Tests` | Parsing, output, exit codes, HTTP behavior |
| `Marsa.E2E.Tests` | Real Docker, Caddy, PostgreSQL, deploy/rollback scenarios |

- Unit tests have no I/O.
- Integration tests use real PostgreSQL; Docker/Caddy tests are opt-in by trait.
- Every bug fix gets a regression test.
- A flaky test is a defect with an owner and quarantine date.
- Coverage floors: 70% overall, 85% for Domain.

## 11. CI gates

```text
restore --locked-mode
format --verify-no-changes
build -c Release
unit tests
PostgreSQL integration tests
OpenAPI contract diff
architecture boundary tests
secret scan
license/SBOM scan
container build (no push on pull requests)
```

E2E runs on a dedicated Linux runner before release, not on every pull request.

## 12. What must not be added without an ADR

- A second deployable service.
- Redis, a message broker, or an object store.
- A SPA, Node build pipeline, or separate frontend host.
- A second database or ORM.
- A plugin loader or arbitrary assembly loading.
- Multi-host, Kubernetes, Compose, or multi-service orchestration.
- A background job framework that owns deployment state.
- A public SDK or generated client outside the documented API.
