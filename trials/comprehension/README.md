# The comprehension trial

This trial measures whether a section of the documentation site, or a corpus (one repository of knowledge records),
lets its reader act. A sub-agent is given one section or one corpus and nothing else. It answers the questions in its
set, each one a question its reader would ask, and each answer is scored two ways:

* **Accuracy**: whether each answer matches the key.
* **Words read**: how many words its tools returned to it before it answered.

Run it before and after any change that cuts or moves a large share of a section. The first run is the baseline, and a
later run is compared against it.

## The sets

Each section of the site has one reader, as [the documentation charter](../../docs/maintaining/charter.md) states. Three
sections have a set of fifteen questions. Maintaining `kac` has none while #825 and #826 rewrite it. The library set
asks three questions of `examples/library`, one for each scenario in [`learn.feature`](../use-cases/learn.feature).

| Set                                                         | Section           | Reader                                        | Asked of            |
|-------------------------------------------------------------|-------------------|-----------------------------------------------|---------------------|
| [`use-it.yaml`](questions/use-it.yaml)                      | Use it            | The `kac` user                                | `docs/use/`         |
| [`framework.yaml`](questions/framework.yaml)                | The framework     | The framework reader                          | `docs/framework/`   |
| [`schema-reference.yaml`](questions/schema-reference.yaml) | Schema reference  | The schema owner                              | `docs/reference/`   |
| [`library.yaml`](questions/library.yaml)                    | Library corpus    | A new member of the consortium's digital team | `examples/library/` |

Each question has six keys:

* `id`: the question's identifier, which a run's scores cite.
* `topic`: the reader's task the question tests.
* `question`: the reader's own words, not the page's.
* `answer`: the key a scorer marks against, and what makes an answer partial.
* `doc`: the page and heading that state the answer, with a quote copied from it.
* `source`: a file outside `docs/` that proves the page right, with the line to read. In the library set the record is
  its own source, so `source` gives the line of the record that states the answer.

A question whose page disagreed with its source was dropped before the baseline, and the run it was dropped from says
so.

## Running the trial

Run each step from the root of the repository.

1. **Stage each section three times**, as readers A, B and C. Each copy goes to its own empty directory outside the
   repository:

   ```sh
   trials/comprehension/stage.sh use "$TMP/use-a"
   trials/comprehension/stage.sh use "$TMP/use-b"
   trials/comprehension/stage.sh use "$TMP/use-c"
   ```

   The section names are `use`, `framework`, `reference` and `library`. `library` copies the files git tracks under
   `examples/library`.

2. **Start one sub-agent per copy.** Use Claude Code's Agent tool with the `general-purpose` type, which has every
   tool, and restrict none of them. Give it the prompt below and nothing else, with only the `id` and `question` of
   each entry in the set. Record the model it ran on.

3. **Count the words each one read** from its transcript, which Claude Code writes under
   `~/.claude/projects/<project>/<session>/subagents/`:

   ```sh
   trials/comprehension/words-read.sh agent-<id>.jsonl
   ```

   The count includes every tool result the agent received, a repeated read included. It removes the line numbers the
   Read tool adds.

4. **Score each answer against the key** as correct, partial or wrong. The session that ran the readers scores them,
   and it can see which reader wrote each answer. "Not stated" scores wrong. An answer missing a part the key asks for
   scores partial, even where the key names no partial case. Where the key does not settle a score, the run says how
   it was scored.

5. **Write the run** to `runs/`, under the date and a name. Give each question's score from readers A, B and C, and
   keep each reader's reply verbatim in a folder of the same name.

### The prompt

Replace `{reader}` and `{what they want}` with that set's row below, and `{staged directory}` with the copy's path.
For the library set, replace `index.md` with `README.md`, because the corpus has no `index.md`.

| Set                     | `{reader}`                                    | `{what they want}`                                                                           |
|-------------------------|-----------------------------------------------|----------------------------------------------------------------------------------------------|
| `use-it.yaml`           | the `kac` user                                | you want to install the tool, run a corpus, wire CI, build and install the plugin            |
| `framework.yaml`        | the framework reader                          | you want to know how the types are laid out, where they came from, and how to write a record |
| `schema-reference.yaml` | the schema owner                              | you want the keys, the expression grammar, the checks and the export contract                |
| `library.yaml`          | a new member of the consortium's digital team | you want to learn the estate's words, how its parts work, and what it offers                 |

```text
You are {reader}: {what they want}. The documentation section written for you is a set of
Markdown files in {staged directory}. It is the only source you may use. Read nothing outside that directory, use
no web access, and answer nothing from what you already know.

Answer the questions below. You are scored on two things: whether each answer is correct, and how few words you
read to reach them. Start from index.md, follow what it points you to, and search rather than reading every page
whole where a search will do.

Where the pages do not answer a question, write "Not stated". Do not guess.

Reply with one entry per question, in this form and nothing else:

{id}. {your answer, in at most three sentences}
Page: {the file the answer came from}

{the questions, one per line, as "{id}. {question}"}
```

## Scoring

A correct answer scores 1, a partial answer scores 0.5, and a wrong answer scores 0. A reader's score is the sum over
its answers. A set's score is the sum over readers A, B and C, out of three times the set's question count: 45 for a
set of fifteen, and 9 for the library set.

For the library set, an answer cites a record where its `Page:` line names the record's file, or where it gives the
record's id.

Words read is reported for each reader and as the median of the three. One reader with an unusual reading habit cannot
move a median. The share of the staged section that median represents is reported beside it.

## The pass rule

A later run passes a set when all three of these are true against the baseline:

* **No question is lost.** Every question that all three baseline readers answered correctly is answered correctly by
  all three readers of the later run.
* **The set scores at least the baseline.** Its score is no lower.
* **The words read fall.** The median of readers A, B and C is lower than the baseline's.

Where a set fails, the run names the page whose change cost the answer.

## Runs

| Run                                     | Site at                                      | What it measured                                                                                   |
|-----------------------------------------|----------------------------------------------|----------------------------------------------------------------------------------------------------|
| [Baseline](runs/2026-10-13-baseline.md) | `6df1b43a`                                   | The four-section site from #821, before #824 trims                                                 |
| [Trimmed](runs/2026-10-13-trimmed.md)   | `e0d55c52`, and `09812365` for the framework | The site #824 trimmed. Use it and Schema reference pass, and The framework is accepted as variance |
| [Library](runs/2026-10-21-library.md)   | `3779317b`                                   | `examples/library` for #991, the library set's baseline. It scored 8 of 9                          |
