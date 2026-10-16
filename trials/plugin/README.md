# The plugin trial

This trial measures whether the plugin a corpus (one repository of knowledge records) ships changes what an agent
builds. It gives one coding task to three kinds of session. Each task breaks a standard if the session does not know
the rule. It then checks whether the files each session wrote comply with that rule.

Whoever decides whether a corpus ships its plugin, or how the plugin delivers its standards, reads the result.

## The three arms

| Arm    | The session has                                                                      | How it runs                                                           |
|--------|--------------------------------------------------------------------------------------|-----------------------------------------------------------------------|
| plugin | the plugin `kac bundle` builds from the corpus                                       | the with-arm of `claude plugin eval` on the plugin                    |
| prose  | every standard in the export, as one `CLAUDE.md` text, appended to its system prompt | a second run, on a plugin with no components, under `--ablation none` |
| none   | nothing                                                                              | the without-arm of the same run as the plugin arm                     |

The prose arm has the same words as the plugin, so the difference between those two arms is delivery.
[`prose-claude-md.sh`](prose-claude-md.sh) writes the text from the export. It takes each standard's title, status,
`applies-to`, Summary, link, every rule line with its id and obligations, and the Conformance checklist.

`claude plugin eval` loads no `CLAUDE.md` in a run, even one a scaffold writes into the workspace. The prose arm
therefore sends the text through the case's `append_system_prompt`. A `CLAUDE.md` in a real session arrives as
context in the conversation, so the prose arm is close to that delivery and not the same.

## The full pass and the pilot

The full pass is about 20 cases, run 3 times each in each of the 3 arms: about 180 graded sessions. The pilot is 3
cases, run once each in each arm: 9 graded sessions. It measures the cost of a session and proves the grading works
before anyone approves the full pass.

The cases for a corpus live in their own folder under [`cases/`](cases/). [`cases/engineering/`](cases/engineering/)
contains the pilot's cases for `examples/engineering`. Nothing else in the method knows which corpus it runs against.

| Case                     | Standard     | The rule a session breaks                                                       |
|--------------------------|--------------|---------------------------------------------------------------------------------|
| `orders-db-connection`   | `std-SECRET` | `std-SECRET.nowhere-else-holds-a-secret`: the password lands in a file          |
| `label-printer-image`    | `std-CONT`   | `std-CONT.the-container-holds-only-what-the-service-needs`: no non-root `USER`  |
| `stock-reserve-endpoint` | `std-API`    | `std-API.the-contract-is-the-source-of-truth`: no contract for the new endpoint |

## What you need

| Tool                         | Why                                                                   |
|------------------------------|-----------------------------------------------------------------------|
| Claude Code 2.1.292 or later | `claude plugin eval` runs the cases. The pilot ran on 2.1.292.        |
| `kac`                        | `kac export` and `kac bundle` build the plugin from the corpus.       |
| `sh`, `jq`                   | The stage and score scripts are POSIX shell, and read JSON with `jq`. |
| `git`                        | Each scaffold makes its workspace a git repository.                   |

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
   cases × runs. An `llm` grader adds three judge calls per run. No case here has one.
4. **Run the plugin and none arms:**

   ```sh
   cd "$STAGE/plugin" && claude plugin eval . --eval-dir evals --ablation with-without --runs <runs> \
     --model claude-opus-5-5 --scaffold --trust-plugin --no-publish --keep-temp -j 3 \
     --output-dir "$STAGE/out-plugin" --allow-tools Write Edit
   ```

5. **Run the prose arm:**

   ```sh
   cd "$STAGE/prose" && claude plugin eval . --eval-dir evals --ablation none --runs <runs> \
     --model claude-opus-5-5 --scaffold --trust-plugin --no-publish --keep-temp -j 3 \
     --output-dir "$STAGE/out-prose" --allow-tools Write Edit
   ```

6. **Score the run** into a new folder under `runs/`, named by its date:

   ```sh
   trials/plugin/score.sh "$STAGE/out-plugin/aggregate-result.json" "$STAGE/out-prose/aggregate-result.json" \
     trials/plugin/runs/<date>
   ```

7. **Read every verdict by hand** against the files the session wrote. Each run prints a `kept temp:` folder, whose
   workspace is sealed. Run `chmod 700 <folder> <folder>/sealed` to open it. The workspace is
   `<folder>/sealed/home/cwd`, and the trace is `<folder>/out/trace.jsonl`. Run no `git` inside the folder.
8. **Write what you found** to `runs/<date>.md`: the setup, the results, the cost, and any grader you think is wrong.
   Where the result file gives no tokens, take them from the `result` line of each trace.

