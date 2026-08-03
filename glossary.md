# Glossary

The words we use, and what we mean by them.

## What is the glossary?

The ubiquitous language of the domain — one entry per term whose meaning is specific to us, or which is easily
confused with a neighbouring term.

Unlike every other type, this is a **single document rather than a collection**. It is meant to be read end to end, and
it carries its own frontmatter as one descriptive document; there are no per-term files and no per-term metadata.

## Why we use it

It is the highest value-per-byte content in the wiki, and the only document included *in full* in the generated rules
digest that every session loads.

The reason is specific. The terms particular to the domain are often not interchangeable, and neighbouring terms are
easily confused. A contributor — human or agent — who doesn't know the distinctions will produce work that is
plausible, confident and subtly wrong, in code and in documentation alike. Every other document in this wiki assumes
these terms mean something precise; this is where that precision lives.

## Scope

A term belongs here if it is **specific to the domain, or easily confused with something else**. General industry vocabulary does
not — we are not writing a dictionary, and every entry costs digest budget.

Not the place for:

* **A component** — that is a [service](/services). The glossary may define the *concept* the service is named after.
* **A rule about using the term** — that is a [standard](/standards).
* **A full explanation of a pattern** — that is an [explanation](/explanations). A glossary entry is a sentence, and
  links out for the rest.

## Terms

_(One entry per term, alphabetical, flat — no A–Z subheadings. Each is an H3, singular, in its canonical casing:
a one-sentence definition, an optional `**Not:**` line naming what it is confused with, and links out where the detail
lives. One paragraph maximum — the digest carries this whole file.)_

### Example term

A one-sentence definition of what this means in the domain.

**Not:** the neighbouring term it is most often confused with, and the difference in a few words.

Owned by [svc-example](/services/example.md). See [ADR-NNNN].

## Adding a term

1. Add an H3 in alphabetical position. Do not create a file — this type has no folder.
2. One sentence of definition. If it needs a paragraph, the paragraph belongs in an
   [explanation](/explanations) and the entry links to it.
3. Add a `**Not:**` line wherever confusion is plausible. Those lines are the most useful content here.
4. Name the owning [service](/services) where the concept has one.

**Conventions**

* **Cross-references use the heading anchor** — `[tenant](/glossary#tenant)`. The anchor is the term's identifier;
  there are no numeric ids.
* **Terms are singular and in canonical casing.** `Term`, not `terms`.
* **Keep it tight.** CI enforces a budget on the generated digest, and this file is carried in full.

## What CI checks

<!-- BEGIN GENERATED: checks-glossary -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-glossary -->
