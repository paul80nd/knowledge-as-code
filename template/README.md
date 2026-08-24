# `template`: what a corpus is made of

Everything a corpus receives is authored here beyond its schema: the framework's own documentation, the tree a corpus's
plugin is built from, and the pages and templates a corpus starts writing against. The machine-readable schema `kac`
enforces is authored at [`../.schema/`](../.schema/), one copy above this folder and `../example/` alike.

**A new corpus copies this folder and that schema.** Take everything here but this file, which describes the template
rather than a corpus.
[`../README.md`](../README.md#starting-a-corpus-of-your-own) has the steps, and a copy validates clean before a word is
written.

[`../manifest.yaml`](../manifest.yaml) classifies every file into a layer, and is the only place that decision is
recorded. It sits at the repository root rather than here, because a template is not a folder: it is the set of files
the manifest names, read from wherever they are authored.

| Layer        | What it means                                                                             |
|--------------|-------------------------------------------------------------------------------------------|
| **overlay**  | Identical in every corpus, and written again whenever the corpus takes a newer framework. |
| **seed**     | Written once, when the corpus is created, and never touched again.                        |
| **removed**  | A tombstone. Deleted from a corpus when it takes a newer framework.                       |
| **withheld** | This repository's own machinery. Never written into a corpus at all.                      |

The rules are read in order and the first match wins, so a rule carving one file out of a folder is written above the
rule claiming that folder. A rule's `to:` says where its files land in a corpus, which is how a file authored here
reaches a corpus's own root.

Two catch-alls sit at the foot. The first seeds anything in this folder nobody classified, so a corpus's own edit is
never overwritten by a file no one thought about. The second withholds everything else, because a file elsewhere in the
repository that nobody classified is far more likely to be the repository's own.

## Its relationship to `example/`

[`../example/`](../example/) is a corpus that took this template. It holds its own copy of every overlaid file, real,
visible and git-tracked, the way a corpus keeps them.

Two copies of one file is what this arrangement is for, and also how it goes quietly wrong. `TemplateTests` in
[`../tooling/kac.tests`](../tooling/kac.tests) holds the two to matching in both directions. It catches an overlaid file
the corpus changed, and one the corpus has that the template does not.

**Author an overlaid change here, then copy it across.** Both trees are checked, so the order is a habit rather than a
rule. The template is where the file belongs, and the corpus is where a copy of it lives.

Every test layer reads the schema straight from [`../.schema/`](../.schema/), where it is authored. A schema edit
surfaces as a broken golden in the same run.

## What is not here

`.corpus.yaml` is not a template file, though one sits here. It is present so that `kac` reads this folder as a corpus
and can validate it, and the manifest withholds it. A corpus's descriptor names the corpus, says where it publishes, and
lists the types it adopted, and no copied file could answer any of that.

A `README.md` for the corpus is not here either. This folder's own is withheld, and what a corpus says about itself is
the one page nobody else can write.

The repository-shaped files are here: [`.gitignore`](.gitignore), [`.gitattributes`](.gitattributes),
[`.editorconfig`](.editorconfig), [`.order`](.order) for an Azure DevOps wiki, and a starter pipeline for each of the
two hosts. The first two are load-bearing. `kac` reads the git listing, so a corpus without an ignore file tracks
`.dist/` and `_reports/` and then validates its own generated output as records, and without `eol=lf` a Windows checkout
trips `generate --check`.
