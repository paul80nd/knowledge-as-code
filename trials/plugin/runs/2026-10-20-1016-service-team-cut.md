# A service team's cut

**The plugin has no lead over a 40,000-character `CLAUDE.md` chosen by a team that builds and runs services.** Read by
hand, the plugin arm and the prose arm each complied in 31 of 38 house sessions. The cut kept the rule of 18 of the 19
house cases, and on those the prose arm complied in 31 of 36 and the plugin arm in 29.

The run answers [story #1016](https://github.com/paul80nd/knowledge-as-code/issues/1016).
[Past the paste size](2026-10-20-923-past-paste-size.md) gave the prose arm a cut chosen by the team that owns
`examples/dog-fooding`, and the plugin led it by 0.45. This run changes only the cut, so the two leads compare what
each team chose to keep.

## The prose text was incomplete before this run

`prose-claude-md.sh` paired a part with its record by id. A record that a layer brings in has a plain id, such as
`std-OBS`, and its parts name it with the layer's prefix, `eng:std-OBS`. So the script wrote no part of any consumed
record. Over the `dog-fooding` stack it dropped 350 parts:

| Type      | Parts dropped | What they are                     |
|-----------|--------------:|-----------------------------------|
| standards |            41 | every rule of the `eng` standards |
| policies  |           276 | every clause of every policy      |
| glossary  |            33 | every term                        |

The script now pairs a part under its layer's prefix, and fails where the parts it wrote differ in number from the
parts the export's manifest counts. Over #923's export the text grows from 238,622 characters to 312,470, and keeps
every line it had.

[Past the paste size](2026-10-20-923-past-paste-size.md) ran on the incomplete text, so its prose arm had each `eng`
standard as a summary and a checklist, with none of its rules. This run gives the prose arm the full text to cut from.

## Choosing the cut

A subagent chose the cut from the full text, briefed as a team that builds and runs services in a repository of its
own. It read only that text, and saw no case. It copied record blocks verbatim, as for #923. The cut is
[`prose-cut.md`](2026-10-20-1016-service-team-cut/prose-cut.md), at 39,485 characters, and
[`left-out.md`](2026-10-20-1016-service-team-cut/left-out.md) lists every record and what it kept.

| Kept                                                                                                           | Left out                                                          |
|----------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------|
| every rule of the `eng` standards API, CONT, CSSTY, DEPLOY, DEPS, GATES, NETTST, OBS, PR, SECRET, TEST and VCS | every standard's conformance checklist                            |
| `std-INCSEV.every-incident-has-a-severity`                                                                     | this corpus's 6 standards                                         |
| 77 clauses from 23 of the 24 policies, chosen where no kept rule covers them                                   | `pol-A11Y`, `pol-COST` and `pol-EVER`, and every other clause     |
| 8 glossary terms                                                                                               | every control, deviation, fix, process, runbook, service and tool |

The cut kept the rule of 18 of the 19 house cases. It left out `pol-OBSV.AUDIT`, which `audit-log-retention` tests.

## Setup

| Item          | Value                                                                                                            |
|---------------|------------------------------------------------------------------------------------------------------------------|
| Corpus        | `examples/dog-fooding` at content-version 0.71.1, consuming `examples/engineering`, at `370bec1c`                |
| Plugin arm    | [Past the paste size](2026-10-20-923-past-paste-size.md)'s, scored again with its verdicts read by hand          |
| None arm      | the same, from the same stage                                                                                    |
| Prose text    | [`prose-cut.md`](2026-10-20-1016-service-team-cut/prose-cut.md), cut from the text the fixed script writes       |
| Cases         | 19 house and 3 control in [`cases/engineering/`](../cases/engineering/), less `excluded.tsv`, as staged for #923 |
| Model         | `claude-opus-5-5` for every session                                                                              |
| Claude Code   | 2.1.304                                                                                                          |
| Runs          | 2 a case in the prose arm, as 2 batches of 1 run side by side, each at `-j 3`                                    |
| Tools granted | `Bash`, `Write` and `Edit`, beside the read-only tools each case lists                                           |
| Output        | [`2026-10-20-1016-service-team-cut/`](2026-10-20-1016-service-team-cut/)                                         |

The prose stage is a copy of #923's, with each prose `case.yaml` rewritten from the case folder with this cut appended.

## Results

| Measure                      | plugin       | prose (cut)  | none        |
|------------------------------|--------------|--------------|-------------|
| House compliance, by hand    | 0.82 (31/38) | 0.82 (31/38) | 0.24 (9/38) |
| on the rules the cut kept    | 0.81 (29/36) | 0.86 (31/36) | 0.22 (8/36) |
| on the rule the cut left out | 2/2          | 0/2          | 1/2         |
| House citation               | 0.82 (31/38) | 0.26 (10/38) | 0.00 (0/38) |
| Controls complied            | 6/6          | 5/6          | 4/6         |

| Lead           | This run, by hand | #923, by hand | This run, graders |
|----------------|------------------:|--------------:|------------------:|
| over the prose |              0.00 |          0.45 |             -0.01 |
| over none      |              0.58 |          0.58 |              0.57 |

No session ended on an error.

| Case                  | plugin | prose | none | What decided it                                                                                                     |
|-----------------------|-------:|------:|-----:|---------------------------------------------------------------------------------------------------------------------|
| `audit-log-retention` |    2/2 |   0/2 |  1/2 | the cut left the rule out                                                                                           |
| `health-endpoint`     |    2/2 |   0/2 |  0/2 | the cut kept the rule, and left out the `eng` control that applies it to a health endpoint                          |
| `dotnet-ci`           |    0/2 |   2/2 |  0/2 | both prose sessions upload the Cobertura file. One plugin session publishes only a figure, and one collects nothing |
| `ci-secret-scan`      |    1/2 |   2/2 |  0/2 | one plugin session ran no scanner                                                                                   |
| `pricing-call`        |    1/2 |   2/2 |  0/2 | one plugin session dropped the `traceparent`                                                                        |

Every other house case scored the same in the plugin and prose arms.

## Adjudication

The plugin and none verdicts are #923's, read again from its
[`adjudication.tsv`](2026-10-20-923-past-paste-size/adjudication.tsv). 16 prose verdicts were read by hand: every
failing compliance verdict, the prose side of every case where the arms differ, and every pass by the
`request-logging` and `stock-alert-rules` graders. None overturned the graders.

`flaky-test-skip` run 1 was the one borderline call. The skip cites a deviation file the session wrote itself, which
gives the user's address as accepting the risk. Nobody accepted it, so it is a pending deviation, and it fails as #923
ruled. The other calls follow [the full pass](full-pass.md#adjudication).

## What the result shows

**What a cut keeps decides the prose arm's result.** The service team kept the rules the cases test, and the prose arm
matched the plugin. The `dog-fooding` team left them out, and the prose arm matched the none arm on them. The plugin
scored the same against both.

**A rule in the text is not always applied.** The prose arm had the rule that an artefact records its commit, and
neither session made the health endpoint report it. Both plugin sessions did, from a commit the Dockerfile bakes in.

## What the result does not show

* **Whether a team would choose this way.** One subagent chose the cut, once, knowing the team's work.
* **#923's lead on a complete text.** Its prose arm had no `eng` rule text.
  [The dog-fooding team's cut, complete](2026-10-20-1027-dogfooding-cut-complete.md) measured it at 0.47.
* **A corpus past the context window.** None exists here.
* **A judgement made blind.** The reader knew each session's arm.

## Cost

| Stage          | Sessions | Wall clock | List cost |
|----------------|---------:|-----------:|----------:|
| prose, batch 1 |       22 |      440 s |     $8.08 |
| prose, batch 2 |       22 |      425 s |     $8.11 |
| Total          |       44 |            |    $16.19 |

The batches ran side by side, in about 7 minutes. `cost_usd` in
[`aggregate.md`](2026-10-20-1016-service-team-cut/aggregate.md) is $41.23, because it includes #923's plugin stage.
The cut and the reading by hand ran as subagents, which the list cost does not include.
