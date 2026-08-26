# The export format

`kac export` writes `.dist/export/` for something that will never open a corpus's Markdown. This page is the contract:
what each file holds, what a type decides about its own records, and which version number moves when any of that
changes.

Two readers need it. One declares the `export:` block of a type, meaning one kind of knowledge record, and wants to
know what each key produces. The other writes a consumer against the output and wants to know what it may rely on.
[`export`](cli/export.md) is the page for running the command.

## What a type declares

**What travels is the type's decision**, written in the `export:` block of its schema file and described key by key in
[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json). The
exporter reads that declaration and nothing else, so a corpus adopting a new type exports it without the tool changing.

Three keys select what a record sends:

* **`fields:`** is a plain list. A field travels whole or not at all.
* **`sections:`** names each section beside a **fidelity**, saying how much of that section travels.
* **`parts:`** names the keys of one part's line, and the fidelity that entry carries.

Neither `sections:` nor `parts:` falls back to a default. A type states the fidelity or the schema pass fails.

A corpus that adopted no exporting type still writes a manifest, with an empty type list. "Nothing" is a valid
statement of what a corpus has.

### What each fidelity carries

Take a policy whose `Exceptions` section runs to two paragraphs. Its record carries one of these three.

| Fidelity    | What the record's `Exceptions` key holds                                                    |
|-------------|---------------------------------------------------------------------------------------------|
| `full`      | `"A team may deviate where the data stays in the region.\n\nRecord it against the clause."` |
| `summary`   | `"A team may deviate where the data stays in the region."`                                  |
| `reference` | `null`                                                                                      |

`full` keeps the wrapped lines joined and the paragraph breaks. `summary` takes the opening paragraph, which is where a
section says what it is about. `reference` leaves a consumer the record's own `path` and `links` to follow.

A section the record never wrote is absent from `sections` altogether. A `null` says the type sent a reference, and an
absent key says nobody wrote the section.

**A `parts:` entry carries `full` alone.** A part line is already a reduction: `line:` names key by key what of a part
travels, so a type wanting a thinner line drops a key from it.

## The tree

```text
.dist/export/
  manifest.json          what this export is, and where it came from
  glossary/
    gls-<name>.json      one record: its declared fields, its declared sections, and its links
    terms.jsonl          every term, one to a line, addressed by path and anchor
  policies/
    pol-<MNEM>.json      the same, for a policy
    clauses.jsonl        every clause, one to a line, carrying the level it binds at
```

The names are read from the schema. A type's directory is its own key, and its flat file is named for what the type
calls one of its parts: `terms.jsonl`, because a glossary's `parts:` block says `noun: term`. Both are fixed once the
type has declared them, because a skill addresses them by name.

## The manifest

A flat file is read whole and grepped, because a lookup does not know which record holds the term it wants. A record
file is read one at a time, because a reader that has a hit wants the single file behind it. One large file would
charge the second reader the first one's cost. The manifest is what lets a reader choose.

### Each type carries two counts, named apart

One is how many records it holds and the other how many parts. For a glossary the two differ by an order of magnitude. A
reader sizing the vocabulary wants the parts, and a reader asking how many files it was handed wants the records.

### It states the fidelity each section travelled at

Without that, a summary reaches a consumer looking exactly like a whole section. Fidelity belongs to the type, so each
entry under `types` carries it once.

```json
"sections": { "Purpose": "summary", "Scope": "full", "Exceptions": "full" }
```

### It carries both of the corpus's names

