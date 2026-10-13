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

The listing includes every file. The records come from the Markdown in it, and a link resolves against the rest. A
tree with no git repository, such as an archive download, gets [the fallback walk](#the-fallback-walk).

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

## `from: sub-path`

The first folder in a record's path decides its type. You arrange the folders below that one, and `kac` reads them too.
A field declaring `from: sub-path` takes its value from them. So `policies/security/accs-access-by-identity.md` gets
`category: security`, and `standards/platform/dotnet/testing.md` gets `category: platform/dotnet`.

Nest those folders as deep as you want. A record saved straight into its type folder gets an empty value.
[Metadata](../framework/metadata.md#a-derived-field) says what an author writes.

## Frontmatter

`kac` validates a document only if it has a YAML frontmatter block. It counts a file that sits in a type folder without
one as skipped without frontmatter, reports that count in the summary, and does not fail the run.

## The framework's own documentation

`knowledge-as-code.md` and the documents below it are not records. `kac` reads them for link and fragment resolution,
the way it reads a type page, and for `framework-names-types`. It empties their generated blocks first, which
`generate --check` checks instead.

## Type pages

A type page such as `adrs.md` or `services.md` is not a record and has no frontmatter, so the structural checks do not
apply to it. `kac` checks it for link resolution, undefined labels, non-canonical labels, unused definitions, and
frontmatter it should not have at all.

## Generated block markers

`kac` checks that every type page and every framework document still has both markers of each block `generate` writes
into it. The list of blocks is the one `generate` writes from.

If a block loses its markers, `generate` no longer writes it, and `generate --check` sees no difference. This check is
the only thing that reports the loss. [Generation](../reference/generation.md) says what each block is built from.

## The fallback walk

`kac` walks the tree for `*.md` when the tree is not a repository, or when git cannot be run. The walk skips `.git`,
`.idea` and `.claude` at the root. It obeys no exclude file.

Two things change:

* A Markdown file the corpus had ignored is discovered and validated.
* The walk lists Markdown alone, so a link to an image or a YAML file fails `link-resolves`.

## Where to go next

[`validate`](cli/validate.md) runs this pass and reports what it finds.
