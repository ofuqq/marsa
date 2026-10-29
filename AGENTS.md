# AGENTS.md — Marsa

Marsa is a clean-room, self-hosted deployment platform for a single Linux host.
It builds and runs one service per deployment using .NET 10, PostgreSQL, Docker,
and Caddy. The brand is Ofuq and the repository is `ofuqq/marsa`.

## Build and test

- `dotnet restore --locked-mode`
- `dotnet build Marsa.sln -warnaserror`
- `dotnet test Marsa.sln`
- `./eng/verify.sh`

Contributors should read `CONTRIBUTING.md`, `docs/development/getting-started.md`,
and `docs/testing.md` before changing code.

## Code rules

- Propagate a `CancellationToken` through all asynchronous API methods.
- Read-only EF Core queries must use `AsNoTracking()` and avoid N+1 queries via projection or `Include`.
- Migrations are applied explicitly out-of-band; never auto-migrate during server startup.
- Configuration options validate at startup (`ValidateOnStart`) using the canonical `Marsa__Category__Key` shape.
- Treat compiler and analyzer warnings as errors (`-warnaserror`); keep the build clean.
- Every functional change must be covered by automated tests.
- Run `./eng/verify.sh` locally and ensure all gates pass before opening a pull request.

## Conventions

- Follow the architecture and code boundaries in `docs/architecture/`.
- Keep changes small, reversible, and covered by tests.
- Additive API changes inside `/api/v1`; no breaking changes without a decision.
- Keep secrets, SQL, stack traces, and tokens out of logs and API responses.
- Preserve the one-host, one-service, PostgreSQL-only, and no-auto-rollback invariants.

## Documentation

Start at `docs/README.md`. Architecture, data model, API, security, roadmap, and
release notes live under `docs/`. Architecture decision records are in
`docs/decisions/`.

## Tasks

Use GitHub issues for work. Pick an issue labeled `good first issue` or
`help wanted`, create a branch named `feat/<slug>` or `fix/<slug>`, and open a
pull request against `main`.
