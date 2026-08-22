---
id: dsc-{{slug}}
tier: observed
status: open
source: human
confidence: unverified
expires:
provenance:
applies-to:
promoted-to:
owner:
tags: [ a, b ]
---

# {{What you noticed, in one line}}

`Discovery: dsc-{{slug}}` `OPEN`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md)** — where a document goes, how it is written, and how it
is reviewed. What is below is only what a discovery adds to that.

**Frontmatter**

* **`source`** — `human` · `session` · `dreamed`.
* **`confidence`** — Leave `unverified` unless you have genuinely proven it.
* **`expires`** — A quoted date — 90 days from capture by default.
* **`provenance`** — Required when `source: dreamed`. A reference back to the session and passage it came from, so
  review is a thirty-second check rather than an act of faith.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## What I saw

The observation. Two or three sentences.

## Context

What you were doing when you hit it, and anything about the environment that might be relevant — branch, environment,
version, time of day.

## Why it might matter

A sentence on who this could bite, or what it might indicate. Speculation is fine here; that is what `unverified` is
for.

---

_(**Keep this short.** Discoveries are deliberately low-ceremony — the whole point is that capture costs nothing, so
don't tidy it up, don't verify it first, and don't write it as an [FAQ](../faqs.md). If it turns out to be real, general and
current, a human promotes it and the polish happens then.)_
