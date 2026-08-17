# `.tooling` — the knowledge-as-code tooling

`kac` validates and generates a knowledge corpus against the machine-readable schema in `.schema/`. The command you run
is a **thin .NET 10 file-based entrypoint** (`kac.cs`) over a small **`kac.core`** library that holds the mechanics;
`dotnet run` builds and runs it with no build step to manage. The schema is the source of truth: `kac` reads it and
enforces it, so **adding a knowledge type is adding a YAML file, not editing this tool**.

Two declarations the tool reads sit here too: [`manifest.yaml`](manifest.yaml), which says which files a corpus shares
with the framework, and each corpus's own `.corpus.yaml` at the repository root.

## Running

```bash
./kac validate            # validate the corpus
./kac validate --json     # machine-readable summary + findings
./kac index               # regenerate indexes and blocks
./kac index --check       # verify generated output is fresh
./kac export              # write the corpus to .dist/ as data a consumer reads
./kac export --type glossary                        # …one type rather than every one that contributes
./kac checks              # list every check the validator implements
./kac checks --json       # …as JSON (the test suite reads this)
./kac mechanism --check --against ../other-corpus   # shared-layer drift vs a reference
./kac mechanism --sync                              # take the shared layers from upstream
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
  `**/_index.md` and `**/_template.md` as well as `_plan/` and `_reports/`. A type's `_template.md` is not a record and
  is discovered as none, but it is checked: see `template-fields` below
- `knowledge-as-code/`, and `.git/` `.idea/` `.claude/` — excluded as *records*; the framework's own documents are still
  read for their links, see below
- root `README.md` and root `CLAUDE.md`
- anything outside a folder that maps to a type schema

A document is validated **only if it carries a YAML frontmatter block** — that is how a document opts into the schema.
Files in a type folder without frontmatter are counted as *skipped (not yet migrated)* and reported in the summary, not
failed.

**The framework's own documentation gets a pass of its own.** `knowledge-as-code.md` and the documents beneath it are
not records and are excluded from discovery, but they are still Markdown that links to things: they are read for link
and fragment resolution like a type page, and for `framework-names-types`. Generated blocks are emptied first —
`index --check` answers for those, and their links are written from this corpus rather than from the framework.

The framework's own glossary is in that set and is also a record, filed under a type and validated like any other. It
gets the naming rule and not a second link pass, which would report every dead link in it twice.

**Type pages get a pass of their own.** A page — `adrs.md`, `services.md` — is not a record and carries no frontmatter,
so the structural checks do not apply. It is checked for link resolution, undefined and non-canonical labels, unused
definitions, and frontmatter it should not be carrying.

**Every file carrying a generated block gets one more.** A type's page and the framework's own pages alike are held to
still having both markers of each block `index` writes into them, read from the same list `index` writes from. A block
whose markers have gone is written by nothing and, without this, reported by nothing: `index --check` compares the file
against what the generator would produce, and what it would produce for a file it cannot find a marker in is the file as
it stands.

## Checks

All checks are read from the schema; nothing here is hard-coded per type. A check marked **warning** does not fail the
build.

**A check is defined once, in the schema.** [`../.schema/_checks.yaml`](../.schema/_checks.yaml) declares the checks
that run against every document, one entry each: its severity, the group it belongs to, what it proves correct, and the
reasoning behind it. A type's own rules are declared beside the type, in `../.schema/<type>.yaml`. Between them they are
every check the validator can emit.

**`kac checks` prints what runs**, read from the schema of the corpus it is run in, and exits non-zero where the
reader-facing table on a type page has drifted from it.

### The schema itself

Before any document is read, the schema is held against what the tool can act on. A declaration nothing dispatches is
not harmlessly inert: `rules:` is documented as behaviour the validator applies, so a rule id no code answers to reads
as a commitment — and these files are copied into corpora whose authors cannot ask what a key was meant to do. Findings
name the schema file and the key rather than a record.

Of a value, the question asked is not whether it is spelled correctly but whether code acts on it — `style: mnemonic`
is a real style, and what makes it sound is the branch in `CheckId`. Of a key, the question is whether the loader reads
it at all, answered by the loader itself: it records what it asks each mapping for, and `schema-unknown-key` reports the
remainder. Neither vocabulary is written down beside the check, because a copy is a list of what is spelled correctly
rather than of what runs. A rule declaring no `severity:` is exempt by design: that is how the schema records an
intention, and the type page renders those beneath the checks table as *Declared, not yet enforced*.

### A type's own rules

A rule fires against the documents of the type whose schema declares it, and reports under its own id. Most are answered
by an `expr:` — a one-line condition the schema states and the tool evaluates, so adding one is adding YAML rather than
editing this tool; [`../.schema/README.md`](../.schema/README.md) is the reference for what one may say. The rest are a
class each in `kac.core/Rules/`, with unit tests beside them, for the questions the grammar cannot ask.

`dependency-cycle` is the one that asks about the records together rather than about each one. It is reported once per
loop against the lowest id on it.

The schema declares roughly as many rules again that do not run — intentions, carrying a `description:` and no
`severity:`, rendered on their type page under *Declared, not yet enforced*. Naming a severity without running is the
one arrangement this forbids, and `schema-dispatch` is what forbids it.

A rule that counts words or links is a ratio or a ceiling whose number is a judgement rather than a measurement — no
corpus has yet held enough of those types to calibrate one. Each is pinned by a fixture, so changing it is visible.

A rule that matches text is a heuristic, and its pattern lives in `.schema/` for that reason: a heuristic gets tuned,
and tuning a regex there is a schema edit rather than a release every corpus has to take. Most read the document **as
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

| File                         | Holds                                                                                 |
|------------------------------|---------------------------------------------------------------------------------------|
| `kac.core/Facts.cs`          | the fact functions, and nothing else an expression can reach                          |
| `kac.core/RuleExpr.cs`       | lexer, recursive-descent parser, type checker, evaluator — no dependencies            |
| `RuleSpec` in `Schema.cs`    | `Expr`, `Compiled`, `Severity`, `Message`; `ParseRule` compiles at load               |
| `kac.core/Rules/`            | one class per rule that needs C#, and the registry each dispatcher looks them up in   |
| `Validator.CheckRules`       | evaluates every compiled rule, and looks up by id the ones that are not               |
| `Validator.CheckCorpusRules` | runs the rules that read every record at once, over the index the corpus checks build |

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

| Artefact                                             | Built from                              | Rule                                                                                                                                                                                                                               |
|------------------------------------------------------|-----------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `<type>/_index.md`                                   | frontmatter across the folder           | Regenerated **wholly**; columns and sort come from the schema's `index` block; carries a do-not-edit banner. A type with no records yet gets an index saying so rather than a table with no rows; a type with no folder gets none. |
| `<!-- … schema-<type> -->` block in `<type>.md`      | `_universal.yaml` + the type's `fields` | The frontmatter reference table — universal fields first, marked `†`, then the type's own. Each row renders the field's `description`, falling back to `notes` where the schema declares none.                                     |
| `<!-- … schema-universal -->` block in `metadata.md` | `_universal.yaml`                       | The universal field reference, documented once for the taxonomy rather than per type.                                                                                                                                              |
| `<!-- … checks-<type> -->` block in `<type>.md`      | the checks the validator implements     | The "What CI checks" table. Rows a type cannot trip — a rule it does not declare, a reciprocal or mirrors-section field it does not have — are omitted, so each page lists only its own checks.                                    |
| five blocks in `knowledge-as-code/taxonomy.md`       | the adopted types                       | `types-placement`, `types-detail`, `types-versus`, `types-graph`, `types-edges` — the decision table, the catalogue by tier, the disambiguations, the relation diagram and its edges.                                              |
| `<!-- … types-metadata -->` block in `metadata.md`   | the adopted types                       | Which types carry which of the fields the universal table above it describes.                                                                                                                                                      |
| two blocks in `knowledge-as-code/lineage.md`         | the adopted types                       | `types-lineage` and `types-collisions` — where each type's name came from, and where it already means something else to a reader.                                                                                                  |
| `<!-- … types-index -->` block in `README.md`        | the adopted types                       | The corpus's own index of the types it carries. The one block a corpus may decline, by deleting the markers, because the file is the corpus's own.                                                                                 |

`GeneratedFiles` holds that list, so `validate` holds a corpus to the same files and blocks this writes.

Only the region **between** each `BEGIN`/`END` marker is rewritten; the rest of the file is byte-preserved. Every
adopted type is regenerated whether or not it holds records: the blocks derive from the schema alone, and an index that
waits for its first record is a dead link from the type page until then.

Generation covers the types the corpus adopted and no others. `types:` in `.corpus.yaml` decides, and a corpus that has
not declared is read off its folders — a type counts where both halves are there, the page and the folder.

A type the corpus declined is left alone whatever `.schema/` says about it, down to the hand-written text between the
markers on a page left behind. Writing there would create an artefact no generated list of this corpus's types names,
and `index --check` would then hold the corpus to keeping it fresh. Standing a type up without adopting it is a defect
`validate` reports.

Two rules hold this together:

- **CI never commits.** `index` writes locally. In CI, run `index --check`: it recomputes the generated content, and if
  any file differs it prints the stale files, names the command to run (`dotnet run .tooling/kac.cs -- index`, or just
  `./kac index`), and exits `1`. A pipeline never pushes.
- **Output is byte-stable.** Generation is a pure function of frontmatter + schema, so running
  `index` twice produces no diff. Tables use fixed column widths, `|` is escaped, and files are LF with a trailing
  newline — so if a Markdown formatter is added later, the freshness check keeps working instead of failing forever.

## `export` — the corpus as data

A consumer of a corpus should not clone it. `export` writes what the corpus knows into `.dist/` as data built for an
agent to read: a manifest saying what the export is, one file per record, and a flat file cheap to grep.

`.dist/` is gitignored and rebuilt whole, so it is never something to review. **The overwrite is delete-then-write.** A
record deleted from the corpus must not leave an entry behind in the output. Nothing else would catch it: the export is
untracked, so no diff shows the orphan and no check goes looking for one.

```
.dist/
  manifest.json          what this export is, and where it came from
  glossary/
    gls-<name>.json      one record: its declared fields, its declared sections, and its links
    terms.jsonl          every term, one to a line
