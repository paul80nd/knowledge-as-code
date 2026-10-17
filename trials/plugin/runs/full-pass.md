# Full pass

**On the house cases, the plugin and the prose arm comply at about the same rate, and both far above the none arm.**
Read by hand, the plugin complies in 0.84 of house sessions, the prose arm in 0.88 and the none arm in 0.23. The
plugin leads the none arm by 0.61, and trails the prose arm by 0.04. The plugin cites the record in 0.82 of house
sessions, the prose arm in 0.65. A prose session costs 2.2 times a plugin session.

The pass ran 22 scored cases against `examples/engineering`: 19 house cases and 3 controls, 3 times each in each of
the 3 arms. That is 198 graded sessions, in 3 batches of one run each. The plugin is the one `kac bundle` built from
that corpus (one repository of knowledge records).

## Setup

| Item          | Value                                                                                        |
|---------------|----------------------------------------------------------------------------------------------|
| Corpus        | `examples/engineering` at `dcadf3f9`, content-version 0.29.0                                 |
| Plugin        | `example-engineering` 0.29.0, from `kac export` and `kac bundle` on that commit              |
| Cases         | 19 house and 3 control in [`cases/engineering/`](../cases/engineering/), less `excluded.tsv` |
| Model         | `claude-opus-5-5` for every session                                                          |
| Claude Code   | 2.1.292                                                                                      |
| Runs          | 3 per case per arm, as 3 batches of 1                                                        |
| Tools granted | `Bash`, `Write` and `Edit`, beside the read-only tools each case lists                       |
| Judge calls   | none. No case has an `llm` grader.                                                           |
| Output        | [`full-pass/`](full-pass/): `aggregate.md`, `sessions.tsv`, `adjudication.tsv`, both results |

**Every batch ran the 0.29.0 plugin.** Batches 2 and 3 were staged from a copy of batch 1's staged plugin. `main` has
since moved to content-version 0.30.0, whose one record change moves the deadline for a high dependency finding in
`std-DEPS` from 30 days to 14. No case tests that deadline. The prose text staged for batches 2 and 3 is byte-identical
to batch 1's.

## Batches

| Batch | Concurrency | Cases staged | Sessions | Wall clock, plugin and prose stages | `costUsd` |
|-------|-------------|-------------:|---------:|------------------------------------:|----------:|
| 1     | `-j 3`      |           25 |       75 |                       773 s + 415 s |     31.14 |
| 2     | `-j 1`      |           22 |       66 |                     2187 s + 1130 s |     27.74 |
| 3     | `-j 1`      |           22 |       66 |                     2038 s + 1072 s |     27.23 |

