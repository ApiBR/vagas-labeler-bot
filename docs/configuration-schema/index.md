---
title: Configuration Schema
nav_order: 4
permalink: /configuration-schema/
---

# 📝 Configuration Schema
{: .no_toc }

The word → label mapping that drives classification for a single vacancy repository — owned and edited by that repo's own maintainers, not by this bot's operators.

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

---

## Discovery

The bot looks for a config file at the **root of the vacancy repository**, on the default branch, in this order — first hit wins:

1. `vagas-labeler.yml`
2. `vagas-labeler.yaml`
3. `.vagas-labeler.yml`
4. `.github/vagas-labeler.yml`

Fetched via the GitHub Contents API with the installation token and an `If-None-Match` ETag, cached with a 15-minute TTL plus event-driven invalidation whenever a `push` to the default branch touches a candidate path.

## Fallback: default 1:1 match

If no config file exists, or the file fails validation, the bot falls back to an implicit taxonomy derived from the repository's own existing labels:

- Every label in the repo becomes its own matcher — the label's **name** is the only search term.
- Matching is normalized: diacritic-insensitive, case-insensitive, whole-word, punctuation-tolerant (`front-end` also matches `front end` / `frontend`).
- **Prefix grouping:** if label names use a `key: value` or `key/value` convention (e.g. `senioridade: pleno`), those labels are grouped by prefix for reporting purposes.
- Numeric/range extraction (salary) is disabled in 1:1 mode — it can't be inferred from label names alone.
- The LLM tier is not invoked in 1:1 mode unless explicitly enabled, since there's no `required` group to leave unresolved.

## Minimal form

What most repos need — just the word → label mapping:

```yaml
version: 1

# label name (must already exist in this repo)  ->  words that imply it
mappings:
  "remoto":       [remote, "home office", teletrabalho, "100% remoto", anywhere]
  "presencial":   [on-site, onsite, "no escritório", presencial]
  "híbrido":      [hybrid, hibrido, "modelo híbrido"]
  "júnior":       [junior, jr, trainee, iniciante, "entry level"]
  "pleno":        [mid, "mid-level", pl, intermediate]
  "sênior":       [senior, sr, senior-level]
  "clt":          [clt, "carteira assinada", efetivo]
  "pj":           [pj, "pessoa jurídica", contractor, "recibos verdes"]
```

## Extended form

Adds grouping, regex, negations, field hints, and salary ranges:

```yaml
version: 1
locale: pt-BR
currency: BRL          # fallback only — the salary extractor detects R$/€ from the text itself
salaryPeriod: monthly   # monthly | annual | hourly

options:
  reclassifyOnConfigChange: true   # off by default — opt in per repo
  minConfidence: 0.6
  unresolvedLabel: null   # e.g. "precisa-revisão" — applied when a required group is empty
  llm:
    enabled: false        # per-repo opt-in; the global flag must ALSO be on
    mode: on-gap           # on-gap | always | shadow

groups:
  - key: senioridade
    exclusive: false        # default — see "Why groups are non-exclusive by default" below
    required: true          # if unresolved, the LLM tier may be consulted
    fieldAliases: [senioridade, nível, seniority, level]
    labels:
      - name: "júnior"
        words: [junior, jr, trainee, iniciante]
        patterns: ['\bj[uú]nior\b', '\bjr\.?\b']
      - name: "pleno"
        words: [mid, "mid-level", pl]
      - name: "sênior"
        words: [senior, sr]
      - name: "especialista"
        words: [specialist, staff, principal, "tech lead"]

  - key: modalidade
    exclusive: false
    required: true
    fieldAliases: [modalidade, local, localização, regime, modality]
    labels:
      - name: "remoto"
        words: [remote, "home office", teletrabalho]
        negations: ['n[aã]o\s+[ée]?\s*remoto', 'sem\s+remoto', 'remoto\s*[:=]\s*n[aã]o']
      - name: "presencial"
        words: [on-site, onsite, presencial]
      - name: "híbrido"
        words: [hybrid, hibrido]

  - key: salario
    exclusive: true
    kind: range          # handled by the numeric extractor, not word matching
    fieldAliases: [salário, remuneração, faixa salarial, salary, compensation]
    bucketOn: midpoint     # midpoint | min | max | overlap
    buckets:
      - { name: "até 5k",  max: 5000 }
      - { name: "5k-10k",  min: 5000,  max: 10000 }
      - { name: "10k-15k", min: 10000, max: 15000 }
      - { name: "15k+",    min: 15000 }
      - { name: "a combinar", undisclosed: true }
```

