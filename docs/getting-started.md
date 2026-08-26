# Getting started

## Install the tool

`kac` is published as the dotnet tool `KnowledgeAsCode.Tool` and needs the **.NET 10 SDK**. Installing it globally puts
`kac` on your `PATH`:

```bash
dotnet tool install --global KnowledgeAsCode.Tool
kac --version
```

## Start a corpus

`kac new` turns the folder you are standing in into a corpus. It takes the framework from the knowledge-as-code
repository, writes the files that framework says a corpus receives, and writes the two no template can supply:
`.corpus.yaml`, which names your corpus, and a `README.md` to rewrite.

```bash
mkdir my-corpus && cd my-corpus
kac new
```

It asks four things and has a default for each: what the corpus is called, which types it adopts, where it publishes,
and what builds it. Answer nothing at all and you still end with a corpus that validates. `--yes` takes every default
and asks nothing, which is what a pipeline runs.

The folder does not have to be a repository yet. `kac` reads the git listing to find what a corpus holds, so `new`
offers to run `git init` where there is none. It finishes by running `generate`, then `validate`, then `git add -A`, and
stops there. The first commit is yours to make, once you have read what is staged.

```bash
git commit -m "Start a corpus"
```

!!! note "Look at `.corpus.yaml` before you commit"

    `new` writes it from your answers. [The corpus descriptor](corpus-descriptor.md) says what every key in it means,
    and which ones you move by hand afterwards.

You arrive with ignore rules, editor conventions and a wiki ordering. Name a CI system and its starter pipeline comes
too. `new` writes that one system's and no other, so a corpus is never handed a workflow for a host it does not build
on. [`new`](cli/new.md) covers every flag, the order it asks in, and what stops it.

## Run the tool against your corpus

`kac` finds a corpus by walking up from the working directory looking for `.corpus.yaml`. Run it from inside your
corpus.

```bash
cd path/to/your/corpus

kac validate            # frontmatter, links, structure, clauses and the graph
kac generate            # rewrite the indexes and the tables inside the markers
kac export              # write what the corpus knows to .dist/export/, as data
kac bundle              # assemble that export and .plugin/ into a plugin under .dist/
```

Every command takes the same few options. Each answers with one of three exit codes.
[The CLI reference](cli/index.md) covers both, and gives a page to every command.

## Add your first record

Your corpus arrives holding three records. The ADR under `adrs/` and the policy under `policies/` are there to show the
shape, and yours go beside them. The glossary under `glossary/` is the framework's own vocabulary, inherited word for
word: write your own glossaries beside it and leave that one as it is.

1. **Pick the type.** `knowledge-as-code/taxonomy.md` in your own corpus has a decision table saying where a record
   goes, covering the types that corpus adopted. [The default types](framework/types.md) introduces every one of them.
2. **Copy that type's `_template.md`** to a new file in the same folder. It marks what you supply as `{{placeholder}}`
   and fences its own guidance between `DELETE FROM HERE` and `DELETE TO HERE` comments. A finished record has neither
   left in it.
3. **Run `kac validate`, then `kac generate`.** The first names what is still missing. The second writes your record
   into the folder's index.

[Metadata](framework/metadata.md) says how an id is formed and how a citation reaches a part of a record.
[Running it in CI](ci.md) is what to read once a record of your own passes locally.
