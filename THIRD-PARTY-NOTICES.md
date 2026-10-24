# Third-Party Notices

> Status: initial inventory. Last updated: 2026-10-22.

## Current repository

This repository currently contains **documentation and project metadata only**. It does not contain a Marsa server image, CLI binary, installer, or vendored third-party source code. No third-party code or assets are bundled in the current commit.

Tools used to review or validate the documentation are not vendored and are not distributed as part of Marsa.

## Future release artifacts

Before any public release, the release owner must:

1. Generate an inventory of NuGet packages, including transitive dependencies.
2. Generate an SBOM for each container image.
3. Record the license of every bundled dependency and base image.
4. Include required copyright and attribution notices here.
5. Verify that no GPL, AGPL, SSPL, BUSL, Commons Clause, or unknown-license component is in the default distribution without an approved exception.
6. Include the notices in the release artifact and repository.

The policy and blocked-license list are in [`docs/license-and-provenance.md`](docs/license-and-provenance.md). The first release gate is tracked by issue `m6-03-supply-chain-and-release`.

## Notices

No bundled third-party notices are required for the current documentation-only repository. This file must not be interpreted as a generated SBOM or as proof that a future dependency inventory is complete.
