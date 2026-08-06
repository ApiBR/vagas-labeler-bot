---
title: 🏷️🤖 Vagas Labeler Bot
layout: home
nav_order: 1
---

# 🏷️🤖 Vagas Labeler Bot
{: .fs-9 }

Automatic label classification for developer job postings on GitHub Issues.
{: .fs-6 .fw-300 }

[Get started now]({{ site.baseurl }}/getting-started/){: .btn .btn-primary .fs-5 .mb-4 .mb-md-0 .mr-2 }
[View on GitHub](https://github.com/ApiBR/vagas-labeler-bot){: .btn .fs-5 .mb-4 .mb-md-0 }

---

## What is Vagas Labeler Bot?

Four GitHub repositories publish developer job vacancies as **issues**:

- [`backend-pt/vagas`](https://github.com/backend-pt/vagas)
- [`dotnetdevbr/vagas`](https://github.com/dotnetdevbr/vagas)
- [`frontend-pt/vagas`](https://github.com/frontend-pt/vagas)
- [`nodejsdevbr/vagas`](https://github.com/nodejsdevbr/vagas)

A GitHub App installed on them reads every issue opened, edited, or reopened, infers the vacancy's attributes (seniority, work modality, salary range, contract type, and more), and applies the **labels that already exist in that repository**.

{: .important }
The bot never creates, renames, deletes, or recolors labels. It only applies and removes labels that already exist in the target repo. If a configured label is missing, it logs a warning and skips it.

---

## Quick overview

```text
GitHub issues ──HTTPS──► Webhook Ingress ──publish──► LavinMQ ──► Worker ──► GitHub REST API
                              │                                      │
                              └──────────── persist raw ─────────────┴──► MariaDB ◄── Private API
```

---

## Feature highlights

| 🏷️ Feature | 📝 Description |
| :--------- | :------------- |
| **Per-repo configuration** | Each vacancy repo owns its own `vagas-labeler.yml` word → label mapping — no bot redeploy needed to retune it |
| **1:1 fallback** | Repos without a config file get an implicit taxonomy derived from their own existing labels |
| **Tiered classification** | Explicit label directives → structured fields → section matching → whole-body matching, in that priority order |
| **Optional LLM tier** | Fully built, off by default, feature-flagged, with a shadow mode for measuring agreement before ever enabling it |
| **Safe reconciliation** | Only removes a label if the bot itself applied it — human-applied labels are untouchable |
| **Full explainability** | Every classification records which tier matched, what text, what confidence, and which config produced it |

---

## Technology stack

| Layer | Technology |
| :---- | :--------- |
| Runtime | .NET 10 / ASP.NET Core 10, C# 14 |
| Architecture | Clean Architecture — domain has zero infrastructure references |
| Broker | LavinMQ (AMQP 0.9.1) |
| Database | MariaDB |
| LLM (optional) | OpenAI-compatible endpoint, Groq `llama-3.1-8b-instant` by default |
| CI/CD | GitHub Actions + SonarCloud |
| Hosting | Docker on Oracle Cloud Infrastructure VMs behind NGINX |
| Docs | Jekyll + Just the Docs (this site) |

---

## Getting started

Head to [Getting Started]({{ site.baseurl }}/getting-started/) to build and run the solution locally, or read [Architecture]({{ site.baseurl }}/architecture/) to understand how everything fits together.
