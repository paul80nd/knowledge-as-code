# `.corpus.yaml` what a corpus says about itself

## What it is for

`.corpus.yaml` sits at the root of a corpus and says what that corpus is, and where it stands against the shared
framework. Everything else a corpus holds arrives from a template and can be taken again. This file is the corpus's own,
and no update writes over it.

[`new`](cli/new.md) writes it when the corpus is created, from what that invocation was told, and it arrives commented
key by key. After that it is yours: you edit it by hand, and [`update`](cli/update.md) stamps four of its keys. The
longer worked copy is
[`examples/library/.corpus.yaml`](https://github.com/paul80nd/knowledge-as-code/blob/main/examples/library/.corpus.yaml),
commented throughout, and it is the one to read while changing yours.

## What it is not

**It is not the schema.** `.schema/` says what a record of each type must carry. This file says which of those types the
corpus adopted, and says nothing about their shape.

**It is not configuration for a run.** It records decisions, and a command reads them. `--from`, `--ref`, `--path` and
`--policy` each replace a value here, and only for the run they are passed to.

## Three versions live here

None of them is called `version` alone, because each answers a different question.

| Key                         | Answers                                 | Written by | Moved by |
|-----------------------------|-----------------------------------------|------------|----------|
| `descriptor-version`        | the format of this file                 | `new`      | `update` |
| `content-version`           | the version of what this corpus *knows* | `new`      | by hand  |
| `upstream.template-version` | the template shape this corpus took     | `new`      | `update` |

`content-version` is semantically versioned: major where a meaning changed or a published URL broke, minor for
additions, patch for wording. Quote it when one corpus tells another which version of the content it holds. Nothing
refuses to load because the number moved. It is a notification.

## Identity

```yaml
corpus: knowledge-as-code
shortcode: kac
content-version: "0.1.0"
```

`corpus` is what this corpus calls itself. An export states it, so a consumer holding several exports can tell whose
vocabulary it is reading. The folder it vendored the files into may not say.

`shortcode` is the shorthand another corpus cites this one by, as the `eng` in `eng:pol-VURM.TIMEBOX`. You declare it
and a corpus citing yours writes what you chose, so one obligation carries one spelling wherever it is referred to.
[A shortcode is the half before the colon](framework/metadata.md#a-shortcode-is-the-half-before-the-colon) sets out what
a legal one looks like and why it never changes. `kac validate` holds you to it.

Leave the key out until something cites this corpus, which is the ordinary case. [`new`](cli/new.md) writes it bare, and
an export from a corpus that has not declared one states `null`.

## Publishing

```yaml
publishing-target: github
publishing:
  base: https://github.com/paul80nd/knowledge-as-code
  path-prefix: examples/library
```

`publishing-target` is one of `azure-devops`, `azure-devops-wiki`, `github`, `mkdocs` or `none`. You state it rather
than leave it to be guessed, so `export` knows whether it can build a link at all and which form to build.
[The export format](design/export.md#the-link-and-the-ingredients) sets out what an export carries.

`base` is the URL a person opens to browse the corpus. One base, whatever the target: an agent reads a record's source
from the same place, through a client that authenticates.

### What each target says

`github` says the repository is itself the published form, and a record is read rendered on github.com. Write the
repository's own URL, with no `/blob` on the end. `export` adds that segment along with the commit.

`azure-devops` says the corpus lives in Azure Repos and no wiki publishes it. Write the repository's `_git` URL, as
`https://dev.azure.com/{org}/{project}/_git/{repo}`.

`azure-devops-wiki` says an Azure DevOps wiki publishes it. Write the wiki's own URL, as
`https://dev.azure.com/{org}/{project}/_wiki/wikis/{wiki}`, and nothing after it. The address bar shows a numeric page
id once you have navigated to a page, and a base carrying one addresses that page and misaddresses every other record,
so `export` refuses it and builds no links at all.

This is also the one target whose link is not pinned to a commit. No `?pagePath=` URL takes one, so a person following
the link reads whatever the wiki holds now. An agent still reads the version the export was built from, because the
commit reaches it through the manifest rather than through the link.

A descriptor may carry `mkdocs`, and nothing yet addresses it, so an export from a corpus on it carries no links. No
link is better than a link built on a convention nobody has settled.

`none` says a corpus is not published. Its export carries no links and says so in its manifest, so a reader is never
handed an address that resolves nowhere. It is the one value that needs no `publishing:` block.

### A wiki spells a record's path its own way

`export` writes one template, and `{path}` is substituted into it. What that placeholder takes is the target's business.
`github` and `azure-devops` address a file, so they take the record's path whole, as `policies/rtnt-retention.md`. An
Azure DevOps wiki addresses a page, so it takes the same path with `.md` removed and every `/` written as `%2F`.

### Only *where* the corpus is served from lives here

How a record's path and a term's anchor join the base is a property of the target, and it lives in the mechanism. Every
corpus on one target then builds the same link, and none of them writes the rule down a second time.

### `path-prefix` is for a repository holding more than the corpus

You might expect to fold that folder into the base above. The commit a link resolves against sits between the base and
the record's path, so there is nowhere to put it. Leave the key out where the corpus is the repository, which is the
ordinary case.

## `consumes:` names the corpora this one reads

```yaml
consumes:
  - corpus: example-engineering
    shortcode: eng
    version: ^0.1.0
    source: https://nuget.pkg.github.com/OWNER/index.json
    resolved: "0.1.0"
```

One organisation often keeps its policies in one corpus and its teams' knowledge in several others. A team's corpus
cites those policies without holding them, and this block is where it says which corpus it takes them from.
[`restore`](cli/restore.md) fetches each one and unpacks it under `.imports/<shortcode>/`, which is not committed.

| Key         | What it holds                                              | Written by |
|-------------|------------------------------------------------------------|------------|
| `corpus`    | the name the producer publishes the package under          | you        |
| `shortcode` | the word you cite it by, as the `eng` in `eng:pol-VURM`    | you        |
| `version`   | the range you mean, as `0.1.0` or `^0.1.0`                 | you        |
| `source`    | the registry's service index, where the package is fetched | you        |
| `resolved`  | the version the last restore took                          | `restore`  |

The range is what you meant and `resolved` is what your build used, so both sit on the entry rather than in a lock file
beside this one. [`restore`](cli/restore.md) says how a range resolves and what a run refuses.

Leave the key out entirely where this corpus stands on its own, which is the ordinary case.

### `consumes:` and `upstream:` are different relationships

`upstream:` below is one framework flowing down to this corpus as files it receives and holds. `consumes:` is a graph of
records this corpus reads and never holds. A corpus has one upstream and any number of the other.

## Upstream

```yaml
upstream:
  url: https://github.com/paul80nd/knowledge-as-code
  path: template
  ref: main
  commit: 5fa039b03c1e4d7a
  template-version: 4
  taken-on: "2026-08-08"
```

Where this corpus takes the framework from. The first three keys are yours, and `update` writes the last three.

| Key                | What it holds                                            | Written by |
|--------------------|----------------------------------------------------------|------------|
| `url`              | the repository or folder serving the template            | you        |
| `path`             | the folder inside it holding `manifest.yaml`             | you        |
| `ref`              | the branch or tag to take from, followed on every update | you        |
| `commit`           | what the last take resolved to                           | `update`   |
| `template-version` | the template shape that take was on                      | `update`   |
| `taken-on`         | the day the framework last came down                     | `update`   |

`url` is what [`update`](cli/update.md) reads when no `--from` is passed, and a run with neither has nothing to take.
Leave `path` out where the manifest sits at the repository root, which is where this project keeps it.

`ref` is followed and `commit` is never read back. Together they say that a corpus tracks a moving line, and that you
can still see exactly what arrived. A template read from a folder resolves no commit, so `commit` is left as it stands.

### What `new` writes, and what it leaves bare

[`new`](cli/new.md) writes the whole block when the corpus is created, from the flags it was given and the clone it
made. A `new` that read a folder rather than a repository leaves `ref` and `commit` bare, because a folder has no ref
to follow and no commit to resolve.

### Where a schema change goes back

Real content is the only thing that reveals a schema is wrong, so the corpus that found the problem is often the one
best placed to fix it. A change is settled once the repository serving the template accepts it.

## `update-policy:` sets how far an update goes

```yaml
update-policy: cautious
```

One of `cautious` or `full`, and `cautious` is the default.

A seed file is your own words: a type's root page, its `_template.md`, the agent guidance. `cautious` writes one only
where the corpus has none, so an update does not open with three dozen files to revert by hand. `full` refreshes them
too and hands the reconciliation to the diff.

Overlay files are written either way. They are the framework's rather than the corpus's, and an edit to one is drift.

## Adopted types

```yaml
types:
  - adrs
  - policies
```

Validation, index generation and what an update writes all cover the types listed here and no others.
[`new`](cli/new.md) writes the list, because a corpus created by it has already been asked. Omit the key and you have
not declared yet, so the tool reads adoption off the folders it finds. A type counts where both halves are there,
meaning the page and the folder.

### `validate` holds the corpus to the list it declared

"These folders happen to be here" becomes "these are the types this corpus chose", and validation can hold the corpus
to it. A type you declined is left alone, whatever `.schema/` says about it. Once you have declared, standing a type up
without adopting it is a defect [`validate`](cli/validate.md) reports.

### `--add-type` and `--drop-type` are what change the list

They move the name and the type's files together. Editing the list by hand leaves the corpus holding a type it does not
claim, or claiming one it does not hold.

## `export.exclude:` drops a record from the output

```yaml
export:
  exclude: [ ]
```

Empty by default, and that is the important part. A record still in draft travels, and so does one whose review date has
passed. Each carries its own state, so a consumer reads what the corpus actually holds and decides for itself how far to
trust it.

Filter here and you make the corpus's own state invisible downstream. Your consumer sees a smaller, tidier vocabulary
and no sign that anything was withheld. The option is there for a corpus publishing to an audience it cannot warn.

Two values are accepted. `draft` drops a record whose status says so, and `overdue` drops one whose `review-by` is in
the past.

## Where the plugin tree comes from

```yaml
plugin:
  from: ../../shared/.plugin
```

`kac bundle` reads the plugin tree, meaning the skills and hooks an agent installs, from `.plugin/` at the corpus root.
Say `plugin.from` and it reads them from one tree elsewhere instead, resolved against the corpus root. Several corpora
in a repository then share one copy, and `update` withholds the shared half rather than writing it here.

The manifest is never read from the shared tree. It carries the name the plugin installs under, so it stays at
`.plugin/.claude-plugin/plugin.json` in each corpus, along with the components that corpus declares. A file a corpus
writes beside it wins over the shared tree's copy of the same path, so one skill can be overridden.

Omit the key and the corpus keeps its own tree, which is what [`new`](cli/new.md) creates.

## `skip:` is how a corpus takes a file back

```yaml
skip:
  - path: .plugin/hooks/breadcrumb
    reason: Patched for our proxy.
```

Each entry names a file and says why. A path listed here is neither read nor written, in either direction, so this is
the one way to say "I own this and I mean it" about a file the framework would otherwise reclaim on every run. `update`
steps over them and reports what it stepped over. Delete an entry once the file matches the framework again.

The reason is for whoever opens the file next.
[Layers](design/layers.md#skip-is-how-a-corpus-takes-a-file-back) says what it separates.

[The export format](design/export.md) is this page's other half: here is what a corpus writes about itself, and there
is what a consumer reads out of it.
