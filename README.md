# 🏷️🤖 Vagas Labeler Bot

> **Automatic label classification for developer job postings** — reads issues opened on `backend-pt/vagas`, `dotnetdevbr/vagas`, `frontend-pt/vagas`, and `nodejsdevbr/vagas`, and applies the labels each repository already has. Never creates, renames, or recolors a label — only applies and removes.

[![Build](https://github.com/ApiBR/vagas-labeler-bot/actions/workflows/build.yml/badge.svg)](https://github.com/ApiBR/vagas-labeler-bot/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ApiBR_vagas-labeler-bot&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=ApiBR_vagas-labeler-bot)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ApiBR_vagas-labeler-bot&metric=coverage)](https://sonarcloud.io/summary/new_code?id=ApiBR_vagas-labeler-bot)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Docs](https://img.shields.io/badge/docs-apibr.github.io-blue)](https://apibr.github.io/vagas-labeler-bot/)

---

## ✨ What it does

| Step | Detail |
| ---- | ------ |
| 📥 **Reads** | GitHub issues opened/edited/reopened on the four `vagas` repos, via a GitHub App webhook |
| 🧠 **Classifies** | A deterministic, tiered text engine (explicit label directives → structured fields → section matching → whole-body matching), with an optional LLM tier for the gaps |
| 🏷️ **Labels** | Applies/removes only labels that already exist in the target repo — configured per repo via a `vagas-labeler.yml` the maintainers own |
| 🚫 **Never** | Creates, renames, deletes, or recolors a label |

Full design rationale, the per-repo configuration schema, and the classification tiers are documented on the [docs site](https://apibr.github.io/vagas-labeler-bot/).

---

## 🏗️ Architecture

```text
GitHub issues ──HTTPS──► Webhook Ingress ──publish──► LavinMQ ──► Worker ──► GitHub REST API
                              │                                      │
                              └──────────── persist raw ─────────────┴──► MariaDB ◄── Private API
```

Three independently deployable entry points, Clean Architecture underneath:

```text
src/
├── ApiBr.Vagas.Domain            # entities, value objects — zero dependencies
├── ApiBr.Vagas.Application       # use cases, ports (interfaces)
├── ApiBr.Vagas.Contracts         # AMQP message envelopes
├── ApiBr.Vagas.Configuration     # per-repo YAML schema, parser, validator
├── ApiBr.Vagas.Labeling          # deterministic classification engine
├── ApiBr.Vagas.Inference         # optional LLM tier — off by default
├── ApiBr.Vagas.Infrastructure    # MariaDB, LavinMQ, GitHub client, health/version
├── ApiBr.Vagas.Webhook           # entry point 1 — ingress (ASP.NET Core minimal API)
├── ApiBr.Vagas.Worker            # entry point 2 — AMQP consumer + scheduled jobs
└── ApiBr.Vagas.Api               # entry point 3 — private API (internal only)
```

Dependency direction: `Webhook`/`Worker`/`Api` → `Infrastructure` → `Application` → `Domain`. `Domain` never references anything else.

---

## 🚀 Getting started

```bash
git clone https://github.com/ApiBR/vagas-labeler-bot.git
cd vagas-labeler-bot
dotnet build
dotnet test
```

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (see [`global.json`](global.json) for the pinned version). Running the entry points locally against real MariaDB/LavinMQ instances is covered in [Getting Started](https://apibr.github.io/vagas-labeler-bot/getting-started/) on the docs site.

---

## 📊 Project status

This repository is being built in phases; see the [roadmap](https://apibr.github.io/vagas-labeler-bot/roadmap/) for the full plan and current status. In short:

- ✅ **Skeleton** — solution, Clean Architecture project graph, GitVersion, CI, health/version endpoints on all three entry points
- 🚧 **Ingress, broker, persistence, config subsystem, classification engine, inference tier, private API, deployment** — not yet built

---

## 🤝 Contributing

1. 🍴 Fork the repository
2. 🌿 Create a feature branch: `git checkout -b feature/my-feature`
3. ✅ Commit your changes and ensure `dotnet build` / `dotnet test` pass
4. 📬 Push and open a pull request against `main`

All PRs run through CI (build + test + coverage) and SonarCloud analysis.

---

## 📄 License

MIT © [API BR](https://github.com/ApiBR)
