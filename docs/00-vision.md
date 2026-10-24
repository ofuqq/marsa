# Marsa Vision

> Status: **DECIDED — Phase 1 confirmed**
> Date: 2026-10-22
> Owner: Ofuq
> Product name: **Marsa** (مرسى)
> Tagline: **Where your code docks.**
> Canonical names, handles, and identifiers: [`00-identity.md`](00-identity.md)
> Prior analysis: a separate read-only analysis bundle, especially architecture, roadmap, and license notes. Links are intentionally omitted because that bundle is local to the maintainer and is not part of this repository.

**Legend.** **VERIFIED** = checked in a public registry or document on the date shown. **INFERRED** = a conclusion drawn from the prior analysis. **PROPOSED** = a Marsa decision that is not confirmed yet. **UNKNOWN** = not checked or not knowable from here.

**How this document relates to the rest of the repository.** This file states why Marsa should exist and what it refuses to become. The requirements are in [`01-product-requirements.md`](01-product-requirements.md), the competitive reasoning is in [`02-differentiation.md`](02-differentiation.md), and the technical design is in [`03-architecture.md`](03-architecture.md). If this file and a later file disagree, this file wins on intent and the later file wins on implementation detail only after an ADR records the change.

---

## 1. The name

### 1.1 Why “Marsa”

**Marsa** (مرسى) means an anchorage or mooring place: the calm water where a ship ties up. It is short, pronounceable in Arabic and English, and it describes the product without a metaphor that needs explaining.

The product is the harbor. The applications are the boats. The operator is the harbor master, not a person who edits Nginx files at midnight.

Naming is governed by one canonical file: [`00-identity.md`](00-identity.md). In summary:

| Thing | Value | Rule |
|---|---|---|
| Brand and organization | **Ofuq** (أفق) | The parent brand and copyright holder, spelled with one `q`. |
| Product | **Marsa** (مرسى) | The deployment platform. |
| GitHub handle, URLs, remotes, CODEOWNERS, badges | **`ofuqq`** | Two `q`s. Used only where an account handle or URL is required. |
| Repository | `ofuqq/marsa` | `https://github.com/ofuqq/marsa.git`; default branch `main`. |
| .NET namespaces and assemblies | `Marsa.*` | Code-facing identifiers use the product name. |
| NuGet family | `ofuqq.Marsa.*` | Registry identifiers use the `ofuqq` handle; umbrella package `ofuqq.Marsa`. |
| npm scope | `@ofuqq/*` | Registry identifiers use the `ofuqq` handle. |
| CLI, config, environment, labels, tokens | `marsa`, `marsa.json`, `MARSA_`, `io.marsa.*`, `marsa_pat_` | Product-derived identifiers. |
| Arabic tagline | **حيث ترسو تطبيقاتك** | “Where your applications dock.” Review with a native copy editor. |

“Ofuq” is the organization, not the product. Documentation should say “Marsa, an Ofuq project” on first mention and then “Marsa.” Do not call the product “Ofuq Marsa” in the UI; do not call the organization “Marsa.” The extra `q` in `ofuqq` is an account handle, not a second brand.

### 1.2 Availability checks — what was verified

These checks were performed on **2026-10-22** with public, read-only HTTP requests, DNS lookups, WHOIS, and RDAP. They are not reservations and they are not legal advice.

