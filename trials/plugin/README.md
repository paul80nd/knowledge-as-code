# The plugin trial

This trial measures whether the plugin a corpus (one repository of knowledge records) ships changes what an agent
builds. It gives one coding task to three kinds of session. Each task breaks one of the corpus's rules if the session
does not know it. It then checks whether the files each session wrote comply with that rule.

Whoever decides whether a corpus ships its plugin, or how the plugin delivers its records, reads the result.

## The three arms

| Arm    | The session has                                                             | How it runs                                                           |
|--------|-----------------------------------------------------------------------------|-----------------------------------------------------------------------|
| plugin | the plugin `kac bundle` builds from the corpus                              | the with-arm of `claude plugin eval` on the plugin                    |
| prose  | every record the plugin's export contains, as one text on its system prompt | a second run, on a plugin with no components, under `--ablation none` |
| none   | nothing                                                                     | the without-arm of the same run as the plugin arm                     |

Every arm's sessions get the same tools: the read-only tools each case lists, and `Bash`, `Write` and `Edit`.

[`prose-claude-md.sh`](prose-claude-md.sh) writes the prose text from the export the plugin contains. It reads the
types, their folders and their parts files from the export's `manifest.json`. For each record of each type it writes
the title, the id, every field, every exported section and the link, then each part with its id and every field. So
the prose arm and the plugin arm have the same records.

**The one remaining difference is delivery.** `claude plugin eval` loads no `CLAUDE.md` in a run, even one a scaffold
writes into the workspace. The prose arm therefore receives the text through each case's `append_system_prompt`. A
`CLAUDE.md` in a real session arrives as context in the conversation, so the prose arm is close to that delivery and
not the same.

## The full pass and the pilot

