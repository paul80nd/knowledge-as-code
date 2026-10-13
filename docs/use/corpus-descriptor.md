# `.corpus.yaml`

`.corpus.yaml` sits at the root of a corpus (one repository of knowledge records kept in git). It states what the corpus
is, which types it adopted, where it is published, and where it takes the shared framework from.

[`new`](cli/new.md) writes the file when the corpus is created, commented key by key. You edit it by hand after that.
`update` rewrites four of the keys below and never replaces the file. A worked copy is
[`examples/library/.corpus.yaml`](https://github.com/paul80nd/knowledge-as-code/blob/main/examples/library/.corpus.yaml).

The file says which types the corpus adopted, and `.schema/` says what a record of each type must have. `--from`,
`--ref`, `--path` and `--policy` each replace a value here for one run only.

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

Three keys state a version: `descriptor-version` for the format of this file, `content-version` for what the corpus
knows, and [`upstream.template-version`](#upstream) for the template shape it last took.

### `corpus`

The name this corpus publishes under.

```yaml
corpus: knowledge-as-code
```

An export states it, so a consumer with several exports can tell whose vocabulary it is reading.

### `shortcode`

The shorthand another corpus cites this one by, as the `eng` in `eng:pol-VURM.TIMEBOX`.

```yaml
shortcode: kac
```

[`validate`](cli/validate.md) checks the spelling, and [the shortcode](../framework/metadata.md#the-shortcode) says what
a legal one looks like. Leave it out until something cites this corpus. [`new`](cli/new.md) writes it bare, and an
export from a corpus with none states `null`.

### `content-version`

The version of what this corpus knows.

```yaml
content-version: "0.1.0"
```

It is semantically versioned. Move the major where a meaning changed or a published URL broke. Move the minor for
additions, and the patch for wording. Nothing refuses to load because the number moved.

### `descriptor-version`

The format of this file.

```yaml
descriptor-version: 5
```

[`update`](cli/update.md) stamps it with the format the tool writes, so a corpus records which shape it last took.

## What a reader sees outside the repository

These four keys are for a reader who meets this corpus as a package or an installed plugin.

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

Leave a key bare and all three omit it. [`new`](cli/new.md) writes all four bare.

## Where the corpus is published

### `publishing-target`

How the corpus is published, which decides whether `export` builds a link to each record and in which form.

```yaml
publishing-target: github
```

[The export format](../reference/export.md#the-link-and-the-ingredients) says how a record's path and a part's anchor
join the base.

**`github`** means the repository is itself the published form, and a record is read rendered on github.com. Write the
repository's own URL, with no `/blob` on the end. `export` adds that segment along with the commit.

**`azure-devops`** means the corpus lives in Azure Repos and no wiki publishes it. Write the repository's `_git` URL, as
`https://dev.azure.com/{org}/{project}/_git/{repo}`.

**`azure-devops-wiki`** means an Azure DevOps wiki publishes it. Write the wiki's own URL, as
`https://dev.azure.com/{org}/{project}/_wiki/wikis/{wiki}`, and nothing after it. Once you open a page, the address bar
shows a numeric page id. `export` refuses a base with one in it and builds no links. A link to a wiki page is not
pinned to a commit, so a person following it reads whatever the wiki has now. The commit travels in the manifest.

**`mkdocs`** is accepted, and nothing addresses it yet, so an export from a corpus on it has no links.

**`none`** means the corpus is not published. Its export has no links and says so in its manifest. It is the one value
that needs no `publishing:` block.

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

Set `path-prefix` where the repository contains more than the corpus. Do not append that folder to `base`: `export`
inserts the commit between `base` and the record's path. Leave the key out where the corpus is the repository.

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

One base covers one host and one organisation. Leave it bare where the estate spans several, and the check stops.

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

**Leave the block out where the backlog belongs to what `publishing:` already names.** [`export`](cli/export.md) then
derives the address by [the table below](#what-export-derives). A corpus published on GitHub states nothing.

**State the block where the backlog and the published form are two places**, as an Azure DevOps project is.

`target` is one of `github`, `azure-devops` or `none`. Write `none` where the corpus has no tracker at all.
[`validate`](cli/validate.md) refuses any other value.

Write the project URL for `azure-devops`, as `https://dev.azure.com/{org}/{project}`. A URL naming a repository or a
wiki inside the project is cut back to the project.

#### `area`

`area` is the area path below that project a new work item is filed under. Write it separated by `/`, the way `base` is
already written, as `kac-it-swdev` or `kac-it-swdev/subteam`. The client that files converts it to the backslashes Azure
Boards uses.

State it where one Azure DevOps project contains several corpora, so each team sees its own work. Leave it out and an
item lands in the project's default area.

`area` applies to `azure-devops` alone, and [`validate`](cli/validate.md) refuses it beside `github` or `none`. A block
stating no `target` is judged by the target [`publishing-target`](#publishing-target) implies. Nothing derives `area`,
and [`new`](cli/new.md) and [`update`](cli/update.md) never write it.

`validate` does not check the path against Azure DevOps. An area that does not exist fails when the item is created.

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
block only where the two targets agree, and otherwise `export` writes no base.

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
has the address.

`target` takes the three values [`tracker`](#tracker) takes, and `area` follows the [`tracker` rule](#area). Write
`none`, or leave the block out, where you know of no tracker.

[`new`](cli/new.md) writes `target` and `base` when the corpus is created. It takes `base` from `--from` where that
named a repository, and from the framework's own repository where it named a folder. It reads `target` from the host,
and writes `none` for a host that neither `gh` nor `az boards` serves. Nothing derives this block.

#### Three addresses, and telling them apart

`tracker.base` is where a problem with one of your records goes. `framework.base` is where a problem with `kac`, the
schema, the template or a skill goes. Each corpus in [`consumes:`](#consumes) brings a third, which is its producer's.
[The export format](../reference/export.md#id-and-the-trackers-it-compares) says how a reader tells them apart when one
team writes the same address twice.

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

`update` reads `url` when no `--from` is passed. Leave `path` out where the manifest sits at the repository root.

`update` follows `ref`. It writes `commit` for your information and never reads it. A template read from a folder
resolves no commit, so `commit` is left as it stands, and [`new`](cli/new.md) leaves `ref` beside it bare.

A corpus has one `upstream`, which it receives as files and keeps. It may have any number of [`consumes`](#consumes)
entries, whose records it reads and never commits. `upstream.url` says where the template was copied from, and
[`framework.base`](#framework) says where to report a problem with it.

### `update-policy`

How far an update goes. One of `cautious` or `full`, and `cautious` is the default.

```yaml
update-policy: cautious
```

| Value      | Seed files (a type's root page, its `_template.md`, the agent guidance)     |
|------------|-----------------------------------------------------------------------------|
| `cautious` | written only where the corpus has none                                      |
| `full`     | refreshed as well, and you reconcile the result from the diff               |

`update` writes overlay files under either value. [Layers](layers.md) says which files are which.

## `types`

The types this corpus adopted.

```yaml
types:
  - adrs
  - policies
```

Validation, index generation and what an update writes all cover the types listed here and no others.

[`new`](cli/new.md) writes the list. Omit the key and `kac` reads adoption off the folders it finds, counting a type
where both its page and its folder are there.

Once the key is declared, [`validate`](cli/validate.md) reports a type in the list that has no page or folder, and a
type with both that is not in the list.

Use [`update --add-type`](cli/update.md) and `update --drop-type` to change the list. Each moves the name and the type's
files together.

## `enums`

The ranges the schema leaves to the corpus.

```yaml
enums:
  component-type: [asset, cli, website]
  platform: [dotnet, static]
```

A field declaring `values: $corpus.<name>` in `.schema/` takes its range from here. `.schema/services.yaml` declares
`platform` and `component-type` that way. Write each range here, and [`validate`](cli/validate.md) checks every record
against it. The `services` type page says how to derive the two lists from your own deployables.

**Write each value in lower case.** `enum-lowercase` refuses a value such as `Dotnet-Web` in a record, so no record can
satisfy a range containing one.

[`new`](cli/new.md) writes the key with nothing under it. Write a record with such a field before you write the range,
and `validate` reports `corpus-enum-undeclared` once, against `.corpus.yaml`. A required field is asked from the first
record of its type, and an optional one from the first record that states it.

A value the range does not admit is an ordinary `enum` failure, reported against the record, and the message quotes
the range.

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

[`restore`](cli/restore.md) fetches each entry and unpacks it under `.imports/<shortcode>/`, which is not committed.
`resolved` is the lock, so there is no lock file. A `source` is a registry's service index, or a path to a folder
holding what a producer packed. [`restore`](cli/restore.md) says how a range resolves and what a run refuses.

Leave the key out where this corpus stands on its own.

## `export`

What the export leaves behind.

```yaml
export:
  exclude: [ ]
```

It is empty by default, so a record in draft travels, and so does one whose review date has passed. Each states its own
status. A consumer sees no sign of a record the list withheld.

| Value     | Drops a record                       |
|-----------|--------------------------------------|
| `draft`   | whose `status` is `draft`            |
| `overdue` | whose `review-by` is in the past     |

Set it where a draft would be read as an answer, as a `fix` in draft would be by `fix-lookup`.

## `plugin`

Where [`bundle`](cli/bundle.md) reads the plugin tree from, meaning the skills and hooks an agent installs.

```yaml
plugin:
  from: ../../shared/.plugin
```

Omit the key and `bundle` reads `.plugin/` at the corpus root, which is what [`new`](cli/new.md) creates. Set it and
`bundle` reads one tree elsewhere, resolved against the corpus root, so several corpora in a repository share one copy.
`update` then withholds the plugin files it would otherwise write here.

The manifest, `.plugin/.claude-plugin/plugin.json`, stays in each corpus and is never read from the shared tree. A file
a corpus writes under its own `.plugin/` wins over the shared tree's copy of the same path.

## `skip`

The files this corpus owns.

```yaml
skip:
  - path: .plugin/hooks/breadcrumb
    reason: Patched for our proxy.
```

`update` neither reads nor writes a path listed here, and reports each file it stepped over. Each entry gives a path
and a reason, for whoever opens the file next. Delete an entry once the file matches the framework again.
[Layers](layers.md) says which files `update` would otherwise write.

[The export format](../reference/export.md) is the page for what a consumer reads out of a corpus.
