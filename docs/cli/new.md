# `new` stand a corpus up in the folder you are in

<!-- BEGIN GENERATED: usage-new -->

```text
kac new [--ci <SYSTEM>] [--from <URL|PATH>] [--name <NAME>] [--no-color] [--path <PATH>] [--publishing <TARGET>] [--ref <REF>] [--types <TYPES>] [--yes]
```

| Option                  | What it does                                                                          |
|-------------------------|---------------------------------------------------------------------------------------|
| `--ci <SYSTEM>`         | What builds the corpus: github, azure-devops or none.                                 |
| `--from <URL\|PATH>`    | The repository or folder serving the template. Defaults to the framework's own.       |
| `--name <NAME>`         | What the corpus is called. Defaults to the name of this folder.                       |
| `--no-color`            | Turn colour off. NO_COLOR in the environment does the same.                           |
| `--path <PATH>`         | The folder inside that repository holding manifest.yaml, where it is not at the root. |
| `--publishing <TARGET>` | Where the corpus is published: github, azure-devops-wiki, mkdocs or none.             |
| `--ref <REF>`           | The branch or tag to take the template from.                                          |
| `--types <TYPES>`       | The types to adopt, comma-separated, or 'all'.                                        |
| `--yes`                 | Take the default for every answer not given, and ask nothing.                         |

<!-- END GENERATED: usage-new -->

## What it does

`new` turns the folder you are standing in into a corpus. It fetches the framework from a template repository at a ref,
writes the files that template says a corpus receives, and writes what no template can supply: `.corpus.yaml`, which
names your corpus, and a `README.md` to rewrite where the template sends none of its own.

It asks what the corpus is called, which types it adopts, where it publishes and what builds it, with a default for
each. Answer nothing at all and you still end with a corpus that validates, holding every type the template declares.
[Layers](../design/layers.md) says which files it writes and who owns each one afterwards.

Use it once, on an empty or nearly empty folder. Taking a newer framework into a corpus that already exists is
[`update`](update.md).

## Examples

### Create a corpus, answering the questions

```bash
mkdir my-corpus && cd my-corpus
kac new
```

It asks for the corpus's name, which types to adopt, where it publishes and which CI system builds it. Each has a
default and each has a flag. Name a publishing target and it asks two more, for where a person reads a record and
where an agent fetches one, and those two have no flag.

| Asked              | Default                          | Flag           |
|--------------------|----------------------------------|----------------|
| The corpus's name  | the folder's name                | `--name`       |
| Which types        | every type the template declares | `--types`      |
| Where it publishes | `none`                           | `--publishing` |
| Which CI system    | `none`                           | `--ci`         |

### Create one without being asked anything

```bash
kac new --yes --name my-corpus --ci github
```

`--yes` takes the default for every answer not given, which is what a pipeline runs. A run with no terminal and a
missing answer exits rather than waiting, because a hung pipeline is worse than a failed one.

It names each file as it writes it, then generates, validates and stages. The tail of a default run:

```text
new: did not write azure-pipelines.yml: this corpus is built by github.
new: wrote 101 file(s) for my-corpus, taken from /path/to/template.
updated 1 of 38 generated file(s).
validated 3 document(s) and 17 template(s), skipped 0 without frontmatter. 0 error(s), 0 warning(s)
new: staged. `git status` shows everything this wrote, and the first commit is yours.
```

Taking the framework from a URL rather than a folder adds the commit it resolved, as `…knowledge-as-code at 3b812bb.`

It stops short of committing. Read what is staged, then:

```bash
git commit -m "Start a corpus"
```

### Create one with no network

```bash
kac new --from ../knowledge-as-code
```

`--from` accepts a local path as well as a URL. This is the offline route, and it is what the tool's own tests use.

## Known limits

**A corpus that declines types arrives with links to what it declined.** The type pages cross-reference each other, so
`glossary.md` names `services.md` whether or not you adopted services. `new` validates before it finishes and names the
fault for what it is:

```text
glossary.md
  error  [link-resolves]  link target 'services.md' does not resolve.  (glossary.md:36)

validated 2 document(s) and 2 template(s), skipped 0 without frontmatter. 6 error(s), 0 warning(s)
new: staged. `git status` shows everything this wrote, and the first commit is yours.
new: the corpus this created does not validate. a page it received links to a type this corpus declined. those pages
are yours from here, so edit the links out. the files are written and staged.
```



**It needs a network and a git client**, unless you pass a local `--from`. The template is fetched at run time.

**It is not idempotent and does not try to be.** Running it twice in the same folder stops on the first check. A
`.corpus.yaml` at or above the working directory means the corpus is already there.

**It does not decide what your repository looks like.** Branch protection, reviewers and issue templates are questions
about your repository, and it asks none of them.

**It does not install the tool.** `new` is run by a `kac` already on the machine, and fetches only the rest of the
framework. The two halves are versioned apart from the moment a corpus is created.

[`update`](update.md) is what takes a newer framework into the corpus this created.
