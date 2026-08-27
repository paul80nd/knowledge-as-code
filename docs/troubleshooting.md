# Troubleshooting

What `kac` prints when something is wrong, and what to do about it. Most headings below are the message itself, so
search this page for the words on your screen. The rest name the situation you are in.

A **corpus** is one repository of knowledge records kept in git. A **record** is one Markdown document in it, filed
under a type and carrying YAML frontmatter above its prose.

## `kac: command not found`

The tool is not on your `PATH`. Install it, and check:

```bash
dotnet tool install --global KnowledgeAsCode.Tool
kac --version
```

A global install puts `kac` in `~/.dotnet/tools`, which your shell has to know about. If the install worked and the
command still does not, add that folder to `PATH` and open a new shell.

Inside a repository pinning the tool, run it through the manifest instead:

```bash
dotnet tool restore
dotnet tool run kac validate
```

## `could not locate a corpus (no .corpus.yaml above the cwd)`

```text
kac: could not locate a corpus (no .corpus.yaml above the cwd).
```

Every command but `new` answers a question about a corpus, and finds one by walking up from the working directory.
You are outside one. The exit code is `2`, which means no corpus rather than a fault in one.

`cd` into your corpus and run it again. If you meant to create a corpus, [`new`](cli/new.md) is the command.

## `is already a corpus, so there is nothing here to create`

```text
new: /path/to/corpus is already a corpus, so there is nothing here to create.
taking a newer framework into one is `kac update`.
```

`new` stands a corpus up where there is none. Taking a newer framework into one that exists is
[`update`](cli/update.md).

## `generated files are stale`

```text
generated files are stale. These differ from the schema/frontmatter:
  glossary/_index.md
run:  kac generate
```

An index or a generated table no longer matches the records it was built from. Run `kac generate` on your own machine
and commit what it writes. CI never commits, so this is always yours to fix locally.

Where an index still looks wrong, the frontmatter it was built from is wrong.

## `this repository holds uncommitted changes`

```text
update: this repository holds uncommitted changes. commit or stash them first,
so that what `update` writes reads as a diff of its own.
```

`update` writes files and stops, and git is the review step. A clean tree is what makes everything it wrote
distinguishable from everything you wrote.

Commit or stash, then run it again. To see what it *would* do without any of this, `kac update --check` writes nothing
and runs over a tree in any state.

## `no export at .dist/export/`

```text
bundle: no export at .dist/export/. Run it first: kac export
```

`bundle` and `pack` both read what `export` wrote, and neither loads the corpus itself. Run `export` first:

```bash
kac export
kac bundle     # the plugin an agent installs
kac pack       # the package another corpus imports
```

## `Unknown option` or `No such command`

```text
       validate --wrong
                ^^^^^^^ Unknown option
```

A mistyped flag is a bad invocation and exits `1`. `kac --help` lists the commands, and `kac <command> --help` prints
what that one accepts.

## A finding you do not understand

Every finding names the check that fired, in brackets:

```text
adrs/0001-knowledge-as-code.md
  error  [required-field]  missing required field 'owner'.  (adrs/0001-knowledge-as-code.md:1)
```

`kac checks` prints what every check proves, read from your own `.schema/`:

```bash
kac checks | grep required-field
```

The five you are most likely to meet:

| Check            | What it means                                                       |
|------------------|---------------------------------------------------------------------|
| `required-field` | The type's schema declares a field this record does not carry.      |
| `id-format`      | The id does not match the style its type declares.                  |
| `link-resolves`  | An internal link points at a file that is not there.                |
| `identity-id`    | The id in the identity line disagrees with the one in frontmatter.  |
| `unknown-key`    | A frontmatter key is not one the schema declares.                   |

[Checks](design/checks.md) is the page for adding a check of your own.

## A record is skipped and you expected it to be checked

The summary counts it:

```text
validated 13 document(s) and 8 template(s), skipped 1 without frontmatter. 0 error(s), 0 warning(s)
```

A document is validated only if it carries a YAML frontmatter block. Frontmatter is how a document opts into its
type's schema. Add one, or accept that the file is prose the corpus does not judge.

A file that is not counted at all was never discovered. [Discovery](design/discovery.md) lists the five rules that drop
a file from the listing, and a `_` anywhere in its path is the usual answer.

## A new corpus does not validate

```text
glossary.md
  error  [link-resolves]  link target 'services.md' does not resolve.  (glossary.md:36)
```

You declined some types, and the type pages cross-reference each other. Those pages are yours from the moment they
land, so edit the links out. [`new`](cli/new.md#known-limits) says why they arrive that way.

## Findings appear that a `.gitignore` should have hidden

`kac` lists a corpus with `git ls-files`, so your exclude files count. A tree that is not a repository, or one where
git cannot be run, falls back to a directory walk that honours none of them.

Check you are in a repository, and that CI checks out with git rather than downloading an archive.
[Running it in CI](ci.md#ci-never-commits-and-checks-out-with-git) covers that.

## Something else

The exit code narrows it:

| Code | Meaning                                                     |
|------|-------------------------------------------------------------|
| `0`  | No errors. Warnings may still have been printed.            |
| `1`  | A corpus error, or a bad invocation.                        |
| `2`  | No corpus found.                                            |

If the tool is wrong rather than your corpus, the
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues) is where to say so. `kac --version` names the
release and the commit it was built from, which is the first thing worth putting in the report.
