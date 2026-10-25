# Metadata

Frontmatter is what makes a corpus machine-readable. The validator checks it, the generator builds indexes from it, and
an agent greps it to find things. What each type declares is the type's own business. This page is the model every type
answers to.

## What goes in a record

A record holds three kinds of thing, and each has one home.

**Frontmatter is metadata about the record**: what identifies it, places it in the taxonomy, and describes it as a
whole. A field holds one value, or a set of them. An index sorts on it, a citation resolves against it, and an agent
greps it.

**The body holds the record's parts**, where a part needs a rule or an address of its own. What counts as a part is the
type's business, declared under `parts:` in its schema. The declaration names the section holding them, and says whether
they are rows of a table or headings beneath it. The validator reads them out of the body and checks them.

Two shapes are in use. A policy's obligations are rows of `## Clauses`, each with an id written beside it, cited as
`pol-VURM.TIMEBOX`, so a clause can be reworded without its address moving. A standard's rules are headings, each
opening with its id, so `### PERMS: A job declares the permission it needs` is cited as `std-CI.PERMS` and linked at
`#perms-a-job-declares-the-permission-it-needs`. A glossary's terms are headings, addressed by the anchor each heading
slugs to, so `gls-knowledge-as-code.corpus` and a link to `#corpus` name the same entry.

**Everything else is prose**, in the section where it belongs. Ask whether anything has to address the piece: a citation
that must resolve, a check that must fire. Where nothing does, a table is prose set in columns and a heading is a
heading, and the schema says nothing about either.

## Derive what the system already knows

A field costs every record of its type a column, because a wiki renders frontmatter as a table at the top of the page.
So a field is a decision about the reader as much as about the schema.

A field states what is semantically the record's own. `decided-on` is a real fact about a decision, where the file's
last-modified date is git's business. Three fields are deliberately absent:

| Not a field           | Because                                          |
|-----------------------|--------------------------------------------------|
| `title`               | It is the H1, verbatim                           |
| `created` / `updated` | Git knows, and will not forget to update it      |
| `lifecycle`           | Follows from tier. A second field could disagree |

`type` and `tier` are the deliberate exceptions. The folder gives a record both, and both are written down anyway, so a
record read away from its folder still says what kind of thing it is. The validator checks each against the value the
schema derives.

### A derived field

A type can also take a field's value from the folder a record is saved in. A field declaring `from: sub-path` takes the
folders between the type's own folder and the file. So a policy at `policies/security/accs-access-by-identity.md` gets
`category: security` with no line of frontmatter saying so. Writing the key by hand is an error, reported as
`derived-key`, because the folder and the line could then say different things. Folders can nest, so
`standards/platform/dotnet/testing.md` gets `platform/dotnet`.

Save a record straight into its type folder and the field is empty, which is a normal state. You start using
categories by making a folder. Everything that reads the field gets the value: the generated index, its sort, and
`kac export`.

## Filling `sources`

`sources` is a universal optional field. It states where a record's content came from, one entry per source. An entry
takes one of two forms: the address of something a reader can follow, such as a specification, a vendor page or a
ticket, or a description of the population the content was drawn from. Write the address in full. No type renders
`sources`, so a short form such as `owner/repo#123` is a link nowhere.

Name a source where a reader could check the record against something the corpus does not keep. Where the record
states the estate's own facts, leave the field out. `reports` is the one type that requires an entry.

| Type                                                     | What `sources` holds                                                                                        |
|----------------------------------------------------------|-------------------------------------------------------------------------------------------------------------|
| Data, Glossary, Integration, NFR, Standard, Tool         | the published document the record restates: a specification, a regulation, a vendor page, a contract        |
| ADR, Offering, Deviation, Explanation, Fix, Postmortem | the material the record was written from: a benchmark, a ticket, a thread, an incident channel              |
| Control, Process, Runbook, Service                       | usually nothing. These records state the estate's own facts, and cite the repository or rule behind them    |
| Policy                                                   | the minute or the opinion the commitment was written from, and never a framework, which `aligns-with` names |
| Report                                                   | each corpus the report answers for, and the `content-version` it was true of                                |

### `sources` and `lineage`

`sources` sits on the record. `lineage` sits on the type, in `.schema/<type>.yaml`, and states what that kind of record
is modelled on.

One document can belong in either, and what you took from it decides which. Take the form, meaning the sections, the
fields, or the rule about superseding, and it belongs to the type's [lineage](lineage.md). Take the content, meaning a
threshold, a definition, or a list, and it belongs to this record's `sources`.

## Filling `accepts`

`accepts` is a universal optional field. It lists the warnings a record keeps, one entry per check, each with the
reason. `validate` reports an accepted warning as info, so CI stops raising it. The field stays in the record and never
travels in an export.

