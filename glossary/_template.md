---
id: gls-{{slug}}
tier: descriptive
status: draft
owner:
review-by:
tags:
---

# {{Context}}

`Glossary: gls-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](/knowledge-as-code/contributing.md)** — where a document goes, how it is written, and how it
is reviewed. What is below is only what a glossary adds to that.

**One glossary, one bounded context.** The framework itself, a product surface, a system that names things its own way.
Never a topic: a file called "infrastructure terms" starts an argument about placement every time somebody adds a word.

**Frontmatter.** Delete this block once the fields above are filled in.

* **`status`** — `draft` · `active`. `draft` while the terms are still settling.
* **`review-by`** — A quoted date. The whole glossary is reviewed at once.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

**A glossary every corpus shares names a type and cites no record.** Service ownership and citations are this corpus's,
and a corpus that took this file has neither. Where an entry is destined upstream, leave both out and say the sentence
in full instead.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence naming the context these words belong to.

## Scope

Which words this glossary admits, and which neighbouring glossary holds the rest. A term belongs to the most general
glossary that admits it, so say what makes this context different rather than what it contains.

## Terms

### {{Term}}

A one-sentence definition of what the word means in this context.

**Not:** the neighbouring term it is most often confused with, and the difference in a few words.

Owned by [svc-{{a}}]. See [adr-{{a}}].

[adr-{{a}}]: /adrs/{{a}}.md
[svc-{{a}}]: /services/{{a}}.md
