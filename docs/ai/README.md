# Marsa — AI agent documentation

> Status: **PROPOSED** operating guidance. Last reviewed: 2026-10-22.
> This directory is for AI coding agents and the humans supervising them. It does not replace the product specification.

## Read in this order

1. [`../../AGENTS.md`](../../AGENTS.md) — non-negotiable rules.
2. [`agent-guide.md`](agent-guide.md) — context map and operating loop.
3. [`architecture-context.md`](architecture-context.md) — invariants an agent must not break.
4. [`coding-standards.md`](coding-standards.md) — .NET, API, EF Core, UI, and error conventions.
5. [`testing-guide.md`](testing-guide.md) — suites, fixtures, and evidence.
6. [`workflows.md`](workflows.md) — issue, bug, ADR, migration, and release workflows.
7. [`known-pitfalls.md`](known-pitfalls.md) — failure modes that have bitten self-hosted deployers.
8. [`review-checklist.md`](review-checklist.md) — self-review before claiming completion.
9. [`definition-of-done.md`](definition-of-done.md) — the completion gate, including the Arabic explanation and handoff update.

## What agents are allowed to do

- Read the repository and the read-only prior analysis notes kept outside this repository.
- Edit documentation, issue drafts, templates, scripts, and future product code.
- Run local builds, tests, linters, and dry-run automation.
- Ask the user only when genuinely blocked; otherwise choose the smallest option consistent with the PRD and mark it **PROPOSED**.

## What agents must not do

- Push, create issues, open PRs, publish releases, or change repository settings.
- Print or commit secrets, tokens, private URLs, or customer data.
- Commit, push, expose, or publicly link `.local/` progress, Arabic learning, handoff, prompt, or notes files.
- Copy Openship or third-party code, prose, schemas, tests, or assets.
- Invent a public domain or a final legal decision.
- Change an approved requirement without an explicit decision record.
- Claim a command passed if it was not run.

## Repository state

The current phase, decisions, and next action may be supplied in the owner's private handoff. Treat it as context, not as a public contract: never commit, quote, or link it from public docs. If no handoff is available, start from the active issue and the public decision log.

## Configuration changes

After editing `opencode.json` or adding a project skill, quit and restart opencode. A running session keeps its previously loaded configuration and skill list. Verify with `opencode debug config` and `opencode debug skill`.
