---
id: gls-{{slug}}
type: glossary
tier: descriptive
status: draft
owner:
review-by:
---

# {{Context}}

`Glossary: gls-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a glossary adds to it. Add an
optional field from [the type page](../glossary.md#metadata) where you have a value for it.

**One glossary, one bounded context.** The framework itself, a product surface, a system that names things its own way.
Never a topic: a file called "infrastructure terms" starts an argument about placement every time somebody adds a word.
Name the file for the context, in kebab case.

**Frontmatter**

* **`id`**: `gls-` plus the filename's slug, as `gls-search` in `search.md`.
* **`status`**: write `draft` while the terms are still settling.
* **`narrows`**: the more general glossary this one sits inside. Left empty by the corpus-wide glossary, which nothing
  sits above.
* **`review-by`**: a quoted date. The whole glossary is reviewed at once.

**A glossary every corpus shares names a type and cites no record.** A record citation is this corpus's, and a corpus
that took the file has none. Leave it out of an entry meant to travel upstream.

**The term.** Write it as an H3, singular and in its canonical casing, in alphabetical position. The anchor is the
term's identifier, and a term has no numeric id.

**The definition.** Write one paragraph, usually one or two sentences. Name the class the thing belongs to, then what
sets it apart within that class. Where it needs more, link an explanation for the rest.

**Follow these drafting rules yourself, because nothing runs them.** They come from [ISO/IEC Directives, Part
2](https://www.iso.org/sites/directives/current/part2/index.xhtml), clause 16.5.

* Write one definition per entry.
* Where the definition comes from a document, link that document inside the entry.
* Read the definition back into a sentence that used the term. Check that the sentence still reads correctly.
* Leave out a term a qualified reader already knows. [The type page's Scope](../glossary.md#scope) says which words
  stay out.

**Three labelled lines sit under a definition, and each answers a different question.** `**Also:**` gives the reader
another name for this thing, so a search for an acronym or an older word reaches the entry. `**Avoid:**` gives them a
name for it this glossary has dropped, and says what to write instead. `**Not:**` gives them a different thing this one
is confused with. Write them in that order. Each travels to a consumer under its own key.

**All three are optional.** Leave a line out rather than reach for something to put under it, because everyone who
reads the glossary reads the entry. A `**Not:**` in particular names another thing you can point at: a neighbouring
entry, or a word another context uses differently. It never names a property this thing lacks. Where you find yourself
writing "not everything we would like", the definition is what needs rewriting.

**Point a redefinition at the term, not at the file.** Where another glossary defines the same word differently, the
`**Not:**` line names that entry and lands on it. Write `[gls-other.term]: other.md#term`. A link to `other.md` alone
puts a reader at the top of a glossary and leaves them to find what was meant, and anything reading the corpus can only
carry the reference the link states. Each of the two entries names the other. Where the general glossary is one every
corpus shares, only the narrower entry states the reference, so the shared one still reads correctly in a corpus that
never had the other half.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence naming the context these words belong to.

## Scope

Which words this glossary admits, and which neighbouring glossary holds the rest. A term belongs to the most general
glossary that admits it, so say what makes this context its own rather than listing what it holds.

## Terms

### {{Term}}

A one-paragraph definition of what the word means in this context.

**Also:** the acronym, abbreviation or second word this thing goes by.

**Avoid:** a name for it this glossary has dropped, and what to write instead.

**Not:** the neighbouring term it is most often confused with, and the difference in a few words. See
[gls-{{b}}.{{term}}].

See [adr-{{a}}].

[adr-{{a}}]: ../adrs/{{a}}.md
[gls-{{b}}.{{term}}]: {{b}}.md#{{term}}
