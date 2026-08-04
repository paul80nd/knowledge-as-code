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
5. **Lists use YAML sequences**, which ADO renders as separate cells.

## Naming

* **Type name** — singular. An *ADR*, a *standard*, a *control*.
* **Folder and page** — plural. `adrs/`, `standards/`, `controls/`. The folder is a collection, and CI infers a
  document's type from it, so the mapping is a rule rather than a lookup.
* **ID prefix** — singular, since an ID names a single document. `adr-0017`, `std-0004`.

* **Slug length** — the filename slug (excluding any `NNNN-` prefix) is at most 30 characters. The filename is a handle,
  not a title: it identifies the document at a glance while the H1 carries the full descriptive title. CI fails on
  longer slugs.

  A slug you cannot get under 30 characters is often a signal the document is doing two things.
  `internal-services-backing-public-surfaces` was one idea too many; splitting or narrowing the scope is usually the
  better fix than abbreviating harder.

Two exceptions, both because English has no plural to give: `data/` (mass noun) and
`glossary.md` (one document, not a collection).

## Universal fields

Carried by every document in the taxonomy.

<!-- BEGIN GENERATED: schema-universal -->

| Field    | Req | Type   | Notes                                                                                |
| -------- | --- | ------ | ------------------------------------------------------------------------------------ |
| `id`     | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                |
| `tier`   | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder. |
| `status` | ●   | enum   | Values vary by type.                                                                 |
| `owner`  | ●   | string | A named person, never a team alias.                                                  |
| `tags`   |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                     |

**Enum values**

| Field  | Values                                                              |
| ------ | ------------------------------------------------------------------- |
| `tier` | `decided` · `normative` · `descriptive` · `procedural` · `observed` |

<!-- END GENERATED: schema-universal -->

`id` is the anchor for every cross-reference — see [IDs](#ids); `status` values are set by each type and are
listed under [per-type fields](#per-type-fields).

Deliberately absent, and why:

| Not a field           | Because                                          |
|-----------------------|--------------------------------------------------|
| `type`                | Inferred from the folder                         |
| `title`               | It's the H1                                      |
| `created` / `updated` | Git knows, and won't forget to update it         |
| `lifecycle`           | Follows from tier; a second field could disagree |

## IDs

Format: `<type-prefix>-<number>` — `adr-0017`, `std-0004`, `ctl-0012`, `svc-billing-api`.

Numeric IDs are zero-padded to four digits, allocated sequentially, and **never reused** — if a document is withdrawn
before acceptance, its number is retired. Services, explanations and glossary terms use slugs rather than numbers, since
they have natural stable names.

The ID is the anchor for every cross-reference in the wiki. Filenames may be corrected; IDs may not.

## Per-type fields

Each type's fields are documented on its own page, generated from the schema:

[ADR](/adrs#metadata) · [Standard](/standards#metadata) · [Policy](/policies#metadata) ·
[Control](/controls#metadata) · [NFR](/nfrs#metadata) · [Service](/services#metadata) ·
[Capability](/capabilities#metadata) · [Process](/processes#metadata) · [Explanation](/explanations#metadata) ·
[Runbook](/runbooks#metadata) · [Tools](/tools#metadata) ·
[Integration](/integrations#metadata) · [Data](/data#metadata) · [FAQ](/faqs#metadata) ·
[Discovery](/discoveries#metadata) · [Postmortem](/postmortems#metadata)

They are generated into those pages from `knowledge-as-code/schema/`, so a reader working in one folder has what they
need without leaving it, and there is still one definition.

[Glossary](/glossary) is the seventeenth type and the exception: a single document rather than a collection, so its two
fields — `status` and `review-by` — live in its own frontmatter and are described on the page itself, not in a generated
per-type table.

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
tags:
  - public-api
  - http
---
```

## Adding a field

A new field appears as a column on every document of that type, so it needs to justify itself. Before adding one, check
that the information isn't already derivable from git, the folder, the H1, or an existing link. If it is genuinely new,
add it here, add it to the validator, and note it in the changelog below.

## Changelog

- Initial version.
