# Controls

How we know the standards are being followed.

**[→ Index](controls/_index.md)**

## What is a control?

A control binds a rule to its enforcement. It names the rules it verifies, the mechanism that checks them, how often
that check runs, and where the evidence lives.

## Why we use them

A standard states a rule and says nothing about whether anyone still follows it. A control answers that question for one
rule. Taken together, the controls make the gap between *rule* and *enforcement* countable: what share of our standards
does something check?

When someone asks whether we follow our own standards, the controls answer honestly. Without them we can only point
at the standards and hope.

**`not-enforced` matters more than any other `mechanism` value.** Using it turns an aspiration into a number someone can
see, and the number is worth reading only while it is honest.

## Scope

A control is not the rule it checks. The rule lives in a [standard](standards.md), and the control says how that rule is
checked.

| Standard                                                   | Control                                                                  |
|------------------------------------------------------------|--------------------------------------------------------------------------|
| "Secrets **MUST** come from a managed vault."              | "CI runs secret scanning on every PR, and failures block merge."         |
| "Every public endpoint **MUST** carry a conformance test." | "Quarterly manual audit of the OpenAPI document against the test suite." |

Anything that can fail a build, block a merge, raise an alert or produce an audit artefact is a control. Anything that
tells you what to do is a standard.

A control names the services it applies to, because a control with no scope is one nobody owns. One control may verify
several rules, and one rule may need several controls.

## Level of detail and cadence

One control describes one mechanism: what it checks, how it runs, and the evidence it leaves. It cites the rules it
verifies and leaves their wording to the record that states them. The code of the check lives in the repository that
runs it.

A control changes when the check or the pipeline running it changes. A manual control also changes each time somebody
runs it, because `last-verified` records the day. New controls arrive when a rule gains a check.

## A control or a skill

Where a CI job, a pre-commit check or a Claude Code hook can check a rule, write a control for that check. The control
proves the rule. A skill in the corpus's plugin (the package of skills a Claude Code session installs) only explains it,
and helps only when a session loads it and acts on it. A check fails a broken rule whether or not anyone read it.

## Metadata

<!-- BEGIN GENERATED: schema-controls -->

| Field           | Value                                                                     | Notes                                                                                                       |
|-----------------|---------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------|
| `tier` *†       | `normative`                                                               | The record's trust level, fixed for the type and checked against the folder.                                |
| `status` *†     | `active` `planned` `retired`                                              | Whether the record is in force, intended, or stood down, whatever its `mechanism` is. In force at `active`. |
| `verifies` *    | list                                                                      | The rules this control checks, as `std-CI.PERMS`.                                                           |
| `mechanism` *   | `ci` `review-checklist` `manual` `runtime-alert` `not-enforced`           | How the check runs, or `not-enforced` where nothing checks the rule.                                        |
| `frequency`     | `continuous` `per-pr` `per-deploy` `daily` `monthly` `quarterly` `annual` | How often it runs, or `continuous` where the check never stops. Required when `mechanism != not-enforced`.  |
| `last-verified` | date                                                                      | Quoted. The day somebody last ran this check, or `"never"`. Required when `mechanism == manual`.            |
| `evidence`      | string                                                                    | Where the proof lives: the build log, the audit note, or the dashboard.                                     |
| `applies-to`    | list                                                                      | Service ids, or `all`.                                                                                      |
| `review-by` *   | date                                                                      | Quoted. The day somebody checks this control still describes what runs.                                     |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-controls -->

## Adding a control

Copy [`_template.md`](controls/_template.md). Its guidance block states how to write the id and each field. The prompt
under each section states how to write that section.

## What CI checks

<!-- BEGIN GENERATED: checks-controls -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                      | Level   | What it verifies                                                                                      |
|----------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `clause-quoted-faithfully` | error   | A span quoted beside a clause citation is still in the clause it cites.                               |
| `ref-resolves`             | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `reciprocal`               | error   | A reciprocal field and its counterpart agree in both directions.                                      |
| `mechanism-has-evidence`   | warning | A running control says where its evidence can be found.                                               |
| `control-in-date`          | warning | A control names a review date that has not passed.                                                    |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule              | What it would verify                                                                   |
|-------------------|----------------------------------------------------------------------------------------|
| `coverage-report` | The share of MUST / MUST NOT rules in active standards claimed by an enforced control. |

<!-- END GENERATED: checks-controls -->
