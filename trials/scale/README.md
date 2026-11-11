# The scale trial

This trial measures what `kac` costs on a corpus (one repository of knowledge records) the size of an estate of eight
applications. It times `validate`, `export` and `bundle`, and measures what `export` and `bundle` write. Its second
half asks a session questions about that corpus through the plugin, through the whole corpus as one prose text in its
context, and with nothing. It shows what each costs per answer, and whether either loses answers at that size.

Whoever decides whether `kac` and its plugin hold up at an adopting estate's size reads the result. The run also reads
the build index against the 8,000 characters at which #1034 starts.

## The corpus

[`generate.mjs`](generate.mjs) writes a corpus of 1 to 8 applications from the records under `examples/`. Nothing it
writes is committed. The size comes from the 2a trial corpus, as #1017 read it: about 115,000 characters of prose for
each application, on top of about 256,000 that do not grow with the applications.

Each application gets a copy of every record of these types:

| Type         | Copied from                                   |
|--------------|-----------------------------------------------|
| services     | `examples/payments`, `library`, `dog-fooding` |
| integrations | `examples/library`                            |
| data         | `examples/library`                            |
| nfrs         | `examples/payments`                           |
| offerings    | `examples/payments`                           |
| runbooks     | `examples/dog-fooding`                        |
| fixes        | `examples/payments`, `dog-fooding`            |
| postmortems  | `examples/payments`                           |
| deviations   | `examples/dog-fooding`                        |

A copy takes its application's name as the first word of its slug, as `svc-cedar-payment-api`. A postmortem is numbered,
and counts on from the application before, as `pmt-0007`. Every id a copy cites in its own application is rewritten to
that application's copy, and so are its links, its repositories and its `owner`. Its H1 ends with the application's
name.

The fixed records are written once: the standards of `examples/payments` and `examples/dog-fooding`, and the ADRs,
glossary and explanations of `examples/library`. A fixed record that cites an application's record cites Alder's copy.
The corpus consumes `examples/engineering`, as `examples/payments` does, so its policies, controls and engineering
standards arrive through `kac restore`.

The other records under `examples/` stay out: the processes, tools, controls and reports of `examples/dog-fooding`, and
the offerings of `examples/library`. A link to one of them becomes a code span, and a list in frontmatter drops its id.
The offerings of `examples/library` stay out because none of them states `nfrs`. This corpus requires `nfrs` of a live
offering, because it has NFRs.

The eight applications are Alder, Birch, Cedar, Damson, Elder, Fir, Gorse and Hazel, in that order. A corpus of
three applications has the first three, with the same ids as the corpus of eight.

## What you need

