# Tools

The packages, services and utilities we have approved, rejected, or are trialling.

**[→ Index](tools/_index.md)**

## What is a tool entry?

A record of something we build with: a package, a framework, a linter, a hosted service, a CLI. It says what the tool is
for, whether it is approved, which versions we stand behind, its licence, and what we chose it over.

Together the entries are a lightweight software approval register. A larger organisation would run one as an approval
board. Three engineers run it as a folder of records.

## Why we use them

Two problems, one register. Nobody can answer *"are we allowed to use this?"* without asking around, so each project
answers it alone. The estate ends up with several ways of doing one job. Nobody can answer *"what are we actually
depending on?"* without opening every manifest we own. That second question arrives with a licence review, with a
security advisory, and on the day a dependency is abandoned.

**Declared.** `drift-against-manifests` is declared and does not run. `packages` states the package URL it would match
a manifest entry against. Once something implements it, the register can be compared against the real manifests in both
directions: packages in use that were never approved, and approved packages nothing uses any more.

## Scope

A tool is something we **build with**, not something we run. A running system we call is an
[integration](integrations.md). Something we deploy is a [service](services.md).

The register records **current state**. An [ADR](adrs.md) records the **decision**, where there was one worth recording.
A small, uncontroversial adoption needs only a register entry. A contested or expensive choice earns both, and the entry
cites the ADR in `decided-in`.

A `rejected` entry earns its place. Somebody proposes the same package two years later, and the entry hands them
the evaluation we already did.

## Level of detail and cadence

One record describes one tool: its status, what you use it for, and the version range you stand behind. How to install
or configure it belongs in a process, or in the tool's own documentation.

A record changes when you adopt, deprecate or reject the tool, or move its version range. New records arrive when a
team proposes a tool.

## Metadata

<!-- BEGIN GENERATED: schema-tools -->

| Field              | Value                                      | Notes                                                                                                          |
|--------------------|--------------------------------------------|----------------------------------------------------------------------------------------------------------------|
| `tier` *†          | `descriptive`                              | The record's trust level, fixed for the type and checked against the folder.                                   |
| `status` *†        | `approved` `trial` `deprecated` `rejected` | `approved` applies to new work. Existing use of a tool with any other status is drift. In force at `approved`. |
| `category`         | derived from the record's sub-path         | The folder the tool is filed under, below `tools/`.                                                            |
| `packages`         | list                                       | Every package this entry approves, with the version range approved for each.                                   |
| `homepage` *       | string                                     | The project's own page.                                                                                        |
| `licence` *        | string                                     | The licence concluded to apply, as an SPDX expression, `NONE` or `NOASSERTION`.                                |
| `licence-declared` | string                                     | The package's own statement of its licence, kept word for word.                                                |
| `decided-on` *     | date                                       | Quoted. The day the current status was decided.                                                                |
| `review-by` *      | date                                       | Quoted. The day by which somebody checks this entry is still right.                                            |
| `decided-in`       | id                                         | The ADR id recording the decision to adopt this tool.                                                          |
| `replaces`         | id                                         | The tool id this supersedes.                                                                                   |
| `successor`        | id                                         | The tool id that replaces this one.                                                                            |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-tools -->

## Adding a tool

1. Copy [`_template.md`](tools/_template.md) to `<slug>.md`. Tools use slug ids: `tol-vitest`.
2. Set `status`. `trial` covers something being evaluated in one place. Promote or reject it once the evaluation ends.
3. Set `decided-on` to the day you decided, and `review-by` to the day somebody checks the entry again.
4. List each package in `packages`, as a package URL with the range approved for new work.
5. Record the `homepage`.
6. Read the package's own licence files. Write what the package states in `licence-declared`, and what you concluded
   in `licence`.
7. Where this tool takes over from an older one, name the older tool in `replaces`, so the deprecation path is visible.
8. Cite `decided-in` where an ADR exists. Where the choice was contested and no ADR exists, write one.

**Conventions**

* **Approved means approved for new work.** Something already in use but not approved is drift, and finding it is a
  manual job until something implements `drift-against-manifests`.
* **A deprecated entry names its successor.** Set `successor` to whatever took over, so somebody arriving from a
  manifest has somewhere to go next.
* **Give a package a range, not a pin.** The pin belongs in the manifest.
* **An entry expires.** `review-in-date` warns once `review-by` has passed, so an approval nobody revisits stops
  speaking for the estate.
* **A conclusion that departs from the declaration says why.** `conclusion-states-a-reason` warns where `licence` and
  `licence-declared` differ and no `## Licence and obligations` section says what you read.

## What CI checks

<!-- BEGIN GENERATED: checks-tools -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                        | Level   | What it verifies                                                                                      |
|------------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `derived-key`                | error   | A field derived from the record's folder is not written in frontmatter.                               |
| `min-items`                  | error   | A list field carries at least as many entries as its schema asks for.                                 |
| `entry-shape / entry-key`    | error   | An object field, and each entry of an object list, carries the keys the field declares and no others. |
| `ref-resolves`               | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `reciprocal`                 | error   | A reciprocal field and its counterpart agree in both directions.                                      |
| `deprecated-has-successor`   | warning | A deprecated tool states what replaces it.                                                            |
| `trial-has-criteria`         | warning | A tool in `trial` states what would end the trial.                                                    |
| `exit-states-a-reason`       | warning | A deprecated or rejected tool says what went wrong with it.                                           |
| `conclusion-states-a-reason` | warning | A tool whose concluded licence differs from its declared one says how that was reached.               |
| `review-in-date`             | warning | An entry names a review date that has not passed.                                                     |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                      | What it would verify                                                        |
|---------------------------|-----------------------------------------------------------------------------|
| `drift-against-manifests` | Every package in use is in the register, and every approved tool is in use. |

<!-- END GENERATED: checks-tools -->
