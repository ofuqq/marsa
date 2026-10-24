# Marsa — License and Clean-Room Process

> Status: Proposed for 0.1. Last reviewed: 2026-10-22.
> This document is not legal advice. It records the project's policy, the known provenance boundaries, and the evidence needed before distribution.

## 1. Outbound license

**Proposed:** first-party code and documentation are licensed under **Apache License 2.0**.

Why:

- It includes an express patent grant.
- It is familiar to .NET and self-hosted users.
- It permits commercial and private use, modification, and redistribution.
- It is compatible with the permissive dependencies expected in the stack.

Alternatives rejected:

| Alternative | Why not now |
|---|---|
| MIT | Simpler, but no express patent grant and less explicit contribution terms |
| AGPL | Conflicts with the goal of broad self-hosted adoption; imposes network-copyleft obligations |
| BSL/SSPL | Not open source under OSI terms; scares contributors and distributors |
| Dual license | Adds legal and contribution complexity before there is a commercial reason |
| CC BY 4.0 for docs | Friendlier for prose, but a second license to maintain; reconsider if docs are published separately |

`LICENSE` contains the full Apache-2.0 text. `NOTICE` records copyright and any required attributions. Contributions use the Developer Certificate of Origin (DCO) unless a CLA is explicitly adopted later.

## 2. What the product may learn from

Marsa is a clean-room design informed by publicly observable behavior and prior analysis of Openship. The analysis folder is reference material, not a source of code.

Allowed:

- Public facts, product behavior, protocols, standards, and documented interfaces.
- Independent reimplementation of an idea in original code and prose.
- Permissively licensed dependencies with notices.
- User-provided content processed at runtime, not redistributed.
- Comparisons that cite public sources.

Not allowed:

- Copying source files, comments, docstrings, tests, migrations, schemas, DTOs, error strings, UI copy, or assets.
- Mechanical translation from TypeScript or another language into C#.
- Copying Openship's visual design, logos, screenshots, or marketing text.
- Using the Openship name or logo as Marsa branding.
- Bundling GPL, AGPL, SSPL, BUSL, Commons Clause, or unknown-license code in the default distribution.
- Relying on a repository-level license as proof that every bundled file is permissively licensed.

## 3. Known provenance boundary

The prior analysis found:

- The Openship root and authored packages identify Apache-2.0, but root metadata does not automatically license every bundled file.
- Several packages have no explicit `license` field; missing metadata is not permission.
- The mail engine includes a GPLv3 boundary. Marsa does not include a mail engine in 0.1.
- Some mail-adjacent code had unresolved provenance at the time of analysis.
- Bundled icons and logos have their own permissive licenses and attribution requirements.

For Marsa:

- No mail engine, mail server, or iRedMail-derived material is in the default distribution.
- No Zero Email code is used.
- No Openship source, test, migration, schema, DTO, or asset is copied.
- The product does not claim to be a fork, port, or derivative of Openship.
- Behavioral comparisons are attributed in documentation; they do not copy expression.

## 4. Source-informed clean-room review

This is not a formal double-blind clean room. One analyst has read the Openship source. The process is honest about that and relies on evidence and independent expression.

### Stage A — Evidence extraction

- Record behavioral facts with file paths and line ranges in the prior analysis.
- Record what the fact means, not code to reuse.
- Do not copy code, comments, schemas, or tests into the Marsa repository.
- Mark uncertain claims as uncertain.

### Stage B — Independent design

- Design from Marsa requirements, the public behavior record, and general .NET/PostgreSQL/Caddy knowledge.
- Do not open the Openship source while writing Marsa design prose or code.
- Record the chosen design, rejected alternatives, and invariants.
- Use original names, folder structures, API paths, error codes, and UI copy.

### Stage C — Implementation and review

- Implement from Marsa design documents, not from an Openship editor tab.
- Do not translate line by line or preserve code structure.
- Do not port tests or fixtures.
- Review for structural similarity, not only literal copying.
- Keep the evidence ledger current.
- If a behavior cannot be expressed without copying expression, use a public standard or write a new design.

### 4.1 Evidence ledger

Every non-obvious borrowed fact gets a row:

| Claim | Evidence | Use in Marsa | Expression boundary |
|---|---|---|---|
| Behavior or constraint | Analysis path + line range | Requirement/ADR | What was not copied |

The ledger lives with the analysis or in `CONTRIBUTING.md` and is reviewed before release. It is not a license to copy.

### 4.2 Reviewer questions

- Can the behavior be explained without pointing to an Openship file?
- Are the data model names, relationships, and constraints independently chosen?
- Are API paths, payloads, error codes, and UI strings original?
- Is any dependency GPL, unknown-license, or unmaintained?
- Does the implementation resemble the source structurally?
- Is every third-party notice present?

## 5. Dependency policy

### 5.1 Allowed by default

