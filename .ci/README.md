# `.ci` — the knowledge-as-code tooling

`kac` validates and generates a knowledge corpus against the machine-readable schema in
`knowledge-as-code/schema/`. It is a **.NET 10 file-based app** — no project file, no build step to manage. The schema
is the source of truth: `kac` reads it and enforces it, so **adding a knowledge type is adding a YAML file, not editing
this tool**.

## Running

```bash
dotnet run .ci/kac.cs -- validate            # validate the whole repo
dotnet run .ci/kac.cs -- validate adrs/      # validate a subtree or file
dotnet run .ci/kac.cs -- validate --json     # machine-readable summary + findings
dotnet run .ci/kac.cs -- index               # regenerate indexes and blocks
dotnet run .ci/kac.cs -- index --check       # verify generated output is fresh
dotnet run .ci/kac.cs -- checks              # list every check the validator implements
dotnet run .ci/kac.cs -- checks --json       # …as JSON (the test suite reads this)
dotnet run .ci/kac.cs -- mechanism --check --against ../other-corpus   # synced-layer drift vs a reference
```

The rules are covered by a golden-file suite — see [`tests/README.md`](tests/README.md):

```bash
dotnet run .ci/kac-tests.cs                  # run fixtures, diff findings against goldens
dotnet run .ci/kac-tests.cs -- --update      # regenerate goldens after an intended rule change
```

