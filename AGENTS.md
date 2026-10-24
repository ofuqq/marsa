# AGENTS.md — Marsa

Marsa is a clean-room, self-hosted deployment platform for a single Linux host.
It builds and runs one service per deployment using .NET 10, PostgreSQL, Docker,
and Caddy. The brand is Ofuq and the repository is `ofuqq/marsa`.

## Build and test

The .NET solution is not created yet. The first implementation issue is
`M0: Create the .NET solution skeleton` (see `docs/backlog/issues/`). Once code
lands, use:

- `dotnet restore --locked-mode`
- `dotnet build Marsa.sln -warnaserror`
- `dotnet test Marsa.sln`
- `./eng/verify.sh`

Contributors should read `CONTRIBUTING.md`, `docs/development/getting-started.md`,
and `docs/testing.md` before changing code.

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
