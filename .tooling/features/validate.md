# `validate` — the corpus read and reported on

`kac validate` is the command CI runs over a corpus. This page says which files it reads, which it passes over, and
which get a pass of their own. Where each check comes from, and what it proves, is [`checks.md`](checks.md).

`kac` discovers Markdown via `git ls-files` (so **`.gitignore`, `.git/info/exclude` and global excludes are respected**,
and `.git/` is never walked), then applies the taxonomy exclusions from `knowledge-as-code/automation.md`:

- anything on a path with a `_`-prefixed segment — the reserved prefix for a framework artefact, which covers
  `**/_index.md` and `**/_template.md` as well as `_plan/` and `_reports/`. A type's `_template.md` is not a record and
  is discovered as none, but it is checked — `template-fields` is the check, and [`checks.md`](checks.md) says where
  it comes from
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

