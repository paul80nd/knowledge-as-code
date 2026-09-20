---
id: dat-{{slug}}
type: data
tier: descriptive
status: active
owned-by:
classification:
personal-data:
data-subjects:
retention:
region:
review-by: "{{date}}"
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
* **`classification`**: `public` · `internal` · `confidential`. How widely the data may be shared, and nothing else.
* **`personal-data`**: `none` · `personal` · `special-category`. A category of data, not a grade of sensitivity. Data
  can be `confidential` and `none`, or `public` and `personal`.
* **`data-subjects`**: required where `personal-data` is anything but `none`. The categories of people the data is
  about, in the corpus's own words.
* **`retention`**: required where `personal-data` is `personal` or `special-category`. "indefinitely" is an answer, and
  a revealing one.
* **`region`**: where the owning service keeps the data, as a cloud region or a place.
* **`flows-to`**: the services and integrations that receive this data. Data leaving the estate is the part that matters
  most.
* **`review-by`**: the day someone confirms the classification and the retention are still right. A year ahead is
  usual. Nothing fails the build when that day passes.

**Fields this template leaves out.** This type takes optional fields the frontmatter above does not carry. Add a key
where you have a value for it, and leave it out where you do not. [The type page](../data.md#metadata) lists every field
and says what each one holds.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence on what this domain covers.

## Purpose

What the estate does with this data, and why it holds it. One purpose per bullet.

For `personal` or `special-category`: name the lawful basis beside the purpose it supports.

## Entities

* **{{Entity}}**: what it represents.
* **{{Entity}}**: …

_(Names and meanings, not schemas. Schemas live with the code that owns them.)_

## Where it lives

|                    |                                           |
|--------------------|-------------------------------------------|
| **Owning service** | [svc-{{a}}]                               |
| **Store**          | {{SQL Server / blob / table storage / …}} |

## Classification

{{classification}}, because {{reason}}.

For `personal` or `special-category`: what personal data is present. `data-subjects` already says who it is about.

## Retention

How long we keep it, what triggers deletion, and whether deletion is actually implemented. If the policy says one thing
and the system does another, record both. That gap is the useful part.

## Flows

| Goes to     | Why | What is shared | Where they process it |
|-------------|-----|----------------|-----------------------|
| [svc-{{a}}] |     |                |                       |
| [int-{{a}}] |     |                |                       |

_(Especially anything crossing outside the estate. `region` covers the owning service alone, so the last column is
where a transfer out of the country is recorded.)_

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
