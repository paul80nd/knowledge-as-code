# Data

Where data lives, how long we keep it, and how sensitive it is.

**[→ Index](data/INDEX.md)**

## What is a data document?

One per data domain: which entities it covers, which [service](/services) owns them, what store they live in, how
sensitive they are, how long we keep them, and where they flow.

## Why we use them

Two audiences, one document. For anyone building something, it answers the question asked constantly and answered
inconsistently: *where do bookings actually live, and who owns them?* For the [policy](/policies) tier, it is the
evidence — an auditor's first question is what personal data exists and how long it is kept, and the answer should not
require an archaeology exercise.

Recording ownership also surfaces the cases where two services believe they own the same entity, which is a design
problem worth finding on paper.

## Scope

Data documents are **descriptive** — they mirror what is actually stored. They are organised by data domain rather than
by store, because a domain often spans stores and that is the interesting part.

Not the place for:

* **Schema definitions** — those live with the code that owns them.
* **How to query it** — that is a [process](/processes) or a service document.
* **Retention rules as commitments** — the *commitment* is a [policy](/policies); this records the *actual* retention.
  Where they differ, that gap is worth knowing about.

Note the folder is singular — `data/` — because English gives no plural. It is the one exception alongside
[`glossary.md`](/glossary).

## Metadata

<!-- BEGIN GENERATED: schema-data -->

| Field            | Req | Type   | Notes                                                                    |
|------------------|-----|--------|--------------------------------------------------------------------------|
| `status`         | ●   | enum   | `active` · `deprecated`                                                  |
| `owned-by`       | ●   | id     | Service id                                                               |
| `classification` | ●   | enum   | `public` · `internal` · `confidential` · `personal` · `special-category` |
| `retention`      | ●   | string | Required where classification is `personal` or `special-category`        |
| `flows-to`       |     | list   | Service or integration ids                                               |

<!-- END GENERATED: schema-data -->

## Adding a data document

1. Copy [`template.md`](data/template.md) to `<slug>.md`. Data documents use slug ids — `dat-<name>`.
2. Name the entities it covers and the single service that owns them. If two services claim ownership, resolve that
   before writing the document.
3. Classify honestly. Customer names, email addresses and payment histories are `personal`; anything special-category
   needs a lawful basis recorded.
4. State `retention` concretely — "indefinitely" is an answer, and a revealing one.
5. Record `flows-to`: which services and [integrations](/integrations) receive this data. Data leaving the estate is the
   part that matters most.

**Conventions**

* **One owning service per domain.** Shared ownership means nobody is answerable.
* **Never put actual data here** — no sample records, no identifiers, no connection strings.
* **Personal data without a stated retention** is reported by CI. It is the first thing anyone external will ask.

## What CI checks

<!-- BEGIN GENERATED: checks-data -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-data -->
