# What the schema is held to

Before `kac` validates a single record, it validates the schema. It reports any declaration the tool does nothing with,
such as a `rules:` entry nothing runs or an `id.style:` no code reads. Every finding names the file and the key. A
**corpus** is one repository of knowledge records kept in git, and its `.schema/` may have been copied from elsewhere.

## What the pass reports

| Reported                                                                                 | Check                |
|------------------------------------------------------------------------------------------|----------------------|
| A key at any level the loader never reads, `notes:` excepted                             | `schema-unknown-key` |
| An `expr:` that will not compile, or that names no `severity:` or `message:`             | `schema-unreadable`  |
| A `required-when:` outside its three forms                                               | `schema-unreadable`  |
| `values: $enums.x` where `_enums.yaml` declares no `x`                                   | `schema-unreadable`  |
| `shape: x` where `_shapes.yaml` declares no `x`, or a `shape:` beside an `entry:` block  | `schema-unreadable`  |
| A rule claiming a `severity:` that neither an `expr:` nor a rule class answers           | `schema-dispatch`    |
| A rule class reporting under a check id `_checks.yaml` does not declare                  | `schema-dispatch`    |
| A config key, such as `max-words:`, on an `expr:` rule or a rule whose class ignores it  | `schema-dispatch`    |
| A config value its rule cannot use, which then checks no record: `max-words: five`       | `schema-dispatch`    |
| `values:` on any field that is not an `enum`                                             | `schema-dispatch`    |
| `in-force:` on any field that is not an `enum`                                           | `schema-dispatch`    |
| An `in-force:` value the field's `values:` does not include                              | `schema-shape`       |
| A `status:` that states a range and names no `in-force:` value                           | `schema-shape`       |
| `min-items:` or `min-records:` on any field that is not a `list`                         | `schema-dispatch`    |
| A `type:` or an `of:` naming a value no check reads, at any depth of an `entry:` block   | `schema-dispatch`    |
| An `of:` on any field that is not a `list`                                               | `schema-dispatch`    |
| A `type: id` or an `of: id` with no `ref:`, so nothing resolves what the value points at | `schema-dispatch`    |
| A `type: id` or an `of: id` on an entry key, which the reference pass never walks        | `schema-dispatch`    |
| An `entry:` block or a `shape:` on a field whose value is never an object                | `schema-dispatch`    |
| `type: object` or `of: object` with neither an `entry:` block nor a `shape:`             | `schema-shape`       |
| An `index.order:` that is neither `ascending` nor `descending`                           | `schema-dispatch`    |
| A `tier:` no `_tiers.yaml` declares, or a tier only one of the two files knows           | `schema-shape`       |
| A tier declaring no `label:` or no `behaviour:` (both head its section in the taxonomy)  | `schema-shape`       |
| An `id.style` with no code behind the value                                              | `schema-dispatch`    |
| An `id.width` span on a `numbered` type, which pads to one width so that ids sort        | `schema-shape`       |
| An `id.width` span beside a filename still carrying the id, or a `min:` above its `max:` | `schema-shape`       |
| A `filename.carries-id: false` on a `slug` type, whose id is the filename stem           | `schema-shape`       |
| A `from:` naming a source no derivation reads                                            | `schema-dispatch`    |
| A `from:` on a field that is also `required: true`                                       | `schema-shape`       |
| A type declaring no `folder:`                                                            | `schema-shape`       |
| A `mirrors-section:` at a section the type's `sections:` block does not declare          | `schema-shape`       |
| A `mirrors-citations:` on a field with no `ref:`, so its ids resolve against nothing     | `schema-shape`       |
| An `export.sections:` key at a section the type's `sections:` block does not declare     | `schema-shape`       |
| An `export.fields:` entry naming a field neither the type nor `_universal.yaml` declares | `schema-shape`       |
| An `export:` block declaring no `version:`, which a consumer reads its files at          | `schema-shape`       |
| An `export.parts:` on a type carrying no `parts:` block                                  | `schema-shape`       |
| An `export.parts:` with no `line:` beneath it, or a `line:` key naming no source         | `schema-shape`       |
| A `line:` source outside the vocabulary the exporter fills                               | `schema-dispatch`    |
| A `front.<field>` naming a field neither the type nor `_universal.yaml` declares         | `schema-shape`       |
| A `front.<field>` naming a list or an object field, where a line reads one value         | `schema-shape`       |
| A `column.<Header>` at a header the type's `parts.columns:` does not declare             | `schema-shape`       |
| A `part.lead`, `part.bullets` or `part.aside.<Label>` on a table-sourced type            | `schema-shape`       |
| A `part.aside.<Label>` at a label the type's `parts.asides:` does not declare            | `schema-shape`       |
| A `part.level` or `part.bullets` on a type declaring no binding or advisory modals       | `schema-shape`       |
| A `parts.source:` outside the sources the tool extracts                                  | `schema-dispatch`    |
| A `parts.section:` at a section the type's `sections:` block does not declare            | `schema-shape`       |
| A table-sourced `parts:` block declaring no `binding:`                                   | `schema-shape`       |
| An export entry declaring no fidelity at all                                             | `schema-shape`       |
| A fidelity no export carries                                                             | `schema-dispatch`    |
| A missing `label-plural:`, `summary:`, `goes-here:`, `detail:` or `lineage.prior-art:`   | `schema-shape`       |
| A `label-plural:`, `summary:` or `goes-here:` past 120 characters (they render as cells) | `schema-shape`       |
| A rule `description:` past 120 characters, for the same reason                           | `schema-shape`       |
| A `versus:` against the declaring type itself, or one both sides declare                 | `schema-shape`       |

