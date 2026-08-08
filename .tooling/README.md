# `.tooling` — the knowledge-as-code tooling

`kac` validates and generates a knowledge corpus against the machine-readable schema in
`.schema/`. The command you run is a **thin .NET 10 file-based entrypoint** (`kac.cs`) over a small **
`kac.core`** library that holds the mechanics; `dotnet run` builds and runs it with no build step to manage. The schema
is the source of truth: `kac` reads it and enforces it, so **adding a knowledge type is adding a YAML file, not editing
this tool**.

## Running

```bash
./kac validate            # validate the whole repo
./kac validate adrs/      # validate a subtree or file
./kac validate --json     # machine-readable summary + findings
./kac index               # regenerate indexes and blocks
./kac index --check       # verify generated output is fresh
./kac checks              # list every check the validator implements
./kac checks --json       # …as JSON (the test suite reads this)
./kac mechanism --check --against ../other-corpus   # synced-layer drift vs a reference
```

`./kac` (Windows: `kac.cmd`) is a launcher at the repo root that wraps `dotnet run .tooling/kac.cs` — run it from the
solution root, or add the root to your `PATH` to drop the `./`. The explicit `dotnet run .tooling/kac.cs -- …` form
works identically and is what CI uses.

Argument parsing is handled by [`System.CommandLine`](https://www.nuget.org/packages/System.CommandLine), so every
command and option carries generated `--help`. The first run restores the packages (`System.CommandLine` on the
entrypoint; `YamlDotNet` and `Markdig` via `kac.core`) and is slow; subsequent runs are cached. Run **one `kac`
invocation per subcommand** — file-based apps share build output and contend if run concurrently.

## Tests

Three layers, all run in CI (see [`.github/workflows/kac.yml`](../.github/workflows/kac.yml)):

| Layer       | Project / file            | Run                                 | Covers                                                                                                                                                         |
|-------------|---------------------------|-------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Unit**    | `kac.tests` (xUnit v3)    | `dotnet test .tooling/kac.tests`    | `kac.core`'s shared primitives (`Glob`, `Yaml`, `Schema` helpers, `Manifest.Resolve`, `Md`, …) — fast, precise localization.                                   |
| **Feature** | `kac.features` (Reqnroll) | `dotnet test .tooling/kac.features` | Validator **behaviour** — "what findings this document produces" — as Gherkin specs driving `kac.core` in-process.                                             |
| **Golden**  | `kac-tests.cs`            | `dotnet run .tooling/kac-tests.cs`  | Fixtures diffed against committed goldens, plus the coverage & checks-table gates and the CLI contract (exit codes). See [`tests/README.md`](tests/README.md). |

The unit layer catches breakage in the pieces early; the feature layer is the readable regression net for what the
validator does; the golden/subprocess layer owns the end-to-end CLI contract that the in-process layers bypass.
Regenerate golden expectations after an intended rule change with `dotnet run .tooling/kac-tests.cs -- --update`.

The feature layer runs `Corpus.Load` then `Validator.CheckAll`, the pair `kac validate` itself calls, so every check the
command can emit is reachable from a spec. The golden layer builds `kac.cs` once per run and invokes the built assembly,
so each scenario is a real process without paying `dotnet run`'s up-to-date check for each one.

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

- anything on a path with a `_`-prefixed segment — the reserved prefix for a framework artefact, which covers
  `**/_index.md` and `**/_template.md` as well as `_plan/` and `_reports/`
- `knowledge-as-code/`, and `.git/` `.idea/` `.claude/`
- root `README.md` and root `CLAUDE.md`
- anything outside a folder that maps to a type schema

A document is validated **only if it carries a YAML frontmatter block** — that is how a document opts into the schema.
Files in a type folder without frontmatter are counted as *skipped (not yet migrated)* and reported in the summary, not
failed.

**Type pages get a pass of their own**, chosen by the type's `shape`:

- a **`collection`** page — `adrs.md`, `services.md` — is not a record and carries no frontmatter, so the structural
  checks do not apply. It is checked for link resolution, undefined and non-canonical labels, unused definitions, and
  that its generated blocks still have their markers.
- a **`single-document`** page — `glossary.md` — *is* the record, so it is validated like any other document, plus the
  same generated-block check.

## Checks

All rules are read from the schema; nothing below is hard-coded per type. A rule marked **warning**
is `severity: warning` in the schema (or a core check that reports at that level) and does **not**
fail the build.

### The schema itself (`.schema/*.yaml`)

Before any document is read, the schema is held against what the tool can act on. A declaration nothing dispatches is
not harmlessly inert: `rules:` is documented as behaviour the validator applies, so a rule id no code answers to reads
as a commitment — and these files are copied into corpora whose authors cannot ask what a key was meant to do. Findings
name the schema file and the key rather than a record.

| Check               | Level | What it enforces                                                                                                                                                                                                                                                      |
|---------------------|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `schema-unreadable` | error | A declaration the loader cannot read as written: an `expr:` that does not compile or names no `severity:` or `message:`, a `required-when:` outside its vocabulary, `values: $enums.x` naming no enum.                                                                |
| `schema-dispatch`   | error | A value nothing acts on: a rule id claiming a `severity:` that neither an `expr:` nor a `DocumentRule` answers, a `ref:` at a folder no schema covers, `values:` on anything but an enum, an unknown `id.style` or `shape`, a `mirrors-section` other than `Related`. |
| `schema-shape`      | error | A `collection` names the `folder:` holding its records; a `single-document` type names none.                                                                                                                                                                          |

The question asked is not whether a key is spelled correctly but whether code acts on the value — `style: literal` is a
real style, and what makes it sound is the branch in `CheckId`. A rule declaring no `severity:` is exempt by design:
that is how the schema records an intention, and the type page renders those beneath the checks table as *Declared, not
yet enforced*.

### Frontmatter (from `_universal.yaml` + `<type>.yaml`)

| Check                                             | Level   | What it enforces                                                                                                                                                  |
|---------------------------------------------------|---------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`                              | error   | The block is valid YAML and a mapping.                                                                                                                            |
| `unknown-key`                                     | error   | Every key is a universal field, a type field, or a reserved ADO key.                                                                                              |
| `key-order`                                       | error   | Order is a **topological extension** of the schema's declared field orders — see below.                                                                           |
| `required-field`                                  | error   | Every `required` field (and every `required-when` field whose condition holds) is present.                                                                        |
| `bare-key`                                        | error   | An absent value is a bare key (`decided-on:`), never `null`, `~`, `""`, `''` or `—`.                                                                              |
| `date-quoted` / `date-format`                     | error   | `type: date` fields are quoted and `YYYY-MM-DD` in shape.                                                                                                         |
| `enum` / `enum-lowercase`                         | error   | `type: enum` values are in range and lowercase.                                                                                                                   |
| `field-pattern`                                   | error   | A field's value matches its declared `pattern:` — per entry for a list, per value for a scalar.                                                                   |
| `list-order`                                      | warning | A `type: list` field's entries are in alphabetical order, digit runs compared as numbers (`A.8.7` before `A.8.29`). Only the first pair out of order is reported. |
| `tier-matches-type`                               | error   | `tier` equals the tier the type declares.                                                                                                                         |
| `id-prefix` / `id-format` / `id-matches-filename` | error   | `id` has the type's prefix and width, and its number matches the filename.                                                                                        |

### Identity & structure (from `<type>.yaml`)

| Check                             | Level | What it enforces                                                          |
|-----------------------------------|-------|---------------------------------------------------------------------------|
| `filename-pattern`                | error | Filename matches the type's `filename.pattern`.                           |
| `slug-length`                     | error | The slug (filename minus the `NNNN-` prefix) is within `slug-max` (30).   |
| `h1`                              | error | The document has an H1. Its text is the title, and nothing constrains it. |
| `identity`                        | error | Two code spans follow the H1 — `` `Type: id` `STATUS` ``.                 |
| `identity-type`                   | error | The line's type name is the `label` the folder's schema declares.         |
| `identity-id` / `identity-status` | error | The line's id and status are the frontmatter's, the status upper-cased.   |
| `required-section`                | error | Every heading in `sections.required` is present.                          |

### Clauses (from `<type>.yaml`'s `clauses:` block)

A type that declares no `clauses:` block is checked for none of these.

| Check              | Level   | What it enforces                                                                                                                                                |
|--------------------|---------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `clause-table`     | error   | The declared section holds a table headed with the block's `columns`, with at least one row.                                                                    |
| `clause-id-format` | error   | Each id is a single code span matching the block's `id-pattern`.                                                                                                |
| `clause-id-unique` | error   | Ids are unique within the document — a citation names one obligation.                                                                                           |
| `clause-modal`     | error   | Each clause opens with a declared modal; `binding` ones are bold, `advisory` ones plain.                                                                        |
| `clause-order`     | warning | Rows are grouped `binding` then `advisory`, in declared order. Reported once, on the row that breaks it.                                                        |
| `clause-compound`  | warning | A clause carries one modal, not two — a second is two obligations sharing an id.                                                                                |
| `clause-ref`       | error   | A `pol-VURM.TIMEBOX` code span resolves: the document exists and carries that clause. Corpus-wide, and applies to every type, since anything may cite a clause. |

### Links & the graph

| Check                     | Level   | What it enforces                                                                                                                                                                                                                                                                        |
|---------------------------|---------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `link-resolves`           | error   | Every internal link resolves. All forms are handled: repo-relative, wiki-root-absolute (a leading `/` = repo root), inline, reference and **shortcut reference** links; `.md` may be omitted (ADO resolves both). External `http(s)/mailto/tel` and pure `#fragment` links are skipped. |
| `undefined-label`         | error   | A shortcut label shaped like any type's id (`[adr-0013]`, `[pol-DEVI]`) with no link definition.                                                                                                                                                                                        |
| `label-canonical`         | error   | An id-shaped shortcut label is written as the canonical id — prefix lower-case, mnemonic upper-case, slug lower-case. Reference and definition match case-insensitively, so a mis-cased label resolves and nothing else would catch it.                                                 |
| `related-matches-section` | error   | A `mirrors-section` field (e.g. `related`) reconciles case-insensitively with the ids referenced in that section (`## Related`).                                                                                                                                                        |
| `id-unique`               | error   | `id` is unique across the whole wiki.                                                                                                                                                                                                                                                   |
| `reciprocal`              | error   | A `reciprocal` field agrees in both directions (`supersedes` ⇄ `superseded-by`) and points at a document that exists.                                                                                                                                                                   |
| `type-setup`              | error   | A type the schema declares is stood up as both a `<type>.md` and a `<type>/` holding `_template.md`, or as neither. A declared type nobody has built yet is silent; half of one is not. A `single-document` type has a page and no folder. Skipped when the run is narrowed to paths.   |
| `generated-block`         | error   | A type page still carries both markers of each block `kac index` writes into it. `SpliceBlock` leaves the page alone when a marker is missing, and `index --check` then calls the page fresh, so nothing else can notice.                                                               |
| `unused-definition`       | warning | A link definition that nothing references.                                                                                                                                                                                                                                              |
| `bracket-literal`         | warning | A `[...]` left in prose that looks like a reference but has no definition (use an inline link if it is deliberate).                                                                                                                                                                     |

### Content quality (a type's own `rules`)

A rule fires against the documents of the type whose schema declares it, and reports under its own id. Most are answered
by an `expr:` — a one-line condition the schema states and the tool evaluates, so adding one is adding YAML rather than
editing this tool; [`../.schema/README.md`](../.schema/README.md) is the reference for what one may say. Only the last
two need more than the grammar can say, and each is a class in `kac.core/Rules/` with its own unit tests.

The table below is every rule that runs. The schema declares roughly as many again that do not — intentions, carrying a
`description:` and no `severity:`, rendered on their type page under *Declared, not yet enforced*. Naming a severity
without running is the one arrangement this forbids, and `schema-dispatch` is what forbids it.

| Check                           | Type         | Level   | What it enforces                                                                                                                      |
|---------------------------------|--------------|---------|---------------------------------------------------------------------------------------------------------------------------------------|
| `detected-not-before-occurred`  | postmortems  | error   | `detected-on` is on or after `occurred-on` — an incident cannot be found before it began.                                             |
| `symptoms-first`                | runbooks     | error   | Symptoms is the first section, because that is what someone reaching for a runbook matches on.                                        |
| `provenance-required`           | standards    | error   | The standard cites an ADR in `derived-from` or a policy in `implements`. With neither it is guidance.                                 |
| `hub-not-specification`         | capabilities | warning | Prose has not outgrown the links. A capability points at detail; it does not carry it.                                                |
| `links-rather-than-restates`    | explanations | warning | As above — a fact restated rather than linked is a second copy to keep true.                                                          |
| `low-ceremony`                  | discoveries  | warning | A capture stays short. Length here means the tier boundary is being ignored.                                                          |
| `no-credentials`                | integrations | error   | Nothing reads as a credential rather than as a reference to one — code fences included.                                               |
| `no-actual-data`                | data         | error   | No address outside `example.com`, which RFC 2606 reserves so that it can never be anybody's.                                          |
| `fallback-required`             | integrations | warning | The *Failure modes* section names a fallback, or says plainly that there is none.                                                     |
| `not-normative`                 | explanations | warning | No bold RFC 2119 keyword — a bold modal binds, and an explanation does not.                                                           |
| `no-hedged-ordering`            | processes    | warning | No "typically", "usually" or "normally" inside *Steps*.                                                                               |
| `posture-belongs-to-frameworks` | policies     | warning | No claim of standing beside a framework's name — that belongs in `frameworks.md`.                                                     |
| `target-is-measurable`          | nfrs         | warning | `measured-by` names an instrument rather than hedging — "monitored", "where practical" answer nothing.                                |
| `what-went-well-required`       | postmortems  | warning | Something follows the *What went well* heading. `sections` can require the heading and not its contents.                              |
| `mechanism-has-evidence`        | controls     | warning | A control whose `mechanism` is not `not-enforced` names where the proof of it lives.                                                  |
| `one-problem-per-document`      | faqs         | warning | One *Symptom* section, because an FAQ is found by its symptom.                                                                        |
| `trial-has-criteria`            | tools        | warning | A tool in `trial` carries a *Trial criteria* section; without one the trial has no way to end.                                        |
| `deprecated-has-successor`      | tools        | warning | A deprecated tool names its `successor`, so the reader is sent somewhere rather than told a dead end.                                 |
| `y-statement`                   | adrs         | warning | A block-quote follows the H1, states all six moves, and is within `max-words` (60).                                                   |
| `alternatives-verdict`          | adrs         | warning | Each *Alternatives Considered* bullet states an outcome. Heuristic: an explicit verdict word or a contrastive / negative-outcome cue. |

The three length rules are ratios or ceilings whose numbers are judgements rather than measurements — no corpus has yet
held enough of these types to calibrate them. Each is pinned by a fixture, so changing one is visible.

The seven that match text are heuristics, and their patterns live in `.schema/` for that reason: a heuristic gets tuned,
and tuning a regex there is a schema edit rather than a release every corpus has to take. Six read the document **as
written** — a credential pasted into a fenced block is the case they exist for, and the flattened text a word count
walks would never see it. `target-is-measurable` is the exception: it reads a frontmatter value, which the body patterns
deliberately cannot see, because a field is judged against what its own declaration says.

Code is excluded from every link and marker check: they walk the Markdig AST (inline links, literal runs), and fenced or
indented code carries none of those nodes.

### Why rules are data

Wiring a rule as C# means a class, a registry line, unit tests, a row in `Generator.DocRows`, a row in two READMEs, and
a fixture. Wiring it as an expression means a line of YAML and a fixture. That difference is the whole argument, and it
compounds: a corpus that has *taken* this framework rather than authored it may add a whole type file of its own, and
before this layer existed every rule in one was inert — enforcing it needed an upstream code change and a release.

OPA/Rego was the obvious alternative and is the wrong shape. It would replace only the evaluation *tail* of the
pipeline, leaving all the markdown and frontmatter extraction untouched, while adding a language and a runtime
dependency and breaking the single-file, no-build-step design. A small hand-rolled evaluator buys the one property worth
having — new rules as data — at a fraction of that. `RuleExpr.cs` says when that judgement expires.

| File                      | Holds                                                                              |
|---------------------------|------------------------------------------------------------------------------------|
| `kac.core/Facts.cs`       | the fact functions, and nothing else an expression can reach                       |
| `kac.core/RuleExpr.cs`    | lexer, recursive-descent parser, type checker, evaluator — no dependencies         |
| `RuleSpec` in `Schema.cs` | `Expr`, `Compiled`, `Severity`, `Message`; `ParseRule` compiles at load            |
| `kac.core/Rules/`         | one class per rule that needs C#, and the registry the dispatcher looks them up in |
| `Validator.CheckRules`    | evaluates every compiled rule, and looks up by id the ones that are not            |

`CheckRules` emits at the rule's own severity, which is why it is not `CheckWarnings`. `Facts` is built per document and
discarded once its rules have run, which is what makes `words()` safe to memoise there rather than on the immutable
`Doc`.

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

| Artefact                                             | Built from                              | Rule                                                                                                                                                                                                                                          |
|------------------------------------------------------|-----------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `<type>/_index.md`                                   | frontmatter across the folder           | Regenerated **wholly**; columns and sort come from the schema's `index` block; carries a do-not-edit banner. A type with no records yet gets an index saying so rather than a table with no rows; a type with no folder (glossary) gets none. |
| `<!-- … schema-<type> -->` block in `<type>.md`      | `_universal.yaml` + the type's `fields` | The frontmatter reference table — universal fields first, marked `†`, then the type's own. Each row renders the field's `description`, falling back to `notes` where the schema declares none.                                                |
| `<!-- … schema-universal -->` block in `metadata.md` | `_universal.yaml`                       | The universal field reference, documented once for the taxonomy rather than per type.                                                                                                                                                         |
| `<!-- … checks-<type> -->` block in `<type>.md`      | the checks the validator implements     | The "What CI checks" table. Rows a type cannot trip — a rule it does not declare, a reciprocal or mirrors-section field it does not have — are omitted, so each page lists only its own checks.                                               |

Only the region **between** each `BEGIN`/`END` marker is rewritten; the rest of `<type>.md` is byte-preserved. Every
type is regenerated whether or not it holds records: the blocks derive from the schema alone, and an index that waits
for its first record is a dead link from the type page until then.

Three rules hold this together:

- **CI never commits.** `index` writes locally. In CI, run `index --check`: it recomputes the generated content, and if
  any file differs it prints the stale files, names the command to run (`dotnet run .tooling/kac.cs -- index`, or just
  `./kac index`), and exits `1`. A pipeline never pushes.
- **Output is byte-stable.** Generation is a pure function of frontmatter + schema, so running
  `index` twice produces no diff. Tables use fixed column widths, `|` is escaped, and files are LF with a trailing
  newline — so if a Markdown formatter is added later, the freshness check keeps working instead of failing forever.

## `mechanism` — portability

`manifest.yaml` declares each file's layer — `synced`, `forked`, `generated`, `local`, `ignored` — but the declaration
needs enforcing. `mechanism --check` resolves every tracked file against the manifest and compares the **synced** layer
against a reference corpus, following the same discipline as `index --check`: recompute, compare, name what differs,
exit non-zero, never write.

```bash
./kac mechanism --check --against ../other-corpus
```

The reference defaults to `upstream.url` in `.mechanism.lock`, so a consumer that records where it synced from can run a
bare `mechanism --check`. What it reports:

- **synced** files that differ, are missing on either side, or match no manifest rule at all — each an **error** (exit
  `1`).
- **forked** files are compared too, but only counted: how many differ from the reference is informational and never
  fails.
- **generated**, **local** and **ignored** files are skipped — each corpus owns its own.
- **accepted divergences** listed in `.mechanism.lock` are honoured rather than flagged as drift, and any that have
  quietly become identical to the reference again are named as `RESOLVED` so the stale entry can be removed.

Comparison is LF-normalised, so line-ending differences never read as drift. `mechanism --sync` — the write half that
copies the synced layer into a consumer — is not implemented yet.

## Known gaps

- **`immutable-after-accepted`** (content of an accepted document must not change) needs git history and is not
  implemented in the static validator; it belongs in a diff-aware CI step.
- **Date validity** is checked for *shape* (`YYYY-MM-DD`), not calendar validity (`2026-13-40`
  passes the shape check).
