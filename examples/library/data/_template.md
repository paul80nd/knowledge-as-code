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

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a data document adds to it. Add
an optional field from [the type page](../data.md#metadata) where you have a value for it.

**Frontmatter**

* **`owned-by`**: a single service id. Shared ownership means nobody is answerable, so resolve it before writing the
  document.
* **`classification`**: how widely you may share the data, and nothing else about it.
* **`personal-data`**: a category of data, not a grade of sensitivity. Data can be `confidential` and `none`, or
  `public` and `personal`.
* **`data-subjects`**: required where `personal-data` is anything but `none`. The categories of people the data is
  about, in the corpus's own words.
* **`retention`**: required where `personal-data` is `personal` or `special-category`. "indefinitely" is an answer, and
  a revealing one.
* **`region`**: where the owning service keeps the data, as a cloud region or a place.
* **`flows-to`**: the services and integrations that receive this data. Data leaving the estate is the part that matters
  most.
* **`review-by`**: the day someone confirms the classification and the retention are still right. A year ahead is
  usual. `classification-in-date` warns once that day passes, and a warning leaves the build green.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence on what this domain covers.

## Purpose

What the estate does with this data, and why it holds it. One purpose per bullet.

For `personal` or `special-category`: name the lawful basis beside the purpose it supports.

## Entities

* **{{Entity}}**: what it represents.
* **{{Entity}}**: …

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
| [itg-{{a}}] |     |                |                       |

_(Especially anything crossing outside the estate. `region` covers the owning service alone, so the last column is
where a transfer out of the country is recorded.)_

## Related

* [adr-{{a}}] decides where this data lives.

[adr-{{a}}]: ../adrs/{{a}}.md
[itg-{{a}}]: ../integrations/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
