---
trigger: Use before changing a check or CI job, or asked what checks a rule.
---

# Reading a control

A control states how the estate knows a rule is followed. Use it as evidence: what checks a rule, how, and where the
check stops. Read the control before you say a rule is enforced, and before you say it is not, because either claim
made from memory reaches somebody deciding whether to rely on it.

**Say which sense of control you mean.** In this corpus a control is the **verification that a rule is followed**. In
NIST SP 800-53, ISO/IEC 27001 Annex A and ISO/IEC 27002 a *control* is the safeguard itself, and the verification is
an assessment procedure, a test or a metric. Say which sense you mean to a reader with a governance background, because
a coverage figure from here claims only that checks exist.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/controls/<record>.json             # one control this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/controls/<shortcode>/<record>.json # one control a corpus this one consumes wrote
```

Each control is its own file, because a control is read whole. There is no flat file of parts.

## Find the record

Search `${CLAUDE_PLUGIN_ROOT}/corpus/controls/`, and read every file that hits.

* **Search the rule's id in `verifies`**, as `std-CI.PERMS`. Every entry names one rule the control checks. Search the
  standard's id followed by a dot, as `std-CI.`, to find every control checking any rule of it.
* **Search the subject** where you do not yet know the rule, as `changelog`, `secret` or `lint`.
* **Collect every hit.** One rule is often covered by two controls, each holding half of one promise, and each says in
  `Coverage and gaps` what the other picks up.

**Where no control matches, say that nothing in this export claims to check the rule.** An empty search states only
that the estate has not written a control down: a pipeline may run a check nobody recorded. Offer the rule as one worth
a control. Where a tool could check it, tell the developer so, because they can add the check and the control together.

### Find what a job is holding up

Search `How it works` and `evidence` for the name of a job, a script or a dashboard before you delete or rename it. A
mechanism is written in the author's own words, so the thing appears under its own name. Every control the search
returns loses its mechanism with the job. Name each one and the rules in its `verifies`, and say those rules go back
to unchecked, because that cost is the part the diff does not show.

### List the controls that travelled

**Where someone asks which controls the estate has, list `${CLAUDE_PLUGIN_ROOT}/corpus/controls/`.** Each file is
one control. A file under `corpus/controls/<shortcode>/` came from a corpus this one consumes, and it counts as fully
as one written here. Its `fields.id` is bare, so cite it with the prefix, as `eng:ctl-secret-scanner`.

Answer with each control's id and `fields.title`, so a reader can open each one.

**Read the sections to test a check in hand.** Where the question is whether a check in hand is already recorded,
read the `What it checks` and `Coverage and gaps` of each control on its subject. A title on the same subject often
leaves that check out.

### Find the rules nothing checks

**Answer a question about the whole corpus from a report.** Where someone asks which rules or clauses nothing checks
across the corpus, read `reports.md` beside this file and answer from the report it finds. Coverage is a question about
the whole corpus, and a report holds the verdicts and limits a person wrote. A count built here from the controls has
neither. Count from the controls only where no report answers, and say so.

To count, collect every value of `verifies`, then read `standards.md` beside this file to find which rules this export
has, and compare the sets. Check `types` in `manifest.json` first. Where `standards` did not travel, report the rules
the controls name, and say the other half of the count is not here.

A rule that no control names is a different answer from a `not-enforced` control. Nothing here says whether anybody
looked, so say that, and say which corpora you searched.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key. The
three section keys are always present, because this type requires all three.

| Field                        | Type                     | What it holds                                                                                      |
|------------------------------|--------------------------|----------------------------------------------------------------------------------------------------|
| `fields.id`                  | string                   | the address to cite the control by                                                                 |
| `fields.title`               | string                   | what the control checks, in one line                                                               |
| `fields.status`              | string                   | `active`, `planned` or `retired`                                                                   |
| `fields.review-by`           | string                   | the date the control is meant to be read again                                                     |
| `fields.verifies`            | list of strings          | the rules it checks, as `std-CI.PERMS`. Never empty                                                |
| `fields.mechanism`           | string                   | how the check happens, in the six values below                                                     |
| `fields.frequency`           | string, or null          | how often it runs, in the eight values below                                                       |
| `fields.last-verified`       | object, string, or null  | the last manual run: `at` and `by` are strings, and `method` is always null. Or the string `never` |
| `fields.evidence`            | string, or null          | where the proof of a run lives                                                                     |
| `fields.applies-to`          | list of strings, or null | the service ids it covers, or the literal `all`                                                    |
| `fields.tags`                | list of strings, or null | the word a reader arrives with                                                                     |
| `sections.What it checks`    | string of markdown       | the rules it covers, in the author's own words                                                     |
| `sections.How it works`      | string of markdown       | what performs the check, in the author's words                                                     |
| `sections.Coverage and gaps` | string of markdown       | where the control stops, and what nothing watches                                                  |
| `path`                       | string                   | where the record sits in its own repository                                                        |
| `links`                      | object                   | the built link to that record, under `human`                                                       |

`mechanism` takes one of six values:

* **`ci`.** A pipeline runs it, and `evidence` names the log.
* **`local-hook`.** A pre-commit check or a Claude Code hook runs it on a developer's machine. The developer can
  bypass it, so say the rule still needs a person to check it.
* **`review-checklist`.** A person checks it while reviewing a change.
* **`manual`.** Somebody runs it on the cadence `frequency` names, and `last-verified` says who last did and when.
* **`runtime-alert`.** A running system reports it.
* **`not-enforced`.** The rule is written, and nothing checks it. The author recorded that gap, so report it as the
  honest state it is.

`frequency` takes one of eight values: `continuous`, `per-commit`, `per-pr`, `per-deploy`, `daily`, `monthly`,
`quarterly` or `annual`. The schema requires it on every control whose mechanism is not `not-enforced`.

**Give `last-verified` beside any answer from a `manual` control**: who ran the check, from `by`, and the day, from
`at`. It is set on that mechanism alone, because no log records a manual run, and a day long past or `never` is a
check that may not have run.

**Read `Coverage and gaps` before you say a rule is covered.** A control that runs may still miss the half of the rule
the question is about, and this section is where the author says so.

**Search a bracketed id inside a section**, as `[ctl-secret-scanner]`. The export drops the link definitions, so the
id alone is the cross-reference.

### Read what `verifies` names

An entry names one rule: `std-CI.GATE` is the rule `GATE` of the standard `std-CI`. It is the id the standards in this
export give the rule, so a control and its standard agree on what is claimed. A control checking every rule of a
standard lists every rule.

* **A rule id says the control checks that rule**, and its `What it checks` says which obligations of it. A rule can
  hold more obligations than the control watches, so read that section before you call the rule covered.
* **An entry with no dot names a whole standard**, which an export written before rule ids may carry. Report that
  standard as claimed and say its rules are not separated. A per-rule figure divided out of it is a number nothing
  here supports.
* **An entry with a prefix names a rule another corpus wrote.** The prefix on a control's id reaches every id in its
  `verifies`, so a control here may verify `eng:std-GATES.BLOCK`, a rule it inherited.

## Say whether it is current

Read the `status` that puts a control in force from `inForce`, a string, `null`, or absent, on the `controls` entry of
`types` in `manifest.json`. The schema can change it, so take it from the export every time. Read `status` and
`review-by` before you rely on a control, and tell the reader what you saw:

* **The `inForce` value**: in force. Answer from it.
* **`status: planned`**: the check is intended and is not running. Nothing proves the rule today.
* **`status: retired`**: the check was stood down. Find what replaced it, and quote that.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written, and say the control proves
  nothing today.
* **`review-by` earlier than today**: nobody has confirmed the control still runs as this record says.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. Call no control in force on the strength of its status.
`planned` and `retired` still mean what the bullets above say.

The export reads the same however old it is. Quote `generatedAt` and `commit` from `manifest.json` beside any of these.

## Fetch the source

`types` in `manifest.json` lists what travelled. Read it before you say anything is missing, and send the reader to the
published control for the rest.

* **The rules a control names** are here only where `standards` travelled. Otherwise report the ids alone.
* **Who stewards the control** stays behind, because `owner` is a fact about the corpus that wrote it.
* **Any section beyond the three required ones** stays in the record.

Quote the record's `links.human` to link the control. To read what stayed behind, read
`${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the record's `path`. A control has no
anchor, because the whole record is the unit.

**Where the record cannot be fetched**, answer from the three sections that travelled, and give `links.human`. Say that
the rest of the record did not come with the plugin, and ask the reader to open the link for it.