Each row is grouped by what trips it, so one check id can appear on several rows.

## Whether code acts on the value

A `schema-dispatch` row asks whether code acts on the value. `style: mnemonic` passes because a branch reads it, and a
name with no branch beneath it fails. A key the loader never asks for is `schema-unknown-key`.

[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json) lists
the keys for an editor. It advises an author and gates nothing.

### A derived field

`from: sub-path` says the field's value comes from the folders a record sits in. A source name no derivation reads is
`schema-dispatch`.

Declaring it `required: true` as well is `schema-shape`. No author can write a derived field, and writing it trips
`derived-key`. [Discovery](../use/discovery.md#from-sub-path) says how the value is read.

## What `schema-shape` asks

A `schema-shape` row asks whether the value agrees with a second declaration in the same file, or with the page it
lands on. A `mirrors-section:` must name a section the `sections:` block declares, and a `summary:` must fit the table
cell it becomes.

## `ref:` and `versus:`

The value of either is outside the pass, because a corpus may have declined the type it names. A field's `ref:` names
the type its ids point at, and a type's `versus:` names the type it is most often confused with. The pass reports a
field declaring an id and no `ref:` at all, and an id on an entry key, which the reference pass never walks.

Where a `ref:` names only types this corpus neither adopted nor imports:

* `required-field` and `required-when:` say nothing about it, unless it takes a word under `allow-literal:`.
* `ref-resolves` still refuses a cited id, and says which types the field wanted.
* `kac update --add-type` starts the reference without an edit to `.schema/`.

A misspelled type name in either key is reported nowhere in a corpus. A test over the authored `.schema/`, where every
type is present, is what catches it.

## `values: $corpus.x`

`$enums.x` draws a field's range from `_enums.yaml`, and a name declared there nowhere is `schema-unreadable`.
`$corpus.x` draws it from [`enums:`](../use/corpus-descriptor.md#enums) in `.corpus.yaml`, which this pass does not
read.

## A rule with no `severity:`

A rule you have not built yet keeps its `description:`, drops its `severity:`, and the type page renders it under
**Declared, not yet enforced**. `kac checks` does not list it. A rule naming a severity that nothing dispatches is
`schema-dispatch`.

## Where to go next

[Checks](checks.md) is the page for adding a check, or for deciding whether the one you want already exists.
