# kac: knowledge as code

`kac` reads a folder of Markdown documents kept in git and holds each one to a schema. The folder a document sits in
names its type. The corpus's own `.schema/` holds one file per type, saying what fields and rules that type carries. A
repository of those documents, with the schema it runs, is a **corpus**.

What the structure buys you: indexes built from the documents themselves, and a broken cross-reference that fails CI
rather than rotting quietly. An agent contributing to the corpus can be told where a document goes.

`kac` carries no rules about any particular type. It reads what to enforce from the schema the corpus holds, so you add
a knowledge type by writing a YAML file.

## Install

```bash
dotnet tool install --global KnowledgeAsCode.Tool
```

## Start a corpus

`kac new` turns the folder you are standing in into one. It takes the framework from the repository below, writes what a
corpus is made of, and stops short of committing so you can read what it staged.

```bash
mkdir my-corpus && cd my-corpus
kac new          # asks four things, and has a default for each
```

## Use

`kac` finds a corpus by walking up from the working directory for a `.corpus.yaml`, the descriptor naming the corpus, so
run it from anywhere inside one.

```bash
kac validate     # frontmatter, links, structure, clauses and the graph
kac generate     # regenerate the indexes and the generated blocks in each type page
kac update       # take a newer framework from the template this corpus was made from
kac export       # write the corpus to .dist/export/ as data a consumer reads instead of cloning
kac bundle       # assemble that export and .plugin/ into an installable plugin
kac pack         # seal that export into a versioned package a registry can hold
kac checks       # list every check the validator implements
```

`kac update` writes into the working tree and commits nothing, so `git diff` is where you read what arrived.
`kac update --check` reports the same and writes nothing, which is the form a pipeline runs.

`kac --help` lists them, and every command carries its own `--help`.

## Exit codes

| Code | Meaning                                                                       |
|------|-------------------------------------------------------------------------------|
| `0`  | No errors. Warnings may still have been printed.                              |
| `1`  | A corpus error, or a bad invocation.                                          |
| `2`  | A verb found no corpus. `--version` and `--help` need none and answer anyway. |

## Where to go next

The repository this tool is built from carries the framework's documentation, a page for each record type, and a
reference for every command:

**<https://github.com/paul80nd/knowledge-as-code>**

It also holds a worked corpus you can read before you write your own.

Released under the MIT licence.
