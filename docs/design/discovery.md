# Discovery

Which files `kac` opens, which of those it judges as records, and which it reads for something narrower. Every command
answering a question about a corpus starts here, so a file this pass leaves out is a file no check reaches.

A **corpus** is one repository of knowledge records kept in git. A **record** is one Markdown document in it, filed
under a type and carrying YAML frontmatter above its prose. A **type** is one kind of record, such as a policy or a
runbook, declared in the corpus's own `.schema/`.

## `git ls-files` decides what exists

`kac` lists a corpus with `git ls-files --cached --others --exclude-standard`, so `.gitignore`, `.git/info/exclude`
and your global excludes all count. `.git/` is never walked. A file git does not list is a file no command sees.

The listing carries every file, not only the Markdown. Records are the Markdown in it, and the rest is what a link
resolves against.

This is why [Running it in CI](../ci.md) asks you to check the repository out with git rather than download an archive.

## What the listing then drops

Five rules narrow that listing, and a file matching any of them is not a record.

* **Anything on a path with a `_`-prefixed segment.** The underscore is reserved for the framework's own artefacts, so
  it covers `**/_index.md` and `**/_template.md` as well as `_plan/` and `_reports/`. A type's `_template.md` is
  discovered as no record and is still checked, under `template-fields`.
* **`knowledge-as-code/`**, which holds the framework's own documentation. It is excluded as a *record* only, and the
  pass below still reads it.
* **`.git/`, `.idea/` and `.claude/` at the corpus root.** Nothing reads these at all. The rule tests the first segment
  of the path, so a `.claude/` nested inside a type folder is not caught by it.
* **A root `README.md` and a root `CLAUDE.md`.** Both are orientation pages.
* **Anything outside a folder that maps to a type.** A record lives in its type's folder, so those folders are the whole
  of what is read.

[Automation](../framework/automation.md#what-is-not-a-record) says why each path is named rather than globbed.

## Frontmatter is how a document opts in

A document is validated only if it carries a YAML frontmatter block. A file sitting in a type folder without one is
counted in the summary as skipped without frontmatter, and does not fail the run.

## The framework's own documentation gets a pass of its own

`knowledge-as-code.md` and the documents beneath it are not records, and discovery leaves them out. They are still
Markdown that links to things, so they are read for link and fragment resolution, the way a type page is, and for
`framework-names-types`. Generated blocks are emptied first, because `generate --check` answers for those and the links
inside them are written from this corpus rather than from the framework.

The framework's own glossary sits in that set and is also a record, filed under a type and validated like any other. It
takes the naming rule and not a second link pass, which would report every dead link in it twice.

## A type page gets a pass of its own

A type page such as `adrs.md` or `services.md` is not a record and carries no frontmatter, so the structural checks do
not apply. It is checked for link resolution, undefined and non-canonical labels, unused definitions, and frontmatter it
should not be carrying at all.

## A generated block is held to its markers

A type page and the framework's own pages alike are held to still carrying both markers of every block `generate`
writes into them. The list of blocks is the one `generate` writes from.

A block whose markers have gone is written by nothing, and without this pass nothing would report it. `generate --check`
compares a file against what the generator would produce, and what the generator produces for a file it finds no marker
in is that file as it stands. [Generation](generation.md) says what each block is built from.

## The fallback walk honours nothing

A tree that is not a repository, or one where git cannot be run, is walked for `*.md` instead. That walk skips `.git`,
`.idea` and `.claude` at the root and honours no exclude file.

Two things change, and the second is the one that bites. A Markdown file the corpus had ignored is discovered and
validated. And the walk lists Markdown alone where `git ls-files` lists everything, so a link to an image or a YAML
file resolves against nothing and fails `link-resolves`.

The tool's own test harness assembles such a tree deliberately. A corpus outside version control meets it without
asking, which is why nothing is proved on this path.

## Where to go next

[`validate`](../cli/validate.md) is the command that runs this pass and reports what it finds.
