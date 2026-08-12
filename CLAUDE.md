# Working in this repository

## Read the role before you touch `.schema/` or `.tooling/`

[`.corpus.yaml`](.corpus.yaml) declares `role:`. This repository is a **source**: those two directories are yours to
change, what you write propagates to every corpus that took a copy, and the tests that prove the tool live here. Write
for someone who cannot see this conversation.

Where a corpus declares `role: consumer`, both directories arrive from upstream and a local edit is drift rather than
customisation. `kac mechanism --check` reports it. Fix it upstream and run `kac mechanism --sync`. A consumer holds the
tool and none of the tests, because the tool reaches it already proven.

**Adding a knowledge type is adding a YAML file to `.schema/`.** A corpus adopts a type by adding its name to `types:`
in `.corpus.yaml` and running `kac mechanism --sync`, which brings down the schema and seeds the root page and
template. To decline a type, leave it out of `types:` rather than deleting files afterwards.

## Before you commit

```bash
./kac validate                      # the corpus
./kac index --check                 # generated output is fresh
dotnet test .tooling/kac.tests      # unit
dotnet test .tooling/kac.features   # Reqnroll behaviour specs
dotnet run .tooling/kac-tests.cs    # golden fixtures, plus the coverage and checks-table gates
```

All three test layers gate the branch and assert different things about the same corpus, so regenerating goldens can
leave you green locally and red in CI. Run **one `kac` invocation at a time**: file-based apps share build output and
contend.

## Conventions

* **Regenerate rather than edit between `BEGIN GENERATED` and `END GENERATED`.** Change the schema or the frontmatter,
  then run `./kac index`. A schema edit without a regeneration fails CI.
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
* **Extend one fictional estate** — Example Libraries, a public-library consortium, on `example.com`, which RFC 2606
  reserves. [`README.md`](README.md) explains why.
* **Branch and open a PR.** Pushes to `main` are rejected.

## Writing a record

**How a document is written follows its tier, not its type.** Read all three pages below before writing or rewriting
one: a runbook step and an ADR paragraph obey different constraints, and nothing in CI will tell you that you used the
wrong ones.

* [`knowledge-as-code/style.md`](knowledge-as-code/style.md) — the rules for the words, which are the same in every
  document, comment and commit message. Run the checklist at its foot over the finished draft, as its own pass: holding
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

* [`.tooling/CLAUDE.md`](.tooling/CLAUDE.md) — changing the validator, the generator, or the fixtures they are tested
  against.
* [`.schema/CLAUDE.md`](.schema/CLAUDE.md) — changing the schema, or writing a rule.
