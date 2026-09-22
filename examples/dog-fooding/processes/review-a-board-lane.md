---
id: prc-review-a-board-lane
type: process
tier: procedural
status: active
last-rehearsed: "2026-09-22"
rehearsal-frequency: on-change
owner: human:paul.law
tags: [ issues, tracker ]
---

# Review and tidy a lane of the project board

`Process: prc-review-a-board-lane` `ACTIVE`

Read every open issue in one lane of the project board against the code, and leave the lane ordered and honest.

## When to use this

A lane has taken issues for a while and nobody has read it end to end. Work here raises issues as it goes, so a lane
drifts in two directions: the code answers a question and the issue stays open, and a body states a number the
repository has since changed.

Read the lane before you plan work in it. An issue nobody has re-read is a plan built on what was true months ago.

This process writes to the tracker and changes no file in the repository, so it ends without [prc-pull-request].

## Prerequisites

* The `gh` CLI, authenticated against the repository that publishes this corpus. The agent configuration under
  `.claude/agents-config/` names it and says that issues live as GitHub issues.
* Project 3, and its `Status`, `Phase` and `Rank` fields. `Phase` is the lane.
* A checkout of this repository, because step 3 reads the code rather than the issue.
* Writing prose to a style guide, which steps 5 to 7 need.

## Steps

1. List the lane with each issue's body, status and labels:
   `gh project item-list 3 --owner paul80nd --format json --limit 400`, filtered on the `Phase` value.
2. Count the open issues and the closed ones before you read any of them. A lane that is mostly closed is one to
   finish rather than one to plan.
3. Check every factual claim in each open body against the code. Read the schema file, run the grep, count the
   records. A body states what was true the day it was written.
4. Decide one of three for each issue: the code has answered it, its facts have moved, or it stands as written.
5. Close what the code has answered. Comment the evidence first, naming the file, the field and the count that settle
   it. Closing an issue moves its board item to `Done` on its own.
6. Rewrite a body whose facts have moved. Keep the structure, rewrite the block that changed rather than appending a
   correction, and end with a `## What changed` section saying what was true when it opened.
7. Comment where one part moved and the rest stands. State what you read and the date you read it.
8. Search every other lane for issues on this one's subject, and move those in. An issue amending one in this lane
   from somewhere else is invisible to whoever picks that one up.
9. Move out an issue this lane holds that belongs to another, and say where it went.
10. Write the `Rank` field for every item in the lane: the open issues first, in the order you would do them, then the
    closed ones. `Rank` is what the board sorts on. An issue you moved in at step 8 brought its old lane's number, so
    two items share one until you rewrite both.
11. Put any new issue to the developer and wait for their answer. Filing one unasked is the one thing this process
    must never do.
12. Optional: write an umbrella issue where the lane has none. Label it `Epic`, rank it 1, and add every other issue
    in the lane as a sub-issue of it. The board then reports the lane's progress as a percentage. A pull request
    cannot be a sub-issue, so leave one out and say so.
13. Optional: archive the closed items where the lane's own work is finished. The lane empties, which is what a
    finished phase looks like here. Ask first: an archived item leaves the board for everybody.
14. Report what you closed, what you rewrote, what moved in and what moved out.

## Verification

The lane lists its open issues in `Rank` order, no two share a number, and every claim left in an open body matches
something you read in the repository.

Close by stating what you closed and on what evidence, which bodies you rewrote, what moved in and out, and anything
you left open because the decision is the developer's.

## If it goes wrong

Nothing here destroys work. `gh issue reopen` undoes a close, an archived item returns from the project's Archived
view, and a rewritten body is in that issue's own edit history.

Renaming a lane, or adding one, is the exception. That mutation replaces the whole `Phase` option set, so an option
whose `id` you leave out takes every item's `Phase` with it. Read the current options first, pass every `id` back, and
count the items carrying a `Phase` before and after.

## Related

* [std-PROSE.what-stays-true] sends agreed but unbuilt work to the issue tracker. This process is how that tracker
  stays worth reading.
* [prc-pull-request] is not run here, because this process changes no file.
* `harvest-findings` triages the `kac:finding` issues and drafts the record one asks for. This process reads a whole
  lane, whoever filed it.

[prc-pull-request]: pull-request.md
[std-PROSE.what-stays-true]: ../standards/prose.md#what-stays-true
