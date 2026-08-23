# `.corpus.yaml`: what a corpus says about itself

## What it is for

`.corpus.yaml` sits at the root of a corpus and says what that corpus is, and where it stands against the shared
framework. Everything else a corpus holds arrives from a template and can be taken again. This file is the corpus's
own, so nothing syncs it and nothing reconciles it.

You write it by hand. [`mechanism --sync`](cli/mechanism.md) stamps four of its keys, and it is the only command that
touches the file at all.
[`example/.corpus.yaml`](https://github.com/paul80nd/knowledge-as-code/blob/main/example/.corpus.yaml) is commented
throughout, and it is the copy to keep open while you write your own.

## What it is not

**It is not the schema.** `.schema/` says what a record of each type must carry. This file says which of those types
the corpus adopted, and says nothing about their shape.

**It is not configuration for a run.** No flag on any command overrides what you write here.

## Three versions live here

None of them is called `version` alone, because each answers a different question.

| Key                          | Answers                                 | Moved by           |
|------------------------------|-----------------------------------------|--------------------|
| `descriptor-version`         | the format of this file                 | `mechanism --sync` |
| `content-version`            | the version of what this corpus *knows* | by hand            |
| `upstream.mechanism-version` | the framework version this corpus runs  | `mechanism --sync` |

`content-version` is semantically versioned: major where a meaning changed or a published URL broke, minor for
additions, patch for wording. Quote it when one corpus tells another which version of the content it holds. Nothing
refuses to load because the number moved. It is a notification.

## Identity

```yaml
corpus: knowledge-as-code
content-version: "0.1.0"
role: source
```

`role` is `source` or `consumer`.

A **source** answers for the framework. Changes are introduced, verified or reconciled there, and then shared with the
corpora that took a copy. A source carries the verification layer: the tests, the fixtures they read, and the guidance
for changing the tool.

A **consumer** takes framework content from a source. It carries the tool and uses it. It holds none of the machinery
that proves the tool, because the tool reaches it already proven.

Any corpus may author a change. Real content is the only thing that reveals a schema is wrong, so the corpus that found
the problem is often the one best placed to fix it. A change is settled once a source accepts it.

The role says what part a corpus plays. It does not set the direction of travel, and a source may itself sync from
another source further upstream.

## Publishing

```yaml
publishing-target: github
publishing:
  human-base: https://github.com/paul80nd/knowledge-as-code/blob
  raw-base: https://raw.githubusercontent.com/paul80nd/knowledge-as-code
  path-prefix: example
```

`publishing-target` is one of `azure-devops-wiki`, `github`, `mkdocs` or `none`. You state it rather than leave it to
be guessed. `mechanism --check` can then name the publishing file a corpus is missing. And [`export`](cli/export.md)
knows whether it can write a link that a reader and an agent could each follow.

`github` says the repository is itself the published form. A person reads a record rendered on github.com, and an agent
fetches its source from raw.githubusercontent.com. It is the only target that builds a link today. A descriptor may
carry `azure-devops-wiki` or `mkdocs`, and nothing yet addresses either, so an export from a corpus on one of those
carries no links. No link is better than a link built on a convention nobody has settled.

`none` says a corpus is not published. Its export carries no links and says so in its manifest, so a reader is never
handed an address that resolves nowhere. It is the one value that needs no `publishing:` block.

Only *where* the corpus is served from lives here. How a record's path and a term's anchor join a base is a property of
the target, and it lives in the mechanism. Every corpus on one target then builds the same link, and none of them
writes the rule down a second time.

`path-prefix` is for a repository holding more than the corpus. You might expect to fold that folder into the two bases
above. The commit a link resolves against sits between a base and the record's path, so there is nowhere to put it.
Leave the key out where the corpus is the repository, which is the ordinary case.

## Upstream

```yaml
upstream:
  url: https://github.com/paul80nd/knowledge-as-code
  mechanism-version: 6
  synced-from: ../knowledge-as-code
  synced-on: "2026-08-08"
```

Where this corpus takes the framework from, if anywhere. `url` says it takes from an upstream at all, and
`mechanism --check` uses it as the default for `--against`. A `--sync` refuses to run without it. The corpus at the
head of the chain names none, so a sync there would have nowhere to run from.

`synced-from` and `synced-on` record the last sync: the reference `--sync` actually read, often a local checkout of the
URL above, and the day it read it. `--sync` writes both, so leave them to it.

## Adopted types

```yaml
types:
  - adrs
  - policies
```

Validation and index generation cover the types listed here and no others. Omit the key and you have not declared yet,
so the tool reads adoption off the folders it finds. A type counts where both halves are there, meaning the page and
the folder.

Declaring turns "these folders happen to be here" into "these are the types we chose", and validation can then hold the
corpus to it. A type you declined is left alone, whatever `.schema/` says about it. Once you have declared, standing a
type up without adopting it is a defect [`validate`](cli/validate.md) reports.

## What an export leaves behind

```yaml
export:
  exclude: []
```

Empty by default, and that is the important part. A record still in draft travels, and so does one whose review date
has passed. Each carries its own state, so a consumer reads what the corpus actually holds and decides for itself how
far to trust it.

Filter here and you make the corpus's own state invisible downstream. Your consumer sees a smaller, tidier vocabulary
and no sign that anything was withheld. The option is there for a corpus publishing to an audience it cannot warn.

Two values are accepted. `draft` drops a record whose status says so, and `overdue` drops one whose `review-by` is in
the past.

## Accepted divergences

```yaml
accepted-divergences:
  - path: tooling/kac/Program.cs
    reason: Pinned to .NET 8 because the local build agent does not provide .NET 10.
    since: 2026-08-03
    revisit: When the build agent image is upgraded.
```

Files in the `synced` layer that you deliberately hold differently. `mechanism --check` reports these as accepted
instead of failing on them, and `--sync` steps over them. Delete an entry once the file matches upstream again.

Each entry is a small promise to your future self, which is what `revisit` is for.
