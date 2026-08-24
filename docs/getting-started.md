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
cp -R template/ ../my-corpus && cd ../my-corpus
rm manifest.yaml README.md  # the template's own machinery, not a corpus's

# write .corpus.yaml, which The corpus descriptor covers
git init && git add -A      # kac reads the git listing, so a corpus is a repository
```

[The corpus descriptor](corpus-descriptor.md) is what to write next, and the one file no template can supply.

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
