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

`generate` rewrites the parts of your corpus that `kac` derives from frontmatter and the schema. That covers each type's
index page, the frontmatter reference and the checks table on each type page, and the taxonomy's own tables. `kac`
builds each one from what the corpus has now.

`generate` rewrites a generated block between its markers, so the words around it stay yours. It writes each type's
`_index.md` whole instead, and that file has no markers. [Generation](../design/generation.md) says what each one is
built from.

Run it after you add or edit a record, and before you commit. Run `--check` in CI, which writes nothing.

## Examples

### A plain run

```bash
kac generate
```

A run that changes nothing says so:

```text
generated files already up to date; nothing written.
```

### A stale check

```bash
kac generate --check
```

`--check` recomputes every generated file, lists the ones that differ, and exits `1`. It writes nothing:

```text
generated files are stale. These differ from the schema/frontmatter:
  glossary/_index.md
run:  kac generate
```

Run `kac generate` on your own machine and commit the result. CI never commits.

### A pipeline step

```bash
dotnet tool run kac generate --check
```

Give the job read-only permission. [Running it in CI](../ci.md) has the whole workflow.

## Known limits

**It does not stand a type up.** `generate` covers the types listed in `types:` in
[`.corpus.yaml`](../corpus-descriptor.md). It creates no page and no folder for a type that is missing one.
[`validate`](validate.md) reports that instead.

**Do not edit generated content by hand.** Where an index looks wrong, the frontmatter it was built from is wrong.

**The types graph is written for the narrowest renderer.** Every corpus gets the same Mermaid subset an Azure DevOps
wiki can render, whatever it publishes to.
[Generation](../design/generation.md#the-graph-is-written-to-the-narrowest-renderer) says why.

[`validate`](validate.md) checks that every file this writes into still has both markers of each block.
