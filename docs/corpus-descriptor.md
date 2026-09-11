# `.corpus.yaml`

## What it is for

`.corpus.yaml` sits at the root of a corpus (one repository of knowledge records kept in git). It states what the corpus
is, which types it adopted, and where it takes the shared framework from.

Every other file in a corpus arrives from a template, and `update` can take it again. This file is the corpus's own.
`update` never replaces it. It rewrites four keys, and the tables below mark each one.

[`new`](cli/new.md) writes the file when the corpus is created, commented key by key. After that you edit it by hand.
The longer worked copy is [`examples/library/.corpus.yaml`](https://github.com/paul80nd/knowledge-as-code/blob/main/examples/library/.corpus.yaml).
Read that one while you change yours.

## What it is not

**It is not the schema.** `.schema/` states what a record of each type must have. This file states which of those types
the corpus adopted. It says nothing about their shape.

**It is not configuration for a run.** It records decisions, and a command reads them. `--from`, `--ref`, `--path` and
`--policy` each replace a value here for one run only.

## The three versions

No key is called `version` alone, because the three say different things.

| Key                         | What it states                        | Written by | Rewritten by |
|-----------------------------|---------------------------------------|------------|--------------|
| `descriptor-version`        | the format of this file               | `new`      | `update`     |
| `content-version`           | the version of what this corpus knows | `new`      | you          |
| `upstream.template-version` | the template shape this corpus took   | `new`      | `update`     |

`content-version` is semantically versioned. Move the major where a meaning changed or a published URL broke. Move the
minor for additions, and the patch for wording. Quote it when one corpus tells another which version of the content it
has. Nothing refuses to load because the number moved. It is a notification.

## Identity

```yaml
corpus: knowledge-as-code
shortcode: kac
content-version: "0.1.0"
```

`corpus` is the name this corpus publishes under. An export states it, so a consumer with several exports can tell whose
vocabulary it is reading. The folder it vendored the files into may not say.

`shortcode` is the shorthand another corpus cites this one by, as the `eng` in `eng:pol-VURM.TIMEBOX`. You choose it,
and a corpus citing yours writes what you chose. `kac validate` checks the spelling.
[The shortcode](framework/metadata.md#the-shortcode) says what a legal one looks like, and why it never changes.

Leave `shortcode` out until something cites this corpus, which is the ordinary case. [`new`](cli/new.md) writes it bare,
and an export from a corpus that has not chosen one states `null`.

### Keys a reader sees outside the repository

```yaml
display-name: Knowledge as Code
description: >
  The engineering knowledge of one organisation, held as records a person and an agent read the same way.
license: MIT
author:
  name: A Person
  url: https://github.com/a-person
```

These four keys are for a reader who meets this corpus as a package or an installed plugin, not as a repository.
[`export`](cli/export.md) writes all four into the export, [`pack`](cli/pack.md) writes them into the package a registry
lists, and [`bundle`](cli/bundle.md) writes them into the plugin manifest somebody installs.

Leave a key bare and all three omit it. [`new`](cli/new.md) writes all four bare, because a licence and an author
supplied by the tool would be inherited instead of chosen.

## Publishing

```yaml
publishing-target: github
publishing:
  base: https://github.com/paul80nd/knowledge-as-code
  path-prefix: examples/library
```

`publishing-target` says how the corpus is published. You state it instead of leaving it to be guessed, so `export`
knows whether it can build a link at all, and which form to build.
[The export format](design/export.md#the-link-and-the-ingredients) says what an export writes.

`base` is the URL a person opens to browse the corpus. There is one base, whatever the target. An agent reads a record's
source from the same place, through a client that authenticates.

### Values of `publishing-target`

**`github`** means the repository is itself the published form, and a record is read rendered on github.com. Write the
repository's own URL, with no `/blob` on the end. `export` adds that segment along with the commit.

**`azure-devops`** means the corpus lives in Azure Repos and no wiki publishes it. Write the repository's `_git` URL, as
`https://dev.azure.com/{org}/{project}/_git/{repo}`.

**`azure-devops-wiki`** means an Azure DevOps wiki publishes it. Write the wiki's own URL, as
`https://dev.azure.com/{org}/{project}/_wiki/wikis/{wiki}`, and nothing after it. Once you open a page, the address bar
shows a numeric page id. A base with one of those in it addresses that page and misaddresses every other record, so
`export` refuses it and builds no links at all.

This is also the one target whose link is not pinned to a commit. No `?pagePath=` URL takes one, so a person following
the link reads whatever the wiki has now. An agent still reads the version the export was built from, because the commit
travels in the manifest instead of in the link.

**`mkdocs`** is accepted, and nothing addresses it yet, so an export from a corpus on it has no links. No link is better
than a link built on a convention nobody has settled.

**`none`** means the corpus is not published. Its export has no links and says so in its manifest, so a reader is never
handed an address that resolves nowhere. It is the one value that needs no `publishing:` block.

### The `{path}` placeholder

`export` writes one link template and substitutes `{path}` into it. What that placeholder takes belongs to the target.
`github` and `azure-devops` address a file, so they take the record's path whole, as `policies/rtnt-retention.md`. An
Azure DevOps wiki addresses a page, so it takes the same path with `.md` removed and every `/` written as `%2F`.

### What the mechanism decides

Put only the address the corpus is served from in `publishing:`. How a record's path and a term's anchor join that
address belongs to the target, and `kac` applies it. Every corpus on one target then builds the same link, and none of
them writes the rule down a second time.

### `path-prefix`

Set `path-prefix` where the repository contains more than the corpus. Do not append that folder to `base` instead.
`export` inserts the commit between `base` and the record's path, so there is no room for it there. Leave the key out
where the corpus is the repository, which is the ordinary case.

## `consumes:`

```yaml
consumes:
  - corpus: example-engineering
    shortcode: eng
    version: ^0.1.0
    source: https://nuget.pkg.github.com/OWNER/index.json
    resolved: "0.1.0"
```

`consumes:` lists the corpora this one reads. One organisation often keeps its policies in one corpus and each team's
knowledge in another. A team's corpus cites those policies without keeping a copy, and this block says which corpus it
takes them from. [`restore`](cli/restore.md) fetches each one and unpacks it under `.imports/<shortcode>/`, which is not
committed.

| Key         | What it states                                          | Written by |
|-------------|---------------------------------------------------------|------------|
| `corpus`    | the name the producer publishes the package under       | you        |
| `shortcode` | the word you cite it by, as the `eng` in `eng:pol-VURM` | you        |
| `version`   | the range you mean, as `0.1.0` or `^0.1.0`              | you        |
| `source`    | the registry or folder the package is fetched from      | you        |
| `resolved`  | the version the last restore took                       | `restore`  |

The range says what you meant. `resolved` says what your build used. Both sit on the entry, so there is no lock file
beside this one. A `source` is a registry's service index, or a path to a folder holding what a producer built.
[`restore`](cli/restore.md) covers how a range resolves, what a folder source takes, and what a run refuses.

Leave the key out entirely where this corpus stands on its own, which is the ordinary case.

### `consumes:` and `upstream:`

These are two different relationships. `upstream:` is one framework this corpus receives as files and keeps.
`consumes:` is a graph of records this corpus reads and never keeps. A corpus has one upstream, and any number of
consumed corpora.

## Upstream

```yaml
upstream:
  url: https://github.com/paul80nd/knowledge-as-code
  path: template
  ref: main
  commit: 1d5b531d5bae80dc9805dc501ccc6fe6aa2b4141
  template-version: 11
  taken-on: "2026-08-24"
```

`upstream:` says where the corpus takes the framework from. The first three keys are yours, and `update` writes the last
three.

| Key                | What it states                                           | Written by |
|--------------------|----------------------------------------------------------|------------|
| `url`              | the repository or folder serving the template            | you        |
| `path`             | the folder inside it holding `manifest.yaml`             | you        |
| `ref`              | the branch or tag to take from, followed on every update | you        |
| `commit`           | what the last take resolved to, as a full-length sha     | `update`   |
| `template-version` | the template shape that take was on                      | `update`   |
| `taken-on`         | the day the framework last came down                     | `update`   |

`update` reads `url` when no `--from` is passed, and a run with neither has nothing to take. Leave `path` out where the
manifest sits at the repository root, which is where this project keeps it.

`update` follows `ref` and never reads `commit` back. Together the two say that the corpus tracks a moving line, and
that you can still see exactly what arrived. A template read from a folder resolves no commit, so `commit` is left as it
stands.

### What `new` writes, and what it leaves bare

[`new`](cli/new.md) writes the whole block when the corpus is created, from the flags it was given and the clone it
made. A `new` that read a folder instead of a repository leaves `ref` and `commit` bare, because a folder has no ref to
follow and no commit to resolve.

### Where a schema change goes back

Real content is the only thing that reveals a schema is wrong, so the corpus that found the problem is often the one
best placed to fix it. A change is settled once the repository serving the template accepts it.

## `update-policy:`

```yaml
update-policy: cautious
```

`update-policy:` sets how far an update goes. It is one of `cautious` or `full`, and `cautious` is the default.

A seed file is your own words: a type's root page, its `_template.md`, the agent guidance. `cautious` writes a seed file
only where the corpus has none, so an update does not open with three dozen files to revert by hand. `full`
refreshes them too, and you reconcile the result from the diff.

`update` writes overlay files either way. They belong to the framework, not to the corpus, and an edit to one is drift.

## Adopted types

```yaml
types:
  - adrs
  - policies
```

`types:` lists the types this corpus adopted. Validation, index generation and what an update writes all cover the types
listed here and no others.

[`new`](cli/new.md) writes the list, because a corpus it created has already been asked. Omit the key and you have not
declared yet, so `kac` reads adoption off the folders it finds. A type counts where both halves are there, meaning the
page and the folder.

### Why declare the list

Once `types:` is declared, `validate` checks the corpus against it in both directions. It reports a type in the list
that is not stood up, and a type stood up that is not in the list. A corpus that declares nothing is asked neither
question. Adoption is read off its folders, so the list and the folders cannot disagree.

### `--add-type` and `--drop-type`

Use [`update --add-type`](cli/update.md) and `update --drop-type` to change the list. Each moves the name and the type's
files together. Editing the list by hand leaves the corpus with a type it does not claim, or claiming one it does not
have.

## `enums:`

```yaml
enums:
  platform: [dotnet-tool, static]
```

`enums:` states the ranges the schema leaves to the corpus. Some fields take a value the framework cannot know. What a
service is built on is one list in a library and another in a payments platform, so `.schema/services.yaml` declares
`values: $corpus.platform` and lists no values of its own. You write the range here, and `kac validate` checks every
record against it.

Walk your own deployables, group them by the runtime and framework a contributor has to know, and close the list on what
you found. The type page gives the method in full, and each corpus's copy records the values it settled on.

### When `validate` asks for the range

A corpus created by [`new`](cli/new.md) receives the key with nothing under it, because a corpus with no records has no
estate to derive a range from. Write a record with such a field before you write the range, and
[`validate`](cli/validate.md) reports `corpus-enum-undeclared` once, against `.corpus.yaml`. Nobody who wrote a record
can fix that, so it is reported where the person who can fix it works.

### A value outside the range

A value the range does not admit is an ordinary `enum` failure, reported against the record. The message quotes the
values your descriptor lists, so an author reads back the list they are being checked against.

### Write each value in lower case

An enum value is a grep target first and prose second. A range with `Dotnet-Web` in it refuses `dotnet-web` from one
side and `Dotnet-Web` from the other, because `enum-lowercase` refuses the value in the record. No record can satisfy
such a range, which is the same state as a range you never wrote, so `corpus-enum-undeclared` reports it the same way.

## `export.exclude:`

```yaml
export:
  exclude: [ ]
```

`export.exclude:` drops a record from the export. It is empty by default, and that is the important part. A record still
in draft travels, and so does one whose review date has passed. Each record states its own status, so a consumer reads
what the corpus actually has and decides for itself how far to trust it.

Filter here and the corpus's own state becomes invisible downstream. Your consumer sees a smaller, tidier vocabulary and
no sign that anything was withheld. The option is there for a corpus publishing to an audience it cannot warn.

Two values are accepted. `draft` drops a record whose status says so. `overdue` drops one whose `review-by` is in the
past.

## The plugin tree

```yaml
plugin:
  from: ../../shared/.plugin
```

`kac bundle` reads the plugin tree, meaning the skills and hooks an agent installs, from `.plugin/` at the corpus root.
Set `plugin.from` and it reads them from one tree elsewhere instead, resolved against the corpus root. Several corpora
in a repository then share one copy, and `update` withholds the shared half instead of writing it here.

`kac` never reads the manifest from the shared tree. The manifest gives the name the plugin installs under, so it stays
at `.plugin/.claude-plugin/plugin.json` in each corpus, along with the components that corpus declares. A file a corpus
writes beside it wins over the shared tree's copy of the same path, so one skill can be overridden.

Omit the key and the corpus keeps its own tree, which is what [`new`](cli/new.md) creates.

## `skip:`

```yaml
skip:
  - path: .plugin/hooks/breadcrumb
    reason: Patched for our proxy.
```

`skip:` lists the files this corpus owns. `update` neither reads nor writes a path listed here, in either direction, so
an entry is how you keep a file the framework would otherwise reclaim on every run. `update` reports each file it
stepped over. Delete an entry once the file matches the framework again.

Each entry gives a path and a reason. The reason is for whoever opens the file next.
[Layers](design/layers.md#skip) says what it separates.

[The export format](design/export.md) is the page for what a consumer reads out of a corpus.