{: .note }
`backend-pt` / `frontend-pt` use `locale: pt-PT` and Portugal-specific vocabulary (`recibos verdes`, `teletrabalho`, `estágio profissional`); `dotnetdevbr` / `nodejsdevbr` use Brazilian terms (`CLT`, `PJ`, `Estágio`, `Cooperado`). The four repos do not share a label set — don't assume one config fits all.

## Why groups are non-exclusive by default

The vacancy repos' own issue templates explicitly tell posters they can leave multiple labels per group — including seniority: *"retire os labels que não fazem sentido à vaga, pode deixar quantas quiser em cada grupo sem problemas"* ("remove the labels that don't apply, leave as many as you want in each group, no problem"). Real issues legitimately carry both `Sênior` and `Especialista`, or a title like "Pleno/Sênior." The config schema defaults every group to non-exclusive (union all matches above `minConfidence`) to match that real, intentional pattern. A maintainer can still set `exclusive: true` on a specific group if they want stricter at-most-one behavior.

## Validation

The YAML is authored by third-party maintainers on public repos, so it's treated as untrusted input:

- Enforced against a published JSON Schema, with `# yaml-language-server: $schema=` for editor autocomplete.
- Limits: file ≤ 128 KB, ≤ 50 groups, ≤ 200 labels, ≤ 50 words per label, ≤ 20 patterns per label, pattern length ≤ 200 chars.
- Every regex is compiled with `RegexOptions.NonBacktracking` and a hard match timeout — a ReDoS mitigation, not a nice-to-have.
- Every `name` must resolve to a label that actually exists in the repo; unknown names are dropped with a warning, never created.
- Duplicate label names across exclusive groups is a validation error.
- YAML alias/anchor expansion is bounded.
- If the file is invalid, the **last known-good config** keeps serving; if there is none, the system falls back to the 1:1 default.

## Text normalization and extraction tiers

See [Architecture → Overview]({{ site.baseurl }}/architecture/overview/#the-classification-tiers) for the full tier table. Normalization runs once and feeds every extractor: strip HTML comments, fenced code blocks, images, and blockquotes; unwrap markdown links to their text; collapse whitespace; NFD-decompose and strip diacritics for matching while keeping the original for display; lowercase. A token-offset map lets every extractor report *where* a match came from.

## Salary extraction

Its own dedicated extractor handles `R$ 5.000,00 – R$ 8.000`, `5k a 8k`, `€40.000/ano`, `até 12 mil`, hourly rates, `a combinar` / `a negociar` / `não informado`, and both `.`/`,` decimal conventions. It normalizes to the config's currency and period — falling back to text-detected currency (R$/€) when the issue's actual currency differs from the repo's configured default — then buckets per `bucketOn`. Sanity bounds reject non-salaries (a stray `2024` is a year, not a salary).

## Maintainer tooling

- **This page** is the public spec for vacancy-repo maintainers — copy the minimal-form example above as a starter.
- `tools/ConfigLinter` — `dotnet run -- validate ./vagas-labeler.yml --repo owner/name` prints errors and shows which labels would match a sample body.
- `.github/actions/validate-config` — a reusable composite Action any vacancy repo can call to lint its own YAML on PR.
