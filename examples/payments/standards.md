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
is written at the most general folder where it is still true and left alone below that.

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

## Where to find them

* **[→ Standards index](standards/_index.md)**: the generated catalogue of every standard, with the category of each.
* **[`_template.md`](standards/_template.md).** Copy it to start a new standard. The categories above and the steps
  below cover the rest.

## Metadata

<!-- BEGIN GENERATED: schema-standards -->

| Field          | Value                                      | Notes                                                                               |
|----------------|--------------------------------------------|-------------------------------------------------------------------------------------|
| `id` *†        | string                                     | Stable, unique across the corpus, never reused. Format set by the type.             |
| `tier` *†      | `normative`                                | Fixed for the type. A trust signal for the reader. CI checks it matches the folder. |
| `status` *†    | `draft` `active` `deprecated` `superseded` | Plain values only. Enforcement notes belong in `verified-by`.                       |
| `owner` *†     | string                                     | A named person, never a team alias.                                                 |
| `tags` †       | list                                       | Free-form, lowercase, hyphenated. Used for cross-cutting search.                    |
| `category`     | derived from the record's sub-path         | The folder the standard is filed under, below `standards/`.                         |
| `derived-from` | list                                       | The ADRs this standard distils. Provenance may come from `implements` instead.      |
| `implements`   | list                                       | Policy clause ids this standard puts into practice, as `pol-EVER.BRANCH`.           |
| `verified-by`  | list                                       | Control ids that check it.                                                          |
| `applies-to` * | list                                       | Service ids, or `all`.                                                              |
| `review-by` *  | date                                       | Quoted. The date by which someone confirms this is still true.                      |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-standards -->

## Adding or changing a Standard

Name where the standard comes from: an ADR in `derived-from`, a policy in `implements`, or both. `provenance-required`
fails a standard carrying neither. Where you can name neither, either the decision has not been made (make it), or what
you are writing is guidance rather than a standard.

Write the rules with RFC 2119 keywords, and make each one **testable**. Where a rule cannot be checked against a
concrete artefact, sharpen it or move it to the rationale section. Every **MUST** and **MUST NOT** should have a
corresponding control, even where that control's mechanism is `not-enforced`. An honest gap is more useful than a silent
one.

**Group the rules under `###` headings.** A heading says what the rules beneath it hold a reader to, and it is the
address something else cites: `std-SECRET.every-secret-rotates` names one group, and an export carries the group as a
line of its own. A standard with a single group writes one heading. `part-none` fails a Rules section with none.

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

Standards are living documents, and we edit them in place. Record every material change in the changelog.

## What CI checks

<!-- BEGIN GENERATED: checks-standards -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `derived-key`               | error   | A field derived from the record's folder is not written in frontmatter.                                         |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`    | error   | Filename matches the pattern. The slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `part-none / part-empty`    | error   | The parts section holds at least one heading, and each has something under it.                                  |
| `part-id-unique / part-ref` | error   | No two parts of a record share an address, and a `record-id.part` citation reaches the part it names.           |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `mirrors-citations`         | error   | A field that mirrors a label reconciles with the citations the labelled lines gather.                           |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `provenance-required`       | error   | A standard cites an ADR in `derived-from`, a policy clause in `implements`, or both.                            |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                           | What it would verify                                                                                       |
|--------------------------------|------------------------------------------------------------------------------------------------------------|
| `rules-have-controls`          | Every MUST / MUST NOT rule is claimed by a control, or the standard declares the gap explicitly.           |
| `changelog-begins-at-active`   | Changelog entries are material changes only, and begin when status becomes `active`.                       |
| `changelog-on-material-change` | If the Rules section changed and status is `active`, a new changelog entry is required in the same commit. |

<!-- END GENERATED: checks-standards -->

[std-TELEM]: standards/operations/payment-telemetry.md
