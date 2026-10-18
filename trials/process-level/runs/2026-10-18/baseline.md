# Baseline, 2026-10-18

This run reads the tracker from the commit that wrote `prc-change-the-schema`, `48b0807d` on 2026-09-07, to `4afddc49`,
the last commit on `main` before the pilot. In that window 102 pull requests on `main` touched `.schema/`:

```sh
git log --first-parent 48b0807d..4afddc49 --oneline -- .schema/ | wc -l
```

[`issues.json`](issues.json) is the output of the README's step 2, with `created:<=2026-10-18` as its bound. Of the
issues it lists, the ten below report what the process said or left out. #501 and #502 were filed before `raise-finding`
existed, so they have no `kac:finding` label.

## Findings

Each verdict is step 5 of `harvest-findings`, read against the pilot's level as the trial README defines it. The old
step is the step number in the text before the pilot. Eight of the ten asked for a step. #672 asked for a `kac` verb,
and the comment on #934 asked for a route between two processes.

| Finding      | Opened     | Asked for                                                       | Old step | Asks for a step | Verdict    | Answered by                                                    |
|--------------|------------|-----------------------------------------------------------------|----------|-----------------|------------|----------------------------------------------------------------|
| #501         | 2026-09-07 | the number of trees holding a `_template.md`                    | 4        | yes             | Narrowed   | step 3 states no count, and counts the trees from the files    |
| #502         | 2026-09-07 | the line numbers a universal field moves in the behaviour specs | none     | yes             | Redirected | `tooling/CLAUDE.md` § The feature specs pin more than findings |
| #672         | 2026-09-17 | a `kac` verb to rename a type                                   | none     | no              | Redirected | a change to `kac`, outside the pilot                           |
| #728         | 2026-09-20 | the guard a date-measured rule makes blind                      | none     | yes             | Redirected | `tooling/CLAUDE.md` § The fixtures                             |
| #735         | 2026-09-21 | the number of trees holding a `_template.md`                    | 4        | yes             | Narrowed   | step 3, as #501, which it duplicates                           |
| #776         | 2026-09-22 | the page listing every schema-load fault                        | none     | yes             | Settled    | `prc-pull-request` step 1 already lists it                     |
| #780         | 2026-09-22 | a route for a rule the grammar cannot ask                       | 3        | yes             | Narrowed   | step 2 runs `prc-change-the-tool`                              |
| #786         | 2026-09-22 | a step for removing a rule                                      | none     | yes             | Narrowed   | step 5 accounts for a removed rule                             |
| #804         | 2026-10-10 | the `plugin.json` pins a shape version move reaches             | none     | yes             | Narrowed   | step 2 runs `prc-change-a-skill`, which lists no pin yet       |
| #934 comment | 2026-10-18 | a route to `prc-change-a-skill`                                 | none     | no              | Narrowed   | step 2 runs `prc-change-a-skill`                               |

## Misses

One session missed an obligation that a step stated. Four other findings reported an obligation no step stated: #502,
#728, #786 and #804. They are not misses, because no step asked the session for them.

| Miss                                                   | Found by | Obligation                                   | Stated by                 |
|--------------------------------------------------------|----------|----------------------------------------------|---------------------------|
| `docs/reference/held-to.md` left short of three faults | #776     | list each fault the schema-load pass reports | `prc-pull-request` step 1 |
