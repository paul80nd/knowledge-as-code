---
name: controls-lookup
description: Find out what proves a rule here, in the controls that travel with this plugin. Use when someone asks
  whether a rule is checked, or which rules nothing checks. Use it as well, unprompted, before you change or delete a
  check, a pipeline job, or a rule some check may be watching.
---

# Asking what proves a rule

A control states how the estate knows a rule is followed. Use it as evidence: what checks a rule, how, and where the
check stops. Read the control before you say a rule is enforced, and before you say it is not, because either claim
made from memory reaches somebody deciding whether to rely on it.

The corpus travels with this plugin as data. There is nothing to install and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                      # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/controls/<record>.json             # one control this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/controls/<shortcode>/<record>.json # one control a corpus this one consumes wrote
```

Use those paths exactly as written. An installed plugin sits in a cache outside the repository you are working in.

## Say which sense of control you mean

In this corpus a control is the **verification that a rule is followed**. In NIST SP 800-53, ISO/IEC 27001 Annex A and
ISO/IEC 27002 a *control* is the safeguard itself, and the verification is an assessment procedure, a test or a metric.
Say which sense you mean to a reader with a governance background, because a coverage figure from here claims only that
checks exist.

## Find every control that covers a rule

Each control is its own file, because a control is read whole. Search `${CLAUDE_PLUGIN_ROOT}/corpus/controls/`, and
read the files that hit.

* **Search the standard's id in `verifies`**, as `std-CI`. Every control names the standards it checks.
* **Search the subject** where you do not yet know the standard, as `changelog`, `secret` or `lint`.
* **Collect every hit.** One standard is often covered by two controls, each holding half of one promise, and each says
  in `Coverage and gaps` what the other picks up.

## Read the control

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key. The
three section keys are always present, because this type requires all three.

| Key                          | Type                     | What it holds                                       |
|------------------------------|--------------------------|-----------------------------------------------------|
| `fields.id`                  | string                   | the address to cite the control by                  |
| `fields.title`               | string                   | what the control checks, in one line                |
| `fields.status`              | string                   | `active`, `planned` or `retired`                    |
| `fields.verifies`            | list of strings          | a standard id, or a rule anchor in one. Never empty |
| `fields.mechanism`           | string                   | how the check happens, in the five values below     |
| `fields.frequency`           | string, or null          | how often it runs, in the seven values below        |
| `fields.last-verified`       | string, or null          | the day a manual check last ran, or `never`         |
| `fields.evidence`            | string, or null          | where the proof of a run lives                      |
| `fields.applies-to`          | list of strings, or null | the service ids it covers, or the literal `all`     |
| `fields.tags`                | list of strings, or null | the word a reader arrives with                      |
| `sections.What it checks`    | string of markdown       | the rules it covers, in the author's own words      |
| `sections.How it works`      | string of markdown       | what performs the check, in the author's words      |
| `sections.Coverage and gaps` | string of markdown       | where the control stops, and what nothing watches   |
| `path`                       | string                   | where the record sits in its own repository         |
| `links`                      | object                   | the built link to that record, under `human`        |

`mechanism` takes one of five values:

* **`ci`.** A pipeline runs it, and `evidence` names the log.
* **`review-checklist`.** A person checks it while reviewing a change.
* **`manual`.** Somebody runs it on the cadence `frequency` names, and `last-verified` says when they last did.
* **`runtime-alert`.** A running system reports it.
* **`not-enforced`.** The rule is written, and nothing checks it. The author recorded that gap, so report it as the
  honest state it is.

`frequency` takes one of seven values: `continuous`, `per-pr`, `per-deploy`, `daily`, `monthly`, `quarterly` or
`annual`. The schema requires it on every control whose mechanism is not `not-enforced`.

**Give `last-verified` beside any answer from a `manual` control.** It is set on that mechanism alone, because no log
records a manual run, and a date long past or `never` is a check that may not have run.

**Read `Coverage and gaps` before you say a rule is covered.** A control that runs may still miss the half of the rule
the question is about, and this section is where the author says so.

**Search a bracketed id inside a section**, as `[ctl-0002]`. The export drops the link definitions, so the id alone is
the cross-reference.

## Say what a job is holding up

Search `How it works` and `evidence` for the name of a job, a script or a dashboard before you delete or rename it. A
mechanism is written in the author's own words, so the thing appears under its own name. Every control the search
returns loses its mechanism with the job. Name each one and the standards in its `verifies`, and say those standards
go back to unchecked, because that cost is the part the diff does not show.

## Read what `verifies` names before you count

An entry is a whole standard or one rule inside it, and a dot tells them apart: `std-CI` is the record, and
`std-CI.the-gate-runs-on-every-pull-request-into-main` is one of its rules.

* **A record id vouches for the whole standard**, and never says which rules the control checks. Where every entry you
  collected is a record id, report which standards are claimed and say their rules are not separated. A per-rule
  figure divided out of that is a number nothing here supports.
* **A rule anchor is the finer answer.** It is the address `standards-lookup` gives a rule, so both skills agree on
  what is claimed.

To find which standards nothing claims, collect every value of `verifies`, then ask `standards-lookup` which standards
this export has, and compare the sets. That skill reads the standards, so hand the question to it. Check `types` in
`manifest.json` first: where `standards` did not travel, report the standards the controls name and say the other half
of the count is not here.

A standard that no control names is a different answer from a `not-enforced` control. Nothing here says whether anybody
looked, so say that, and say which corpora you searched.

## Read the prefix on an id

**A prefix names the corpus that wrote the control.** `eng:ctl-0004` was published by the corpus with shortcode `eng`,
and a bare `ctl-0004` was written here. `eng:ctl-0004` is at `${CLAUDE_PLUGIN_ROOT}/corpus/controls/eng/ctl-0004.json`.
The same prefix reaches every id in `verifies`, so a control here may verify `eng:std-GATES`, a standard it inherited.
Look the `shortcode` up in `sources` in `manifest.json`, and name that corpus in words.

## Link the control, and read its source

Quote the record's `links.human` to link the control. To read the source, load `corpus-retrieval` and bring it the
record's `path`. A control has no anchor, because the whole record is the unit.

## Say what stayed behind

`types` in `manifest.json` lists what travelled. Read it before you say anything is missing, and send the reader to the
published control for the rest.

* **The standards a control names** are here only where `standards` travelled. Otherwise report the ids alone.
* **Who stewards the control** stays behind, because `owner` is a fact about the corpus that wrote it.
* **Any section beyond the three required ones** stays in the record.

## Say when a control is unsettled

Read `status` and `review-by` before you rely on a control, and tell the reader what you saw:

* **`status: planned`**: the check is intended and is not running. Nothing proves the rule today.
* **`status: retired`**: the check was stood down. Find what replaced it, and quote that.
* **`review-by` earlier than today**: nobody has confirmed the control still runs as this record says.

The export reads the same however old it is. Quote `generatedAt` and `commit` from `manifest.json` beside any of these.

## Say when there is nothing

Where no control matches, say that nothing in this export claims to check the rule. Name the corpus and every entry in
`sources` from `manifest.json`, so the reader knows what was searched. An empty search states only that the estate has
not written a control down: a pipeline may run a check nobody recorded. Offer the rule as one worth a control. Where a
tool could check it, tell the developer so, because they can add the check and the control together.