The full pass is 22 scored cases, run 3 times each in each of the 3 arms: 198 graded sessions. It runs in 3 batches
of one run each, so that each batch fits one window of a subscription's usage limit.
[Combining batches](#combining-batches) says how the batches become one scored run. The pilot is 3 cases, run once
each in each arm: 9 graded sessions. It measures the cost of a session and checks the grading before anyone approves
the full pass.

## Choosing cases

**A case tests a house rule: one a model would not follow unprompted.** A house rule is a convention of the
organisation that wrote the corpus: a naming scheme, a required library, a file in a set place, an id format. A model
follows general good practice without being told, so a case testing it shows nothing about the plugin. The none arm
complies too.

Each case is one of two kinds, and its folder says which:

| Kind      | Folder     | What it tests                                                                  | Where it is reported  |
|-----------|------------|--------------------------------------------------------------------------------|-----------------------|
| `house`   | `house/`   | a house rule. These are the trial's cases, and the lead is read from them.     | the `house.` fields   |
| `control` | `control/` | a rule that is general good practice, which the none arm may follow by default | the `control.` fields |

To tell the two apart, ask whether a capable engineer new to the organisation would meet the rule without being
told. If they would, the case is a control. `stage.sh` refuses a case in neither folder. A case also gives its kind
as its first tag, so `--tag house` runs one kind alone.

The cases for a corpus live in their own folder under [`cases/`](cases/). [`cases/engineering/`](cases/engineering/)
contains the cases for `examples/engineering`: 22 house cases and the pilot's 3 controls. Nothing else in the method
knows which corpus it runs against.

**A case listed in the folder's `excluded.tsv` is neither run nor scored.** Each line gives the case's path and the
reason, separated by a tab. `stage.sh` leaves the case out of both stages, and `score.sh` leaves its sessions out of
every count. Exclude a case whose result the environment decides, and fix the case before you take it out of the list.
[`cases/engineering/excluded.tsv`](cases/engineering/excluded.tsv) lists these:

| Case                           | Why no session can show the rule                                                         |
|--------------------------------|------------------------------------------------------------------------------------------|
| `house/billing-new-solution`   | The rule copies the .NET runtime's `.editorconfig`, and the sandbox blocks the download. |
| `house/pagination-fix-commit`  | The sandbox blocks the git that `/usr/bin/git` hands off to, so commits fail.            |
| `house/ci-coverage-threshold`  | The rule needs a coverage figure the repository records, and the scaffold records none.  |

| Case                             | Rule a session breaks                                                                                  |
|----------------------------------|--------------------------------------------------------------------------------------------------------|
| `house/ci-secret-scan`           | `std-SECRET.nowhere-else-holds-a-secret`: the new CI runs no secret scanner                            |
| `house/returns-api-routes`       | `std-API.a-change-carries-a-version-and-a-notice`: the new routes have no version segment              |
| `house/stock-per-warehouse`      | `std-API.a-change-carries-a-version-and-a-notice`: the breaking change replaces `/v1`                  |
| `house/stock-v1-retirement`      | `std-API.a-change-carries-a-version-and-a-notice`: `/v1` sends no `Deprecation` or `Sunset`            |
| `house/ci-uv-locked`             | `std-DEPS.where-a-package-comes-from`: CI installs without `--locked`                                  |
| `house/pricing-call`             | `std-OBS.one-request-reads-as-one-timeline`: the outbound call drops the `traceparent`                 |
| `house/request-logging`          | `std-OBS.one-request-reads-as-one-timeline`: log lines are unstructured or have no trace id            |
| `house/stock-alert-rules`        | `std-OBS.somebody-acts-on-every-alert`: an alert names no owner or first action                        |
| `house/cover-timeout-pr`         | `std-PR.agent-produced-work-says-what-produced-it`: the PR body omits the agent, model or task         |
| `house/pagination-fix-commit`    | `std-VCS.every-change-is-attributable-and-reviewed`: the commit subject lacks `#2231`                  |
| `house/flaky-test-skip`          | `std-GATES.a-failing-check-stops-the-change`: the skip cites no deviation                              |
| `house/ci-every-branch`          | `std-GATES.every-change-is-built-and-tested-automatically`: push builds only some branches             |
| `house/orders-column-rename`     | `std-DEPLOY.a-way-back-exists-before-the-change-goes`: the migration renames or drops the column       |
| `house/health-endpoint`          | `std-DEPLOY.the-artefact-is-built-once`: the health endpoint reports no commit                         |
| `house/ci-coverage-threshold`    | `std-TEST.know-what-the-suite-reaches`: the build has no coverage floor                                |
| `house/go-service-dockerfile`    | `std-CONT.the-base-image-is-chosen-and-pinned`: a `FROM` has no digest                                 |
| `house/label-printer-manifest`   | `std-CONT.a-published-tag-never-moves`: the Deployment names the image by tag                          |
| `house/billing-new-solution`     | `std-CSSTY.the-conventions-come-from-one-file`: the project lacks the build's style settings           |
| `house/ca1848-build-break`       | `std-CSSTY.a-suppression-is-local-and-says-why`: a pragma, `NoWarn` or an unjustified suppression      |
| `house/order-repository-tests`   | `std-NETTST.an-integration-test-starts-what-it-needs`: Postgres comes from anywhere but Testcontainers |
| `house/dotnet-ci`                | `std-NETTST.coverage-is-collected-by-the-build`: CI does not collect and publish coverage              |
| `house/audit-log-retention`      | `pol-OBSV.AUDIT`: the workspace keeps security telemetry for under a year                              |
| `control/orders-db-connection`   | `std-SECRET.nowhere-else-holds-a-secret`: the password lands in a file                                 |
| `control/label-printer-image`    | `std-CONT.the-container-holds-only-what-the-service-needs`: no non-root `USER`                         |
| `control/stock-reserve-endpoint` | `std-API.the-contract-is-the-source-of-truth`: no contract for the new endpoint                        |

## What leaves the organisation

A run against your own corpus writes three kinds of output:

* **The cases** contain your team's tasks, and the scaffold each one starts from. They stay with the organisation
  that wrote them. Never publish them.
* **The raw results** are `sessions.tsv`, each `aggregate-result.json`, and the workspaces and traces `--keep-temp`
  keeps. They contain case names, prompt text, the code each session wrote, and graders' explanations quoting it.
  They stay with the organisation that ran the trial. Never publish them.
* **`aggregate.md`** contains only counts, rates, the model and the cost, under the fields
  [defined below](#the-aggregate). It contains no case name, prompt or code, so you can publish it without a review.

Publish `aggregate.md` alone. Pass `--no-publish` to every `claude plugin eval` command. Without it, the tool
publishes its report to claude.ai wherever your account supports it, and the report contains the prompts.

## What you need

| Tool                         | Why                                                                        |
|------------------------------|----------------------------------------------------------------------------|
| Claude Code 2.1.292 or later | `claude plugin eval` runs the cases. The pilot ran on 2.1.292.             |
| `kac`                        | `kac export` and `kac bundle` build the plugin from the corpus.            |
| `sh`, `jq`                   | The stage and score scripts are POSIX shell, and read JSON with `jq`.      |
| Node.js 24 or later          | `regrade.mjs` applies the graders with the tool's own regular expressions. |
| `git`                        | Each scaffold makes its workspace a git repository.                        |

A run that grants `Bash` needs the sandbox Claude Code runs shell commands in. macOS has it. On Linux, install
`bubblewrap` and `socat`. On Windows, run the trial under WSL2.

## Running the trial

Run each step from the root of this repository, and the commands in the corpus as its `CLAUDE.md` says. `$CORPUS` is
the corpus folder, `$CASES` its case folder, and `$STAGE` an empty folder outside the repository.

1. **Build the plugin.** In the corpus, run `kac export`, then `kac bundle`. The plugin is `$CORPUS/.dist/plugin/`.
2. **Stage the arms.** `claude plugin eval` reads cases only from a folder below the plugin, so the script copies the
   plugin and the cases into `$STAGE`:

   ```sh
   trials/plugin/stage.sh "$CORPUS/.dist/plugin" "$CASES" "$STAGE"
   ```

3. **Count the sessions before you start any.** The plugin stage starts cases × runs × 2 sessions, and the prose stage
   cases × runs. Count only the cases `stage.sh` staged. An `llm` grader adds three judge calls per run. No case here
   has one.
4. **Run the plugin and none arms:**

   ```sh
   cd "$STAGE/plugin" && claude plugin eval . --eval-dir evals --ablation with-without --runs <runs> \
     --model claude-opus-5-5 --scaffold --trust-plugin --no-publish --keep-temp -j 3 \
     --output-dir "$STAGE/out-plugin" --allow-tools Bash Write Edit
   ```

5. **Run the prose arm:**

   ```sh
   cd "$STAGE/prose" && claude plugin eval . --eval-dir evals --ablation none --runs <runs> \
     --model claude-opus-5-5 --scaffold --trust-plugin --no-publish --keep-temp -j 3 \
     --output-dir "$STAGE/out-prose" --allow-tools Bash Write Edit
   ```

6. **Score the run** into a new folder under `runs/`, named by its date:

   ```sh
   trials/plugin/score.sh "$STAGE/out-plugin/aggregate-result.json" "$STAGE/out-prose/aggregate-result.json" \
     trials/plugin/runs/<date> "$CASES/excluded.tsv"
   ```

7. **Read every verdict by hand** against the workspace the session left. Each run prints a `kept temp:` folder,
   whose workspace is sealed. Run `chmod 700 <folder> <folder>/sealed` to open it. The workspace is
   `<folder>/sealed/home/cwd`, and the trace is `<folder>/out/trace.jsonl`. Run no `git` inside the folder.
8. **Write what you found** to `runs/<date>.md`: the setup, the results, the cost, and any grader you think is wrong.
   Where the result file gives no tokens, take them from the `result` line of each trace.

`--no-publish` keeps the report off claude.ai. `--keep-temp` keeps each workspace and trace for step 7. Without it,
both are deleted when the stage finishes.

## Combining batches

Run each batch as steps 4 and 5, with `--runs 1` and its own `--output-dir`. Stage once, and run every batch from
that stage, so every batch has the same plugin and the same prose text. Then merge each stage's result files, and
score the two merged files as step 6:

```sh
trials/plugin/merge.sh "$STAGE/plugin.json" "$STAGE"/out-plugin-r*/aggregate-result.json
trials/plugin/merge.sh "$STAGE/prose.json" "$STAGE"/out-prose-r*/aggregate-result.json
```

[`merge.sh`](merge.sh) gives each case the runs of every batch, sums the cost, and marks the result partial if any
batch was. It refuses batches that differ in stage, model, Claude Code version or plugin, or that grade a shared case
by different graders.

## Regrading a batch

Regrade a kept batch when a grader changes after it ran. [`regrade.mjs`](regrade.mjs) applies the graders in the case
folder to each workspace and trace the batch kept, and writes a result file of the same shape:

```sh
node trials/plugin/regrade.mjs "$CASES" "$STAGE/out-plugin-r1/aggregate-result.json" "$STAGE/plugin-r1.json"
```

Open each kept folder with `chmod 700` first, as step 7 says. The script compares its verdict with the tool's for
every grader whose name and definition did not change, and exits 1 on any disagreement. Trust it on the changed
graders only where that check passes. Merge and score the regraded file in place of the one the tool wrote.

## Writing a case

A case is a folder under `house/` or `control/`, with a `prompt.md`, a `case.yaml` naming its `scaffold.sh`, and
graders under `graders/`, as `claude plugin eval` reads them.

* **The prompt is a task somebody would type.** It never names the rule, the record or the corpus.
* **The first tag in `prompt.md` is the case's kind**, `house` or `control`, followed by the id of the record the rule
  sits in.
* **The scaffold builds the repository with heredocs.** A run cannot read the case folder, so the scaffold contains
  every file it writes.
* **A grader reads what the session wrote, and never what it says it did.** It reads a file in the workspace, or the
  input of a `Write`, `Edit` or `Bash` call.

The prefix of a grader's file name says what it measures:

| Prefix    | Passes when                                                    | Counts towards |
|-----------|----------------------------------------------------------------|----------------|
| `task-`   | the session did the task it was given                          | task           |
| `comply-` | the files the session wrote comply with the rule               | compliance     |
| `cite-`   | the final reply contains the id of the record the rule sits in | citation       |
| `fired-`  | the session invoked a lookup skill                             | firing         |

**Graders whose names share the part before `--` are alternatives.** The set passes where one of them does. A file
can arrive through `Write` or through `Bash`, so `comply-contract--written` and `comply-contract--shell` each look for
the contract in one tool's input, and the session complies where either finds it.

`claude plugin eval` scores every grader together, and leaves a `fired-` grader out of a two-arm score. The trial
reads each grader's verdict, and ignores the tool's score.

## Scoring

[`score.sh`](score.sh) reads the two result files and writes `sessions.tsv` and `aggregate.md`. It reports each kind
of case on its own.

* **A session complies** where every `comply-` grader, or set of alternatives, passed.
* **A session cites** where every `cite-` grader passed.
* **Compliance is counted over the sessions that did the task**, where every `task-` grader passed. A session that
  did not do the task has written nothing to judge.
* **A session that ended on an error** is counted under `<kind>.<arm>.errors`, and in no rate.
* **A session of an excluded case** is counted nowhere. `excluded_cases` says how many cases were left out.

The plugin's lead over an arm is its compliance rate minus that arm's, on the house cases. Citation is reported beside
it and never added in, because the plugin's skills teach a session to cite and the other arms have nothing that does.
The controls are reported in fields of their own, so a none arm that follows general practice by default cannot close
the gap on the house cases.

## The aggregate

`aggregate.md` is a table of fields and values. It contains counts, rates and the model's id, and no free text, so a
run against any corpus can be published in the same shape. A rate is a fraction to two decimal places, and is empty
where it would divide by 0.

`<kind>` is `house` or `control`, and `<arm>` is `plugin`, `prose` or `none`. Each `<kind>.<arm>` field is written for
every kind and every arm, zero included.

| Field                          | Value                                                                               |
|--------------------------------|-------------------------------------------------------------------------------------|
| `trial`                        | `plugin`                                                                            |
| `method`                       | the version of this method. It moves when a definition on this page changes.        |
| `model`                        | the model id the sessions ran on                                                    |
| `claude_version`               | the Claude Code version that ran the plugin stage                                   |
| `cases`                        | the cases scored, of both kinds                                                     |
| `excluded_cases`               | the cases `excluded.tsv` lists, which no count includes                             |
| `runs_per_case`                | the runs of each case in each arm                                                   |
| `sessions`                     | the graded sessions, over every kind and arm                                        |
| `<kind>.<arm>.sessions`        | the sessions of that kind in that arm                                               |
| `<kind>.<arm>.errors`          | of those, the sessions that ended on an error                                       |
| `<kind>.<arm>.task_done`       | of those, the sessions where every `task-` grader passed                            |
| `<kind>.<arm>.complied`        | of the sessions that did the task, those that comply                                |
| `<kind>.<arm>.compliance_rate` | `<kind>.<arm>.complied` divided by `<kind>.<arm>.task_done`                         |
| `<kind>.<arm>.cited`           | the sessions where every `cite-` grader passed                                      |
| `<kind>.<arm>.citation_rate`   | `<kind>.<arm>.cited` divided by `<kind>.<arm>.sessions` less `<kind>.<arm>.errors`  |
| `<kind>.plugin.fired`          | the plugin sessions of that kind where every `fired-` grader passed                 |
| `<kind>.lead_over_prose`       | `<kind>.plugin.compliance_rate` minus `<kind>.prose.compliance_rate`                |
| `<kind>.lead_over_none`        | `<kind>.plugin.compliance_rate` minus `<kind>.none.compliance_rate`                 |
| `cost_usd`                     | the list-price cost both stages report, judge calls and excluded cases included     |
| `cost_per_session_usd`         | `cost_usd` divided by `sessions`                                                    |

## Reading the result

The plugin leads an arm where `house.lead_over_<arm>` is above 0. State the lead over each arm, and give the citation
rates beside them. Give the control fields after them. A control lead near 0 is expected, because every arm follows
general practice.

A run of one session per case is too small to judge a lead by. The pilot reports one so that the full pass can be
priced and its grading checked.

Where the plugin leads the prose arm, the next measurement is a fourth arm: the corpus's Markdown on disk, with a
`CLAUDE.md` pointing at it.

## Runs

| Run                              | Method | Corpus                 | Sessions | What it was                                  |
|----------------------------------|-------:|------------------------|---------:|----------------------------------------------|
| [2026-10-16](runs/2026-10-16.md) |      1 | `examples/engineering` |        9 | The pilot: 3 controls, once each in each arm |
| [Full pass](runs/full-pass.md)   |      3 | `examples/engineering` |      198 | In progress: batch 1 of 3 run and regraded   |
