---
name: controls-lookup
description: Find out what proves a rule here, in the controls that travel with this plugin. Use when someone asks
  whether a rule is checked, in words like "what checks this", "is that enforced", "where is the evidence", "which
  rules have no control", "what goes unchecked if I delete this job". Use it as well, unprompted, before you change a
  check, a job in a pipeline, or a rule some check may be watching. Read the control before you say a rule is
  enforced, and before you say it is not.
---

# Asking what proves a rule

The corpus travels with this plugin as data. You read it with the tools you already have. There is nothing to install
and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                      # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/controls/<record>.json             # one control this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/controls/<shortcode>/<record>.json # one control a corpus this one consumes wrote
```

Use those paths exactly as they appear above. They are already absolute. An installed plugin sits in a cache of its own
rather than in the repository you are working in. A path you build relative to the working directory resolves nowhere.

## A control here is the check, not the safeguard

**Say which sense you mean before the word reaches anybody with a governance background.** In NIST SP 800-53, ISO/IEC
27001 Annex A and ISO/IEC 27002, a *control* is the safeguard itself: the measure that reduces the risk. In this corpus
a control is the **verification that a rule is being followed**, which those frameworks call an assessment procedure, a
test, or a metric.

A reader who takes the other sense reads a coverage figure from here as a claim that safeguards exist. Such a figure
claims only that checks exist.

## Find the control that covers a rule

**Start from the standard, and search `verifies`.** Every control names the standards it checks, so the standard's id is
the string that finds it.

**There is no flat file here.** A control is read whole, so the record is the unit and each one is its own file. Search
`${CLAUDE_PLUGIN_ROOT}/corpus/controls/`, and read the files that hit.

**A standard may be covered by more than one control, and commonly is.** Two controls often hold two halves of one
promise, and each says in `Coverage and gaps` what the other picks up. Collect every hit before you answer.

**Search the id, then search the subject.** `std-CI` finds the controls naming that standard. Where you do not yet know
which standard covers the subject, search the ordinary word instead: `changelog`, `secret`, `lint`.

## Read the control

Every key in `fields` is present, and holds `null` where the record left it empty. Test the value rather than the key.
The three section keys are always present, because this type requires all three of them.

| Key                          | Type                     | What it holds                                       |
|------------------------------|--------------------------|-----------------------------------------------------|
| `fields.id`                  | string                   | the address to cite the control by                  |
| `fields.title`               | string                   | what the control checks, in one line                |
| `fields.status`              | string                   | `active`, `planned` or `retired`                    |
| `fields.verifies`            | list of strings          | a standard id, or a rule anchor in one. Never empty |
| `fields.mechanism`           | string                   | how the check happens, in the five values below     |
| `fields.frequency`           | string, or null          | how often it runs, in the six values below          |
| `fields.evidence`            | string, or null          | where the proof of a run lives                      |
| `fields.applies-to`          | list of strings, or null | the service ids it covers, or the literal `all`     |
| `fields.tags`                | list of strings, or null | the word a reader arrives with                      |
| `sections.What it checks`    | string of markdown       | the rules it covers, in the author's own words      |
| `sections.How it works`      | string of markdown       | what performs the check, in the author's words      |
| `sections.Coverage and gaps` | string of markdown       | where the control stops, and what nothing watches   |
| `path`                       | string                   | where the record sits in its own repository         |
| `links`                      | object                   | the built link to that record, under `human`        |

**`mechanism` takes one of five values**, and the fifth is the one worth reading for:

* **`ci`.** A pipeline runs it, and `evidence` names the log.
* **`review-checklist`.** A person checks it while reviewing a change.
* **`manual-periodic`.** Somebody runs it on the cadence `frequency` names.
* **`runtime-alert`.** A running system reports it.
* **`not-enforced`.** The rule is written and nothing looks.

**`frequency` takes one of six values**: `per-pr`, `per-deploy`, `daily`, `monthly`, `quarterly` or `annual`. It is set
on every control whose mechanism is not `not-enforced`, because the schema requires it there.

**Read `Coverage and gaps` before you tell anybody a rule is covered.** A control that runs may still miss the half of
the rule that matters to the question in front of you, and that section is where the author says so.

**A bracketed id inside a section is a cross-reference.** The export drops the link definitions at the foot of the
record, so `[ctl-0002]` arrives as the id alone. The id is the address, so search it here rather than treating it as
broken markdown.

## Ask what a job is holding up

**Search `How it works` and `evidence` for the name of the thing you are about to change.** A control's mechanism is
written in the author's own words, so a job, a script or a dashboard appears there under its own name. Search
`${CLAUDE_PLUGIN_ROOT}/corpus/controls/` for that name before you delete it or rename it.

**Every control the search returns loses its mechanism with that job.** Name each one, name the standards its
`verifies` holds, and say that those standards go back to unchecked. That is the cost of the change, and it is the part
the diff does not show.

## Read what `verifies` names before you count

**An entry names a whole standard or one rule inside it, and the dot tells them apart.** `std-CI` is the record.
`std-CI.the-gate-runs-on-every-pull-request-into-main` is one rule of it. Read the values you collected before you
report anything, because the two answer different questions.

**A record id vouches for the whole document, whatever the control checks inside it.** `verifies: [std-CI]` says a
control watches that standard and never says which of its rules. Where every entry you collected is a record id, no
per-rule figure can be had from this export. Say which standards are claimed, and say that the rules beneath them are
not separated, instead of dividing a number nothing here supports.

**A rule anchor is the finer answer, and you may report it as one.** It is the same address `standards-lookup` gives a
rule, so the two skills agree on what has been claimed.

**For which standards nothing claims, work in two steps.** Collect every value of `verifies` across
`${CLAUDE_PLUGIN_ROOT}/corpus/controls/`, then ask `standards-lookup` which standards this export carries and compare
the two sets. That skill reads the standards and this one does not, so hand the question over instead of opening its
files.

**Check `types` in `manifest.json` before you hand it over.** A corpus may adopt `controls` and decline `standards`,
which leaves this plugin holding neither that skill nor the standards to compare against. Where that is what you find,
report the standards the controls name and say the other half of the count is not here.

**A `not-enforced` control is a gap somebody wrote down.** The rule is stated, nothing checks it, and the author
recorded that instead of leaving the standard looking covered. Report it as the honest state it is, and never as an
omission.

**A standard nothing names at all is a different answer.** Nothing here says whether anybody looked. Say that, and say
which corpora you searched.

## Read the prefix on an id

**A prefix names the corpus that wrote the control.** `eng:ctl-0004` was published by the corpus whose shortcode is
`eng`, and a bare `ctl-0004` was written here. The shortcode moves out of the id and becomes the directory, so
`eng:ctl-0004` is at `${CLAUDE_PLUGIN_ROOT}/corpus/controls/eng/ctl-0004.json`.

**The same prefix rule reaches every id inside `verifies`.** `eng:std-GATES` is a standard the consumed corpus wrote,
and `std-CI` is one written here. A control here may verify a standard it inherited, and commonly does.

**`shortcode` is the key into `sources`** in `manifest.json`, which names the producing corpus, the version that
travelled, and where it publishes. Look it up before you say anything about where a control came from, and name the
corpus in words.

## Say what stayed behind

**`types` in `manifest.json` is the list of what travelled.** Read it before you say anything is missing. Where
`standards` is not in it, the standards a control names did not travel, and you can report the ids and no wording.

Two things stay behind whatever `types` says:

* **Who stewards the control.** `owner` is a fact about the corpus that wrote it, and whoever holds a vendored copy has
  no standing to ask.
* **Any Evidence or Owner section the author wrote.** `evidence` carries the address of the proof, and the sections
  beneath that heading stay in the record.

## Say when a control is unsettled

Read `status` before you rely on a control, and tell the reader what you saw:

* **`status: planned`.** The check is intended and is not running. Nothing proves the rule today.
* **`status: retired`.** The check has been stood down. Find what replaced it before you quote it.

An export is a copy taken on a day, and it reads the same however long ago that was. `generatedAt` and `commit` in
`manifest.json` say when it was taken, and are worth quoting alongside either.

## Link to the control, and read its source

**Load the `corpus-retrieval` skill.** It carries which publishing block addresses the corpus that wrote the record, how
to fetch the file through the client that authenticates to that platform, and what to say where nothing reaches it.

**Bring it the record's `path`.** A control has no anchor of its own, because the whole record is the unit. The record's
`links.human` is the URL to quote to a person.

## Say when there is nothing

Where no control matches, say nothing in this export claims to check the rule, and name the corpus and every entry in
`sources` from `manifest.json`, so a reader knows what was searched.

**An empty search is not evidence that a rule goes unchecked.** It says the estate has not written a control down. A
pipeline may well run something nobody recorded here, and this export cannot see it. Offer the rule as one worth a
control, and leave that to whoever owns it.
