# Discovery

Discovery decides which corpus `kac` reads, which schema it judges that corpus against, and which files in it are
records. A corpus is one repository of knowledge records kept in git. This page lists what discovery is built to do,
and the scenario proving each behaviour. [Discovery](../use/discovery.md) and
[where a command runs](../use/cli/index.md#where-a-command-runs) own the full statement of each one.

## What it does

* It finds the corpus by walking up from the working directory for a `.corpus.yaml`.
  [Where a command runs](../use/cli/index.md#where-a-command-runs)
* It finds the schema by walking up again from the corpus for `.schema/`.
  [Where a command runs](../use/cli/index.md#where-a-command-runs)
* It lists files with `git ls-files`, so a file git ignores is a file no command sees.
  [What `git ls-files` lists](../use/discovery.md#what-git-ls-files-lists)
* It walks the tree for Markdown where the tree is not a repository. The walk obeys no exclude file and skips `.git/`,
  `.idea/` and `.claude/` at the root. It lists Markdown alone, so a link to any other file fails there.
  [The fallback walk](../use/discovery.md#the-fallback-walk)
* It reads a path with a `_`-prefixed segment as no record, and still checks a type's `_template.md`.
  [What discovery excludes](../use/discovery.md#what-discovery-excludes)
* It reads `knowledge-as-code/` as no record, and still resolves links in it.
  [The framework's own documentation](../use/discovery.md#the-frameworks-own-documentation)
* It reads `.git/`, `.idea/` and `.claude/` at the corpus root as nothing, and still reads a `.claude/` deeper down.
  [What discovery excludes](../use/discovery.md#what-discovery-excludes)
* It reads a root `README.md` and a root `CLAUDE.md` as no record.
  [What discovery excludes](../use/discovery.md#what-discovery-excludes)
* It reads only the folders that map to a type for records.
  [What discovery excludes](../use/discovery.md#what-discovery-excludes)
* It reads a record in a sub-folder of its type's folder, at any depth, and derives `from: sub-path` from those folders.
  [`from: sub-path`](../use/discovery.md#from-sub-path)
* It counts a file in a type folder with no frontmatter as skipped, and does not fail the run.
  [Frontmatter](../use/discovery.md#frontmatter)
* It checks a type page and a framework document for their links and their generated block markers, and not as a
  record. [Type pages](../use/discovery.md#type-pages)

## What it refuses

* Every command but `new` refuses a working directory with no `.corpus.yaml` above it.
  [Where a command runs](../use/cli/index.md#where-a-command-runs)
* `new` refuses a folder that already has a `.corpus.yaml` above it.
  [Where a command runs](../use/cli/index.md#where-a-command-runs)

## Specified by

[`Creation.feature`][Creation.feature]:

* A folder already inside a corpus is refused

[`Clean.feature`][Clean.feature]:

* A record filed in a sub-folder validates, at one level and at two

[`Structure.feature`][Structure.feature]:

* A field the type derives from the folder is not the author's to write
* A template carrying a derived field is reported, not only a record

[`Discovery.feature`][Discovery.feature]:

* A schema above the corpus judges it
* A file git ignores is not read
* A file the corpus ignores is read where the corpus is not a repository
* A link to a file that is not Markdown resolves in a repository
* A link to a file that is not Markdown fails where the corpus is not a repository
* A folder discovery skips at the corpus root is read inside a type folder
* A link into a folder the fallback walk skips fails
* A Markdown file outside every type folder is not a record
* A file in a type folder with no frontmatter is skipped, and the run passes

[`Shape.feature`][Shape.feature]:

* A type's template is checked without being counted as a record
* Every file carrying a generated block is checked for its markers, and for its links

## Unspecified

* `.git/`, `.idea/` and `.claude/` at the root read as nothing in a repository. `git ls-files` lists a root `.claude/`,
  so a link into it resolves there, and the use page says nothing reads it.
* A command other than `new` refusing a folder outside any corpus. The refusal comes before a command runs, in
  `tooling/kac/Program.cs`, which the feature layer does not reference.

## Where to go next

[Validation](validation.md) says what `kac` checks in the records discovery selects.

[Clean.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Clean.feature
[Creation.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Creation.feature
[Discovery.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Discovery.feature
[Shape.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Shape.feature
[Structure.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Structure.feature
