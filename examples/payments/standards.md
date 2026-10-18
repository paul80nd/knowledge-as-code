# Standards

Teams follow these standards when they build and review functionality: the normative patterns and practices of the
platform.

## What is a standard?

A standard is a short, living Markdown document that states a rule we follow. It says what to do, imperatively, with
concrete examples and a conformance checklist you tick a design against. Standards are the day-to-day rulebook. Design a
new endpoint, header or contract, and the standard for that area tells you the shape it must take.

## Where a standard's authority comes from

Every standard here cites a policy in the engineering corpus, and none of them cites an ADR. This corpus has not adopted
`adrs`: a decision about how payments is built is recorded next to the code, and a decision about how the organisation
builds anything belongs to the governance layer.

So a standard's `implements:` names an imported clause, as `eng:pol-SCRT.STORE`, and `provenance-required` is satisfied
by that alone. The policy owns the *why* and states the obligation at a height that survives replacing the whole estate.
The standard owns the *what*, and says what that obligation means for a payment.

| Layer                          | Answers                                      | Where it lives     |
|--------------------------------|----------------------------------------------|--------------------|
| Policy, as `eng:pol-SCRT`      | What are we committed to, whatever we build? | `../engineering/`  |
| Standard, as `std-TELEM`       | What must I do about it in payments?         | Here               |

A clause may be discharged in both corpora at once. `eng:pol-SCRT.LOGS` prohibits writing a secret to a log, the
governance layer's own secret-handling standard says what that means estate-wide, and [std-TELEM] says what it means for
a PSP key. Neither restates the other, and a reader following the clause id finds both.

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

Every rule here is about handling a payment, so the folders follow the stages a payment passes through.

* **checkout**: what the browser collects and who authenticates the cardholder.
* **authorisation**: the call to the payment service provider (PSP), and what happens when it does not answer.
* **ledger**: how a movement of money is recorded and reconciled.
* **operations**: what a running payment leaves behind, and how long it is kept.

There is no `platform` folder here. A rule about the runtime binds every service we run, so it is written once in the
governance corpus and inherited.

## Level of detail and cadence

Each rule states one outcome with **MUST**, the usual way to reach it with **SHOULD**, or a permission with **MAY**. A
command, a configuration line or a tool's setting belongs in the standard's Examples, or in the tool's own
documentation.

A standard changes a few times a year as the practice matures, and at its review. New standards arrive rarely, when a
decision or a policy needs one.

## Where to find them

* **[→ Standards index](standards/_index.md)**: the generated catalogue of every standard, with the category of each.
* **[`_template.md`](standards/_template.md).** Copy it to start a new standard. The categories above and the steps
  below cover the rest.

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

Name where the standard comes from: an ADR in `derived-from`, a policy in `implements`, or both. `provenance-required`
fails a standard carrying neither. Where you can name neither, either the decision has not been made (make it), or what
you are writing is guidance rather than a standard.

Write each rule as a bullet with one BCP 14 keyword in bold capitals: **MUST**, **MUST NOT**, **SHOULD**,
**SHOULD NOT** or **MAY**. `part-modal` reports a bullet with none, and one that leaves a keyword in plain text. It
calls the keyword a modal, because the same declaration governs a policy's clause table.

**Choose the keyword by what a departure costs.** A **MUST** or **MUST NOT** states the outcome the standard's owner
answers for, and departing from it takes an accepted deviation. A **SHOULD** or **SHOULD NOT** states
the usual way to reach that outcome. An engineer or an agent may depart from it and say why in the change. A **MAY**
permits something a reader might otherwise think is ruled out. Write **MUST** for the outcome and **SHOULD** for the
way to reach it.

**Give a heading one short reason where the reason is not obvious.** End the bullet it explains with a clause opening
"because". The reason then travels in the export with the rule, and the longer argument stays in Rationale and
provenance.

Make each rule **testable**. Where a rule cannot be checked against a concrete artefact, sharpen it or move it to the
rationale section. Every **MUST** and **MUST NOT** should have a corresponding control, even where that control's
mechanism is `not-enforced`. An honest gap is more useful than a silent one.

**Group the rules under `###` headings.** A heading says what the rules beneath it hold a reader to, and it is the
address something else cites: `std-SECRET.every-secret-rotates` names one group, and an export carries the group as a
line of its own. A standard with a single group writes one heading. `part-none` fails a Rules section with none.

**The conformance checklist tests what the rules require, and nothing else.** An item for a SHOULD is met where the
work follows it or the change says why it departs.

**Close a heading with the clauses it covers**, as a footnote in italic with the label bold:
`_**Covers:** [pol-SCRT].EMBED, [pol-SCRT].LOGS_`. `mirrors-citations` holds the union of those lines equal to
`implements`, in both directions, so the frontmatter says which obligations the standard discharges and each heading
says which rule discharges which. A heading covering no clause carries no line, and a line naming none is reported, as
is one a stray space left out of italic. The rules themselves carry no citation.

**A standard's size follows its subject.** One binding every piece of work usually runs to seven or eight rules, and one
covering a single interface may run to thirty. Neither is padded or cut to meet the other.

**Name the external document a standard defers to, in an optional `Sources and further reading` section.** A house rule
seldom starts from nothing, and a standard adding exceptions to somebody else's conventions is incomplete without that
document. Mark an entry **normative** where a reader has not read the rule until they have read the source, and
**informative** where the source is background.

**A baseline is not a posture.** [Frameworks](frameworks.md) records the standing we take against a framework this
corpus cites, and it is the only page recording one. A baseline is the document a standard's own rules defer to. Where
the two look alike, ask whether it would still be true after replacing the entire technology estate. A PCI DSS posture
survives that. A C# style baseline dies with C#, and it goes in the standard deferring to it.

**End every standard with the line saying how to depart from it.** The template writes it after the Changelog. It
sends a reader departing from a **MUST** to a deviation, and asks a reader departing from a **SHOULD** for the reason.

Standards are living documents, and we edit them in place. Record every material change in the changelog, newest
entry first.

## What CI checks

<!-- BEGIN GENERATED: checks-standards -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                       | Level   | What it verifies                                                                                            |
|-----------------------------|---------|-------------------------------------------------------------------------------------------------------------|
| `derived-key`               | error   | A field derived from the record's folder is not written in frontmatter.                                     |
| `part-none / part-empty`    | error   | The parts section holds at least one heading, and each has something under it.                              |
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

| Rule                           | What it would verify                                                                              |
|--------------------------------|---------------------------------------------------------------------------------------------------|
| `rules-have-controls`          | Every MUST and MUST NOT rule has a control that checks it, or the standard says which have none.  |
| `changelog-begins-at-active`   | Changelog entries are material changes only, and begin when status becomes `active`.              |
| `changelog-on-material-change` | A change to the Rules section of an `active` standard needs a changelog entry in the same commit. |

<!-- END GENERATED: checks-standards -->

[std-TELEM]: standards/operations/payment-telemetry.md
