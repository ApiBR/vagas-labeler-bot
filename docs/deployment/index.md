---
title: Deployment
nav_order: 5
permalink: /deployment/
---

# 🚢 Deployment
{: .no_toc }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

---

{: .note }
This describes the target deployment topology from the approved design. Compose files, NGINX configs, and the deploy workflow land later in the roadmap — see [Roadmap]({{ site.baseurl }}/roadmap/).

## Nodes

Three deployable units, three images, three DNS names, on Docker over Oracle Cloud Infrastructure VMs behind NGINX:

| Node | Component | Proposed domain | Exposure |
| :--- | :-------- | :--------------- | :------- |
| Node A | Webhook ingress | `webhook.vagas.apibr.com` | Public (GitHub only) — NGINX + Let's Encrypt, rate-limited, optionally restricted to GitHub's `meta` hook IP ranges |
| Node B | Worker | `worker.vagas.apibr.com` | Internal only — health endpoint on the private interface |
| Node C | Private API | `api.vagas.apibr.com` | Internal only — NGINX allow-list + VPN |

Each node runs `docker compose` with `restart: unless-stopped`, explicit resource limits, a healthcheck, and log rotation. Target platform: `linux/amd64`.

## The RAM constraint

The existing OCI VMs are AMD E2.1.Micro (1 vCPU / 1 GB) — tight for three .NET services, and not viable at all for a self-hosted inference model. The Worker — and any optional Ollama profile — belongs on the larger VPS instead. Container memory limits are set explicitly, since .NET's default heap sizing on a 1 GB box will otherwise trigger OOM kills.

## Images

Multi-stage Docker builds produce `ghcr.io/apibr/vagas-labeler-{webhook,worker,api}`, tagged `:{semver}`, `:{major.minor}`, `:sha-{commit}`, and `:latest`, with OCI labels `org.opencontainers.image.version` / `.revision` / `.source` — see [Versioning]({{ site.baseurl }}/architecture/overview/) for how the same GitVersion-resolved value is stamped everywhere.

## Optional self-hosted inference

`docker compose --profile inference up` brings up an Ollama profile so the inference tier can run fully self-hosted with zero external API cost. The worker reaches it over the private network — this only makes sense on the larger VPS, not the 1 GB micro instances.

## Broker and database

- **LavinMQ** — a new dedicated topic exchange `apibr.vagas` and durable queue `apibr.vagas.labeler` on the existing CloudAMQP instance, isolated from anything else running there.
- **MariaDB** — a new dedicated database, `vagas_labeler`, on the existing `osasco` (NuvemHospedagem) MariaDB server. Deliberately **not** sharing tables with the unrelated `ApiBR/vagas-aggregator-service`'s database on the same server, which already has its own `repositories` table under a different shape.

## CI/CD secrets

| Secret | Used for |
| :----- | :------- |
| `GH_APP_ID`, `GH_APP_PRIVATE_KEY`, `GH_WEBHOOK_SECRET` | GitHub App authentication and webhook verification |
| `AMQP_URL` | LavinMQ connection |
| `MARIADB_CONNECTION` | Database connection |
| `INFERENCE_API_KEY` | Optional LLM provider (Groq by default) |
| `WIREGUARD_CONF` | Reaching MariaDB from the migrations CI job |
| Deploy SSH keys | `deploy.yml`'s SSH-to-VM step |

GitHub Environments gate production deploys behind required reviewers, with OIDC used where available. Everything in this repository is public, so CI logs are audited for accidental secret leakage and configuration values are never echoed verbatim.

## Workflows

| Workflow | Trigger | Does |
| :------- | :------ | :--- |
| `build.yml` | push, PR | GitVersion → restore → build → test → coverage → SonarCloud analysis |
| `codeql.yml` | PR, weekly | CodeQL for C# |
| `dependency-review.yml` + Dependabot | PR, weekly | Supply-chain checks |
| `classifier-eval.yml` | PR touching the classification engine or fixtures | Runs `ClassifierBench` against the golden corpus; fails the PR on a precision/recall regression |
| `release.yml` | push to `main` / tag | Multi-stage Docker builds → `ghcr.io/apibr/*` → GitHub Release with GitVersion notes |
| `migrations.yml` | manual + pre-deploy | Connects via WireGuard, runs DbUp, uploads the applied-script log |
| `deploy.yml` | after release, environment-gated | SSH to target VM → `docker compose pull && up -d` → poll `/health/ready` → auto-rollback on failure |
| `pages.yml` | push touching `docs/**` | Builds and deploys this documentation site |
