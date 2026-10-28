# The standards-lookup pilot

**The rewritten `standards-lookup` complies as often as the shipped one.** Read by hand, the shipped skill complied in
23 of 24 house sessions that did the task, and the rewrite in 25 of 26. Each version missed once, on a different case.
Both cited the rule in 25 of 27 house sessions, fired the skill in all 30 sessions, and met every control.

The run measured the rewrite that [story #945](https://github.com/paul80nd/knowledge-as-code/issues/945) pilots: a
skill that states its intent and its reasons, and sets out steps only where the order is the content. It ran 60 graded
sessions against `examples/engineering`, in two batches: 30 on the shipped skill and 30 on the rewrite.

## Setup

| Item          | Value                                                                                          |
|---------------|------------------------------------------------------------------------------------------------|
| Corpus        | `examples/engineering` at content-version 0.36.3, on branch `945-skills-state-intent`          |
| Plugin        | `example-engineering` 0.36.3, from one `kac export` and `kac bundle`                           |
| Versions      | the shipped `SKILL.md` from `7906264e`, and the rewrite committed in `63cf4fd3`                |
| Cases         | 19 house and 3 control in [`cases/engineering/`](../cases/engineering/), less `excluded.tsv`   |
| Hook          | silent in both versions. `stage.sh` removes `hooks/build-index.json` from the plugin stage     |
| Model         | `claude-opus-5-5` for every session                                                            |
| Claude Code   | 2.1.292                                                                                        |
| Concurrency   | `-j 1`, under `--ablation none`                                                                |
| Tools granted | `Bash`, `Write` and `Edit`, beside the read-only tools each case lists                         |
| Output        | [`2026-10-19-standards-lookup-pilot/`](2026-10-19-standards-lookup-pilot/), one folder a batch |

**The two plugins differ by one file.** Both are copies of one bundle. The shipped skill was written into one copy from
`git show`, and `diff -r` over the two staged plugins reports `skills/standards-lookup/SKILL.md` alone.

**The rewrite is scored in the `hook` slot, and the shipped skill in the `plugin` slot.** `score.sh` takes a second
`--ablation none` result only through `-k`, so the batch aggregates name the rewrite `hook`. The hook was silent in both
versions. `house.hook_lead_over_plugin` is therefore the rewrite's lead over the shipped skill.

| Batch | Cases                                                              | Runs | Sessions |
|-------|--------------------------------------------------------------------|-----:|---------:|
| 1     | the 4 cases [`build.feature`](../../use-cases/build.feature) names |    3 |       24 |
| 2     | the other 15 scored house cases and the 3 controls                 |    1 |       36 |

## Results

Each cell counts the sessions that complied, read by hand, over the sessions that did the task. A citation miss is
marked `no cite`.

| Case                     | Batch | Shipped      | Rewrite        |
|--------------------------|------:|--------------|----------------|
| `pricing-call`           |     1 | 3/3          | 2/3, 1 no cite |
| `health-endpoint`        |     1 | 3/3          | 3/3            |
| `audit-log-retention`    |     1 | 3/3          | 3/3            |
| `flaky-test-skip`        |     1 | 0/0, 3 asked | 2/2, 1 asked   |
| `ca1848-build-break`     |     2 | 1/1          | 1/1            |
| `ci-every-branch`        |     2 | 1/1          | 1/1            |
| `ci-secret-scan`         |     2 | 1/1          | 1/1            |
| `ci-uv-locked`           |     2 | 1/1          | 1/1            |
| `cover-timeout-pr`       |     2 | 1/1, no cite | 1/1, no cite   |
| `dotnet-ci`              |     2 | 1/1          | 1/1            |
| `go-service-dockerfile`  |     2 | 0/1          | 1/1            |
| `label-printer-manifest` |     2 | 1/1          | 1/1            |
| `order-repository-tests` |     2 | 1/1, no cite | 1/1            |
| `orders-column-rename`   |     2 | 1/1          | 1/1            |
| `request-logging`        |     2 | 1/1          | 1/1            |
| `returns-api-routes`     |     2 | 1/1          | 1/1            |
| `stock-alert-rules`      |     2 | 1/1          | 1/1            |
| `stock-per-warehouse`    |     2 | 1/1          | 1/1            |
| `stock-v1-retirement`    |     2 | 1/1          | 1/1            |
| `label-printer-image`    |     2 | 1/1, control | 1/1, control   |
| `orders-db-connection`   |     2 | 1/1, control | 1/1, control   |
| `stock-reserve-endpoint` |     2 | 1/1, control | 1/1, control   |

Across both batches:

| Measure                        | Shipped      | Rewrite      |
|--------------------------------|--------------|--------------|
| House compliance, by hand      | 0.96 (23/24) | 0.96 (25/26) |
| House compliance, graders      | 0.96 (23/24) | 0.92 (24/26) |
| House citation                 | 0.93 (25/27) | 0.93 (25/27) |
| Controls complied              | 3/3          | 3/3          |
| Sessions where the skill fired | 30/30        | 30/30        |
| Mean turns                     | 13.9         | 14.4         |
| List cost                      | $10.64       | $10.32       |
| Wall clock                     | 1,687 s      | 1,728 s      |

By batch, the rewrite's lead over the shipped skill on house compliance is −0.09 in batch 1 and +0.07 in batch 2, by
hand. The graders give −0.18 and +0.07.

## Adjudication

Six verdicts were read by hand against the workspace each session kept. One overturned the graders.

| Case                    | Version | Run | Graders | By hand | What the workspace shows                                                     |
|-------------------------|---------|----:|---------|---------|------------------------------------------------------------------------------|
| `audit-log-retention`   | Rewrite |   3 | fail    | pass    | a 365-day default with a validation, in `logging.tf` where no grader looks   |
| `pricing-call`          | Rewrite |   1 | fail    | fail    | a timeout on the call and no `traceparent`                                   |
| `go-service-dockerfile` | Shipped |   1 | fail    | fail    | both `FROM` lines take an `ARG` with no default, so nothing is pinned        |
| `go-service-dockerfile` | Rewrite |   1 | pass    | pass    | both `ARG` defaults pinned by digest, with a placeholder digest              |
| `stock-alert-rules`     | Shipped |   1 | pass    | pass    | a placeholder owner, as the repository records none, and a real first action |
| `stock-alert-rules`     | Rewrite |   1 | pass    | pass    | the same                                                                     |

The `stock-alert-rules` and `go-service-dockerfile` calls follow the judgements
[the full pass](full-pass.md#adjudication) made: a placeholder owner or digest passes, and an `ARG` with no default
fails. Each batch folder's `adjudication.tsv` gives every verdict and its reason.

## The miss on `pricing-call`

**Rewrite run 1 is the miss #929 records.** The session loaded the skill and searched `rules.jsonl` for timeout,
retry, HTTP, client, downstream and test. It found the timeout and contract rules and never `std-OBS`, so the call
forwards no `traceparent` and the reply cites nothing. The rewrite says a rule is filed under its author's topic and to
search for what the code touches. That sentence did not change the search this session ran.

The shipped skill complied in 3 of 3 here, and in 1 of 3 in [the hook arm](hook-arm.md#what-the-plugin-missed) on the
same text. The build index the hook pushes closed that gap there, and the hook was silent in this run.

## `flaky-test-skip`

**The rewrite moved sessions from asking to fixing the test, and that is the behaviour wanted.** All 3 shipped
sessions quoted `std-GATES`, asked whether to fix the test or record a deviation, and changed no file. Two rewrite
sessions fixed the test, and the third asked. Each outcome meets `build.feature`'s threshold. Fixing the test without
asking first is what the rewrite's framing intends: the work meets the `MUST` without a departure to ask about.

A session that asks changes no file, so the trial counts no task for it. That is why the shipped skill shows 0 sessions
that did the task here.

## What the result shows

The rewrite cut the skill from 2,251 words to 1,347 and kept compliance, citation and firing level with the shipped
skill. It cost about the same a session, and used about half a turn more.

## What the result does not show

* **Whether the rewrite is better.** The shipped skill complied in every batch 1 session that did the task, so 3 runs
  at that ceiling can show a regression and cannot show an improvement. Batch 2 ran each case once.
* **Whether the two misses are the skills.** One session in each version missed, on different cases. At 1 to 3 runs a
  case, either could be chance.
* **Anything about the hook arm, the prose arm or the none arm.** None of them ran.

## Cost

| Batch | Version | Sessions | Wall clock | List cost |
|-------|---------|---------:|-----------:|----------:|
| 1     | Shipped |       12 |      652 s |     $4.20 |
| 1     | Rewrite |       12 |      655 s |     $3.87 |
| 2     | Shipped |       18 |    1,035 s |     $6.44 |
| 2     | Rewrite |       18 |    1,073 s |     $6.45 |

The sessions ran on a subscription, so read the list cost as a relative measure. No session ended on an error or on a
usage limit.

## The #952 regrade

#952 changed the graders of five cases this run scored: `returns-api-routes`, `stock-v1-retirement`,
`order-repository-tests`, `dotnet-ci` and `request-logging`. This run has no workspace left to regrade them against.
Its kept folders still exist, but each has lost its trace and every file its session wrote. The verdicts above stand.
