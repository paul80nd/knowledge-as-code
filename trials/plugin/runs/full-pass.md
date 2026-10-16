# Full pass

**This pass is in progress: batch 1 of 3 has run.** The full pass runs 22 scored cases, 3 times each in each of the 3
arms, as 3 batches of one run each. Each batch fits one window of a subscription's usage limit. The numbers below are
batch 1 alone. One session per case and arm cannot show a lead.

## Setup

| Item          | Value                                                                                        |
|---------------|----------------------------------------------------------------------------------------------|
| Corpus        | `examples/engineering` at `dcadf3f9`, content-version 0.29.0                                 |
| Plugin        | `example-engineering` 0.29.0, from `kac export` and `kac bundle` on that commit              |
| Cases         | 19 house and 3 control in [`cases/engineering/`](../cases/engineering/), less `excluded.tsv` |
| Model         | `claude-opus-5-5` for every session                                                          |
| Claude Code   | 2.1.292                                                                                      |
| Runs          | 1 per case per arm in each batch                                                             |
| Tools granted | `Bash`, `Write` and `Edit`, beside the read-only tools each case lists                       |
| Judge calls   | none. No case has an `llm` grader.                                                           |

**Every batch runs the plugin batch 1 ran.** Batches 2 and 3 are staged from a copy of batch 1's staged plugin, not a
rebuild. `main` has since moved to content-version 0.30.0. The only record change is the deadline for a high
dependency finding in `std-DEPS`, from 30 days to 14. No case tests it. The prose text staged for batches 2 and 3 is
byte-identical to batch 1's.

## Batches

| Batch | Cases staged | Sessions | Wall clock | `costUsd` | Graded by                    |
|-------|-------------:|---------:|-----------:|----------:|------------------------------|
| 1     |           25 |       75 |     1190 s |     31.14 | `regrade.mjs`, after the run |
| 2     |           22 |       66 |            |           |                              |
| 3     |           22 |       66 |            |           |                              |

Batch 1 ran every case, the 3 now excluded among them. No session ended on an error. `costUsd` is the list-price
estimate the tool reports, and is read here as a relative measure only.

## Excluded cases

Three cases are excluded from the score. The environment decides their result, not the rule.
[`excluded.tsv`](../cases/engineering/excluded.tsv) gives each reason, `stage.sh` stages none of them, and `score.sh`
counts none of their sessions. Batches 2 and 3 save 18 sessions by not running them.

## Graders changed after batch 1

Batch 1 was read by hand against every kept workspace. Five graders failed a session that complied, and are fixed:

| Case                     | Grader                                                          | Now also passes                                             |
|--------------------------|-----------------------------------------------------------------|-------------------------------------------------------------|
| `stock-alert-rules`      | `comply-names-first-action`                                     | a `first_action` key. `\b` never matched after `_`.         |
| `label-printer-manifest` | `comply-image-digest` and `comply-no-tag-only-image`            | an image named `@${VAR}`, which CI fills with the digest    |
| `go-service-dockerfile`  | `comply-digest-present--arg`, `comply-no-tag-only-base`         | a `FROM ${VAR}` whose `ARG` default is pinned by digest     |
| `audit-log-retention`    | `comply-kept-a-year--local`                                     | a retention of a year or more set through a Terraform local |
| `flaky-test-skip`        | `task-turned-off--fixed`, `comply-names-deviation--not-skipped` | a test fixed without a sleep, and left unskipped            |

Each fixed grader still fails every non-compliant file batch 1 kept. Each passes every compliant one.

## Regrading batch 1

Batch 1 ran under the old graders, so [`regrade.mjs`](../regrade.mjs) regraded it from the workspaces and traces it
kept. Before the graders changed, the script agreed with the tool on every one of the 422 verdicts in both stages. Its
result files scored to a `sessions.tsv` and an `aggregate.md` byte-identical to the tool's. After the change, it
agreed on all 401 verdicts whose grader did not change.

| Field              | plugin | prose | none |
|--------------------|-------:|------:|-----:|
| house sessions     |     19 |    19 |   19 |
| house task done    |     19 |    19 |   19 |
| house compliance   |   0.89 |  1.00 | 0.26 |
| house citation     |   0.79 |  0.74 | 0.00 |
| control compliance |   1.00 |  0.67 | 0.67 |
| control citation   |   1.00 |  1.00 | 0.00 |

The lookup skill fired in all 19 house and all 3 control plugin sessions. The plugin's lead on house compliance is
-0.11 over the prose arm and 0.63 over the none arm.

Two plugin sessions break a rule they had looked up. In `health-endpoint` the skill found `std-API` and `std-OBS`, and
the endpoint reports no commit. In `pricing-call` it found the timeout and contract rules, and the outbound call
drops the `traceparent`. The prose arm's control fail is `stock-reserve-endpoint`: the reply says the API has no
contract, and the session wrote none.
