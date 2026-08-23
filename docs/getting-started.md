# Getting started

## Install it

`kac` is published as the dotnet tool `KnowledgeAsCode.Tool` and needs the **.NET 10 SDK**. Installing it globally puts
`kac` on your `PATH`:

```bash
dotnet tool install --global KnowledgeAsCode.Tool
kac --version
```

A corpus with CI of its own wants the version pinned instead, so that every machine and every build runs the version
the corpus was written against. Install it into a
[tool manifest](https://learn.microsoft.com/dotnet/core/tools/local-tools-how-to-use). The version lands in
`.config/dotnet-tools.json` and travels with the repository.

```bash
dotnet new tool-manifest
dotnet tool install KnowledgeAsCode.Tool

dotnet tool run kac validate
```

Neither install carries a corpus, and neither needs one. `--version` and `--help` are answered from wherever you typed
the command.

## Run it against a corpus

`kac` finds a corpus by walking up from the working directory for a `.schema/`, so run it from inside one. Where the
tool's own files sit says nothing about which corpus it reads.

```bash
cd path/to/your/corpus

kac validate            # validate the corpus
kac validate --json     # machine-readable summary and findings
kac generate            # regenerate indexes and blocks
kac generate --check    # verify generated output is fresh
kac export              # write the corpus to .dist/export/ as data a consumer reads
kac export --type glossary                        # one type, of the several that contribute
kac bundle              # assemble that export and .plugin/ into a plugin under .dist/plugin/
kac checks              # list every check the validator implements
kac checks --json       # the same catalogue as JSON
kac mechanism --check --against ../other-corpus   # shared-layer drift against a reference
kac mechanism --sync                              # take the shared layers from upstream
```

## Colour

Every verb takes `--no-color`, and every verb reads `NO_COLOR` from the environment. `NO_COLOR` is the cross-tool
standard for the same request, and the flag is there for a caller who cannot set a variable.

A redirected stream carries no colour on its own. An environment naming a runner that renders escapes in its logs turns
it back on, and GitHub Actions is one such runner. Set `NO_COLOR` wherever the bytes have to be the same everywhere.

## Exit codes

| Code | Meaning                                                                            |
|------|------------------------------------------------------------------------------------|
| `0`  | No errors. Warnings may still have been printed.                                   |
| `1`  | A corpus **error**, or a bad invocation (missing or unknown subcommand or option). |
| `2`  | A verb found no corpus. `--version` and `--help` need none and answer anyway.      |

Warnings never change the exit code.

## Start a corpus from nothing

There is no corpus to run against until one exists. Copy
[`template/`](https://github.com/paul80nd/knowledge-as-code/tree/main/template), which is the corpus with the content
taken out: the schema, the framework's own documentation, a root page, and a template for every type. Copy `example/`
instead and you inherit a fictional library consortium to delete.

```bash
cp -R template/ ../my-corpus && cd ../my-corpus
rm manifest.yaml README.md          # the template's own machinery, not a corpus's

# write .corpus.yaml, which The corpus descriptor covers
git init && git add -A              # kac reads the git listing, so a corpus is a repository

kac generate                        # write the indexes and generated blocks
kac validate                        # comes back clean on an empty corpus
```

[The corpus descriptor](corpus-descriptor.md) is what to write next, and the one file no template can supply.

You also arrive with no `README.md`, no ignore rules, no editor conventions and no CI. Each of those is a question about
your repository rather than about the framework.
