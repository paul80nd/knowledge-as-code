# The plugin trial

This trial measures whether the plugin a corpus (one repository of knowledge records) ships changes what an agent
builds. It gives one coding task to four kinds of session. Each task breaks one of the corpus's rules if the session
does not know it. It then checks whether the files each session wrote comply with that rule.

Whoever decides whether a corpus ships its plugin, or how the plugin delivers its records, reads the result.

## The four arms

| Arm    | The session has                                                             | How it runs                                                           |
|--------|-----------------------------------------------------------------------------|-----------------------------------------------------------------------|
| hook   | the plugin `kac bundle` builds from the corpus, as it ships                 | a third run, on the plugin as built, under `--ablation none`          |
| plugin | the same plugin, its build-index hook silent                                | the with-arm of `claude plugin eval` on the plugin                    |
| prose  | every record the plugin's export contains, as one text on its system prompt | a second run, on a plugin with no components, under `--ablation none` |
| none   | nothing                                                                     | the without-arm of the same run as the plugin arm                     |

**The hook arm is the plugin that ships, so the trial reports the plugin's lead from it.** The plugin arm measures the
lookup alone. The hook and plugin arms differ by one file. `stage.sh` deletes `hooks/build-index.json` from the plugin
arm's copy, so its build-index hook prints nothing. The hook arm keeps it, so its hook pushes the build index at the
first tool call of the session, whichever tool the call uses, and at the first tool call of each subagent. A plugin
whose bundle trimmed the build index ships no `hooks/build-index.json`, as `examples/library` does. `stage.sh` warns
and stages it anyway, and its hook arm pushes nothing.

`claude plugin eval` leaves a hook's context out of `trace.jsonl`. Read whether the build index arrived from the
session's own transcript, under `config/projects/` in the folder `--keep-temp` kept, where it is an attachment of type
`hook_additional_context`. The hook's marker is under `config/plugins/data/`.

Every arm's sessions get the same tools: the read-only tools each case lists, and `Bash`, `Write` and `Edit`.

[`prose-claude-md.sh`](prose-claude-md.sh) writes the prose text from the export the plugin contains. It reads the
types, their folders and their parts files from the export's `manifest.json`. For each record of each type it writes
the title, the id, every field, every exported section and the link, then each part with its id and every field. So
the prose arm and the plugin arm have the same records.

**The one remaining difference is delivery.** `claude plugin eval` loads no `CLAUDE.md` in a run, even one a scaffold
writes into the workspace. The prose arm therefore receives the text through each case's `append_system_prompt`. A
`CLAUDE.md` in a real session arrives as context in the conversation, so the prose arm is close to that delivery and
not the same.

**Where the prose text is too large for a `CLAUDE.md`, give the prose arm a cut of it.** Claude Code warns on a
`CLAUDE.md` over 40,000 characters, so cut the text to that size. Have a reader who has not seen the cases choose the
cut as the corpus's team would, copying record blocks verbatim. After `stage.sh`, rewrite each prose `case.yaml` from
the case folder with the cut appended in place of the whole text. Keep the cut, and the list of what it left out,
beside the run. [Past the paste size](runs/2026-10-20-923-past-paste-size.md) is a run made this way.

## The full pass and the pilot

