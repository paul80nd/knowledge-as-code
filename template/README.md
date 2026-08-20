# `template` — what a corpus is made of

Everything a corpus receives is authored here: the machine-readable schema `kac` enforces, the framework's own
documentation, the tree a corpus's plugin is built from, and the pages and templates a corpus starts writing against.

[`manifest.yaml`](manifest.yaml) classifies every file here into a layer, and is the only place that decision is
recorded.

| Layer        | What it means                                                                             |
|--------------|-------------------------------------------------------------------------------------------|
| **overlay**  | Identical in every corpus, and written again whenever the corpus takes a newer framework. |
| **seed**     | Written once, when the corpus is created, and never touched again.                        |
| **withheld** | The template's own machinery. Never written into a corpus at all.                         |

The rules are read in order and the first match wins, so a rule carving one file out of a folder is written above the
rule claiming that folder. The catch-all at the foot seeds anything nobody classified. That is the safe way round: a
corpus's own edit is never overwritten by a file no one thought about.

## Its relationship to `example/`

[`../example/`](../example/) is a corpus that took this template. It holds its own copy of every overlaid file, real,
visible and git-tracked, the way a corpus keeps them.

Two copies of one file is what this arrangement is for, and also how it goes quietly wrong. `TemplateTests` in
[`../tooling/kac.tests`](../tooling/kac.tests) holds the two to matching in both directions. It catches an overlaid file
the corpus changed, and one the corpus has that the template does not.

**Author an overlaid change here, then copy it across.** Both trees are checked, so the order is a habit rather than a
rule. The template is where the file belongs, and the corpus is where a copy of it lives.

Every test layer reads the schema straight from [`.schema/`](.schema/) rather than from the corpus's copy. A schema edit
made here surfaces as a broken golden in the same run.

## What is not here

`.corpus.yaml` is not a template file. A corpus's descriptor names the corpus, says where it publishes, and lists the
types it adopted. No copied file could answer any of that.

Nor are the repository-shaped files a corpus needs and this folder does not yet carry: its ignore rules, its editor
conventions, its wiki ordering and its own CI. Each is a question about the corpus rather than about the framework. They
are answered where the command that creates a corpus is built.
