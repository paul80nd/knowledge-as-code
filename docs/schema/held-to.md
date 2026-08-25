# What the schema is held to

Before `kac` validates a single record, it validates the schema. A declaration the tool does nothing with is not
harmlessly inert: `rules:` reads as behaviour the validator applies, and a `ref:` reads as a target being checked.
Either one can sit in a file for a year while nobody notices it does nothing.

This matters most in a corpus that copied the framework from somewhere else. A **corpus** is one repository of knowledge
records kept in git, and whoever holds a copied `.schema/` cannot ask what a key was meant to do. The pass answers for
them, and every finding names the file and the key.

## What the pass reports

| Reported                                                                                 | Check                |
|------------------------------------------------------------------------------------------|----------------------|
| A key at any level the loader never reads, `notes:` excepted                             | `schema-unknown-key` |
| An `expr:` that will not compile, or that names no `severity:` or `message:`             | `schema-unreadable`  |
| A `required-when:` outside its three forms                                               | `schema-unreadable`  |
| `values: $enums.x` where `_enums.yaml` declares no `x`                                   | `schema-unreadable`  |
| A rule claiming a `severity:` that neither an `expr:` nor a rule class answers           | `schema-dispatch`    |
| A rule class reporting under a check id `_checks.yaml` does not declare                  | `schema-dispatch`    |
| A `ref:` entry naming a folder no schema covers                                          | `schema-dispatch`    |
| A `versus:` entry naming a folder no schema covers                                       | `schema-dispatch`    |
| `values:` on any field that is not an `enum`                                             | `schema-dispatch`    |
| `min-items:` or `min-records:` on any field that is not a `list`                         | `schema-dispatch`    |
| An `index.order:` that is neither `ascending` nor `descending`                           | `schema-dispatch`    |
| A `tier:` no `_tiers.yaml` declares, or a tier only one of the two files knows           | `schema-shape`       |
| A tier declaring no `label:` or no `behaviour:` (both head its section in the taxonomy)  | `schema-shape`       |
| An `id.style` with no code behind the value                                              | `schema-dispatch`    |
| A type declaring no `folder:`                                                            | `schema-shape`       |
| A `mirrors-section:` at a section the type's `sections:` block does not declare          | `schema-shape`       |
| An `export.sections:` key at a section the type's `sections:` block does not declare     | `schema-shape`       |
| An `export.fields:` entry naming a field neither the type nor `_universal.yaml` declares | `schema-shape`       |
| An `export.parts:` on a type carrying no `parts:` block                                  | `schema-shape`       |
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
holds a list and is the exception that proves the point. It can be behind, which is why it advises an author and gates
nothing.

## What `schema-shape` asks instead

The `schema-shape` rows ask a different question. There the tool acts on whatever the value says: any section is
reconciled, any folder is read, any sentence is rendered. What makes one sound is a second declaration in the same file,
or the shape of the page the value lands on. A `sections:` block sits beside a `mirrors-section:`, and the width of a
table cell bounds the `summary:` that becomes it.

## A target nothing resolves

A `ref:` at a type the corpus never adopted is reported for the same reason as one that is misspelled. Whether the
folder was deleted locally or never existed upstream, the field claims a target nothing can resolve. Re-adopt the type
file or drop the `ref:`. Those are two ways of settling one question about what this corpus holds.

## Aspiration is allowed, silence is not

A rule you have not built yet keeps its `description:`, drops its `severity:`, and renders on the type page under
*Declared, not yet enforced*. A rule naming a severity that nothing dispatches would read as enforced from every angle
and not be, so the load fails on it.

## Where to go next

[Checks](../checks.md) is the page for adding a check, or for deciding whether the one you want already exists.
