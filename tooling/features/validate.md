# `validate`: the corpus read and reported on

## Intent

`validate` is the command CI runs over a corpus, and the one an author runs before pushing. It decides which files are
records, applies the checks the schema declares, and reports each fault against the file that caused it, with a line
where it has one. Its reader is whoever writes a record: a finding has to name the fault precisely enough to be fixed
without opening this tool. Where each check comes from, and what it proves, is [`checks.md`](checks.md).

## What it is not

**It is not `checks`.** `kac checks` prints what could fire, read from the schema. `validate` fires it against
documents. A check absent from a validate run is either undeclared or simply untripped, and only `checks` tells those
apart.

**It is not `generate --check`.** What sits between a generated block's markers is not `validate`'s to judge.
`validate` holds a file to still carrying the markers of every block the generator writes into it, and freshness is
`generate --check`'s one question.

**It is not `mechanism --check`.** That asks whether this corpus's copy of the framework has drifted from upstream.
`validate` asks whether this corpus's own records are correct, and a corpus that has drifted badly can still be entirely
valid.

## Approach

`kac` discovers Markdown via `git ls-files` (so **`.gitignore`, `.git/info/exclude` and global excludes are respected**,
and `.git/` is never walked), then applies the taxonomy exclusions from `knowledge-as-code/automation.md`:

- anything on a path with a `_`-prefixed segment, the reserved prefix for a framework artefact. The exclusion covers
  `**/_index.md` and `**/_template.md` as well as `_plan/` and `_reports/`. A type's `_template.md` is not a record and
  is discovered as none, but it is checked. `template-fields` is the check, and [`checks.md`](checks.md) says where it
  comes from
- `knowledge-as-code/`, and `.git/` `.idea/` `.claude/`, which are excluded as *records* only. The framework's own
  documents are still read for their links (see below)
- root `README.md` and root `CLAUDE.md`
- anything outside a folder that maps to a type schema

A document is validated **only if it carries a YAML frontmatter block**: the frontmatter is how a document opts into
the schema. Files in a type folder without frontmatter are counted as *skipped (not yet migrated)* and reported in the
summary, not failed.

**The framework's own documentation gets a pass of its own.** `knowledge-as-code.md` and the documents beneath it are
not records and are excluded from discovery, but they are still Markdown that links to things: they are read for link
and fragment resolution like a type page, and for `framework-names-types`. Generated blocks are emptied first:
`generate --check` answers for those, and their links are written from this corpus rather than from the framework.

The framework's own glossary is in that set and is also a record, filed under a type and validated like any other. It
gets the naming rule and not a second link pass, which would report every dead link in it twice.

**Type pages get a pass of their own.** A page (`adrs.md`, `services.md`) is not a record and carries no frontmatter,
so the structural checks do not apply. It is checked for link resolution, undefined and non-canonical labels, unused
definitions, and frontmatter it should not be carrying.

**Every file carrying a generated block gets one more.** A type's page and the framework's own pages alike are held to
still having both markers of each block `generate` writes into them, read from the same list `generate` writes from. A
block whose markers have gone is written by nothing and, without this, reported by nothing: `generate --check` compares
the file against what the generator would produce, and what it would produce for a file it cannot find a marker in is
the file as it stands.

## Known limits

**Discovery falls back to a directory walk where git cannot answer.** `git ls-files` is what makes `.gitignore`,
`.git/info/exclude` and global excludes count. A tree that is not a repository, or one where git cannot be run, is
walked for `*.md` instead, skipping `.git`, `.idea` and `.claude` by name and honouring nothing else. The taxonomy
exclusions still apply, so what changes is narrow: a Markdown file the corpus had ignored is discovered and validated.
The test harness assembles such a tree deliberately; a corpus outside version control would meet it without asking.