| Target | Result | Evidence | Honest interpretation |
|---|---|---|---|
| GitHub organization `ofuqq` | **Confirmed by owner; HTTP 200** | `api.github.com/orgs/ofuqq` | The chosen handle has two `q`s. The earlier check of `ofuq` was only a free-handle observation and is not the project handle. |
| GitHub repository `ofuqq/marsa` | **Confirmed by owner; HTTP 200** | `api.github.com/repos/ofuqq/marsa`; remote `https://github.com/ofuqq/marsa.git` | The repository exists and is the project remote. Default branch is `main`. |
| NuGet `ofuqq.Marsa` | **No package found** (HTTP 404) | `api.nuget.org/v3-flatcontainer/ofuqq.marsa/index.json` | Appears available. Actual publish was not attempted; the `ofuqq` prefix is not reserved. |
| npm `@ofuqq/marsa` | **No package found** (HTTP 404) | `registry.npmjs.org/@ofuqq%2Fmarsa` | Appears available. npm scope policy and publish behavior were not tested. |
| `marsa.dev` | **Registered** | WHOIS/RDAP: created 2021-03-04, expires 2027-03-04; DNS points to Vercel | Not available at normal registration. Could be purchased from the owner; price UNKNOWN. |
| `marsa.io` | **Registered** | WHOIS: created 2022-07-26, expires 2027-07-26; Cloudflare nameservers; domain does not currently resolve | Not available. |
| `marsa.app` | **Registered** | RDAP: created 2019-09-22, expires 2027-09-22; Cloudflare | Not available. |
| Public name usage | **A prior use in the same category exists** | 1,972 GitHub repositories match “marsa” in the name, mostly `Mars*`. A third-party directory (Tracxn) lists a Jordan-based **Marsa** at `marsa.io`, described as a “cloud based container automated deployment solution provider,” founded 2020 and marked deadpooled. `marsa.io` did not resolve in a DNS/HTTP check on 2026-10-22. Other “Marsa” software and services companies exist (for example Marsa Software Solutions in Canada/Dubai, MARSA AI). | This is a real brand-collision risk in the exact product category, even if the old product is inactive. The owner accepted that risk on 2026-10-22; it does not change the naming decision. Trademark clearance is **UNKNOWN** and required before public launch. |

**What could not be verified:** NuGet prefix ownership; whether npm will permit the scope at publish time; trademark availability in any jurisdiction; company-name availability; social handles; domain aftermarket prices; and whether `marsa.dev`, `marsa.io`, or `marsa.app` can be bought at a reasonable price.

**DECIDED name decision (2026-10-22):** the project name is **Marsa**. The GitHub handle is **`ofuqq`** and the repository is `ofuqq/marsa`. Naming values are centralized in [`00-identity.md`](00-identity.md). The prior deployment product and the registered domains are accepted risks, not blockers for development; trademark clearance and a public domain remain required before public launch. Do not print a public URL based on `marsa.dev`, `marsa.io`, or `marsa.app`. Until a domain is secured, specifications use `urn:marsa:...` identifiers and documentation uses relative links.

**Domain candidates to check with a registrar** (quick DNS/WHOIS checks found no public record, but this is **not** a registrability guarantee): `marsa.run`, `marsa.sh`, `marsa.host`, `marsa.tools`, `getmarsa.dev`. A subdomain of an Ofuq-controlled domain is also acceptable. Do not claim any of these as owned.

### 1.3 Fallback names

Marsa is decided. These fallbacks remain documented only as a contingency if a trademark or domain blocker appears before public launch.

| Name | Arabic | Meaning | Quick check on 2026-10-22 | Trade-off |
|---|---|---|---|---|
| **Rasif** | رصيف | Quay; the platform beside the water where a ship ties up | Exact GitHub organization free (HTTP 404); GitHub user `@rasif` exists; npm `rasif` free; `ofuqq.Rasif` free; no public DNS/WHOIS record found for `rasif.dev`, `rasif.io`, `rasif.app` | Strongest fallback. The word also means “platform” in everyday Arabic, which fits a deployment platform. Domain availability is not confirmed. |
| **Marfa** | مرفأ | Harbor; a place of shelter for ships | Exact GitHub organization free; GitHub user `@marfa` exists; npm `marfa` free; `ofuqq.Marfa` free; `marfa.io` and `marfa.app` are in use | The meaning is closest to Marsa, but the name collides with Marfa, Texas and its art foundation, and the obvious domains are taken. |

A third option, **Mina** (ميناء, “port”), is rejected as a primary fallback: it is a common given name and place name, which makes search and trademark clearance harder. **Bandar** (بندر, “port”) has the same problem.

---

## 2. The problem

### 2.1 What deploying an app looks like today

A .NET developer with a small VPS often follows a path like this:

