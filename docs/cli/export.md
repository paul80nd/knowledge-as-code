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

A consumer of your corpus should not have to clone it. `export` writes what the corpus knows into `.dist/export/` as
data built for an agent to read.

It writes three kinds of file: a manifest saying what the export is, one file per record for a reader wanting a whole
record, and a flat file cheap to grep for a reader holding only a word. What travels is each type's own decision,
declared beside the type, so a corpus adopting a new type exports it without the tool changing.

**What your corpus consumes travels with what it wrote.** A corpus, meaning one repository of knowledge records, may
name others in `consumes:`. Their records arrive merged into the flat file for each type, so a consumer greps once for
everything that reaches it. Run [`restore`](restore.md) first: `export` refuses rather than writing a smaller export
that reads as whole.

[The export format](../design/export.md) is the contract those files answer to. Run `export` before
[`bundle`](bundle.md) or [`pack`](pack.md), each of which reads what this writes.

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

### Carry what this corpus consumes

Where `.corpus.yaml` names another corpus in `consumes:`, the run says which arrived and at what version:

```text
export: carried example-engineering 0.7.4, which this corpus consumes. Their records travel merged with its own.
```

Their records are filed under the shortcode of the corpus that wrote them, and their lines carry that shortcode too.
A line with none is your own, which is the rule a citation already follows.

### Notice a dirty tree

An export names the commit it was built from. Where the tree has uncommitted changes, the run says so and the manifest
records it:

```text
export: built from a dirty working tree, and the manifest says so. The commit it names does not reproduce it.
```

Commit first where you are about to publish the result.

### Meet a refusal

Three things end the run with the reason and nothing written, because each would otherwise publish a file that reads as
whole and answers two ways:

```text
export: nothing is restored for eng, which this corpus consumes and an export carries. Run kac restore.
```

The other two are a consumed corpus at an export format this `kac` does not read, and one exporting a type at a
different shape or a different section fidelity from yours. The first is fixed by re-exporting and re-packing upstream.
The second is a decision about the two corpora, not about this command.

## Known limits

**It is not a backup.** A record travels as the fields and sections its type declared, so a corpus cannot be rebuilt
from an export of it. `.dist/export/` is rebuilt whole from the corpus, and never the other way.

**Four limits belong to a type's declaration rather than to this command.**
[The export format](../design/export.md#what-a-type-cannot-say) states each one and what it costs a consumer.

Two commands read what this writes. [`bundle`](bundle.md) assembles it into a plugin an agent installs, and
[`pack`](pack.md) seals it into a versioned package another corpus imports.