```

**What travels is the type's decision**, declared in its `export:` block and described in
[`../.schema/README.md`](../.schema/README.md). The exporter reads that declaration and nothing else, so a corpus that
adopted no exporting type still writes a manifest, with an empty type list — "nothing" is a valid statement of what a
corpus has.

**The flat file is JSONL because it exists to be grepped.** A hit has to hand back something parseable on its own. A
matching line of an indented document is a fragment, and the reader is left seeking outward for its braces.

So each line repeats what a reader would otherwise look up: the record it came from, the state of that record, its
cross-references as ids, and the links back to it. That costs bytes. It is worth them, because the alternative is a hit
that sends the reader to the very file this one exists to save them opening.

**Records are ordered roots-by-id, each root's chain depth-first beneath it.** Terms sort alphabetically within a
record. Generality holds **within a chain** and nowhere else: `gls-search` narrows `gls-example-libraries`, so a grep
for `title` meets the general entry before the one refining it. Across unrelated roots the order is stable and says
nothing — `record` is defined by `gls-example-libraries` and `gls-knowledge-as-code`, neither narrowing the other, and
reading the first hit as the more general one would give a reader the wrong domain. `narrows` on the owning records is
what tells the two cases apart, and every line names the record it came from.

**Absent is `null`,** in every file and for every key. A field a record leaves blank and a field it does not carry are
one absence to a consumer. Writing `""` in one file beside `null` in another would leave that consumer checking which
file it had opened before it could test for nothing.

**Prose arrives unwrapped.** The corpus wraps at 120 columns, which is a fact about the file rather than about the
words, and a grep for a phrase straddling the wrap would find nothing. Blank lines are the author's and stay; a list,
heading, quote, table or fence is left exactly as written. That last part is a decision rather than an unfinished case,
because the two mistakes do not cost the same: a list joined onto one line is destroyed and cannot be recovered by the
reader, where a paragraph left wrapped merely arrives as it was written. Every doubtful line therefore goes the safe
way, and a corpus whose sections happen to hold only paragraphs today is not a reason to narrow it.

**Two link forms, both naming a ref.** A person follows the rendered one and an agent fetches the raw one. The rules
joining a base to a path, and the anchor rule for a part, belong to `publishing-target` and live in `Publishing`;
`.corpus.yaml` supplies only the bases. Every link resolves against the commit the export was built from, so a citation
names the version the agent read rather than whatever the branch holds later.

Four kinds of corpus have no address the tool can build on: one publishing nowhere, one naming a target nothing builds
links for, one stating a target but no bases, and one git cannot answer for. Each exports without links. The manifest
carries the target it was given and null bases beside it, so a consumer sees the absence stated; the run itself says
which of the four caused it.

**Two versions, and they are independent.** `formatVersion` in the manifest is the shape of the output, and a consumer
reads it to know whether it can parse what it was handed. `contentVersion` is `content-version` from `.corpus.yaml` —
what the corpus knows, semantically versioned and bumped by hand — and a consumer reads it to know whether to re-read
the words. Neither implies the other: a corpus can rewrite every definition without `formatVersion` moving, and
`formatVersion` can move over a corpus nobody has edited.

**Output is deterministic.** Ordering is `StringComparer.Ordinal` throughout, as the generator's is, and every value
that varies between two runs is confined to the manifest. Two runs from one commit produce identical bytes but for
`generatedAt`.

**An unsettled record travels by default.** A draft glossary, and one whose `review-by` has passed, are both exported
carrying their own state, because filtering them would make the corpus's own condition invisible downstream. A corpus
may exclude either with `export.exclude:` in `.corpus.yaml`. Where it does, the run names every record it withheld,
because a record left out of the output cannot be seen there.

**The manifest records whether the export can be reproduced.** It carries the commit and a dirty flag. The flag is
there because a commit on its own would describe a dirty tree as reproducible.

## `mechanism` — portability

[`manifest.yaml`](manifest.yaml), beside this file, declares each file's layer — `synced`, `verification`, `forked`,
`generated`, `local`, `ignored`. Copies drift away from a declaration nobody enforces, so `mechanism` enforces this one
from both ends. `--check` reports how far a corpus has moved from a reference. `--sync` takes the shared layers from
one.

### `--check`

`mechanism --check` resolves every tracked file against the manifest and compares the shared layers against a reference
corpus. It follows the same discipline as `index --check`: recompute, compare, name what differs, exit non-zero, never
write.

```bash
./kac mechanism --check --against ../other-corpus
```

The reference defaults to `upstream.url` in `.corpus.yaml`, so a corpus that recorded where it synced from can run a
bare `mechanism --check`. It reports:

- **synced** and **verification** files that differ, are missing on either side, or match no manifest rule — each an
  **error** (exit `1`).
- **forked** files that differ — counted, never failed on, because a forked file is meant to diverge.
- **generated**, **local** and **ignored** files — skipped, because each corpus owns its own.
- **accepted divergences** named in `.corpus.yaml` — honoured rather than flagged, and reported as `RESOLVED` once they
  match the reference again, so you can delete the stale entry.
- **what the descriptor declines** — skipped, and counted where the corpus holds it anyway.

It opens by reporting the three versions the descriptor states: `content-version`, `descriptor-version` and
`upstream.mechanism-version`. A version the corpus has not stated is reported as not declared, because only the corpus
can say what it knows. A descriptor still carrying the older `version:` key stops the command outright, in either half.
The message names the old key, the new one and the file, and the rename is the corpus's to make — nothing rewrites this
file on a corpus's behalf.

A corpus declines in two ways, and both work alike. Leaving a type out of `types:` leaves out its `.schema/<type>.yaml`,
so that file is neither missing nor drifted. Setting `role:` to `consumer` does the same for the `verification` layer,
because a consumer runs a tool proven upstream instead of proving it. These are the only ways a corpus may hold less of
a shared layer than upstream does, and the descriptor is where it says so. Without that entry the same absence reads as
a deletion nobody recorded. A descriptor that declares neither takes the whole shared layer.

`--check` normalises line endings before it compares, so a working copy checked out with CRLF never reads as drift. It
then compares the **authored half** of each file, emptying everything between `BEGIN GENERATED` and `END GENERATED`
first. A shared page may therefore carry a block built from the corpus holding it — the taxonomy's tables list the types
that corpus adopted — while the prose around the block stays byte-identical everywhere. The markers themselves are
compared, so deleting a block rather than regenerating it is still drift. `index --check` stays the one voice on whether
the generated half is right.

### `--sync`

`mechanism --sync` takes the shared layers from the reference, records what it took, and regenerates.

```bash
./kac mechanism --sync                      # from upstream.url
./kac mechanism --sync --against ../source  # …or from a local checkout of it
```

`--against` says which copy of the upstream to read. `upstream.url` says the corpus takes from an upstream at all. A
corpus that names none sits at the head of the chain — changes leave it and none arrive — so `--sync` refuses to run
there. A corpus that names one syncs from it whatever its role, so a mirror of the framework takes the tooling and the
tests down like anything else.

In one pass over both trees:

- **synced** and **verification** files come down whole where their authored halves differ. A file already in step stays
  as it is, so a page's generated block survives when the prose around it has not moved.
- **forked** files are *seeded*: copied only where this corpus has none. Sync never reconciles a forked file that is
  already here.
- **What the descriptor declines** never comes down. Leaving a type out withholds its `.schema/<type>.yaml`, its root
  page and everything under its folder, so adopting one means adding a line to `types:` and syncing.
- **Accepted divergences** are skipped and named, with their recorded reason beside them. Delete the entry to take the
  upstream copy, which keeps the decision in one place.
- Files this corpus holds and the reference does not are **named, not deleted**. Sync copies. Emptying a corpus because
  an upstream tree was smaller is not a decision a tool makes.

Sync then stamps `descriptor-version`, `upstream.mechanism-version`, `synced-from` and `synced-on` into `.corpus.yaml`.
It rewrites those four lines rather than re-serialising the file, so the descriptor's commentary survives. The file's
own format is the mechanism's to state, because a corpus cannot know the shape a newer one writes. `content-version` is
left alone: what a corpus knows is not something an upstream can tell it. Finally it runs `index`.
Copying a page whole is only safe because of that last step: the page arrives carrying the reference's generated block,
and is right only once rebuilt against the types the receiving corpus holds. A passing `index --check` is sync's
postcondition.

## Known gaps

- **`immutable-after-accepted`** (content of an accepted document must not change) needs git history and is not
  implemented in the static validator; it belongs in a diff-aware CI step.
