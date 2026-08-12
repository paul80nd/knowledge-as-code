# Metadata

Every document in the taxonomy opens with a YAML frontmatter block. This is what makes the wiki machine-readable: CI
validates it, indexes are generated from it, and agent sessions grep it to find things.

Azure DevOps renders frontmatter as a table at the top of the page, so **every field you add appears as a column on
every document of that type**. Fields are therefore a design decision about the reader as much as about the schema. Add
them sparingly, and derive rather than state wherever possible.

## Principles

1. **Derive what the system already knows.** Document type comes from the folder. Tier comes from the type. Title comes
   from the H1. Creation and modification dates come from git. None of these are stated in frontmatter.
2. **State only what is semantically yours.** `decided-on` is a real fact about an ADR and belongs in frontmatter. The
   file's last-modified date is git's business.
3. **Quote all dates.** Unquoted `2026-06-12` is parsed as a datetime and rendered with a locale format and a timezone
   shift. `"2026-06-12"` renders as written.
4. **Enums are lowercase, hyphenated.** They are grep targets first and prose second.
5. **Lists use YAML sequences** — ADO renders either form as separate cells, so the choice is about reading the source.
   Search metadata takes the compact flow form, `tags: [ a, b ]`: it is how a document is found rather than what it
   says, and a block list gives the least interesting fields in the block the most lines. Every other list stays block,
   one entry per line — entries stay individually reviewable in a diff, and a validation finding can point at the entry
   that caused it rather than at the field.
6. **Lists are alphabetical.** No list field's sequence carries meaning, so alphabetical is the order that scan-reads
   and the one two authors will agree on without discussion. Numbers inside an entry compare as numbers, so
   `ISO27001:2022 A.8.7` comes before `ISO27001:2022 A.8.29`. CI warns on a list that is out of order.

## Naming

* **Type name** — singular. An *ADR*, a *standard*, a *control*.

* **Folder and page** — plural. `adrs/`, `standards/`, `controls/`. The folder is a collection, and CI infers a
  document's type from it, so the mapping is a rule rather than a lookup. Two types take the singular: `data/` is a mass
  noun, and a corpus has one `glossary/` held in several files, one per bounded context. The singular says those files
  are one vocabulary presented in sections.

* **ID prefix** — singular, since an ID names a single document. `adr-0017`, `std-0004`.

* **ID style** — set per type by the schema, and one of three shapes.

  *Numbered* (`adr-0017`) suits anything chronological: the id records the order things happened, which is information
  none of the others can carry.

  *Slug* (`svc-billing-api`) suits anything with a natural stable name, where a number would be an arbitrary handle for
  something already well identified.

  *Mnemonic* (`pol-VURM`, filed as `vurm-vulnerability-remediation.md`) suits a small, long-lived set that other
  documents cite constantly — the id is what a reader meets most often, so it should say something. Upper-case in the
  id, lower-case in the filename, and its first letter matches the slug's so the folder still reads alphabetically. A
  mnemonic makes a claim a number never does, so it is drawn from the concept rather than the current wording, and it is
  immutable once the document is active.

* **Slug length** — the filename slug (excluding the `NNNN-` or `mnem-` prefix) is at most 30 characters. The filename
  is a handle, not a title: it identifies the document at a glance while the H1 carries the full descriptive title. CI
  fails on longer slugs.

  A slug you cannot get under 30 characters is often a signal the document is doing two things.
  `internal-services-backing-public-surfaces` was one idea too many; splitting or narrowing the scope is usually the
  better fix than abbreviating harder.

## Universal fields

Carried by every document in the taxonomy.

<!-- BEGIN GENERATED: schema-universal -->

| Field    | Req | Type   | Notes                                                                                |
|----------|-----|--------|--------------------------------------------------------------------------------------|
| `id`     | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                |
| `tier`   | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder. |
| `status` | ●   | enum   | Values vary by type.                                                                 |
| `owner`  | ●   | string | A named person, never a team alias.                                                  |
| `tags`   |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                     |

**Enum values**

| Field  | Values                                                              |
|--------|---------------------------------------------------------------------|
| `tier` | `decided` · `normative` · `descriptive` · `procedural` · `observed` |

<!-- END GENERATED: schema-universal -->

