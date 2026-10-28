---
name: raise-finding
description: File something you noticed about this corpus as an issue on the tracker that corpus states. Use when a
  record here is wrong, missing or out of date, or a lookup answered badly or not at all, and when someone asks you to
  raise a finding or report it back.
---

# Filing a finding on the corpus

A finding is an observation about the corpus, sent to the people who can change it. The corpus beside you is a copy
taken on a day, and nothing written to it reaches anybody, so an observation worth keeping leaves as an issue on the
repository that published the corpus. Triage decides what the observation becomes, so the issue states what you saw
and proposes no record.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json   # which corpus this is, what it consumes, and where each one publishes
```

Use that path exactly as written. An installed plugin sits in a cache outside the repository you are working in.

Writing the body needs nothing installed. Filing it runs `gh` on GitHub or `az` on Azure DevOps. Where neither can run,
print the whole body, because the body is the valuable part.

The sections below run in order. Each one decides whether the next is needed.

## Ask before you file

**Show the finished body and wait for a yes**, every time, including when somebody asked you to raise a finding. It
goes out under their name, onto a repository other people read, so they see the words first. Where nobody answers,
print the body and stop.

**File one finding per issue.** Two observations in one issue get half-closed.

## Check whether the corpus already says it

**Search the fixes before you file anything about a problem**, because a problem somebody already wrote down is
answered in the export beside you. Load `fix-lookup` and give it your symptom. Where it returns a fix that resolves
the problem, follow it and file nothing.

Where this plugin has no `fix-lookup`, the corpus declined the `fixes` type, and `types` in `manifest.json` says so.
The finding is then the only route.

A fix that is wrong is still a finding, and so is one whose resolution failed, or one your words did not reach. Say in
`## What I saw` which fix you read and which words missed it.

## Decide that it is a finding

A finding is something you observed that the corpus does not already say: a record that contradicts the estate, a rule
that misfired on real work, a question a lookup should have answered, a page that sent you the wrong way.

Three other things each have a better route:

* **A rule you are about to depart from** is a deviation, and `request-deviation` asks for it. A deviation asks
  somebody to accept a risk before the work merges. A finding reports what you saw afterwards.
* **A record you are already changing** is an edit. Make the change.
* **A guess you have not seen happen** is not an observation. Report what you saw.

## Pick the tracker

`manifest.json` states a tracker for this corpus's own records under `tracker`, and one for each corpus it consumes
under `sources`. Pick the one that owns what you noticed, because an issue on the wrong tracker lands where nobody owns
the thing you saw.

| Field                 | Type            | What it holds                                                       |
|-----------------------|-----------------|---------------------------------------------------------------------|
| `corpus`              | string          | the name of this corpus, as `example-payments`                      |
| `shortcode`           | string or null  | this corpus's own prefix                                            |
| `commit`              | string          | the commit this export was taken at                                 |
| `tracker`             | object          | where work about this corpus's records is filed                     |
| `tracker.target`      | string          | one of `github`, `azure-devops`, `none`                             |
| `tracker.base`        | string or null  | the repository or project to file against                           |
| `tracker.area`        | string or null  | the area path inside an Azure DevOps project, separated by `/`      |
| `sources`             | list of objects | one entry per corpus this one consumes                              |
| `sources[].corpus`    | string          | that corpus's name, for the `corpus:` line of the body              |
| `sources[].shortcode` | string          | the prefix its records carry, as the `eng` in `eng:pol-AGNT.PROV`   |
| `sources[].tracker`   | object          | where work about that corpus's records is filed                     |

Every `sources[].tracker` states the same keys `tracker` does. A tracker block is always there, and its `base` may be
present and `null`. Test the value rather than the key.

* **A record whose id has a shortcode** goes to the `sources` entry with that shortcode, and its `tracker`.
* **A record with no shortcode** goes to the top-level `tracker`. So does anything about the plugin, a skill or the
  export itself.
* **A finding about the framework** goes to the top-level `tracker` too. A framework finding contains your session id,
  your repository and its commit, and the maintainer of `kac` may be a stranger. Triage sends a stripped copy upstream
  by hand, and `looks-like: framework` marks it for that. The `framework` block in `manifest.json` is for triage.
* **A `base` of `null`, or a `target` of `none`,** means nothing here addresses that backlog. Say so, print the body,
  and ask whoever is with you where it goes.
