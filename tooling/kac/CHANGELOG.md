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

## 0.2.0 - 2026-08-20

### Changed

- **`kac index` is now `kac generate`.** The command writes each type's `_index.md` *and* rewrites the generated blocks
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

- `kac validate` — holds a corpus to the schema it carries: frontmatter, identity, structure, clauses, links, the graph
  and the type setup.
- `kac index` — regenerates `_index.md` and the generated blocks in each type page. `--check` reports what is stale
  rather than writing it.
- `kac checks` — lists every check the validator implements, read from the schema rather than from a list in the tool.
- `kac export` — writes the corpus to `.dist/export/` as data a consumer reads instead of cloning.
- `kac bundle` — assembles that export and `.plugin/` into an installable plugin.
- `kac mechanism` — compares the shared layers against a reference corpus, or takes them from one.
