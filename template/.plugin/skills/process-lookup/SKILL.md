---
name: process-lookup
description: Find the procedure for a planned task, in the processes that travel with this plugin. Use when someone
  asks how a task is carried out here — "how do we release", "what is the process for onboarding", "is there a
  procedure for rotating a secret", "what do I do to provision this". Use it as well, unprompted, before you carry out
  a planned task somebody here may already have written down. Read the process before you invent an order of your own.
---

# Finding a process to follow

The corpus travels with this plugin as data. You read it with the tools you already have.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                       # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/processes/<record>.json             # one process this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/processes/<shortcode>/<record>.json # one process a corpus this one consumes wrote
```

Use those paths exactly as they appear above; they are already absolute. An installed plugin sits in a cache of its own
rather than in the repository you are working in.

## Find one process, and follow that one

**Processes do not compose.** A rule binds alongside every other rule that reaches your work, and a process does not.
You are looking for the single procedure written for the task in front of you, and following two is following neither.

**Are you doing this because you planned to, or because something is broken?** Planned is a process. Broken is a
runbook, and a runbook is a different type that may not have travelled at all.

## Search the trigger, not the title

**There is no flat file here.** A process is followed whole, so the record is the unit and each one is its own file.
Point your Grep tool at `${CLAUDE_PLUGIN_ROOT}/corpus/processes/`, ask for matching content, and search
case-insensitively.

What travels is what decides whether a process is yours:

| Key                                     | What it holds                                                          |
|-----------------------------------------|------------------------------------------------------------------------|
| `fields.title`                          | what the process achieves                                              |
| `sections.When to use this`             | the trigger. This is the field that answers whether it is yours        |
| `sections.Prerequisites`                | the access, the tools and the prior work needed before the first step  |
| `fields.applies-to`                     | the service ids the process concerns                                   |
| `fields.status`                         | `active`, `draft` or `retired`                                         |
| `fields.last-rehearsed`                 | a date, or `never`                                                     |
| `fields.rehearsal-frequency`            | the cadence that date is read against                                  |
| `fields.tags`, `fields.id`              | the word a reader arrives with, and the address to cite                |
| `path`, `links`                         | where the full record lives, and the built link to it                  |

**Read `When to use this` before you decide.** A title says what a process achieves and two processes can achieve
similar things. The trigger says which one was written for the situation you are in.

## Check the reader can start

**`Prerequisites` travels whole for a reason.** It names the access, the tooling and the prior process a reader needs
before step 1. Where one of those is missing, say so before anybody begins, rather than after four steps have run.

## The steps are not here

**This export carries the trigger and stops.** `Steps`, `Verification` and any rollback stay in the published record,
because a procedure is followed in order against the version in force rather than against a copy taken on some earlier
day.

**Load the `corpus-retrieval` skill and fetch the record.** It says which publishing block addresses this corpus, how
to fetch the file through the client that authenticates to that platform, and what to do where nothing can reach it.
The record's own `links.human` is the URL to quote to a person.

**Do not reconstruct the steps.** Neither from the trigger, nor from what the task usually involves elsewhere. A
plausible procedure is worse than none, because whoever asked cannot tell it from the one their colleagues follow.

## Read the prefix on an id

**A prefix names the corpus that wrote the process.** `eng:prc-release` was published by the corpus whose shortcode is
`eng`, and a bare `prc-release` was written here. The shortcode moves out of the id and becomes the directory, so
`eng:prc-release` is at `${CLAUDE_PLUGIN_ROOT}/corpus/processes/eng/prc-release.json`.

**`shortcode` is the key into `sources`** in `manifest.json`, which names the producing corpus and where it publishes.
Look it up before you say anything about where a process came from, and name the corpus in words.

## Say when a process is unsettled

Read these before you hand anybody a procedure, and tell them what you saw:

* **`last-rehearsed: never`** — nobody has followed this end to end. It is a hypothesis, and the first person to run it
  is testing it.
* **`last-rehearsed` older than `rehearsal-frequency` allows** — the procedure has drifted out of its own cadence.
* **`status: draft`** — it was not agreed when the export was taken.
* **`status: retired`** — it has been stood down. Find what replaced it before you follow it.

## Say when there is nothing

Where no process matches, say nothing in this export describes a procedure for the task, and name the corpus and every
entry in `sources` from `manifest.json`, so a reader knows what was searched.

**Silence is not a procedure you may supply.** It says only that nobody here has written this down. Offer the task as
one worth a process, and leave that to whoever owns it.
