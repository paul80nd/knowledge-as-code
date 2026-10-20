# The dog-fooding team's cut, complete

**The plugin leads a 40,000-character `CLAUDE.md` chosen by the team that owns `examples/dog-fooding` by 0.47, on a
prose text that is now complete.** [Past the paste size](2026-10-20-923-past-paste-size.md) measured 0.45 on a text
missing every `eng` rule, clause and term. So that lead came from what the team left out, and not from the missing
text.

The run answers [story #1027](https://github.com/paul80nd/knowledge-as-code/issues/1027). It repeats #923 with one
change: the cut is chosen from the text the fixed `prose-claude-md.sh` writes, as
[a service team's cut](2026-10-20-1016-service-team-cut.md) was.

## Choosing the cut

A subagent chose the cut from the full text, with #923's brief: the team that owns this repository, which builds
`kac`, its documentation site, its plugin and its example corpora, and runs no production service. It read only that
text, and saw no case. The cut is [`prose-cut.md`](2026-10-20-1027-dogfooding-cut-complete/prose-cut.md), at 39,739
characters, and [`left-out.md`](2026-10-20-1027-dogfooding-cut-complete/left-out.md) lists every record and what it
kept.

| Kept                                                                           | Left out                                                                 |
|--------------------------------------------------------------------------------|--------------------------------------------------------------------------|
| every rule of `std-CI`, `std-CONFIG` and `std-PROSE`, and both of `std-GATES`  | the `eng` standards API, CONT, DEPLOY, DEPS, INCSEV, OBS, SECRET and VCS |
| some rules of `std-VERS`, `std-PLUGIN`, `std-A11Y`, CSSTY, NETTST, PR and TEST | every policy, control, deviation, tool, glossary term and ADR            |
| both fixes, 4 processes, 2 runbook symptoms, and part of `svc-kac`             | every standard's conformance checklist                                   |

The cut kept the rule of 5 of the 19 house cases: `cover-timeout-pr`, `flaky-test-skip`, `ci-every-branch`,
`ca1848-build-break` and `dotnet-ci`. #923's cut kept 7.

## Setup

As [a service team's cut](2026-10-20-1016-service-team-cut.md#setup): #923's plugin and none arms and their verdicts,
and a copy of #923's prose stage with each prose `case.yaml` rewritten with this cut. The prose arm ran 2 batches of 1
run side by side, each at `-j 3`, on `claude-opus-5-5` under Claude Code 2.1.304.

## Results

| Measure                       | plugin       | prose (cut)  | none        |
|-------------------------------|--------------|--------------|-------------|
| House compliance, by hand     | 0.82 (31/38) | 0.35 (13/37) | 0.24 (9/38) |
| on the rules the cut kept     | 6/10         | 8/9          | 2/10        |
| on the rules the cut left out | 25/28        | 5/28         | 7/28        |
| House citation                | 0.82 (31/38) | 0.08 (3/37)  | 0.00 (0/38) |
| Controls complied             | 6/6          | 4/6          | 4/6         |

| Cut                                                         | Prose text | Cases kept | Lead over prose |
|-------------------------------------------------------------|------------|-----------:|----------------:|
| [the dog-fooding team's](2026-10-20-923-past-paste-size.md) | incomplete |          7 |            0.45 |
| the dog-fooding team's, this run                            | complete   |          5 |            0.47 |
| [a service team's](2026-10-20-1016-service-team-cut.md)     | complete   |         18 |            0.00 |

The lead over none is 0.58, as in both earlier runs. By the graders, the lead over prose is 0.46. No session ended on
an error. One prose session did no task.

The plugin and prose arms differ on 13 house cases. On 12 of them the cut left the rule out, and the prose arm complied
in 1 of 24 sessions. The 13th is `dotnet-ci`, whose rule the cut kept: both prose sessions upload the coverage report,
and neither plugin session does.

## Adjudication

The plugin and none verdicts are #923's. 29 prose verdicts were read by hand: every failing or no-task verdict, and
every session where the prose arm and the plugin arm differ. None overturned the graders. The calls follow
[the full pass](full-pass.md#adjudication).

* `label-printer-manifest` writes `:IMAGE_TAG` and `:CHANGE-ME`, which are placeholders for a tag. A placeholder digest
  passes and a placeholder tag fails.
* `flaky-test-skip` run 1 ran the test 40 times and changed no file, so it did no task.

## What the result shows

**A team's `CLAUDE.md` decides the prose arm's result by what it keeps, and the text being complete does not move it.**
On the rules this cut kept, the prose arm complied in 8 of 9 sessions. On the rules it left out, it complied in 5 of 28,
and the none arm in 7.

## What the result does not show

* **Whether a team would choose this way.** One subagent chose each cut, once.
* **A corpus past the context window.** None exists here.
* **A judgement made blind.** The reader knew each session's arm.

## Cost

| Stage          | Sessions | Wall clock | List cost |
|----------------|---------:|-----------:|----------:|
| prose, batch 1 |       22 |      428 s |     $7.63 |
| prose, batch 2 |       22 |      368 s |     $7.56 |
| Total          |       44 |            |    $15.19 |

The batches ran side by side, in about 7 minutes. `cost_usd` in
[`aggregate.md`](2026-10-20-1027-dogfooding-cut-complete/aggregate.md) includes #923's plugin stage. The cut and the
reading by hand ran as subagents, which the list cost does not include.
