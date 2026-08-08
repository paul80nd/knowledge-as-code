# Working in this repository

## First: which repository is this?

Read `role:` in [`.mechanism.lock`](.mechanism.lock) before you change anything under `.schema/` or `.tooling/`. It
decides whether those directories are yours.

* **`role: source`** — the framework master. `.schema/` and `.tooling/` are yours to change, and what you write
  propagates to every corpus that has taken a copy. Assume it will be read by someone who cannot see this
  conversation and cannot ask you what you meant.
* **`role: consumer`** — a corpus derived from the framework. `.schema/` and `.tooling/` arrive from upstream, and a
  local edit to either is **drift, not customisation** — `kac mechanism --check` reports it as a defect. Fix it
  upstream and resync. The exception is adding or deleting a whole type file in `.schema/`, which is a corpus's own
  decision about what it has adopted.

If a change seems to need editing the tool, it almost certainly does not: **adding a knowledge type is adding a YAML
file to `.schema/`**.

## Before you commit

```bash
./kac validate                      # the corpus
./kac index --check                 # generated output is fresh
dotnet test .tooling/kac.tests      # unit
dotnet test .tooling/kac.features   # Reqnroll behaviour specs
dotnet run .tooling/kac-tests.cs    # golden fixtures, plus the coverage and checks-table gates
```

**All three test layers are the gate.** `kac-tests.cs` is the golden layer, not the suite — the feature layer pins
things the goldens do not, so regenerating goldens can leave you green locally and red in CI.

Run **one `kac` invocation at a time**. File-based apps share build output and contend if run concurrently.

## Conventions you would not guess

* **Never hand-edit between `BEGIN GENERATED` and `END GENERATED`.** Change the schema or the frontmatter, then run
  `./kac index`. A schema edit without a regeneration fails CI.
* **Markdown prose wraps at 120 columns**; tables are exempt. `.editorconfig` says so and no check enforces it.
* **Comments and documentation are timeless.** Describe the design as it is, not as it changed, and never as a
  correction of what it was. The history of a change belongs in its commit message.
* **A Markdown edit leaves a whole document, not a diff.** Fold new material into what is already there and delete
  what it supersedes, so the file reads in one voice and someone arriving cold cannot tell which paragraph is the
  newest. Give each point the detail it earns and make it once — length is not thoroughness, and a paragraph
  justifying a change is a paragraph that will read as noise a month later.
* **Branch and open a PR.** Pushes to `main` are rejected.
* **Example records use one fictional estate** — Example Libraries, a public-library consortium, on `example.com`
  (reserved by RFC 2606). Extend it rather than inventing a second one; [`README.md`](README.md) explains why.

## Going deeper

* [`.tooling/CLAUDE.md`](.tooling/CLAUDE.md) — changing the validator or the generator.
* [`.schema/CLAUDE.md`](.schema/CLAUDE.md) — changing the schema.
