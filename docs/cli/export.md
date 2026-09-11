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

`export` writes what your corpus (one repository of knowledge records kept in git) knows into `.dist/export/`, as data
built for an agent to read. A consumer then reads the corpus without cloning it.

It writes three kinds of file: a manifest saying what the export is, one file per record for a reader who wants a whole
record, and a flat file cheap to grep for a reader who has only a word. Each type decides what travels, and declares it
beside the type, so a corpus that adopts a new type exports it without any change to the tool.

**What your corpus consumes travels with what it wrote.** A corpus may list others in `consumes:`. Their records arrive
merged into the flat file for each type, so a consumer greps once for everything that reaches it. Run
[`restore`](restore.md) first. `export` refuses instead of writing a smaller export that reads as whole.

[The export format](../design/export.md) is the contract those files answer to. Run `export` before
[`bundle`](bundle.md) or [`pack`](pack.md). Each of those reads what this writes.

## Examples

### The whole corpus

```bash
kac export
```

`export` prints each file as it writes it, and closes with a count per type:

```text
wrote .dist/export/glossary/gls-example-libraries.json
wrote .dist/export/glossary/gls-knowledge-as-code.json
wrote .dist/export/glossary/gls-search.json
wrote .dist/export/glossary/terms.jsonl
wrote .dist/export/manifest.json
export: wrote 5 file(s) for glossary.
```

### One type

```bash
kac export --type glossary
```

`kac` still loads the corpus whole, so every id resolves. It refuses a type your corpus has not adopted, and says which
one.

### A corpus that consumes another

Where `.corpus.yaml` lists another corpus in `consumes:`, the run says which one arrived and at what version:

```text
export: carried example-engineering 0.16.0, which this corpus consumes. Their records travel merged with its own.
```

Their records are filed under the shortcode of the corpus that wrote them, and their lines carry that shortcode too. A
line with no shortcode is your own, which is the rule a citation already follows.

### A dirty tree

An export records the commit it was built from. Where the tree has uncommitted changes, the run says so and the manifest
records it:

```text
export: built from a dirty working tree, and the manifest says so. The commit it names does not reproduce it.
```

Commit first where you are about to publish the result.

### A refusal

Three things end the run with a reason and nothing written. Each would otherwise publish a file that reads as whole and
answers two ways:

```text
export: nothing is restored for eng, which this corpus consumes and an export carries. Run kac restore.
```

The other two are a consumed corpus at an export format this `kac` cannot read, and one exporting a type at a different
shape or a different section fidelity from yours. Fix the first by re-exporting and re-packing upstream. The second is a
decision about the two corpora, not about this command.

## Known limits

**It is not a backup.** A record travels as the fields and sections its type declared, so nobody can rebuild a corpus
from an export of it. `kac` rebuilds `.dist/export/` whole from the corpus, and never the other way.

**Four limits belong to a type's declaration rather than to this command.**
[The export format](../design/export.md#what-a-type-cannot-say) states each one and what it costs a consumer.

Two commands read what this writes. [`bundle`](bundle.md) assembles it into a plugin an agent installs, and
[`pack`](pack.md) zips it into a versioned package another corpus imports.
