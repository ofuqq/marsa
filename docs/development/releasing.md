# Marsa — Release and Versioning

> Status: Proposed for 0.1. Last reviewed: 2026-10-22.
> Release engineering is part of the product promise: a self-hosted operator must be able to install, upgrade, and recover without guessing.

## 1. Versioning

Marsa uses **Semantic Versioning**.

| Change | Version |
|---|---|
| Incompatible public behavior or configuration | Major |
| New backward-compatible capability | Minor |
| Backward-compatible fix | Patch |

- The first public release is `0.1.0`.
- Pre-1.0 breaking changes are allowed with a migration note and a minor version bump.
- Build metadata may include the commit SHA, e.g. `0.1.0+abc1234`.
- The server exposes its version and commit at `/api/v1/system/version`.
- Container images carry `org.opencontainers.image.version`, `revision`, and `created` labels.

### 1.1 API versioning

- Public API base path: `/api/v1`.
- Additive fields are allowed within v1.
- Removing a field, narrowing a type, changing an enum value, or adding a required request field is breaking.
- A breaking API change requires `/api/v2` and at least one minor release of overlap.
- Unknown response fields must be ignored by clients.
- The CLI and server publish a compatibility matrix.

### 1.2 Configuration and snapshot versions

- `marsa.json` is versioned; version 1 rejects unknown fields.
- Deployment snapshots carry `snapshotSchemaVersion`.
- Plan documents carry `planSchemaVersion`.
- A new schema version is required for incompatible changes.
- Older snapshots remain readable for the retention window.

## 2. Release artifacts

| Artifact | Contents |
|---|---|
| Server image | `ghcr.io/ofuqq/marsa-server:<version>` for `linux/amd64` and `linux/arm64`, pinned by digest |
| CLI binaries | Linux and macOS, amd64 and arm64, plus checksums |
| Migration bundle | Self-contained EF Core migration executable |
| Compose template | `compose.yaml` with pinned image digests |
| Caddy template | Static control route and admin socket policy |
| systemd unit | Optional service wrapper for the Compose stack |
| Install script | Preflight, secrets, compose start, setup URL |
| Upgrade script | Backup reminder, migration, rollback notes |
| Backup/restore scripts | PostgreSQL dump/restore and master-key guidance |
| SBOM | SPDX or CycloneDX for source and images |
| Checksums | SHA-256 for every downloadable artifact |
| Release notes | Changes, breaking changes, upgrade steps, known issues |

Artifacts are built from the tagged commit in CI. No local-only build is a release.

## 3. Supported platforms

| Component | Supported |
|---|---|
| Control-plane host | Linux x86_64, Linux arm64 |
| Docker | Engine 27+ with BuildKit |
| PostgreSQL | 17 |
| Caddy | 2.x, official image pinned by digest |
| Browsers | Current and previous major Chrome, Firefox, Safari, Edge |
| CLI | Linux x86_64/arm64, macOS x86_64/arm64 |
| Windows server | Not supported in 0.1 |
| Rootless Docker | Best-effort, not a launch gate |
| External reverse proxy | Not supported in the default topology |

## 4. Install

The supported install is:

1. Prepare a Linux host with Docker and Compose.
2. Download the release bundle and verify checksums.
3. Copy `compose.env.example` to `compose.env` and set the public base URL.
4. Generate a master key and store it outside the database.
5. Run `install.sh` or `docker compose up -d`.
6. Open the setup URL and use the one-time token from the install output.
7. Create the owner and organization.
8. Deploy a fixture or connect a repository.

The installer must not:

- enable a default password;
- expose PostgreSQL or the server port publicly;
- print the master key into persistent logs;
- start with telemetry enabled;
- overwrite an existing master key;
- run migrations automatically without a clear upgrade path.

## 5. Upgrade

### 5.1 Standard upgrade

1. Read the release notes and compatibility notes.
2. Back up the database and master key separately.
3. Pull the new image by digest.
4. Run the migration bundle.
5. Start the new server version.
6. Check readiness and diagnostics.
7. Complete a smoke deploy and rollback.
8. Keep the previous image and a database snapshot until the new version is accepted.

### 5.2 Migration policy

- Expand/contract only.
- A new binary must run against the previous schema during rollout.
- Column drops happen in a later release.
- Automatic migration on web startup is forbidden.
- The server refuses to serve if the database schema is newer than the binary.
- A failed migration stops the upgrade; the previous version remains runnable if the schema is backward-compatible.
- Downgrade requires a database restore; it is documented, not automated.

### 5.3 Self-upgrade

Marsa does not deploy its own control-plane project. Upgrading Marsa is an operator action through the install/upgrade scripts, not a Marsa deployment.

