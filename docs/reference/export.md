# The export format

`kac export` writes `.dist/export/` for something that will never open the Markdown of a corpus (one repository of
knowledge records kept in git). This page is the contract: what each file contains, what a type decides about its own
records, and which version number moves when any of that changes.

It is for whoever declares the `export:` block of a type, meaning one kind of knowledge record, and for whoever writes a
consumer against the output. [`export`](../use/cli/export.md) is the page for running the command.

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

A fourth key, **`frameworks:`**, names a second flat file. [`frameworks.jsonl`](#frameworksjsonl) below says what it
contains.

A corpus that adopted no exporting type still writes a manifest, with an empty type list.

### A type held back

A type declaring no `export:` block is not exported. A type held back on purpose gives the reason in its own schema
file. A corpus adopting a type receives what that type declared and narrows none of it.

### A type declaring no sections

It travels as frontmatter. `reports` is that case. Its records have `sections` as an empty object, and its manifest
entry does the same. A consumer reads the frontmatter, then follows the record's own `links` for the body.

### `trust`

A record carries one key its type never declares. `trust` says how far the record has been taken on trust, and the
exporter derives it from the `verified` field:

| `verified` contains          | `trust`             |
|------------------------------|---------------------|
| nothing                      | `unverified`        |
| agents alone                 | `machine-confirmed` |
| at least one `human:` actor  | `human-reviewed`    |

Only the `human:` prefix reads as a person. A verification taken before `generated.at` is left out. The three tiers are
the [Open Knowledge Format](https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md)'s.

A type whose `fields:` does not name `verified` carries `trust` as `null`. `reports` and `fixes` name it.

A record a consumer inherited has no `trust` key at all, because it is copied from the producer's export byte for
byte. Test for the key on an inherited record, as you do for `shortcode`.

### What each fidelity carries

Take a policy whose `Exceptions` section runs to two paragraphs. Its record carries one of these three.

| Fidelity    | What the record's `Exceptions` key contains                                                 |
|-------------|---------------------------------------------------------------------------------------------|
| `full`      | `"A team may deviate where the data stays in the region.\n\nRecord it against the clause."` |
| `summary`   | `"A team may deviate where the data stays in the region."`                                  |
| `reference` | `null`                                                                                      |

`full` keeps the wrapped lines joined and the paragraph breaks. `summary` takes the opening paragraph. `reference`
leaves a consumer the record's own `path` and `links` to follow.

A section the record never wrote is absent from `sections` altogether. A `null` says the type sent a reference, and an
absent key says nobody wrote the section.

**A `parts:` entry carries `full` alone.** `line:` names key by key what of a part travels, so a type wanting a thinner
line drops a key from it.

## How a run writes it

A run loads the corpus whole, decides which records travel and what of each one goes with them, then deletes
`.dist/export/` and writes it again.

* **The corpus is loaded whole whatever `--type` says.** The flag narrows what is written and never what is read. `kac`
  refuses a type the corpus has not adopted, and names it.
* **An unsettled record travels by default.** A draft, and a record whose `review-by` has passed, are both exported with
  their own state. [`export.exclude:`](../use/corpus-descriptor.md#export) withholds either, and the run names every
  record it withheld.
* **The export is untracked, and rebuilt whole.** A record deleted from the corpus leaves no entry behind. The manifest
  gives the commit it was built from and a dirty flag beside it.

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
named by the `frameworks:` key. A skill addresses all three by name.

## A consumer inherits what its producers published

A corpus naming another in `consumes:` publishes that corpus's records alongside its own.

* **Every type the producer exported travels**, including types the consumer never adopted, so
  [`bundle`](../use/cli/bundle.md) ships a skill reading one of them.
* **The parts merge.** One flat file per type contains both corpora's lines.
* **The records do not.** An inherited record file is filed under the shortcode of the corpus that wrote it, as
  `standards/eng/std-TEST.json`, and the consumer's own records stay where they were.

### A bare id

It is the corpus's own. An inherited line has its producer's shortcode on every key containing an id: the part's
own id, the record it belongs to, and each id under `seeAlso`. It names that producer again under `shortcode`, which is
the key into `sources`. A line with no `shortcode` was written by the corpus publishing the export.

### A chain of any depth

A grandparent's records arrive inside its child's export already naming the grandparent, and they keep that name. `kac`
stamps only an unstamped line, so a value is prefixed once however many corpora it passes through. An ancestor's record
files stay one folder deep however long the chain. A corpus can consume both a producer and that producer's own
producer. The grandparent's record files and part lines then arrive twice, and `kac` writes and counts each one once. A
consumer publishes a `sources` entry for each corpus it consumes, and for each corpus they consumed in turn.

### What stops the run

Each of these ends the run with a reason and nothing written:

* a declared import with nothing restored
* a consumed corpus at an export format this build does not read
* two corpora exporting one type at two shapes, or sending its sections at two fidelities
* two corpora stating two [in-force statuses](#inforce) for one type
* one corpus arriving twice, at two versions
* a part line naming a record whose file the export does not write, such as a line for `gp:gls-old` with no
  `glossary/gp/gls-old.json` beside it
* a consumed type that arrives with fewer record files than its producer's manifest entry counts in `records`
* a consumed type that arrives with no record files, where its producer sent part lines for it or its manifest entry
  counts `parts` above zero

A consumed type with no record files, no part lines and both counts at zero is empty. The export leaves it out and says
nothing.

### `sources`

Each entry has the three addresses its producer wrote: `publishing`, `tracker` and `reposBase`. A producer's records are
read, filed against and resolved at the producer's addresses, never the consuming corpus's.

```json
"sources": [
  { "shortcode": "eng", "corpus": "example-engineering", "contentVersion": "0.7.4",
    "publishing": { "target": "github", "ref": "133ebc79…", "pathPrefix": "examples/engineering" },
    "reposBase": "https://github.com/example",
    "tracker": { "target": "github", "base": "https://github.com/example/engineering",
                 "area": null, "id": "github:github.com/example/engineering" } }
]
```

The keys at the root stay what this corpus says about itself. An entry written by a producer that predates `tracker` is
derived from the `publishing` beside it, by the rule [`tracker`](#tracker) below states. One that predates `reposBase`
states `null`.

## The manifest

A reader opens the manifest first, then greps a flat file whole or opens one record file. Each entry under `types`
gives the following.

### Two counts per type, named apart

`records` is how many records the type has, and `parts` how many parts.

### The fidelity each type's sections travelled at

Each entry gives it once, so a consumer can tell a summary from a whole section.

```json
"sections": { "Purpose": "summary", "Scope": "full", "Exceptions": "full" }
```

### The keys that address a part

A part line's keys are the type's own words, so each entry names them: which record a line belongs to, which part of
that record it is, the part's own full id, and the ids of the parts it points at.

```json
"partsFile": "policies/clauses.jsonl", "recordKey": "record", "partKey": "part",
"idKey": "id", "seeAlsoKey": null
```

Read a producer's key names from here, and never assume a spelling. `seeAlsoKey` is null for a type declaring no such
key. A type keeping no parts writes all five as null, beside `parts` at zero, and its records travel one JSON apiece:

```json
"type": "processes", "shapeVersion": 1, "records": 7, "parts": 0, "dir": "processes",
"partsFile": null, "recordKey": null, "partKey": null, "idKey": null, "seeAlsoKey": null
```

### `frameworksFile`

It names the second flat file, where a type keeps one.

```json
"frameworksFile": "policies/frameworks.jsonl"
```

It is null for every type declaring no `frameworks:` key, and null as well where no clause cites a framework.

### `inForce`

It states the `status` value at which a record of the type is in force. It is the `in-force:` the type's
`.schema/<type>.yaml` declares on `status`.

```json
"inForce": "active"
```

Read it to tell whether a record binds, and never assume `active`. It is null for a type with no `status` range. An
export written before the key existed omits it, so treat a missing key as null.

A type this corpus inherited and never adopted states the value its producer published. Where two corpora state
different values for one type, the export [stops](#what-stops-the-run). A corpus stating none agrees with any value.

### `about`

It contains what the corpus says about itself, which `kac pack` and `kac bundle` read.

```json
"about": { "displayName": "…", "description": "…", "author": { "name": "…", "url": "…" }, "license": "…" }
```

Every field is `null` where the corpus said nothing, and `kac` fills nothing in.

### `tracker`

It says where work about this corpus's own records is filed.

```json
"tracker": { "target": "github", "base": "https://github.com/paul80nd/knowledge-as-code",
             "area": null, "id": "github:github.com/paul80nd/knowledge-as-code" }
```

`kac` writes the target as the descriptor states it and builds no issues URL. A corpus that
[states no `tracker:`](../use/corpus-descriptor.md#tracker) gets the block its `publishing:` implies.

* `base` is the address of the backlog, so an `azure-devops` base is cut back to the project, whether the corpus stated
  it or `kac` derived it.
* `base` and `id` are `null` together wherever the block addresses no backlog: a `target` of `none`, a target that
  files nowhere, or a base the corpus never supplied.
* `area` is the area path below an Azure DevOps project, separated by `/`. It is `null` on every other target and
  wherever the corpus stated none. Nothing derives it.

### `reposBase`

It says where the corpus addresses its code repositories from.

```json
"reposBase": "https://github.com/paul80nd"
```

A `services` record states a bare repository name under `repos`, which resolves to `<reposBase>/<entry>`. It is `null`
where the corpus states none. Each entry in `sources` has its producer's own.

### `id`, and the trackers it compares

`id` is `target` and `base` normalised. To tell two tracker blocks apart, compare their `id` strings. Do not parse
either base.

A manifest states three trackers: this corpus's under `tracker`, the framework's under `framework`, and each producer's
under `sources`. All three can name one backlog.

The normalisation drops the scheme, a `www.` prefix, a `.git` suffix, a trailing `/`, and upper case. A corpus
published in a wiki and one published in a repository of the same Azure DevOps project compare equal. `area` is left
out, so two corpora sharing a project share an `id`. `id` is `null` wherever `base` is.

### `framework`

It says where to report a problem with the framework the corpus took.

```json
"framework": { "target": "github", "base": "https://github.com/paul80nd/knowledge-as-code",
               "area": null, "id": "github:github.com/paul80nd/knowledge-as-code" }
```

It takes the four keys `tracker` takes, written the same way. Nothing derives it. `target` is `none` and `base` is
`null` where the corpus states no framework tracker.

### `corpus` and `shortcode`

`corpus` is what the corpus calls itself. `shortcode` is what a citation writes before the colon, so a consumer
resolving `eng:pol-VURM` knows which export answers it. It is `null` where the corpus declares none.

## The flat file is JSONL

Each line parses on its own. It repeats what a reader would otherwise look up: the record it came from, the state of
that record, and its cross-references as ids.

### The address

A line has the record's `path`, the part's `anchor`, and `links.human`, the link built from both for the corpus that
wrote the part. An inherited line keeps the link its producer built.

`part` is what a citation resolves against, and `anchor` is the fragment a link lands on. A glossary term's heading slug
is both, so each line has the pair equal. A policy clause id such as `TIMEBOX` is authored and has no fragment, so
a clause line's `anchor` is the slug of the section containing the table.

### A key name the corpus also defines

`record` and `title` are keys on every line, and terms in the framework's own glossary. A search for either returns the
whole file, so read each hit's `title` before using it. The same holds for any key a type's `export:` block names with
an ordinary English noun.

### Order within a chain

**Records are ordered roots-by-id, each root's chain depth-first beneath it.** So a grep meets `gls-example-libraries`
before `gls-search`, which narrows it. Order across unrelated roots is stable and means nothing. `narrows` on the owning
records tells the two cases apart.

**Within a record, the part source decides.** A glossary's terms sort alphabetically. A policy's clauses travel in the
order the table writes them.

### `frameworks.jsonl`

It contains one line per external framework reference, for the type declaring a `frameworks:` key:

```json
{"framework":"ISO 27001:2022","reference":"A.8.13","standing":"Obliged",
 "clauses":["pol-BKUP.COPY","pol-BKUP.SHARED"],"records":["pol-BKUP"],
 "path":"frameworks.md","anchor":"iso-27001","links":{"human":"https://…/frameworks.md#iso-27001"}}
```

* **The exporter writes these keys, and the type names only the file.** A reference is read from a clause's `Alignment`
  cell and from the register, the corpus's own `frameworks.md`, that the cell links to.
* **`standing` is the register's word, verbatim.** `path` and `anchor` address the register entry, and `links.human`
  is the link to it, as on a part line.
* **A line names its own corpus's clauses alone.** An inherited clause left its `Alignment` cell behind, so read the
  producer's own file for that half.

### A cross-reference is read, never inferred

A `**Not:**` line linking a term in another glossary goes into `seeAlso`, as `gls-search.title`. A link naming a
record and no term inside it adds nothing to `seeAlso`, and the run names each link it could not read.

## What holds in every file

**Absent is `null`.** A field a record leaves blank and a field it does not have are one absence. A list a record left
empty is that same absence, so no key arrives as `[]`.

**`shortcode` is the one key that can be missing outright.** A line the publishing corpus wrote has no such key. Test
the value of every other key, and test for this one key itself.

**A list field's shape comes from the type.** A field its type declares as a list is a JSON array in every record file
that has anything under it. Where the type declares its entries as objects, each entry has the keys that declaration
names, and an entry key containing a list is an array inside it.

**Prose arrives unwrapped.** The 120-column wrap is joined, and blank lines stay. A list, heading, quote, table or fence
arrives exactly as written.

**A link in a section or a part line arrives inline.** A full, collapsed or shortcut reference link arrives as an inline
link to the target its definition states, with the definition's title. An image reference arrives as an inline image. A
reference with no definition, and one inside a code span, arrives as written. The definitions themselves never travel.

**A relative link arrives as the address of the page it points at.** This applies where the corpus has an address. The
target must resolve to a Markdown file the corpus contains, such as a record, a type page or a framework page. The
export fills in the [template](#the-human-form-as-a-template) with that file's path and the target's anchor. A query
on the target, such as `?plain=1`, is dropped. A link to a type's folder, such as `../runbooks/`, arrives as the
address of that type's page.

Any other link or image keeps the target its author or its definition wrote. That covers an absolute URL, an anchor
alone, an image, and a target that resolves to nothing in the corpus. A relative target that stays is relative to the
record's `path`. An inherited record arrives exactly as its producer exported it. A package exported by `kac` 0.38.0
or earlier has relative targets, and they stay relative until that producer publishes a new version.

On a part line, this covers a value read from `part.lead`, `part.aside.<Label>` or `part.bullets`. A value read from
`part.text` or `column.<Header>` is plain text, so a link there arrives as its words alone.

**Output is deterministic.** Ordering is ordinal everywhere but a term line's own position, which sorts
case-insensitively on the term. Two runs from one commit produce identical bytes but for `generatedAt`.

## The link and the ingredients

The rules joining the base to a path, and the anchor rule for a part, belong to `publishing-target`.
[`.corpus.yaml`](../use/corpus-descriptor.md#publishing) supplies where the corpus is served from.

### Ingredients for a fetch

The manifest has `base`, `pathPrefix` and `ref`. To fetch a record's source, join `pathPrefix` ahead of the
record's `path`, then ask the client that authenticates to `target` for that file at that `ref`. `gh` does it for
GitHub and `az rest` for both Azure DevOps targets. Where you have no such client, quote the link a person follows.

### The human form as a template

The manifest states it, and `export` fills it in for every record and every line. The template is
`https://…/blob/<sha>/{path}#{anchor}` for GitHub, and its own shape for each other target. You fill it in yourself
only for a line from a producer that exported before lines had `links`.

* **The ref is inside the template**, so a link resolves against the commit the export was built from.
* **`publishing.path-prefix` is inside it too**, between the commit and the record's path.
* **What `{path}` takes is the target's.** GitHub and Azure Repos take the record's path whole. An Azure DevOps wiki
  takes the path with `.md` removed and its separators percent-encoded.

`azure-devops-wiki` cannot pin its link to a commit, because no `?pagePath=` URL takes one. The manifest still gives an
agent the `ref`.

### Resolved links

A per-record file, a part line and each relative link in their prose resolve against the commit. All of them rewrite on
every export from a new commit.

### Five kinds of corpus with no address

A corpus that publishes nowhere, names a target nothing builds links for, states a target but no base, states a base
that target cannot join to, or is one git cannot answer for. Each exports without links:

* The manifest states the target it was given, with nulls beside it.
* A part line still has `path` and `anchor`, and its `links` is `null`.
* A relative link in a section or a part line keeps its target, relative to the record's `path`.

## Views generated from the export

A **view** is a Markdown file [`bundle`](../use/cli/bundle.md) writes into the plugin from the export, shaped for one
moment of the work. It is not part of `.dist/export/`, and another corpus that imports the export never receives one.
Each view lists the rules under their standard's id, title and status.

* **`build-index`** opens on how many rules the corpus states, then lists each rule by title. A rule's id is its
  standard's id, a dot, and the slug of its title.
* **`review`** lists the controls whose `verifies` names the standard or one of its rules, then each rule's
  obligations.

A view reads the files of each type it needs at the shape version its plugin manifest declares, so a change to those
files moves that type's `shapeVersion` like any other. A fresh bundle of one export writes every view byte for byte.

## Three versions, and none implies another

| Version          | Covers                                                                                        | Moved by          |
|------------------|-----------------------------------------------------------------------------------------------|-------------------|
| `formatVersion`  | the manifest's keys, the layout of the tree, and how a reader builds a link from the manifest | the tool          |
| `shapeVersion`   | one type's files, given on that type's entry under `types`                                    | the type's schema |
| `contentVersion` | what the corpus knows, as `content-version` in `.corpus.yaml`                                 | you, by hand      |

`contentVersion` is the one a consumer depends on by name. [`pack`](../use/cli/pack.md) publishes the export at that
version.

### Where a type is versioned

It is versioned where its keys are declared. A type declares the keys of its own part line in `export.parts.line:`, so
a change to one type's line moves that type's `shapeVersion` alone.

### A number a reader does not know

Meet a `formatVersion` above the one you were written against, and refuse the export. Meet a `shapeVersion` you do not
know, and leave that type alone and read the rest. [`bundle`](../use/cli/bundle.md) reads both and refuses what it
cannot read.

### What moves either number

The number moves when a reader written against the shape before it would now be wrong. Adding a key to a file, or a
file to a type's directory, leaves that reader correct and moves nothing. Renaming a key, dropping
one, changing the type of a value, or changing what a key means each moves it.

Reordering the keys within a line moves nothing. Reordering the records in a flat file moves the number, because that
order means something within a chain.

A change to one type's files moves that type's `shapeVersion`. A change to the manifest, to the layout of the tree, or
to how a reader builds a link from the manifest moves `formatVersion`. A link written into exported prose is a target
like any other, and changing it moves neither number.

## What a type cannot say

**A part line has no list or object.** A `line:` key reading `front.<field>` takes one value. The schema pass refuses a
source naming a list or an object field, whether the type declares it or `_universal.yaml` does. The record file has
that field as an array or an object.

**A clause states no framework alignment.** [`frameworks.jsonl`](#frameworksjsonl) states that edge from the
framework's end, with its standing. A framework names its clauses, and a clause names no framework.

**A clause has no `seeAlso`.** A clause pointing at another policy reaches a consumer as the id inside its words.

**A part line travels at `full` alone.** A type declaring `summary` or `reference` against its parts fails the schema
pass.

[`export`](../use/cli/export.md) is the command that writes this, and [`pack`](../use/cli/pack.md) zips it for a
consumer.