1. Buy a server and a domain.
2. Install the .NET runtime, Docker, a reverse proxy, and a certificate tool by hand.
3. Create a database, a Linux user, a service account, and firewall rules.
4. Copy or clone the code onto the server.
5. Write environment variables into a file that is easy to leak.
6. Run `dotnet publish`, build a container, or copy files into a folder.
7. Write a reverse-proxy configuration and reload it.
8. Request a TLS certificate and hope the DNS is correct.
9. Start the app, discover it listens on the wrong port, and fix the proxy again.
10. Repeat for every release, usually over SSH.
11. When a release is bad, try to remember the previous command, image, or folder.
12. When something fails at 2 a.m., read logs across three different tools.

None of these steps is impossible. The problem is that they are **unrelated chores with hidden state**, and one small mistake can take the site down. The developer wanted to ship a feature, not become a part-time reverse-proxy operator.

The pain is worse for a beginner because the failure messages do not explain the system:

- “502 Bad Gateway” does not say whether the container is down, the port is wrong, or the proxy cannot reach the network.
- “Certificate error” does not say whether DNS, port 80, or the ACME challenge failed.
- A failed deploy often destroys the working release before the new one is proven.

### 2.2 Why existing platforms do not fully solve it

**INFERRED** from the prior analysis and public product positioning. Detailed comparison is in [`02-differentiation.md`](02-differentiation.md).

- **Openship** is a broad, capable self-hosted platform with many runtimes, interfaces, a desktop app, MCP, an app catalog, and mail. That breadth is valuable, but it is also a large surface to learn and operate. It is written in TypeScript/Node.js, so a .NET developer who wants to read or extend the platform is working in a second ecosystem. Its mail engine carries a GPLv3 boundary. (Prior analysis: `01-repo-map.md`, `02-capability-analysis.md`, `08-license-notes.md`.)
- **Coolify** is a mature, broad PaaS-style tool with many integrations. It is not .NET-first, and its breadth can be overwhelming for a first deployment.
- **Dokku** is small and elegant for a Heroku-like workflow, but it is opinionated around its own buildpack/Procfile model, not around the .NET developer’s mental model, and its UI and documentation are not Arabic-first.
- **DeployWise** (if considered) is another general deployment product; it does not address the specific combination of .NET-first workflows, Arabic/RTL first-class experience, and a deliberately narrow single-host operating model.

The gap is not “another dashboard.” The gap is a **small, legible, .NET-native harbor** with a clear deployment story, first-class Arabic, and an honest operating model for one server.

### 2.3 The problem statement

> A .NET developer can build an application but cannot confidently turn a Git repository into a secure, observable, reversible deployment on their own server without learning a pile of unrelated infrastructure tools.

Marsa exists to remove that gap without pretending the server does not exist.

---

## 3. Target users

### 3.1 Primary persona — the solo .NET builder

**Who:** A developer who knows C#, ASP.NET Core, EF Core, and Git. They may have one to five small applications. They rent a VPS or own a small server. They are comfortable with the terminal but not with Docker networking, Caddy, ACME, Linux capabilities, or reverse-proxy debugging.

**Job to be done:** “Take my repository and put it online at my domain, safely, and let me undo a bad release without rebuilding my mental model of the server.”

**What they need from Marsa:**

- a guided first deploy that explains each step;
- sensible defaults for ASP.NET Core;
- clear logs and a clear failure reason;
- HTTPS without a certificate tutorial;
- a rollback button that actually restores the previous release;
- secrets that are not pasted into a shell history;
- documentation in their language.

**What they do not need at launch:** Kubernetes, multi-region failover, a marketplace, or a plugin API.

### 3.2 Secondary persona — the small team or agency

**Who:** Two to ten developers or a small studio running client applications. One person is the de facto operator.

**Job to be done:** “Give the team a shared, auditable deployment path so releases do not depend on one person’s memory.”

**Needs:** roles, audit history, safe concurrent use, a stable API/CLI, clear ownership of secrets, and a deployment history that answers “who changed what and when.”

### 3.3 Cross-cutting persona — the Arabic-first team

