# `.corpus.yaml`

`.corpus.yaml` sits at the root of a corpus (one repository of knowledge records kept in git). It states what the corpus
is, which types it adopted, where it is published, and where it takes the shared framework from.

[`new`](cli/new.md) writes the file when the corpus is created, commented key by key. You edit it by hand after that.
`update` rewrites four of the keys below and never replaces the file. Every other file in a corpus arrives from a
template and `update` can take it again; this one is the corpus's own.

The longer worked copy is
[`examples/library/.corpus.yaml`](https://github.com/paul80nd/knowledge-as-code/blob/main/examples/library/.corpus.yaml).
Read that one while you change yours.

**It is not the schema.** `.schema/` states what a record of each type must have. This file states which of those types
the corpus adopted. It says nothing about their shape.

**It is not configuration for a run.** It records decisions, and a command reads them. `--from`, `--ref`, `--path` and
`--policy` each replace a value here for one run only.

## Every key

| Key                                                              | What it states                                  | Who writes it                   |
|------------------------------------------------------------------|-------------------------------------------------|---------------------------------|
| [`descriptor-version`](#descriptor-version)                      | the format of this file                         | `new`, then `update`            |
| [`corpus`](#corpus)                                              | the name this corpus publishes under            | `new`                           |
| [`shortcode`](#shortcode)                                        | the word another corpus cites this one by       | you                             |
| [`content-version`](#content-version)                            | the version of what this corpus knows           | `new`, then you                 |
| [`display-name`](#what-a-reader-sees-outside-the-repository)     | what a person calls this corpus                 | you                             |
| [`description`](#what-a-reader-sees-outside-the-repository)      | what this corpus is, in a sentence              | you                             |
| [`license`](#what-a-reader-sees-outside-the-repository)          | the licence it publishes under                  | you                             |
| [`author`](#what-a-reader-sees-outside-the-repository)           | who publishes it, as a name and a URL           | you                             |
| [`publishing-target`](#publishing-target)                        | how the corpus is published                     | `new`                           |
| [`publishing.base`](#publishing)                                 | the URL a person opens to browse it             | `new`, then you                 |
| [`publishing.path-prefix`](#publishing)                          | where the corpus sits inside its repository     | you                             |
| [`repos-base`](#repos-base)                                      | where this estate's code repositories live      | you                             |
| [`tracker`](#tracker)                                            | where work about this corpus's records is filed | you                             |
| [`framework`](#framework)                                        | where to report a problem with the framework    | `new`                           |
| [`upstream.url`, `.path`, `.ref`](#upstream)                     | where the framework is taken from               | `new`, then you                 |
| [`upstream.commit`, `.template-version`, `.taken-on`](#upstream) | what the last take resolved to                  | `update`                        |
| [`update-policy`](#update-policy)                                | how far an update goes                          | `new`                           |
| [`types`](#types)                                                | the types this corpus adopted                   | `new`, then `update --add-type` |
| [`enums`](#enums)                                                | the ranges the schema leaves to the corpus      | you                             |
| [`consumes`](#consumes)                                          | the corpora this one reads                      | you, and `restore` resolves     |
| [`export.exclude`](#export)                                      | what the export leaves behind                   | `new`                           |
| [`plugin.from`](#plugin)                                         | where the plugin tree is read from              | you                             |
| [`skip`](#skip)                                                  | the files this corpus owns                      | you                             |

## Identity

No key here is called `version` alone, because three of them say different things: the format of this file, the version
of what the corpus knows, and the template shape it last took.

### `corpus`

The name this corpus publishes under.

```yaml
corpus: knowledge-as-code
```

An export states it, so a consumer with several exports can tell whose vocabulary it is reading. The folder it vendored
the files into may not say.

### `shortcode`

The shorthand another corpus cites this one by, as the `eng` in `eng:pol-VURM.TIMEBOX`.

```yaml
shortcode: kac
```

You choose it, and a corpus citing yours writes what you chose. [`validate`](cli/validate.md) checks the spelling.
[The shortcode](framework/metadata.md#the-shortcode) says what a legal one looks like, and why it never changes.

Leave it out until something cites this corpus, which is the ordinary case. [`new`](cli/new.md) writes it bare, and an
export from a corpus that has not chosen one states `null`.

### `content-version`

The version of what this corpus knows.

```yaml
content-version: "0.1.0"
```

It is semantically versioned. Move the major where a meaning changed or a published URL broke. Move the minor for
additions, and the patch for wording. Quote it when one corpus tells another which version of the content it has.

Nothing refuses to load because the number moved. It is a notification.

### `descriptor-version`

The format of this file, which the tool owns rather than the corpus.

```yaml
descriptor-version: 5
```

[`update`](cli/update.md) stamps it with the format the tool writes, so a corpus records which shape it last took.

## What a reader sees outside the repository

These four keys are for a reader who meets this corpus as a package or an installed plugin, not as a repository.

```yaml
display-name: Knowledge as Code
description: >
  The engineering knowledge of one organisation, held as records a person and an agent read the same way.
license: MIT
author:
  name: A Person
  url: https://github.com/a-person
```

[`export`](cli/export.md) writes all four into the export, [`pack`](cli/pack.md) writes them into the package a registry
lists, and [`bundle`](cli/bundle.md) writes them into the plugin manifest somebody installs.

Leave a key bare and all three omit it. [`new`](cli/new.md) writes all four bare, because a licence and an author
supplied by the tool would be inherited instead of chosen.

## Where the corpus is published

### `publishing-target`

How the corpus is published, which decides whether `export` can build a link at all and which form to build.

```yaml
publishing-target: github
```

State it instead of leaving it to be guessed. [The export format](design/export.md#the-link-and-the-ingredients) says
what an export writes, and how a record's path and a part's anchor join the base. Every corpus on one target builds the
same link, and none of them writes the rule down a second time.

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

[`validate`](cli/validate.md) refuses any other value, and names these five in the message.

### `publishing`

Where the published corpus is served from, and nothing else.

```yaml
publishing:
  base: https://github.com/paul80nd/knowledge-as-code
  path-prefix: examples/library
```

| Key           | What it states                                       | Who writes it   |
|---------------|------------------------------------------------------|-----------------|
| `base`        | the URL a person opens to browse the corpus          | `new`, then you |
| `path-prefix` | where the corpus root sits inside that repository    | you             |

There is one base, whatever the target. An agent reads a record's source from the same place, through a client that
authenticates.

Set `path-prefix` where the repository contains more than the corpus. Do not append that folder to `base` instead.
`export` inserts the commit between `base` and the record's path, so there is no room for it there. Leave the key out
where the corpus is the repository, which is the ordinary case.

## Where the code is

### `repos-base`

Where this estate's code repositories are addressed from. A `services` record states a bare repository name under
`repos`, and `kac` resolves it to `<repos-base>/<entry>`.

```yaml
repos-base: https://git.example.com/example-libraries
```

Write whatever sits between the host and the repository name into the base. An Azure DevOps corpus addresses a
repository as `https://dev.azure.com/{org}/{project}/_git/{repo}`, so its base ends in `_git`:

```yaml
repos-base: https://dev.azure.com/acme/platform/_git
```

`mirrors-repo-links` compares the resolved set against the links a record writes under `Where it lives`, in both
directions, and warns where the two disagree. It reports nothing in a corpus that states no base, and nothing in a
corpus that has not adopted `services`.

One base covers one host and one organisation. Leave it bare where the estate spans several. The check then stops,
rather than reporting every repository outside the one host you could name.

## Where work is filed

### `tracker`

Where work about this corpus's records is filed.

```yaml
tracker:
  target: azure-devops
  base: https://dev.azure.com/acme/Standards
  area: kac-it-swdev
```

| Key      | What it states                                          | Who writes it |
|----------|---------------------------------------------------------|---------------|
| `target` | the client that opens a ticket on the tracker           | you           |
| `base`   | the repository or project the backlog belongs to        | you           |
| `area`   | the area path inside an Azure DevOps project to file in | you           |

**Leave the block out where the backlog belongs to what `publishing:` already names.** [`export`](cli/export.md) derives
the address from that block instead, by the table below. A corpus published on GitHub therefore states nothing: a
repository there has an issue list of its own.

**State the block where the backlog and the published form are two places.** One Azure DevOps project holds one backlog
and many repositories, so a repository URL does not address the backlog.

`target` is one of `github`, `azure-devops` or `none`, the same three values [`framework`](#framework) takes. Write
`none` where the corpus has no tracker at all. [`validate`](cli/validate.md) refuses any other value.

Write the project URL for `azure-devops`, as `https://dev.azure.com/{org}/{project}`. A URL naming a repository or a
wiki inside the project is cut back to the project, because that is where the backlog is.

#### `area`

`area` is the area path below that project a new work item is filed under. Write it separated by `/`, the way `base` is
already written, as `kac-it-swdev` or `kac-it-swdev/subteam`. The client that files converts it to the backslashes Azure
Boards uses.

**State it where one Azure DevOps project holds several corpora.** One project has one backlog, so without an area every
corpus's work lands in one undifferentiated queue. With one, a team sees its own.

**Leave it out and an item lands in the project's default area.** That is the ordinary case for a project holding one
corpus.

`area` applies to `azure-devops` alone. GitHub divides one repository's issue list with labels, so
[`validate`](cli/validate.md) refuses an `area` stated beside `github` or `none`. A block stating no `target` is asked
about the one [`publishing-target`](#publishing-target) implies, because that is the client that will file.

Nothing derives `area`, and [`new`](cli/new.md) and [`update`](cli/update.md) never write it. Only the corpus knows
which area inside a shared project is its own. Set it by hand.

`area` is not checked against Azure DevOps. `validate` reaches no network. An area path that does not exist, or one the
identity filing has no rights to, fails at the point the item is created.

#### What `export` derives

| `publishing-target` | The tracker derived from `base`             |
|---------------------|---------------------------------------------|
| `github`            | the repository, which is its own issue list |
| `azure-devops`      | the project holding the repository          |
| `azure-devops-wiki` | the project holding the wiki                |
| `mkdocs` and `none` | none                                        |

Write `base` alone where your backlog moved inside the platform you publish on, and the client follows from
`publishing-target`. Write `target` alone where the corpus has no tracker, and nothing states a base.

**State both keys where you state a target `publishing-target` does not imply.** A base is taken from the publishing
block only where the two targets agree. A corpus published on GitHub and filing on Azure Boards is filing somewhere only
it knows, so `export` writes no base rather than the repository it publishes from.

### `framework`

Where to report a problem with the framework itself: `kac`, the schema, the template or a skill.

```yaml
framework:
  target: github
  base: https://github.com/paul80nd/knowledge-as-code
```

| Key      | What it states                                          | Who writes it |
|----------|---------------------------------------------------------|---------------|
| `target` | the client that opens an issue on the tracker           | `new`         |
| `base`   | the repository or project the tracker belongs to        | `new`         |
| `area`   | the area path inside an Azure DevOps project to file in | you           |

[`export`](cli/export.md) writes the block into the manifest, so an agent that meets your corpus as an installed plugin
has the address. Without it, the only address that agent has is your own `publishing.base`.

`area` means here what it means on [`tracker`](#area), and is held to the same rule. `new` writes none.

`target` takes the same three values [`tracker`](#tracker) takes. Write `github` for a tracker on github.com, and
`azure-devops` for one on Azure Boards. Write `none`, or leave the block out, where you know of no tracker.
`azure-devops-wiki` and `mkdocs`, which [`publishing-target`](#publishing-target) also takes, address no tracker, so
neither belongs here.

[`new`](cli/new.md) writes both keys when the corpus is created. It takes `base` from `--from` where that named a
repository, and from the framework's own repository where it named a folder. It reads `target` from the host, and writes
`none` for a host that neither `gh` nor `az boards` serves.

Nothing derives this block. Where a corpus publishes says nothing about who maintains what it took.

#### Three addresses, and telling them apart

`tracker.base` is where a problem with one of your records goes. `framework.base` is where a problem with `kac`, the
schema, the template or a skill goes. Each corpus in [`consumes:`](#consumes) brings a third, which is its producer's.
A corpus whose records and framework are maintained by the same team writes one address twice, and
[the export format](design/export.md#id-and-the-trackers-it-compares) says how a reader tells that case from the other.

## Where the framework comes from

### `upstream`

Where the corpus takes the framework from, and what the last take resolved to.

```yaml
upstream:
  url: https://github.com/paul80nd/knowledge-as-code
  path: template
  ref: main
  commit: 1d5b531d5bae80dc9805dc501ccc6fe6aa2b4141
  template-version: 11
  taken-on: "2026-08-24"
```

| Key                | What it states                                           | Who writes it |
|--------------------|----------------------------------------------------------|---------------|
| `url`              | the repository or folder serving the template            | you           |
| `path`             | the folder inside it holding `manifest.yaml`             | you           |
| `ref`              | the branch or tag to take from, followed on every update | you           |
| `commit`           | what the last take resolved to, as a full-length sha     | `update`      |
| `template-version` | the template shape that take was on                      | `update`      |
| `taken-on`         | the day the framework last came down                     | `update`      |

`update` reads `url` when no `--from` is passed, and a run with neither has nothing to take. Leave `path` out where the
manifest sits at the repository root, which is where this project keeps it.

`update` follows `ref` and never reads `commit` back. Together the two say that the corpus tracks a moving line, and
that you can still see exactly what arrived. A template read from a folder resolves no commit, so `commit` is left as it
stands, and [`new`](cli/new.md) leaves `ref` beside it bare.

Real content is the only thing that reveals a schema is wrong, so the corpus that found the problem is often the one
best placed to fix it. A change is settled once the repository serving the template accepts it.

**`upstream` is not [`consumes`](#consumes).** `upstream` is one framework this corpus receives as files and keeps.
`consumes` is a graph of records this corpus reads and never keeps. A corpus has one upstream, and any number of
consumed corpora.

**`upstream.url` is not [`framework.base`](#framework).** `upstream.url` says where `kac` copied the template from, and
it is often a folder on the same disk. `framework.base` says where to report a problem with what arrived, and it is
always a tracker.

### `update-policy`

How far an update goes. One of `cautious` or `full`, and `cautious` is the default.

```yaml
update-policy: cautious
```

A seed file is your own words: a type's root page, its `_template.md`, the agent guidance. `cautious` writes a seed file
only where the corpus has none, so an update does not open with three dozen files to revert by hand. `full` refreshes
them too, and you reconcile the result from the diff.

`update` writes overlay files either way. They belong to the framework, not to the corpus, and an edit to one is drift.

## `types`

The types this corpus adopted.

```yaml
types:
  - adrs
  - policies
```

Validation, index generation and what an update writes all cover the types listed here and no others.

[`new`](cli/new.md) writes the list, because a corpus it created has already been asked. Omit the key and you have not
declared yet, so `kac` reads adoption off the folders it finds. A type counts where both halves are there, meaning the
page and the folder.

Once the key is declared, [`validate`](cli/validate.md) checks the corpus against it in both directions. It reports a
type in the list that is not stood up, and a type stood up that is not in the list. A corpus that declares nothing is
asked neither question.

Use [`update --add-type`](cli/update.md) and `update --drop-type` to change the list. Each moves the name and the type's
files together. Editing the list by hand leaves the corpus with a type it does not claim, or claiming one it does not
have.

## `enums`

The ranges the schema leaves to the corpus.

```yaml
enums:
  component-type: [asset, cli, website]
  platform: [dotnet, static]
```

Some fields take a value the framework cannot know. What a service is built on, and what sort of component it is, are
one pair of lists in a library and another in a payments platform. So `.schema/services.yaml` declares
`values: $corpus.platform` and `values: $corpus.component-type`, and lists no values of its own. You write each range
here, and [`validate`](cli/validate.md) checks every record against it.

Walk your own deployables, group them by the runtime and framework a contributor has to know, and close the list on what
you found. Group them a second time by what they are, and close that list the same way. The type page gives the method
in full, and each corpus's copy records the values it settled on.

**Write each value in lower case.** An enum value is a grep target first and prose second. A range with `Dotnet-Web` in
it refuses `dotnet-web` from one side and `Dotnet-Web` from the other, because `enum-lowercase` refuses the value in the
record. No record can satisfy such a range, which is the same state as a range you never wrote.

A corpus created by [`new`](cli/new.md) receives the key with nothing under it, because a corpus with no records has no
estate to derive a range from. Write a record with such a field before you write the range, and `validate` reports
`corpus-enum-undeclared` once, against `.corpus.yaml`. Nobody who wrote a record can fix that, so it is reported where
the person who can fix it works. A required field is asked from the first record of its type, because every record of
that type must carry a value. An optional one is asked from the first record that states it, because a record satisfies
an optional field by leaving the key out.

A value the range does not admit is an ordinary `enum` failure, reported against the record. The message quotes the
values your descriptor lists, so an author reads back the list they are being checked against.

## `consumes`

The corpora this one reads.

```yaml
consumes:
  - corpus: example-engineering
    shortcode: eng
    version: ^0.1.0
    source: https://nuget.pkg.github.com/OWNER/index.json
    resolved: "0.1.0"
```

| Key         | What it states                                          | Who writes it |
|-------------|---------------------------------------------------------|---------------|
| `corpus`    | the name the producer publishes the package under       | you           |
| `shortcode` | the word you cite it by, as the `eng` in `eng:pol-VURM` | you           |
| `version`   | the range you mean, as `0.1.0` or `^0.1.0`              | you           |
| `source`    | the registry or folder the package is fetched from      | you           |
| `resolved`  | the version the last restore took                       | `restore`     |

One organisation often keeps its policies in one corpus and each team's knowledge in another. A team's corpus cites
those policies without keeping a copy, and this block says which corpus it takes them from.
[`restore`](cli/restore.md) fetches each one and unpacks it under `.imports/<shortcode>/`, which is not committed.

The range says what you meant. `resolved` says what your build used. Both sit on the entry, so there is no lock file
beside this one. A `source` is a registry's service index, or a path to a folder holding what a producer built.
[`restore`](cli/restore.md) covers how a range resolves, what a folder source takes, and what a run refuses.

Leave the key out entirely where this corpus stands on its own, which is the ordinary case.

## `export`

What the export leaves behind.

```yaml
export:
  exclude: [ ]
```

It is empty by default, and that is the important part. A record still in draft travels, and so does one whose review
date has passed. Each record states its own status, so a consumer reads what the corpus actually has and decides for
itself how far to trust it.

Filter here and the corpus's own state becomes invisible downstream. Your consumer sees a smaller, tidier vocabulary and
no sign that anything was withheld. Reach for it where you cannot warn the audience you publish to. Reach for it as well
where a draft would be read as an answer: a `fix` in draft is a resolution nobody has checked, and `fix-lookup` hands
one over as it would any other.

Two values are accepted. `draft` drops a record whose status says so. `overdue` drops one whose `review-by` is in the
past.

## `plugin`

Where [`bundle`](cli/bundle.md) reads the plugin tree from, meaning the skills and hooks an agent installs.

```yaml
plugin:
  from: ../../shared/.plugin
```

It reads them from `.plugin/` at the corpus root by default. Set `plugin.from` and it reads them from one tree
elsewhere, resolved against the corpus root. Several corpora in a repository then share one copy, and `update` withholds
the shared half instead of writing it here.

`kac` never reads the manifest from the shared tree. The manifest gives the name the plugin installs under, so it stays
at `.plugin/.claude-plugin/plugin.json` in each corpus, along with the components that corpus declares. A file a corpus
writes beside it wins over the shared tree's copy of the same path, so one skill can be overridden.

Omit the key and the corpus keeps its own tree, which is what [`new`](cli/new.md) creates.

## `skip`

The files this corpus owns.

```yaml
skip:
  - path: .plugin/hooks/breadcrumb
    reason: Patched for our proxy.
```

`update` neither reads nor writes a path listed here, in either direction, so an entry is how you keep a file the
framework would otherwise reclaim on every run. `update` reports each file it stepped over. Delete an entry once the
file matches the framework again.

Each entry gives a path and a reason. The reason is for whoever opens the file next.
[Layers](design/layers.md#skip) says what it separates.

[The export format](design/export.md) is the page for what a consumer reads out of a corpus.
