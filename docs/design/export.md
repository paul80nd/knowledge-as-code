# The export format

`kac export` writes `.dist/export/` for something that will never open a corpus's Markdown. This page is the contract:
what each file holds, what a type decides about its own records, and which version number moves when any of that
changes.

Two readers need it. One declares the `export:` block of a type, meaning one kind of knowledge record, and wants to know
what each key produces. The other writes a consumer against the output and wants to know what it may rely on.
[`export`](../cli/export.md) is the page for running the command.

## What a type declares

**What travels is the type's decision**, written in the `export:` block of its schema file and described key by key in
[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json). The
exporter reads that declaration and nothing else, so a corpus adopting a new type exports it without any change to the
tool.

Three keys select what a record sends:

* **`fields:`** is a plain list. A field travels whole or not at all, and in the shape its type declared: a list as a
  JSON array, anything else as a string.
* **`sections:`** names each section beside a **fidelity**, saying how much of that section travels.
* **`parts:`** names the keys of one part's line, and the fidelity that entry carries.

Neither `sections:` nor `parts:` falls back to a default. A type states the fidelity, or the schema pass fails.

A fourth key, **`frameworks:`**, names a second flat file rather than selecting anything from a record.
[`frameworks.jsonl`](#frameworksjsonl) below says what it holds.

A corpus that adopted no exporting type still writes a manifest, with an empty type list.

### A type held back

Every other type travels. A type declaring no `export:` block reaches nobody but a reader browsing the repository it
sits in, which is a different product from the one this framework describes. So declaring the block is part of writing a
type. A type deliberately held back gives the reason in its own schema file, where the next person to ask the question
is already reading.

Which types travel is therefore a fact about `.schema/` rather than about the tool. A corpus adopting a type receives
what that type declared and narrows none of it, so two corpora publishing one type publish it the same way.

`discoveries` is the type held back, and `.schema/discoveries.yaml` gives the reason. A few others declare no block
either, and none of those is a decision. No corpus in this repository holds a record of one, so nothing here could prove
the block was written right.

### A type declaring no sections

It travels as frontmatter. A type may declare no sections at all. `reports` is that case, because `kac report` writes
the headings the question it answers needs, and a schema naming any of them would hold every future report to the shape
of the first two.

Its records carry `sections` as an empty object, and its manifest entry does the same. A consumer reads the frontmatter,
then follows the record's own `links` for the body.

### `trust`

A record carries one key its type never declares. `trust` says how far the record has been taken on trust, and the
exporter derives it from the `verified` field. An empty list is `unverified`, agents alone are `machine-confirmed`, and
one `human:` actor is `human-reviewed`. Only that prefix reads as a person, and each type declaring the field limits its
entries to a person or a producer, so that nothing else can reach the tier. Those three tiers are the
[Open Knowledge Format](https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md)'s, and a type
gains them by taking that specification's field name.

`kac` derives it rather than storing it, so that a record has one place saying who checked it. A stored tier is a second
account of the same fact, and the two go out of step the day somebody adds a line to the list.

A type whose `fields:` does not name `verified` carries `trust` as `null`, which is the absence every other key already
spells. A type answering with a tier names the field, as `reports` and `fixes` do.

A record a consumer inherited carries no `trust` key at all, because it is copied from the producer's export byte for
byte and a producer older than this key wrote none. So a consumer tests for the key on an inherited record, the way it
already tests for `shortcode`.

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

**A `parts:` entry carries `full` alone.** A part line is already a reduction. `line:` names key by key what of a part
travels, so a type wanting a thinner line drops a key from it.

## How a run writes it

A run loads the corpus whole, decides which records travel and what of each one goes with them, then deletes
`.dist/export/` and writes it again.

**The corpus is loaded whole whatever `--type` says.** A narrowed run still resolves ids against every record, because a
question about a set answered from some of its members is answered wrongly. The flag narrows what is written and never
what is read. `kac` refuses a type the corpus has not adopted, and names it.

**An unsettled record travels by default.** A draft record, and one whose `review-by` has passed, are both exported with
their own state, so a consumer reads what the corpus actually holds and decides for itself how far to trust it.
Filtering them would make the corpus's own condition invisible downstream.

A corpus may exclude either with `export.exclude:` in [`.corpus.yaml`](../corpus-descriptor.md). Where it does, the run
names every record it withheld, because a record left out of the output cannot be seen there.

**The export is untracked, and rebuilt whole.** `.dist/` is gitignored, so the export is never something to review, and
a tracked one would put a diff nobody reads on every change to the words. Two things follow. The overwrite is
delete-then-write, because a record deleted from the corpus must not leave an entry behind and no diff would show the
orphan. And the manifest describes itself, giving the commit it was built from and a dirty flag beside it, since git can
say nothing about an export once it has left. A commit on its own would describe a dirty tree as reproducible.

What holds the shape steady in place of a diff is a committed fixture in the tool's own test suite. It exports a corpus
and compares the whole tree file by file, so a corpus running the tool without the tests receives a format already
proved.

## The tree

A corpus that adopted three types, and consumes a corpus with standards of its own:

```text
.dist/export/
  manifest.json          what this export is, and where it came from
  glossary/
    gls-<name>.json      one record: its declared fields, its declared sections, and its links
    terms.jsonl          every term, one to a line, addressed by path and anchor
  policies/
    pol-<MNEM>.json      the same, for a policy
    clauses.jsonl        every clause, one to a line, carrying the level it binds at
    frameworks.jsonl     every external framework reference, one to a line, and the clauses citing it
  standards/
    std-<MNEM>.json      the same, for a standard
    eng/
      std-<MNEM>.json    a record of a corpus this one consumes, filed under that corpus
    rules.jsonl          every rule of both corpora, one to a line
```

`kac` reads the names from the schema. A type's directory is its own key, and its parts file is named for what the type
calls one of its parts: `terms.jsonl`, because a glossary's `parts:` block says `noun: term`. `frameworks.jsonl` is
named outright, by the `frameworks:` key. All three are fixed once the type has declared them, because a skill addresses
them by name.

## A consumer inherits what its producers published

A corpus naming another in `consumes:` publishes that corpus's records alongside its own. The alternative is what the
skills already tell an agent to expect and what an export could not deliver. Standards compose, so the rules binding a
piece of work are the union of every layer that reaches it, and half that union sitting unexported reads to an agent
exactly like a corpus that states no rule.

### Every type the producer exported

All of them travel. A consumer receives types it never adopted. A rule of this corpus cites a clause of a policy it does
not hold, and that address resolves only where the policy travelled too. Filtering the inheritance down to the types the
consumer adopted would publish an obligation whose authority is unreachable.

It is also what decides which skills a plugin ships. [`bundle`](../cli/bundle.md) trims a component whose types the
export does not carry, so a corpus inheriting a glossary ships the glossary skill without declaring anything.

### The parts merge and the records do not

One flat file per type, holding both corpora's lines, is the whole point. An agent greps once for every rule that binds
it, rather than learning which corpus wrote which. A line can say whose it is, so nothing is lost by merging them.

A record file has only its name to say that, and two corpora may each hold a `std-TEST`. So an inherited record is filed
under the shortcode of the corpus that wrote it, and the consumer's own records stay where they were.

### A bare id

It is the corpus's own. An inherited line carries its producer's shortcode on every key holding an id: the part's own
id, the record it belongs to, and each id under `seeAlso`. It names that producer again under `shortcode`, which is the
key into `sources`.

A line with no `shortcode` was written by the corpus publishing the export. That is the rule a citation already follows,
where `eng:pol-VURM` names another corpus and a bare `pol-VURM` names this one, so a reader learns one rule rather than
two.

`seeAlso` is the value worth being careful about. Left bare, an inherited reference would point at whatever the
consuming corpus happens to call the same thing. That resolves, and resolves to the wrong record, which is worse than
resolving to none.

### A chain of any depth

It costs no code. A grandparent's records arrive inside its child's export already carrying the grandparent's shortcode,
and they keep it. `kac` stamps only an unstamped line, so a value is prefixed once however many corpora it passes
through.

`sources` has the same property. A consumer publishes an entry for each corpus it consumes and for each corpus they
consumed in turn, so a line naming a grandparent finds the address its own producer published for it.

### What stops the run

Each of these ends the run with a reason and nothing written. None is visible in the output, so the run that built it is
the last place anyone would see it.

* **A declared import with nothing restored.** The export would be missing a layer and say nothing about it.
* **A consumed corpus at an export format this build does not read.** A merge reads the producer's own key names out of
  its manifest, so an envelope this build does not know is one whose keys it cannot be sure of.
* **Two corpora exporting one type at two shapes**, or carrying its sections at two fidelities. A merged file whose
  halves are shaped differently reads as one file and answers two ways.
* **One corpus arriving twice at two versions.** Two corpora consumed here may each have consumed a third. Whichever
  account of it won, a line naming it would resolve to a commit half its records were never read at.

### `sources`

It is the one thing a merge cannot merge. Each entry holds the publishing block its producer wrote: its target, its
base, its path prefix and its commit. A record of `eng` is read at eng's commit, under eng's path prefix, in eng's
repository, and the consuming corpus's own block gets all three wrong.

```json
"sources": [
  { "shortcode": "eng", "corpus": "example-engineering", "contentVersion": "0.7.4",
    "publishing": { "target": "github", "ref": "133ebc79…", "pathPrefix": "examples/engineering" } }
]
```

`publishing` stays what this corpus says about itself, so a reader holding an export that inherits nothing reads it
exactly as before.

## The manifest

It lets a reader choose which file to open. A flat file is read whole and grepped, because a lookup does not know which
record holds the term it wants. A record file is read one at a time, because a reader that has a hit wants the single
file behind it. One large file would charge the second reader the first one's cost.

### Two counts per type, named apart

One is how many records the type holds and the other how many parts. For a glossary the two differ by an order of
magnitude. A reader sizing the vocabulary wants the parts, and a reader asking how many files it was handed wants the
records.

### The fidelity each type's sections travelled at

Every type states it. Without that, a summary reaches a consumer looking exactly like a whole section. Fidelity belongs
to the type, so each entry under `types` gives it once.

```json
"sections": { "Purpose": "summary", "Scope": "full", "Exceptions": "full" }
```

### The two keys that address a part

Every type names them. A part line's keys are the type's own words, so a consumer holding a corpus with a type it never
adopted has no schema to read them from. Four keys are named: which record a line belongs to, which part of that record
it is, the part's own full id, and the ids of the parts it points at.

```json
"partsFile": "policies/clauses.jsonl", "recordKey": "record", "partKey": "part",
"idKey": "id", "seeAlsoKey": null
```

The first two are what any reader needs to address a part. The last two are what a corpus merging this type stamps its
shortcode onto, so a producer choosing its own words for them is stamped correctly rather than left unlabelled.

A consumer assuming a spelling would read a producer's parts as empty wherever that producer chose different words, and
every citation into them would fail for a reason nothing states. `seeAlsoKey` is null for a type declaring no such key.

A type keeping no parts writes all five as null, beside `parts` at zero, and its records travel one JSON apiece. Most
types are that case, and the entry says so rather than leaving a consumer to infer it from a file that is not there:

```json
"type": "processes", "shapeVersion": 1, "records": 7, "parts": 0, "dir": "processes",
"partsFile": null, "recordKey": null, "partKey": null, "idKey": null, "seeAlsoKey": null
```

### `frameworksFile`

It names the second flat file, where a type keeps one.

```json
"frameworksFile": "policies/frameworks.jsonl"
```

It is null for every type declaring no `frameworks:` key, and null as well where the type declares one and no clause
cites a framework. So a consumer opens the file the manifest names and never seeks one that was not written.

### `about`

It carries what the corpus says about itself. `kac pack` never loads the corpus and a plugin is assembled from the
export rather than from the tree, so the descriptor's own words reach a registry and a marketplace through the manifest
or not at all.

```json
"about": { "displayName": "…", "description": "…", "author": { "name": "…", "url": "…" }, "license": "…" }
```

Every field is `null` where the corpus said nothing, and `kac` fills nothing in from a neighbour's value. An author
nobody named and a licence nobody chose are claims about a person, and a template supplying them is how a corpus comes
to publish under somebody else's name.

### `corpus` and `shortcode`

`corpus` is what the corpus calls itself, which tells one export from another. `shortcode` is what a citation writes
before the colon, so a consumer resolving `eng:pol-VURM` knows which of the exports it holds answers it. It is `null`
where the corpus declares none, and [`.corpus.yaml`](../corpus-descriptor.md#identity) is where a corpus declares one.

## The flat file is JSONL

A hit has to hand back something parseable on its own. A matching line of an indented document is a fragment, and the
reader is left seeking outward for its braces.

So each line repeats what a reader would otherwise look up: the record it came from, the state of that record, and its
cross-references as ids. That costs bytes, and it is worth them. The alternative is a hit that sends the reader to the
very file this one exists to save them opening.

### The address

It is the one thing a line does not repeat. A line carries the record's `path` and the part's `anchor`, and the manifest
carries the two templates they go into.

**`part` and `anchor` answer different questions**, and the part source decides whether one string does both. A part's
id is what a citation from elsewhere in the corpus resolves against, where an anchor is what a link's fragment has to
be. A heading's slug is its id and its anchor alike, so every line of a glossary's flat file carries the pair equal.

A table row has no fragment of its own, and its id is authored. A policy clause id is `TIMEBOX`, and no fragment
resolves to that. So a clause line carries the clause id in `part` and the slug of the section holding the table in
`anchor`, and a link built from that line lands on the table.

### A key name the corpus also defines

`record` and `title` are keys on every line and terms in the framework's own glossary. Every line carries both keys, so
a search for either hands back the whole file and identifies nothing in it. That is a property of the format rather than
of the content, and it holds for any key named with an ordinary English noun. It reaches what a type's `export:`
block may call a field. A reader compensates by reading each hit's `title` before using the hit.

### Order within a chain

It carries meaning. Order across chains does not.

**Records are ordered roots-by-id, each root's chain depth-first beneath it.** Generality holds within a chain and
nowhere else. `gls-search` narrows `gls-example-libraries`, so a grep for `title` meets the general entry before the one
refining it.

Across unrelated roots the order is stable and says nothing. Reading the first hit as the more general one would give a
reader the wrong domain. `narrows` on the owning records is what tells the two cases apart, and every line names the
record it came from.

**Within a record, the part source decides.** A glossary's terms sort alphabetically. A policy's clauses travel in the
order the table writes them, because that order groups the obligations ahead of the recommendations. Sorting them would
hand a consumer a different policy from the one the page shows.

### `frameworks.jsonl`

It carries the framework edge from the framework's end. A policy clause maps to an external framework in its
`Alignment` cell, and that edge travels in a file of its own rather than on the clause line. One line per reference, and
the type that declares a `frameworks:` key gets it:

```json
{"framework":"ISO 27001:2022","reference":"A.8.13","standing":"Obliged",
 "clauses":["pol-BKUP.COPY","pol-BKUP.SHARED"],"records":["pol-BKUP"],
 "path":"frameworks.md","anchor":"iso-27001"}
```

**The exporter writes these keys, and the type names only the file.** A reference is read from a clause's cell and from
the register, meaning the corpus's own `frameworks.md`, that the cell links to. Neither is a field the type declares, so
a `line:` block over its own fields could not name where any of it comes from.

**`standing` is the register's word, verbatim.** Example Engineering files a framework under `Obliged`,
`Self-obligated` or `Inspiration`, and another corpus may use words of its own. `path` and `anchor` address the entry
itself, so a reader can reach the page that placed it.

**A line names its own corpus's clauses alone.** An inherited clause left its `Alignment` cell behind, so a consumer
knows nothing about what its producer's clauses cite and reads the producer's own file for that half. This is the one
place the merge does not reach.

### A cross-reference is read, never inferred

A `**Not:**` line pointing at another glossary is a link, and a link's target is stripped out of the prose. The export
carries the part it names in `seeAlso`, as `gls-search.title`. It resolves to the term rather than the record, because
`redefinitions-are-reciprocal` is about a term and its counterpart.

A link naming a record and no term inside it leaves nothing to read, so nothing is carried. The obvious guess is the
same word in the other glossary. It is right only for a pair that happens to share a spelling, and silently wrong for a
`Borrower` that redefines a `Patron`. The run names each link it could not read, since an omission in an artefact nobody
reviews is invisible.

## What holds in every file

**Absent is `null`.** A field a record leaves blank and a field it does not carry are one absence to a consumer, and
`""` in one file beside `null` in another would leave that consumer checking which file it had opened before it could
test for nothing. A list a record left empty is that same absence, so no key arrives as `[]`.

**`shortcode` is the one key that can be missing outright.** A line the publishing corpus wrote carries no such key,
because absence there is the same rule a bare id already follows. So a consumer tests the value of every other key, and
tests for this one key itself.

**A list field's shape comes from the type, not from the record.** A field its type declares as a list is a JSON array
in every record file that carries anything under it, so a consumer reads one key one way across every record of the
type.

Where the type declares those entries as objects, each entry carries the keys that declaration names. An entry key
holding a list of its own is an array inside it, so an entry naming a framework and the clauses it maps to carries both.

**Prose arrives unwrapped.** The corpus wraps at 120 columns, which is a fact about the file rather than about the
words, and a grep for a phrase straddling the wrap would find nothing. Blank lines are the author's and stay.

`kac` leaves a list, heading, quote, table or fence exactly as written, and that is a decision rather than an unfinished
case. A list joined onto one line is destroyed and the reader cannot recover it, where a paragraph left wrapped merely
arrives as it was written. Every doubtful line therefore goes the safe way.

**A link reference definition never travels.** The definitions sit in a block at the foot of a record, which puts them
inside whichever section is written last. They render as nothing on the page, so a section carried as prose ends on the
author's words and a consumer is never handed the paths.

**Output is deterministic.** Ordering is `StringComparer.Ordinal` everywhere but a term line's own position, which sorts
case-insensitively on the term. Two runs from one commit produce identical bytes but for `generatedAt`.

## The link and the ingredients

A person follows a link and an agent fetches a file, and only the first of those is an address the export can write. The
rules joining the base to a path, and the anchor rule for a part, belong to `publishing-target`.
[`.corpus.yaml`](../corpus-descriptor.md#publishing) supplies where the corpus is served from and nothing else.

### Ingredients rather than a second URL

That is what an agent is handed. An export used to carry a second template, pointing at raw source an agent could fetch
without credentials. Only GitHub ever served that, and only for a public repository. A private GitHub repository never
had such a host, and Azure DevOps has none at all, so the second template was a rule one target could follow and the
rest could not.

So the manifest carries `base`, `pathPrefix` and `ref` instead. An agent joins `pathPrefix` ahead of the record's
`path` to reach the file inside the repository, then asks the client that authenticates to `target` for that file at
that `ref`. `gh` does it for GitHub and `az rest` for both Azure DevOps targets. Where the agent has no such client, the
honest answer is to quote the human link and say so, rather than to assemble a URL that returns a sign-in page it will
read as the record.

### The human form as a template

The manifest states it. The template is `https://…/blob/<sha>/{path}#{anchor}` for GitHub, and its own shape for each
other target. A consumer substitutes the `path` and `anchor` a line carries. Four things follow from writing it this
way:

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

### The target that cannot pin its link

Every link resolves against the commit the export was built from, so a citation names the version the agent read rather
than whatever the branch holds later. `azure-devops-wiki` is the exception it cannot help, because no `?pagePath=` URL
takes a commit. An agent reading that corpus still reads the pinned version, because `ref` reaches it through the
manifest.

### A per-record file

It carries its links already resolved, rather than a template and a substitution rule. The commit sits inside them, so
the file rewrites on every export from a new commit whatever its content did. That churn is bought deliberately. A
reader that has already dereferenced one record wants a URL in its hand rather than a template and a substitution rule.

At a handful of records the cost is a few untracked files. It is worth reopening where a type exports records by the
hundred, because the churn scales with the record count and what it buys does not.

### Five kinds of corpus with no address

One publishing nowhere, one naming a target nothing builds links for, one stating a target but no base, one whose base
is not a URL that target can join to, and one git cannot answer for. Each exports without links, and the manifest
carries the target it was given with nulls beside it, so a consumer sees the absence stated.

A part line is unaffected. `path` and `anchor` are facts about the corpus rather than about where it is published, and
they travel either way.

## Three versions, and none implies another

`formatVersion` covers the envelope: the keys the manifest carries, the layout of the tree, and how a link template is
built. Each entry under `types` carries a `shapeVersion` covering that one type's files. `contentVersion` is
`content-version` from [`.corpus.yaml`](../corpus-descriptor.md), semantically versioned and moved by hand, and it says
what the corpus knows. A corpus can rewrite every definition and move none of the three.

`contentVersion` is the one a consumer depends on by name. [`pack`](../cli/pack.md) publishes the export at that
version, so a corpus that changes what it knows and leaves the number alone has nothing new to release.

### Where a type is versioned

It is versioned where its keys are declared. A type declares the keys of its own part line in `export.parts.line:`, so
two types exporting parts write different files. The glossary's terms carry a `definition` and the `not` line beneath
them, both read from a term's body. A type whose parts are table rows has no such body, and names its columns instead.

One number across every type would refuse a bundle over a change nobody's consumer reads. A plugin whose only skill
looks terms up would be refused on the day some other type gained a key.

### A number a reader does not know

It obliges the reader to stop. The section above says when the exporter moves a number. This is the other half of the
same promise, and it is what a reader owes in return: meet a `formatVersion` above the one you were written against, and
refuse the export rather than read the parts you recognise. A number moves exactly when a reader written against the
shape before it would now be wrong, so reading on is reading something whose meaning has changed underneath you.

A `shapeVersion` obliges less, and deliberately. It covers one type's files, so a reader that does not know a type's
number leaves that type alone and reads the rest of the export. That is the whole reason the two numbers are separate.
One number across every type would stop a reader over a change to a type it never opens.

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
its `raw` half in the same edit and moved no `shapeVersion`, because the exporter writes that object for every type
rather than any one type declaring it in its `export:` block. How a link is built is the envelope's business.

It went from 3 to 4 when `sources` arrived and a consumer began carrying what it consumes. `idKey` and `seeAlsoKey`
arrived on each type's entry in the same edit and moved no `shapeVersion`, because they name keys that were already
there rather than changing any line.

## What a type cannot say

**A part line carries no list.** A `line:` key reading `front.<field>` takes one value, so a type naming a list field
there writes `null` on every line of the flat file. The record file carries that same field as an array, which is where
a consumer reads it.

**A clause carries no framework alignment.** A reference on the clause line would say a clause touches `A.8.24` and
leave the reader to work out what that commits anyone to, because the standing that answers it sits on the corpus's own
`frameworks.md` and no consumer receives the page. `frameworks.jsonl` carries the same edge from the other end, where
the standing travels beside it. Navigation is therefore one way: a framework names its clauses, and a clause names no
framework.

**A clause carries no `seeAlso`.** A cross-reference is read from the body beneath a part, and a table row has none. So
a clause pointing at another policy reaches a consumer as the id inside its words, and the run does not name that link
as unread. A glossary term is unaffected, because its body is everything under the heading.

**A part line carries one fidelity of three.** A section travels at any of `full`, `summary` and `reference`, and a
`parts:` entry travels at `full`. A type declaring either of the others against its parts fails the schema pass, which
is the safe way round. The alternative is an export quietly thinner than the type asked for. The value stays
three-valued so that the first type wanting a shorter line does not force it to be rebuilt.

[`export`](../cli/export.md) is the command that writes this, and [`pack`](../cli/pack.md) zips it for a consumer.