The previous server image and a compatible schema snapshot are the rollback path. If the schema has advanced beyond the previous binary's compatibility, restore the database snapshot.

## 6. Backup and restore

### 6.1 What to back up

- PostgreSQL database.
- Master key, separately and securely.
- Caddy data volume (certificates) or willingness to re-issue.
- `compose.env` without secret values committed to the repository.

### 6.2 Backup

```bash
pg_dump --format=custom --file=marsa-$(date -u +%Y%m%dT%H%M%SZ).dump "$MARSA_DATABASE_URL"
```

- Encrypt backups at rest.
- Store them outside the host.
- Record the schema version and application version with each backup.
- Test restore before release, not only after an incident.

### 6.3 Restore

1. Stop the server.
2. Restore the database dump.
3. Restore the matching master key.
4. Start the matching server version.
5. Check readiness, deployments, secrets, and routes.
6. Reconcile Caddy if certificates are not restored.

Without the master key, encrypted secrets cannot be recovered. This must be stated in the install output and the restore runbook.

## 7. Rollback of Marsa itself

| Scenario | Action |
|---|---|
| New version fails before migration | Start the previous image |
| Migration succeeded, previous binary compatible | Start the previous image |
| Migration is not backward-compatible | Restore the database snapshot, then start the previous image |
| Bad release artifact | Pull the previous digest and redeploy |

A rollback is not complete until readiness, login, deployment history, and a smoke deploy pass.

## 8. Release channels

| Channel | Purpose | Stability |
|---|---|---|
| `nightly` | Automated main builds | Unstable; no support |
| `rc` | Release candidate | Feature-frozen; migration and E2E tested |
| `latest` | Current stable | Supported |
| `0.1.x` | Patch line | Security and critical fixes |

Tags are immutable. A published tag is never moved to a different commit. If a release is wrong, publish a new patch.

## 9. Release checklist

### Code and tests

- [ ] All CI gates pass on the release commit.
- [ ] Unit, PostgreSQL integration, contract, and security tests pass.
- [ ] E2E passes on a clean Linux runner.
- [ ] Worker kill/lease recovery passes.
- [ ] Failed health/route never changes the active route.
- [ ] Cross-tenant and secret-canary tests pass.
- [ ] OpenAPI diff is reviewed and additive.

### Packaging

- [ ] Multi-arch images build and are pinned by digest.
- [ ] CLI binaries and checksums are produced.
- [ ] Migration bundle builds and runs.
- [ ] Compose and Caddy templates match the release.
- [ ] SBOM and third-party notices are attached.
- [ ] No GPL/unknown component in the default distribution.
- [ ] No SDK, Git, Docker CLI, or Caddy binary in the runtime image.

### Operations

- [ ] Clean-host install passes.
- [ ] Upgrade from the previous release passes.
- [ ] Backup/restore drill passes with the master key.
- [ ] Install, upgrade, backup, restore, rotate-key, troubleshoot, and uninstall runbooks are updated.
- [ ] A person who did not write the feature followed the runbook.
- [ ] Disk-pressure and Docker/Caddy/DB-down behavior is documented.
- [ ] The previous image and schema compatibility are recorded.

### Security and legal

- [ ] Security review checklist signed off.
- [ ] No unapproved High/Critical vulnerability.
- [ ] License inventory reviewed.
- [ ] Clean-room evidence reviewed.
- [ ] No secrets or internal URLs in release notes.

### Communication

- [ ] Release notes list breaking changes and upgrade steps.
- [ ] Known issues are documented.
- [ ] The version is visible in the UI and `/system/version`.
- [ ] No promise of HA or multi-host support.

## 10. Support envelope

For 0.1:

- The current minor release and the previous minor release receive security fixes.
- The current patch line receives critical fixes.
- Pre-release channels are unsupported.
- Self-modified images and unsupported reverse proxies are not supported.
- Upgrades skipping multiple minors require sequential upgrades or a restore.

## 11. Changelog rules

- Keep `CHANGELOG.md` in the repository.
- Group by Added, Changed, Deprecated, Removed, Fixed, Security.
- Link the release tag and relevant issues.
- State migration steps and breaking changes explicitly.
- Never claim a fix that was not verified.
- Credit external contributors.

## 12. Release notes template

```markdown
# Marsa 0.1.0

## Highlights
- ...

## Breaking changes
- None / ...

## Upgrade
1. Back up the database and master key.
2. Run the migration bundle.
3. Start the new image by digest.
4. Verify readiness and run a smoke deploy.

## Fixes
- ...

## Known issues
- ...

## Verification
- Commit:
- Image digest:
- E2E run:
- SBOM:
```
