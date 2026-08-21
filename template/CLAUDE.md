# Working in this repository

## Read the role before you touch `.schema/`

[`.corpus.yaml`](.corpus.yaml) declares `role:`. This corpus is a **consumer**. `.schema/` and the framework's own
documentation arrive from upstream, and a local edit to either is drift rather than customisation.
`kac mechanism --check` reports it. Fix it upstream and take it down again.

A consumer holds the tool and none of the tests that prove it, because the tool reaches it already proven. Only a corpus
declaring `role: source` develops the framework, and that one holds both.

**Adding a knowledge type is adding a YAML file to `.schema/`.** A corpus adopts a type by adding its name to `types:`
in `.corpus.yaml` and running `kac mechanism --sync`. That brings down the schema and seeds the root page and template.
To decline a type, leave it out of `types:` rather than deleting files afterwards.

## Before you commit

```bash
kac validate          # the corpus
kac generate --check  # generated output is fresh
```

Both gate the branch, so a clean local run is what a pull request expects rather than something to aim for.

## Conventions

* **Regenerate rather than edit between `BEGIN GENERATED` and `END GENERATED`.** Change the schema or the frontmatter,
  then run `kac generate`. A schema edit without a regeneration fails CI.
* **Wrap Markdown prose at 120 columns.** Tables and link definitions are exempt — a URL cannot be broken.
  `.editorconfig` says so and no check enforces it.
* **Write what exists today.** Agreed and unbuilt work goes to the issue tracker. One exception: a schema rule the tool
  does not implement, where prose says the rule is declared and does not run, and the generated checks table carries it.
* **Keep comments and documentation timeless.** Describe the design as it stands. The history of a change belongs in its
  commit message.
* **Leave a whole document, not a diff.** Fold new material into what is there and delete what it supersedes, so the
  file reads in one voice and someone arriving cold cannot tell which paragraph is newest.
* **Say it once.** Cite rather than duplicate. A paragraph that belongs in two documents belongs in
  `knowledge-as-code/`, written a single time.
* **Branch and open a PR.** Pushes to `main` are rejected.

## Writing a record

**How a document is written follows its tier, not its type.** Read all three pages below before writing or rewriting
one. A runbook step and an ADR paragraph obey different constraints, and nothing in CI will tell you that you used the
wrong ones.

* [`knowledge-as-code/style.md`](knowledge-as-code/style.md) — the rules for the words, which are the same in every
  document and every commit message. Run the checklist at its foot over the finished draft, as its own pass: holding
  it in mind while writing produces different prose.
* [`knowledge-as-code/authoring.md`](knowledge-as-code/authoring.md) — what the record's tier adds on top.
* [`knowledge-as-code/contributing.md`](knowledge-as-code/contributing.md) — the link and template conventions CI
  enforces, and what outranks what when two rules disagree.

## Your working style

Say in one sentence what you are about to do before your first tool call. While working, report what you found or where
you changed direction, and nothing else. Finish by leading with the outcome — what happened, or what you found — with
the supporting detail after it.

Keep answers brief: a high-level summary unless depth is asked for, short caveats, and a written document no longer than
its substance needs.

Deliver what was asked at the scope asked, making routine judgement calls yourself. Ask only where two readings would
produce materially different work. Where the request looks mistaken, say so in a sentence and carry on with it as asked.

## Going deeper

* [`.schema/CLAUDE.md`](.schema/CLAUDE.md) — changing the schema, or writing a rule.
* `kac --help`, and the repository the tool is built in — changing the validator or the generator themselves.
