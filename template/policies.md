# Policies

The engineering commitments we hold ourselves to: the *what* and the *why*.

**[→ Index](policies/_index.md)**

## What is a policy?

A policy states what we commit to and why. It is durable, it is written at the level of principle, and it rarely names a
technology: *"secrets are never stored in source control"*, *"quality checks are gates, not advisories"*, *"changes to
non-development environments go through the pipeline"*.

Policies sit at the top of the normative hierarchy. A policy says what we hold true, a [standard](standards.md) says
what to do about it, a [control](controls.md) says how we know it happened, and a [process](processes.md) says how to do
it.

## Why we use them

Standards change with the stack and policies do not. Keep the two apart, and replacing a web framework never reopens the
question of where secrets live.

Policies also give the standards somewhere to point. A standard citing no ADR and no policy has no provenance, and that
usually means it is guidance in disguise or a decision nobody made.

Each clause maps to external frameworks in the `Alignment` column of the clause table, most often to **ISO/IEC 27001:
2022** Annex A. The mapping sits on the clause because a policy meets any one control through a single obligation.

A framework can bind us by certification, bind us because we chose to hold ourselves to it, or serve as something we
borrow from. [Frameworks](frameworks.md) records which of the three applies, and no policy repeats it. Our standing
changes on a schedule of its own, so a policy that carried it would need correcting alongside twenty others.

## Scope

**The test:** would this still be true after replacing the entire technology estate? If yes, it is a policy. If it names
a tool, a framework or a protocol, it is a [standard](standards.md).

| Policy                                          | Standard                                                                     |
|-------------------------------------------------|------------------------------------------------------------------------------|
| "Secrets are never stored in source control."   | "Services **MUST** read secrets from a managed vault via workload identity." |
| "Quality checks are gates that fail the build." | "ESLint **MUST** run with `--max-warnings 0`."                               |

A policy commits and a [control](controls.md) verifies, so the two are never the same document. An [ADR](adrs.md)
records one decision and the alternatives weighed against it, where a policy states a position we hold across all such
decisions.

## Level of detail and cadence

Each clause states one commitment that stays true after you replace the whole technology estate. A product, a
version or a mechanism belongs in a standard that implements the clause. A figure in a clause, such as a remediation
window, is the limit the policy's owner accepts. A standard implementing the clause may only tighten it.

A policy changes about once a year, at its review, or when a law or a framework behind it changes. New policies
arrive rarely.

## Categories

A policy's category is the folder it sits in under `policies/`, and the tool reads it from there. It answers *why this
policy exists*, where `tags` answer *what topics it touches*. Two questions need two fields: a secrets policy filed at
`policies/security/` carries `tags: [credentials, key-management, secrets]` as well.

Make the folders your own commitments need, and keep the set small enough to scan. A policy saved straight into
`policies/` has no category, which is the right shape while there are few enough to read as one list.

## Metadata

<!-- BEGIN GENERATED: schema-policies -->

| Field         | Value                              | Notes                                                                                                                            |
|---------------|------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|
| `tier` *†     | `normative`                        | The record's trust level, fixed for the type and checked against the folder.                                                     |
| `status` *†   | `draft` `active` `retired`         | `draft` while it is being agreed, `retired` once it is stood down. Retire a policy instead of deleting it. In force at `active`. |
| `category`    | derived from the record's sub-path | The folder the policy is filed under, below `policies/`.                                                                         |
| `aligns-with` | list                               | The binding frameworks this policy's clauses map to, grouped with the references they cite.                                      |
| `active-from` | date                               | The day the policy came into force. Required when `status == active`.                                                            |
| `review-by` * | date                               | The day the policy is looked at again, usually a year ahead.                                                                     |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-policies -->

## Adding a policy

Copy [`_template.md`](policies/_template.md). Its guidance block states how to write the id, each field, the title,
the clauses and the `Alignment` column. The steps below add what the template leaves out.

1. Apply the test above. Most things that feel like policies are standards.
2. Give a framework cited for the first time an entry in [Frameworks](frameworks.md). Decide its posture there
   before citing it here.

**Conventions**

* **A policy does not state our standing against a framework.** A policy maps its clauses to controls and says nothing
  about what that mapping is worth. Leave out "compliant", "certified" and "registered". [Frameworks](frameworks.md)
  holds the standing, and holds it once.
* **A policy has no roles section and no approver field.** The `owner` answers for the policy, and the template says how
  it is approved. The process that does the work says who carries out a commitment.
* **A clause id stays once the policy is active.** A standard cites a clause by its id, and a citation outlives the
  edit that removes the clause. Under `validate --since`, `clause-id-kept` fails a change that removes or renames a
  clause id in an active policy. Where a clause has to go, retire the policy and write a new one.

## What CI checks

<!-- BEGIN GENERATED: checks-policies -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                                  | Level   | What it verifies                                                                                                    |
|----------------------------------------|---------|---------------------------------------------------------------------------------------------------------------------|
| `derived-key`                          | error   | A field derived from the record's folder is not written in frontmatter.                                             |
| `entry-shape / entry-key`              | error   | An object field, and each entry of an object list, carries the keys the field declares and no others.               |
| `clauses`                              | error   | The clause section is a table of `Id \| Clause` rows, each id a code span and each clause opening with its modal.   |
| `clause-order / clause-compound`       | warning | Clause rows are grouped by binding level, and no row states a second modal.                                         |
| `part-id-unique / part-ref`            | error   | No two parts of a record share an address, and a `record-id.part` citation reaches the part it names.               |
| `alignment-rollup / framework-posture` | error   | `aligns-with` carries every binding reference the `Alignment` column cites, and the register places each framework. |
| `alignment-unlinked`                   | error   | Every framework an `Alignment` cell cites is written as a link the corpus resolves.                                 |
| `framework-uncited`                    | error   | Every framework on the register is cited by at least one clause.                                                    |
| `clause-id-kept`                       | error   | With `--since`, a policy active at that revision keeps the id of every clause it had there.                         |
| `review-not-before-active`             | error   | `review-by` is at or after `active-from`.                                                                           |
| `posture-belongs-to-frameworks`        | warning | "compliant", "certified" or "registered" written near a framework reference.                                        |
| `policy-in-date`                       | warning | A policy names a review date that has not passed.                                                                   |

<!-- END GENERATED: checks-policies -->
