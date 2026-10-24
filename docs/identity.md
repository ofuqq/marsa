# Marsa — Canonical Identity

> Status: Decided on 2026-10-22.
> This file is the **single source of truth** for the product name, organization handle, repository URL, and identifier prefixes. If a rename ever happens, change the `product` values in the block below first, then apply one repository-wide replacement. Do not introduce alternative spellings in other files.

## 1. Machine-readable values

```yaml
identity:
  product: Marsa
  product_ar: مرسى
  product_slug: marsa

  brand: Ofuq
  brand_ar: أفق

  github_org: ofuqq
  github_repo_slug: ofuqq/marsa
  github_repo_url: https://github.com/ofuqq/marsa.git
  default_branch: main

  dotnet_namespace: Marsa
  nuget_prefix: ofuqq.Marsa
  npm_scope: "@ofuqq"

  cli: marsa
  config_file: marsa.json
  env_prefix: MARSA_
  docker_label_prefix: io.marsa
  token_prefix: marsa_pat_

  public_domain: null
  identifier_urn_prefix: urn:marsa
```

## 2. Usage rules

| Context | Value | Rule |
|---|---|---|
| Product name in prose and UI | **Marsa** (مرسى) | The product is always “Marsa,” never “Ofuq Marsa” or “Marsa by Ofuq” in the UI. “Marsa, an Ofuq project” is allowed on first mention. |
| Parent brand, copyright, legal owner | **Ofuq** (أفق) | The brand keeps one `q`. Do not use `ofuqq` as the brand name in prose, headings, or UI copy. |
| GitHub organization, URL, remote, CODEOWNERS, badges | **ofuqq** | The handle has two `q`s. Use it only where a URL or account handle is required. |
| Repository | `ofuqq/marsa` | Remote: `https://github.com/ofuqq/marsa.git`. Default branch: `main`. |
| .NET namespaces and assemblies | `Marsa.*` | Code-facing identifiers use the product name, not the brand or handle. |
| NuGet packages | `ofuqq.Marsa.*` | Registry identifiers use the `ofuqq` handle. Umbrella package: `ofuqq.Marsa`. |
| npm packages | `@ofuqq/*` | Registry scope uses the `ofuqq` handle. Example: `@ofuqq/marsa-cli`. |
| CLI command | `marsa` | Lowercase, no prefix. |
| Repository config file | `marsa.json` | Replaces the prior analysis placeholder `ofuq.json`. |
| Environment variables | `MARSA_` | Example: `MARSA_Database__ConnectionString`, not `OFUQ_...`. |
| Docker labels | `io.marsa.*` | Ownership and guard labels. |
| API token prefix | `marsa_pat_` | Personal access tokens are recognizable in logs and secret scanners. |
| Public domain |  | No domain is owned. Use `urn:marsa:...` identifiers and relative links until one is secured. |

## 3. Why the brand and the handle differ

“Ofuq” is the brand and reads correctly in Arabic (أفق) and English. The GitHub handle `ofuqq` is an account identifier, not a second brand. The extra `q` exists only because handles are globally unique. Mixing the two is a documentation bug:

- Correct: “Marsa is an Ofuq project. The source is at `github.com/ofuqq/marsa`.”
- Incorrect: “Ofuqq Marsa” in UI copy.
- Incorrect: “Ofuq/marsa” in a URL.
- Incorrect: `ofuq.Marsa` as a NuGet ID after this decision.

## 4. Rename procedure

The product name is centralized so a rename is a deliberate, reviewable change:

1. Change `product`, `product_ar`, and `product_slug` in the YAML block above.
2. Run a repository-wide, case-sensitive replacement of the old product name, then review the diff. Identifiers that use `marsa` (CLI, config, env prefix, labels, token prefix, URNs) must be renamed with a compatibility plan; renaming a published package or environment prefix is a breaking change.
3. Update the decision log in [`../risks-and-open-questions.md`](risks-and-open-questions.md) and the ADR that records the rename.
4. Do not rename the GitHub organization, repository, or published package IDs as part of a product rename without a separate migration plan.

## 5. Open naming risk

The name **Marsa** is decided for this project, but it is **not trademark-cleared**. A prior container-deployment company used the name at `marsa.io`, and the obvious domains are registered. Trademark clearance and domain acquisition must be completed before public launch; see [`../license-and-provenance.md`](license-and-provenance.md) and the risk register in [`../architecture/security.md`](architecture/security.md). This document records the naming decision, not legal clearance.
