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

## What it is for

A consumer of a corpus, meaning one repository of knowledge records, should not clone it. `export` writes what the
corpus knows into `.dist/export/` as data built for an agent to read.

It writes three kinds of file: a manifest saying what the export is, one file per record for a reader that wants a
whole record, and a flat file cheap to grep for a reader holding only a word. What travels is each type's own decision,
declared beside the type, so a corpus adopting a new type exports it without the tool changing.
[The export format](../export-format.md) is the contract those files answer to.

## What it is not

**It is not [`bundle`](bundle.md).** `export` produces data. `bundle` assembles that data and the `.plugin/` tree into
something a consumer can install. Nothing here trims components, packages a plugin or publishes anything, and nothing
here knows a bundle exists.

**It is not [`generate`](generate.md).** `generate` writes into the corpus, for a person reading the corpus. `export`
writes outside it, for something that will never open the Markdown. Both are built from the same frontmatter, and
neither is derivable from the other, because they answer to different readers.

**It is not a backup.** A record travels as the fields and sections its type declared, so a corpus cannot be rebuilt
from an export of it. The direction is one way: `.dist/export/` is rebuilt whole from the corpus.

## How it works

A run loads the corpus whole, then decides which records travel and what of each one goes with them. It deletes
`.dist/export/` and writes it again: a manifest, one file per record, and one flat file per type. Every link it writes
resolves against the commit it was built from.

### `--type` narrows what is written and never what is read

The corpus is loaded whole whatever the flag says, so ids resolve against every record. A narrowed run would otherwise
resolve them against the handful it happened to want, and a question about a set answered from some of its members is
answered wrongly. A type the corpus has not adopted is refused by name.

### An unsettled record travels by default

A draft glossary, and one whose `review-by` has passed, are both exported carrying their own state. Filtering them
would make the corpus's own condition invisible downstream.

A corpus may exclude either with `export.exclude:` in [`.corpus.yaml`](../corpus-descriptor.md). Where it does, the run
names every record it withheld, because a record left out of the output cannot be seen there.

### The export is untracked

`.dist/` is gitignored and the export inside it is rebuilt whole, so it is never something to review. A tracked export
would put a diff nobody reads on every change to the words.

Two things follow. The overwrite is delete-then-write, because a record deleted from the corpus must not leave an entry
behind and no diff would show the orphan. And the manifest has to describe itself, since git can say nothing about an
export once it has left: it carries the commit it was built from, and a dirty flag beside it. A commit on its own would
describe a dirty tree as reproducible.

What holds the shape steady in place of a diff is a committed fixture in the framework's own test suite. It exports a
corpus and compares the whole tree file by file, so a corpus that runs the tool without the tests receives a format
already proved.

## Known limits

**A record's exported field is a scalar**, and three more limits belong to a type's declaration rather than to this
command. [The export format](../export-format.md) states each one and what it costs a consumer.
