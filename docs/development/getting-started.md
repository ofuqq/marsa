# Marsa — Development Environment

> Status: Proposed for 0.1. Last reviewed: 2026-10-22.
> The supported development workflow is Linux or WSL2. macOS may work for editing and unit tests, but the Docker/Caddy end-to-end path is only claimed on Linux.

## 1. Prerequisites

| Tool | Version | Why |
|---|---|---|
| .NET SDK | `10.0.100` (pinned by `global.json`) | Build and test |
| Docker Engine | 27+ with BuildKit | Builds and runtime |
| Docker Compose | v2 | Local PostgreSQL and Caddy |
| Git | Current | Source control |
| PostgreSQL client | 17 (optional) | Debugging the local database |
| Caddy | Not installed on host | Runs in Compose; do not mix host Caddy |
| Node.js | Not required | No SPA or frontend build |

Linux host requirements for a realistic E2E run:

- 2 vCPU minimum, 4 vCPU recommended.
- 4 GB RAM minimum, 8 GB recommended.
- 20 GB free disk.
- Ports `80` and `443` free for Caddy; local development uses `8080`/`8443`.
- User can access `/var/run/docker.sock` or a rootless Docker socket.

## 2. First checkout

```bash
git clone https://github.com/ofuqq/marsa.git
cd marsa
dotnet restore
dotnet tool restore
cp deploy/compose.dev.env.example deploy/compose.dev.env
docker compose -f deploy/compose.dev.yaml up -d postgres caddy
dotnet run --project src/Marsa.Server
```

The server prints the local URL and a one-time setup token in Development. Do not commit the token or the local env file.

## 3. Local ports

| Service | Port | Notes |
|---|---|---|
| Marsa server | `5080` | HTTP only in Development |
| PostgreSQL | `5432` | Bound to `127.0.0.1` |
| Caddy HTTP | `8080` | Local edge |
| Caddy HTTPS | `8443` | Local TLS, self-signed/ACME staging |
| Caddy admin | Unix socket in a named volume | Never TCP |

The production stack publishes only `80` and `443` through Caddy. The server and database are never published.

## 4. Local configuration

Use environment variables or `dotnet user-secrets`. Never commit a real `.env`.

```text
ASPNETCORE_ENVIRONMENT=Development
MARSA_Database__ConnectionString=Host=localhost;Port=5432;Database=marsa;Username=marsa;Password=marsa
MARSA_Edge__AdminSocket=/run/marsa/caddy/admin.sock
MARSA_Security__MasterKeyPath=./.marsa/master.key
MARSA_PublicBaseUrl=http://localhost:5080
MARSA_Docker__SocketPath=/var/run/docker.sock
```

Rules:

- `.marsa/`, `master.key`, local connection strings, and `compose.dev.env` are gitignored.
- A development master key is disposable; never reuse a production key.
- The development Caddy admin socket is a named volume, not the host Docker socket.
- Do not mount the real Docker socket into Caddy.
- Do not run the local server against a production database.

## 5. Compose development stack

`deploy/compose.dev.yaml` starts:

| Service | Image | Purpose |
|---|---|---|
| `postgres` | `postgres:17` | Local database |
| `caddy` | `caddy:2` | Local edge and admin socket |
| `mailpit` (optional profile) | `axllent/mailpit` | Local email capture, if email is added |

The server runs on the host with `dotnet run` so debugging and hot reload are simple. An optional `server` profile builds the server image for packaging tests.

## 6. Database workflow

```bash
dotnet ef migrations add <Name> \
  --project src/Marsa.Infrastructure \
  --startup-project src/Marsa.Server \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project src/Marsa.Infrastructure \
  --startup-project src/Marsa.Server
```

Rules:

- Migrations are generated from Infrastructure only.
- Every migration is reviewed as code.
- Do not use `EnsureCreated`.
- Do not edit a migration without updating the model.
- Integration tests create and drop their own database; they do not use the developer database.
- Reset local data with `docker compose -f deploy/compose.dev.yaml down -v` only when you intend to delete it.

## 7. Sample repositories

