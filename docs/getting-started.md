# Getting started

## Install the tool

`kac` is published as the dotnet tool `KnowledgeAsCode.Tool` and needs the **.NET 10 SDK**. Installing it globally puts
`kac` on your `PATH`:

```bash
dotnet tool install --global KnowledgeAsCode.Tool
kac --version
```

## Start a corpus

Copy [`template/`](https://github.com/paul80nd/knowledge-as-code/tree/main/template). It holds the schema, the
framework's own documentation, a root page and a template for every type, plus three worked records to show the shape.
Copy `example/` instead and you inherit a fictional library consortium corpus.

```bash
git clone https://github.com/paul80nd/knowledge-as-code
cp -R knowledge-as-code/template/ my-corpus && cd my-corpus
rm manifest.yaml README.md  # the template's own machinery, not a corpus's
```

!!! note "Write `.corpus.yaml` next"

    It is the one file no template can supply, because no template can name your corpus for you.
    [The corpus descriptor](corpus-descriptor.md) says what goes in it.

`kac` reads the git listing to find what a corpus holds, so make the corpus a repository. Write a `.gitignore` holding
`.dist/` first, because the commands below build into it. Then commit: an export stamps the commit it was built from,
and a dirty tree gets a manifest naming a commit that does not reproduce it.

```bash
echo '.dist/' > .gitignore
git init && git add -A
git commit -m "Start a corpus from the knowledge-as-code template"
```

You also arrive with no `README.md`, no ignore rules, no editor conventions and no CI. Each of those is a question for
you about your repository rather than about the framework.

## Run the tool against your corpus

`kac` finds a corpus by walking up from the working directory looking for `.schema/`. Run it from inside your corpus.

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

Your corpus arrives holding three records, one each under `adrs/`, `policies/` and `glossary/`. They are there to show
the shape, and yours go beside them.

1. **Pick the type.** `knowledge-as-code/taxonomy.md` in your own corpus has a decision table saying where a record
   goes, covering the types that corpus adopted. [The default types](framework/types.md) introduces all seventeen.
2. **Copy that type's `_template.md`** to a new file in the same folder. It marks what you supply as `{{placeholder}}`
   and fences its own guidance between `DELETE FROM HERE` and `DELETE TO HERE` comments. A finished record has neither
   left in it.
3. **Run `kac validate`, then `kac generate`.** The first names what is still missing. The second writes your record
   into the folder's index.

[Metadata](framework/metadata.md) says how an id is formed and how a citation reaches a part of a record.
[Running it in CI](ci.md) is what to read once a record of your own passes locally.
