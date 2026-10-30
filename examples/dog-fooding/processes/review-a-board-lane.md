---
id: prc-review-a-board-lane
type: process
tier: procedural
status: active
last-rehearsed: { at: "2026-09-22", by: claude-code/opus-5 }
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
* Project 3, and its `Status`, `Phase` and `Rank` fields. `Phase` is the lane. A stage lane is one whose name starts
  with a stage number and a letter, such as `3b`. Each stage lane has one epic. `Intake` and `4 Future functionality`
  have none.
* A checkout of this repository, because step 4 reads the code rather than the issue.
* Writing prose to a style guide, which steps 6 to 8 need.

## Steps

1. Save this GraphQL query of project 3's items as `lane.graphql`. It reads each item's `Status`, `Phase` and `Rank`,
   and its issue's number, title, state, labels and body. Do not list the lane with `gh project item-list`, because it
   can leave out an item that is on the board.

   ```graphql
   query($endCursor: String) {
     user(login: "paul80nd") {
       projectV2(number: 3) {
         items(first: 100, after: $endCursor) {
           pageInfo { hasNextPage endCursor }
           nodes {
             phase: fieldValueByName(name: "Phase") { ... on ProjectV2ItemFieldSingleSelectValue { name } }
             status: fieldValueByName(name: "Status") { ... on ProjectV2ItemFieldSingleSelectValue { name } }
             rank: fieldValueByName(name: "Rank") { ... on ProjectV2ItemFieldNumberValue { number } }
             content { ... on Issue { number title state body labels(first: 20) { nodes { name } } } }
           }
         }
       }
     }
   }
   ```

2. List the lane by running the query with the lane's `Phase` value in place of `<Phase>`. `--paginate` pages to the
   last item.

   ```sh
   gh api graphql --paginate -F query=@lane.graphql \
     --jq '.data.user.projectV2.items.nodes[] | select(.phase.name == "<Phase>")'
   ```

   The output lists every item in the lane, one JSON object each.
3. Count the open issues and the closed ones before you read any of them. A lane that is mostly closed is one to
   finish rather than one to plan.
4. Check every factual claim in each open body against the code. Read the schema file, run the grep, count the
   records. A body states what was true the day it was written.
5. Decide one of three for each issue: the code has answered it, its facts have moved, or it stands as written.
6. Close what the code has answered. Comment the evidence first, naming the file, the field and the count that settle
   it. Closing an issue moves its board item to `Done` on its own.
7. Rewrite a body whose facts have moved. Keep the structure, rewrite the block that changed rather than appending a
   correction, and end with a `## What changed` section saying what was true when it opened. Retitle the issue where
   its title states a count the rewrite changed.
8. Comment where one part moved and the rest stands. State what you read and the date you read it.
9. Search every other lane for issues on this one's subject, and move those in. An issue amending one in this lane
   from somewhere else is invisible to whoever picks that one up.
10. Move out an issue this lane holds that belongs to another, and say where it went.
11. Search the lane and the tracker for two issues reporting one thing. Where you find a pair, merge the facts the
    newer one adds into the older body, then close the newer one with the `duplicate` label and a comment naming the
    older. A count the newer one states is the more recent reading, so carry it across.
12. Write the `Rank` field for every item in the lane: the open issues first, in the order you would do them, then the
    closed ones. `Rank` is what the board sorts on. An issue you moved in at step 9 brought its old lane's number, so
    two items share one until you rewrite both.
13. Put any new issue to the developer and wait for their answer. Filing one unasked is the one thing this process
    must never do.
14. In a stage lane, check that the issue at `Rank` 1 carries the `Epic` label. A story at `Rank` 1 is easily taken
    for the epic, and the lane loses its epic the day a pull request closes that story.
15. If the issue at `Rank` 1 is a story, give the lane's epic `Rank` 1 instead.
16. If a stage lane has no epic, write one with the `Epic` label and give it `Rank` 1.
17. In a stage lane, add each issue that is not yet a sub-issue of the epic as one. The board then reports the lane's
    progress as a percentage. A pull request cannot be a sub-issue, so leave one out and say so.
18. Optional: archive the closed items where the lane's own work is finished. The lane empties, which is what a
    finished phase looks like here. Ask first: an archived item leaves the board for everybody.
19. Report what you closed, what you rewrote, what moved in and what moved out.

## Verification

The lane lists its open issues in `Rank` order, no two share a number, no two report the same thing, and every claim
left in an open body matches something you read in the repository. In a stage lane, the issue at `Rank` 1 carries the
`Epic` label.

Close by stating what you closed and on what evidence, which bodies you rewrote, what moved in and out, and anything
you left open because the decision is the developer's.

## If it goes wrong

Nothing here destroys work. `gh issue reopen` undoes a close, an archived item returns from the project's Archived
view, and a rewritten body is in that issue's own edit history.

Renaming a lane, or adding one, is the exception. That mutation replaces the whole `Phase` option set, so an option
whose `id` you leave out takes every item's `Phase` with it. Read the current options first, pass every `id` back, and
count the items carrying a `Phase` before and after.

## Related

* [std-PROSE.TRUE] sends agreed but unbuilt work to the issue tracker. This process is how that tracker
  stays worth reading.
* [prc-pull-request] is not run here, because this process changes no file.
* `harvest-findings` triages the `kac:finding` issues and drafts the record one asks for. This process reads a whole
  lane, whoever filed it.

[prc-pull-request]: pull-request.md
[std-PROSE.TRUE]: ../standards/prose.md#true-what-stays-true