**Who:** A developer or team that thinks in Arabic and works in English technical tools all day. They can read English docs, but they learn faster and trust more when the product speaks their language and lays out right-to-left correctly.

**Job to be done:** “Teach me this platform in Arabic without translating every sentence in my head, and do not give me a broken RTL layout as a checkbox.”

**Needs:** Arabic UI and docs that are written as Arabic, not machine-translated; correct RTL layout, numerals, code blocks, and form behavior; English technical terms explained on first use; examples that make sense in the region.

### 3.4 Secondary persona — the platform-minded developer

**Who:** A developer who wants to understand the system, extend it, or integrate it with CI and internal tools.

**Job to be done:** “Give me a documented API, a CLI, and a state machine I can reason about, so I can automate deployments without reverse-engineering the product.”

**Needs:** OpenAPI, stable error codes, idempotency, a CLI with `--json`, webhook contracts, and an architecture that is honest about its trust boundaries.

### 3.5 Who Marsa is not for (at launch)

- Teams that need Kubernetes, multi-host scheduling, or high availability on day one.
- Windows-container workloads.
- People who want a no-code website builder.
- Organizations that need a public multi-tenant SaaS with billing.
- Mail-server hosting.
- Teams that require a plugin marketplace before adopting a platform.

These users may be served later. Designing for them now would make the first release worse for the primary persona.

---

## 4. Vision

> **Marsa is the harbor for your applications: a small, self-hosted deployment platform that takes code from Git to HTTPS on your own server, explains every step, lets you undo a bad release, and speaks Arabic and English as first-class languages.**

Marsa should feel like a capable colleague who happens to be very organized:

- It knows what it is doing and says so.
- It does not hide the server, but it does not make the user operate every moving part.
- It keeps the previous release alive until the new one is proven.
- It treats secrets as secrets and logs as evidence.
- It is small enough for one person to install, upgrade, and recover.
- It is built in .NET, for people who build in .NET, without excluding other stacks.

The long-term product can grow into more hosts, more integrations, and AI-assisted operations. The first release must earn the right to grow by being trustworthy on one host.

### 4.1 Positioning statement

**For** .NET developers and small teams who want their own server to feel like a managed platform,
**Marsa is** a self-hosted deployment harbor
**that** turns a repository into a healthy, HTTPS-routed release with visible phases and a safe rollback,
**unlike** broad PaaS-in-a-box platforms,
**Marsa** is deliberately narrow, .NET-first, and Arabic-first, and it treats “one host, one clear path” as a feature rather than a limitation.

The differentiation bets are evaluated honestly in [`02-differentiation.md`](02-differentiation.md). This vision does not assume that every bet will survive contact with users.

---

## 5. Product principles

These are decision rules, not slogans. When a design choice is unclear, the earlier principle wins.

### P1. Boring on purpose

Use the fewest moving parts that can be correct: one Linux host, Docker, PostgreSQL, Caddy, and one .NET server process. No message broker, no Redis, no service mesh, no Kubernetes in the MVP. A smaller system is easier to secure, explain, and recover.

**Consequence:** if a feature needs a new stateful dependency, it needs an ADR and a strong reason.

### P2. Safe by default

No default password. No unsigned webhook. No secret in a log, an error page, or a telemetry payload. No route to a container that has not passed health checks. The previous release keeps serving until the new one is proven. Docker-socket access is documented as root-equivalent, not hidden.

**Consequence:** the unsafe path may be possible only through an explicit, audited operator action, and some unsafe paths are not built at all.

### P3. Legible over clever

Every deployment shows its phase, its evidence, its logs, and a stable failure code. The user should be able to answer “what happened?” without SSH. A plain design that a beginner can follow beats a clever abstraction that only the author understands.

**Consequence:** we invest in the deployment timeline, error taxonomy, and documentation before we invest in advanced automation.

### P4. .NET-first, not .NET-only

ASP.NET Core is the best-supported path: detection, build recipe, health defaults, examples, and docs. Node/Next.js, static sites, Dockerfiles, and prebuilt images are supported because real teams have more than one kind of workload. Other stacks are added by data, not by forking the deployment engine.

