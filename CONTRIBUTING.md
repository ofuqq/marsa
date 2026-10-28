# Contributing to Marsa

Thanks for considering a contribution. Marsa is currently a **documentation and design repository**; the product has not been implemented yet. Contributions that improve the specification, security model, backlog, or Arabic material are as valuable as code will be later.

By participating, you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## 1. Before you start

Read:

1. [`AGENTS.md`](AGENTS.md) — repository-wide rules for contributors.
2. [`docs/vision.md`](docs/vision.md) — intent and non-goals.
3. [`docs/identity.md`](docs/identity.md) — canonical names and identifiers.
4. [`docs/roadmap.md`](docs/roadmap.md) — construction order and MVP boundary.
5. [`https://github.com/ofuqq/marsa/issuesREADME.md`](https://github.com/ofuqq/marsa/issuesREADME.md) — issue format and dependencies.

Check whether an existing issue covers the change. For a large design change, open a proposal or an ADR before writing a large patch.

Automated labeling maps changed paths to area labels. When a pull request references an issue with "Closes #N" or "Fixes #N", automation copies that issue's labels and milestone to the pull request.

## 2. Current contribution workflow

1. Fork the repository or create a focused branch.
2. Make one coherent change.
3. Run the documentation checks:

   ```bash
   python3 scripts/check-docs.py
   python3 scripts/validate-backlog.py
   ./scripts/sync-labels.sh --dry-run
   ./scripts/create-issues.sh --dry-run
   ```

4. Review the diff for secrets, broken links, naming drift, and copied prose.
5. Commit with DCO sign-off:

   ```bash
   git commit -s -m "docs: explain the rollback boundary"
   ```

6. Open a pull request that states what changed, why, and what you verified.

Do **not** run `./scripts/sync-labels.sh --apply` or `./scripts/create-issues.sh --apply` unless you are the repository owner and intend to change GitHub. The scripts default to dry-run for a reason.

## 3. Contribution license and provenance

- First-party contributions are accepted under the repository's outbound license, intended to be Apache-2.0. The outbound license is still Proposed; see [`docs/license-and-provenance.md`](docs/license-and-provenance.md).
- Sign off every commit with the Developer Certificate of Origin (DCO). A separate CLA is not required for 0.1.
- Confirm that you wrote the contribution or have permission to submit it.
- Do not submit code, prose, tests, schemas, migrations, assets, or UI copy translated or copied from Openship or another project.
- Do not add a dependency without recording its license and checking it against the dependency policy.
- Do not submit an asset without a clear license and attribution.

If provenance is unclear, maintainers will reject the contribution until it is resolved. That is not a judgment about intent; it protects the project.

## 4. Naming rules

`docs/identity.md` is the single source of truth. In summary:

| Context | Use |
|---|---|
| Product in prose and UI | **Marsa** |
| Parent brand and copyright | **Ofuq** |
| GitHub organization, URL, remotes, CODEOWNERS, badges | **`ofuqq`** |
| Repository | `ofuqq/marsa` |
| Namespaces and assemblies | `Marsa.*` |
| NuGet | `ofuqq.Marsa.*` |
| npm | `@ofuqq/*` |
| CLI, config, environment, labels, tokens | `marsa`, `marsa.json`, `Marsa__`, `io.marsa.*`, `marsa_pat_` |

Do not use `ofuq/marsa`, `Ofuq.Marsa.*`, `@marsa/*`, or “Ofuq Marsa” as a product name. Do not invent a public Marsa domain. Use relative links and `urn:marsa:*` identifiers until a domain is secured.

## 5. Design and architecture changes

- The architecture is in [`docs/architecture/overview.md`](docs/architecture/overview.md); significant changes require an ADR in [`docs/decisions/`](docs/decisions/README.md).
- The deployment state machine, health gate, trust boundaries, and single-host scope are not casual implementation details. Changing them needs an explicit decision and updates to the requirements, tests, and docs.
- The single-service and PostgreSQL-only constraints are deliberate. Adding a stateful dependency requires an ADR and an operating-cost analysis.
- A new public API breaks compatibility only through `/api/v2` and a documented deprecation path.

## 6. Documentation style

- Write in your own words. Prefer concrete behavior over marketing language.
- Keep links relative unless a public URL is genuinely owned and stable.
- Add every new public document to [`docs/README.md`](docs/README.md).
- Update the public decision log in [`docs/risks-and-open-questions.md`](docs/risks-and-open-questions.md) when an important decision changes. Do not commit the owner's private progress log.
- Use stable requirement IDs (`FR-*`, `NFR-*`), ADR numbers, and issue IDs when referring to decisions.
- Do not claim a test, benchmark, SBOM, or release exists unless it was produced and reviewed.
- Do not use a screenshot or copied paragraph from another product.

### Arabic material

- Use Modern Standard Arabic and explain a technical term on first use.
- Keep code, logs, and technical identifiers LTR inside RTL pages.
- Check RTL layout at small and large widths; do not treat translation as a substitute for layout review.
- Arabic copy should read naturally, not as a word-for-word translation of English.
- The owner's Arabic explainer, glossary, and learning notes are private; do not commit or link them from public docs.

## 7. Security, privacy, and the local workspace

- Never commit passwords, tokens, private keys, database dumps, private repository URLs, or customer data.
- Never commit progress logs, personal notes, prompts, costs, or `.env` files.
- The owner's `/` directory is excluded through `.git/info/exclude`, not `.gitignore`. Do not force-add it or expose its filenames.
- Public docs must never link to `/`.
- Before any push, review `git status` and list anything that looks personal. Stop; do not push it.
- Do not test destructive operations against a real deployment.
- Do not paste secret values into an issue or pull request. If a secret leaks, rotate it first and then report the exposure.
- Report vulnerabilities privately according to [`SECURITY.md`](SECURITY.md).
- Treat Docker socket access, host mounts, and privileged containers as root-equivalent. Do not weaken the guardrails to make a test pass.

## 8. When code exists

The future development environment is documented in [`docs/development/getting-started.md`](docs/development/getting-started.md). The expected baseline is:

```bash
dotnet restore --locked-mode
dotnet build Marsa.sln -c Release
dotnet test Marsa.sln -c Release
dotnet format --verify-no-changes
```

Also run the relevant PostgreSQL, Docker, Caddy, and E2E lanes described in [`docs/development/testing.md`](docs/development/testing.md). A change is not complete because it compiles; it needs the failure paths and the evidence described in `CONTRIBUTING.mdreview-checklist.md`.

For every code change, write the beginner Arabic explanation at `/learn/NNN-<issue-or-topic>.ar.md` before calling the work done. It is local only, must not be committed or linked from public docs, and must include the explanation, file walkthrough, run/test commands, failure modes, three quiz questions with answers, and next reading. See `CONTRIBUTING.mddefinition-of-done.md`.

## 9. Review and merge

Maintainers review for:

- correctness against the requirements and ADRs;
- security and tenant isolation;
- failure behavior and recovery;
- tests that fail without the change;
- naming and documentation consistency;
- Arabic/RTL quality for user-facing changes;
- license and clean-room provenance.

A maintainer may ask for a smaller change. That is usually about keeping the design legible, not about rejecting the idea.

## 10. Reporting a problem

- Bugs and design gaps: use the issue templates.
- Security vulnerabilities: follow [`SECURITY.md`](SECURITY.md); do not open a public issue with exploit details.
- Conduct concerns: follow [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).
