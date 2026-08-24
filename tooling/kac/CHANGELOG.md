# Changelog

> What changed in each published version of `kac`.

This page covers the tool, published to nuget.org as
[`KnowledgeAsCode.Tool`](https://www.nuget.org/packages/KnowledgeAsCode.Tool). The same repository holds the schema, the
framework's documentation and the pages a corpus starts from. Those carry no version of their own and reach a corpus
through `kac mechanism`, so nothing about them is recorded here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Versions sit below 1.0.0 while the command
surface may still change shape.

A push to `main` publishes whenever `kac.csproj` names a version nuget.org does not already hold, and that publish tags
the commit and opens a release carrying the section for that version.

## Unreleased

### Changed

- **`kac mechanism --help` reads its two option descriptions as sentences.** `--check` closed on a semicolon,
  and `--against` opened on a bare noun phrase. What either flag does has not moved.

## 0.3.0 - 2026-08-23

### Added

- **`--no-color` on every verb.** `NO_COLOR` in the environment asks for the same thing, and the tool already read it.
  Colour goes either way, and bold stays.

### Changed

- **`generate` writes a relative link naming the file**, where it wrote a root-relative link naming the folder. A block
  in `README.md` links `[ADR](adrs.md)`, and one in `knowledge-as-code/taxonomy.md` links `[ADRs](../adrs.md)`. The link
  resolves wherever the corpus sits, rather than only where a renderer maps a folder to the page inside it. Run
  `kac generate` after upgrading: `--check` reports every block carrying the old form until you do.
- **`validate` and `checks` list in aligned columns**, with the severity coloured. Only the message column wraps, so a
  narrow terminal breaks a sentence and never a check id. `checks` splits its count by severity.
- **`generate` marks a file it created**, and counts what it wrote against the size of the whole plan.
- **`export` and `bundle` dim the directory in each path they write**, and colour a remark by whether it is advice or
  an account of the run. Neither changes a word it prints.
- **A failure is red on stderr.** That covers every verb's hard stop, and the heading over a list of what stopped it.
  What the heading names stays plain beneath it.
- **`--json` and every exit code answer as before.** `--json` goes straight to the stream and never carries colour,
  whatever the terminal.
- **Two messages lose a semicolon the house style does not keep.** The `filename / slug-length` row in every generated
  checks table, and the meta-test reporting an over-long description. Run `kac generate` after upgrading: `--check`
  reports every type page carrying the old wording until you do.

## 0.2.1 - 2026-08-21

### Changed

- **The command line is parsed by `Spectre.Console.Cli` rather than `System.CommandLine`.** Every verb, option and exit
  code answers as it did. `--help` reflows into Spectre's layout, `-v` joins `--version`, and `-?` no longer stands for
  `--help`. The tool carries one library for reading a command line and asking a question, rather than two.

## 0.2.0 - 2026-08-20

### Changed

- **`kac index` is now `kac generate`.** The command writes each type's `_index.md` and rewrites the generated blocks
  in every type page, and only the first of those is an index. `--check` is unchanged, and so is everything either half
  writes. There is no alias: a pipeline or script still naming `index` fails until it names `generate`.

## 0.1.1 - 2026-08-20

### Added

- An icon on the nuget.org package page.
- A link from the package page to the release notes for the version being installed.

The tool answers exactly as 0.1.0 does. Only what nuget.org shows about it changed.

## 0.1.0 - 2026-08-20

The first published version.

### Added

- `kac validate` holds a corpus to the schema it carries: frontmatter, identity, structure, clauses, links, the graph
  and the type setup.
- `kac index` regenerates `_index.md` and the generated blocks in each type page. `--check` reports what is stale
  rather than writing it.
- `kac checks` lists every check the validator implements, read from the schema rather than from a list in the tool.
- `kac export` writes the corpus to `.dist/export/` as data a consumer reads instead of cloning.
- `kac bundle` assembles that export and `.plugin/` into an installable plugin.
- `kac mechanism` compares the shared layers against a reference corpus, or takes them from one.
