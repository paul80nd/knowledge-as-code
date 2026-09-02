# Working in this repository

## Know what is yours before you touch `.schema/`

`.schema/` and the framework's own documentation arrive from the template this corpus took, and a local edit to either
is drift. `kac update --check` reports it. Fix it in the template and take it down again with `kac update`. Where this
corpus means to own a file, say so with a `skip:` entry in [`.corpus.yaml`](.corpus.yaml).

This corpus holds the tool and none of the tests that prove it, because the tool reaches it already proven.

**Adopting a knowledge type is one command.** `kac update --add-type <name>` writes the type's schema, its root page and
its template, and adds the name to `types:`. The template declares which types there are to take.
`kac update --drop-type <name>` gives one up, and refuses where the folder still holds records.

## Before you commit

```bash
kac validate          # the corpus
kac generate --check  # generated output is fresh
```

Both gate the branch, so a clean local run is what a pull request expects rather than something to aim for.

## Conventions

* **Regenerate rather than edit between `BEGIN GENERATED` and `END GENERATED`.** Change the schema or the frontmatter,
  then run `kac generate`. A schema edit without a regeneration fails CI.
* **Wrap Markdown prose at 120 columns.** Tables and link definitions are exempt: a URL cannot be broken.
  `.editorconfig` says so and no check enforces it.
* **Write what exists today.** Agreed and unbuilt work goes to the issue tracker. One exception: a schema rule the tool
  does not implement, where prose says the rule is declared and does not run, and the generated checks table carries it.
* **Keep comments and documentation timeless.** Describe the design as it stands. The history of a change belongs in its
  commit message.
* **Leave a whole document, not a diff.** Fold new material into what is there and delete what it supersedes, so the
  file reads in one voice and someone arriving cold cannot tell which paragraph is newest.
* **Say it once.** Cite rather than duplicate. A paragraph that belongs in two documents belongs in
  `knowledge-as-code/`, written a single time.
* **Branch and open a pull request.** A reviewed merge is what the starter pipeline is written to gate.

## Writing a record

**How a document is written follows its tier, not its type.** A runbook step and an ADR paragraph obey different
constraints, and nothing in CI will tell you that you used the wrong ones.

* [`knowledge-as-code/contributing.md`](knowledge-as-code/contributing.md) is the way in, and names the two skills that
  carry the rules for the words.
* [`knowledge-as-code/taxonomy.md`](knowledge-as-code/taxonomy.md) says which tier a type belongs to.

## Your working style

Say in one sentence what you are about to do before your first tool call. While working, report what you found or where
you changed direction, and nothing else. Finish by leading with the outcome (what happened, or what you found) and put
the supporting detail after it.

Keep answers brief: a high-level summary unless depth is asked for, short caveats, and a written document no longer than
its substance needs.

Deliver what was asked at the scope asked, making routine judgement calls yourself. Ask only where two readings would
produce materially different work. Where the request looks mistaken, say so in a sentence and carry on with it as asked.

## Going deeper

* [`.schema/CLAUDE.md`](.schema/CLAUDE.md) covers changing the schema, or writing a rule.
* `kac --help`, and the repository the tool is built in, cover changing the validator or the generator themselves.
