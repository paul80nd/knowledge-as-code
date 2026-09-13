# `restore` fetch the corpora this one consumes

<!-- BEGIN GENERATED: usage-restore -->

```text
kac restore [--no-color]
```

| Option       | What it does                                                |
|--------------|-------------------------------------------------------------|
| `--no-color` | Turn colour off. NO_COLOR in the environment does the same. |

<!-- END GENERATED: usage-restore -->

## What it does

`restore` reads the `consumes:` block of [`.corpus.yaml`](../corpus-descriptor.md). It fetches each corpus (one
repository of knowledge records kept in git) named there from the source that entry declares, and unpacks it under
`.imports/<shortcode>/`.

Your corpus can then cite another corpus's records without keeping a copy of them. One organisation often puts its
policies in one corpus and each team's knowledge in another.

What `restore` fetches is the package [`pack`](pack.md) builds, so you take a released version. You do not get whatever
is on the producer's default branch today. [Imports](../design/imports.md) describes the whole round trip, from the
producer's export to the citation your build resolves.

**Do not commit `.imports/`.** It contains another corpus's whole content, and a copy in your history is a second place
that content lives. The `.gitignore` a corpus arrives with already excludes it, and CI restores before it validates, the
way a build restores packages.

**A corpus that consumes nothing needs none of this.** Standing alone is the ordinary case. `restore` prints
`this corpus consumes nothing, so there is nothing to restore.` and exits `0`.

### The `consumes:` block

Each entry declares one corpus. You write the first four keys, and `restore` writes the fifth.

```yaml
consumes:
  - corpus: example-engineering
    shortcode: eng
    version: ^0.16.0
    source: https://nuget.pkg.github.com/OWNER/index.json
    resolved: "0.16.0"
```

| Key         | What it is                                                                                                |
|-------------|-----------------------------------------------------------------------------------------------------------|
| `corpus`    | The name the producer publishes under.                                                                    |
| `shortcode` | The word you cite it by, as the `eng` in `eng:pol-VURM.TIMEBOX`. The producer owns its spelling.          |
| `version`   | The range you mean. Write `1.2.0` for one version, or `^1.2.0` for the newest with the same meaning.      |
| `source`    | Where the package is fetched from: a registry's service index, or a folder holding what a producer built. |
| `resolved`  | What the last restore took. `restore` writes this line.                                                   |

`^0.16.0` admits the same major version. Below `1.0.0` it admits the same minor version instead, because a `0.x` major
promises nothing.

The range says what you meant. The lock says what your build used. Both sit on one entry, so there is no second file to
keep in step.

#### A folder source

`source` can also be a folder, which is what [`pack`](pack.md) fills at `.dist/package/`. A path is relative to the
corpus that declares it, the same way `upstream.url` is:

```yaml
consumes:
  - corpus: example-engineering
    shortcode: eng
    version: ^0.16.0
    source: ../engineering/.dist/package
```

Nothing else changes. The range resolves against the versions in the folder, and what arrives is the same package a
registry would have served. So a corpus consuming a sibling in its own repository needs no registry, no token and no
release.

Pack the producer first. Run `kac export` and then `kac pack` in that corpus, and the folder contains a package to take.

**A folder contains one version. A registry keeps every version it ever accepted.** [`pack`](pack.md) rebuilds its
output directory whole, so the folder contains whatever was packed last. `restore` refuses a `resolved:` that names any
other version, and reports that the folder has no package for it. Move the lock with the range when the producer's
`content-version` moves, or delete `resolved:` and let the next restore write it.

#### A private feed

A registry serving a private feed refuses an anonymous read. GitHub Packages is one. Put a token in the environment,
where nothing echoes it into a log:

```bash
export KAC_REGISTRY_TOKEN="$GITHUB_TOKEN"
kac restore
```

One token covers every entry. `kac` sends it to each `https://` source your descriptor declares, and never over plain
HTTP. Consume from two registries and each one sees it, so declare a source you are willing to hand the token to.

## Examples

### A first restore

```bash
kac restore
```

`restore` reports each corpus with the version it came in at and the folder it was unpacked into:

```text
wrote .imports/eng
restore: example-engineering 0.16.0 as 'eng:'.
restore: 1 fetched, 0 already current. 1 resolved version(s) written to .corpus.yaml, and .imports/ is not committed.
```

### A repeat run

Nothing is fetched twice. If `.imports/<shortcode>/` already contains the resolved version, `restore` skips it:

```text
restore: example-engineering 0.16.0 as 'eng:'. Already current.
restore: 0 fetched, 1 already current. 1 resolved version(s) written to .corpus.yaml, and .imports/ is not committed.
```

Where the lock still satisfies the range, `kac` never asks the registry. Two restores of an unchanged `.corpus.yaml`
write the same bytes, which is what makes a validation run reproducible.

### A move to a newer version

Edit `version`. The entry re-resolves on the next run, because its lock no longer satisfies the range:

```yaml
version: ^0.17.0
```

`restore` rewrites `resolved` to whatever the range came to. Commit that line with the change to the range. It is what
your pipeline will take.

### A refusal

A refusal reports the entry and exits `1`. Nothing is written.

A package whose stamped shortcode disagrees with your declaration is one:

```text
restore: the package for 'example-engineering' is cited as 'eng:' by its own manifest, and this corpus declares it as 'gov:'. The producer owns the spelling, so change the declaration to match it.
```

A range no published version satisfies is another, and it lists the versions the registry does have:

```text
restore: 'example-engineering' has no version matching '^2.0.0' at https://nuget.pkg.github.com/OWNER/index.json. It holds 0.16.0.
```

Two entries claiming one shortcode are refused together, because they would restore into one folder. Two entries
consuming one corpus are refused too, because there is one entry to write the resolved version onto.

## Known limits

**It does not tell you a newer version exists.** `restore` takes a lock the range still admits without asking the
registry. Whether the producer has released since is a different question, and this command never asks it.

**A folder source cannot serve a version it no longer contains.** A registry keeps every version published to it. A
folder contains the last one packed, so `restore` refuses a consumer locked to an earlier one.

**It refuses everything or fetches everything.** A run that fetched two corpora and then refused the third would leave
`.imports/` describing a graph your descriptor does not, so a problem anywhere stops the whole run.

**A range is `1.2.0` or `^1.2.0`, and nothing else.** `restore` refuses a comparator, a union or a wildcard where it is
written, instead of quietly matching nothing. It refuses a caret over a prerelease too, because no caret takes one. Name
`0.2.0-rc.1` exactly to opt in.

**A package unpacks to 256MB at most, and a single entry to 16MB.** `restore` refuses a corpus past either cap. Both
caps count the bytes actually read, not the size an entry declares, because that size is the package's own claim about
itself.

**It reports nothing about a corpus that has not restored.** Whether the imports a corpus declares are on disk is
[`validate`](validate.md)'s question. `validate` fails and tells you to run this command where one is not.

[The corpus descriptor](../corpus-descriptor.md) is the page for the rest of `.corpus.yaml`. That includes the
`upstream:` block, which is a different relationship. `upstream:` is one framework you receive as files. `consumes:`
is a graph of records you read.
