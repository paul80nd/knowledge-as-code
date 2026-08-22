---
id: gls-{{slug}}
tier: descriptive
status: draft
owner:
narrows:
review-by:
tags:
---

# {{Context}}

`Glossary: gls-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md)** — where a document goes, how it is written, and how it
is reviewed. What is below is only what a glossary adds to that.

**One glossary, one bounded context.** The framework itself, a product surface, a system that names things its own way.
Never a topic: a file called "infrastructure terms" starts an argument about placement every time somebody adds a word.

**Frontmatter**

* **`status`** — `draft` · `active`. `draft` while the terms are still settling.
* **`narrows`** — The more general glossary this one sits inside. Left empty by the corpus-wide glossary, which nothing
  sits above.
* **`review-by`** — A quoted date. The whole glossary is reviewed at once.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

**A glossary every corpus shares names a type and cites no record.** An owning service and a record citation are this
corpus's, and a corpus that took the file has neither. Leave both out of an entry meant to travel upstream.

**Point a redefinition at the term, not at the file.** Where another glossary defines the same word differently, the
`**Not:**` line names that entry and lands on it — `[gls-other.term]: other.md#term`. A link to `other.md` alone puts a
reader at the top of a glossary and leaves them to find what was meant, and anything reading the corpus can only carry
the reference the link states.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence naming the context these words belong to.

## Scope

Which words this glossary admits, and which neighbouring glossary holds the rest. A term belongs to the most general
glossary that admits it, so say what makes this context its own rather than listing what it holds.

## Terms

### {{Term}}

A one-sentence definition of what the word means in this context.

**Not:** the neighbouring term it is most often confused with, and the difference in a few words — see
[gls-{{b}}.{{term}}].

Owned by [svc-{{a}}]. See [adr-{{a}}].

[adr-{{a}}]: ../adrs/{{a}}.md
[gls-{{b}}.{{term}}]: {{b}}.md#{{term}}
[svc-{{a}}]: ../services/{{a}}.md