| License | Status |
|---|---|
| MIT | Allowed with notice |
| Apache-2.0 | Allowed with notice |
| BSD-2-Clause / BSD-3-Clause | Allowed with notice |
| ISC | Allowed with notice |
| PostgreSQL | Allowed with notice |
| CC0 / public domain | Allowed with provenance |
| MS-PL | Case-by-case review |

### 5.2 Review required

- LGPL (dynamic linking claims are not assumed).
- MPL-2.0 (file-level copyleft; review modifications).
- EPL-2.0.
- Custom or source-available licenses.
- Any package with no license metadata.

### 5.3 Blocked by default

- GPL-2.0, GPL-3.0, AGPL-3.0.
- SSPL, BUSL, Elastic License, Commons Clause.
- Non-commercial or field-of-use restrictions.
- Unknown or missing license.
- Any license that would force Marsa's default distribution to adopt incompatible terms.

A blocked dependency requires a written exception with owner, expiry, distribution impact, compensating control, and legal review.

### 5.4 Special cases

- **Git CLI:** GPL-2.0. Use only as a separate process in a builder image, never linked into Marsa. If the builder image is distributed, comply with source-offer and notice obligations. `LibGit2Sharp` is deliberately avoided in 0.1.
- **Base images:** .NET, PostgreSQL, and Caddy images carry their own licenses and notices. Pin by digest and include them in the SBOM.
- **Fonts and icons:** use only permissively licensed assets; include attribution. Do not copy Openship or vendor logos.
- **Documentation examples:** original; no copied README prose or screenshots.

## 6. SBOM and notices

Release artifacts include:

- NuGet inventory including transitive packages.
- Container image SBOM in SPDX or CycloneDX.
- License summary with allowed/review/blocked status.
- `THIRD-PARTY-NOTICES.md`.
- Source revision and build provenance.
- SHA-256 checksums.

Suggested commands (tool availability is not assumed):

```bash
dotnet restore --locked-mode
dotnet list package --include-transitive
syft dir:. -o spdx-json > sbom/source.spdx.json
syft <image> -o spdx-json > sbom/server.spdx.json
trivy fs --scanners vuln,license --exit-code 1 .
trivy image --scanners vuln --exit-code 1 <image>
gitleaks detect --no-git
```

If a tool is unavailable, the release is blocked until an equivalent reviewed inventory is produced. Do not claim an SBOM exists when it was not generated.

## 7. CI policy

Fail the build when:

- a dependency has no license;
- a blocked license appears without an approved exception;
- an exception is expired;
- an SBOM cannot be generated;
- the notices file is stale;
- a runtime image contains build-only tools;
- a secret scan finds a credential;
- an unapproved High/Critical vulnerability is present.

Exceptions are recorded in `docs/license-exceptions.md` or an equivalent reviewed file with package/version, license, need, alternative analysis, distribution impact, control, owner, and expiry.

## 8. Contribution policy

- Contributions are accepted under Apache-2.0.
- Contributors sign off commits with DCO (`Signed-off-by`).
- A CLA is not required for 0.1.
- Contributors confirm they wrote the contribution or have permission to submit it.
- Do not submit code generated by translating another project.
- Do not submit assets without a license.
- Maintainers may reject a contribution with unclear provenance.
- Security reports follow `SECURITY.md`.

## 9. Release checklist

- [ ] `LICENSE` is Apache-2.0 and included in the image/repository.
- [ ] `NOTICE` is present and accurate.
- [ ] `THIRD-PARTY-NOTICES.md` is generated and reviewed.
- [ ] No iRedMail, mail-engine, GPL, AGPL, or unknown-license file is in the default distribution.
- [ ] No Openship source, tests, migrations, schemas, DTOs, assets, or marketing copy are present.
- [ ] API paths, schemas, error codes, and UI copy are original.
- [ ] Git CLI is absent from the runtime image or its obligations are handled.
- [ ] Base-image notices are included.
- [ ] SBOM is attached.
- [ ] License inventory has no unexpired exception.
- [ ] Clean-room reviewer signs off that no code was translated.
- [ ] Legal review is complete before public distribution.

## 10. Honest limitations

- This is a source-informed process, not a formal clean room.
- The same analyst read Openship and writes Marsa; independent review is required.
- A license inventory is a snapshot, not a legal opinion.
- Public behavior can be patented or trademarked independently of copyright.
- The Marsa name is not trademark-cleared; see `00-identity.md`.
- The final outbound license remains Proposed until the owner and legal review approve it.

## 11. Related documents

- [`../identity.md`](identity.md) — naming and trademark risk.
- [`../risks-and-open-questions.md`](risks-and-open-questions.md) — legal risks and decisions.
- [`../development/releasing.md`](development/releasing.md) — release artifacts and gates.
- [`../development/testing.md`](development/testing.md) — verification evidence.
