# Discovery

Discovery decides which files `kac` reads. It lists every file in the corpus (one repository of knowledge records kept
in git), selects the records from that list, and passes the rest to the link checks. Every command that asks a question
about a corpus starts here. No check runs against a file that discovery leaves out.

```sh
git ls-files --cached --others --exclude-standard   # the listing kac starts from
```

A record is one Markdown document with YAML frontmatter above its prose. Each record sits in the folder of its type,
such as `policies/` or `runbooks/`. A type is one kind of record, and the corpus's own `.schema/` declares which types
it has.

## What `git ls-files` lists

The listing obeys `.gitignore`, `.git/info/exclude` and your global excludes. It never includes `.git/`. A file git does
not list is a file no command sees.

The listing includes every file, not only the Markdown. The records come from the Markdown in it. The other files are
what a link resolves against.

This is why [Running it in CI](../ci.md) asks you to check the repository out with git. An archive download has no git
repository, so `kac` falls back to [the fallback walk](#the-fallback-walk) instead, and that walk lists Markdown alone.

## What discovery excludes

Five rules narrow the listing. A file matching any of them is not a record.

* **A path with a `_`-prefixed segment.** The underscore is reserved for the framework's own files, so the rule covers
  `**/_index.md` and `**/_template.md` as well as `_plan/` and `_reports/`. A type's `_template.md` is not a record, and
  `template-fields` still checks it.
* **`knowledge-as-code/`.** This folder contains the framework's own documentation. The rule excludes it as a record
  only. [The framework's own documentation](#the-frameworks-own-documentation) below still reads it.
* **`.git/`, `.idea/` and `.claude/` at the corpus root.** Nothing reads these. The rule tests the first segment of the
  path, so discovery still reads a `.claude/` inside a type folder.
* **A root `README.md` and a root `CLAUDE.md`.** Both are orientation pages.
* **Anything outside a folder that maps to a type.** A record lives in its type's folder, and discovery reads those
  folders and nothing else.

[Automation](../framework/automation.md#what-is-not-a-record) explains why each path is named instead of globbed.

## `from: sub-path`

The first folder in a record's path decides its type. You arrange the folders below that one, and `kac` reads them too.
A field declaring `from: sub-path` takes its value from them. So `policies/security/accs-access-by-identity.md` gets
`category: security`, and `standards/platform/dotnet/testing.md` gets `category: platform/dotnet`.

Nest those folders as deep as you want. A record saved straight into its type folder gets an empty value, so a corpus
that files everything at the top of each type folder declares nothing and reads no differently.
[Metadata](../framework/metadata.md#a-field-the-schema-derives-which-you-never-write) explains what an author does about
it.

## Frontmatter

`kac` validates a document only if it has a YAML frontmatter block. It counts a file that sits in a type folder without
one as skipped without frontmatter, reports that count in the summary, and does not fail the run.

## The framework's own documentation

`knowledge-as-code.md` and the documents below it are not records, and discovery excludes them. They are still Markdown
with links in it. So `kac` reads them for link and fragment resolution, the way it reads a type page, and for
`framework-names-types`.

`kac` empties the generated blocks before it reads. `generate --check` checks those blocks instead. Their links are
written from this corpus, not from the framework.

The framework's own glossary sits in that set and is also a record, filed under a type and validated like any other.
`kac` applies the naming rule to it, and not a second link pass. A second pass would report every dead link in it twice.

## Type pages

A type page such as `adrs.md` or `services.md` is not a record and has no frontmatter, so the structural checks do not
apply to it. `kac` checks it for link resolution, undefined labels, non-canonical labels, unused definitions, and
frontmatter it should not have at all.

## Generated block markers

`kac` checks that every type page and every framework document still has both markers of each block `generate` writes
into it. The list of blocks is the one `generate` writes from.

If a block loses its markers, `generate` no longer writes it. This check is the only thing that reports the loss.
`generate --check` compares a file against what the generator would produce. For a file with no marker in it, that is
the file as it stands. [Generation](generation.md) explains what each block is built from.

## The fallback walk

`kac` walks the tree for `*.md` when the tree is not a repository, or when git cannot be run. The walk skips `.git`,
`.idea` and `.claude` at the root. It obeys no exclude file.

Two things change, and the second one is easy to miss:

* A Markdown file the corpus had ignored is discovered and validated.
* The walk lists Markdown alone, where `git ls-files` lists everything. A link to an image or a YAML file then resolves
  against nothing and fails `link-resolves`.

The tool's own test harness builds such a tree on purpose. A corpus outside version control meets the walk without
asking for it. The walk is the fallback, so do not rely on it.

## Where to go next

[`validate`](../cli/validate.md) runs this pass and reports what it finds.
