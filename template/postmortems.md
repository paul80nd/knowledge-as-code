# Postmortems

What actually happened, and why.

**[→ Index](postmortems/_index.md)**

## What is a postmortem?

A blameless account of one incident: when it began, what customers lost, what caused it, what worked, and the actions it
left us with.

Publishing one makes it the account the estate stands behind. Where a fact in it turns out wrong, we correct it,
because a fair account is worth more than an unchanged one.

## Why we use them

The ADR log records what we intended. A postmortem records what the estate did instead, and the gap between the two is
what we did not know on the day we decided.

One incident routinely produces a [fix](fixes.md), a [runbook](runbooks.md), a revised [NFR](nfrs.md) and sometimes an
[ADR](adrs.md). A root cause that recurs shows up in no single account, so read several postmortems together when you
want to know what keeps breaking.

## Scope

A postmortem is blameless, and [the template](postmortems/_template.md) says how to write one. Its boundaries:

* **[Runbook](runbooks.md)**: instructions for an incident that might happen. A postmortem is an account of one that
  did.
* **[Fix](fixes.md)**: a reusable resolution, which an incident often produces as a by-product. A postmortem is the
  account of the incident itself.
* **A work item.** Each action is an item in your tracker. The postmortem links to it and tracks nothing itself.

Not every incident needs one. The corpus's severity scale says which incidents do, and `severity` records the highest
level the incident reached on it. Applied the same way each time, the scale makes the absence of a postmortem mean
something.

## Level of detail and cadence

One postmortem covers one incident. The timeline records what happened at each moment, and each action is one
follow-up with one owner. A step somebody should run next time belongs in a runbook or a fix, which an action creates.

A published postmortem changes where a fact in it is wrong. New postmortems arrive with each incident the severity scale
says needs one.

## Metadata

<!-- BEGIN GENERATED: schema-postmortems -->

| Field           | Value                | Notes                                                                                                                          |
|-----------------|----------------------|--------------------------------------------------------------------------------------------------------------------------------|
| `tier` *†       | `decided`            | The record's trust level, fixed for the type and checked against the folder.                                                   |
| `status` *†     | `draft` `published`  | `published` is the account the estate stands behind, changed after that only to fix a typo or a link. In force at `published`. |
| `occurred-at` * | timestamp            | The moment the incident began, in UTC as `2026-09-07T20:18:00Z`.                                                               |
| `detected-at` * | timestamp            | The moment somebody noticed the incident, in UTC as `2026-09-07T20:18:00Z`.                                                    |
| `restored-at`   | timestamp            | The moment service was back for users, in UTC as `2026-09-07T20:18:00Z`. Required when `status == published`.                  |
| `duration`      | string               | How long the incident lasted, as an ISO 8601 duration such as `PT4H20M`. Required when `status == published`.                  |
| `severity` *    | `sev1` `sev2` `sev3` | The highest severity the incident reached, on the corpus's severity scale.                                                     |
| `affected` *    | list                 | Service and offering ids the incident affected.                                                                                |
| `prompted`      | list                 | The ADRs, runbooks, NFRs, fixes and standards this incident caused.                                                            |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-postmortems -->

## Adding a postmortem

Copy [`_template.md`](postmortems/_template.md). Its guidance block states how to write the id and each field. The
prompt under each section states how to write that section.

## What CI checks

<!-- BEGIN GENERATED: checks-postmortems -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                          | Level | What it verifies                                                                                      |
|--------------------------------|-------|-------------------------------------------------------------------------------------------------------|
| `timestamp-format`             | error | Timestamp fields name a moment the calendar has, in UTC: `YYYY-MM-DDThh:mm:ssZ`.                      |
| `ref-resolves`                 | error | An id in a field that references another document names one that exists, of the type the field names. |
| `duration-matches-the-moments` | error | `duration` is the span from `occurred-at` to `restored-at`.                                           |
| `immutable-after-published`    | error | With `--since`, a published postmortem has changed by no more than a typo, a link or its status.      |
| `detected-not-before-occurred` | error | `detected-at` is at or after `occurred-at`.                                                           |
| `restored-not-before-occurred` | error | `restored-at` is at or after `occurred-at`.                                                           |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                    | What it would verify                                         |
|-------------------------|--------------------------------------------------------------|
| `recurring-root-causes` | Scheduled. Reports root causes recurring across postmortems. |

<!-- END GENERATED: checks-postmortems -->