E2E fixtures live under `tests/fixtures/`:

| Fixture | Purpose |
|---|---|
| `aspnet-minimal` | ASP.NET Core detection and health |
| `node-express` | Node detection and port |
| `static-site` | Static build and Caddy file serving |
| `dockerfile-app` | Explicit Dockerfile |
| `bad-health` | Health failure without route change |
| `crash-loop` | Container restart detection |
| `prebuilt-image` | Image pull and digest pinning |

Fixtures are original, small, and licensed with the repository. Do not copy sample repositories from another product.

Run the full local verification gate before opening a pull request:

```bash
./eng/verify.sh
```

It restores with the lock file, builds with warnings as errors, runs all tests,
and checks the public documentation links.

## 8. Running tests

```bash
# Fast lane
dotnet test Marsa.sln -c Release --no-build

# PostgreSQL integration
dotnet test tests/Marsa.Infrastructure.Tests -c Release \
  --filter Category=RequiresPostgres

# Docker/Caddy integration (explicit opt-in)
dotnet test tests/Marsa.Infrastructure.Tests -c Release \
  --filter "Category=RequiresDocker|Category=RequiresCaddy"

# E2E on a dedicated Linux runner
dotnet test tests/Marsa.E2E.Tests -c Release
```

If Docker is unavailable, Docker/Caddy tests are skipped with a clear message, not failed as product failures. The release lane requires them.

## 9. Debugging

| Problem | Check |
|---|---|
| Server fails startup | Master key path, connection string, Caddy socket, schema version |
| Setup token missing | Server console output; Development logs |
| Caddy route not applied | Socket permissions, Caddy container health, route compiler output |
| Docker build fails | BuildKit enabled, disk space, build context exclusions |
| Database migration fails | Previous migration applied, expand/contract order, lock timeouts |
| E2E container leaks | Labels, test cleanup, previous failed run |
| Arabic layout breaks | `dir="rtl"`, logical CSS properties, LTR isolation for code/logs |

Structured logs are JSON in production and human-readable in Development. Set `Logging__LogLevel__Default=Debug` locally, never in production.

## 10. Development safety

- Use disposable local data.
- Do not point local Docker at a shared host.
- Do not test destructive actions against a real deployment.
- Do not disable auth or CSRF "just for local"; use a test fixture.
- Do not commit generated keys, tokens, database dumps, or screenshots containing secrets.
- Kill runaway containers and networks by label, not by broad `docker system prune`.
- Report a suspected secret leak immediately and rotate the value.

## 11. IDE and formatting

Recommended:

- JetBrains Rider or VS Code with C# Dev Kit.
- Enable analyzers and format on save.
- Use the repository `.editorconfig`.
- Do not reformat unrelated files in a feature pull request.

```bash
dotnet format
dotnet format --verify-no-changes
```

## 12. Container build

```bash
docker build -f deploy/server.Dockerfile -t marsa-server:dev .
```

The runtime image:

- uses the ASP.NET Core runtime image, not the SDK;
- runs as non-root UID/GID `10001`;
- has a read-only root filesystem;
- writes only to `/var/lib/marsa` and `/tmp`;
- does not contain the SDK, Git, Docker CLI, or Caddy;
- disables .NET diagnostics unless explicitly enabled for debugging.

## 13. What is not supported locally

- Windows containers.
- Kubernetes or Podman as the runtime (Podman may work unofficially; it is not a supported target).
- A remote Docker host over TCP.
- A host-installed Caddy sharing port 80 with the Compose edge.
- A production database with local test data.

## 14. Onboarding checklist

- [ ] SDK version matches `global.json`.
- [ ] Docker and Compose work.
- [ ] `dotnet restore` and `dotnet build` pass.
- [ ] Local PostgreSQL and Caddy start.
- [ ] Setup completes with a one-time token.
- [ ] A fixture deploys to a local hostname.
- [ ] Logs stream and survive refresh.
- [ ] Rollback works.
- [ ] Fast tests pass.
- [ ] PostgreSQL integration tests pass.
- [ ] No secret or local env file is staged.
