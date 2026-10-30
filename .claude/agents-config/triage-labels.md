# Triage Labels

The skills speak in terms of five canonical triage roles. This file maps each role to a label the tracker has, and
says where a role has none.

| Label in mattpocock/skills | Label in our tracker                 | Meaning                                  |
|----------------------------|--------------------------------------|------------------------------------------|
| `needs-triage`             | `kac:finding`, without `kac:triaged` | Maintainer needs to evaluate this issue  |
| `needs-info`               | `question`                           | Waiting on reporter for more information |
| `ready-for-agent`          | none                                 | Fully specified, ready for an AFK agent  |
| `ready-for-human`          | none                                 | Requires human implementation            |
| `wontfix`                  | `wontfix`                            | Will not be actioned                     |

When a skill mentions a role (e.g. "apply the AFK-ready triage label"), use the corresponding label string from this
table. Where the row says none, apply no label. A story's readiness here is its `Phase` and `Status` on the project
board, which [`issue-tracker.md`](issue-tracker.md#project-board) covers.

`gh` refuses a label the tracker does not have. Check with `gh label list --repo paul80nd/knowledge-as-code` before
adding a row.