`--no-publish` keeps the report off claude.ai. Without `--keep-temp`, every workspace and trace is deleted when the
stage finishes, and step 7 cannot be done. Each run is granted `Write` and `Edit` and no `Bash`, so every file a
session writes passes through a tool call a grader can read.

## Writing a case

A case is a folder with a `prompt.md`, a `case.yaml` naming its `scaffold.sh`, and graders under `graders/`, as
`claude plugin eval` reads them.

* **The prompt is a task somebody would type.** It never names the rule, the standard or the corpus.
* **The scaffold builds the repository with heredocs.** A run cannot read the case folder, so the scaffold contains
  every file it writes.
* **A grader reads the files, and never what the session says it did.** It reads a file in the workspace, or the
  input of a `Write` or `Edit` call.

The prefix of a grader's file name says what it measures:

| Prefix    | Passes when                                                      | Counts towards |
|-----------|------------------------------------------------------------------|----------------|
| `task-`   | the session did the task it was given                            | task           |
| `comply-` | the files the session wrote comply with the rule                 | compliance     |
| `cite-`   | the final reply contains the id of the standard the rule sits in | citation       |
| `fired-`  | the session invoked the lookup skill                             | firing         |

`claude plugin eval` scores every grader together, and leaves a `fired-` grader out of a two-arm score. The trial
reads each grader's verdict, and ignores the tool's score.

## Scoring

[`score.sh`](score.sh) reads the two result files and writes `sessions.tsv` and `aggregate.md`.

* **A session complies** where every `comply-` grader passed.
* **A session cites** where every `cite-` grader passed.
* **Compliance is counted over the sessions that did the task**, where every `task-` grader passed. A session that
  did not do the task has written nothing to judge.
* **A session that ended on an error** is counted under `<arm>.errors`, and in no rate.

The plugin's lead over an arm is its compliance rate minus that arm's. Citation is reported beside it and never added
in, because the plugin's skills teach a session to cite and the other arms have nothing that does.

## The aggregate

`aggregate.md` is a table of fields and values. It contains counts, rates and the model's id, and no free text, so a
run against any corpus can be published in the same shape. A rate is a fraction to two decimal places, and is empty
where it would divide by 0.

| Field                   | Value                                                                               |
|-------------------------|-------------------------------------------------------------------------------------|
| `trial`                 | `plugin`                                                                            |
| `method`                | the version of this method. It moves when a definition on this page changes.        |
| `model`                 | the model id the sessions ran on                                                    |
| `claude_version`        | the Claude Code version that ran the plugin stage                                   |
| `cases`                 | the cases run                                                                       |
| `runs_per_case`         | the runs of each case in each arm                                                   |
| `sessions`              | the graded sessions, over every arm                                                 |
| `<arm>.sessions`        | the sessions in that arm. `<arm>` is `plugin`, `prose` or `none`.                   |
| `<arm>.errors`          | of those, the sessions that ended on an error                                       |
| `<arm>.task_done`       | of those, the sessions where every `task-` grader passed                            |
| `<arm>.complied`        | of the sessions that did the task, those where every `comply-` grader passed        |
| `<arm>.compliance_rate` | `<arm>.complied` divided by `<arm>.task_done`                                       |
| `<arm>.cited`           | the sessions where every `cite-` grader passed                                      |
| `<arm>.citation_rate`   | `<arm>.cited` divided by `<arm>.sessions` less `<arm>.errors`                       |
| `plugin.fired`          | the plugin sessions where every `fired-` grader passed                              |
| `lead_over_prose`       | `plugin.compliance_rate` minus `prose.compliance_rate`                              |
| `lead_over_none`        | `plugin.compliance_rate` minus `none.compliance_rate`                               |
| `cost_usd`              | the list-price cost both stages report, judge calls included, to two decimal places |
| `cost_per_session_usd`  | `cost_usd` divided by `sessions`                                                    |

## Reading the result

The plugin leads an arm where its lead over that arm is above 0. State the lead over each arm, and give the citation
rates beside them. A run of one session per case is too small to judge a lead by. The pilot reports one so that the
full pass can be priced and its grading checked.

Where the plugin leads the prose arm, the next measurement is a fourth arm: the corpus's Markdown on disk, with a
`CLAUDE.md` pointing at it.

## Runs

| Run                              | Corpus                 | Sessions | What it was                               |
|----------------------------------|------------------------|---------:|-------------------------------------------|
| [2026-10-16](runs/2026-10-16.md) | `examples/engineering` |        9 | The pilot: 3 cases, once each in each arm |
