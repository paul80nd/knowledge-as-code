# Working in this repository

## First: which repository is this?

Read `role:` in [`.mechanism.lock`](.mechanism.lock) before you change anything under `.schema/` or `.tooling/`. It
decides whether those directories are yours.

* **`role: source`** — the framework master. `.schema/` and `.tooling/` are yours to change, and what you write
  propagates to every corpus that has taken a copy. Assume it will be read by someone who cannot see this conversation
  and cannot ask you what you meant.
* **`role: consumer`** — a corpus derived from the framework. `.schema/` and `.tooling/` arrive from upstream, and a
  local edit to either is **drift, not customisation** — `kac mechanism --check` reports it as a defect. Fix it upstream
  and resync. The exception is adding or deleting a whole type file in `.schema/`, which is a corpus's own decision
  about what it has adopted.

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
* **Markdown prose wraps at 120 columns**; tables and link definitions are exempt — a URL cannot be broken.
  `.editorconfig` says so and no check enforces it.
* **How a document is written follows its tier, not its type.**
  [`knowledge-as-code/style.md`](knowledge-as-code/style.md) holds the rules for the words, which are the same in every
  document, comment and commit message. [`knowledge-as-code/authoring.md`](knowledge-as-code/authoring.md) holds what
  the tier adds. Read both before writing or rewriting any record. A runbook step and an ADR paragraph obey different
  constraints, and nothing in CI will tell you that you used the wrong ones.
* **Say less, once.** Cut filler, do not restate frontmatter in prose, and cite rather than duplicate. If a paragraph
  would appear in more than one document, it belongs in `knowledge-as-code/` and gets written a single time.
* **Anything not yet built carries a marker.** Unmarked prose describes what exists today; direction is marked
  **Planned** or **Aspirational**. Do not write aspirational content unmarked, and do not delete a marker without
  deleting the gap it describes.
* **Comments and documentation are timeless.** Describe the design as it is, not as it changed, and never as a
  correction of what it was. The history of a change belongs in its commit message.
* **A Markdown edit leaves a whole document, not a diff.** Fold new material into what is already there and delete what
  it supersedes, so the file reads in one voice and someone arriving cold cannot tell which paragraph is the newest.
  Give each point the detail it earns and make it once — length is not thoroughness, and a paragraph justifying a change
  is a paragraph that will read as noise a month later.
* **Branch and open a PR.** Pushes to `main` are rejected.
* **Example records use one fictional estate** — Example Libraries, a public-library consortium, on `example.com`
  (reserved by RFC 2606). Extend it rather than inventing a second one; [`README.md`](README.md) explains why.

## Your working style

Keep responses focused, brief, and concise. Keep disclaimers and caveats short, and spend most of the response on the
main answer. When asked to explain something, give a high-level summary unless an in-depth explanation is specifically
requested.

Before your first tool call, say in one sentence what you're about to do. While working, give a brief update only when
you find something important or change direction. When you finish, lead with the outcome: your first sentence should
answer "what happened" or "what did you find," with supporting detail after it for readers who want it.

Match the length of written documents to what the task needs: cover the substance, but do not pad with filler sections,
redundant summaries, or boilerplate.

Deliver what was asked, at the scope intended. Make routine judgment calls yourself, and check in only when different
readings of the request would lead to materially different work. If the request seems mistaken or a better approach
exists, say so in a sentence and continue with the task as asked rather than quietly narrowing, widening, or
transforming it. Finish the whole task, and stop short of actions that are clearly beyond what was asked.

## Going deeper

* [`.tooling/CLAUDE.md`](.tooling/CLAUDE.md) — changing the validator or the generator.
* [`.schema/CLAUDE.md`](.schema/CLAUDE.md) — changing the schema.
