---
title: Security
parent: Architecture
nav_order: 3
---

# 🔒 Security
{: .no_toc }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

---

## Webhook verification

Every delivery's `X-Hub-Signature-256` is verified with a fixed-time HMAC-SHA256 comparison over the **raw** request body before anything else happens. The raw bytes are buffered specifically so this check sees exactly what GitHub sent, not a re-serialized version of it.

## Untrusted config parsing

Every vacancy repo's `vagas-labeler.yml` is authored by third-party maintainers on public repos, so it's treated as untrusted input end-to-end:

- Validated against a published JSON Schema before use.
- Hard limits: file ≤ 128 KB, ≤ 50 groups, ≤ 200 labels, ≤ 50 words per label, ≤ 20 patterns per label, pattern length ≤ 200 chars.
- **Every regex is compiled with `RegexOptions.NonBacktracking` and a hard match timeout.** A pattern the engine can't compile non-backtracking is rejected outright — this is a direct ReDoS mitigation, not an afterthought.
- YAML alias/anchor expansion is bounded (billion-laughs guard).
- If the file is invalid, the **last known-good config** keeps serving; if there is none, the system falls back to the 1:1 default. The bot never crashes and never partially applies a broken config.

## LLM prompt injection

Issue bodies are written by anonymous third parties on public repos and will eventually contain "ignore previous instructions" or similar. The inference tier treats this as an expected, not exceptional, threat:

- The issue body is wrapped in explicit delimiters and the model is instructed that it is data, never instructions.
- **The model's output is validated against an allow-list of candidate labels for the unresolved group.** Any label outside that allow-list is discarded silently. A model that returns garbage can only ever result in *no* label — never a wrong or invented one.
- Structured output (`temperature: 0`, small `max_tokens`) is parsed strictly; a parse failure is a discarded result, not an exception that propagates.
- A hard timeout plus a circuit breaker mean an LLM outage or slowdown never blocks the pipeline or DLQs a message — the deterministic result is used as-is on any failure.

## Least privilege

The GitHub App requests only `Issues: Read & write`, `Metadata: Read`, and `Contents: Read` (the last one specifically to fetch each repo's config file), subscribed to `Issues`, `Label`, `Push`, `Installation`, and `Installation repositories` events — nothing broader.

The private API (`ApiBr.Vagas.Api`) is never exposed publicly; it sits behind an NGINX IP allow-list plus an API key or JWT.

## Data handling

- Parameterized SQL only.
- Raw webhook payload retention policy: purge after ~180 days, since vacancy bodies routinely contain contact details (emails, phone numbers).
- Obvious PII (emails, phone numbers) is redacted from text before it's ever sent to a third-party inference provider.
- Enabling the inference tier sends public issue content to an external service — this is documented explicitly, not buried, since it's a real data-handling decision maintainers should be able to see plainly.

## Secrets

`GH_APP_PRIVATE_KEY`, `GH_WEBHOOK_SECRET`, `INFERENCE_API_KEY`, and deploy credentials live in a secret store — never in the repository or a container image. Because the repository itself is public, CI logs are audited for accidental secret leakage and configuration is never echoed verbatim into logs.
