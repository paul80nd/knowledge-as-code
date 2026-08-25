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

### The flat file is JSONL because it exists to be grepped

A hit has to hand back something parseable on its own. A matching line of an indented document is a fragment, and the
reader is left seeking outward for its braces.

So each line repeats what a reader would otherwise look up: the record it came from, the state of that record, and its
cross-references as ids. That costs bytes. It is worth them, because the alternative is a hit that sends the reader to
the very file this one exists to save them opening.

An address is the one thing a line does not repeat. It carries the record's `path` and the part's `anchor`, and the
manifest carries the two templates they go into.

**`part` and `anchor` hold the same string, and that is an invariant rather than duplication.** The two answer different
questions. A part's id is what a citation from elsewhere in the corpus resolves against. An anchor is what a link's
fragment has to be. A type that takes its parts from headings makes one string do both jobs, because a heading's slug is
its id and its anchor alike. Every line of a glossary's flat file therefore carries the pair equal. A line where they
differ is a defect.

### A key name is a word the corpus may also define

`record` and `title` are keys on every line and terms in this corpus's own glossary. Every line carries both keys, so a
search for either hands back the whole file and identifies nothing in it. That is a property of the format rather than
of the content, and it holds for any key named with an ordinary English noun. That constraint reaches what a type's
`export:` block may call a field. The skill compensates by reading each hit's `title` before it uses the hit. A line
defines a term when its `title` says so, never because it matched.

### Order carries meaning within a chain

**Records are ordered roots-by-id, each root's chain depth-first beneath it.** Terms sort alphabetically within a
record. Generality holds **within a chain** and nowhere else: `gls-search` narrows `gls-example-libraries`, so a grep
for `title` meets the general entry before the one refining it. Across unrelated roots the order is stable and says
nothing: `record` is defined by `gls-example-libraries` and `gls-knowledge-as-code`, and neither narrows the other.
Reading the first hit as the more general one would give a reader the wrong domain. `narrows` on the owning records is
what tells the two cases apart, and every line names the record it came from.

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

### Two versions, and they are independent

`formatVersion` in the manifest is the shape of the output. `contentVersion` is
`content-version` from `.corpus.yaml`. It states what the corpus knows, semantically versioned and bumped by hand, with
major, minor and patch defined in that file beside the key. Neither implies the other: a corpus can rewrite every
definition without `formatVersion` moving, and `formatVersion` can move over a corpus nobody has edited.

**`bundle` is what reads `formatVersion`.** It refuses an export whose number is not the one this build of the tool
writes. It names both: the number the export declares and the number the tool reads. `.dist/export/` is untracked and
outlives the run that wrote it. So a bundle built after a pull is the ordinary way to meet an export this tool did not
ship beside.

What the refusal prevents is a plugin assembled around a manifest whose keys have moved. Its breadcrumb would state
nothing and its skill would find nothing. A lookup that silently returns nothing is indistinguishable from a term the
corpus does not define.

**What moves `formatVersion`.** It moves when a reader written against the shape before it would now be wrong. Adding a
key to a file, or a file to a type's directory, leaves that reader correct and moves nothing. Renaming a key, dropping
one, changing the type of a value, or changing what a key means: each turns a correct read into a wrong one, and each
moves the number. Reordering the keys within a line does not, because every key is addressed by name. Reordering the
records in a flat file does, because that order carries meaning within a chain. It went from 1 to 2 when a term line's
two resolved URLs became a `path` and an `anchor`. Dropping those keys is the breaking side of that boundary.

## Known limits

**A term line's `anchor` is the part id, whatever the type sourced its parts from.** The exporter writes both keys from
one value. That is correct for a heading-sourced type, where a heading's slug is its id and its anchor alike. It is
wrong for a table-sourced one, whose part id is authored. A policy clause id is `TIMEBOX`, and no fragment resolves to
it. Deriving the anchor from the part source is what would fix it. No table-sourced type declares an `export:` block
today, so the path is unexercised and no fixture covers it.

**One fidelity of three is carried.** A type may declare `full`, `summary` or `reference` against each piece its
`export:` block selects, and the exporter carries `full`. A type declaring either of the others is reported as declaring
a fidelity nothing carries, and the schema pass fails. Failing is the safe way round, since the alternative is an export
quietly thinner than the type asked for. Glossary needs only `full`. The model is three-valued so that the first type
wanting a section reduced to a line does not force it to be rebuilt.
