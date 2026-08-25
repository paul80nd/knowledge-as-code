# `export` write the corpus out as data a consumer can read

<!-- BEGIN GENERATED: usage-export -->

```text
kac export [--no-color] [--type <TYPE>]
```

| Option          | What it does                                                |
|-----------------|-------------------------------------------------------------|
| `--no-color`    | Turn colour off. NO_COLOR in the environment does the same. |
| `--type <TYPE>` | Export one type rather than every type that contributes.    |

<!-- END GENERATED: usage-export -->

## What it is for

A consumer of a corpus should not clone it. `export` writes what the corpus knows into `.dist/export/` as data built for
an agent to read. It writes three kinds of file: a manifest saying what the export is, one file per record for a reader
that wants a whole record, and a flat file cheap to grep for a reader holding only a word. What travels is each type's
own decision, declared beside the type. The command carries no list of its own, so a corpus adopting a new type exports
it without the tool changing.

## What it is not

**It is not `bundle`.** `export` produces data. [`bundle`](bundle.md) assembles that data and the `.plugin/` tree into
something a consumer can install. Nothing here trims components, packages a plugin or publishes anything, and nothing
here knows a bundle exists. `bundle` reads what `export` wrote, and `export` reads nothing a bundle holds.

**It is not [`generate`](generate.md).** `generate` writes into the corpus, for a person reading the corpus. `export`
writes outside it, for something that will never open the Markdown. Both are built from the same frontmatter, and
neither is derivable from the other, because they answer to different readers.

**It is not a backup.** A record travels as the fields and sections its type declared, so a corpus cannot be rebuilt
from an export of it. The direction is one way: `.dist/export/` is rebuilt whole from the corpus.

## How it works

A run loads the corpus whole, then decides which records travel and what of each one goes with them. It deletes
`.dist/export/` and writes it again: a manifest, one file per record, and one flat file per type. Every link it writes
resolves against the commit it was built from.

### What travels

**What travels is the type's decision**, declared in its `export:` block and described key by key in
[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json). The
exporter reads that declaration and nothing else. A corpus that adopted no exporting type still writes a manifest, with
an empty type list. "Nothing" is a valid statement of what a corpus has. Every section and the parts entry names a
**fidelity** beside the piece it selects, saying how much of that piece travels, and neither falls back to one.
`fields:` is a plain list: a field travels whole or not at all.

#### What each fidelity carries

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

**An `export.parts:` entry carries `full` alone.** A part line is already a reduction: `line:` names key by key what of
a part travels, so a type wanting a thinner line drops a key from it.

**An unsettled record travels by default.** A draft glossary, and one whose `review-by` has passed, are both exported
carrying their own state. Filtering them would make the corpus's own condition invisible downstream. A corpus may
exclude either with `export.exclude:` in `.corpus.yaml`. Where it does, the run names every record it withheld, because
a record left out of the output cannot be seen there.

**`--type` narrows what is written and never what is read.** The corpus is loaded whole whatever the flag says, so ids
resolve against every record. A narrowed run would otherwise resolve them against the handful it happened to want. A
question about the set, answered from some of its members, is answered wrongly. A type the corpus has not adopted is
refused by name.

### What a run writes

```
.dist/export/
  manifest.json          what this export is, and where it came from
  glossary/
    gls-<name>.json      one record: its declared fields, its declared sections, and its links
    terms.jsonl          every term, one to a line, addressed by path and anchor
  policies/
    pol-<MNEM>.json      the same, for a policy
    clauses.jsonl        every clause, one to a line, carrying the level it binds at
```

That tree is one corpus's, and the names in it are read from the schema. A type's directory is its own key. Its flat
file is named for what the type calls one of its parts: `terms.jsonl`, because a glossary's `parts:` block says
`noun: term`. Both are fixed once the type has declared them, because a skill addresses them by name.

### The export is untracked

`.dist/` is gitignored and the export inside it is rebuilt whole, so it is never something to review. A tracked export
would put a diff nobody reads on every change to the words, restating what the corpus already holds.

Two things follow. The overwrite is delete-then-write, because a record deleted from the corpus must not leave an entry
behind and no diff would show the orphan. And the manifest has to describe itself, since git can say nothing about an
export once it has left. It carries the commit it was built from, and a dirty flag beside it. A commit on its own would
describe a dirty tree as reproducible.

What holds the shape steady in place of a diff is a committed fixture in the framework's own test suite. It exports a
corpus and compares the whole tree file by file. So a corpus that runs the tool without the tests receives a format
already proved.

### The manifest makes the tree usable two ways

A flat file is read whole and grepped, because a lookup does not know which record holds the term it wants. A record
file is read one at a time, because a reader that has a hit wants the single file behind it. One large file would charge
the second reader the first one's cost. A bare tree of files would leave a reader nothing to orient on: which types are
here, how many records and parts each holds, and where the flat file for one of them sits. The manifest answers that
first, so a reader can choose.

**Each type in the manifest carries two counts, named apart.** One is how many records it holds and the other how many
parts. For a glossary the two differ by an order of magnitude. A reader sizing the vocabulary wants the parts. A reader
asking how many files it was handed wants the records. One number would be read as either.