**Consequence:** a .NET developer should never feel like a second-class user of their own platform.

### P5. Arabic and RTL are first-class

Arabic is not a translation layer added at the end. It affects layout direction, typography, numerals, form validation, documentation structure, error messages, and the examples we choose. English remains the source language for code and API contracts, but Arabic documentation is written for learners, not generated word-for-word.

**Consequence:** RTL and Arabic copy are part of the acceptance criteria for user-facing work, not a post-launch task.

### P6. One obvious way to do the common thing

The MVP supports one host, one deployment path, one edge, and one database. We do not expose a matrix of modes and expect the user to understand the interactions. Advanced escape hatches come later and are explicit.

**Consequence:** “we could also support X” is not a reason to add X before the core path is trusted.

### P7. The API is the contract

The dashboard, CLI, and future integrations use the same application services and the same versioned API. OpenAPI is generated, not hand-maintained. Error codes are stable. The UI is allowed to call application handlers directly for server-rendered pages, but it does not get a private business layer.

**Consequence:** no feature is “dashboard-only” unless it is explicitly a presentation concern.

### P8. Own your data and your server

Self-hosted means the operator controls the database, the logs, the secrets, and the host. Telemetry is off by default. No feature requires a Marsa cloud account. Updates are explicit. Backups and the master key are the operator’s responsibility, and Marsa must make that responsibility visible rather than pretending it away.

**Consequence:** no silent phone-home, no forced SaaS dependency, no hidden remote control plane.

### P9. Clean-room, with evidence

Marsa is informed by the behavior of Openship and other tools, not copied from them. We describe behavior in our own words, design our own models, and keep a provenance record. GPL components, including the iRedMail engine, are not part of the default distribution. See [`12-license-and-clean-room.md`](12-license-and-clean-room.md).

**Consequence:** a convenient copy is not convenient if it contaminates the license or hides provenance.

### P10. Build it so agents can help

The repository is designed for humans and AI agents working together: small issues with acceptance criteria, explicit docs, project skills, and a definition of done. An agent should be able to implement one issue without guessing the architecture.

**Consequence:** documentation and issue quality are part of the product, not overhead. See [`AGENTS.md`](../AGENTS.md) and [`ai/`](ai/).

---

## 6. What Marsa is and is not

| Marsa is | Marsa is not |
|---|---|
| A control plane that decides, records, and explains deployments | A container runtime or a hypervisor |
| A harbor for a small number of applications on one host | A Kubernetes replacement or a multi-region scheduler |
| A safe default path from Git to HTTPS | A general-purpose CI system |
| A .NET-first product that also supports other stacks | A .NET-only product |
| Arabic-first in UI and documentation | An English product with a translated help page |
| A place where the previous release stays alive until the new one is healthy | A “push and pray” script with a dashboard |
| Open source, self-hosted, and operator-controlled | A SaaS that happens to run on your server |
| A clean-room implementation with a license boundary | A fork or a repackaging of Openship |

---

## 7. Non-goals

### 7.1 Non-goals for the MVP

The MVP is defined in detail in [`01-product-requirements.md`](01-product-requirements.md) and [`08-roadmap.md`](08-roadmap.md). The intentional exclusions are:

- multi-host scheduling, SSH targets, Kubernetes, Nomad, or a cluster;
- Docker Compose import and multi-service orchestration beyond one service per deployment;
- a one-click application catalog or marketplace;
- managed database provisioning, failover, or scaling;
- scheduled application-data backups and a restore UI (an operator script and drill are in scope);
- a public SaaS, billing, or usage metering;
- Windows or macOS workload hosts;
- Windows containers;
- a desktop application;
- built-in mail hosting (the GPL boundary is a hard blocker for the default distribution);
- an MCP server or an AI assistant that can mutate production;
- a third-party plugin marketplace or arbitrary .NET assembly loading;
- PR preview environments, approvals, and advanced team workflows;
- a general-purpose secrets manager beyond what deployments need;
- wildcard DNS/TLS;
- automatic rollback (the MVP keeps the old release serving and requires an explicit rollback);
- an in-browser terminal or arbitrary host shell.

