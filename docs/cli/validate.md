# `validate` check the corpus against its schema

<!-- BEGIN GENERATED: usage-validate -->

```text
kac validate [--json] [--no-color]
```

| Option       | What it does                                                |
|--------------|-------------------------------------------------------------|
| `--json`     | Emit the summary and findings as JSON.                      |
| `--no-color` | Turn colour off. NO_COLOR in the environment does the same. |

<!-- END GENERATED: usage-validate -->

## What it is for

`validate` is the command CI runs over a corpus, meaning one repository of knowledge records, and the one an author
runs before pushing. It decides which files are
records and applies the checks the schema declares. It reports each fault against the file that caused it, with a line
where it has one. Its reader is whoever writes a record, so a finding has to name the fault precisely enough to be fixed
without opening this tool. Where each check comes from, and what it proves, is [Checks](../checks.md).

## What it is not

**It is not [`checks`](checks.md).** `kac checks` prints what could fire, read from the schema. `validate` fires it
against documents. A check absent from a validate run is either undeclared or untripped, and only `checks` tells those
two apart.

**It is not [`generate --check`](generate.md).** What sits between a generated block's markers is not `validate`'s to
judge. `validate` holds a file to still carrying the markers of every block the generator writes into it, and freshness
is `generate --check`'s one question.

**It is not [`update --check`](update.md).** That asks whether this corpus's copy of the framework has fallen behind the
template it took. `validate` asks whether this corpus's own records are correct, and a corpus a long way behind can
still be entirely valid.

## How it works

A run lists the corpus's Markdown, decides which of it counts as a record, and applies the checks the schema declares to
each one. What is not a record still gets read, by the narrower passes below.

### What is discovered

`kac` discovers Markdown with `git ls-files`, so **`.gitignore`, `.git/info/exclude` and global excludes all count**,
and `.git/` is never walked. It then applies the taxonomy exclusions that
[Automation](../framework/automation.md) states:

- anything on a path with a `_`-prefixed segment, the reserved prefix for a framework artefact. The exclusion covers
  `**/_index.md` and `**/_template.md` as well as `_plan/` and `_reports/`. A type's `_template.md` is not a record and
  is discovered as none, but it is checked. `template-fields` is the check, and [Checks](../checks.md) says where it
  comes from
- `knowledge-as-code/`, excluded as a *record* only. The framework's own documents are still read for their links (see
  below)
- `.git/`, `.idea/` and `.claude/`, which nothing reads at all
- root `README.md` and root `CLAUDE.md`
- anything outside a folder that maps to a type schema

### What makes a document a record

A document is validated **only if it carries a YAML frontmatter block**. The frontmatter is how a document opts into the
schema. A file in a type folder without one is counted in the summary as skipped without frontmatter, and does not fail
the run.

### The framework's own documentation gets a pass of its own

`knowledge-as-code.md` and the documents beneath it are not records, and discovery leaves them out. They are still
Markdown that links to things, so they are read for link and fragment resolution, the way a type page is, and for
`framework-names-types`. Generated blocks are emptied first.
`generate --check` answers for those, and the links inside them are written from this corpus and not from the framework.

The framework's own glossary is in that set and is also a record, filed under a type and validated like any other. It
gets the naming rule and not a second link pass, which would report every dead link in it twice.

### Type pages get a pass of their own

A page (`adrs.md`, `services.md`) is not a record and carries no frontmatter, so the structural checks do not apply. It
is checked for link resolution, undefined and non-canonical labels, unused definitions, and frontmatter it should not be
carrying.

### Every file carrying a generated block is held to its markers

A type's page and the framework's own pages alike are held to still carrying both markers of every block `generate`
writes into them. The list of blocks is the one `generate` writes from.

A block whose markers have gone is written by nothing, and without this pass it would be reported by nothing.
`generate --check` compares the file against what the generator would produce. What the generator would produce for a
file it can find no marker in is that file as it stands.

## Known limits

**Discovery falls back to a directory walk where git cannot answer.** `git ls-files` is what makes `.gitignore`,
`.git/info/exclude` and global excludes count. A tree that is not a repository, or one where git cannot be run, is
walked for `*.md` instead. That walk skips `.git`, `.idea` and `.claude` by name and honours nothing else.

The taxonomy exclusions still apply, so what changes is narrow: a Markdown file the corpus had ignored is discovered and
validated. The test harness assembles such a tree deliberately. A corpus outside version control would meet it without
asking.

[`generate`](generate.md) writes the blocks this command holds a file to still carrying.
