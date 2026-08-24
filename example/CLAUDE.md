# Working in this repository

## Know where you are before you touch `../.schema/` or `../tooling/`

This corpus sits inside the repository serving the template, so those two directories are yours to change and what you
write reaches every corpus that took a copy. Write for someone who cannot see this conversation. The schema sits at the
repository root and not in this corpus, because `template/` is judged against the same copy.

A corpus created elsewhere receives its own copy of `.schema/`, and a local edit there is drift. `kac update --check`
reports it. Fix it in the template and run `kac update`.

**Adding a knowledge type is adding a YAML file to `../.schema/`.** A corpus adopts a type with
`kac update --add-type <name>`, which writes the schema, the root page and the template, and adds the name to `types:`.
`kac update --drop-type <name>` gives one up, and refuses where the folder still holds records.

## Before you commit

```bash
# here, in the corpus, through the tool this repository builds
dotnet run --project ../tooling/kac -- validate         # the corpus
dotnet run --project ../tooling/kac -- generate --check # generated output is fresh

# from the repository above it, which holds the tool and the tests that prove it
dotnet test tooling/kac.tests      # unit
dotnet test tooling/kac.features   # Reqnroll behaviour specs
dotnet run tooling/kac-tests.cs    # golden fixtures, plus the coverage and checks-table gates
```

A bare `kac` runs the published tool rather than this one. [`../tooling/CLAUDE.md`](../tooling/CLAUDE.md) says what that
costs, and carries the `template/` runs that go beside these.

All three test layers gate the branch and assert different things about the same corpus, so regenerating goldens can
leave you green locally and red in CI. Run **one `kac` invocation at a time**: concurrent runs build the same project
and contend over its output.

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
* **Extend one fictional estate**: Example Libraries, a public-library consortium, on `example.com`, which RFC 2606
  reserves. [`README.md`](README.md) explains why.
* **Branch and open a PR.** Pushes to `main` are rejected.

## Writing a record

**How a document is written follows its tier, not its type.** Read everything below before writing or rewriting one: a
runbook step and an ADR paragraph obey different constraints, and nothing in CI will tell you that you used the wrong
ones.

* **Load `technical-writing`.** The rules for the words, which are the same in every document and every commit message.
* **Then `writing-a-record`.** What this corpus adds to them, what the record's tier adds on top, the link forms CI
  enforces, and what a `_template.md` may say.
* **Where a prose rule and the schema disagree, the schema is right.** Report the contradiction rather than editing
  records to match.

## Your working style

Say in one sentence what you are about to do before your first tool call. While working, report what you found or where
you changed direction, and nothing else. Finish by leading with the outcome (what happened, or what you found) and put
the supporting detail after it.

Keep answers brief: a high-level summary unless depth is asked for, short caveats, and a written document no longer than
its substance needs.

Deliver what was asked at the scope asked, making routine judgement calls yourself. Ask only where two readings would
produce materially different work. Where the request looks mistaken, say so in a sentence and carry on with it as asked.

## Going deeper

* [`../tooling/CLAUDE.md`](../tooling/CLAUDE.md) covers changing the validator, the generator, or the fixtures they are
  tested against.
* [`../.schema/CLAUDE.md`](../.schema/CLAUDE.md) covers changing the schema, or writing a rule.