The full pass is 22 scored cases, run 3 times each in each of the hook, plugin, prose and none arms: 264 graded
sessions. It runs in 3 batches of one run each, so that each batch fits one window of a subscription's usage limit.
[Combining batches](#combining-batches) says how the batches become one scored run. The pilot is 3 cases, run once
each in each arm: 12 graded sessions. It measures the cost of a session and checks the grading before anyone approves
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

The cases for a corpus live in their own folder under [`cases/`](cases/). Nothing else in the method knows which
corpus it runs against.

| Folder                                     | Corpus                 | Cases                                |
|--------------------------------------------|------------------------|--------------------------------------|
| [`cases/engineering/`](cases/engineering/) | `examples/engineering` | 25 house, and the pilot's 3 controls |
| [`cases/dog-fooding/`](cases/dog-fooding/) | `examples/dog-fooding` | 8 house                              |
| [`cases/library/`](cases/library/)         | `examples/library`     | 6 house                              |
| [`cases/payments/`](cases/payments/)       | `examples/payments`    | 4 house                              |

**A case listed in the folder's `excluded.tsv` is neither run nor scored.** Each line gives the case's path and the
reason, separated by a tab. `stage.sh` leaves the case out of both stages, and `score.sh` leaves its sessions out of
every count. Exclude a case whose result the environment decides, and fix the case before you take it out of the list.
The lists in `engineering`, `library` and `dog-fooding` exclude these:

| Case                                      | Why no session can show the rule                                                         |
|-------------------------------------------|------------------------------------------------------------------------------------------|
| `engineering/house/billing-new-solution`  | The rule copies the .NET runtime's `.editorconfig`, and the sandbox blocks the download. |
| `engineering/house/pagination-fix-commit` | The sandbox blocks the git that `/usr/bin/git` hands off to, so commits fail.            |
| `engineering/house/ci-coverage-threshold` | The rule needs a coverage figure the repository records, and the scaffold records none.  |
| `library/house/notices-down`              | The sandbox blocks the fetch of the record's source, where a session reads `owner`.      |
| `dog-fooding/house/golden-red-in-ci`      | The sandbox stops `dotnet`, which fails to create a NuGet mutex under `/tmp`.            |

| Case                                 | Rule a session breaks                                                                                   |
|--------------------------------------|---------------------------------------------------------------------------------------------------------|
| `house/ci-secret-scan`               | `std-SECRET.NOWHERE`: the new CI runs no secret scanner                             |
| `house/returns-api-routes`           | `std-API.VERSION`: the new routes have no version segment, and the change gives no reason |
| `house/stock-per-warehouse`          | `std-API.VERSION`: the breaking change replaces `/v1`                   |
| `house/stock-v1-retirement`          | `std-API.VERSION`: `/v1` sends no `Deprecation` or `Sunset`, and the change gives no reason |
| `house/ci-uv-locked`                 | `std-DEPS.SOURCE`: CI installs without `--locked`                                   |
| `house/pricing-call`                 | `std-OBS.TRACE`: the outbound call drops the `traceparent`                  |
| `house/request-logging`              | `std-OBS.TRACE`: log lines are unstructured or have no trace id             |
| `house/stock-alert-rules`            | `std-OBS.ALERT`: an alert names no owner or first action                         |
| `house/cover-timeout-pr`             | `std-PR.AGENT`: the PR body omits the agent, model or task          |
| `house/pagination-fix-commit`        | `std-VCS.ATTRIB`: the commit subject lacks `#2231`                   |
| `house/flaky-test-skip`              | `std-GATES.BLOCK`: the skip cites no deviation                               |
| `house/request-a-deviation`          | `std-GATES.BLOCK`: no draft request, or one naming an owner                  |
| `house/ci-every-branch`              | `std-GATES.BUILD`: push builds only some branches              |
| `house/orders-column-rename`         | `std-DEPLOY.WAYBACK`: the migration renames or drops the column        |
| `house/health-endpoint`              | `std-DEPLOY.ONCE`: the health endpoint reports no commit                          |
| `house/ci-coverage-threshold`        | `std-TEST.REACH`: the build has no coverage floor                                 |
| `house/go-service-dockerfile`        | `std-CONT.BASE`: a `FROM` has no digest                                  |
| `house/label-printer-manifest`       | `std-CONT.TAG`: the Deployment names the image by tag                           |
| `house/billing-new-solution`         | `std-CSSTY.ONEFILE`: the project lacks the build's style settings            |
| `house/ca1848-build-break`           | `std-CSSTY.SUPPR`: a pragma, `NoWarn` or an unjustified suppression       |
| `house/order-repository-tests`       | `std-NETTST.INTEG`: Postgres comes from anywhere but Testcontainers, and the change gives no reason |
| `house/dotnet-ci`                    | `std-NETTST.COVER`: CI does not collect and publish coverage, or writes no Cobertura report and gives no reason |
| `house/audit-log-retention`          | `pol-OBSV.AUDIT`: the workspace keeps security telemetry for under a year                               |
| `house/is-deploy-enforced`           | `ctl-0002`: the reply does not cite the control, or does not say nothing enforces the rule              |
| `house/public-path-between-services` | `pol-MEXP.PRIVATE`: the design takes the public path, and neither cites the clause nor gives the reason |
| `control/orders-db-connection`       | `std-SECRET.NOWHERE`: the password lands in a file                                  |
| `control/label-printer-image`        | `std-CONT.MINIMAL`: no non-root `USER`                          |
| `control/stock-reserve-endpoint`     | `std-API.SPEC`: no contract for the new endpoint                         |

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

Run `sh trials/plugin/fixture/test.sh` on a machine before its first run. It runs the scripts over a small fixture with
`jq` as installed, then with a `jq` ending each line with CRLF, as a Windows build does. Go on once it prints
`every case passed`. It needs `jq`, and Node.js for `regrade.mjs`.

## Running the trial

Run each step from the root of this repository, and the commands in the corpus as its `CLAUDE.md` says. `$CORPUS` is
the corpus folder, `$CASES` its case folder, and `$STAGE` an empty folder outside the repository.

1. **Build the plugin.** Commit the cases first, because `kac export` marks an untracked case as a dirty tree. In the
   corpus, run `kac export`, then `kac bundle`. The plugin is `$CORPUS/.dist/plugin/`.
2. **Stage the arms.** `claude plugin eval` reads cases only from a folder below the plugin, so the script copies the
   plugin and the cases into `$STAGE`. `stage.sh` refuses a folder that is not empty, so stage each run into a new one:

   ```sh
   trials/plugin/stage.sh "$CORPUS/.dist/plugin" "$CASES" "$STAGE"
   ```

3. **Count the sessions before you start any.** The plugin stage starts cases × runs × 2 sessions, and the hook and
   prose stages cases × runs each. Count only the cases `stage.sh` staged. An `llm` grader adds three judge calls per
   run. No case here has one.
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

6. **Run the hook arm:**

   ```sh
   cd "$STAGE/hook" && claude plugin eval . --eval-dir evals --ablation none --runs <runs> \
     --model claude-opus-5-5 --scaffold --trust-plugin --no-publish --keep-temp -j 3 \
     --output-dir "$STAGE/out-hook" --allow-tools Bash Write Edit
   ```

7. **Score the run** into a new folder under `runs/`, named by its date:

   ```sh
   trials/plugin/score.sh -k "$STAGE/out-hook/aggregate-result.json" "$STAGE/out-plugin/aggregate-result.json" \
     "$STAGE/out-prose/aggregate-result.json" trials/plugin/runs/<date> "$CASES/excluded.tsv"
   ```

   Leave out `-k` to score a run with no hook arm, and pass `-` for the prose result to score one with no prose arm.
   Every field of the missing arm is then 0 or empty.

8. **Read the verdicts by hand** against the workspace each session left. Read every failing compliance verdict,
   every house case where the hook arm disagrees with the plugin or prose arm, and every pass by a grader you know
   passes weak output.
   Each run prints a `kept temp:` folder, whose workspace is sealed. Run `chmod 700 <folder> <folder>/sealed` to open
   it. The workspace is `<folder>/sealed/home/cwd`, and the trace is `<folder>/out/trace.jsonl`. Run no `git` inside
   the folder.
9. **Record each verdict you read** in `adjudication.tsv` in the run folder, as [Adjudication](#adjudication) says.
   Then score the run again with it:

   ```sh
   trials/plugin/score.sh -a trials/plugin/runs/<date>/adjudication.tsv -k "$STAGE/out-hook/aggregate-result.json" \
     "$STAGE/out-plugin/aggregate-result.json" "$STAGE/out-prose/aggregate-result.json" trials/plugin/runs/<date> \
     "$CASES/excluded.tsv"
   ```

10. **Write what you found** to `runs/<date>.md`: the setup, the results, the cost, and each grader you fixed. Where
   the result file gives no tokens, take them from the `result` line of each trace.

`--no-publish` keeps the report off claude.ai. `--keep-temp` keeps each workspace and trace for step 8. Without it,
both are deleted when the stage finishes.

## Combining batches

Run each batch as steps 4, 5 and 6, with `--runs 1` and its own `--output-dir`. Stage once, and run every batch from
that stage, so every batch has the same plugin and the same prose text. Then merge each stage's result files, and
score the three merged files as step 7:

```sh
trials/plugin/merge.sh "$STAGE/plugin.json" "$STAGE"/out-plugin-r*/aggregate-result.json
trials/plugin/merge.sh "$STAGE/hook.json" "$STAGE"/out-hook-r*/aggregate-result.json
trials/plugin/merge.sh "$STAGE/prose.json" "$STAGE"/out-prose-r*/aggregate-result.json
```

[`merge.sh`](merge.sh) gives each case the runs of every batch, in the order you pass the batches. It sums the cost,
and marks the result partial if any batch was. It refuses batches that differ in stage, model, Claude Code version or
plugin, or that grade a shared case by different graders.

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
* **A grader for a `SHOULD` rule passes a departure that gives its reason.** Add a `--reason` alternative that finds
  the clause's id in what the session wrote, beside the code that departs. A departure with no reason still fails.
  Read each pass the alternative gives, because a session can cite the clause and give no reason.
* **What the case grades travels in the export.** Check that the record type's `export:` block in
  `.schema/<type>.yaml` lists the field or section the case grades. One it leaves out reaches a session only through a
  fetch, and the sandbox blocks the fetch.

The prefix of a grader's file name says what it measures:

| Prefix    | Passes when                                                    | Counts towards |
|-----------|----------------------------------------------------------------|----------------|
| `task-`   | the session did the task it was given                          | task           |
| `comply-` | the files the session wrote comply with the rule               | compliance     |
| `cite-`   | the final reply contains the id of the record the rule sits in | citation       |
| `fired-`  | the session invoked a lookup skill or `ask-the-corpus`         | firing         |

**Graders whose names share the part before `--` are alternatives.** The set passes where one of them does. A file
can arrive through `Write` or through `Bash`, so `comply-contract--written` and `comply-contract--shell` each look for
the contract in one tool's input, and the session complies where either finds it.

`claude plugin eval` scores every grader together, and leaves a `fired-` grader out of a two-arm score. The trial
reads each grader's verdict, and ignores the tool's score.

## Scoring

[`score.sh`](score.sh) reads the plugin result file, the prose result file unless you pass `-` in its place, and the
hook result file where `-k` names one. It writes `sessions.tsv` and `aggregate.md`, and reports each kind of case on its
own. An arm with no result file has no sessions, so its rates and every lead over it are empty.

* **A session complies** where every `comply-` grader, or set of alternatives, passed.
* **A session cites** where every `cite-` grader passed.
* **Compliance is counted over the sessions that did the task**, where every `task-` grader passed. A session that
  did not do the task has written nothing to judge.
* **A session that ended on an error** is counted under `<kind>.<arm>.errors`, and in no rate.
* **A session of an excluded case** is counted nowhere. `excluded_cases` says how many cases were left out.
* **A verdict read by hand replaces the graders' verdict** in every count but the `graders_` fields.

The hook arm's lead over an arm is its compliance rate minus that arm's, on the house cases. Citation is reported
beside it and never added in, because the plugin's skills teach a session to cite and the prose and none arms have
nothing that does. The controls are reported in fields of their own, so a none arm that follows general practice by
default cannot close the gap on the house cases. The plugin arm's lead over an arm is measured the same way. The hook
arm's lead over the plugin arm is what the build index adds to the lookup.

## Adjudication

A grader is a regular expression, so it can fail a file that complies in a form nobody foresaw, or pass a placeholder.
The verdict you read by hand is the result. `adjudication.tsv` has a header line, then one line for each session you
read:

| Column        | Value                                                                                          |
|---------------|------------------------------------------------------------------------------------------------|
| `case`        | the case's folder name                                                                         |
| `arm`         | `plugin`, `hook`, `prose` or `none`                                                            |
| `run`         | the session's place in its arm, from 1. Where batches were merged, batch 2's session is run 2. |
| `graders`     | the graders' compliance verdict: `pass`, `fail`, or `no task` where a `task-` grader failed    |
| `adjudicated` | your verdict, in the same three values                                                         |
| `reason`      | one sentence on what the workspace shows                                                       |

`score.sh -a` refuses a file whose `graders` column differs from the graders' verdict, or whose line matches no
session. Either means the file belongs to another run. Adjudication covers compliance only. A `cite-` grader reads the
final reply for an id, so read its fails, and fix the grader where one is wrong.

When adjudication finds a grader fault, fix the grader for the next run. Prove the fix against the kept workspaces with
`regrade.mjs`. Do not score the recorded run by the fixed grader, because the adjudication is its result.

## The aggregate

`aggregate.md` is a table of fields and values. It contains counts, rates and the model's id, and no free text, so a
run against any corpus can be published in the same shape. A rate is a fraction to two decimal places, and is empty
where it would divide by 0.

`<kind>` is `house` or `control`, and `<arm>` is `plugin`, `hook`, `prose` or `none`. Each `<kind>.<arm>` field is
written for every kind and every arm, zero included.

| Field                                  | Value                                                                                 |
|----------------------------------------|---------------------------------------------------------------------------------------|
| `trial`                                | `plugin`                                                                              |
| `method`                               | the version of this method. It moves when a definition on this page changes.          |
| `model`                                | the model id the sessions ran on                                                      |
| `claude_version`                       | the Claude Code version that ran the plugin stage                                     |
| `cases`                                | the cases scored, of both kinds                                                       |
| `excluded_cases`                       | the cases `excluded.tsv` lists, which no count includes                               |
| `runs_per_case`                        | the runs of each case in each arm                                                     |
| `sessions`                             | the graded sessions, over every kind and arm                                          |
| `adjudicated`                          | the sessions whose compliance verdict was read by hand                                |
| `<kind>.<arm>.sessions`                | the sessions of that kind in that arm                                                 |
| `<kind>.<arm>.errors`                  | of those, the sessions that ended on an error                                         |
| `<kind>.<arm>.task_done`               | of those, the sessions that did the task, by the adjudicated verdict where one exists |
| `<kind>.<arm>.complied`                | of the sessions that did the task, those that comply, by the same verdict             |
| `<kind>.<arm>.compliance_rate`         | `<kind>.<arm>.complied` divided by `<kind>.<arm>.task_done`                           |
| `<kind>.<arm>.graders_task_done`       | of the sessions with no error, those where every `task-` grader passed                |
| `<kind>.<arm>.graders_complied`        | of those, the sessions where every `comply-` grader or set of alternatives passed     |
| `<kind>.<arm>.graders_compliance_rate` | `<kind>.<arm>.graders_complied` divided by `<kind>.<arm>.graders_task_done`           |
| `<kind>.<arm>.overturned`              | the sessions whose adjudicated verdict differs from the graders' verdict              |
| `<kind>.<arm>.cited`                   | the sessions where every `cite-` grader passed                                        |
| `<kind>.<arm>.citation_rate`           | `<kind>.<arm>.cited` divided by `<kind>.<arm>.sessions` less `<kind>.<arm>.errors`    |
| `<kind>.plugin.fired`                  | the plugin sessions of that kind where every `fired-` grader passed                   |
| `<kind>.hook.fired`                    | the hook sessions of that kind where every `fired-` grader passed                     |
| `<kind>.lead_over_prose`               | `<kind>.plugin.compliance_rate` minus `<kind>.prose.compliance_rate`                  |
| `<kind>.lead_over_none`                | `<kind>.plugin.compliance_rate` minus `<kind>.none.compliance_rate`                   |
| `<kind>.graders_lead_over_<arm>`       | the same lead over `prose` or `none`, from the `graders_compliance_rate` fields       |
| `<kind>.hook_lead_over_<arm>`          | `<kind>.hook.compliance_rate` minus that of `plugin`, `prose` or `none`               |
| `<kind>.graders_hook_lead_over_plugin` | the hook arm's lead over the plugin arm, from the `graders_compliance_rate` fields    |
| `cost_usd`                             | the list-price cost every stage reports, judge calls and excluded cases included      |
| `cost_per_session_usd`                 | `cost_usd` divided by `sessions`                                                      |

## Reading the result

**The hook arm's lead answers whether the plugin works**, because the hook arm is the plugin that ships. The plugin
leads an arm where `house.hook_lead_over_<arm>` is above 0. State its lead over the prose and none arms, and give the
citation rates beside them. Give the control fields after them. A control lead near 0 is expected, because every arm
follows general practice.

A run of one session per case is too small to judge a lead by. The pilot reports one so that the full pass can be
priced and its grading checked.

The plugin arm's lead, `house.lead_over_<arm>`, measures the lookup alone. The build index helps where
`house.hook_lead_over_plugin` is above 0. Read it case by case against the scenarios in
[`build.feature`](../use-cases/build.feature), whose thresholds and baselines are per case.

Where the hook arm leads the prose arm, the next measurement is another arm: the corpus's Markdown on disk, with a
`CLAUDE.md` pointing at it.

## Runs

| Run                                                                              | Method | Corpus                                | Sessions | What it was                                                       |
|----------------------------------------------------------------------------------|-------:|---------------------------------------|---------:|-------------------------------------------------------------------|
| [2026-10-16](runs/2026-10-16.md)                                                 |      1 | `examples/engineering`                |        9 | The pilot: 3 controls, once each in each arm                      |
| [Full pass](runs/full-pass.md)                                                   |      4 | `examples/engineering`                |      198 | 19 house cases and 3 controls, 3 runs each                        |
| [Hook arm](runs/hook-arm.md)                                                     |      5 | `examples/engineering`                |       36 | The 4 build cases, 3 runs each in hook, plugin, none              |
| [standards-lookup pilot](runs/2026-10-19-standards-lookup-pilot.md)              |      5 | `examples/engineering`                |       60 | The shipped and the rewritten `standards-lookup`, plugin arm only |
| [#945 firing check](runs/2026-10-20-945-firing.md)                               |      5 | `examples/engineering`                |       22 | The nine other skills rewritten, plugin arm only, 1 run a case    |
| [Past the paste size](runs/2026-10-20-923-past-paste-size.md)                    |      5 | `examples/dog-fooding`                |      132 | The prose arm given a 40,000-character cut, 2 runs a case         |
| [#990 assure cases](runs/2026-10-20-990-assure.md)                               |      5 | `examples/dog-fooding`, `engineering` |        9 | The three assure cases, plugin arm only, 3 runs a case            |
| [A service team's cut](runs/2026-10-20-1016-service-team-cut.md)                 |      5 | `examples/dog-fooding`                |       44 | The prose arm given a cut a service team chose, 2 runs a case     |
| [The ask-the-corpus router](runs/2026-10-20-1026-ask-the-corpus.md)              |      5 | `examples/dog-fooding`                |       12 | The router on 3 cases, plugin arm only, 3 runs a case             |
| [The dog-fooding cut, complete](runs/2026-10-20-1027-dogfooding-cut-complete.md) |      5 | `examples/dog-fooding`                |       44 | #923's cut chosen again from the complete prose text              |
| [#986 design cases](runs/2026-10-20-986-design.md)                               |      5 | all four under `examples/`            |       15 | The five design cases, plugin arm only, 3 runs a case             |
| [#987 build cases](runs/2026-10-21-987-build.md)                                 |      5 | `examples/library`, `dog-fooding`     |       18 | The three build cases, plugin and none arms, 3 runs a case        |
| [#988 operate cases](runs/2026-10-21-988-operate.md)                             |      5 | `payments`, `library`, `dog-fooding`  |       22 | The five operate cases, plugin and none arms, two excluded        |
| [#989 contribute cases](runs/2026-10-21-989-contribute.md)                       |      5 | `engineering`, `library`              |       12 | The two contribute cases, plugin and none arms, 3 runs a case     |
| [The runbook route](runs/2026-10-21-1029-runbook-route.md)                       |      5 | `examples/dog-fooding`                |       21 | Two runbook cases before and after the route, then the pointer    |
