# Hook arm

**The build-index hook closes the gap #929 found on `pricing-call`, and loses nothing on the other three build cases.**
With the hook, the session forwarded the traceparent in 3 of 3 runs. With the lookup alone it did so in 1 of 3, and
with no plugin in 0 of 3. Across the four cases, the hook arm complied in every session that did the task (9 of 9),
the plugin arm in 7 of 9 and the none arm in 3 of 12.

The run measured the hook from [#943](https://github.com/paul80nd/knowledge-as-code/issues/943) on the four cases
[`build.feature`](../../use-cases/build.feature) names: 36 graded sessions, 3 runs of each case in each of 3 arms. The
hook pushes the build index, the list of every standard rule by title, once a session.

## Setup

| Item          | Value                                                                                      |
|---------------|--------------------------------------------------------------------------------------------|
| Corpus        | `examples/engineering` at `d2bc961d`, content-version 0.36.2                               |
| Plugin        | `example-engineering` 0.36.2, from `kac export` and `kac bundle` on that commit            |
| Cases         | `health-endpoint`, `pricing-call`, `audit-log-retention` and `flaky-test-skip`             |
| Arms          | hook, plugin and none. The prose arm did not run.                                          |
| Hook matcher  | `Read\|Grep\|Glob`                                                                         |
| Model         | `claude-opus-5-5` for every session                                                        |
| Claude Code   | 2.1.292                                                                                    |
| Runs          | 3 per case per arm, at `-j 1`                                                              |
| Tools granted | `Bash`, `Write` and `Edit`, beside the read-only tools each case lists                     |
| Output        | [`hook-arm/`](hook-arm/): `aggregate.md`, `sessions.tsv` and both results                  |

One probe session of `health-endpoint` in the hook arm ran first. It showed the build index arriving once, after the
session's first `Glob`, and the marker written inside the eval sandbox. It is not in the scores.

## Results

| Case                  | Hook | Plugin | None | `build.feature` threshold |
|-----------------------|-----:|-------:|-----:|---------------------------|
| `pricing-call`        |  3/3 |    1/3 |  0/3 | at least 2 of 3           |
| `health-endpoint`     |  3/3 |    3/3 |  0/3 | at least 2 of 3           |
| `audit-log-retention` |  3/3 |    3/3 |  3/3 | 3 of 3                    |
| `flaky-test-skip`     |  3/3 |    3/3 |  0/3 | 3 of 3                    |

Each cell counts the sessions that complied. In `flaky-test-skip`, the plugin and hook sessions asked before they
skipped the test, which the case counts as complying, and changed no file. The hook arm cited the rule in 11 of 12
sessions, the plugin arm in 10 of 12.

The scores are the graders', and no session was adjudicated. The three hook sessions of `pricing-call` were read by
hand: each forwards the inbound `traceparent` in `orders/app.py` and adds a test for it.

## What the plugin missed

**`pricing-call`, runs 1 and 2.** Each session searched the corpus for timeouts, retries, outbound calls and contracts.
It found `std-API`, `std-TEST` and `std-DEPLOY`, and never `std-OBS`, the same miss
[the full pass](full-pass.md#what-the-plugin-missed) recorded. The run that complied listed every rule title and picked
out "One request reads as one timeline". With the hook, every session had that list before it wrote the call, and every
run forwarded the traceparent.

**`health-endpoint` no longer misses.** On 0.29.0 the plugin arm complied in 0 of 3 runs. On 0.36.2 it complied in 3 of
3, with no hook. The corpus and the lookup skills changed between the two, so this case no longer separates the arms.

## How the build index arrived

Ten of the 12 hook sessions received the build index once, before their first write. In each of them the hook fired on
the session's second tool, the first `Read`, `Grep` or `Glob`, straight after a `Bash` listing of the repository.

The other two read the repository through `Bash` alone, so the hook never fired:

* A `flaky-test-skip` session ran `Bash` and the lookup, changed no file, and asked before skipping.
* An `audit-log-retention` session ran `Bash` and the lookup, then wrote with `Write`.

Both complied through the lookup. The matcher now includes `Write`, `Edit` and `NotebookEdit` as well, so a session
reading through `Bash` receives the index straight after its first edit. That change came after this run, and no run
has measured it.

`claude plugin eval` leaves a hook's context out of `trace.jsonl`. Each session's own transcript, in the folder
`--keep-temp` kept, shows it as an attachment of type `hook_additional_context`.

## Cost

| Arm    | Sessions | List cost | Per session | Mean turns |
|--------|---------:|----------:|------------:|-----------:|
| hook   |       12 |     $3.84 |       $0.32 |       13.9 |
| plugin |       12 |     $3.72 |       $0.31 |       14.9 |
| none   |       12 |     $2.11 |       $0.18 |       11.0 |

The hook adds about $0.01 a session over the plugin arm. The build index is 2,983 characters.

## What the result shows

The build index gets a rule to the session where the lookup did not find it. On `pricing-call`, the task names neither
tracing nor the rule, and the index put `std-OBS` in front of the session before it wrote the call.

## What the result does not show

* **Whether the index or the lookup did the work in the other three cases.** The plugin arm complied there too.
* **How the index scales.** Engineering's index is 2,983 characters. Dog-fooding's is 5,544, and a large estate's
  passes the 10,000-character cap. #993 holds scoping the index by path.
* **Whether the rule titles matter.** `std-OBS`'s rule reads "One request reads as one timeline", and it fired. No
  title was sharpened for this run.
* **Anything about the prose arm.** It did not run. The full pass's prose figures were measured on 0.29.0.

Three runs a case is a small sample, and 1 of 3 against 3 of 3 rests on two sessions.
