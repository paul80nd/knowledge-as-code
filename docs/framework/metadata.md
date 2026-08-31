# Metadata

Frontmatter is what makes a corpus machine-readable. The validator checks it, the generator builds indexes from it, and
an agent greps it to find things. What each type declares is the type's own business. This page is the model every type
answers to.

## What a record carries

A record holds three kinds of thing, and each has one home.

**Frontmatter is metadata about the record**: what identifies it, places it in the taxonomy, and describes it as a
whole. A field carries one value, or a set of them. An index sorts on it, a citation resolves against it, and an agent
greps it.

**The body carries the record's parts**, where a part needs a rule or an address of its own. What counts as a part is
the type's business, declared under `parts:` in its schema. The declaration names the section holding them, and says
whether they are rows of a table or headings beneath it. The validator reads them out of the body and holds them to it.

Two shapes are in use, and they differ in where the address comes from. A policy's obligations are rows. `## Clauses`
holds one per obligation with an id written beside it, which a standard or a control cites as `pol-VURM.TIMEBOX`. A
glossary's terms are headings, addressed by the anchor each heading slugs to, so `gls-knowledge-as-code.corpus` and a
link to `#corpus` name the same entry.

A policy writes its ids down because a clause gets reworded and its address must not move with it. A term is its own
name, so a glossary derives the address from the heading. Writing one beside the heading as well would give the two a
way to disagree.

**Everything else is prose**, in the section where it belongs. Ask whether anything has to address the piece: a citation
that must resolve, a check that must fire. Where nothing does, a table is prose set in columns and a heading is a
heading, and the schema says nothing about either.

## Derive what the system already knows

A field costs every record of its type a column, because a wiki renders frontmatter as a table at the top of the page.
So a field is a decision about the reader as much as about the schema.

**Type comes from the folder. Title is the H1. Creation and modification dates come from git.** None of those belong in
frontmatter. What does belong is what is semantically the record's own: `decided-on` is a real fact about a decision,
where the file's last-modified date is git's business.

`tier` is the deliberate exception. It follows from the type and is stated anyway. A reader meets it at the top of the
page as a trust signal, and the validator holds the two to agreeing.

Four fields are deliberately absent, and the reasoning is the same each time.

| Not a field           | Because                                          |
|-----------------------|--------------------------------------------------|
| `type`                | Inferred from the folder                         |
| `title`               | It is the H1, verbatim                           |
| `created` / `updated` | Git knows, and will not forget to update it      |
| `lifecycle`           | Follows from tier. A second field could disagree |

### A field the schema derives, which you never write

A type can also take a field's value from the folder a record is saved in. A field declaring `from: sub-path` carries
the folders between the type's own folder and the file. So a policy at `policies/security/accs-access-by-identity.md`
carries `category: security` with no line of frontmatter saying so. Folders can nest, so
`standards/platform/node/testing.md` carries `platform/node`.

Save a record straight into its type folder and the field is empty. That is a normal state, not a gap. You start
using categories by making a folder, and you choose the folders yourself.

The value reaches everything that reads the field: the generated index, its sort, and `kac export`. Writing the key by
hand is an error, reported as `derived-key`, because the folder and the line could then say different things.

## Naming a type

**A type name is singular** and its folder and page are plural: an *ADR* in `adrs/`, a *standard* in `standards/`. The
folder is a collection, and a record's type is inferred from it, so the mapping is a rule rather than a lookup. Two
types take the singular. `data/` is a mass noun, and a corpus has one `glossary/` held in several files, one per bounded
context.

## Allocating an id

An id is `<type-prefix>-<discriminator>`. **The prefix is singular**, since an id names a single record: `adr-0017`,
`std-SECRET`. The discriminator names the document, and its shape is the type's `id.style`.

Numeric discriminators are zero-padded, allocated sequentially, and **never reused**: where a record is withdrawn before
acceptance, its number is retired. Mnemonics are allocated by meaning rather than in sequence, so there is no next one
to take. A slug is the thing's own name.

The id is the anchor for every cross-reference in a corpus. **You may correct a filename. You may not correct an id.**
That binds hardest on a mnemonic, because a mnemonic makes a claim that can go stale and a number does not. A record
whose meaning has moved that far is replaced and the old one retired.