`id` is the anchor for every cross-reference — see [IDs](#ids); `status` values are set by each type and are listed
under [per-type fields](#per-type-fields).

`tags` are **entry points**: the word a reader arrives with, on a document that does not use it. One document may be the
only one carrying a tag, and often is — a searcher who types `payments` wanted the one service that handles them. What a
tag must never do is restate another field, since the two can only ever disagree.

**Grouping is a different job, and a tag does it badly.** A value like `public` or `internal` is worth carrying because
several documents share it and it divides a type into groups worth browsing; that same value, used once, has failed at
the only thing it was for. The two tests are opposite, which is why they belong in two fields. A type wanting the
grouping job declares a list field of its own with `min-records:`, the floor on how many records must carry each value,
and CI warns below it. The service type declares one, named `facets`, and its page records how it reached its
vocabulary. Membership is never declared in the schema: what a corpus groups by is the corpus's to settle.

Deliberately absent, and why:

| Not a field           | Because                                          |
|-----------------------|--------------------------------------------------|
| `type`                | Inferred from the folder                         |
| `title`               | It's the H1, verbatim                            |
| `created` / `updated` | Git knows, and won't forget to update it         |
| `lifecycle`           | Follows from tier; a second field could disagree |

## IDs

Format: `<type-prefix>-<discriminator>` — `adr-0017`, `pol-VURM`, `svc-billing-api`. Which of the three
[ID styles](#naming) a type uses is set by its schema.

Numeric IDs are zero-padded to four digits, allocated sequentially, and **never reused** — if a document is withdrawn
before acceptance, its number is retired. Mnemonic IDs are allocated by meaning rather than in sequence, so there is no
next one to take: pick a four-character mnemonic for the concept that no document of that type already holds. A slug is
the thing's own name.

The ID is the anchor for every cross-reference in the wiki, and it is what a shortcut link label must say. Filenames may
be corrected; IDs may not — which binds hardest on a mnemonic, because unlike a number it makes a claim that can go
stale. A document whose meaning has moved that far is replaced and the old one retired, not renamed.

## The identity line

Every record carries one line directly beneath its H1 — the type, the ID, then the status in upper case:

```markdown
# Software we build is usable by everyone

`Policy: pol-A11Y` `DRAFT`
```

You arrive at these documents from a citation, so the top of the page has to answer three questions before the prose
starts: what kind of document is this, which one is it, and is it in force. Frontmatter answers all three, but it
renders as a metadata table an Azure DevOps reader may never look at, and it is written for a machine. The identity line
is the same three facts written for a person, at the one place their eye already is.

The ID appears exactly as the frontmatter carries it, so there is one casing of an ID across the wiki rather than a
second invented for headings. The status is the exception: lower-case in frontmatter because a machine reads it,
upper-case on the line because it is read as a stamp. CI holds all three to the frontmatter — a document cannot go
`active` and leave the line saying `DRAFT`.

**The H1 is the title and nothing else** — no ID, no prefix, no type name. It carried the ID as a code span until the
identity line took that job, and the split is worth it: a title that competes with a handle is a worse title, and
generated indexes had to strip the ID back off to fill a column that already held it.

## Per-type fields

Each type's fields are documented on its own page, generated into it from `.schema/` — so a reader working in one folder
has what they need without leaving it, and there is still one definition.

<!-- BEGIN GENERATED: types-metadata -->

[ADR](/adrs#metadata) · [Capability](/capabilities#metadata) · [Control](/controls#metadata) · [Data](/data#metadata) ·
[Discovery](/discoveries#metadata) · [Explanation](/explanations#metadata) · [FAQ](/faqs#metadata) ·
[Glossary](/glossary#metadata) · [Integration](/integrations#metadata) · [NFR](/nfrs#metadata) ·
[Policy](/policies#metadata) · [Postmortem](/postmortems#metadata) · [Process](/processes#metadata) ·
[Runbook](/runbooks#metadata) · [Service](/services#metadata) · [Standard](/standards#metadata) ·
[Tool](/tools#metadata)

<!-- END GENERATED: types-metadata -->

## Example

```yaml
---
id: adr-0017
tier: decided
status: accepted
decided-on: "2026-07-14"
owner: alex.doe
deciders:
  - alex.doe
  - sam.patel
related:
  - adr-0007
  - adr-0008
tags: [ public-api, http ]
---
```

## Adding a field

A new field appears as a column on every document of that type, so it needs to justify itself. Before adding one, check
that the information is not already derivable from git, the folder, the H1, or an existing link. If it is genuinely new,
declare it in the type's `.schema/<folder>.yaml`, add it to that type's `_template.md`, and run `./kac index` so the
generated tables carry it. The validator reads the schema, so it needs no change of its own.
