# `new` stand a corpus up in the folder you are in

<!-- BEGIN GENERATED: usage-new -->

```text
kac new [--ci <SYSTEM>] [--from <URL|PATH>] [--name <NAME>] [--no-color] [--path <PATH>] [--publishing <TARGET>] [--ref <REF>] [--types <TYPES>] [--yes]
```

| Option                  | What it does                                                                            |
|-------------------------|-----------------------------------------------------------------------------------------|
| `--ci <SYSTEM>`         | What builds the corpus: github, azure-devops or none.                                   |
| `--from <URL\|PATH>`    | The repository or folder serving the template. Defaults to the framework's own.         |
| `--name <NAME>`         | What the corpus is called. Defaults to the name of this folder.                         |
| `--no-color`            | Turn colour off. NO_COLOR in the environment does the same.                             |
| `--path <PATH>`         | The folder inside that repository holding manifest.yaml, where it is not at the root.   |
| `--publishing <TARGET>` | Where the corpus is published: github, azure-devops, azure-devops-wiki, mkdocs or none. |
| `--ref <REF>`           | The branch or tag to take the template from.                                            |
| `--types <TYPES>`       | The types to adopt, comma-separated, or 'all'.                                          |
| `--yes`                 | Take the default for every answer not given, and ask nothing.                           |

<!-- END GENERATED: usage-new -->

## What it does

`new` turns the folder you are standing in into a corpus (one repository of knowledge records kept in git). It fetches
the framework from a template repository at a ref, writes the files that template says a corpus receives, and writes the
two a template cannot supply: `.corpus.yaml`, which states your corpus's name, and a `README.md` to rewrite where
template sends none of its own.

It asks what the corpus is called, which types it adopts, where it publishes and what builds it. Each question has a
default. Whichever types you adopt, the corpus it writes validates before the command finishes.
[Layers](../design/layers.md) says which files it writes, and who owns each one afterwards.

If you adopt only some types, `new` removes the links to the types you skipped. A type page links the other type pages,
and `new` drops each dead link and leaves the type's name standing in the sentence. A type's schema points `ref:` and
`versus:` at the types beside it, and each of those points at a type, not at a folder, so a type you declined enforces
nothing until [`update --add-type`](update.md) brings it in.

Use it once, on an empty or nearly empty folder. To take a newer framework into a corpus that already exists, run
[`update`](update.md).

## Examples

### A corpus, answering the questions

```bash
mkdir my-corpus && cd my-corpus
kac new
```

`new` asks four questions. Each has a default and each has a flag:

| Asked              | Default                          | Flag           |
|--------------------|----------------------------------|----------------|
| The corpus's name  | the folder's name                | `--name`       |
| Which types        | every type the template declares | `--types`      |
| Where it publishes | `none`                           | `--publishing` |
| Which CI system    | `none`                           | `--ci`         |

Name a publishing target and it asks a fifth: the URL the published corpus is browsed at. That one has no flag.

### A corpus with no questions

```bash
kac new --yes --name my-corpus --ci github
```

`--yes` takes the default for every answer you did not give, which is what a pipeline runs. A run with no terminal and a
missing answer exits rather than waiting, because a hung pipeline is worse than a failed one.

`new` prints each file as it writes it, then generates, validates and stages. The tail of a default run:

```text
new: did not write azure-pipelines.yml: this corpus is built by github.
new: wrote 116 file(s) for my-corpus, taken from /path/to/template.
wrote README.md
updated 1 of 42 generated file(s).
validated 3 document(s) and 19 template(s), skipped 0 without frontmatter. 0 error(s), 0 warning(s)
new: staged. `git status` shows everything this wrote, and the first commit is yours.
```

Take the framework from a URL rather than a folder and the second line adds the commit it resolved, as
`…knowledge-as-code at 3b812bb.`

`new` stops short of committing. Read what is staged, then:

```bash
git commit -m "Start a corpus"
```

### A corpus with no network

```bash
kac new --from ../knowledge-as-code
```

`--from` takes a local path as well as a URL. This is the offline route, and it is what the tool's own tests use.

## Known limits

**It needs a network and a git client**, unless you pass a local `--from`. `new` fetches the template at run time.

**It is not idempotent and does not try to be.** Run it twice in the same folder and it stops on the first check. A
`.corpus.yaml` at or above the working directory means the corpus is already there.

**It does not decide what your repository looks like.** Branch protection, reviewers and issue templates are questions
about your repository, and `new` asks none of them.

**It does not install the tool.** A `kac` already on the machine runs `new`, and `new` fetches only the rest of the
framework. The two halves are versioned apart from the moment a corpus is created.

[`update`](update.md) is what takes a newer framework into the corpus this created.
