---
title: Roadmap
nav_order: 6
permalink: /roadmap/
---

# 🗺️ Roadmap
{: .no_toc }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

---

The project is built in strict phases — no phase's code is written until the previous one is either done or explicitly approved. This page tracks where things actually stand, not just what's planned.

## Status

| Phase | What it delivers | Status |
| :---- | :---------------- | :----- |
| 1. Discovery | Real label taxonomies, issue templates, and traffic patterns pulled from all four vacancy repos; starter `vagas-labeler.yml` drafts seeded from production data | ✅ Done |
| 2. Design review | Architecture, project graph, config schema, message contract, DB schema restated and approved | ✅ Done |
| 3. Skeleton | Solution, Clean Architecture project graph, `Directory.Build.props`, GitVersion, CI build, health/version endpoints | ✅ Done |
| 4. Ingress + broker + persistence | End-to-end delivery capture (HMAC verify, dedupe, persist raw, publish to LavinMQ) — no classification yet | 🚧 Not started |
| 5. Config subsystem | Fetch, validate, cache, 1:1 fallback, config history, `ConfigLinter`, reusable validation Action — fully tested before classification logic depends on it | 🚧 Not started |
| 6. Deterministic engine + golden corpus | The bulk of the classification logic, dry-run only | 🚧 Not started |
| 7. Reconciliation + GitHub writes | Ships to production with `DRY_RUN=true` first, compared against reality for a week before going live | 🚧 Not started |
| 8. Inference tier | Wired and shipped **disabled**, then run in shadow mode to gather an agreement report before ever enabling `on-gap` | 🚧 Not started |
| 9. Private API + stats | `/v1/*` endpoints, explainability, dry-run sandbox, feature flags | 🚧 Not started |
| 10. Deploy manifests | Compose files, NGINX configs, deploy workflow for the three OCI nodes | 🚧 Not started |
| 11. Backfill | Re-classify existing open issues once the pipeline is live | 🚧 Not started |

## What discovery changed about the plan

Real data from the four repos overrode a few of the original design's assumptions — see [Configuration Schema → Why groups are non-exclusive by default]({{ site.baseurl }}/configuration-schema/#why-groups-are-non-exclusive-by-default) for the most consequential one. In short:

- Group exclusivity defaults to **off**, not on — the repos' own templates explicitly invite multi-label groups.
- The classification engine is designed **prose-first** — most real traffic bypasses issue templates entirely.
- Salary currency is **detected from text**, not trusted from a fixed per-repo config default — the same repo has posted both R$ and € listings.
- Two of the four repos (`backend-pt`, `frontend-pt`) collect a salary value via their form but currently have **no salary labels at all** — those get persisted to the database for stats but no label is applied, since the bot never creates labels.

## Where to go next

- [Architecture]({{ site.baseurl }}/architecture/) for the full system design.
- [Configuration Schema]({{ site.baseurl }}/configuration-schema/) for the `vagas-labeler.yml` format.
- [GitHub Issues](https://github.com/ApiBR/vagas-labeler-bot/issues) for in-flight work on the current phase.
