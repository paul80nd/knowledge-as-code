# `export` write the corpus out as data a consumer can read

<!-- BEGIN GENERATED: usage-export -->

```text
kac export [--no-color] [--type <TYPE>]
```

| Option          | What it does                                                |
|-----------------|-------------------------------------------------------------|
| `--no-color`    | Turn colour off. NO_COLOR in the environment does the same. |
| `--type <TYPE>` | Export one type rather than every type that contributes.    |

<!-- END GENERATED: usage-export -->

## What it does

A consumer of a corpus, meaning one repository of knowledge records kept in git, should not have to clone it. `export`
writes what the corpus knows into `.dist/export/` as data built for an agent to read.

It writes three kinds of file: a manifest saying what the export is, one file per record for a reader wanting a whole
record, and a flat file cheap to grep for a reader holding only a word. What travels is each type's own decision,
declared beside the type, so a corpus adopting a new type exports it without the tool changing.

[The export format](../design/export.md) is the contract those files answer to. Run `export` before
[`bundle`](bundle.md), which reads what this writes.

## Examples

### Write the whole corpus out

```bash
kac export
```

Each file is named as it is written, and the run closes with a count per type:

```text
wrote .dist/export/glossary/gls-example-libraries.json
wrote .dist/export/glossary/gls-knowledge-as-code.json
wrote .dist/export/glossary/gls-search.json
wrote .dist/export/glossary/terms.jsonl
wrote .dist/export/manifest.json
export: wrote 5 file(s) for glossary.
```

### Export one type

```bash
kac export --type glossary
```

The corpus is still loaded whole, so every id resolves. A type your corpus has not adopted is refused by name.

### Notice a dirty tree

An export names the commit it was built from. Where the tree has uncommitted changes, the run says so and the manifest
records it:

```text
export: built from a dirty working tree, and the manifest says so. The commit it names does not reproduce it.
```

Commit first where you are about to publish the result.

## Known limits

**It is not a backup.** A record travels as the fields and sections its type declared, so a corpus cannot be rebuilt
from an export of it. `.dist/export/` is rebuilt whole from the corpus, and never the other way.

**An exported field is a scalar.** A field the record writes as a list arrives as `null`. A policy's `aligns-with` is
that case, and stays out of the block for that reason.

**Three more limits belong to a type's declaration rather than to this command.**
[The export format](../design/export.md#what-a-type-cannot-say) states each one and what it costs a consumer.

[`bundle`](bundle.md) assembles what this writes into something a consumer can install.
