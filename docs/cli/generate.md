# `generate` rewrite the parts of a corpus derived from its records

<!-- BEGIN GENERATED: usage-generate -->

```text
kac generate [--check] [--no-color]
```

| Option       | What it does                                                |
|--------------|-------------------------------------------------------------|
| `--check`    | Fail if a generated file is stale instead of writing it.    |
| `--no-color` | Turn colour off. NO_COLOR in the environment does the same. |

<!-- END GENERATED: usage-generate -->

## What it does

`generate` rewrites the parts of your corpus that are derived from frontmatter and the schema rather than written by
hand. Each type's index page, the frontmatter reference and
checks table on each type page, and the taxonomy's own tables are all written from what the corpus holds now.

A generated block is rewritten between its markers, so the words around it stay yours. Each type's `_index.md` is
written whole instead, and carries no markers. [Generation](../design/generation.md) says what each one is built
from.

Run it after you add or edit a record, and before you commit. Run `--check` in CI, which writes nothing.

## Examples

### Rewrite what the corpus derives

```bash
kac generate
```

A run that changes nothing says so:

```text
generated files already up to date; nothing written.
```

### Fail a build where the derived content has fallen behind

```bash
kac generate --check
```

It recomputes every generated file, names the ones that differ, and exits `1`. It writes nothing:

```text
generated files are stale. These differ from the schema/frontmatter:
  glossary/_index.md
run:  kac generate
```

Fix it by running `kac generate` on your own machine and committing the result. CI never commits.

### Run it in a pipeline

```bash
dotnet tool run kac generate --check
```

Give the job read-only permission. [Running it in CI](../ci.md) carries the whole workflow.

## Known limits

**It does not stand a type up.** Generation covers the types named in `types:` in
[`.corpus.yaml`](../corpus-descriptor.md), so a folder appearing without its type declared is not something `generate`
fills in. [`validate`](validate.md) reports it.

**Nobody edits generated content by hand.** Where an index looks wrong, the frontmatter it was built from is wrong.

**The types graph is written for the narrowest renderer.** Every corpus receives the same Mermaid subset an Azure
DevOps wiki can render, whatever it publishes to.
[Generation](../design/generation.md#the-graph-is-written-to-the-narrowest-renderer) says why.

[`validate`](validate.md) holds a corpus to carrying the markers this writes between.
