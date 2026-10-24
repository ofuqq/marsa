# Marsa

[![CI](https://github.com/ofuqq/marsa/actions/workflows/ci.yml/badge.svg)](https://github.com/ofuqq/marsa/actions/workflows/ci.yml)

**Marsa** is a self-hosted deployment platform for a single Linux host. It builds
and runs one service per deployment using .NET 10, PostgreSQL, Docker, and Caddy.

Status: early design stage. The repository currently contains the specification,
architecture, backlog, and project setup. There is no released product, server
image, CLI, or installer yet.

## Why Marsa

Marsa is for small teams and solo developers who want the benefits of a
deployment platform without running a cluster. It favors a clear mental model
over breadth: one host, one service per deployment, explicit health gates, and
a deployment history you can inspect.

## Planned 0.1 capabilities

- Local authentication, roles, and personal access tokens.
- Projects and source connections for GitHub, GitLab, and generic HTTPS Git.
- Deterministic detection for ASP.NET Core, Node/Next.js, static sites, Dockerfiles,
  and prebuilt images.
- Builds and immutable releases with redacted logs.
- Runtime containers with memory and CPU limits.
- Health-gated cutover through Caddy, with the previous release kept available
  on failure.
- Domains, TLS, secrets, audit, rollback, a CLI, and an Arabic-localized dashboard.
- Install, upgrade, backup, restore, diagnostics, and release gates.

## How it works

```text
git push
  -> webhook
  -> build an image and create a release
  -> start the new release on an isolated port
  -> run the health check
  -> move traffic through Caddy if healthy
  -> keep the previous release for rollback
```

## Technology stack

- .NET 10 / ASP.NET Core
- PostgreSQL 17
- Docker Engine with BuildKit
- Caddy 2.x
- Razor Components for the dashboard
- OpenAPI 3.1 for the versioned API

## Documentation

- [Documentation index](docs/README.md)
- [Vision](docs/vision.md)
- [Product requirements](docs/product-requirements.md)
- [Architecture overview](docs/architecture/overview.md)
- [Security model](docs/architecture/security.md)
- [Roadmap](docs/roadmap.md)
- [License and provenance](docs/license-and-provenance.md)

## Contributing

Contributions are welcome. Start with issues labeled `good first issue` or
`help wanted`. See [CONTRIBUTING.md](CONTRIBUTING.md).

## Security reporting

Please report vulnerabilities privately using GitHub's private vulnerability
reporting. See [SECURITY.md](SECURITY.md).

## License

Proposed for Apache License 2.0. The final license is subject to maintainer
and legal review. See [LICENSE](LICENSE).
