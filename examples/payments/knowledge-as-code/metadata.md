# Metadata

> The YAML block every document in the taxonomy opens with.

Frontmatter is what makes this corpus machine-readable. CI validates it, the generator builds indexes from it, and agent
sessions grep it to find things. The table and the strip below are generated from this corpus's own schema, so they
cover the fields it actually carries.

[Metadata][metadata] carries the model behind them: what belongs in frontmatter and what belongs in the body, why a
field is derived wherever it can be, how ids and their prefixes are formed, and how a citation reaches a part of a
record. The rules for filling a block in are in the `writing-a-record` skill.

## Universal fields

Carried by every document in the taxonomy.

<!-- BEGIN GENERATED: schema-universal -->

| Field      | Value                                                       | Notes                                                                         |
|------------|-------------------------------------------------------------|-------------------------------------------------------------------------------|
| `id` *     | string                                                      | Stable, unique across the corpus, never reused, in the format the type sets.  |
| `type` *   | string                                                      | The singular name of the type, which CI checks against the folder.            |
| `tier` *   | `decided` `normative` `descriptive` `procedural` `observed` | The record's trust level, fixed for the type and checked against the folder.  |
| `status` * | enum                                                        | The record's current state, from the values this type sets.                   |
| `owner` *  | string                                                      | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.        |
| `sources`  | list                                                        | Where the content came from, one entry per source.                            |
| `tags`     | list                                                        | Free-form, lowercase and hyphenated. A reader searches on these across types. |

\* Field is required

<!-- END GENERATED: schema-universal -->

`id` is the anchor for every cross-reference. Each type sets its own `status` values, and lists its own fields on its
own page.

## Per-type fields

Each type's fields are documented on its own page, generated into it from `.schema/`. A reader working in one folder has
what they need without leaving it, and there is still one definition.

<!-- BEGIN GENERATED: types-metadata -->

[Fix](../fixes.md#metadata) · [NFR](../nfrs.md#metadata) · [Service](../services.md#metadata) ·
[Standard](../standards.md#metadata)

<!-- END GENERATED: types-metadata -->

## Example

```yaml
---
id: adr-0017
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

Declare it in the type's `.schema/<folder>.yaml`, add it to that type's `_template.md`, and run `kac generate` so the
type's own page carries it. The validator reads the schema, so it needs no change of its own. What to ask before you add
one is in [Metadata][metadata].

[metadata]: https://paul80nd.github.io/knowledge-as-code/framework/metadata/
