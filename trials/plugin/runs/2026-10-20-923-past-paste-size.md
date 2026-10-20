# Past the paste size

**The plugin leads a 40,000-character `CLAUDE.md` by 0.45 on house compliance, and the whole lead comes from the rules
the cut left out.** Read by hand, the plugin arm complied in 31 of 38 house sessions and the prose arm in 14. On the
rules the cut kept, the prose arm complied in 12 of 14 and the plugin arm in 10. On the rules it left out, the prose arm
complied in 2 of 24, and the none arm in 3.

The run answers [story #923](https://github.com/paul80nd/knowledge-as-code/issues/923). The
[full pass](full-pass.md) found no plugin lead over a prose arm given the whole export of `examples/engineering`. This
run gives the prose arm what a team writing a `CLAUDE.md` could fit, from a corpus stack it could not fit whole.

## Choosing the corpus

No corpus here is too large for a context window. The largest stack is `examples/dog-fooding` over the
`examples/engineering` layer it consumes, and its prose text is about 60,000 tokens.

| Plugin                     | Words  | Characters | Tokens, about |
|----------------------------|-------:|-----------:|--------------:|
| `library`                  |  8,153 |     61,077 |        15,000 |
| `payments`, with `eng`     | 15,638 |    110,534 |        28,000 |
| `engineering`              | 21,905 |    157,333 |        39,000 |
| `dog-fooding`, with `eng`  | 33,757 |    238,622 |        60,000 |

Each count is of the text `prose-claude-md.sh` writes from the plugin. Tokens are characters divided by 4.

The limit a team meets first is Claude Code's warning on a `CLAUDE.md` over 40,000 characters. The run takes that as
the size the prose arm can hold. The `dog-fooding` stack is 6 times that size. It also contains every `eng` record, so
the 22 engineering cases run against it unchanged.

## Choosing the cut

A subagent chose the prose arm's 40,000 characters from the whole prose text, briefed as the team that owns
`examples/dog-fooding`. It read only that text, and saw no case. It copied record blocks verbatim, and could drop a
type, a record, a part or a field. The cut is [`prose-cut.md`](2026-10-20-923-past-paste-size/prose-cut.md), at 39,929
characters, and [`left-out.md`](2026-10-20-923-past-paste-size/left-out.md) lists every record and what it kept.

| Kept                                                                    | Left out                                                                    |
|-------------------------------------------------------------------------|-----------------------------------------------------------------------------|
| this corpus's 6 standards, `std-PLUGIN` in part                         | the `eng` standards API, CONT, DEPLOY, INCSEV, OBS and SECRET               |
| the `eng` standards CSSTY, DEPS, GATES, NETTST, PR, TEST and VCS        | every policy, tool, service, process, runbook, report, ADR and glossary     |
| 9 controls in part, 1 deviation in part, 1 fix in part                  | every other control, deviation and fix                                      |

The cut fits the repository the team works in, where nothing runs a service. It also leaves out the rules of 12 of the
19 house cases. The cases test a service estate, so a cut chosen for `examples/engineering` alone would keep more of
them.

## Setup

| Item          | Value                                                                                                     |
|---------------|-----------------------------------------------------------------------------------------------------------|
| Corpus        | `examples/dog-fooding` at content-version 0.71.1, consuming `examples/engineering`, at `370bec1c`        |
| Plugin        | `example-dogfooding` 0.71.1, from one `kac export` and `kac bundle`                                       |
| Prose text    | [`prose-cut.md`](2026-10-20-923-past-paste-size/prose-cut.md), in place of the whole text `stage.sh` writes |
| Cases         | 19 house and 3 control in [`cases/engineering/`](../cases/engineering/), less `excluded.tsv`              |
| Hook          | silent. No hook arm ran                                                                                   |
| Model         | `claude-opus-5-5` for every session                                                                       |
| Claude Code   | 2.1.304                                                                                                   |
| Runs          | 2 a case, as 2 batches of 1, each stage at `-j 3`                                                         |
| Tools granted | `Bash`, `Write` and `Edit`, beside the read-only tools each case lists                                    |
| Output        | [`2026-10-20-923-past-paste-size/`](2026-10-20-923-past-paste-size/)                                      |

`stage.sh` appends the whole prose text to each prose case. After staging, each prose `case.yaml` was rewritten from
the case folder with the cut appended in its place. A third batch was not run, to leave room in the usage window for the
reading by hand.

## Results

| Measure                         | plugin       | prose (cut)  | none        |
|---------------------------------|--------------|--------------|-------------|
| House compliance, by hand       | 0.82 (31/38) | 0.37 (14/38) | 0.24 (9/38) |
| on the rules the cut kept       | 0.71 (10/14) | 0.86 (12/14) | 0.43 (6/14) |
| on the rules the cut left out   | 0.88 (21/24) | 0.08 (2/24)  | 0.13 (3/24) |
| House citation                  | 0.82 (31/38) | 0.16 (6/38)  | 0.00 (0/38) |
| Controls complied               | 6/6          | 4/6          | 4/6         |

| Lead            | By hand | Graders |
|-----------------|--------:|--------:|
| over the prose  |    0.45 |    0.44 |
| over none       |    0.58 |    0.57 |

A lookup fired in every plugin session. No session ended on an error.

## Adjudication

89 verdicts were read by hand: every failing compliance verdict, both sides of every case and run where the plugin and
prose arms differ, and every `stock-alert-rules` pass. One overturned the graders.

| Case                     | Arm    | Run | Graders | By hand | What the workspace shows                                                  |
|--------------------------|--------|----:|---------|---------|---------------------------------------------------------------------------|
| `order-repository-tests` | plugin |   2 | no task | pass    | the tests and a Testcontainers fixture arrived through a Bash heredoc     |
| `dotnet-ci`              | plugin |   1 | fail    | fail    | the line rate reaches the job summary, and the Cobertura report does not  |

The `dotnet-ci` session was read as a pass first. The rule asks the build to publish the report, and a figure in the
summary is not the report, so it stays a fail. The other calls follow [the full pass](full-pass.md#adjudication).

* A base image taken from an `ARG` with no default fails.
* A placeholder digest passes, and so does a placeholder alert owner beside a real first action.
* A skip citing a deviation that is pending, with none recorded, fails.

## Grader faults

* **`order-repository-tests`**: every `task-tests` alternative read `Edit` or `Write` input only, so a session writing
  the tests through `Bash` scored `no task`. Fixed for the next run by `task-tests--shell`. Regraded over batch 2's
  plugin stage, it passes the overturned session, and the 234 unchanged verdicts agree with the tool.
* **`request-logging`**: `comply-*--written` matches `json` or `trace` anywhere in a `Write` input, so a README
  mentioning either passes. Both plugin passes in this run write structured lines with the trace id.

## What the result shows

**A team's `CLAUDE.md` leaves rules out, and a session cannot follow a rule its text leaves out.** The prose arm
complied with a dropped rule no more often than a session holding nothing. The plugin reached those rules through its
lookup, because the export it searches is whole.

**What the prose arm holds, it delivers as well as the plugin.** On the 7 cases the cut kept, it led the plugin by 2
sessions in 14. This matches the full pass, where the prose arm held everything.

## What the result does not show

* **What a different cut would score.** The cut decides the prose arm's result. A cut chosen for a service team would
  keep the API, OBS and CONT standards, and lose others in their place.
* **A corpus past the context window.** None exists here. The adopting corpus is the next place to measure that.
* **A judgement made blind.** The reader knew each session's arm.
* **The variance of a third run.** Each case ran twice in each arm.

## Cost

| Stage                    | Sessions | Wall clock | List cost |
|--------------------------|---------:|-----------:|----------:|
| plugin and none, batch 1 |       44 |      851 s |    $12.73 |
| prose, batch 1           |       22 |      384 s |     $7.62 |
| plugin and none, batch 2 |       44 |      768 s |    $12.31 |
| prose, batch 2           |       22 |      365 s |     $7.64 |
| Total                    |      132 |            |    $40.30 |

Each batch ran its 2 stages side by side, in about 14 minutes. The cut and the reading by hand ran as subagents, which
the list cost does not include.