**The manifest states the fidelity each section travelled at.** Without it a summary reaches a consumer looking exactly
like a whole section. It belongs to the type rather than to a record, so each entry under `types` carries it once.

```json
"sections": { "Purpose": "summary", "Scope": "full", "Exceptions": "full" }
```

**The manifest carries both of the corpus's names.** `corpus` is what the corpus calls itself, which tells one export
from another. `shortcode` is what a citation writes before the colon, so a consumer resolving `eng:pol-VURM` knows which
of the exports it holds answers it. It is `null` where the corpus declares none, and
[`.corpus.yaml`](../corpus-descriptor.md#identity) is where a corpus declares one.

### The flat file is JSONL because it exists to be grepped

A hit has to hand back something parseable on its own. A matching line of an indented document is a fragment, and the
reader is left seeking outward for its braces.

So each line repeats what a reader would otherwise look up: the record it came from, the state of that record, and its
cross-references as ids. That costs bytes. It is worth them, because the alternative is a hit that sends the reader to
the very file this one exists to save them opening.

An address is the one thing a line does not repeat. It carries the record's `path` and the part's `anchor`, and the
manifest carries the two templates they go into.

**`part` and `anchor` answer different questions, and the part source decides whether one string does both.** A part's
id is what a citation from elsewhere in the corpus resolves against. An anchor is what a link's fragment has to be. A
heading's slug is its id and its anchor alike, so every line of a glossary's flat file carries the pair equal. A table
row has no fragment of its own, and its id is authored: a policy clause id is `TIMEBOX`, and no fragment resolves to
that. So a clause line carries the clause id in `part` and the slug of the section holding the table in `anchor`, and a
link built from that line lands on the table.

### A key name is a word the corpus may also define

`record` and `title` are keys on every line and terms in this corpus's own glossary. Every line carries both keys, so a
search for either hands back the whole file and identifies nothing in it. That is a property of the format rather than
of the content, and it holds for any key named with an ordinary English noun. That constraint reaches what a type's
`export:` block may call a field. The skill compensates by reading each hit's `title` before it uses the hit. A line
defines a term when its `title` says so, never because it matched.

### Order carries meaning within a chain

**Records are ordered roots-by-id, each root's chain depth-first beneath it.** Generality holds **within a chain** and
nowhere else: `gls-search` narrows `gls-example-libraries`, so a grep for `title` meets the general entry before the
one refining it. Across unrelated roots the order is stable and says nothing: `record` is defined by
`gls-example-libraries` and `gls-knowledge-as-code`, and neither narrows the other.
Reading the first hit as the more general one would give a reader the wrong domain. `narrows` on the owning records is
what tells the two cases apart, and every line names the record it came from.

**Within a record, the part source decides.** A glossary's terms sort alphabetically. A policy's clauses travel in the
order the table writes them, because that order groups the obligations ahead of the recommendations. Sorting them would
hand a consumer a different policy from the one the page shows.

### A cross-reference is read, never inferred

A `**Not:**` line pointing at another glossary is a link, and a link's target is stripped out of the prose. The export
therefore carries the part it names in `seeAlso`, as
`gls-search.title`. It resolves to the term rather than the record, because `redefinitions-are-reciprocal` is about a
term and its counterpart.

A link naming a record and no term inside it leaves nothing to read, so nothing is carried. The obvious guess is the
same word in the other glossary. It is right only for a pair that happens to share a spelling, and silently wrong for a
`Borrower` that redefines a `Patron`. The run therefore names each link it could not read, since an omission in an
artefact nobody reviews is invisible.

### What holds in every file

**Absent is `null`.** Every key writes it that way. A field a record leaves blank and a field it does not carry are one
absence to a consumer. Writing `""` in one file beside `null` in another would leave that consumer checking which file
it had opened before it could test for nothing.

**Prose arrives unwrapped.** The corpus wraps at 120 columns, which is a fact about the file rather than about the
words. A grep for a phrase straddling the wrap would find nothing. Blank lines are the author's and stay. A list,
heading, quote, table or fence is left exactly as written. That last part is a decision rather than an unfinished case,
because the two mistakes do not cost the same. A list joined onto one line is destroyed, and the reader cannot recover
it. A paragraph left wrapped merely arrives as it was written. Every doubtful line therefore goes the safe way, and a
corpus whose sections happen to hold only paragraphs today is not a reason to narrow it.

**A link reference definition never travels.** The definitions sit in a block at the foot of a record, which puts them
inside whichever section is written last. They render as nothing on the page, so a section carried as prose ends on the
author's words and a consumer is never handed the paths.

**Output is deterministic.** Ordering is `StringComparer.Ordinal` everywhere but a term line's own position, which sorts
case-insensitively on the term, and every value that varies between two runs is confined to the manifest. Two runs from
one commit produce identical bytes but for
`generatedAt`.

### The two links

#### A person follows one form and an agent fetches the other

The rules joining a base to a path, and the anchor rule for a part, belong to `publishing-target` and live in
`Publishing`. `.corpus.yaml` supplies where the corpus is served from and nothing else. Every link resolves against the
commit the export was built from. So a citation names the version the agent read rather than whatever the branch holds
later.

#### The manifest states both forms as templates

A per-record file resolves them. The templates are `https://…/blob/<sha>/{path}#{anchor}` and
`https://raw…/<sha>/{path}`, and a consumer substitutes the `path` and `anchor` a term line carries. Four things follow
from writing them this way:

* **The ref is inside the template.** A ref left as a placeholder is forty hex characters copied by an agent. A
  one-digit slip there is a plausible 404 nobody checks. With the commit already in the string, the worst a substitution
  can produce is a wrong path, which the reader can see and correct.
* **Only the human template takes an anchor.** Raw source has no fragment to honour, so the asymmetry is a property of
  the templates rather than a rule each reader has to remember.
* **The bases are not carried beside them.** They are in the templates already, and a manifest stating one address twice
  is a manifest that can state it two ways.
* **A corpus that is not its repository names the folder it sits in.** `publishing.path-prefix` lands between the commit
  and the record's path, which is the only place it can go. It is settled in the template for the reason the ref is:
  what a reader supplies is the path alone, so the two cannot be joined in the wrong order.

`Publishing.Links` substitutes into those same templates, so a link the export resolves and a link a consumer builds for
one part are the same string.

#### A per-record file pays the churn a term line no longer does

Its links are resolved, so the commit sits inside them. The file therefore rewrites on every export from a new commit,
whatever its content did. It is bought deliberately: a reader that has already dereferenced one record wants a URL in
its hand, not a template and a substitution rule. At a handful of records the cost is a few untracked files. It is worth
reopening where a type exports records by the hundred, because the churn scales with the record count and what it buys
does not.

#### Four kinds of corpus have no address

The tool can build on none of them: one publishing nowhere, one naming a target nothing builds links for, one stating a
target but no bases, and one git cannot answer for. Each exports without links. The manifest carries the target it was
given and null templates beside it, so a consumer sees the absence stated. The run itself says which of the four caused
it. A term line is unaffected: `path` and `anchor` are facts about the corpus rather than about where it is published,
and they travel either way.

### Three versions, and none implies another

`formatVersion` covers the envelope: the keys the manifest carries, the layout of the tree, and how a link template is
built. Each entry under `types` carries a `shapeVersion` covering that one type's files. `contentVersion` is
`content-version` from `.corpus.yaml`, semantically versioned and bumped by hand, and states what the corpus knows. A
corpus can rewrite every definition and move none of the three.

#### Each type is versioned where its keys are declared

A type declares the keys of its own part line in `export.parts.line:`, so two types exporting parts write different
files. The glossary's terms carry a `definition` and the `not` line beneath them, both read from a term's body. A type
whose parts are table rows has no such body, and names its columns instead.

That is why the number sits beside the keys. One number across every type would refuse a bundle over a change nobody's
consumer reads. A plugin whose only skill looks terms up would be refused on the day some other type gained a key.

#### What moves `formatVersion` or a `shapeVersion`

One test decides both. The number moves when a reader written against the shape before it would now be wrong. Adding a
key to a file, or a file to a type's directory, leaves that reader correct and moves nothing. Renaming a key, dropping
one, changing the type of a value, or changing what a key means: each turns a correct read into a wrong one, and each
moves the number.

Reordering the keys within a line does not, because every key is addressed by name. Reordering the records in a flat
file does, because that order carries meaning within a chain.

Which of the two moves follows from which files moved. `formatVersion` went from 1 to 2 when a term line's two resolved
URLs became a `path` and an `anchor`. The same edit today moves the glossary's `shapeVersion`, because a term line is a
glossary's file and no consumer of another type reads it.

#### `bundle` is what reads them

`bundle` refuses an export whose `formatVersion` this build does not write, and a component reading a type at a
`shapeVersion` the export does not carry. `.dist/export/` is untracked and outlives the run that wrote it, so a bundle
built after a pull is the ordinary way to meet an export this tool did not ship beside.
[`bundle`](bundle.md) says what each refusal prevents and how a component names the shape it reads.

## Known limits

**A record's exported field is a scalar.** `export.fields:` reads each name as one value, so a field the record writes
as a list arrives as `null`. A policy's `aligns-with` is that case, and it stays out of the block for that reason.
Nothing is lost: the roll-up summarises the `Alignment` cells, and every clause line carries its own.

**A clause line carries no `seeAlso`.** A cross-reference is read from the body beneath a part, and a table row has
none. So a clause pointing at another policy reaches a consumer as the id inside its words, and the run does not name
that link as unread. A glossary term is unaffected: its body is everything under the heading.

**A part line carries one fidelity of three.** A section travels at any of `full`, `summary` and `reference`, and an
`export.parts:` entry travels at `full`. A type declaring either of the others against its parts is reported as
declaring a fidelity nothing carries there, and the schema pass fails. Failing is the safe way round, since the
alternative is an export quietly thinner than the type asked for. The value stays three-valued so that the first type
wanting a shorter line does not force it to be rebuilt.
