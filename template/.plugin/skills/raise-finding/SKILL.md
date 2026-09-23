---
name: raise-finding
description: File something you noticed about this corpus as an issue on the repository that publishes it. Use when a
  record here is wrong, missing, out of date, or sent you the wrong way, and when someone says "raise a finding",
  "file that" or "report that back". Use it as well, unprompted, whenever a lookup answered badly or answered nothing
  and the estate should have had the answer. The corpus travelling with this plugin is a frozen copy, so an issue is
  the only way anything you noticed reaches the people who can change it.
---

# Filing a finding on the corpus

The corpus beside you is a copy taken on a day. Nothing you write to it survives, and nobody else ever reads it. So an
observation worth keeping leaves as an issue on the repository that published the corpus.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json   # which corpus this is, what it consumes, and where each one publishes
```

Use that path exactly as it appears. It is already absolute. An installed plugin sits in a cache of its own rather than
in the repository you are working in.

**Producing the body needs nothing installed.** Filing it needs whichever client already signs in to the platform. Where
that client is missing, the body is still the valuable part, so this skill prints it rather than giving up.

## Never file one unasked

**Show the finished body and wait for a yes.** Every time, including when somebody asked you to raise a finding. What
you write goes out under their name, onto a repository other people read, and they get to see the words first.

**A silent no is a no.** Where nobody answers, print the body and stop. Do not file it and mention it afterwards.

**One finding, one issue.** Two things you noticed are two findings. A single issue carrying both gets half-closed.

## Check whether the corpus already says it

**Search the fixes before you file anything about a problem.** Seven sessions meeting the same problem should not open
seven issues, and a problem somebody already wrote down is answered in the export beside you.

**Load the `fix-lookup` skill and give it your symptom.** It searches `symptom-keywords`, which is filled with the
words a reader arrives with. Where it returns a fix, follow the resolution and file nothing.

**Where this plugin has no `fix-lookup`, the corpus declined the `fixes` type.** `types` in `manifest.json` says so.
There is nothing to search, and the finding is the only route.

**A fix that is wrong is still a finding.** So is one whose resolution no longer works, and one you could not find
under the words you searched. Say in `## What I saw` which fix you read and which words missed it.

## Decide what is actually a finding

**A finding is something you observed that the corpus does not already say.** A record that contradicts the estate. A
rule that misfired on real work. A question a lookup should have answered and could not. A page that sent you the wrong
way.

Three things are not findings, and each has somewhere better to go.

* **A rule you are about to depart from** is a deviation. It is asked for rather than reported, it goes to the corpus
  owning the clause, and `request-deviation` is the skill for it. A finding says you noticed something. A deviation
  request asks somebody to accept a risk, and the two arrive at different moments.
* **A record you are already changing** is an edit. Make the change.
* **A guess you have not seen happen** is not an observation. Say what you saw, or say nothing.

## Pick the tracker to file on

`manifest.json` states three trackers: `tracker` for this corpus's own records, `framework` for the framework it took,
and one under each entry in `sources` for a corpus this one consumes. Pick the tracker that owns what you noticed.

| Field                 | Type            | What it holds                                                        |
|-----------------------|-----------------|----------------------------------------------------------------------|
| `corpus`              | string          | the name of this corpus, as `example-payments`                       |
| `shortcode`           | string or null  | this corpus's own prefix                                             |
| `commit`              | string          | the commit this export was taken at                                  |
| `tracker`             | object          | where work about this corpus's records is filed                      |
| `tracker.target`      | string          | one of `github`, `azure-devops`, `none`                              |
| `tracker.base`        | string or null  | the repository or project to file against                            |
| `tracker.area`        | string or null  | the area path inside an Azure DevOps project, separated by `/`        |
| `framework`           | object          | where a problem with `kac`, the schema, the template or a skill goes  |
| `sources`             | list of objects | one entry per corpus this one consumes                               |
| `sources[].corpus`    | string          | that corpus's name, for the `corpus:` line of the body               |
| `sources[].shortcode` | string          | the prefix its records carry, as the `eng` in `eng:pol-AGNT.PROV`    |
| `sources[].tracker`   | object          | where work about that corpus's records is filed                      |

`framework` and every `sources[].tracker` state the same three keys `tracker` does.

Three rules decide which one, and they do not overlap.

* **A record whose id carries a shortcode goes to the `sources` entry with that shortcode.** Use its `tracker`.
* **A record carrying no shortcode goes to the top-level `tracker`.**
* **Anything about `kac`, the schema, the template, the plugin or a skill goes to `framework`.** Those arrived from
  somewhere else, and the corpus holding them cannot change them.

File against the wrong one and the issue lands where nobody owns the thing you saw.

**Where `manifest.json` is missing or will not parse, stop and say so.** The plugin is not assembled as it should be,
which is itself worth reporting. Print the body and ask whoever is with you where it belongs.

**A tracker block is always there, and its `base` may be present and `null`.** Test the value rather than the key. A
`base` of `null`, or a `target` of `none`, means nothing here addresses that backlog. Say so, print the body, and ask
whoever is with you where it should go. Do not invent a repository.

**A tracker is not `publishing`.** `publishing.base` is where a record is read, and on Azure DevOps that is a repository
inside a project. The backlog is the project itself, so filing against `publishing.base` files nowhere.