## The three id styles

The schema sets one per type, and `id.width` says how long the discriminator runs.

*Numbered* (`adr-0017`) suits anything chronological. The id records the order things happened, which is information
none of the others can carry.

*Slug* (`svc-billing-api`) suits anything with a natural stable name, where a number would be an arbitrary handle for
something already well identified.

*Mnemonic* (`pol-VURM`) suits a small, long-lived set that other records cite constantly. The id is what a reader meets
most often, so it should say something. A mnemonic makes a claim a number never does, so draw it from the concept
rather than the current wording.

**`id.width` is an exact count or a span.** `width: 4` gives every policy a four-character handle. A span, written as
`min:` and `max:`, lets the length follow the concept. Standards set `min: 2` and `max: 7`, so `std-PR` and
`std-SECRET` are both well formed. A slug reads neither.

## What the filename carries

**`filename.carries-id` says whether the filename opens with the discriminator.** A policy's does, so `pol-VURM` is
filed as `vurm-vulnerability-remediation.md`. The mnemonic is upper-case in the id and lower-case in the filename, and
its first letter matches the slug's so the folder still reads alphabetically.

A standard's filename carries nothing of the id. It is a topical slug under a category folder, and the mnemonic lives
in frontmatter alone. A span of widths needs that, because one filename cannot say which of several lengths its opening
segment is.

**A filename slug is at most 30 characters**, excluding the `NNNN-` or `mnem-` prefix where the filename carries one.
The filename is a handle and the H1 carries the full descriptive title. A slug you cannot get under 30 characters is
often a signal the record is doing two things: `internal-services-backing-public-surfaces` was one idea too many.
Splitting or narrowing the scope beats abbreviating harder.

## Referring to an id

Two separators reach past an id, each with one job.

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

The third form is the cheapest. It needs one link definition however many clauses of that policy the document goes on
to cite. Where the reader lands is the definition's to decide, so point it at the section holding the parts. The part
id has to touch the bracket, so `See [pol-VURM]. The policy...` stays a full stop and a sentence.

A field may require the part rather than admit it. A standard's `implements:` names the clauses it puts into practice,
one entry each, and `kac validate` refuses a bare policy id there. The whole policy is shorter to write than the list it
stands for, and it reads to anything counting coverage as every clause covered.

**`:` scopes a reference to the corpus supplying the record.** `eng:pol-VURM.TIMEBOX` reads scope, record, part. A
record the reading corpus holds is cited bare, and qualifying one is an error, because two spellings of a single
obligation defeat search. `kac validate` resolves a scoped reference against the corpus `kac restore` unpacked under
`.imports/`, and holds it to carrying the part named.

### A shortcode is the half before the colon

A corpus declares its shortcode as `shortcode:` in [`.corpus.yaml`](../corpus-descriptor.md#identity). The producer
declares it, and a corpus citing that one writes what the producer chose. A consumer picking an alias of its own would
put two consumers on two spellings of one obligation, which the colon exists to prevent.

**A shortcode is immutable**, for the reason an id is and more strictly. A rename invalidates citations in repositories
its owner cannot edit. So a corpus declares one when something is about to cite it, rather than at creation.
[`kac new`](../cli/new.md) writes the key bare and leaves the value to you.

**It is two to eight characters, lower case, opening on a letter and carrying letters and digits after it.** No hyphen.
The parser reads a hyphen before a colon as an id, and would take the reference for a citation written with the wrong
separator. No type's prefix either, since `std:pol-VURM` reads as a standard. `kac validate` refuses both, and
[`kac export`](../cli/export.md) states the declared shortcode in its manifest.

## Adding a field

First ask whether the content belongs in frontmatter at all. [What a record carries](#what-a-record-carries) answers
that. Then check that git, the folder, the H1 or an existing link does not already hold the fact.

Where the field is new, declare it in the type's `.schema/<folder>.yaml` and add it to that type's `_template.md`. Then
run [`kac generate`](../cli/generate.md) so the generated tables carry it. The validator reads the schema, so it needs
no change of its own.

[Taxonomy](taxonomy.md) is the page for deciding which type a new field belongs to.
