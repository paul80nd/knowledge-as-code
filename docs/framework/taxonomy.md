# Taxonomy

A **type** is what a record is about: a policy, a runbook, a glossary. A **tier** is how it behaves: whether it may be
edited after acceptance, whether it decays, what has to be true of it. The two are different things, and it is behaviour
that sets the rules.

That split is what lets the taxonomy grow without new machinery. Every validation rule, review expectation, language
rule and generated report keys off the tier, so a new kind of knowledge needs a tier and nothing else.
[Principles](principles.md#behaviour-before-subject) argues why.

## The five tiers

| Tier            | Behaviour                                                         |
|-----------------|-------------------------------------------------------------------|
| **Decided**     | Immutable once accepted. Superseded, never rewritten              |
| **Normative**   | Living. Owned. Edited in place with a changelog                   |
| **Descriptive** | Living. Must mirror reality, and is verifiable against the estate |
| **Procedural**  | Living. Must be rehearsed to stay true                            |
| **Observed**    | Perishable. Unreviewed until promoted, and expires by default     |

What review each tier asks for is in [Contributing](contributing.md#review-follows-the-tier).

**Observed is the row that surprises people.** The tier carrying the least authority is the one a corpus most depends
on, because capture that is not free does not happen.
[Cheap capture, deliberate promotion](principles.md#cheap-capture-deliberate-promotion) is where that argument lives.

**No record states its lifecycle.** Immutable, living and perishable are readable off the table above, so a record
carries `tier` and nothing that could disagree with it. A type declares both, and the schema requires it.

## Which types a corpus holds

The framework declares more types than any one corpus stands up, and
[The default types](types.md) introduces every one of them. A corpus names the ones it adopted in `types:` in
[`.corpus.yaml`](../corpus-descriptor.md). Its own taxonomy page then carries a decision table covering those and no
others: what you are holding in the left column, where it goes in the right.

Most mistakes here are placement mistakes rather than writing mistakes. Somebody writes a good record and files it where
it either duplicates something or is never found. Where nothing fits, the answer is a taxonomy conversation and
sometimes the adoption of a type the corpus declined. A `misc/` folder is a failure nobody notices until it is large.

**Session state is the one thing with no type.** Where a piece of work got to, for handover between sessions, stays
local and never reaches a corpus. Session logs routinely hold stack traces, connection strings and customer identifiers.
Only distilled, reviewed discoveries travel.

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
anything under it is excluded from discovery and never validated as a record. A record must therefore not take it.

The prefix also sorts ahead of letters, whether or not a listing folds case. That keeps the framework's files together
at the top of a folder somebody is scanning for content.

Alongside the types sit the corpus's own root files. A README and agent guidance. The register of external frameworks it
stands against. The framework's own documentation, and the machine-readable schema in `.schema/`. And
`.corpus.yaml`, which says what this corpus is and where it takes the framework from.

## The call that has only one side

A **capability** is the product surface (Billing, Search, Notifications), described once, above the epic layer, as a hub
of links. A **spec** is the per-feature application of standards to a concrete contract. It belongs in the repository
that owns the feature, next to the API description and the feature files it describes.

That is the same central-versus-local rule a decision record follows: cross-repo synthesis lives in the corpus, and
feature-level detail lives with the code. Every other close call is between two types a corpus holds, so each is written
on the type its heading names first and appears on that corpus's own page.

## Changing the taxonomy

Adding a type, merging two, or moving a type between tiers is a larger act than editing any record within it. Where a
corpus holds ADRs, that change belongs in one, amending whichever recorded the taxonomy in the first place.

Adding a type is adding a YAML file to `.schema/`, and a corpus adopts it by naming it in `types:` and syncing. Nothing
in the tool changes. [Principles](principles.md#schema-before-prose) says why that matters.