Accept a warning where the record already says why it stands, such as a gap its Operational notes describe. Leave a
warning standing where it points at work nobody has done, such as a runbook nobody has walked.
[`validate`](../use/cli/validate.md#the-accepts-field) says which entries it refuses.

## Naming a type

**A type name is singular** and its folder and page are plural: an *ADR* in `adrs/`, a *standard* in `standards/`. Each
record writes the singular name in its `type` field. Two folders take the singular: `data/` is a mass noun, and a
corpus has one `glossary/` kept in several files, one per bounded context.

## Allocating an id

An id is `<type-prefix>-<discriminator>`. **The prefix is singular**, since an id names a single record: `adr-0017`,
`std-SECRET`. The discriminator names the document, and its shape is the type's `id.style`.

Numeric discriminators are zero-padded, allocated sequentially, and **never reused**. Where a record is withdrawn before
acceptance, its number is retired. Mnemonics are allocated by meaning rather than in sequence, so there is no next one
to take. A slug is the thing's own name.

**You may correct a filename. You may not correct an id.** Every cross-reference resolves against it. A record whose
meaning has moved past its mnemonic is replaced, and the old one retired.

## The three id styles

The schema sets one per type, and `id.width` says how long the discriminator runs.

*Numbered* (`adr-0017`) suits anything chronological. The id records the order things happened, which is information
none of the others can give.

*Slug* (`svc-billing-api`) suits anything with a natural stable name, where a number would be an arbitrary handle for
something already well identified.

*Mnemonic* (`pol-VURM`) suits a small, long-lived set that other records cite constantly, so the id says something.
Draw it from the concept, since the wording will change.

**`id.width` is an exact count or a span.** `width: 4` gives every policy a four-character handle. A span, written as
`min:` and `max:`, lets the length follow the concept. Standards set `min: 2` and `max: 7`, so `std-PR` and
`std-SECRET` are both well formed. A slug reads neither.

## What the filename says

**`filename.carries-id` says whether the filename opens with the discriminator.** A policy's does, so `pol-VURM` is
filed as `vurm-vulnerability-remediation.md`. The mnemonic is upper-case in the id and lower-case in the filename, and
its first letter matches the slug's so the folder still reads alphabetically.

A standard's filename takes nothing of the id. It is a topical slug under a category folder, because one filename
cannot say which of several widths its opening segment is.

**A filename slug names what the record is.** The H1 states what it commits to. So `pol-EVER` is titled `Everything is
in version control` and filed as `ever-version-control.md`.

**A filename slug is at most 30 characters**, excluding the `NNNN-` or `mnem-` prefix where the filename has one. A slug
that will not fit is often a record doing two things, so split or narrow it.

## Referring to an id

Two separators extend an id. `.` selects a part. `:` selects a corpus.

**`.` addresses a part of a record.** `pol-VURM.TIMEBOX` names the policy, then the clause inside it.
`gls-knowledge-as-code.corpus` names the glossary, then the term. Each citation resolves against the record it names, so
a reference to a part that does not exist fails the build. So does a reference into a type that keeps no parts.

A citation takes three forms, and `kac validate` resolves each. A code span states the reference. A link carrying the
whole citation takes the reader to it. A link naming the record, with the part id written after the closing bracket,
does the same and reuses one definition:

```markdown
The clause is `pol-VURM.TIMEBOX`.
The default branch is protected. See [pol-VURM.TIMEBOX].
The default branch is protected. See [pol-VURM].TIMEBOX, and the window in [pol-VURM].WINDOW.

[pol-VURM.TIMEBOX]: vurm-a-title.md#clauses
[pol-VURM]: vurm-a-title.md#clauses
```

The third form needs one link definition however many clauses of that policy the document cites. Point the
definition at the section containing the parts. The part id has to touch the bracket, so `See [pol-VURM]. The
policy...` stays a full stop.

A field may require the part. A standard's `implements:` names the clauses it puts into practice, and a deviation's
`departs-from:` names the clauses it breaks. `kac validate` refuses a bare policy id in either field, and on a `Covers`
line, because it would read as every clause covered.

A field may also be checked against what the prose cites. `implements:` declares `mirrors-citations: Covers`, so each
rule closes on a footnote naming the clauses it discharges, and `kac validate` checks that the union of those lines
equals the field:

```markdown
### Nowhere else holds a secret

- A repository **MUST NOT** contain a secret, in source or in a pipeline definition.
- A build **MUST NOT** bake a secret into an artefact or an image.

_**Covers:** [pol-SCRT].EMBED, [pol-SCRT].LEAKED, [pol-SCRT].LOGS_
```

The line is italic with the label bold, and it closes the section it covers. A section covering nothing has no line.
`kac validate` reports a line that is not italic, names no clause, or names a policy whole.

**A field declaring `reciprocal:` agrees with its counterpart.** `supersedes` and `superseded-by` are one such pair,
and `replaces` and `successor` another. `kac validate` reports `reciprocal` where one record points at another and the
other does not point back. [Knowledge is a graph](principles.md#knowledge-is-a-graph) says why.

**`:` scopes a reference to the corpus supplying the record.** `eng:pol-VURM.TIMEBOX` reads scope, record, part. A
record the reading corpus keeps is cited bare, and qualifying one is an error. [Spelling a
citation](../use/imports.md#spelling-a-citation) says how a scoped reference resolves.

### The shortcode

It is the half before the colon. The producer declares it as `shortcode:` in
[`.corpus.yaml`](../use/corpus-descriptor.md#identity), and every corpus citing that one writes what the producer chose,
so one obligation has one spelling.

**A shortcode is immutable**, because a rename breaks citations in repositories its owner cannot edit. So declare one
when something is about to cite the corpus. [`kac new`](../use/cli/new.md) writes the key bare.

**It is two to eight characters, lower case, opening on a letter with letters and digits after it.** It has no hyphen,
and it is no type's prefix, since `std:pol-VURM` reads as a standard. `kac validate` refuses both.

## Adding a field

First ask whether the content belongs in frontmatter at all, as [What goes in a record](#what-goes-in-a-record) says.
Then check that git, the folder, the H1 or an existing link does not already state the fact.

On a type the corpus declares itself, declare a new field in the type's `.schema/<folder>.yaml` and add it to that
type's `_template.md`. Then run [`kac generate`](../use/cli/generate.md) so the generated tables pick it up. The
validator reads the schema, so it needs no change of its own.

On a type the framework ships, a new field is a change to the framework, because `kac update` reverts a corpus's edit
to a schema file it receives. That includes a field every type has, which goes in `.schema/_universal.yaml`. A type file
may refine it with a declaration of its own.

[Taxonomy](taxonomy.md) is the page for deciding which type a new field belongs to.
