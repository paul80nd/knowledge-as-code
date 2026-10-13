# The comprehension trial

This trial measures whether a section of the documentation site lets its reader act. A sub-agent is given one section
and nothing else. It answers fifteen questions that section's reader would ask, and each answer is scored two ways:

* **Accuracy**: whether each answer matches the key.
* **Words read**: how many words its tools returned to it before it answered.

Run it before and after any change that cuts or moves a large share of a section. The first run is the baseline, and a
later run is compared against it.

## The sets

Each section of the site has one reader, as [the documentation charter](../../docs/maintaining/charter.md) states. Three
sections have a set. Maintaining `kac` has none while #825 and #826 rewrite it.

| Set                                                         | Section           | Reader               | Asked of          |
|-------------------------------------------------------------|-------------------|----------------------|-------------------|
| [`use-it.yaml`](questions/use-it.yaml)                      | Use it            | The `kac` user       | `docs/use/`       |
| [`framework.yaml`](questions/framework.yaml)                | The framework     | The framework reader | `docs/framework/` |
| [`schema-reference.yaml`](questions/schema-reference.yaml) | Schema reference  | The schema owner     | `docs/reference/` |

Each question has six keys:

* `id`: the question's identifier, which a run's scores cite.
* `topic`: the reader's task the question tests.
* `question`: the reader's own words, not the page's.
* `answer`: the key a scorer marks against, and what makes an answer partial.
* `doc`: the page and heading that state the answer, with a quote copied from it.
* `source`: a file outside `docs/` that proves the page right, with the line to read.

A question whose page disagreed with its source was dropped before the baseline, and the run it was dropped from says
so.

## Running the trial

Run each step from the root of the repository.

1. **Stage each section twice**, as readers A and B. Each copy goes to its own empty directory outside the repository:

   ```sh
   trials/comprehension/stage.sh use "$TMP/use-a"
   trials/comprehension/stage.sh use "$TMP/use-b"
   ```

   The section names are `use`, `framework` and `reference`.

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

5. **Write the run** to `runs/`, under the date and a name. Give each question's score from readers A and B, and keep
   each reader's reply verbatim in a folder of the same name.

### The prompt

Replace `{reader}` and `{what they want}` with that set's row below, and `{staged directory}` with the copy's path.

| Set                     | `{reader}`           | `{what they want}`                                                                           |
|-------------------------|----------------------|----------------------------------------------------------------------------------------------|
| `use-it.yaml`           | the `kac` user       | you want to install the tool, run a corpus, wire CI, build and install the plugin            |
| `framework.yaml`        | the framework reader | you want to know how the types are laid out, where they came from, and how to write a record |
| `schema-reference.yaml` | the schema owner     | you want the keys, the expression grammar, the checks and the export contract                |

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
its fifteen answers. A set's score is the mean of readers A and B.

Words read is reported for reader A, for reader B, and as their mean. The share of the staged section that mean
represents is reported beside it.

## The pass rule

A later run passes a set when all three of these are true against the baseline:

* **No question is lost.** Every question that both baseline readers answered correctly is answered correctly by both
  readers of the later run.
* **The set scores at least the baseline.** Its score is no lower.
* **The words read fall.** The mean of readers A and B is lower than the baseline's.

Where a set fails, the run names the page whose change cost the answer.

## Runs

| Run                                       | Site at    | What it measured                                   |
|-------------------------------------------|------------|----------------------------------------------------|
| [Baseline](runs/2026-10-13-baseline.md)   | `6df1b43a` | The four-section site from #821, before #824 trims |
