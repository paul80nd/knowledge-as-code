---
id: prc-add-a-record
type: process
tier: procedural
status: active
last-rehearsed: "never"
owner: human:paul.law
tags: [ authoring, taxonomy ]
---

# Add a record to a corpus

`Process: prc-add-a-record` `ACTIVE`

Write a new record of a type a corpus already declares.

## When to use this

You are adding knowledge to a corpus. The tier decides how a record is written. The type decides what it contains.
Nothing in CI reports that you used the wrong tier's rules.

## Prerequisites

* A corpus, and a type it lists under `types:` in its `.corpus.yaml`.
* `kac` run from inside that corpus as `dotnet run --project ../../tooling/kac --`.
* A corpus declaring `consumes:` needs `kac restore` first. `restore` needs its producer packed.

## Steps

1. Pick the type. [Taxonomy](../knowledge-as-code/taxonomy.md) has the decision table. Where two types both fit, put
   the record in the more general one.
2. Read the type's root page and its `_template.md`. The page says what the type contains and what it excludes. The
   template says which sections the schema requires.
3. Run `kac checks` for that type. Read the `rules:` block in its schema file. A rule with no `severity:` binds you and
   fails nothing.
4. Load `technical-writing`, then `writing-a-record`. Read the section for this type's tier.
5. List what the record needs against what you were told. Ask a person for the difference. A field you cannot answer is
   a question for that person. Write no placeholder.
6. Copy the template and fill it in. Keep every required section. A section with nothing to say is a content gap.
   Report it.
7. Write the frontmatter once the record has settled. Field order is topological, and `key-order` checks it.
8. List the records that now point at this one. An edge such as `depends-on` is written one way, and nothing generates
   the reverse view.
9. List the pages that cite records of this type by id, and fix the ones this record makes wrong. Nothing in CI reads
   prose for meaning. Where the record is a standard or a deviation, [rpt-clause-coverage] reports on its clauses.
   `writing-a-report` has the merge that brings that report up to date.
10. Run `kac validate`, then `kac generate`. The record's H1 is copied into a generated index, so the corpus is stale
    until you regenerate.
11. Run [prc-pull-request].

## Verification

`kac validate` reports zero errors, and `kac generate --check` reports the generated files fresh.

Close by stating the id and its folder, which tier's rules you applied, and anything the type declared that you could
not answer.

## Related

* [prc-pull-request] merges the record.
* [std-PROSE] states the prose rules every record here follows.

[prc-pull-request]: pull-request.md
[rpt-clause-coverage]: ../reports/clause-coverage.md
[std-PROSE]: ../standards/prose.md
