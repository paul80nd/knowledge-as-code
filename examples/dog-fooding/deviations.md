# Deviations

Where we knowingly break one of our own rules, who agreed to it, and when it is looked at again.

**[→ Index](deviations/_index.md)**

## What is a deviation?

A written, owned, time-bound departure from a policy or a [standard](standards.md). It names the rule it departs from,
says what we are doing instead and why, rates the risk it leaves, and gives a date somebody has to come back to it.

It also covers a shortcut that breaks no rule. Skipping the retry logic to ship on Friday, knowing the next person has
to add it, is technical debt. Nothing forbids it, and the debt is real, so it is recorded the same way. Such a record
names `none` in `departs-from`.

## Why we use them

A rule with no way out gets broken quietly. Somebody ships under pressure and tells nobody. A year later the estate
carries a departure that nobody can tell from never having known the rule.

A deviation is the way out, and it costs one page. The departure is decided rather than drifted into, one person has
their name against the risk, and a date brings it back. That is what makes the exception clause in a policy mean
something.

The count going up is the type working. A folder holding twelve deviations is not a worse estate than one holding
none. It is the same estate with twelve things written down.

## Scope

A deviation records a departure from a rule. It does not change the rule.

| Rule                                                | Deviation                                                                           |
|-----------------------------------------------------|-------------------------------------------------------------------------------------|
| "Secrets **MUST** come from the vault."             | "The batch importer reads one key from an environment variable until 2027-03-31."   |
| "Every endpoint **MUST** carry a conformance test." | "The legacy reports endpoint has none, and is retired in the next quarter instead." |

Where everybody is deviating from the same clause, the clause is wrong. Rewrite the policy or the standard, and close
the deviations it was carrying.

A deviation is also not:

* **A decision about how we build.** "We chose Postgres over MySQL" is an ADR, and it stays true. A deviation is
  written to be closed.
* **An audit finding.** A finding is something an auditor noticed afterwards. A deviation is something we agreed to
  before, or immediately after an incident left no time.
* **A backlog item.** The record says what has to be true for a deviation to close, and the work that closes it stays
  in the tracker. A request in the tracker is not a deviation either, until a named person accepts the risk.

**A deviation sits in the corpus whose people accepted the risk.** It names the clause it departs from, wherever that
clause was written. A corpus consuming a governance layer holds its own departures from that layer's policies. Nothing
flows upward in an export: a corpus reads what it consumes and writes nothing back. The corpus stating a policy never
sees what was taken against it, so an auditor reads every corpus in the estate rather than the governance layer alone.

## Level of detail and cadence

One deviation covers one departure, from the clauses it names or from no stated rule, for a stated scope and a stated
period. It says what you do instead and what compensates for it. How the work itself is done belongs in the repository
doing it.

A deviation changes at its review and when it closes. New deviations arrive whenever a team has to depart from a rule.

## Metadata

<!-- BEGIN GENERATED: schema-deviations -->

| Field            | Value                     | Notes                                                                                           |
|------------------|---------------------------|-------------------------------------------------------------------------------------------------|
| `tier` *†        | `normative`               | The record's trust level, fixed for the type and checked against the folder.                    |
| `status` *†      | `active` `draft` `closed` | `draft` while it is being agreed, `closed` once it no longer applies. In force at `active`.     |
| `owner` *†       | string                    | The individual who accepted the risk, as `human:alex.doe`.                                      |
| `departs-from` * | list                      | Policy or standard clause ids this departs from, as `pol-TRUS.SCREEN`, or `none`.               |
| `risk` *         | `high` `medium` `low`     | How bad the risk is while this stands, after what compensates for it.                           |
| `accepted-on`    | date                      | The day the named owner accepted the risk. Required when `status != draft`.                     |
| `review-by` *    | date                      | The day the deviation is looked at again.                                                       |
| `closed-on`      | date                      | The day the gap was fixed, or the risk knowingly re-accepted. Required when `status == closed`. |
| `assigned-to`    | string                    | Who does the work that closes this, as `human:alex.doe` or `role:head-of-engineering`.          |
| `applies-to`     | list                      | Service ids, or `all`.                                                                          |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-deviations -->

## Adding a deviation

Copy [`_template.md`](deviations/_template.md). Its guidance block states how to write the id and each field. The prompt
under each section states how to write that section.

## What CI checks

<!-- BEGIN GENERATED: checks-deviations -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                     | Level   | What it verifies                                                                                      |
|---------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `ref-resolves`            | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `review-after-acceptance` | error   | The review date falls after the day the risk was accepted.                                            |
| `not-open-ended`          | warning | The record does not claim the departure is permanent or open-ended.                                   |
| `expiry`                  | warning | An active deviation is still inside its review date.                                                  |
| `draft-expiry`            | warning | A draft deviation is still inside its review date.                                                    |
| `high-risk-review-window` | warning | A deviation rated high is reviewed within six months of acceptance.                                   |

<!-- END GENERATED: checks-deviations -->
