---
title: Overview
parent: Architecture
nav_order: 1
---

# 🔄 Overview
{: .no_toc }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

---

## End-to-end data flow

```text
GitHub                          Webhook Ingress                    LavinMQ
issues / push / label   ──HTTPS──►  1. verify HMAC              ──publish──►  topic exchange
events                              2. dedupe delivery                         apibr.vagas
                                    3. persist raw ──► MariaDB
                                    4. publish envelope
                         ◄──202───  (must respond in < ~200ms)

                                                                        │
                                                                        ▼
                                                                     Worker
                                                          1. resolve installation token
                                                          2. resolve repo config (YAML or 1:1 fallback)
                                                          3. load repo's real labels (cached)
                                                          4. run deterministic engine (tiers 1–4)
                                                          5. if gap + flag on → inference tier (tier 5)
                                                          6. reconcile labels ──► GitHub REST API
                                                          7. persist classification ──► MariaDB

Private API ──reads──► MariaDB
```

{: .important }
The ingress does no GitHub API calls, no classification, no slow I/O. It must return **202 Accepted** in under ~200ms or GitHub starts marking deliveries as failed.

---

## Three entry points

| Entry point | Role | Exposure |
| :---------- | :--- | :------- |
| **Webhook** (`ApiBr.Vagas.Webhook`) | Ingress — verifies signatures, dedupes, persists raw payloads, publishes to the broker | Public (GitHub only) |
| **Worker** (`ApiBr.Vagas.Worker`) | AMQP consumer, classification pipeline, scheduled jobs | Internal only |
| **Api** (`ApiBr.Vagas.Api`) | Private read/explainability API, dry-run classification sandbox, feature flags | Internal only |

All three expose `/health`, `/health/ready`, and `/version` (the private Api under a `/v1` prefix) — see [Getting Started → Installation]({{ site.baseurl }}/getting-started/installation/) to run them locally.

---

## Clean Architecture project graph

```text
ApiBr.Vagas.Domain            (zero dependencies)
        ▲
        │
ApiBr.Vagas.Configuration ──► Domain
ApiBr.Vagas.Labeling ────────► Domain, Configuration
ApiBr.Vagas.Inference ───────► Domain, Configuration
        ▲
        │
ApiBr.Vagas.Application ─────► Domain, Contracts, Configuration, Labeling, Inference
        ▲
        │
ApiBr.Vagas.Infrastructure ──► Application, Domain, Contracts, Configuration, Labeling, Inference
        ▲
        │
Webhook / Worker / Api ──────► Application, Infrastructure (+ Contracts where messages are touched directly)
```

`Domain` never references anything else in the solution. `Infrastructure` is the only project allowed to know about MariaDB, LavinMQ, or the GitHub REST API — it implements the ports `Application` defines.

---

## The classification tiers

The deterministic engine tries each tier in order and stops at the first confident match; the optional LLM tier only runs for groups still unresolved after tier 4:

| Tier | Source | Notes |
| :--- | :----- | :---- |
| **1** (highest) | Explicit label directive — a trailing `## Labels` section, or an HTML comment marker | Matched case/diacritic-insensitively against the repo's real labels |
| **2** | Issue-form / structured fields — GitHub issue forms, `**Campo:** valor`, two-column tables | Heading maps to a config group via `fieldAliases` |
| **3** | Section-scoped word/regex matching — search only inside the section whose heading maps to the group | Cuts false positives sharply |
| **4** | Whole-body word/regex matching with word boundaries and negations | Last deterministic resort |
| **5** | LLM inference — off by default | Only for `required` groups still unresolved after tier 4, gated by four independent conditions |

For an exclusive group, the highest tier wins; within a tier, the highest confidence; a tie resolves to nothing and the group is marked ambiguous. Non-exclusive groups union every match above `minConfidence`.

{: .note }
Real-world discovery across the four vacancy repos found that **prose-only issues are the dominant traffic shape today**, not structured issue-form submissions — the engine is designed prose-first, with structured tiers as a bonus when maintainers' templates are followed.

See [Configuration Schema]({{ site.baseurl }}/configuration-schema/) for the full `vagas-labeler.yml` format that drives tiers 1–4, and [Data Model]({{ site.baseurl }}/architecture/data-model/) for how every classification decision is persisted for explainability.
