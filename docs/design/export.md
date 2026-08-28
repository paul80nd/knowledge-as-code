# The export format

`kac export` writes `.dist/export/` for something that will never open a corpus's Markdown. This page is the contract:
what each file holds, what a type decides about its own records, and which version number moves when any of that
changes.

Two readers need it. One declares the `export:` block of a type, meaning one kind of knowledge record, and wants to
know what each key produces. The other writes a consumer against the output and wants to know what it may rely on.
[`export`](../cli/export.md) is the page for running the command.

## What a type declares

**What travels is the type's decision**, written in the `export:` block of its schema file and described key by key in
[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json). The
exporter reads that declaration and nothing else, so a corpus adopting a new type exports it without the tool changing.

Three keys select what a record sends:

* **`fields:`** is a plain list. A field travels whole or not at all.
* **`sections:`** names each section beside a **fidelity**, saying how much of that section travels.
* **`parts:`** names the keys of one part's line, and the fidelity that entry carries.

Neither `sections:` nor `parts:` falls back to a default. A type states the fidelity or the schema pass fails.

A corpus that adopted no exporting type still writes a manifest, with an empty type list.

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

## How a run writes it

A run loads the corpus whole, decides which records travel and what of each one goes with them, then deletes
`.dist/export/` and writes it again.

**The corpus is loaded whole whatever `--type` says.** A narrowed run still resolves ids against every record, because
a question about a set answered from some of its members is answered wrongly. The flag narrows what is written and
never what is read. A type the corpus has not adopted is refused by name.

**An unsettled record travels by default.** A draft record, and one whose `review-by` has passed, are both exported
carrying their own state, so a consumer reads what the corpus actually holds and decides for itself how far to trust
it. Filtering them would make the corpus's own condition invisible downstream.

A corpus may exclude either with `export.exclude:` in [`.corpus.yaml`](../corpus-descriptor.md). Where it does, the run
names every record it withheld, because a record left out of the output cannot be seen there.

**The export is untracked, and rebuilt whole.** `.dist/` is gitignored, so the export is never something to review, and
a tracked one would put a diff nobody reads on every change to the words. Two things follow. The overwrite is
delete-then-write, because a record deleted from the corpus must not leave an entry behind and no diff would show the
orphan. And the manifest describes itself, carrying the commit it was built from and a dirty flag beside it, since git
can say nothing about an export once it has left. A commit on its own would describe a dirty tree as reproducible.

What holds the shape steady in place of a diff is a committed fixture in the tool's own test suite. It exports a corpus
and compares the whole tree file by file, so a corpus running the tool without the tests receives a format already
proved.

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

## The manifest lets a reader choose which file to open

A flat file is read whole and grepped, because a lookup does not know which record holds the term it wants. A record
file is read one at a time, because a reader that has a hit wants the single file behind it. One large file would
charge the second reader the first one's cost.

### Each type carries two counts, named apart

One is how many records it holds and the other how many parts. For a glossary the two differ by an order of magnitude. A
reader sizing the vocabulary wants the parts, and a reader asking how many files it was handed wants the records.

### Each type states the fidelity its sections travelled at

Without that, a summary reaches a consumer looking exactly like a whole section. Fidelity belongs to the type, so each
entry under `types` carries it once.

```json
"sections": { "Purpose": "summary", "Scope": "full", "Exceptions": "full" }
```

### Each type names the two keys that address a part

A part line's keys are the type's own words, so a consumer holding a corpus with a type it never adopted has no schema
to read them from. Two of them are the ones any reader needs: which record a line belongs to, and which part of that
record it is. The type's entry names both.

```json
"partsFile": "policies/clauses.jsonl", "recordKey": "record", "partKey": "part"
```

A consumer assuming a spelling would read a producer's parts as empty wherever that producer chose different words, and
every citation into them would fail for a reason nothing states. Both keys are absent where the type keeps no parts, as
`partsFile` is.

### `about` carries what the corpus says about itself

`kac pack` never loads the corpus and a plugin is assembled from the export rather than from the tree, so the
descriptor's own words reach a registry and a marketplace through the manifest or not at all.

```json
"about": { "displayName": "…", "description": "…", "author": { "name": "…", "url": "…" }, "license": "…" }
```

Every field is `null` where the corpus said nothing, and nothing is filled in from a neighbour's value. An author
nobody named and a licence nobody chose are claims about a person, and a template supplying them is how a corpus comes
to publish under somebody else's name.

### `corpus` names the corpus and `shortcode` cites it