* **A missing or unreadable `manifest.json`** means the plugin is not assembled as it should be. Say so, print the
  body, and ask where it belongs.

Read the address from `tracker`, and leave `publishing` alone. `publishing.base` is where a record is read. On Azure
DevOps that is a repository inside a project, and the backlog is the project, so a finding filed there files nowhere.

## Write the body

The body opens with a fenced block a person can read and a later run can copy without rereading the prose. Then three
headings, in this order, with nothing between the block and the first of them.

`````
Title: Rider does not re-read an .editorconfig changed from a shell

```yaml kac-finding
corpus: example-dogfooding
source: session
confidence: unverified
looks-like: fixes
provenance: >
  Claude Code, in session 01J8ZC4M6QK2XR7VN0PYWTB3AE, in paul80nd/knowledge-as-code at 24dcea21,
  editing .editorconfig from a shell while the developer had the repository open in Rider.
tags: [ editorconfig, formatting, rider ]
```

## What I saw

A key edited in `.editorconfig` from the terminal changed nothing about how Rider formatted a file. Opening
`.editorconfig` in the IDE made the same change take effect. Flipping a key with an obvious effect was the
control.

## Context

Seen once, on one machine. The Rider version was not recorded.

## Why it might matter

A session changes `.editorconfig`, sees no difference in the IDE, and concludes the edit did nothing. The
next move is usually to change something else that was already right.
`````

**Write the title as the observation in one line**, so somebody scanning a list of issues can act on it. "An
observation about Rider" names the subject and says nothing.

Each key of the block, in this order:

* **`corpus`** is the name from the tracker block you picked.
* **`source`** is `session`.
* **`confidence`** is `unverified`. Write `corroborated` only where something outside your session confirmed it: a
  second run, a failing check, a colleague who saw it too. Your own account of your own work is not confirmation.
* **`looks-like`** says what the observation resembles, and decides nothing. Write a name from `types[].type` in
  `manifest.json`, the types this corpus adopted. Write `framework` for `kac`, the schema, the plugin or a skill.
  Write `none` for anything else, and wherever you would be guessing, because a consumed corpus may hold types that
  list does not show.
* **`provenance`** names the agent, the session, the repository you worked in, its commit, and what you were doing.
  Add `manifest.json`'s own `commit` where the finding is about the corpus, because that says which export you read.
  Name any of those you cannot reach, so a reviewer can tell a session with no id from one that did not say.
* **`applies-to`** and `tags` are optional, and go after `provenance` in that order. The block takes no other key.

**Keep the three sections short**: what you saw, what you were doing, and who it might bite. Three sentences each is
plenty, because somebody triages the note.

## File it

Use the client that already signs in to the platform, with the credential it already has. Ask nobody for a credential.
Leave the copy under `${CLAUDE_PLUGIN_ROOT}` as it is, and offer no edit to it, because a change there reaches nobody.

Write the body to a file first, because passing it inline turns every backtick and quote in the block into a quoting
problem.

**Read the file for the `target` of the tracker you picked, and no other.** Each holds one platform's command, and
three trackers in one export can name three platforms.

```text
${CLAUDE_PLUGIN_ROOT}/skills/raise-finding/github.md        # target github: gh issue create, and the label
${CLAUDE_PLUGIN_ROOT}/skills/raise-finding/azure-devops.md  # target azure-devops: az rest, and the area path
```

Where the file for that target is missing, the plugin is not assembled as it should be. Say so, and print the body.

### Where the tracker files nowhere

A `target` of `none` names a corpus with no backlog this export can address. Print the whole body, name the corpus it
belongs to, and ask whoever is with you where it goes.

### Where neither `gh` nor `az` can run

This covers a client that is not installed, and a session that cannot run a command at all. Print the whole body, name
the repository it belongs on, and say that you could not file it. Ask whoever is with you to paste it, because a
finding read out to somebody still reaches the people who can act on it.

## Say what you did

Name the issue you opened and its URL, or the repository the body still needs pasting on. Say where you filed it
unlabelled or in the project's default area, and which parts of `provenance` you could not reach.

**Say that it is untriaged.** A repository may route a new issue onto a board or past somebody who decides what
happens to it, and none of that is visible from here. Hand the person you are with the URL, so they can place it.
