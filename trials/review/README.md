# The review trial

This trial measures whether the `standards-review` skill a corpus's plugin ships finds the rules a change breaks. It
asks a session to review a change, and reads the review for the rule's id, the fault, and what the review says about
the control behind the rule. A corpus is one repository of knowledge records, and its plugin is the copy of it that
`kac bundle` packages for Claude Code.

Whoever decides whether the plugin's reviewer is worth shipping reads the result. The scenarios it tests are in
[`review.feature`](../use-cases/review.feature).

## The cases

Each case is a repository with one uncommitted change, and a prompt asking for a review of it. The prompt names no rule
and no record.

| Case                  | The change                                                                                   | What a review should say                                                                                    |
|-----------------------|----------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------|
| `health-endpoint/1-3` | a health endpoint that reports no commit, from the plugin trial's full pass, plugin runs 1-3 | `std-DEPLOY`'s artefact does not record its commit, and `ctl-0002` checks nothing                           |
| `should-departure`    | request logging in London time, with the reason in a comment                                 | `std-OBS`'s SHOULD on UTC is departed from with a reason, so the departure is a judgement call, not a fault |
| `must-in-added-lines` | a fetch from another service, with its API key written into source                           | `std-SECRET`'s MUST NOT on a secret in source is broken, filed under Violations                             |

The `health-endpoint` cases are the three sessions of the [full pass](../plugin/runs/full-pass.md) whose endpoint
reported no commit. Each scaffold writes the plugin trial's scaffold, commits it, then writes the files that session
left, byte for byte, and commits nothing more. So the change is the session's work, uncommitted. The fault there is an
omission the change inherits. In `must-in-added-lines` the change's own lines break the rule.

Every standard `examples/engineering` states is a draft, and the review view reports a finding under a draft standard
as not in force. A review is graded on whether it finds the fault and cites the rule, whatever status it gives.

## Running the trial

Run each step from the root of this repository. `$STAGE` is an empty folder outside it.

1. **Build the plugin.** In `examples/engineering`, run `kac export`, then `kac bundle`.
2. **Stage it.** `claude plugin eval` reads cases only from a folder below the plugin, so the script copies both:

   ```sh
   trials/review/stage.sh examples/engineering/.dist/plugin trials/review/cases/engineering "$STAGE"
   ```

3. **Run the `health-endpoint` cases**, once each:

   ```sh
   cd "$STAGE/plugin" && claude plugin eval . --eval-dir evals --tag std-DEPLOY --ablation none --runs 1 \
     --model claude-opus-5-5 --scaffold --trust-plugin --no-publish --keep-temp -j 1 \
     --output-dir "$STAGE/out-health" --allow-tools Bash
   ```

4. **Run `should-departure` and `must-in-added-lines`** three times each, one command per case:

   ```sh
   cd "$STAGE/plugin" && claude plugin eval . --eval-dir evals --case should-departure --ablation none --runs 3 \
     --model claude-opus-5-5 --scaffold --trust-plugin --no-publish --keep-temp -j 1 \
     --output-dir "$STAGE/out-should" --allow-tools Bash
   ```

   For the second case, pass `--case must-in-added-lines` and `--output-dir "$STAGE/out-must"`.

5. **Read every review by hand**, as [Adjudication](#adjudication) says.
6. **Write the run** to `runs/<date>.md`, with each result file under `runs/<date>/results/`. A second run on one day
   takes `<date>-2`.

There is one arm. A session with no plugin has no rule ids to cite, so a baseline would measure nothing. Nine sessions
make a run.

`git` does not run inside the eval sandbox on macOS, because the sandbox blocks the `xcrun` shim it hands off to. The
skill names no tool for reading the change, so a session finds another way, and the trace shows which.

## Grading

A grader reads the session's final reply, and the prefix of its file name says what it looks for:

| Prefix       | Passes when the final reply                                                  |
|--------------|------------------------------------------------------------------------------|
| `cite-`      | contains the id of the standard the fault is under                           |
| `finds-`     | says the change does not report or record the commit                         |
| `unchecked-` | says no automated check covers the rule, or names the `not-enforced` control |
| `judgement-` | calls the departure a judgement call                                         |
| `quotes-`    | quotes the reason the change gives                                           |
| `filed-`     | names the rule after a Violations heading and before a Judgement calls one   |
| `fired-`     | the session invoked `standards-review`                                       |

A grader is a regular expression, so it cannot tell a finding filed as a fault from one filed as a judgement call.
Adjudication can.

## Adjudication

Read two texts for each session. The skill runs as a forked subagent, so its report reaches the session as the result
of the `Skill` call in `trace.jsonl`, and the final reply is the session's account of that report. A finding can be in
the report and gone from the reply.

For each scenario in `review.feature`, give each session a verdict on the report and on the reply, with one sentence
saying what the text shows. The verdict on the reply is the result, because the reply is what the person asking reads.

Each run prints a `kept temp:` folder. Its trace is `<folder>/out/trace.jsonl`, and it needs no `chmod` to read.

## Runs

| Run                                  | Corpus                        | Sessions | What it was                                                          |
|--------------------------------------|-------------------------------|---------:|----------------------------------------------------------------------|
| [2026-10-19](runs/2026-10-19.md)     | `examples/engineering` 0.36.3 |        6 | The first run: 3 `health-endpoint`, 3 `should-departure`             |
| [2026-10-19-2](runs/2026-10-19-2.md) | `examples/engineering` 0.36.3 |        9 | The skill relaying its report whole, and `must-in-added-lines` added |