`corpus` is what the corpus calls itself, which tells one export from another. `shortcode` is what a citation writes
before the colon, so a consumer resolving `eng:pol-VURM` knows which of the exports it holds answers it. It is `null`
where the corpus declares none, and [`.corpus.yaml`](corpus-descriptor.md#identity) is where a corpus declares one.

## The flat file is JSONL

A hit has to hand back something parseable on its own. A matching line of an indented document is a fragment, and the
reader is left seeking outward for its braces.

So each line repeats what a reader would otherwise look up: the record it came from, the state of that record, and its
cross-references as ids. That costs bytes, and it is worth them. The alternative is a hit that sends the reader to the
very file this one exists to save them opening.

### An address is the one thing a line does not repeat

A line carries the record's `path` and the part's `anchor`, and the manifest carries the two templates they go into.

**`part` and `anchor` answer different questions**, and the part source decides whether one string does both. A part's
id is what a citation from elsewhere in the corpus resolves against, where an anchor is what a link's fragment has to
be. A heading's slug is its id and its anchor alike, so every line of a glossary's flat file carries the pair equal.

A table row has no fragment of its own, and its id is authored: a policy clause id is `TIMEBOX`, and no fragment
resolves to that. So a clause line carries the clause id in `part` and the slug of the section holding the table in
`anchor`, and a link built from that line lands on the table.

### A key name may be a word the corpus also defines

`record` and `title` are keys on every line and terms in the framework's own glossary. Every line carries both keys, so
a search for either hands back the whole file and identifies nothing in it. That is a property of the format rather than
of the content, and it holds for any key named with an ordinary English noun. It reaches what a type's `export:` block
may call a field. A reader compensates by reading each hit's `title` before using the hit.

### Order carries meaning within a chain

**Records are ordered roots-by-id, each root's chain depth-first beneath it.** Generality holds within a chain and
nowhere else: `gls-search` narrows `gls-example-libraries`, so a grep for `title` meets the general entry before the one
refining it.

Across unrelated roots the order is stable and says nothing. Reading the first hit as the more general one would give a
reader the wrong domain. `narrows` on the owning records is what tells the two cases apart, and every line names the
record it came from.

**Within a record, the part source decides.** A glossary's terms sort alphabetically. A policy's clauses travel in the
order the table writes them, because that order groups the obligations ahead of the recommendations. Sorting them would
hand a consumer a different policy from the one the page shows.

### A cross-reference is read, never inferred

A `**Not:**` line pointing at another glossary is a link, and a link's target is stripped out of the prose. The export
carries the part it names in `seeAlso`, as `gls-search.title`. It resolves to the term rather than the record, because
`redefinitions-are-reciprocal` is about a term and its counterpart.

A link naming a record and no term inside it leaves nothing to read, so nothing is carried. The obvious guess is the
same word in the other glossary. It is right only for a pair that happens to share a spelling, and silently wrong for a
`Borrower` that redefines a `Patron`. The run names each link it could not read, since an omission in an artefact
nobody reviews is invisible.

## What holds in every file

**Absent is `null`.** Every key writes it that way. A field a record leaves blank and a field it does not carry are one
absence to a consumer, and `""` in one file beside `null` in another would leave that consumer checking which file it
had opened before it could test for nothing.

**Prose arrives unwrapped.** The corpus wraps at 120 columns, which is a fact about the file rather than about the
words, and a grep for a phrase straddling the wrap would find nothing. Blank lines are the author's and stay.

A list, heading, quote, table or fence is left exactly as written, and that is a decision rather than an unfinished
case. A list joined onto one line is destroyed and the reader cannot recover it, where a paragraph left wrapped merely
arrives as it was written. Every doubtful line therefore goes the safe way.

**A link reference definition never travels.** The definitions sit in a block at the foot of a record, which puts them
inside whichever section is written last. They render as nothing on the page, so a section carried as prose ends on the
author's words and a consumer is never handed the paths.

**Output is deterministic.** Ordering is `StringComparer.Ordinal` everywhere but a term line's own position, which
sorts case-insensitively on the term. Two runs from one commit produce identical bytes but for `generatedAt`.

## The two links

A person follows one form and an agent fetches the other. The rules joining a base to a path, and the anchor rule for a
part, belong to `publishing-target`. [`.corpus.yaml`](corpus-descriptor.md#publishing) supplies where the corpus is
served from and nothing else. Every link resolves against the commit the export was built from, so a citation names the
version the agent read rather than whatever the branch holds later.

### The manifest states both forms as templates

The templates are `https://…/blob/<sha>/{path}#{anchor}` and `https://raw…/<sha>/{path}`, and a consumer substitutes the
`path` and `anchor` a line carries. Four things follow from writing them this way:

* **The ref is inside the template.** A ref left as a placeholder is forty hex characters copied by an agent, and a
  one-digit slip there is a plausible 404 nobody checks. With the commit already in the string, the worst a
  substitution can produce is a wrong path, which the reader can see and correct.
* **Only the human template takes an anchor.** Raw source has no fragment to honour, so the asymmetry is a property of
  the templates rather than a rule each reader has to remember.
* **The bases are not carried beside them.** They are in the templates already, and a manifest stating one address
  twice is a manifest that can state it two ways.
* **A corpus that is not its repository names the folder it sits in.** `publishing.path-prefix` lands between the
  commit and the record's path, which is the only place it can go. What a reader supplies is the path alone, so the two
  cannot be joined in the wrong order.

### A per-record file resolves its links

The commit therefore sits inside them, and the file rewrites on every export from a new commit whatever its content did.
That churn is bought deliberately: a reader that has already dereferenced one record wants a URL in its hand rather than
a template and a substitution rule.

At a handful of records the cost is a few untracked files. It is worth reopening where a type exports records by the
hundred, because the churn scales with the record count and what it buys does not.

### Four kinds of corpus have no address

One publishing nowhere, one naming a target nothing builds links for, one stating a target but no bases, and one git
cannot answer for. Each exports without links, and the manifest carries the target it was given with null templates
beside it, so a consumer sees the absence stated. The run says which of the four caused it.

A part line is unaffected. `path` and `anchor` are facts about the corpus rather than about where it is published, and
they travel either way.

## Three versions, and none implies another

`formatVersion` covers the envelope: the keys the manifest carries, the layout of the tree, and how a link template is
built. Each entry under `types` carries a `shapeVersion` covering that one type's files. `contentVersion` is
`content-version` from [`.corpus.yaml`](corpus-descriptor.md), semantically versioned and bumped by hand, and states
what the corpus knows. A corpus can rewrite every definition and move none of the three.

### Each type is versioned where its keys are declared

A type declares the keys of its own part line in `export.parts.line:`, so two types exporting parts write different
files. The glossary's terms carry a `definition` and the `not` line beneath them, both read from a term's body. A type
whose parts are table rows has no such body, and names its columns instead.

One number across every type would refuse a bundle over a change nobody's consumer reads. A plugin whose only skill
looks terms up would be refused on the day some other type gained a key.

### What moves either number

One test decides both. The number moves when a reader written against the shape before it would now be wrong. Adding a
key to a file, or a file to a type's directory, leaves that reader correct and moves nothing. Renaming a key, dropping
one, changing the type of a value, or changing what a key means each turns a correct read into a wrong one.

Reordering the keys within a line moves nothing, because every key is addressed by name. Reordering the records in a
flat file moves the number, because that order carries meaning within a chain.

Which of the two moves follows from which files moved. `formatVersion` went from 1 to 2 when a term line's two resolved
URLs became a `path` and an `anchor`. The same edit today moves the glossary's `shapeVersion`, because a term line is a
glossary's file and no consumer of another type reads it.

## What a type cannot say

**An exported field is a scalar.** `export.fields:` reads each name as one value, so a field the record writes as a
list arrives as `null`. A policy's `aligns-with` is that case, and it stays out of the block for that reason.

**A clause carries no framework alignment.** A policy states one in the `Alignment` cell of the clause it qualifies,
and that reference resolves through the corpus's own `frameworks.md`, which says whether the corpus is obliged to the
framework, self-obligated to it, or borrowing from it. No consumer receives that page, so a mapping carried without it
would say a clause touches `A.8.24` and leave the reader to work out what that commits anyone to.

**A clause carries no `seeAlso`.** A cross-reference is read from the body beneath a part, and a table row has none. So
a clause pointing at another policy reaches a consumer as the id inside its words, and the run does not name that link
as unread. A glossary term is unaffected: its body is everything under the heading.

**A part line carries one fidelity of three.** A section travels at any of `full`, `summary` and `reference`, and a
`parts:` entry travels at `full`. A type declaring either of the others against its parts fails the schema pass, which
is the safe way round: the alternative is an export quietly thinner than the type asked for. The value stays
three-valued so that the first type wanting a shorter line does not force it to be rebuilt.

[`bundle`](cli/bundle.md) is what reads `formatVersion` and each `shapeVersion`, and refuses an export it cannot read.
