# kac — knowledge as code

`kac` validates and generates a knowledge corpus: markdown records in git, each with a type, and each type with a
machine-readable schema the corpus carries in `.schema/`. An index is generated rather than maintained, a broken
cross-reference fails CI rather than rotting quietly, and an agent can be told where a thing goes instead of guessing.

The tool holds no type-specific rules. Everything it enforces is read from the corpus's own schema, so adding a
knowledge type is adding a YAML file rather than changing the tool.

## Install

```bash
dotnet tool install --global KnowledgeAsCode.Tool
```

## Use

`kac` finds a corpus by walking up from the working directory for a `.schema/`, so run it from anywhere inside one.

```bash
kac validate     # frontmatter, links, structure, clauses and the graph
kac generate     # regenerate the indexes and the generated blocks in each type page
kac export       # write the corpus to .dist/export/ as data a consumer reads instead of cloning
kac bundle       # assemble that export and .plugin/ into an installable plugin
kac checks       # list every check the validator implements
```

`kac --help` lists them, and every command carries its own `--help`. Every one takes `--no-color`, and reads
`NO_COLOR` from the environment. A redirected stream carries no colour either way.

## Exit codes

| Code | Meaning                                                                       |
|------|-------------------------------------------------------------------------------|
| `0`  | No errors. Warnings may still have been printed.                              |
| `1`  | A corpus error, or a bad invocation.                                          |
| `2`  | A verb found no corpus. `--version` and `--help` need none and answer anyway. |

## What a corpus is made of, and where the rest lives

A corpus is a folder of typed markdown plus the `.schema/` describing those types. Documentation for the framework, the
record types, and the reference for each command are in the repository this tool is built from:

**<https://github.com/paul80nd/knowledge-as-code>**

Released under the MIT licence.