### 7.2 Deferred, not rejected forever

Some of these are good ideas. They are deferred because the first release must be small enough to finish and trustworthy enough to recommend. The roadmap in [`08-roadmap.md`](08-roadmap.md) gives each one a phase and an exit condition. Deferring is not a promise to build; it is a promise not to smuggle the feature into the MVP.

### 7.3 Explicitly rejected unless the product thesis changes

- Running arbitrary user shell commands on the host.
- A public container registry.
- Multi-tenant SaaS hosting in the first product.
- Shipping GPL code in the default image.
- A “zero-auth” mode on a network-reachable instance.
- Silent telemetry or a mandatory cloud account.

---

## 8. Success criteria

Success is not “we shipped a dashboard.” It is evidence that the harbor is safe and useful.

### 8.1 User outcomes

1. A developer with a clean Linux VPS and an ASP.NET Core repository can complete setup and reach a working HTTPS deployment in **under 10 minutes**, following the documentation without SSH.
2. A failed build or a failed health check **never replaces a working release**; the previous release keeps serving.
3. A rollback restores the previous release **without a rebuild** when the artifact still exists, and the UI says exactly what was restored.
4. A user can explain a failed deployment from the deployment page alone: phase, failing step, stable error code, and a next action.
5. A secret can be set, used by a deployment, and **never appears** in logs, API projections, audit payloads, or telemetry.
6. An Arabic-speaking beginner can follow the Arabic explainer and the UI without switching to English for the core flow, and the RTL layout is correct at 360 px and 1280 px.
7. One operator can install, upgrade, back up, restore, and diagnose the instance using the runbooks.

### 8.2 MVP metrics (PROPOSED targets)

These are design targets, not contractual SLAs. They are measured in usability tests, the E2E environment, and opt-in feedback — not by silent telemetry.

| Metric | Target | How measured |
|---|---|---|
| Setup completion | ≥ 80% of started installs in usability tests | Facilitated test funnel |
| First-deploy success on supported stacks | ≥ 70% of first attempts | E2E and usability tests |
| Deploy success rate excluding user build errors | ≥ 95% | Deployment outcomes |
| Median sample-app deploy time | < 3 minutes with a cached base image | Phase durations |
| Rollback success | 100% in E2E, no route regression | E2E and audit |
| Time to diagnose a failed deploy | < 5 minutes | Usability test |
| Secret leakage | 0 canary findings | Redaction test suite |
| Unmanaged container operations | 0 | Adapter guard tests |
| Active deployments per environment | ≤ 1 at all times | Database invariant and reconciliation |
| Accessibility | Primary flows usable by keyboard; labels and error summaries present | Manual + automated checks |

### 8.3 Technical gates

- `dotnet build` and `dotnet test` pass on Linux with zero warnings.
- A clean-host install and an upgrade from the previous release pass.
- A control-plane restart resumes or safely fails an in-flight deployment with a clear state.
- Caddy config failure does not take down unrelated routes.
- Cross-organization access fails closed even with a valid token.
- The default distribution contains no GPL or unknown-license component.
- The license/SBOM review has no unresolved blocker.

### 8.4 Community and adoption signals (later)

These are signals, not MVP gates: five external operators completing a first deploy without help; ten external issues with reproducible reports; at least one community contribution merged; Arabic documentation read by Arabic-speaking testers who are not on the team. They are **PROPOSED** and should be revised after the first five user interviews.

---

## 9. Risks that could invalidate the vision

