---
name: fix-lookup
description: Find out whether somebody here has already solved this problem, in the fixes that travel with this
  plugin. Use when someone asks "has anyone hit this before", "is this a known issue", "why does X fail", or pastes an
  error message at you. Use it as well, unprompted, before you debug anything, and before you file a finding about a
  problem you just met. Search the symptom before you spend an hour on the cause.
---

# Asking whether a problem is already solved

The corpus travels with this plugin as data. You read it with the tools you already have. There is nothing to install
and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                   # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/fixes/<record>.json             # one fix this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/fixes/<shortcode>/<record>.json # one fix a corpus this one consumes wrote
```

Use those paths exactly as they appear above. They are already absolute. An installed plugin sits in a cache of its own
rather than in the repository you are working in. A path you build relative to the working directory resolves nowhere.

## A fix is one problem with one resolution

**Search here first, and debug second.** A fix exists because the problem cost somebody real time. The second person
to meet it searches for a minute instead of debugging for an hour.

**Are you looking at a symptom, or running a procedure?** A symptom is a fix. A planned task is a process, and an
incident with a diagnosis tree is a runbook. Both are different types that may not have travelled at all.

## Search the symptom, not the cause

**Search the words you arrived with.** A fix is written for a reader who has the symptom and does not yet know what
causes it. `symptom-keywords` is filled with error text, service names and what somebody types before they understand
the problem, so search that field before anything else.

**Cut the error message down.** A whole line matches nothing. Drop the version numbers, the paths, the timestamps and
the ids, because those differ on every machine. `no matching distribution found` is what a keyword list contains.

**There is no flat file here.** A fix is read whole, so the record is the unit and each one is its own file. Search
`${CLAUDE_PLUGIN_ROOT}/corpus/fixes/`, and read the files that hit.

**Read `Symptom` on every hit before you pick one.** Each fix describes one problem, so two hits are two problems and
one of them is not yours. The title is the symptom in one line and two problems often look alike in one line.

## Read the fix

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key. The
three keys `Symptom`, `Cause` and `Resolution` are always in `sections`, because this type requires all three.

| Key                       | Type                     | What it contains                                         |
|---------------------------|--------------------------|----------------------------------------------------------|
| `fields.id`               | string                   | the address to cite the fix by                           |
| `fields.title`            | string                   | the symptom in one line, as somebody meets it            |
| `fields.status`           | string                   | `active`, `superseded` or `fixed-upstream`               |
| `fields.symptom-keywords` | list of strings          | the search terms. At least three, and never empty        |
| `fields.applies-to`       | list of strings, or null | the service ids the problem concerns                     |
| `fields.verified`         | list of objects          | one entry per verification, oldest first                 |
| `fields.review-by`        | string                   | the date by which somebody re-checks this is still true  |
| `fields.tags`             | list of strings, or null | the record's own subject words, searched across types    |
| `trust`                   | string                   | how far the fix has been taken on trust, from `verified` |
| `sections.Symptom`        | string of markdown       | what you see, in the author's own words                  |
| `sections.Cause`          | string of markdown       | what produces it                                         |
| `sections.Resolution`     | string of markdown       | the steps that resolve it                                |
| `sections.Why it happens` | string of markdown       | why the problem is easy to hit. Absent where none        |
| `path`                    | string                   | where the record sits in its own repository              |
| `links`                   | object                   | the built link to that record, under `human`             |

**`trust` sits beside `fields`, not inside it.** So does `path`, and so does `links`.

**An optional section is absent from `sections` rather than `null`.** `Why it happens` is the one this type exports, so
test for the key before you read it.

**Each entry of `verified` is an object of two keys.** `at` is a UTC timestamp, as `2026-09-02T17:27:31Z`. `by` names a
person as `human:paul.law`, or an agent as `symptom-sweep/1.4.0`.

**Report Symptom, Cause and Resolution together.** A resolution read without its cause is half an answer, and a
reader who cannot see the cause cannot tell whether the steps apply to them.

**A bracketed id inside a section is a cross-reference.** The export drops the link definitions at the foot of the
record, so `[std-IDEM]` arrives as the id alone. The id is the address, so search it rather than reporting broken
markdown.

## Say how far the fix has been taken on trust

**Report `trust` with every answer.** It says who checked the resolution, and it decides whether the reader is applying
a settled answer or testing one. `kac` derives it from `verified`, so the record cannot claim a tier its own list does
not support.

* **`human-reviewed`.** A person checked that the problem is real and the resolution works. Give it as the answer.
* **`machine-confirmed`.** Agents alone. Somebody reproduced the symptom and ran the resolution, and no person has
  reviewed either. Give it as worth trying, and say who ran it.
* **`unverified`.** `verified` is empty. Nobody has checked anything.

**Quote the newest `at` in `verified` alongside the tier.** A resolution checked two years ago and a resolution checked
last week are the same tier and not the same answer.

**Read `by` before you weigh a machine-confirmed fix.** It names the agent and its version, which is what tells a reader
whether the run that verified it resembles the one in front of them.

## Say when a fix is unsettled

Read `status` and `review-by` before you hand anybody a resolution, and tell them what you saw:

* **`status: superseded`.** Something replaced this. Find what, before you follow it.
* **`status: fixed-upstream`.** The cause is gone. The entry stays for whoever searches for the symptom, so read it for
  the history and expect the resolution to be unnecessary. Meeting the symptom today means something else produces it.
* **`review-by` in the past.** Nobody has re-checked the fix since that date, and the thing it repairs may have been
  rewritten.

An export is a copy taken on a day, and it reads the same however long ago that was. `generatedAt` and `commit` in
`manifest.json` say when it was taken, and are worth quoting alongside any of the three.

## Read the prefix on an id

**A prefix names the corpus that wrote the fix.** `eng:fix-0004` was published by the corpus whose shortcode is `eng`,
and a bare `fix-0004` was written here. The shortcode moves out of the id and becomes the directory, so `eng:fix-0004`
is at `${CLAUDE_PLUGIN_ROOT}/corpus/fixes/eng/fix-0004.json`.

**`shortcode` is the key into `sources`** in `manifest.json`, which names the producing corpus, the version that
travelled, and where it publishes. Look it up before you say anything about where a fix came from, and name the corpus
in words.

## Say what stayed behind

**`types` in `manifest.json` is the list of what travelled.** Read it before you say anything is missing. Where
`services` is not in it, the ids in `applies-to` name records that did not travel, and you can report the ids and no
more.

Three things stay behind whatever `types` says:

* **Who stewards the fix.** `owner` is a fact about the corpus that wrote it.
* **The discovery it was promoted from.** `promoted-from` does not travel, so this export cannot say what was noticed
  first or who noticed it.
* **How the author found it, and what it relates to.** A fix may have a `How we found it` section and a `Related`
  section, and neither is exported. The diagnostic route is often the reusable part, so fetch the record where the
  resolution does not fit your case.

## Link to the fix, and read its source

**Load the `corpus-retrieval` skill.** It says which publishing block addresses the corpus that wrote the record, how to
fetch the file through the client that authenticates to that platform, and what to say where nothing reaches it.

**Bring it the record's `path`.** A fix has no anchor of its own, because the whole record is the unit. The record's
`links.human` is the URL to quote to a person.

## Say when the fix is about a different problem

A hit that is not yours is common here, because `symptom-keywords` is deliberately over-filled. `pip` and `macos` reach
a fix about a linter and a fix about a build alike. Answer that case in three steps:

1. **Say the export describes no fix for your symptom**, in the words you searched.
2. **Name the nearest fix, and say what problem it is about.** Quote its `Symptom` as the author wrote it.
3. **Leave its resolution alone.** A resolution is verified against the cause its own record states, and yours is a
   different cause.

Handing over a resolution without saying it was written for something else is worse than handing over nothing. It
arrives verified, and the verification does not reach your case.

## Say when there is nothing

Where no fix matches, say the export describes no problem like this one, and name the corpus and every entry in
`sources` from `manifest.json`, so a reader knows what was searched.

**Try the words a second time before you answer.** An empty search is usually a search of your own vocabulary. The error
message, the tool that printed it, and the thing you were doing are three different sets of words, and a keyword list is
written for one reader who arrived with one of them.

**An empty search is not evidence the problem is new.** It says the estate has not written this one down. Somebody may
have solved it last week and told nobody.

**What you solve next is worth filing.** A fix is verified, and a session cannot verify its own work, so what you
noticed goes back as a discovery for somebody else to check. Load the `raise-finding` skill and let it write the issue.
