# Taxonomy

A **type** is what a record is about: a policy, a runbook, a glossary. A **tier** is how it behaves: whether it may be
edited after acceptance, whether it decays, what has to be true of it. The two are different things, and behaviour is
what sets the rules.

Every validation rule, review expectation, language rule and generated report keys off the tier, so a new kind of
knowledge needs a tier and nothing else. [Principles](principles.md#behaviour-before-subject) argues why that is the
split to make.

## The four tiers

| Tier            | Behaviour                                                                |
|-----------------|--------------------------------------------------------------------------|
| **Decided**     | Immutable once accepted. What was decided is superseded, never rewritten |
| **Normative**   | Living. Owned. Edited in place with a changelog                          |
| **Descriptive** | Living. Must mirror reality, and is verifiable against the estate        |
| **Procedural**  | Living. Must be rehearsed to stay true                                   |

[Contributing](contributing.md#review-by-tier) says what review each tier asks for.

**An observation has no tier.** Nobody has verified it, so it goes to the tracker, as
[In through the tracker, out through the export](principles.md#in-through-the-tracker-out-through-the-export) says.

**No record states its lifecycle.** Immutable and living are readable off the table above. A record
therefore states `tier` and nothing that could disagree with it. A type declares both, and the schema requires it.

## Which types a corpus holds

The framework declares more types than any one corpus stands up, and [The default types](types.md) introduces every one
of them. A corpus names the ones it adopted in `types:` in [`.corpus.yaml`](../use/corpus-descriptor.md). Its own
taxonomy page then has a decision table covering those and no others. What you are holding goes in the left column, and
where it belongs in the right.

Most mistakes here are placement mistakes: a good record filed where it duplicates something or is never found. Where
nothing fits, the answer is a taxonomy conversation, and sometimes the adoption of a type the corpus declined, never a
`misc/` folder.

**Session state has no type.** Where a piece of work got to stays local and never reaches a corpus, because session logs
routinely contain stack traces, connection strings and customer identifiers.

## The shape on disk

Each type is a page and a folder beside it, named for the type in the plural:

```
<type>.md              # what it is, why, how to contribute: human-written
<type>/
  ├── _index.md        # index: generated
  ├── _template.md     # what people and agents copy
  └── <records>.md
```

**A leading underscore is reserved.** It marks the framework's own artefact: the generated index and the template inside
a type folder, and the scaffolding directories alongside them. The tool reads the prefix rather than the names, so
anything under it is excluded from discovery and never validated as a record.

Alongside the types sit the corpus's own root files:

* a README and agent guidance
* the register of external frameworks it stands against
* the framework's own documentation, and the machine-readable schema in `.schema/`
* `.corpus.yaml`, which says what this corpus is and where it takes the framework from

## An offering and a spec

An offering lives in the corpus and a spec lives with the code. An **offering** is the product surface (Billing,
Search, Notifications), described once, above the epic layer, as a hub of links. A **spec** applies the standards to
one feature's contract, in the repository that owns the feature, next to its API description and feature files.

Every other close call is between two types a corpus has, and that corpus's own taxonomy page states it.

## Changing the taxonomy

Adding a type, merging two, or moving a type between tiers is a larger act than editing any record within it. Where a
corpus holds ADRs, that change belongs in one, amending whichever recorded the taxonomy in the first place.

Adding a type is adding a YAML file to `.schema/`, and a corpus adopts it with
[`update --add-type`](../use/cli/update.md). Nothing in the tool changes.
