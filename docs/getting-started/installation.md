---
title: Installation
parent: Getting Started
nav_order: 1
---

# 📦 Installation
{: .no_toc }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

---

## Clone the repository

```bash
git clone https://github.com/ApiBR/vagas-labeler-bot.git
cd vagas-labeler-bot
```

## Restore and build

```bash
dotnet restore
dotnet build
```

The [.NET SDK version pinned in `global.json`](https://github.com/ApiBR/vagas-labeler-bot/blob/main/global.json) is installed automatically if you use an SDK-aware terminal; otherwise install it from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0).

## Run the tests

```bash
dotnet test
```

This runs the full suite, including the integration smoke tests that boot the Webhook and Api entry points in-memory via `WebApplicationFactory` and assert their `/health`, `/health/ready`, and `/version` endpoints respond.

## Run an entry point

Each entry point is independently runnable:

```bash
dotnet run --project src/ApiBr.Vagas.Webhook   # ingress — GET /health, /health/ready, /version
dotnet run --project src/ApiBr.Vagas.Worker    # AMQP consumer + scheduled jobs — same endpoints
dotnet run --project src/ApiBr.Vagas.Api       # private API — GET /v1/health, /v1/health/ready, /v1/version
```

{: .warning }
None of the three entry points do real work yet beyond serving health/version endpoints — the webhook ingress, AMQP consumption, and MariaDB persistence land in later phases. Check the [Roadmap]({{ site.baseurl }}/roadmap/) for current status.

## Solution layout

```text
src/
├── ApiBr.Vagas.Domain            # entities, value objects — zero dependencies
├── ApiBr.Vagas.Application       # use cases, ports (interfaces)
├── ApiBr.Vagas.Contracts         # AMQP message envelopes
├── ApiBr.Vagas.Configuration     # per-repo YAML schema, parser, validator
├── ApiBr.Vagas.Labeling          # deterministic classification engine
├── ApiBr.Vagas.Inference         # optional LLM tier — off by default
├── ApiBr.Vagas.Infrastructure    # MariaDB, LavinMQ, GitHub client, health/version
├── ApiBr.Vagas.Webhook           # entry point 1 — ingress
├── ApiBr.Vagas.Worker            # entry point 2 — AMQP consumer + scheduled jobs
└── ApiBr.Vagas.Api               # entry point 3 — private API

tests/
├── ApiBr.Vagas.Labeling.Tests
├── ApiBr.Vagas.Configuration.Tests
├── ApiBr.Vagas.Inference.Tests
├── ApiBr.Vagas.Application.Tests
└── ApiBr.Vagas.Integration.Tests

tools/
├── ClassifierBench     # replays historical issues; precision/recall
├── CorpusFetcher        # pulls real issues from the four repos into fixtures
└── ConfigLinter          # standalone CLI to validate a repo's vagas-labeler.yml
```

See [Architecture]({{ site.baseurl }}/architecture/) for how these projects depend on each other.
