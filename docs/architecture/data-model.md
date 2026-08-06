---
title: Data Model
parent: Architecture
nav_order: 2
---

# 🗄️ Data Model
{: .no_toc }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

---

MariaDB (`utf8mb4` / `utf8mb4_unicode_ci`), in a dedicated `vagas_labeler` database. Raw payloads, processed data, and stats live in separate tables so the replay source of truth (the raw webhook payload) is never mutated by classification logic.

{: .note }
This is the target schema from the approved design. Migrations land in the persistence phase — see the [Roadmap]({{ site.baseurl }}/roadmap/).

## Tables

| Table | Purpose |
| :---- | :------ |
| `installations` | GitHub App installation records — `installation_id`, `account_login`, `suspended_at`, `permissions_hash` |
| `repositories` | The four vacancy repos — `owner`, `name`, `full_name`, `config_mode` (`explicit`/`default-1to1`), `is_active` |
| `repository_configs` | Append-only history of every fetched `vagas-labeler.yml` — `config_hash` (blob SHA), `raw_yaml`, `parsed_json`, `is_valid`, `validation_errors`, `is_current` |
| `repository_labels` | Cache of each repo's real labels — `name`, `color`, `description`, `last_seen_at`, used for drift detection |
| `webhook_deliveries` | The replay source of truth — full raw JSON payload, `signature_valid`, `status` (`pending`/`published`/`processed`/`failed`), unique on `delivery_id` |
| `issues` | One row per GitHub issue — `repo_id`, `issue_number`, `github_id`, `title`, `author`, `state`; unique on `(repo_id, issue_number)` |
| `issue_revisions` | One row per distinct content hash of an issue — `content_hash`, `title`, `body`, `revision_at`; unique on `(issue_id, content_hash)` |
| `classifications` | One row per classification run — `engine_version`, `config_hash`, `duration_ms`, `outcome`, `used_inference`, `raw_result` |
| `classification_findings` | One row per matched (or unresolved) group — `group_key`, `label_name`, `tier`, `confidence`, `matched_text`, `source_offset` |
| `inference_runs` | LLM tier metadata — `provider`, `model`, `prompt_version`, `mode`, token counts, `cost_estimate`, `latency_ms`, `cache_hit`, `rejected_labels`, `agreed_with_deterministic` |
| `inference_budget` | Daily request/token counters, per repo and global, enforcing the budget caps |
| `feature_flags` | Runtime-togglable flags (including the inference tier), read/write via the private API |
| `applied_labels` | `issue_id`, `label_name`, `group_key`, `applied_by_run_id`, `applied_at`, `removed_at` — the record that makes safe removal possible |
| `processing_errors` | `delivery_id`, `stage`, `exception_type`, `message`, `attempt`, `occurred_at` |
| `stats_daily` | Materialized nightly — `date`, `repo_id`, `group_key`, `label_name`, `issue_count`, `unresolved_count` |

## Why `applied_labels` exists

Reconciliation only removes a label if **(a)** it belongs to a managed group in the effective config, **and** **(b)** `applied_labels` records that *this bot* applied it. Human-applied and unmanaged labels are untouchable — this table is what makes that guarantee enforceable rather than aspirational.

## Why `repository_configs` is append-only

Every classification records the exact `config_hash` that produced it. Because old config versions are never overwritten, any historical classification can be explained by reproducing the exact rules that were active at the time — this is the backbone of the private API's `/v1/issues/{repo}/{number}/classification` explainability endpoint.

## Indexing

Covering indexes on `(repo_id, created_at)`, `(status, received_at)`, `(label_name, date)`, and `(repo_id, is_current)`. Migrations run via DbUp, numbered and idempotent, as a separate CI job — never on application startup.
