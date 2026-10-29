# The scale trial

This trial measures what `kac` costs on a corpus (one repository of knowledge records) the size of an estate of eight
applications. It times `validate`, `export` and `bundle`, and measures what `export` and `bundle` write. Its second
half asks a session questions about that corpus through the plugin, through the corpus as one prose text, and with
nothing. It shows whether the plugin still finds an answer once the prose no longer fits a context window.

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

A copy takes its application's name as the first word of its slug, as `svc-cedar-payment-api`. A numbered type counts
on from the application before, as `nfr-0022`. Every id a copy cites in its own application is rewritten to that
application's copy, and so are its links, its repositories and its `owner`. Its H1 ends with the application's name.

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

6. **Write the run** to `runs/<date>.md`, with both tables, the prose sizes, the validate result and the machine.

## Running the question half

The question half has not run. Have [the questions](questions.md) reviewed before the first run, because the run costs
a large share of a subscription's usage quota. It uses the method and the scripts of the
[plugin trial](../plugin/README.md), with these arms:

| Arm    | The session has                                                                               |
|--------|-----------------------------------------------------------------------------------------------|
| plugin | the plugin `kac bundle` builds from the corpus of 8 applications, its build-index hook silent |
| prose  | the prose text of the corpus of 2 applications, appended to its system prompt                 |
| none   | nothing                                                                                       |

**The prose arm gets the largest whole cut that fits a 200,000-token window with room to work.** The text of 8
applications is about 1.3 million characters, or about 330,000 tokens at four characters to a token. The text of 2
applications, Alder and Birch with every fixed record, is about 525,000 characters, or about 131,000 tokens. The text
of 3 is about 164,000 tokens, which leaves too little of the window for Claude Code's own prompt and the session. The
cut is verbatim, because the corpus of 2 applications has the same records and ids as the first two of 8.

Once the questions are approved:

1. **Write each question as a case** under `trials/scale/cases/house/`, as
   [Writing a case](../plugin/README.md#writing-a-case) says. Its prompt is the question as written. Its scaffold writes
   an empty workspace. Its graders are the `task-` and `cite-` patterns in [the questions](questions.md), each a
   `regex` grader on the final reply.
2. **Build the plugin and the cut.** Run the size half's steps 1 to 4 for the corpus of 8, then:

   ```sh
   node trials/scale/generate.mjs 2 trials/scale/work
   ( cd trials/scale/work/corpus-2 && "$KAC" restore && "$KAC" generate && "$KAC" validate && "$KAC" export )
   node trials/plugin/prose-claude-md.mjs trials/scale/work/corpus-2/.dist/export trials/scale/work/prose-2.md
   ```

3. **Stage the arms** into an empty folder outside every git repository:

   ```sh
   trials/plugin/stage.sh trials/scale/work/corpus-8/.dist/plugin trials/scale/cases "$STAGE"
   ```

   `stage.sh` appends the whole prose text of 8 applications to each prose case, and warns that it is too large.
   Replace it with the cut:

   ```sh
   for c in "$STAGE"/prose/evals/house/*/; do
     name=$(basename "$c")
     { cat "trials/scale/cases/house/$name/case.yaml"
       printf 'execution:\n  append_system_prompt: |\n'
       sed 's/^/    /' trials/scale/work/prose-2.md; } > "$c/case.yaml"
   done
   ```

4. **Run the plugin and none arms, then the prose arm**, as steps 4 and 5 of
   [Running the trial](../plugin/README.md#running-the-trial) say, with `--runs 3`. Twelve questions make 72 sessions
   in the plugin stage and 36 in the prose stage. Each prose session starts with about 131,000 tokens of system prompt.
   Run the prose arm on one question first, and read its cost before you run the rest.
5. **Score the run** with `score.mjs`, as step 7 says, and pass no hook result. Read every reply by hand. Record each
   verdict that differs from the graders' in `adjudication.tsv`.

## Scoring

**The size half passes where the corpus of 8 applications validates with 0 errors.** Compare each timing and size
with the last run, and explain any that moved by more than a quarter. Read the build index against #1034's start
condition of about 8,000 characters.

**In the question half, a session answers where every `task-` grader passed, and cites where every `cite-` grader
did.** `score.mjs` reports these as `house.<arm>.task_done` and `house.<arm>.cited`. Report each arm's answer rate
twice: over the questions marked `in` the cut, and over those marked `out`. The plugin keeps working at scale where
its answer rate on the `out` questions matches its rate on the `in` questions. The prose arm's rate on the `out`
questions is what the cut costs.

## Runs

| Run                              | Corpus               | What it was                                   |
|----------------------------------|----------------------|-----------------------------------------------|
| [2026-10-29](runs/2026-10-29.md) | 1 and 8 applications | The size half. The question half has not run. |
