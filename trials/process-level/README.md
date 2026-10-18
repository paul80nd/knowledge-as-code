# The process-level trial

This trial measures whether a process (a record of a planned task) written at the process level draws fewer findings
than one written step by step. A step at the process level states an outcome and its reason, and leaves the means to
the reader's judgement. `## Level of detail and cadence` on
[`processes.md`](../../examples/dog-fooding/processes.md#level-of-detail-and-cadence) states the level.

The reader of the result is whoever decides whether to rewrite the other processes in
[`examples/dog-fooding/processes/`](../../examples/dog-fooding/processes/) the same way.

## The pilot

[`prc-change-the-schema`](../../examples/dog-fooding/processes/change-the-schema.md) was rewritten at the process level
for #934. Its 12 steps became 7 outcomes. The detail it dropped was deleted where `.schema/CLAUDE.md`, root `CLAUDE.md`
or `docs/maintaining/` already states it, and moved to `tooling/CLAUDE.md` where nothing did. [The
verdicts](runs/2026-10-18/verdicts.md) say why this process was chosen, and what the review found for every other
process and playbook.

The risk the trial watches is the cost of that cut: a session that no longer reads a detail in the process may skip the
obligation behind it.

## Running the trial

Take one run after five sessions have followed `prc-change-the-schema` since the pilot merged. A session is one merged
pull request that changes a file under `.schema/`, because a session leaves no record of the process it followed. Run
each step from the root of the repository.

1. **List the sessions.** Write down the first five pull requests after the pilot's merge commit:

   ```sh
   git log --first-parent --reverse --format='%h %ad %s' --date=short <pilot merge>..main -- .schema/
   ```

2. **List the issues that cite the process** since the pilot merged, whatever their labels. A finding filed before
   `raise-finding` existed has no `kac:finding` label. Save the output as `issues.json` in the run's folder:

   ```sh
   gh issue list --state all --limit 300 --search "prc-change-the-schema created:>=<merge date>" \
     --json number,title,createdAt,labels
   ```

3. **Keep the findings.** Read each body. Keep an issue that reports what the process said or left out, including one
   that cites it in passing. Drop one that cites it only as context for other work.

4. **Triage each finding** with step 5 of
   [`harvest-findings`](../../.claude/skills/harvest-findings/SKILL.md#5-read-the-level-of-detail), and record its
   verdict as step 5 defines it. Then record whether the finding asks for a step, and whether the record it drafts adds
   one.

5. **Count the misses** among the findings. A miss is a finding reporting something a session's change owed and did
   not do, where a step of the process, or of a process it runs, stated that obligation. Count from the findings alone,
   as the baseline was, so the two compare.

6. **Write the run** to `runs/<date>/`, with a table in the shape of the baseline's.

## Scoring

The baseline is [the run of 2026-10-18](runs/2026-10-18/baseline.md), taken from the tracker before the pilot merged.

| Measure                    | Baseline                                    | A later run passes where            |
|----------------------------|---------------------------------------------|-------------------------------------|
| Findings asking for a step | 8 of 10, across 102 PRs touching `.schema/` | no finding adds a step after triage |
| Misses                     | 1, across the same 102 PRs                  | no miss in the five sessions        |

A finding that triage narrows refines a step the process already has, so it does not count against the pilot. One miss
in five sessions is above the baseline rate, so the pass rule allows none.

A pass is the evidence the follow-up story waits on before it rewrites the other processes. A fail names the step that
cost a session, and the run says what that step lost in the rewrite.
