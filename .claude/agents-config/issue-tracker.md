# Issue tracker: GitHub

Issues and specs for this repo live as GitHub issues. Use the `gh` CLI for all operations.

## Conventions

- **Create an issue**: `gh issue create --title "..." --body "..."`. Use a heredoc for multi-line bodies.
- **Read an issue**: `gh issue view <number> --comments`, filtering comments by `jq` and also fetching labels.
- **List issues**: `gh issue list --state open --json number,title,body,labels,comments --jq '[.[] | {number, title, body, labels: [.labels[].name], comments: [.comments[].body]}]'` with appropriate `--label` and `--state` filters.
- **Comment on an issue**: `gh issue comment <number> --body "..."`
- **Apply / remove labels**: `gh issue edit <number> --add-label "..."` / `--remove-label "..."`
- **Close**: `gh issue close <number> --comment "..."`

Infer the repo from `git remote -v` — `gh` does this automatically when run inside a clone.

## Pull requests as a triage surface

**PRs as a request surface: no.** _(Set to `yes` if this repo treats external PRs as feature requests; `/triage` reads this flag.)_

When set to `yes`, PRs run through the same labels and states as issues, using the `gh pr` equivalents:

- **Read a PR**: `gh pr view <number> --comments` and `gh pr diff <number>` for the diff.
- **List external PRs for triage**: `gh pr list --state open --json number,title,body,labels,author,authorAssociation,comments` then keep only `authorAssociation` of `CONTRIBUTOR`, `FIRST_TIME_CONTRIBUTOR`, or `NONE` (drop `OWNER`/`MEMBER`/`COLLABORATOR`).
- **Comment / label / close**: `gh pr comment`, `gh pr edit --add-label`/`--remove-label`, `gh pr close`.

GitHub shares one number space across issues and PRs, so a bare `#42` may be either — resolve with `gh pr view 42` and fall back to `gh issue view 42`.

## When a skill says "publish to the issue tracker"

Create a GitHub issue.

## When a skill says "fetch the relevant ticket"

Run `gh issue view <number> --comments`.

## Project board

The board is project 3, owned by `paul80nd`, with node id `PVT_kwHOATpixc4BfwuW`. These ids are stable. If a write
fails on one, refresh them with `gh project field-list 3 --owner paul80nd --format json`.

| Field    | Field id                         | Value              |
|----------|----------------------------------|--------------------|
| `Status` | `PVTSSF_lAHOATpixc4BfwuWzhaBT6A` | an option id below |
| `Phase`  | `PVTSSF_lAHOATpixc4BfwuWzhaBT_4` | an option id below |
| `Rank`   | `PVTF_lAHOATpixc4BfwuWzhaBT_8`   | a number           |

| `Status` option | Id         |
|-----------------|------------|
| Todo            | `f75ad846` |
| In Progress     | `47fc9ee4` |
| Done            | `98236657` |

| `Phase` option                        | Id         |
|---------------------------------------|------------|
| Intake                                | `808e66e2` |
| 1a Pre-trial - Breaking changes       | `05c1e3d1` |
| 1b Pre-trial - Know what we have      | `4101f470` |
| 1c Pre-trial - Adopter path           | `9a02e71f` |
| 2a Trial - A maintainer and an agent  | `29312778` |
| 2b Trial - The adopting team          | `caff5545` |
| 3a Route to v1 - Shape changes        | `71c6738d` |
| 3b Route to v1 - Clean-up             | `5ab11997` |
| 3c Route to v1 - Open to contributors | `36214a5a` |
| 4 Future functionality                | `a890041a` |

To place issue `<n>` on the board:

1. Read its node id and any board item it already has. A new sub-issue of an epic is added automatically, and
   `item-add` then fails with `Content already exists`.

   ```sh
   gh api graphql -f query='{ repository(owner:"paul80nd",name:"knowledge-as-code") { issue(number:<n>) { id projectItems(first:5) { nodes { id project { number } } } } } }'
   ```

2. Where it has no item on project 3, add one. The output is the item id.

   ```sh
   gh project item-add 3 --owner paul80nd --url https://github.com/paul80nd/knowledge-as-code/issues/<n> --format json --jq .id
   ```

3. Set each field, one call per field:

   ```sh
   gh project item-edit --project-id PVT_kwHOATpixc4BfwuW --id <item-id> --field-id <field-id> --single-select-option-id <option-id>
   gh project item-edit --project-id PVT_kwHOATpixc4BfwuW --id <item-id> --field-id PVTF_lAHOATpixc4BfwuWzhaBT_8 --number <rank>
   ```

4. In a stage lane, make it a sub-issue of that lane's epic. Both ids are issue node ids from step 1.

   ```sh
   gh api graphql -f query='mutation { addSubIssue(input: { issueId:"<epic-node-id>", subIssueId:"<node-id>", replaceParent:true }) { subIssue { number } } }'
   ```

5. Confirm the write with this query. `gh project item-list` can omit an item that is there.

   ```sh
   gh api graphql -f query='{ repository(owner:"paul80nd",name:"knowledge-as-code") { issue(number:<n>) { parent { number } projectItems(first:5) { nodes { project { number } fieldValues(first:20) { nodes { ... on ProjectV2ItemFieldSingleSelectValue { name } ... on ProjectV2ItemFieldNumberValue { number } } } } } } } }'
   ```

## Wayfinding operations

Used by `/wayfinder`. The **map** is a single issue with **child** issues as tickets.

- **Map**: a single issue holding the Notes / Decisions-so-far / Fog body. `gh issue create`, with no label.
- **Child ticket**: an issue linked to the map as a GitHub sub-issue (`gh api` on the sub-issues endpoint). Where
  sub-issues are not enabled, add the child to a task list in the map body and put `Part of #<map>` at the top of the
  child body. Children take no label. Once claimed, the ticket is assigned to the driving dev.
- **Blocking**: GitHub's **native issue dependencies** — the canonical, UI-visible representation. Add an edge with `gh api --method POST repos/<owner>/<repo>/issues/<child>/dependencies/blocked_by -F issue_id=<blocker-db-id>`, where `<blocker-db-id>` is the blocker's numeric **database id** (`gh api repos/<owner>/<repo>/issues/<n> --jq .id`, _not_ the `#number` or `node_id`). GitHub reports `issue_dependencies_summary.blocked_by` (open blockers only — the live gate). Where dependencies aren't available, fall back to a `Blocked by: #<n>, #<n>` line at the top of the child body. A ticket is unblocked when every blocker is closed.
- **Frontier query**: list the map's open children (`gh issue list --state open`, scoped to the map's sub-issues / task list), drop any with an open blocker (`issue_dependencies_summary.blocked_by > 0`, or an open issue in the `Blocked by` line) or an assignee; first in map order wins.
- **Claim**: `gh issue edit <n> --add-assignee @me` — the session's first write.
- **Resolve**: `gh issue comment <n> --body "<answer>"`, then `gh issue close <n>`, then append a context pointer (gist + link) to the map's Decisions-so-far.
