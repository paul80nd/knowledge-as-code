---
id: faq-{{nnnn}}
tier: normative
status: active
symptom-keywords:
applies-to:
promoted-from:
confirmed-by:
confirmed-on:
review-by:
owner:
tags: [ a, b ]
---

# {{The symptom, as you would encounter it}}

`FAQ: faq-{{nnnn}}` `ACTIVE`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](/knowledge-as-code/contributing.md)** — where a document goes, how it is written, and how it
is reviewed. What is below is only what an FAQ adds to that.

**Frontmatter.** Delete this block once the fields above are filled in.

Name the title and the filename for the **symptom**, not the cause — that is what people search for.

* **`symptom-keywords`** — Be generous: the literal error text, the service names, and the words someone would type who
  doesn't yet know what is wrong. This is the field that makes the document findable, so over-fill it.
* **`confirmed-by`** — A named person.
* **`confirmed-on`** — A real quoted date. An FAQ nobody confirmed is a [discovery](/discoveries).
* **`status`** — `active` · `superseded` · `fixed-upstream`.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## Symptom

What you see. Quote the error message verbatim where there is one, including the parts that look like noise — that is
what someone will paste into a search.

## Cause

What is actually happening underneath, in a sentence or two.

## Fix

1. {{Step.}}
2. {{Step.}}

How to confirm it worked.

## Why it happens

The underlying reason, briefly. Enough that a reader can recognise the next variant of this problem rather than only
this exact instance.

_(If the honest answer is "because of a design flaw nobody has fixed", say so and link to where that is tracked. An FAQ
is not a place to park unowned work, but it is a fine place to point at it.)_

## How we found it

The diagnostic route, not just the destination. This is often more reusable than the fix itself — the next problem will
be different, but the way in may be the same.

## Related

* [svc-{{a}}] — the service affected.
* [dsc-{{a}}] — the observation this was promoted from.

[dsc-{{a}}]: /discoveries/{{a}}.md
[svc-{{a}}]: /services/{{a}}.md
