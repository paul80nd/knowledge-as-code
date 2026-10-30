---
name: fix-lookup
description: Find out whether somebody here has already solved this problem, in the fixes that travel with this
  plugin. Use when someone asks whether a problem is known or which fixes there are, or pastes an error message at
  you. Use it as well, unprompted, before you debug anything, and before you file a finding about a problem you just
  met.
---

# Asking whether a problem is already solved

A fix states one problem and how it was resolved. Search here before you debug, because a fix exists where the problem
already cost somebody real time. Where the symptom matches and a person has verified the fix, follow its resolution as
written. Where nobody has verified it, use it as a lead to test. Quote and cite the fix either way, so the person you
work with can see what you relied on.

The corpus travels with this plugin as data. There is nothing to install and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                   # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/fixes/<record>.json             # one fix this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/fixes/<shortcode>/<record>.json # one fix a corpus this one consumes wrote
${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/runbooks.md      # how to answer from a runbook when something is broken
```

Use those paths exactly as written. An installed plugin sits in a cache outside the repository you are working in.

## Search the runbooks when something is broken now

**Where no fix matches, or the problem is a failure happening now, read
`${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/runbooks.md`.** Search the runbooks' exported `Symptoms` as it says, and
answer from a runbook that matches by its rules. A fix is a known issue with a resolution somebody has verified. A
runbook is a controlled procedure for a failure, run under duress, and its steps stay behind on purpose. Where
`runbooks` is not in `types` in `manifest.json`, this corpus exports none, so say that. A planned task is a process,
which `process-lookup` reads.

## Search the symptom

Each fix is its own file, because a fix is read whole. Search `${CLAUDE_PLUGIN_ROOT}/corpus/fixes/`, and read the files
that hit.

**Search the words you arrived with.** `symptom-keywords` is filled with error text, service names and what somebody
types before they understand the problem, because a fix is written for a reader who has the symptom and not the cause.

**Cut an error message down to its stable words.** Drop the version numbers, the paths, the timestamps and the ids,
because those differ on every machine. `no matching distribution found` is what a keyword list contains.

**Read `Symptom` on every hit before you pick one.** Each fix describes one problem, and two problems often look alike
in a one-line title.

## Read the fix

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.
`Symptom`, `Environment`, `Cause` and `Resolution` are always in `sections`, because this type requires all four.

| Key                       | Type                     | What it contains                                         |
|---------------------------|--------------------------|----------------------------------------------------------|
| `fields.id`               | string                   | the address to cite the fix by                           |
| `fields.title`            | string                   | the symptom in one line, as somebody meets it            |
| `fields.status`           | string                   | `active`, `draft`, `superseded` or `fixed-upstream`      |
| `fields.symptom-keywords` | list of strings          | the search terms. At least three, and never empty        |
| `fields.applies-to`       | list of strings, or null | the service ids the problem concerns                     |
| `fields.verified`         | list of objects, or null | one entry per actor who has checked it, oldest first     |
| `fields.review-by`        | string                   | the date by which somebody re-checks this is still true  |
| `fields.tags`             | list of strings, or null | the record's own subject words, searched across types    |
| `trust`                   | string                   | how far the fix has been taken on trust, from `verified` |
| `sections.Symptom`        | string of markdown       | what you see, in the author's own words                  |
| `sections.Environment`    | string of markdown       | the tool, the platform and the versions it happened on   |
| `sections.Cause`          | string of markdown       | what produces it                                         |
| `sections.Resolution`     | string of markdown       | the steps that resolve it                                |
| `sections.Why it happens` | string of markdown       | why the problem is easy to hit. Absent where none        |
| `path`                    | string                   | where the record sits in its own repository              |
| `links`                   | object                   | the built link to that record, under `human`             |

`trust`, `path` and `links` sit beside `fields`. `Why it happens` is optional, so it is absent from `sections` where
the record has none: test for the key before you read it.

Each entry of `verified` is an object with two keys. `at` is a UTC timestamp, as `2026-09-02T17:27:31Z`. `by` names a
person as `human:paul.law`, or an agent as `symptom-sweep/1.4.0`. Each actor appears once, with the last time it
checked, so the list says who has read the fix and not how often.

**Report Symptom, Environment, Cause and Resolution together.** A resolution read without its cause is half an answer,
and a reader who cannot see the environment cannot tell whether the steps apply to them.

**Search a bracketed id inside a section**, as `[std-IDEM]`. The export drops the link definitions, so the id alone is
the cross-reference.

## Say how far the fix has been taken on trust

Report `trust` with every answer, because it decides whether the reader applies a settled answer or tests one. `kac`
derives it from `verified`, so a record cannot claim a tier its own list does not support.

* **`human-reviewed`**: a person checked that the problem is real and the resolution works. Give it as the answer.
* **`machine-confirmed`**: non-`human:` actors alone, agents or processes, reproduced the symptom and ran the
  resolution. Give it as worth trying, and say which actor ran it, from `by`.
* **`unverified`**: `verified` is empty, and nobody has checked anything.

Quote the newest `at` beside the tier, because a resolution checked two years ago and one checked last week share a
tier and not an answer.

## Say when a fix is unsettled

Read the `status` that puts a fix in force from `inForce`, a string, `null`, or absent, on the `fixes` entry of
`types` in `manifest.json`. The schema can change it, so take it from the export every time. Read `status` and
`review-by` before you hand anybody a resolution, and tell them what you saw:

* **The `inForce` value**: in force. Answer from it.
* **`status: draft`**: nobody has verified the resolution. Give it as a suggestion to test.
* **`status: superseded`**: something replaced this. Find that, and quote it.
* **`status: fixed-upstream`**: the cause is gone, and the record stays for whoever searches the symptom. Meeting the
  symptom today means something else produces it.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.
* **`review-by` earlier than today**: nobody has re-checked the fix, and the thing it repairs may have changed.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. Call no fix in force on the strength of its status.
`draft`, `superseded` and `fixed-upstream` still mean what the bullets above say.

The export reads the same however old it is. Quote `generatedAt` and `commit` from `manifest.json` beside any of these.

## Read the prefix on an id

**A prefix names the corpus that wrote the fix.** `eng:fix-build-times-out` was published by the corpus with shortcode
`eng`, and a bare `fix-build-times-out` was written here. `eng:fix-build-times-out` is at
`${CLAUDE_PLUGIN_ROOT}/corpus/fixes/eng/fix-build-times-out.json`. Look the `shortcode` up in `sources` in
`manifest.json`, and name that corpus in words.

## Link the fix, and read its source

Quote the record's `links.human` to link the fix. To read the source, load `corpus-retrieval` and bring it the record's
`path`. A fix has no anchor, because the whole record is the unit.

## Say what stayed behind

`types` in `manifest.json` lists what travelled. Read it before you say anything is missing, and send the reader to the
published fix for the rest.

* **The services in `applies-to`** are here only where `services` travelled. Otherwise report the ids alone.
* **Who stewards the fix** stays behind, because `owner` is a fact about the corpus that wrote it.
* **How the author found it, and what it relates to**, stay in the record's `How we found it` and `Related` sections.
  The diagnostic route is often the reusable part, so fetch the record where the resolution does not fit your case.

## Say when the nearest fix is about a different problem

A hit that is not yours is common, because `symptom-keywords` is filled generously: `pip` and `macos` reach a fix about
a linter and a fix about a build alike. Say the export describes no fix for your symptom, in the words you searched.
Quote the nearest fix's `Symptom`, and say what problem it is about. Leave its resolution out of your answer, because
it was verified against a different cause, and a verified resolution handed over for the wrong problem reads as settled.

## List the fixes that travelled

**Where someone asks which fixes the estate has, list `${CLAUDE_PLUGIN_ROOT}/corpus/fixes/`.** Each file is one fix.
A file under `corpus/fixes/<shortcode>/` came from a corpus this one consumes, and it counts as fully as one written
here. Its `fields.id` is bare, so cite it with the prefix, as `eng:fix-build-times-out`.

Answer with each fix's id and `fields.title`, so a reader can open each one.

**Read the symptom to test a problem in hand.** Where the question is whether a problem in hand is already known,
read the `Symptom` of each fix on its subject. Two problems often look alike in a one-line title.

**Where someone asks what nothing covers, load `ask-the-corpus`, and stop here.** Coverage is a question about the
whole corpus, and a report is what answers it.

## Say when there is nothing

Search a second time with other words before you answer, because the error message, the tool that printed it and the
task you were doing are three vocabularies. Where no fix and no runbook matches, say that this export describes no
problem like this one. Name the corpus and every entry in `sources` from `manifest.json`, so the reader knows what was
searched. An empty search states only that the estate has not written this one down. Somebody may have solved it last
week and told nobody.

What you solve next is worth filing. A session cannot verify its own fix, so load `raise-finding` and let it file what
you noticed for somebody else to check.