| Risk | Why it matters | Early response |
|---|---|---|
| Scope creep | The product becomes a worse Coolify instead of a better harbor for its niche | Enforce the MVP non-goals; every new dependency needs an ADR |
| Docker socket trust | A control-plane bug can become host compromise | Document the boundary, use guardrails, plan a socket proxy/agent, never expose the socket to workloads |
| Name/domain conflict | A prior same-category product used “Marsa,” and rebranding after packages and domains exist is expensive | Name decided and centralized; trademark clearance and a public domain are release gates, tracked in `07-security.md` and `12-license-and-clean-room.md` |
| “Arabic-first” becomes translation | The product feels foreign to its core audience | Native review, RTL acceptance criteria, Arabic examples written from scratch |
| Detection is wrong | The first deploy fails for real repositories | Start with four supported stacks; show evidence; allow `marsa.json`/Dockerfile overrides; never guess silently |
| The platform is too hard to operate | Self-hosted users blame Marsa for host problems | Install/upgrade/backup runbooks, health diagnostics, honest limits |
| Clean-room drift | A copied file or GPL component creates legal exposure | Provenance ledger, dependency gate, license review before release |
| Solo-maintainer burnout | The roadmap outgrows the available time | Vertical milestones, shippable phases, explicit deferrals |

The full risk register is in [`07-security.md`](07-security.md) and the final risk table in [`08-roadmap.md`](08-roadmap.md). The legal boundary is in [`12-license-and-clean-room.md`](12-license-and-clean-room.md).

---

## 10. What must be true before we call it “Marsa 0.1”

This is the vision-level definition of done. The detailed acceptance criteria live in the roadmap.

1. The name is decided and centralized; a public domain is secured and trademark clearance is complete.
2. The license is confirmed (PROPOSED: Apache-2.0) and the clean-room process is documented.
3. One Linux host can run the full stack with the documented install.
4. A real ASP.NET Core application deploys from Git to HTTPS.
5. A bad build and a bad health check leave the old release serving.
6. A rollback works and is visible.
7. Logs stream and survive a page reload.
8. Secrets are encrypted, masked, and absent from logs.
9. The Arabic explainer and glossary are complete enough for a beginner.
10. An operator can install, upgrade, back up, restore, and diagnose the instance.

If any of these is false, the product is not ready for its first external user, regardless of how many features are present.

---

## 11. Decision log for this document

| Decision | Status | Rationale | Revisit when |
|---|---|---|---|
| Product name “Marsa”; GitHub handle `ofuqq`; repo `ofuqq/marsa` | **DECIDED 2026-10-22** | Short, bilingual, meaningful; owner accepted the prior same-category use and the domain situation | Trademark clearance and domain acquisition before public launch; if blocked, revisit Rasif |
| Ofuq is the brand; Marsa is the product; `ofuqq` is only an account handle | **DECIDED 2026-10-22** | Keeps the parent brand stable and prevents double-branding | Ofuq launches multiple products with separate identities |
| Apache-2.0 outbound license | **PROPOSED** | Patent grant, ecosystem compatibility, self-hosted familiarity | Legal review or a dual-license decision |
| Single-host Docker MVP | **PROPOSED**, inherited from prior analysis | Keeps the state machine, security model, and operations tractable | A validated user segment requires multi-host at launch |
| Arabic and English docs from the start | **PROPOSED** | Arabic-first is a differentiator, not a translation afterthought | Native review shows the scope is unsustainable |
| No GPL components in the default distribution | **PROPOSED**, inherited | Avoids copyleft contamination and source-offer complexity | A separately packaged, legally reviewed optional add-on exists |
| MVP excludes MCP/AI mutation | **PROPOSED**, inherited | The API and authorization model must be stable first; an AI that can deploy is a security boundary | After OpenAPI, PAT scopes, and audit are proven |
| No silent telemetry | **PROPOSED** | Self-hosted trust; metrics can be measured with consent | Operators explicitly opt in to anonymous usage data |

---

## 12. Next reading

- [`01-product-requirements.md`](01-product-requirements.md) — who, what, and the MVP boundary.
- [`02-differentiation.md`](02-differentiation.md) — can this really be different, and where?
- [`03-architecture.md`](03-architecture.md) — how the harbor is built.
- [`06-api-contracts.md`](06-api-contracts.md) — the public contract.
- [`07-security.md`](07-security.md) — the trust boundaries and threats.
- [`08-roadmap.md`](08-roadmap.md) — the order of construction.
