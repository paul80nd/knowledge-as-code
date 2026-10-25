---
name: process-lookup
description: Find the procedure for a planned task, in the processes that travel with this plugin. Use when someone
  asks how a task is carried out here. Use it as well, unprompted, before you carry out a planned task somebody here
  may already have written down.
---

# Finding the process for a task

A process states how the estate carries out a planned task. Work in the shape it sets out, and judge each step against
the work in front of you, because a step states an outcome and the means are yours. Find the one process written for
the task, then quote and cite it, so the person you work with can see whose order you are working to.

The corpus travels with this plugin as data. There is nothing to install and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                       # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/processes/<record>.json             # one process this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/processes/<shortcode>/<record>.json # one process a corpus this one consumes wrote
```

Use those paths exactly as written. An installed plugin sits in a cache outside the repository you are working in.

## Find the one process for the task

A process is written for one task, so look for the single one whose trigger is your situation. Two processes followed
together interleave two orders, and each assumes the state the other left out. A task done because something broke is a
runbook's, and a runbook is its own type.

Each process is its own file, because a process is read whole. Search `${CLAUDE_PLUGIN_ROOT}/corpus/processes/`, and
read the files that hit.

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.
Both section keys are always present, because this type requires both sections.

| Key                          | Type                     | What it holds                                                     |
|------------------------------|--------------------------|-------------------------------------------------------------------|
| `fields.id`                  | string                   | the address to cite the process by                                |
| `fields.title`               | string                   | what the process achieves                                         |
| `fields.status`              | string                   | `active`, `draft` or `retired`                                    |
| `fields.applies-to`          | list of strings, or null | the service ids the process concerns                              |
| `fields.last-rehearsed`      | object, or string        | the last walk-through end to end, as `at` and `by`, or `never`    |
| `fields.rehearsal-frequency` | string                   | `on-change`, `per-release`, `quarterly` or `annual`               |
| `fields.tags`                | list of strings, or null | the word a reader arrives with                                    |
| `sections.When to use this`  | string of markdown       | the trigger, which says whether the process is yours              |
| `sections.Prerequisites`     | string of markdown       | the access, tools, prior work and skills the first step needs     |
| `path`                       | string                   | where the record sits in its own repository                       |
| `links`                      | object                   | the built link to that record, under `human`                      |

**Read `When to use this` before you decide.** A title says what a process achieves, and two processes can achieve
similar things. The trigger says which one was written for your situation.

**Check `Prerequisites` before anybody starts.** Say what is missing before the first step, because a gap found after
four steps leaves the work half done.

## Read the steps from the published record

The export has the trigger and the prerequisites, which decide whether the process is yours. `Steps`, `Verification`
and any rollback stay in the published record. Load `corpus-retrieval` to fetch it. It reads the record at the commit
the export was taken at, so the steps you get are the ones written beside the trigger you read. Quote `links.human` to
a person. Work only from the steps you fetched. A plausible order written from the trigger looks the same to a reader
as the one their colleagues follow.

## Read the prefix on an id

**A prefix names the corpus that wrote the process.** `eng:prc-release` was published by the corpus with shortcode
`eng`, and a bare `prc-release` was written here. `eng:prc-release` is at
`${CLAUDE_PLUGIN_ROOT}/corpus/processes/eng/prc-release.json`. Look the `shortcode` up in `sources` in `manifest.json`,
and name that corpus in words.

## Say what stayed behind

`types` in `manifest.json` lists what travelled. Read it before you say anything is missing, and send the reader to the
published process for the rest.

* **Every process's Steps, Verification and If it goes wrong** stay in the record.
* **The services in `applies-to`** are here only where `services` travelled. Otherwise report the ids alone.
* **A runbook for something broken** is here only where `runbooks` travelled, and no skill here reads it.

## Say when a process is unsettled

Read the `status` that puts a process in force from `inForce`, a string, `null`, or absent, on the `processes` entry of
`types` in `manifest.json`. The schema can change it, so take it from the export every time. Read `status` and
`last-rehearsed` before you hand anybody a procedure, and tell them what you saw:

* **The `inForce` value**: in force. Answer from it.
* **`status: draft`**: the process was not agreed when the export was taken.
* **`status: retired`**: the process was stood down. Find what replaced it, and quote that.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.
* **`last-rehearsed: never`**: nobody has followed this end to end, so the first person to run it is testing it.
* **`last-rehearsed.at` older than `rehearsal-frequency` allows**: the process has drifted out of its own cadence.
* **`rehearsal-frequency: on-change`**: no date makes this one stale. Ask what has moved since `last-rehearsed.at`,
  and say nothing here answers that.
* **`last-rehearsed.by`**: who followed it last, and the one to ask where a step reads wrong.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. Call no process in force on the strength of its status.
`draft` and `retired` still mean what the bullets above say.

The export reads the same however old it is. Quote `generatedAt` and `commit` from `manifest.json` beside any of these.

## Say when there is nothing

Where no process matches, say that nothing in this export describes a procedure for the task. Name the corpus and every
entry in `sources` from `manifest.json`, so the reader knows what was searched. An empty search states only that the
estate has not written this down: it supplies no order. Offer the task as one worth a process.
