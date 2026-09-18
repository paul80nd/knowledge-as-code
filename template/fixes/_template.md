---
id: fix-{{nnnn}}
type: fix
tier: normative
status: active
symptom-keywords:
verified:
review-by:
owner:
tags: [ a, b ]
---

# {{The symptom, as you would encounter it}}

`Fix: fix-{{nnnn}}` `ACTIVE`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a fix adds to that.

**Frontmatter**

Name the title and the filename for the **symptom**, not the cause. That is what people search for.

* **`symptom-keywords`**: the literal error text, the service names, and the words someone would type who does not yet
  know what is wrong. This is the field that makes the document findable, so over-fill it.
* **`verified`**: who has checked this fix, one line per actor, oldest first. A `draft` leaves the key out. Every
  other status needs at least one line.

  ```yaml
  verified:
    - { at: 2025-03-11T09:00:00Z, by: symptom-sweep/1.4.0 }
    - { at: 2026-09-07T20:18:00Z, by: human:alex.doe }
  ```

  `at` is a moment in UTC, to the second, and written unquoted. `by` is a person as `human:alex.doe`, or an agent
  named with its version the way a tool names itself. An agent that reproduced the symptom and ran the resolution
  belongs here. A `role:` does not: a post cannot read an answer. Write an actor once: where somebody checks the
  answer again, move the `at` on the line they have, because git keeps every earlier state of the file.

  Read the list to see how far the fix has been taken on trust. Agents alone leave it machine-confirmed, and one
  `human:` line makes it human-reviewed.
* **`status`**: `active` · `draft` · `superseded` · `fixed-upstream`. Write `draft` where nobody has verified the
  resolution yet.

**Fields this template leaves out.** This type takes optional fields the frontmatter above does not carry. Add a key
where you have a value for it, and leave it out where you do not. [The type page](../fixes.md#metadata) lists every
field and says what each one holds.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## Symptom

What you see. Quote the error message verbatim where there is one, including the parts that look like noise. That is
what someone will paste into a search.

## Environment

What you were running when it happened. Name the tool, the operating system and the version, as precisely as you can.
A reader matches on this to tell whether the fix is theirs.

## Cause

What is actually happening underneath, in a sentence or two.

## Resolution

1. {{Step.}}
2. {{Step.}}

How to check it worked.

## Why it happens

The underlying reason, briefly. Enough that a reader can recognise the next variant of this problem rather than only
this exact instance.

_(If the honest answer is "because of a design flaw nobody has fixed", say so and link to where that is tracked. A fix
is not a place to park unowned work, but it is a fine place to point at it.)_

## How we found it

The diagnostic route, not just the destination. This is often more reusable than the resolution itself: the next problem
will be different, but the way in may be the same.

## Related

* [svc-{{a}}] is the service affected.

[svc-{{a}}]: ../services/{{a}}.md
