# Getting started

## Install the tool

`kac` is published as the dotnet tool `KnowledgeAsCode.Tool` and needs the **.NET 10 SDK**. Installing it globally puts
`kac` on your `PATH`:

```bash
dotnet tool install --global KnowledgeAsCode.Tool
kac --version
```

## Start a corpus

There is no corpus to run against until one exists. Copy
[`template/`](https://github.com/paul80nd/knowledge-as-code/tree/main/template), which is the corpus with the content
taken out: the schema, the framework's own documentation, a root page, and a template for every type. Copy `example/`
instead and you inherit a fictional library consortium corpus.

```bash
git clone https://github.com/paul80nd/knowledge-as-code
cp -R knowledge-as-code/template/ my-corpus && cd my-corpus
rm manifest.yaml README.md  # the template's own machinery, not a corpus's
```

!!! note "Write `.corpus.yaml` next"

    It is the one file no template can supply, because no template can name your corpus for you.
    [The corpus descriptor](corpus-descriptor.md) says what goes in it.

`kac` reads the git listing to find what a corpus holds, so a corpus is a repository. Commit before you export, because
an export stamps the commit it was built from. A dirty tree gets a manifest naming a commit that does not reproduce it.

```bash
git init && git add -A
git commit -m "Start a corpus from the knowledge-as-code template"
```

You also arrive with no `README.md`, no ignore rules, no editor conventions and no CI. Each of those is a question for
you about your repository rather than about the framework.

## Run the tool against your corpus

`kac` finds a corpus by walking up from the working directory looking for `.schema/`. Ensure you're running it from
within your corpus.

```bash
cd path/to/your/corpus

kac validate            # validate the corpus
kac generate            # regenerate indexes and blocks
kac export              # write the corpus to .dist/export/
kac bundle              # assemble that export and .plugin/ into a plugin under .dist/plugin/
```

Every command takes the same few options and answers with one of three exit codes.
[The CLI reference](cli/index.md) covers both, and lists all the available commands.

## Add your first record

Your corpus arrives holding three records, one each under `adrs/`, `policies/` and `glossary/`. They are there to show
the shape, and yours go beside them.

1. **Pick the type.** A **type** is a category such as a policy, a runbook or a glossary, and each is a folder with a
   page above it. `knowledge-as-code/taxonomy.md` in your own corpus has a decision table saying where a document goes.
2. **Copy that type's `_template.md`** to a new file in the same folder. It marks what you supply as `{{placeholder}}`
   and fences its own guidance between `DELETE FROM HERE` and `DELETE TO HERE` comments. A finished record has neither
   left in it.
3. **Run `kac validate`** for what is still missing, then `kac generate` to write your record into the folder's index.

`knowledge-as-code/contributing.md` in your own corpus carries the rest: how to allocate an id, and the link forms the
validator holds you to.
