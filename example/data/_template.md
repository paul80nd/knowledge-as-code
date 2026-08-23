---
id: dat-{{slug}}
tier: descriptive
status: active
owned-by:
classification:
retention:
flows-to:
owner:
tags: [ a, b ]
---

# {{Data domain}}

`Data: dat-{{slug}}` `ACTIVE`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a data document adds to that.

**Frontmatter**

* **`owned-by`**: a single service id. Shared ownership means nobody is answerable, so resolve it before writing the
  document.
* **`classification`**: `public` · `internal` · `confidential` · `personal` · `special-category`.
* **`retention`**: required where classification is `personal` or `special-category`. "indefinitely" is an answer, and
  a revealing one.
* **`flows-to`**: the services and integrations that receive this data. Data leaving the estate is the part that
  matters most.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence on what this domain covers.

## Entities

* **{{Entity}}**: what it represents.
* **{{Entity}}**: …

_(Names and meanings, not schemas. Schemas live with the code that owns them.)_

## Where it lives

|                    |                                           |
|--------------------|-------------------------------------------|
| **Owning service** | [svc-{{a}}]                               |
| **Store**          | {{SQL Server / blob / table storage / …}} |
| **Region**         |                                           |

## Classification

{{classification}}, because {{reason}}.

For `personal` or `special-category`: what personal data is present, and the lawful basis for holding it.

## Retention

How long we keep it, what triggers deletion, and whether deletion is actually implemented. If the policy says one thing
and the system does another, record both. That gap is the useful part.

## Flows

| Goes to     | Why | What is shared |
|-------------|-----|----------------|
| [svc-{{a}}] |     |                |
| [int-{{a}}] |     |                |

_(Especially anything crossing outside the estate.)_

## Related

* [pol-{{a}}] governs this classification.
* [adr-{{a}}] decides where this data lives.

---

_(**Never put actual data here**: no sample records, no identifiers, no connection strings. This corpus is broadly
readable.)_

[adr-{{a}}]: ../adrs/{{a}}.md
[int-{{a}}]: ../integrations/{{a}}.md
[pol-{{a}}]: ../policies/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
