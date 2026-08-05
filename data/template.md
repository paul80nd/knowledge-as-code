---
id: dat-<slug>
tier: descriptive
status: active
owned-by:
classification:
retention:
flows-to:
owner:
tags:
  - a
  - b
---

# <Data domain>

_(Frontmatter notes — delete this block. **`owned-by`** is a single service id — shared ownership means nobody is
answerable, so resolve it before writing the document. **`classification`**: `public` · `internal` · `confidential` ·
`personal` · `special-category`. **`retention`** is required where classification is `personal` or `special-category`,
and "indefinitely" is an answer — a revealing one. **`flows-to`** lists the services and integrations that receive this
data; data leaving the estate is the part that matters most.)_

One sentence on what this domain covers.

## Entities

* **<Entity>** — what it represents.
* **<Entity>** — …

_(Names and meanings, not schemas. Schemas live with the code that owns them.)_

## Where it lives

|                    |                                         |
|--------------------|-----------------------------------------|
| **Owning service** | [svc-example](/services/example.md)     |
| **Store**          | <SQL Server / blob / table storage / …> |
| **Region**         |                                         |

## Classification

<classification>, because <reason>.

For `personal` or `special-category`: what personal data is present, and the lawful basis for holding it.

## Retention

How long we keep it, what triggers deletion, and whether deletion is actually implemented. If the policy says one thing
and the system does another, record both — that gap is the useful part.

## Flows

| Goes to                                 | Why | What is shared |
|-----------------------------------------|-----|----------------|
| [svc-example](/services/example.md)     |     |                |
| [int-example](/integrations/example.md) |     |                |

_(Especially anything crossing outside the estate.)_

## Related

* [pol-XXXX](/policies/…) — the policy governing this classification.
* [ADR-NNNN] — decisions about where this data lives.

---

_(**Never put actual data here** — no sample records, no identifiers, no connection strings. This wiki is broadly
readable.)_

[ADR-NNNN]: /adrs/NNNN-kebab-slug.md