## Write the body

The issue is the observation. It proposes no record, because triage decides where the observation lands.

The body opens with a fenced block a person can read and a later run can copy without rereading the prose. Then three
headings, in this order and nothing between them and the block.

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

**The title is the observation in one line.** Write the sentence somebody scanning a list of issues can act on. "An
observation about Rider" names the subject and says nothing.

Each key of the block, and what to put in it:

* **`corpus`** is the name from the block you picked above.
* **`source`** is `session`.
* **`confidence`** is `unverified`. Write `corroborated` only where something outside your own session confirmed it: a
  second run, a failing check, a colleague who saw it too. Your own account of your own work is not confirmation.
* **`looks-like`** says what the observation resembles, and decides nothing. Write a name from `types[].type` in
  `manifest.json`, which is the list of types this corpus adopted. Write `framework` where what you saw is about the
  framework itself: `kac`, the schema, the plugin or a skill. Write `none` for anything else, and wherever you would
  be guessing. A corpus this one consumes may hold types that list does not show, so guess none of them.
* **`provenance`** names the agent, the session it ran in, the repository you were working in, the commit you were at,
  and what you were doing. Add `manifest.json`'s own `commit` where the finding is about the corpus, because that says
  which export you read. **Name any of those you cannot reach**, rather than leaving it out. A reviewer needs to know
  the difference between a session that had no id and a session that did not say.
* **`applies-to`** and `tags` are optional, and go after `provenance` in that order. Nothing else belongs in the block.

**Keep the three sections short.** What you saw, what you were doing when you saw it, and who it might bite. Three
sentences each is plenty. This is a note somebody triages, not a case to be argued.

## File it

**Use the client that already signs in to the platform.** Never ask for a credential, and never offer to edit the copy
under `${CLAUDE_PLUGIN_ROOT}`.

Write the body to a file first. Passing it inline turns every backtick and quote into a quoting problem, and the block
is full of both.

The section to follow is chosen by the tracker's `target`, every time. Three trackers in one export can name three
platforms, so read it from the block you picked rather than from the one above it.

### GitHub

`target` is `github`, and `base` is the repository, as `https://github.com/<owner>/<repo>`.

```bash
gh issue create --repo <owner>/<repo> --title "<the title>" --body-file <path> --label kac:finding
```

**A repository that holds no `kac:finding` label refuses the whole command.** The error names the label. Run it again
with no `--label`, and say in your reply that you filed it unlabelled and why. Never let a missing label cost the
observation.

**A command that fails on sign-in or on permission is not a failure to report.** `gh auth status` says whether you are
signed in to that host. A repository with issues turned off, or an account without the rights to open one, ends the same
way: print the body and say which of the two it was.

### Azure DevOps

`target` is `azure-devops`, and `base` is the project holding the backlog, as `https://dev.azure.com/<org>/<project>`.
`az` wants the two segments apart: the organisation follows the host, and the project follows the organisation.

**A project name arrives percent-encoded, and `az` wants it spelled out.** A base ending
`Engineering%20Standards` names a project `az` takes as `Engineering Standards`. Decode every escape before you pass it.

```bash
az boards work-item create --org https://dev.azure.com/<org> --project <project> --type Issue --title "<the title>" --fields "System.Description=@<path>" "System.Tags=kac:finding"
```

**A tag is what this platform calls a label**, so `System.Tags` is where the mark goes. Azure creates a tag it does not
already hold rather than refusing, so nothing is lost here. Where the command fails for any other reason, print the body
and say so.

#### The area path

**`area` says which area path inside the project the item belongs to.** One project gives one backlog to any number of
corpora, so the area is what separates this corpus's work from the rest.

Where the block states one, add `System.AreaPath` to `--fields` as the project and the area joined by a backslash. Azure
Boards separates an area path with backslashes, and `area` arrives separated by forward slashes, so convert every one:
an `area` of `kac-it-swdev/subteam` goes in as `<project>\kac-it-swdev\subteam`.

```bash
az boards work-item create --org https://dev.azure.com/<org> --project <project> --type Issue --title "<the title>" --fields "System.Description=@<path>" "System.Tags=kac:finding" "System.AreaPath=<project>\<area>"
```

**Where `area` is null, leave the field out.** The command is then the one above it, and the item lands in the project's
default area.

**An area path that does not exist, or one you have no rights to file into, fails the whole command.** Run it again
with `System.AreaPath` left off, and say in your reply that it landed in the project's default area and why. Never let
a wrong area cost the observation.

### Where the tracker files nowhere

`target` of `none` names a corpus with no backlog this export can address. There is nowhere to file. Print the whole
body, name the corpus it belongs to, and ask whoever is with you where it should go.

### Where no client is here

Print the whole body, name the repository it belongs on, and say plainly that you could not file it. Ask whoever is with
you to paste it. A finding read out to somebody is worth more than one lost to a missing tool.

## Say what you did

Close by naming the issue you opened and its URL, or the repository the body still needs pasting on. Where you filed it
unlabelled, say so. Where you left something out of `provenance`, say which.

**Say that it is untriaged.** A repository may route a new issue onto a board, into a milestone, or past somebody who
decides what happens to it, and none of that is visible from here. A finding nobody routes is a finding nobody reads,
so hand the person you are with the URL and let them place it.
