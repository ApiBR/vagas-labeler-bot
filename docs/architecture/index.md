---
title: Architecture
nav_order: 3
has_children: true
permalink: /architecture/
---

# 🏗️ Architecture

How Vagas Labeler Bot is put together, and why.

---

## In this section

| Page | What you'll learn |
| :--- | :---------------- |
| [Overview]({{ site.baseurl }}/architecture/overview/) | The end-to-end data flow, entry points, and the Clean Architecture project graph |
| [Data Model]({{ site.baseurl }}/architecture/data-model/) | The MariaDB schema — what's raw, what's processed, what's audit trail |
| [Security]({{ site.baseurl }}/architecture/security/) | Signature verification, ReDoS-safe config parsing, PII handling, retention policy |

---

## Design principles

- **The bot never creates labels.** It only applies and removes labels that already exist in the target repository.
- **Domain has zero infrastructure references.** `ApiBr.Vagas.Domain` depends on nothing else in the solution.
- **Configuration lives in each vacancy repo, not in the bot.** Maintainers own their own `vagas-labeler.yml` and can retune it without a bot deploy.
- **The inference tier is inert until explicitly enabled — at every layer.** Global flag, provider configured, per-repo opt-in, and a mode condition must *all* be true before an LLM call happens.
- **Never lose an event, never fabricate a label.** Webhook deliveries are persisted before publishing; LLM output is validated against an allow-list and any label outside it is silently discarded.
