# Standards

Teams follow these standards when they build and review functionality: the normative patterns and practices of the
platform.

## What is a standard?

A standard is a short, living Markdown document that states a rule we follow. It says what to do, imperatively, with
concrete examples and a conformance checklist you tick a design against. Standards are the day-to-day rulebook. Design a
new endpoint, header or contract, and the standard for that area tells you the shape it must take.

## How standards relate to ADRs

[ADRs](adrs.md) and standards are two halves of one record:

* An **ADR** captures a decision: the context, the choice, the alternatives weighed, the consequences. It answers *why*,
  and it is immutable once accepted.
* A **standard** captures the practice that decision produces. It answers *what to do*, and we edit it in place as the
  practice matures.

A standard distils accepted ADRs into something you can follow without reading the reasoning again, and cites what it
came from: the ADR owns the *why* and the standard owns the *what*. Where a standard descends from a policy, it cites
the policy. Below both sits a feature **spec**, which applies the standards to a concrete API and documents only what is
unique to that feature.

| Layer           | Answers                      | Lifecycle                    |
|-----------------|------------------------------|------------------------------|
| [ADRs](adrs.md) | *Why* did we decide this?    | Immutable, append-only log   |
| Standard        | *What* must I do, and how?   | Living, maintained catalogue |
| Spec            | What does *this* feature do? | Per-feature instance         |

## Why we use them

The ADR log preserves the reasoning, and reasoning is the wrong thing to read when you are mid-build and want the rule.
A standard states the pattern itself, in one place a reader can scan. Someone designing new functionality (a contributor
or an AI session) finds the rule and checks the design against a conformance checklist. They open the ADR only when they
want the deeper *why*.

## Categories

Standards **compose**. The rule-set enforced for a piece of work is the union of the folders that apply to it, so a rule
is written at the most general folder where it is still true and left alone below that. A standard adding detail to a
more general one sits in a different folder. It names the one it rests on in `depends-on`, so a reader who applies
`platform/dotnet/testing.md` knows to apply `common/testing.md` too.

A folder under `standards/` is the standard's category, and the tool reads it from where the file sits. Folders can
nest, so `platform/node/` is a category and `platform/` is the category above it. A standard saved straight into
`standards/` has no category, which is the right shape while there are few enough to read as one list.

This corpus holds two.

* **common**: the rules that hold whatever a service is written in. Version control, pull requests, secret handling,
  build gates, testing, interface contracts, observability, deployment and dependencies all live here.
* **platform**: the rules that hold for a runtime rather than for the estate. `platform/containers.md` binds anything
  shipped as an image, and `platform/dotnet/` binds C# alone.

Two files here are called `testing.md`. `common/testing.md` says which level a test belongs at, and
`platform/dotnet/testing.md` says what a test at that level looks like in .NET. A C# service is bound by both, and a
Python one by the first alone.

A payment-specific rule belongs in the payments corpus.

## Level of detail and cadence

Each rule states one outcome with **MUST**, the usual way to reach it with **SHOULD**, or a permission with **MAY**. A
command, a configuration line or a tool's setting belongs in the standard's Examples, or in the tool's own
documentation.

A standard changes a few times a year as the practice matures, and at its review. New standards arrive rarely, when a
decision or a policy needs one.

## Where to find them

* **[→ Standards index](standards/_index.md)**: the generated catalogue of every standard, with the category of each.
* **[`_template.md`](standards/_template.md).** Copy it to start a new standard. Its guidance block covers the rest.

## Metadata

<!-- BEGIN GENERATED: schema-standards -->

| Field           | Value                                      | Notes                                                                               |
|-----------------|--------------------------------------------|-------------------------------------------------------------------------------------|
| `tier` *†       | `normative`                                | The record's trust level, fixed for the type and checked against the folder.        |
| `status` *†     | `draft` `active` `deprecated` `superseded` | Plain values only. Enforcement notes belong in `verified-by`. In force at `active`. |
| `supersedes`    | id                                         | The standard this one replaces.                                                     |
| `superseded-by` | id                                         | The standard that replaces this one. Required when `status == superseded`.          |
| `category`      | derived from the record's sub-path         | The folder the standard is filed under, below `standards/`.                         |
| `derived-from`  | list                                       | The ADRs this standard distils. Provenance may come from `implements` instead.      |
| `implements`    | list                                       | Policy clause ids this standard puts into practice, as `pol-EVER.BRANCH`.           |
| `depends-on`    | list                                       | Standards whose rules this one's rules depend on.                                   |
| `verified-by`   | list                                       | Control ids that check it.                                                          |
| `applies-to` *  | list                                       | Service ids, or `all`.                                                              |
| `review-by` *   | date                                       | Quoted. The date by which someone confirms this is still true.                      |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-standards -->

## Adding or changing a Standard

Copy [`_template.md`](standards/_template.md). Its guidance block states how to write each field and each section,
from the keyword a rule takes to the closing line. This section adds what the template leaves out.

**Give every MUST and MUST NOT rule a control**, even one whose mechanism is `not-enforced`. An honest gap is more
useful than a silent one.

**A baseline is not a posture.** [Frameworks](frameworks.md) records the standing we take against a framework a policy
clause cites, and it is the only page recording one. A baseline is the document a standard's own rules defer to. Where
the two look alike, apply the test a policy is written against: would this still be true after replacing the entire
technology estate? A WCAG posture survives it. A C# style baseline dies with C#, and it goes in the standard deferring
to it.

## What CI checks

<!-- BEGIN GENERATED: checks-standards -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                       | Level   | What it verifies                                                                                            |
|-----------------------------|---------|-------------------------------------------------------------------------------------------------------------|
| `derived-key`               | error   | A field derived from the record's folder is not written in frontmatter.                                     |
| `part-none / part-empty`    | error   | The parts section holds at least one heading, and each has something under it.                              |
| `part-id-format`            | error   | Each part heading opens with its id, as `### PERMS: A job declares the permission it needs`.                |
| `part-modal / part-nested`  | error   | A part heading states its obligations as bullets, each with a declared modal in bold and no list inside it. |
| `modal-outside-rules`       | warning | Only the parts section binds: no bullet outside it names a declared modal.                                  |
| `part-id-unique / part-ref` | error   | No two parts of a record share an address, and a `record-id.part` citation reaches the part it names.       |
| `mirrors-citations`         | error   | A field that mirrors a label reconciles with the citations the labelled lines gather.                       |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.       |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                            |
| `changelog-order`           | warning | A standard's changelog entries read newest first.                                                           |
| `provenance-required`       | error   | A standard cites an ADR in `derived-from`, a policy clause in `implements`, or both.                        |
| `standard-in-date`          | warning | A standard names a review date that has not passed.                                                         |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                  | What it would verify                                                                             |
|-----------------------|--------------------------------------------------------------------------------------------------|
| `rules-have-controls` | Every MUST and MUST NOT rule has a control that checks it, or the standard says which have none. |

<!-- END GENERATED: checks-standards -->