`corpus` is what the corpus calls itself, which tells one export from another. `shortcode` is what a citation writes
before the colon, so a consumer resolving `eng:pol-VURM` knows which of the exports it holds answers it. It is `null`
where the corpus declares none, and [`.corpus.yaml`](../corpus-descriptor.md#identity) is where a corpus declares one.

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

## The link and the ingredients

A person follows a link and an agent fetches a file, and only the first of those is an address the export can write. The
rules joining the base to a path, and the anchor rule for a part, belong to `publishing-target`.
[`.corpus.yaml`](../corpus-descriptor.md#publishing) supplies where the corpus is served from and nothing else.

### An agent is handed ingredients rather than a second URL

An export used to carry a second template, pointing at raw source an agent could fetch without credentials. Only GitHub
ever served that, and only for a public repository. A private GitHub repository never had such a host, and Azure DevOps
has none at all, so the second template was a rule one target could follow and the rest could not.

So the manifest carries `base`, `pathPrefix` and `ref` instead. An agent joins `pathPrefix` ahead of the record's `path`
to reach the file inside the repository, then asks the client that authenticates to `target` for that file at that
`ref`. `gh` does it for GitHub and `az rest` for both Azure DevOps targets. Where the agent has no such client, the
honest answer is to quote the human link and say so, rather than to assemble a URL that returns a sign-in page it will
read as the record.

### The manifest states the human form as a template

The template is `https://…/blob/<sha>/{path}#{anchor}` for GitHub, and its own shape for each other target. A consumer
substitutes the `path` and `anchor` a line carries. Four things follow from writing it this way:

* **The ref is inside the template.** A ref left as a placeholder is forty hex characters copied by an agent, and a
  one-digit slip there is a plausible 404 nobody checks. With the commit already in the string, the worst a substitution
  can produce is a wrong path, which the reader can see and correct.
* **The base is carried beside it, for the fetch and not for the link.** Nobody substitutes into `base` to build an
  address. It is there because a client authenticating to the target needs the organisation, the project and the
  repository, and only the base names them.
* **A corpus that is not its repository names the folder it sits in.** `publishing.path-prefix` lands between the commit
  and the record's path, which is the only place it can go. What a reader supplies is the path alone, so the two cannot
  be joined in the wrong order. It is carried out as `pathPrefix` as well, because an agent fetching a file joins it
  itself.
* **What `{path}` takes is the target's business.** GitHub and Azure Repos address a file and take the record's path
  whole. An Azure DevOps wiki addresses a page, so it takes the same path with `.md` removed and its separators
  percent-encoded.

### One target cannot pin its link to a commit

Every link resolves against the commit the export was built from, so a citation names the version the agent read rather
than whatever the branch holds later. `azure-devops-wiki` is the exception it cannot help: no `?pagePath=` URL takes a
commit. An agent reading that corpus still reads the pinned version, because `ref` reaches it through the manifest.

### A per-record file resolves its links

A per-record file carries its links already resolved, rather than a template and a substitution rule. The commit sits
inside them, so the file rewrites on every export from a new commit whatever its content did.
That churn is bought deliberately: a reader that has already dereferenced one record wants a URL in its hand rather than
a template and a substitution rule.

At a handful of records the cost is a few untracked files. It is worth reopening where a type exports records by the
hundred, because the churn scales with the record count and what it buys does not.

### Five kinds of corpus have no address

One publishing nowhere, one naming a target nothing builds links for, one stating a target but no base, one whose base
is not a URL that target can join to, and one git cannot answer for. Each exports without links, and the manifest
carries the target it was given with nulls beside it, so a consumer sees the absence stated.

A part line is unaffected. `path` and `anchor` are facts about the corpus rather than about where it is published, and
they travel either way.

## Three versions, and none implies another

`formatVersion` covers the envelope: the keys the manifest carries, the layout of the tree, and how a link template is
built. Each entry under `types` carries a `shapeVersion` covering that one type's files. `contentVersion` is
`content-version` from [`.corpus.yaml`](../corpus-descriptor.md), semantically versioned and bumped by hand, and states
what the corpus knows. A corpus can rewrite every definition and move none of the three.

`contentVersion` is the one a consumer depends on by name. [`pack`](../cli/pack.md) publishes the export at that
version, so a corpus that changes what it knows and leaves the number alone has nothing new to release.

### Each type is versioned where its keys are declared

A type declares the keys of its own part line in `export.parts.line:`, so two types exporting parts write different
files. The glossary's terms carry a `definition` and the `not` line beneath them, both read from a term's body. A type
whose parts are table rows has no such body, and names its columns instead.

One number across every type would refuse a bundle over a change nobody's consumer reads. A plugin whose only skill
looks terms up would be refused on the day some other type gained a key.

### A number a reader does not know obliges it to stop

The section above says when the exporter moves a number. This is the other half of the same promise, and it is what a
reader owes in return: meet a `formatVersion` above the one you were written against, and refuse the export rather
than read the parts you recognise. A number moves exactly when a reader written against the shape before it would now
be wrong, so reading on is reading something whose meaning has changed underneath you.

A `shapeVersion` obliges less, and deliberately. It covers one type's files, so a reader that does not know a type's
number leaves that type alone and reads the rest of the export. That is the whole reason the two numbers are separate:
one number across every type would stop a reader over a change to a type it never opens.

[`bundle`](../cli/bundle.md) is the worked case. It reads both and refuses what it cannot read.

### What moves either number

One test decides both. The number moves when a reader written against the shape before it would now be wrong. Adding a
key to a file, or a file to a type's directory, leaves that reader correct and moves nothing. Renaming a key, dropping
one, changing the type of a value, or changing what a key means each turns a correct read into a wrong one.

Reordering the keys within a line moves nothing, because every key is addressed by name. Reordering the records in a
flat file moves the number, because that order carries meaning within a chain.

Which of the two moves follows from which files moved. `formatVersion` went from 1 to 2 when a term line's two resolved
URLs became a `path` and an `anchor`. The same edit today moves the glossary's `shapeVersion`, because a term line is a
glossary's file and no consumer of another type reads it.

It went from 2 to 3 when `rawTemplate` left the manifest and `base` and `pathPrefix` arrived. A record's `links` lost
its `raw` half in the same edit and moved no `shapeVersion`, because that object is written for every type by the
exporter rather than declared by any one type's `export:` block. How a link is built is the envelope's business.

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

[`export`](../cli/export.md) is the command that writes this, and [`pack`](../cli/pack.md) seals it for a consumer.