Argument parsing is handled by [`System.CommandLine`](https://www.nuget.org/packages/System.CommandLine), so every
command and option carries generated `--help`. The first run restores `YamlDotNet`, `Markdig` and `System.CommandLine`
and is slow; subsequent runs are cached. Run **one `kac` invocation per subcommand** — file-based apps share build
output and contend if run concurrently.

### Exit codes

| Code | Meaning                                                                         |
|------|---------------------------------------------------------------------------------|
| `0`  | No errors. Warnings may still have been printed.                                |
| `1`  | A corpus **error**, or a bad invocation (missing/unknown subcommand or option). |
| `2`  | Could not locate the repo root — the tool never started.                        |

Warnings never change the exit code.

## What gets validated

`kac` discovers Markdown via `git ls-files` (so **`.gitignore`, `.git/info/exclude` and global excludes are respected**,
and `.git/` is never walked), then applies the taxonomy exclusions from `knowledge-as-code/automation.md`:

- `knowledge-as-code/`, `_plan/`, `_reports/`, and `.git/` `.idea/` `.claude/`
- `**/template.md`, `**/INDEX.md`
- root `README.md`, root `CLAUDE.md`, and each type's root page (`adrs.md`, `services.md`, …)
- anything outside a folder that maps to a type schema

A document is validated **only if it carries a YAML frontmatter block** — that is how a document opts into the schema.
Files in a type folder without frontmatter are counted as *skipped (not yet migrated)* and reported in the summary, not
failed. Today that means **only ADRs are validated**; every other type is pre-migration prose and is skipped until it
gains frontmatter.

## Checks

All rules are read from the schema; nothing below is hard-coded per type. A rule marked **warning**
is `severity: warning` in the schema (or a soft heuristic) and does **not** fail the build.

### Frontmatter (from `_universal.yaml` + `<type>.yaml`)

| Check                                             | Level | What it enforces                                                                           |
|---------------------------------------------------|-------|--------------------------------------------------------------------------------------------|
| `frontmatter-parses`                              | error | The block is valid YAML and a mapping.                                                     |
| `unknown-key`                                     | error | Every key is a universal field, a type field, or a reserved ADO key.                       |
| `key-order`                                       | error | Order is a **topological extension** of the schema's declared field orders — see below.    |
| `required-field`                                  | error | Every `required` field (and every `required-when` field whose condition holds) is present. |
| `bare-key`                                        | error | An absent value is a bare key (`decided-on:`), never `null`, `~`, `""`, `''` or `—`.       |
| `date-quoted` / `date-format`                     | error | `type: date` fields are quoted and `YYYY-MM-DD` in shape.                                  |
| `enum` / `enum-lowercase`                         | error | `type: enum` values are in range and lowercase.                                            |
| `tier-matches-type`                               | error | `tier` equals the tier the type declares.                                                  |
| `id-prefix` / `id-format` / `id-matches-filename` | error | `id` has the type's prefix and width, and its number matches the filename.                 |

### Identity & structure (from `<type>.yaml`)

| Check                          | Level | What it enforces                                                        |
|--------------------------------|-------|-------------------------------------------------------------------------|
| `filename-pattern`             | error | Filename matches the type's `filename.pattern`.                         |
| `slug-length`                  | error | The slug (filename minus the `NNNN-` prefix) is within `slug-max` (30). |
| `h1-pattern` / `h1-matches-id` | error | The H1 matches `title.h1-pattern` and its number matches the id.        |
| `required-section`             | error | Every heading in `sections.required` is present.                        |

### Links & the graph

| Check                     | Level   | What it enforces                                                                                                                                                                                                                                                                        |
|---------------------------|---------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `link-resolves`           | error   | Every internal link resolves. All forms are handled: repo-relative, wiki-root-absolute (a leading `/` = repo root), inline, reference and **shortcut reference** links; `.md` may be omitted (ADO resolves both). External `http(s)/mailto/tel` and pure `#fragment` links are skipped. |
| `undefined-label`         | error   | An `[ADR-NNNN]` shortcut with no link definition.                                                                                                                                                                                                                                       |
| `related-matches-section` | error   | A `mirrors-section` field (e.g. `related`) reconciles case-insensitively with the ids referenced in that section (`## Related`).                                                                                                                                                        |
| `id-unique`               | error   | `id` is unique across the whole wiki.                                                                                                                                                                                                                                                   |
| `reciprocal`              | error   | A `reciprocal` field agrees in both directions (`supersedes` ⇄ `superseded-by`) and points at a document that exists.                                                                                                                                                                   |
| `unused-definition`       | warning | A link definition that nothing references.                                                                                                                                                                                                                                              |
| `bracket-literal`         | warning | A `[...]` left in prose that looks like a reference but has no definition (use an inline link if it is deliberate).                                                                                                                                                                     |

### Content quality (schema `rules` with `severity: warning`)

| Check                  | Level   | What it enforces                                                                                                                      |
|------------------------|---------|---------------------------------------------------------------------------------------------------------------------------------------|
| `y-statement`          | warning | A block-quote follows the H1 and is within `max-words` (60).                                                                          |
| `alternatives-verdict` | warning | Each *Alternatives Considered* bullet states an outcome. Heuristic: an explicit verdict word or a contrastive / negative-outcome cue. |

Code is excluded from every link and marker check: they walk the Markdig AST (inline links, literal runs), and fenced or
indented code carries none of those nodes.

## The key-order rule

The schema specifies field order across two files that share the `status` key: `_universal.yaml`
(`id, tier, status, owner, tags`) and the type file (for ADRs: `status, decided-on, supersedes,
superseded-by, deciders, related`). Neither states a single total order for the merged set.

Rather than invent one arbitrary total order, `kac` enforces that a document's key order is a **topological extension**
of both declared chains: every pair the schema *does* order must hold, and pairs it leaves unconstrained (e.g. `owner`
versus `decided-on`) are free. This is fully derived from the schema, matches both the `metadata.md` example and the ADR
corpus, and still catches genuine disorder (`tags` before `id`, `related` before `status`, and so on).

## `index` — generation

`index` regenerates content that is derived from frontmatter and the schema, so it never has to be maintained by hand:

| Artefact                                        | Built from                          | Rule                                                                                                         |
|-------------------------------------------------|-------------------------------------|--------------------------------------------------------------------------------------------------------------|
| `<type>/INDEX.md`                               | frontmatter across the folder       | Regenerated **wholly**; columns and sort come from the schema's `index` block; carries a do-not-edit banner. |
| `<!-- … schema-<type> -->` block in `<type>.md` | the type's `fields`                 | The frontmatter reference table.                                                                             |
| `<!-- … checks-<type> -->` block in `<type>.md` | the checks the validator implements | The "What CI checks" table.                                                                                  |

Only the region **between** each `BEGIN`/`END` marker is rewritten; the rest of `<type>.md` is byte-preserved. A type is
regenerated only if it has at least one frontmatter-bearing record, so unmigrated types are never touched — today that
means `adrs/INDEX.md` and the two blocks in
`adrs.md`.

Three rules hold this together:

- **CI never commits.** `index` writes locally. In CI, run `index --check`: it recomputes the generated content, and if
  any file differs it prints the stale files, names the command to run (`dotnet run .ci/kac.cs -- index`), and exits
  `1`. A pipeline never pushes.
- **Output is byte-stable.** Generation is a pure function of frontmatter + schema, so running
  `index` twice produces no diff. Tables use fixed column widths, `|` is escaped, and files are LF with a trailing
  newline — so if a Markdown formatter is added later, the freshness check keeps working instead of failing forever.

## `mechanism` — portability

`manifest.yaml` declares each file's layer — `synced`, `forked`, `generated`, `local`, `ignored` — but the declaration
needs enforcing. `mechanism --check` resolves every tracked file against the manifest and compares the **synced** layer
against a reference corpus, following the same discipline as `index --check`: recompute, compare, name what differs,
exit non-zero, never write.

```bash
dotnet run .ci/kac.cs -- mechanism --check --against ../other-corpus
```

The reference defaults to `upstream.url` in `knowledge-as-code/mechanism.lock`, so a consumer that records where it
synced from can run a bare `mechanism --check`. What it reports:

- **synced** files that differ, are missing on either side, or match no manifest rule at all — each an **error** (exit `1`).
- **forked** files are compared too, but only counted: how many differ from the reference is informational and never fails.
- **generated**, **local** and **ignored** files are skipped — each corpus owns its own.
- **accepted divergences** listed in `mechanism.lock` are honoured rather than flagged as drift, and any that have
  quietly become identical to the reference again are named as `RESOLVED` so the stale entry can be removed.

Comparison is LF-normalised, so line-ending differences never read as drift. `mechanism --sync` — the write half that
copies the synced layer into a consumer — is not implemented yet.

## Known gaps

- **`immutable-after-accepted`** (content of an accepted document must not change) needs git history and is not
  implemented in the static validator; it belongs in a diff-aware CI step.
- **Date validity** is checked for *shape* (`YYYY-MM-DD`), not calendar validity (`2026-13-40`
  passes the shape check).
