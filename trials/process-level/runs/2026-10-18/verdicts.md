# Process verdicts, 2026-10-18

Each process in `examples/dog-fooding/processes/` and each `i-want-to` playbook, read against `## Level of detail and
cadence` on `processes.md` for #934. The Words column gives the words in the steps, then the words in the whole file.
An outcome is a step at the process level. A step below it is a command, a file list, a skill to load, or a mechanism
that another page states.

A planned task never sends its detail to a runbook here, because `runbooks.md` scopes a runbook to something broken.
The detail goes to `.schema/CLAUDE.md` or `tooling/CLAUDE.md` where it warns an agent, and to `docs/maintaining/` where
a contributor needs it.

| Record                    | Steps | Words      | Outcomes | Below the level        | Findings                                       | Verdict                      | Reason                                                                                                |
|---------------------------|-------|------------|----------|------------------------|------------------------------------------------|------------------------------|-------------------------------------------------------------------------------------------------------|
| `prc-add-a-record`        | 11    | 288 / 539  | 7        | 3, 4, 7, 10            | none                                           | Keep, and trim               | Skill loading, `kac checks`, key order and the commands are the detail.                               |
| `prc-add-a-type`          | 25    | 584 / 903  | 10       | 9 to 18, 20 to 24      | #647, #672                                     | Rewrite                      | Steps 9 to 18 wire a type into this repository, which `docs/maintaining/` can state.                  |
| `prc-change-a-skill`      | 15    | 756 / 1233 | 8        | 1, 3, 5, 6, 10, 12, 14 | #585, #727, #804, #902                         | Rewrite                      | Steps 8 to 10 and 13 repeat `prc-pull-request`, and its file lists suit `docs/maintaining/plugin.md`. |
| `prc-change-the-schema`   | 12    | 461 / 778  | 5        | 4, 5, 6, 8, 9, 10, 11  | #501, #672, #728, #735, #776, #780, #786, #804 | Rewrite, as the pilot        | It drew the most findings, and its detail already had a home.                                         |
| `prc-change-the-tool`     | 14    | 347 / 621  | 8        | 3, 5, 7, 8, 11, 13     | #728, #793, #804                               | Rewrite                      | Its detail repeats `docs/maintaining/checks.md`, and its steps cover adding a check alone.            |
| `prc-pull-request`        | 13    | 407 / 805  | 9        | 6, 7, 10, 11           | #582, #585, #594, #622                         | Rewrite after the pilot      | It restates `std-CI` and `std-VERS`, and two other pages state its obligations.                       |
| `prc-review-a-board-lane` | 15    | 506 / 945  | 14       | 1                      | none                                           | Keep                         | Every step but the `gh` command is an outcome.                                                        |
| `prc-sweep-prose`         | 10    | 257 / 556  | 8        | 2, 8                   | none                                           | Keep                         | Only the prompt's version check and the overlay copy are detail.                                      |
| `prc-write-public-docs`   | 9     | 190 / 501  | 5        | 3, 4, 7, 8             | none                                           | Keep, and move steps 3 and 4 | Steps 3 and 4 are writing rules, which `writing-the-docs` states.                                     |
| `i-want-to`               | none  | 626        | none     | none                   | #585, #647, #780                               | Keep as the router           | Its copy-the-steps rule fits outcome steps, and #647 still lacks a row.                               |
| `reviewing-records`       | 10    | 387 / 1249 | 7        | 2, 4, 9                | none                                           | Keep as a playbook           | Most of it is judgement a skill states, and its steps depend on that judgement.                       |

Process records and playbooks stay apart. A process states the ordered outcomes of a planned task, and
`process-lookup` reads it from the export. A playbook states the judgement a step needs, and `i-want-to` loads it into
the turn beside the process it routes to.
