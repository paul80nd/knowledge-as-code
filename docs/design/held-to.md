# What the schema is held to

Before `kac` validates a single record, it validates the schema. A declaration the tool does nothing with is not
harmlessly inert: `rules:` reads as behaviour the validator applies, and `id.style:` reads as a spelling every id is
held to. Either one can sit in a file for a year while nobody notices it does nothing.

This matters most in a corpus that copied the framework from somewhere else. A **corpus** is one repository of knowledge
records kept in git, and if you hold a copied `.schema/` you cannot ask what a key was meant to do. The pass answers for
you, and every finding names the file and the key.

## What the pass reports

| Reported                                                                                 | Check                |
|------------------------------------------------------------------------------------------|----------------------|
| A key at any level the loader never reads, `notes:` excepted                             | `schema-unknown-key` |
| An `expr:` that will not compile, or that names no `severity:` or `message:`             | `schema-unreadable`  |
| A `required-when:` outside its three forms                                               | `schema-unreadable`  |
| `values: $enums.x` where `_enums.yaml` declares no `x`                                   | `schema-unreadable`  |
| A rule claiming a `severity:` that neither an `expr:` nor a rule class answers           | `schema-dispatch`    |
| A rule class reporting under a check id `_checks.yaml` does not declare                  | `schema-dispatch`    |
| `values:` on any field that is not an `enum`                                             | `schema-dispatch`    |
| `min-items:` or `min-records:` on any field that is not a `list`                         | `schema-dispatch`    |
| An `entry:` block on a list whose `of:` is not `object`                                  | `schema-dispatch`    |
| `of: object` with no `entry:` block saying what an entry holds                           | `schema-shape`       |
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
| A `column.<Header>` at a header the type's `parts.columns:` does not declare             | `schema-shape`       |
| A `part.lead` or `part.aside` on a type sourcing its parts from a table                  | `schema-shape`       |
| A `part.level` on a type declaring no binding or advisory modals                         | `schema-shape`       |
| A `parts.source:` outside the sources the tool extracts                                  | `schema-dispatch`    |
| A `parts.section:` at a section the type's `sections:` block does not declare            | `schema-shape`       |
| A table-sourced `parts:` block declaring no `binding:`                                   | `schema-shape`       |
| An export entry declaring no fidelity at all                                             | `schema-shape`       |
| A fidelity no export carries                                                             | `schema-dispatch`    |
| A missing `label-plural:`, `summary:`, `goes-here:`, `detail:` or `lineage.prior-art:`   | `schema-shape`       |
| A `label-plural:`, `summary:` or `goes-here:` past 120 characters (they render as cells) | `schema-shape`       |
| A rule `description:` past 120 characters, for the same reason                           | `schema-shape`       |
| A `versus:` against the declaring type itself, or one both sides declare                 | `schema-shape`       |

The rows are grouped by what trips them, where the catalogue holds one entry per check, so the table is written by hand.
A test holds the check ids in it against the catalogue in both directions, which catches one renamed, retired or
introduced. What it cannot catch is an id growing a second way to fail, because nothing in the code tells one arm from
another.

## Whether code acts on the value

**The question is whether code acts on the value, not whether the key is spelled correctly.**
`style: mnemonic` is a real style and would pass a spelling test. What makes it sound is the branch that reads it. Each
vocabulary in the table above is read out of the code that dispatches it, so adding a name with no branch beneath it is
the mistake this pass exists to prevent.

There is no list of permitted keys anywhere. The loader records what it asked each mapping for, and whatever is left
over is reported. So a key gains its meaning and its admission in the same edit, and a key that stops being read stops
being admitted without anyone having to remember.

[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json)
holds a list, and that list can be behind, which is why it advises an author and gates nothing.

### A derived field is not the author's to fill in

`from: sub-path` says the field's value comes from the folders a record sits in. The source is a vocabulary, so a
name no derivation reads is `schema-dispatch`. The field would then be empty on every record, and the page would show
a column the corpus never fills in.

Declaring it `required: true` as well is `schema-shape`. The two declarations contradict each other: the author cannot
write the field, so they cannot meet the requirement, and writing the line to try trips `derived-key` instead.
[Discovery](discovery.md#from-sub-path-reads-a-fields-value-out-of-the-folders) says how the value is read.

## What `schema-shape` asks instead

The `schema-shape` rows ask a different question. There the tool acts on whatever the value says: any section is
reconciled, any folder is read, any sentence is rendered. What makes one sound is a second declaration in the same file,
or the shape of the page the value lands on. A `sections:` block sits beside a `mirrors-section:`, and the width of a
table cell bounds the `summary:` that becomes it.

## A `ref:` and a `versus:` name a type this corpus may not hold

Both are outside the pass. A field's `ref:` names the type its ids point at, and a type's `versus:` names the type it is
most often confused with. A corpus, meaning one repository of knowledge records, adopts as many of the framework's types
as it has use for, so either one may name a type this corpus turned down.

Nothing is reported when it does. The disambiguation renders nothing, and `kac update --add-type` starts the reference
without an edit to `.schema/`. What a record is held to does not soften with it. `ref-resolves` still asks that a cited
id exists, and it refuses one of a type the field never named: a field whose every type this corpus declined admits
nothing at all, and says which types it wanted.

The cost is a misspelled name, which reads the same way from inside a corpus that holds a subset of the types. `kac`
reports it nowhere. What catches it is a test over the authored `.schema/`, held in the repository that writes the
framework, where every type is present. This repository runs one. A fork writing its own framework schema needs its own,
and the shape to copy is `tooling/kac.tests/SchemaReferenceTests.cs`.

## A rule may declare no `severity:`, but not one nothing answers

A rule you have not built yet keeps its `description:`, drops its `severity:`, and renders on the type page under
*Declared, not yet enforced*. A rule naming a severity that nothing dispatches would read as enforced from every angle
and not be, so the load fails on it.

## Where to go next

[Checks](checks.md) is the page for adding a check, or for deciding whether the one you want already exists.