| Tool                | Why                                                                                   |
|---------------------|---------------------------------------------------------------------------------------|
| The .NET SDK        | builds `kac`. `global.json` at the repository root names the version.                 |
| Node.js 24 or later | runs the scripts, which use only its built-in modules                                 |
| `sh`                | runs the commands below. On Windows, use Git Bash, which Git for Windows installs.    |
| Claude Code         | runs the question half, as the [plugin trial](../plugin/README.md#what-you-need) says |

## Running the size half

Run each step from the root of a clone of this repository. Every step writes to `trials/scale/work/`.

**Leave `trials/scale/work/` untracked and unignored, and delete it after the run.** `kac` lists a corpus's files
through git, and leaves out every file git ignores. In an ignored folder it finds no records. Outside the clone it
finds them, but its export has no source links, so the prose text comes out smaller than the corpus #1017 measured.

1. **Build `kac` once, in Release.** The steps then run the built tool, so no step times a build:

   ```sh
   dotnet build tooling/kac -c Release -o trials/scale/work/kac
   KAC="$PWD/trials/scale/work/kac/kac"
   ```

   On Windows, the tool is `trials/scale/work/kac/kac.exe`. Set `KAC` to that path.

2. **Export and pack `examples/engineering`**, which the generated corpus consumes:

   ```sh
   ( cd examples/engineering && "$KAC" export && "$KAC" pack )
   ```

3. **Generate the corpora of 1 and 8 applications**, then restore each, regenerate its index pages and validate it:

   ```sh
   for n in 1 8; do
     node trials/scale/generate.mjs "$n" trials/scale/work
     ( cd "trials/scale/work/corpus-$n" && "$KAC" restore && "$KAC" generate && "$KAC" validate )
   done
   ```

   `validate` must report 0 errors. Where it does not, stop and record the errors.

4. **Time and measure each corpus.** [`measure.mjs`](measure.mjs) runs `validate`, `export` and `bundle` three times in
   that order. It prints the median of each, with every run beside it, and then the sizes:

   ```sh
   node trials/scale/measure.mjs "$KAC" trials/scale/work/corpus-1
   node trials/scale/measure.mjs "$KAC" trials/scale/work/corpus-8
   ```

5. **Measure the prose text** each export makes, in characters:

   ```sh
   for n in 1 8; do
     node trials/plugin/prose-claude-md.mjs "trials/scale/work/corpus-$n/.dist/export" "trials/scale/work/prose-$n.md"
   done
   wc -m trials/scale/work/prose-1.md trials/scale/work/prose-8.md
   ```

   Count its tokens from a prose session's trace, never from the characters. The `usage` on a trace's `result` line
   gives them. This corpus's prose runs at about 2.5 characters to a token.

6. **Write the run** to `runs/<date>.md`, with both tables, the prose sizes, the validate result and the machine.

## Running the question half

The question half costs a large share of a subscription's usage quota, so run it at the start of a 5-hour window.
It uses the method and the scripts of the [plugin trial](../plugin/README.md), with these arms:

| Arm    | The session has                                                                               |
|--------|-----------------------------------------------------------------------------------------------|
| plugin | the plugin `kac bundle` builds from the corpus of 8 applications, its build-index hook silent |
| prose  | the prose text of the corpus of 8 applications, delivered by the prose arm's paged hooks      |
| none   | nothing                                                                                       |

**The prose arm gets the whole corpus.** It is the alternative an adopter weighs against the plugin: everything in
context, and no plugin to install. The text of 8 applications is about 1.3 million characters, or about 530,000
tokens. `claude-opus-5-5` has a window of 1,000,000 tokens, so the text fits. At about 2.5 characters to a token it
stops fitting at about 15 applications. The run record states that line, and no arm measures it.

Once the questions are approved:

1. **Write each question as a case** under `trials/scale/cases/house/`, as
   [Writing a case](../plugin/README.md#writing-a-case) says. Its prompt is the question as written. Its scaffold writes
   an empty workspace. Its graders are the `task-` and `cite-` patterns in [the questions](questions.md), each a
   `regex` grader on the final reply.
2. **Build the plugin and the prose text.** Run the size half's steps 1 to 5 for the corpus of 8.
3. **Stage the arms** into an empty folder outside every git repository, with the whole text as the prose text:

   ```sh
   trials/plugin/stage.sh trials/scale/work/corpus-8/.dist/plugin trials/scale/cases "$STAGE" \
     trials/scale/work/prose-8.md
   ```

   `stage.sh` warns that the text is over 40,000 characters. This trial means it to be.

4. **Run the plugin and none arms with `--runs 3`**, as step 4 of
   [Running the trial](../plugin/README.md#running-the-trial) says. Twelve questions make 72 sessions.
5. **Run the prose arm with `--runs 1`**, as step 5 says. Each prose session starts with about 530,000 tokens of
   context, and costs about ten times a plugin session. Run one question first, and read its cost before you run the
   rest. The run record states that the prose arm ran once.
6. **Score the run** with `score.mjs`, as step 7 says, and pass no hook result. Read every reply by hand. Record each
   verdict that differs from the graders' in `adjudication.tsv`.
7. **Split the result by reach and cost** with [`reach.mjs`](reach.mjs), which reads each arm's
   `aggregate-result.json` and the trace of every session:

   ```sh
   node trials/scale/reach.mjs "$STAGE/out-plugin/aggregate-result.json" "$STAGE/out-prose/aggregate-result.json"
   ```

## Scoring

**The size half passes where the corpus of 8 applications validates with 0 errors.** Compare each timing and size
with the last run, and explain any that moved by more than a quarter. Read the build index against #1034's start
condition of about 8,000 characters.

**In the question half, a session answers where every `task-` grader passed, and cites where every `cite-` grader
did.** `score.mjs` reports these as `house.<arm>.task_done` and `house.<arm>.cited`. Report each arm's answer rate
twice: over the questions whose answer is in one record, and over those that need several. The third tag of a case's
`prompt.md` says which, as `one-record` or `across-records`. An arm holds up at scale where its rate on
`across-records` matches its rate on `one-record`.

Report each arm's median input tokens per session and its cost per answered question beside its rates. Take both from
`reach.mjs`, never from an estimate.

## Runs

| Run                              | Corpus               | What it was                                   |
|----------------------------------|----------------------|-----------------------------------------------|
| [2026-10-29](runs/2026-10-29.md) | 1 and 8 applications | The size half. The question half has not run. |
