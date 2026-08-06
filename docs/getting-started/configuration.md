---
title: Configuration
parent: Getting Started
nav_order: 2
---

# ⚙️ Configuration
{: .no_toc }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

---

{: .note }
This page documents the **target** configuration surface from the approved architecture. As of the current skeleton phase, none of these are consumed by application code yet — they exist as CI/CD secrets and design intent, wired up as each phase lands. Check the [Roadmap]({{ site.baseurl }}/roadmap/) for what's actually implemented.

## GitHub App credentials

| Variable | Used by | Purpose |
| :------- | :------ | :------ |
| `GH_APP_ID` | Worker | The GitHub App's numeric ID, for RS256 JWT signing |
| `GH_APP_PRIVATE_KEY` | Worker | The App's PEM private key — never committed, stored in a secret store |
| `GH_WEBHOOK_SECRET` | Webhook | HMAC-SHA256 secret for verifying `X-Hub-Signature-256` on incoming deliveries |

## Broker and database

| Variable | Used by | Purpose |
| :------- | :------ | :------ |
| `AMQP_URL` | Webhook, Worker | LavinMQ connection string (topic exchange `apibr.vagas`) |
| `MARIADB_CONNECTION` | Webhook, Worker, Api | Connection string to the `vagas_labeler` MariaDB database |

## Inference tier (optional, off by default)

| Variable | Purpose |
| :------- | :------ |
| `Inference:Provider` | `null` (default) or `openai-compatible` |
| `Inference:BaseUrl` | Endpoint for the configured provider |
| `Inference:Model` | Model identifier, e.g. `llama-3.1-8b-instant` for the default Groq provider |
| `INFERENCE_API_KEY` | Provider API key — never committed |
| `Inference:TimeoutSeconds` | Hard timeout before falling back to the deterministic result |
| `Inference:MaxBodyChars` | Truncation budget for issue bodies sent to the provider |
| `Inference:DailyRequestBudget` / `Inference:DailyTokenBudget` | Per-repo and global daily caps |
| `INFERENCE_DISABLED` | Global kill-switch — overrides every other flag when `true` |

See [Architecture → Overview]({{ site.baseurl }}/architecture/overview/) for how these compose into the resolution order that decides whether the inference tier actually runs on a given message.

## Deployment secrets

Full list, including CI/CD-only secrets like `WIREGUARD_CONF` and deploy SSH keys, lives in [Deployment]({{ site.baseurl }}/deployment/).