Batch 1 ran at `-j 3`, and batches 2 and 3 at `-j 1`, for the reason [Failed attempts](#failed-attempts) gives. No
session in the three batches ended on an error. Batch 1 also ran the 3 cases excluded since, and `score.sh` counts none
of their 9 sessions.

Every batch was graded again by [`regrade.mjs`](../regrade.mjs) under the graders batches 2 and 3 ran. On the graders
that had not changed, it agreed with the tool on every verdict: 401 in batch 1, 359 in batch 2 and 359 in batch 3.
[`merge.sh`](../merge.sh) merged the three regraded files of each stage, in batch order, and `score.sh` scored them.

## Failed attempts

Two attempts at batch 2 failed. Neither is in the result, and neither cost is in `cost_usd`.

| Attempt | What happened                                                                                                            | `costUsd` |
|---------|--------------------------------------------------------------------------------------------------------------------------|----------:|
| First   | The subscription's session limit was reached. 6 of 44 plugin-stage sessions and all 22 prose-stage sessions ended on it. |     10.14 |
| Second  | At `-j 3`, 53 of 66 sessions timed out at 300 s: 36 in the plugin stage and 17 in the prose stage.                       |     17.89 |

After the second attempt, a probe ran the plugin and none sessions of one case, and both finished in 99 s at a cost of
0.58. Batches 2 and 3 then ran at `-j 1`, and no session timed out.

## Excluded cases

The environment decides the result of three cases, so [`excluded.tsv`](../cases/engineering/excluded.tsv) lists them,
`stage.sh` stages none of them, and `score.sh` counts none of their sessions.

| Case                    | Why no session can show the rule                                                         |
|-------------------------|------------------------------------------------------------------------------------------|
| `billing-new-solution`  | The rule copies the .NET runtime's `.editorconfig`, and the sandbox blocks the download. |
| `pagination-fix-commit` | The sandbox blocks the git that `/usr/bin/git` hands off to, so commits fail.            |
| `ci-coverage-threshold` | The rule needs a coverage figure the repository records, and the scaffold records none.  |

## Results

**The adjudicated figures are the result.** The graders' figures are beside them. Each "runs" column gives runs 1, 2
and 3, which are batches 1, 2 and 3. Adjudication overturned no citation, so each citation figure is both.

House cases, 19 per run:

| Arm    | Adjudicated compliance | Runs             | Graders' compliance | Runs             | Citation | Runs             |
|--------|-----------------------:|------------------|--------------------:|------------------|---------:|------------------|
| plugin |           0.84 (46/55) | 0.89, 0.78, 0.83 |        0.78 (43/55) | 0.89, 0.72, 0.72 |     0.82 | 0.79, 0.84, 0.84 |
| prose  |           0.88 (50/57) | 0.95, 0.84, 0.84 |        0.93 (53/57) | 1.00, 0.89, 0.89 |     0.65 | 0.74, 0.58, 0.63 |
| none   |           0.23 (13/57) | 0.26, 0.21, 0.21 |        0.21 (12/57) | 0.26, 0.21, 0.16 |     0.00 | 0.00, 0.00, 0.00 |

Controls, 3 per run:

| Arm    | Adjudicated compliance | Runs             | Graders' compliance | Runs             | Citation | Runs             |
|--------|-----------------------:|------------------|--------------------:|------------------|---------:|------------------|
| plugin |             1.00 (9/9) | 1.00, 1.00, 1.00 |          1.00 (9/9) | 1.00, 1.00, 1.00 |     0.89 | 1.00, 0.67, 1.00 |
| prose  |             0.89 (8/9) | 0.67, 1.00, 1.00 |          0.89 (8/9) | 0.67, 1.00, 1.00 |     0.78 | 1.00, 0.67, 0.67 |
| none   |             0.67 (6/9) | 0.67, 0.67, 0.67 |          0.67 (6/9) | 0.67, 0.67, 0.67 |     0.00 | 0.00, 0.00, 0.00 |

The plugin's lead on house compliance:

| Over  | Adjudicated | Runs                | Graders' |
|-------|------------:|---------------------|---------:|
| prose |       -0.04 | -0.06, -0.06, -0.01 |    -0.15 |
| none  |        0.61 | 0.63, 0.57, 0.62    |     0.57 |

On the controls, the plugin leads the prose arm by 0.11 and the none arm by 0.33, by both counts.

**The lookup skill fired in all 57 house sessions and 8 of 9 control sessions.** The control session that did not fire
is `stock-reserve-endpoint` in run 2. It wrote the contract, and cited no record.

## Adjudication

85 compliance verdicts were read by hand against the workspace and trace each session kept.
[`full-pass/adjudication.tsv`](full-pass/adjudication.tsv) gives each one's graders' verdict, adjudicated verdict and
reason. The reading covered every failing compliance verdict, every house case where the plugin and prose arms
disagree, and every pass of `stock-alert-rules` and `flaky-test-skip`, whose graders were known to pass weak output.

| Arm    | Read | Fail to pass | Pass to fail |
|--------|-----:|-------------:|-------------:|
| plugin |   18 |            4 |            1 |
| prose  |   19 |            0 |            3 |
| none   |   48 |            1 |            0 |

The 9 verdicts overturned:

| Case                     | Arm    | Run     | Adjudicated | What the workspace shows                                                         |
|--------------------------|--------|---------|-------------|----------------------------------------------------------------------------------|
| `ca1848-build-break`     | plugin | 2, 3    | pass        | a `[LoggerMessage]` method. The grader matched the session's grep of the corpus. |
| `label-printer-manifest` | plugin | 3       | pass        | `image: …@IMAGE_DIGEST`, a placeholder CI fills                                  |
| `orders-column-rename`   | plugin | 2       | pass        | an expand-only migration. The grader matched "Drop customer_name" in a comment.  |
| `orders-column-rename`   | none   | 3       | pass        | the same migration and the same comment                                          |
| `stock-alert-rules`      | plugin | 2       | fail        | `first_action: "TODO: first action to take"` on both alerts                      |
| `flaky-test-skip`        | prose  | 1, 2, 3 | fail        | a skip whose reason mentions a deviation, with none recorded                     |

Three calls are judgements, and each is applied the same way in every arm:

* **A base image taken from an `ARG` with no default fails.** Plugin runs 2 and 3, and prose run 2, wrote
  `FROM ${BUILD_IMAGE}` and told the user to pass a digest. The file pins nothing, and a build can pass a tag. Passing
  them would raise the plugin to 0.87 and the prose arm to 0.89.
* **An alert owner left as a placeholder passes.** The repository records no owner, so no session could name one.
  Every plugin and prose session wrote a placeholder owner. A placeholder digest passes for the same reason.
* **A session that asked before acting did no task.** In `flaky-test-skip`, plugin runs 2 and 3 quoted the rule and
  asked whether to fix the test or record a deviation. They changed no file, so no compliance is counted. Counting both
  as compliant leaves the plugin at 0.84.

The reader knew each session's arm.

## What the plugin missed

Most of the plugin's house misses are two cases where the skill looked up the wrong records:

* **`health-endpoint`, all 3 runs.** The lookup found `std-API` and `std-OBS`, and not `std-DEPLOY`. No endpoint reports
  the commit. The prose arm reported it in runs 1 and 2.
* **`pricing-call`, all 3 runs.** The lookup found the timeout and contract rules, and not the tracing rule. No call
  forwards the `traceparent`. The prose arm forwarded it in run 1.

## Cost

`costUsd` is the list-price estimate the tool reports. The sessions ran on a subscription, so read it as a relative
measure only.

| Arm    | Scored sessions | `costUsd` | Per session |
|--------|----------------:|----------:|------------:|
| plugin |              66 |     22.07 |        0.33 |
| prose  |              66 |     48.35 |        0.73 |
| none   |              66 |     11.95 |        0.18 |

The stages report 86.12 in all. The 3.75 beyond the scored sessions is batch 1's 9 sessions of excluded cases. The
prose arm costs more because its 18,868-word system prompt is read on every turn.

## Graders fixed

These graders were fixed after batch 1, and before batches 2 and 3 ran. Batch 1 was regraded under them:

| Case                     | Grader                                                          | Now also passes                                             |
|--------------------------|-----------------------------------------------------------------|-------------------------------------------------------------|
| `stock-alert-rules`      | `comply-names-first-action`                                     | a `first_action` key. `\b` never matched after `_`.         |
| `label-printer-manifest` | `comply-image-digest` and `comply-no-tag-only-image`            | an image named `@${VAR}`, which CI fills with the digest    |
| `go-service-dockerfile`  | `comply-digest-present--arg`, `comply-no-tag-only-base`         | a `FROM ${VAR}` whose `ARG` default is pinned by digest     |
| `audit-log-retention`    | `comply-kept-a-year--local`                                     | a retention of a year or more set through a Terraform local |
| `flaky-test-skip`        | `task-turned-off--fixed`, `comply-names-deviation--not-skipped` | a test fixed without a sleep, and left unskipped            |

These were fixed after all three batches, from the faults adjudication found. They apply to the next run. This run is
scored by the graders it ran, and its adjudication is its result.

| Case                     | Grader                                               | Change                                                                                    |
|--------------------------|------------------------------------------------------|-------------------------------------------------------------------------------------------|
| `ca1848-build-break`     | `comply-no-blanket-*`                                | matches `<NoWarn>` and `dotnet_diagnostic.CA1848.severity`, so a grep for the word passes |
| `orders-column-rename`   | `comply-not-destructive-*`                           | matches only inside an `ALTER TABLE` statement, so a comment passes                       |
| `label-printer-manifest` | `comply-image-digest` and `comply-no-tag-only-image` | pass a bare placeholder such as `@IMAGE_DIGEST`                                           |
| `stock-alert-rules`      | `comply-first-action-not-placeholder`, new           | fails a first action that is `TODO`, `TBD`, `FIXME` or `<…>`                              |
| `flaky-test-skip`        | `comply-names-deviation--skipped`                    | needs a deviation reference such as `DEV-0114`, an issue or `#123`                        |

`regrade.mjs` applied the fixed graders to all 198 kept sessions. They changed exactly the 9 verdicts adjudication
overturned, and no other. A rename, a multi-line drop, a `<NoWarn>` and an `.editorconfig` severity, each written as a
test input, still fail.

## What the result shows

* The plugin and the prose arm deliver the same records, and on these cases they change what a session builds by about
  the same amount. Both comply with a house rule about four times as often as a session with neither.
* The plugin cites the record more often than the prose arm, at under half its cost a session.
* The adjudicated lead over the prose arm is -0.06, -0.06 and -0.01 in the three runs. One session moves a rate by
  about 0.02, so the plugin's deficit is two or three sessions in 57.

## What the result does not show

* **A corpus too large to paste.** This corpus fits on the prose arm's system prompt. A larger one would not, and only
  the plugin could deliver it.
* **Several corpora at once.** Each session had one corpus.
* **A `CLAUDE.md` delivered as a real session delivers it.** The prose arm receives the text on its system prompt.
* **Another model, or a session with network access.** Every session ran `claude-opus-5-5` in the sandbox.

## What makes the comparison less fair

* **Batch 1 ran at `-j 3`, and batches 2 and 3 at `-j 1`.** Batch 1 is the best run of the plugin and prose arms, and
  3 runs cannot tell concurrency from chance.
* **Four house cases behave as controls.** The none arm complies in every run of `ci-every-branch`, `ci-uv-locked`,
  `order-repository-tests` and, adjudicated, `orders-column-rename`. Every arm passes all 12 of their sessions.
  Without them, the plugin complies in 0.79, the prose arm in 0.84 and the none arm in 0.02, and the lead over the none
  arm is 0.77.
* **The graders read literal forms**, so the graders' figures penalise an arm that complies in another form. That cost
  the plugin 4 sessions, the none arm 1 and the prose arm none. The graders also passed 4 weak outputs: 3 in the prose
  arm and 1 in the plugin arm.
