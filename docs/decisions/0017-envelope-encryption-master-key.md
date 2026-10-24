# ADR 0017 — AES-GCM envelope encryption with a master key

- Status: Proposed
- Date: 2026-10-22
- Supersedes: none
- Superseded by: none

## Context

Marsa stores environment secrets, source credentials, and webhook secrets. They must be encrypted at rest, write-only through the API, and recoverable without a cloud KMS. The host is self-managed.

## Decision

Use **AES-256-GCM envelope encryption** with a master key outside the database and image.

- Envelope contains key version, nonce, ciphertext, and tag.
- Master key from a root-owned file or environment variable.
- Key version recorded on every ciphertext.
- Rotation creates a new key version and re-encrypts lazily.
- Old versions remain until no deployment needs them.
- Missing key fails closed for secret-dependent operations.
- Secret versions are immutable; snapshots carry references, not plaintext.
- Backups of the database require a separate master-key backup.

## Consequences

### Positive

- No cloud dependency.
- Authenticated encryption with a standard primitive.
- Clear key rotation and backup story.
- Works offline.

### Negative

- Key loss is unrecoverable.
- The operator must protect the key file.
- Rotation requires careful re-encryption.
- No HSM or central audit of key use.

### Follow-ups

- Document key backup and rotation runbooks.
- Add canary tests for plaintext leakage.
- Evaluate KMS/HSM for regulated deployments.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| ASP.NET Data Protection keyring in the database | Key and ciphertext in the same store |
| KMS/HSM | Cloud dependency and setup burden for 0.1 |
| Plaintext with file permissions | Fails the encryption requirement |
| Per-secret keys | More key management, no clear benefit |

## References

- `07-security.md`
- `05-data-model.md`
