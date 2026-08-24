# Metadata

Frontmatter is what makes a corpus machine-readable. The validator checks it, the generator builds indexes from it, and
an agent greps it to find things. What each type declares is the type's own business; this page is the model every type
answers to.

## What a record carries

A record holds three kinds of thing, and each has one home.

**Frontmatter is metadata about the record**: what identifies it, places it in the taxonomy, and describes it as a
whole. A field carries one value, or a set of them. An index sorts on it, a citation resolves against it, and an agent
greps it.

**The body carries the record's parts**, where a part needs a rule or an address of its own. What counts as a part is
the type's business, declared under `parts:` in its schema: which section holds them, and whether they are written as
the rows of a table or as headings beneath it. The validator reads them out of the body and holds them to that
declaration.

Two shapes are in use, and they differ in where the address comes from. A policy's obligations are rows: `## Clauses`
holds one per obligation with an id written beside it, which a standard or a control cites as `pol-VURM.TIMEBOX`. A
glossary's terms are headings: `## Terms` holds one each, addressed by the anchor the heading slugs to, so
`gls-knowledge-as-code.corpus` and a link to `#corpus` name the same entry.

A policy writes its ids down because a clause gets reworded and its address must not move with it. A term is its own
name, so a glossary derives the address from the heading. Writing one beside the heading as well would give the two a
way to disagree.

**Everything else is prose**, in the section where it belongs. Ask whether anything has to address the piece: a
citation that must resolve, a check that must fire. Where nothing does, a table is prose set in columns and a heading
is a heading, and the schema says nothing about either.

## Derive what the system already knows

A field costs every record of its type a column, because a wiki renders frontmatter as a table at the top of the page.
So a field is a decision about the reader as much as about the schema.

**Type comes from the folder. Tier comes from the type. Title is the H1. Creation and modification dates come from
git.** None of those belong in frontmatter. What does belong is what is semantically the record's own: `decided-on` is
a real fact about a decision, where the file's last-modified date is git's business.

Four fields are deliberately absent, and the reasoning is the same each time.

| Not a field           | Because                                          |
|-----------------------|--------------------------------------------------|
| `type`                | Inferred from the folder                         |
| `title`               | It is the H1, verbatim                           |
| `created` / `updated` | Git knows, and will not forget to update it      |
| `lifecycle`           | Follows from tier. A second field could disagree |

## Naming

**A type name is singular** and its folder and page are plural: an *ADR* in `adrs/`, a *standard* in `standards/`. The
folder is a collection, and a record's type is inferred from it, so the mapping is a rule rather than a lookup. Two
types take the singular. `data/` is a mass noun, and a corpus has one `glossary/` held in several files, one per
bounded context.

**An id prefix is singular**, since an id names a single record: `adr-0017`, `std-0004`.

**An id style is one of three**, and the schema sets one per type.

*Numbered* (`adr-0017`) suits anything chronological. The id records the order things happened, which is information
none of the others can carry.

*Slug* (`svc-billing-api`) suits anything with a natural stable name, where a number would be an arbitrary handle for
something already well identified.

*Mnemonic* (`pol-VURM`, filed as `vurm-vulnerability-remediation.md`) suits a small, long-lived set that other records
cite constantly. The id is what a reader meets most often, so it should say something. It is upper-case in the id and
lower-case in the filename, and its first letter matches the slug's so the folder still reads alphabetically. A
mnemonic makes a claim a number never does, so draw it from the concept rather than the current wording.

**A filename slug is at most 30 characters**, excluding the `NNNN-` or `mnem-` prefix. The filename is a handle and the
H1 carries the full descriptive title. A slug you cannot get under 30 characters is often a signal the record is doing
two things: `internal-services-backing-public-surfaces` was one idea too many. Splitting or narrowing the scope beats
abbreviating harder.

## IDs

An id is `<type-prefix>-<discriminator>`. Numeric ids are zero-padded to four digits, allocated sequentially, and
**never reused**: where a record is withdrawn before acceptance, its number is retired. Mnemonic ids are allocated by
meaning rather than in sequence, so there is no next one to take. A slug is the thing's own name.

The id is the anchor for every cross-reference in a corpus. **You may correct a filename. You may not correct an id.**
That binds hardest on a mnemonic, because a mnemonic makes a claim that can go stale and a number does not. A record
whose meaning has moved that far is replaced and the old one retired.

## Referring to an id

Two separators reach past an id, each with one job.

**`.` addresses a part of a record.** `pol-VURM.TIMEBOX` names the policy, then the clause inside it.
`gls-knowledge-as-code.corpus` names the glossary, then the term. Each citation resolves against the record it names,
so a reference to a part that does not exist fails the build. So does a reference into a type that keeps no parts.

**`:` scopes a reference to the corpus supplying the record.** `eng:pol-VURM.TIMEBOX` reads scope, record, part. A
record the reading corpus holds is cited bare, and qualifying one is an error, because two spellings of a single
obligation defeat search. The form is reserved and carries nothing today, since no corpus yet imports another.

**The corpus a shortcode names declares it**, in its `.corpus.yaml`, and the shortcode reaches every corpus consuming
that one. A shortcode is immutable for the same reason an id is, and more strictly: a rename invalidates citations in
repositories its owner cannot edit. It may not take a type prefix's spelling, since `std:pol-VURM` reads as a standard.

## Adding a field

First ask whether the content belongs in frontmatter at all. [What a record carries](#what-a-record-carries) answers
that. Then check that git, the folder, the H1 or an existing link does not already hold the fact.

Where the field is new, declare it in the type's `.schema/<folder>.yaml`, add it to that type's `_template.md`, and run
[`kac generate`](../cli/generate.md) so the generated tables carry it. The validator reads the schema, so it needs no
change of its own.
