# Metadata

> The YAML block every document in the taxonomy opens with.

Frontmatter is what makes this corpus machine-readable. CI validates it, the generator builds indexes from it, and agent
sessions grep it to find things. The tables and the strip below are generated from this corpus's own schema, so they
cover the fields and checks it actually has.

[Metadata][metadata] explains the model behind them: what belongs in frontmatter and what belongs in the body, why a
field is derived wherever it can be, how ids and their prefixes are formed, and how a citation reaches a part of a
record. The rules for filling a block in are in the `writing-a-record` skill.

## Universal fields

Carried by every document in the taxonomy.

<!-- BEGIN GENERATED: schema-universal -->

| Field      | Value                                            | Notes                                                                         |
|------------|--------------------------------------------------|-------------------------------------------------------------------------------|
| `id` *     | string                                           | Stable, unique across the corpus, never reused, in the format the type sets.  |
| `type` *   | string                                           | The singular name of the type, which CI checks against the folder.            |
| `tier` *   | `decided` `normative` `descriptive` `procedural` | The record's trust level, fixed for the type and checked against the folder.  |
| `status` * | enum                                             | The record's current state, from the values this type sets.                   |
| `owner` *  | string                                           | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.        |
| `sources`  | list                                             | Where this record's content came from, one entry per source.                  |
| `tags`     | list                                             | Free-form, lowercase and hyphenated. A reader searches on these across types. |
| `accepts`  | list                                             | The warnings this record keeps, each with the reason it keeps it.             |

\* Field is required

<!-- END GENERATED: schema-universal -->

`id` is the anchor for every cross-reference. Each type sets its own `status` values, and lists its own fields on its
own page. Two fields answer neighbouring questions: `sources` says where one record's content came from, and
[lineage](lineage.md) says what the whole type is modelled on. `accepts` lists the warnings a record keeps, each with
its reason, and `validate` reports each one as info. See
<https://paul80nd.github.io/knowledge-as-code/use/cli/validate/#the-accepts-field>.

## Universal checks

CI runs these checks on every record, whatever its type. Each type's page lists the checks it adds, under What CI
checks.

<!-- BEGIN GENERATED: checks-universal -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""`, `''`, `—` or an unquoted `{{…}}`.                       |
| `empty-optional-key`        | warning | An optional field is filled in or left out, rather than written with no value.                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `acceptance-check`          | error   | An `accepts` entry names a check that reports warnings, and is not `acceptance-stale`.                          |
| `acceptance-stale`          | warning | Each `accepts` entry matches a warning its check reports on the same record.                                    |
| `type-matches-folder`       | error   | `type` matches the singular type name the record's folder declares.                                             |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`    | error   | Filename matches the pattern. The slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A link label is the id of the record it leads to, written as that record carries it.                            |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `retire-not-delete`         | warning | With `--since`, no record in force at that revision has been deleted.                                           |

<!-- END GENERATED: checks-universal -->

## Per-type fields

Each type's fields are documented on its own page, generated into it from `.schema/`. A reader working in one folder has
what they need without leaving it, and there is still one definition.

<!-- BEGIN GENERATED: types-metadata -->

[ADR](../adrs.md#metadata) · [Data](../data.md#metadata) · [Explanation](../explanations.md#metadata) ·
[Glossary](../glossary.md#metadata) · [Integration](../integrations.md#metadata) · [Offering](../offerings.md#metadata)
· [Process](../processes.md#metadata) · [Runbook](../runbooks.md#metadata) · [Service](../services.md#metadata)

<!-- END GENERATED: types-metadata -->

## Example

```yaml
---
id: adr-0017
type: adr
tier: decided
status: accepted
decided-on: "2026-07-14"
owner: human:alex.doe
deciders:
  - human:alex.doe
  - human:sam.patel
related:
  - adr-0007
  - adr-0008
tags: [ public-api, http ]
---
```

## Adding a field

A field on a framework type is a change to the framework, so propose it to the framework's maintainers.
`kac update` reverts a local edit to that type's `.schema/<folder>.yaml`. A corpus adds a field of its own to a type it
declares itself. [Metadata][metadata] says what to ask first.

[metadata]: https://paul80nd.github.io/knowledge-as-code/framework/metadata/
