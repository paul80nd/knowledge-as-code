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

One organisation often keeps its policies in one corpus, meaning one repository of knowledge records, and its teams'
knowledge in several others. A team's corpus cites those policies without holding them. `restore` is what brings them
in: it reads the `consumes:` block of [`.corpus.yaml`](../corpus-descriptor.md), fetches each corpus named there from
the source it declares, and unpacks it under `.imports/<shortcode>/`.

What it fetches is the package [`pack`](pack.md) seals, so a consumer takes a released version rather than cloning the
producer and reading whatever is on their default branch this afternoon. [Imports](../design/imports.md) sets out the
whole round trip, from the producer's export to the citation your build resolves.

**`.imports/` is not committed.** It holds another corpus's whole content, and a copy in your history is a second place
that content lives. The `.gitignore` a corpus arrives with already covers it, and CI restores before it validates,
exactly as a build restores packages.

**A corpus that consumes nothing needs none of this.** Standing alone stays the ordinary case, and a corpus with no
`consumes:` block never grows one.

### Declaring what you consume

Each entry names one corpus. You write the first four keys, and `restore` writes the fifth.

```yaml
consumes:
  - corpus: example-engineering
    shortcode: eng
    version: ^0.1.0
    source: https://nuget.pkg.github.com/OWNER/index.json
    resolved: "0.1.0"
```

`corpus` is the name the producer publishes under. `shortcode` is the word you cite it by, as the `eng` in
`eng:pol-VURM.TIMEBOX`, and the producer owns its spelling. `source` is where the package is fetched from, and it takes
two forms: a registry's service index, which is the URL the producer pushes to, or a folder holding what a producer
built.

`version` is the range you mean, and it takes one of two forms. `0.1.0` is that version and no other. `^0.1.0` is the
newest version that cannot have changed a meaning since: the same major, or below `1.0.0` the same minor, because a
`0.x` major promises nothing.

`resolved` is what the last restore actually took, and `restore` writes it. The range says what you meant and the lock
says what your build used, so the two live on one entry rather than in a second file.

#### Taking a package from a folder

`source` also names a folder, which is what [`pack`](pack.md) fills at `.dist/package/`. A path is relative to the
corpus declaring it, exactly as `upstream.url` is:

```yaml
consumes:
  - corpus: example-engineering
    shortcode: eng
    version: ^0.1.0
    source: ../engineering/.dist/package
```

Nothing else changes. The range resolves against the versions the folder holds, and what arrives is the same sealed
package a registry would have served. So a corpus consuming a sibling in its own repository needs no registry, no token
and no release.

The producer has to have packed first. Run `kac export` and then `kac pack` in that corpus, and the folder holds a
package to take.

**A folder holds one version, where a registry holds every version it ever accepted.** [`pack`](pack.md) rebuilds its
output directory whole, so the folder holds whatever was packed last and nothing else. A `resolved:` naming any other
version cannot be satisfied there, and the run says the folder holds no package for it. Move the lock with the range
when the producer's `content-version` moves, or delete `resolved:` and let the next restore write it.

#### Reading a private feed

A registry serving a private feed refuses an anonymous read. GitHub Packages is one. Put a token in the environment,
where nothing echoes it into a log:

```bash
export KAC_REGISTRY_TOKEN="$GITHUB_TOKEN"
kac restore
```

One token covers every entry, and it is sent to each `https://` source your descriptor names. It is never sent over
plain HTTP. Consuming from two registries means both see it, so declare a source you are willing to hand the token to.

## Examples

### Restore what you declared

```bash
kac restore
```

Each corpus is named with the version it came in at, and the folder it was unpacked into:

```text
wrote .imports/eng
restore: example-engineering 0.1.0 as 'eng:'.
restore: 1 fetched, 0 already current. 1 resolved version(s) written to .corpus.yaml, and .imports/ is not committed.
```

### Run it again

Nothing is fetched twice. A folder already holding the resolved version is the thing a restore produces, so the run
says so and leaves it alone:

```text
restore: example-engineering 0.1.0 as 'eng:'. Already current.
restore: 0 fetched, 1 already current. 1 resolved version(s) written to .corpus.yaml, and .imports/ is not committed.
```

Where the lock still satisfies the range, the registry is never asked at all. Two restores of an unchanged
`.corpus.yaml` write the same bytes, which is what makes a validation run reproducible.

### Move to a newer version

Edit `version`, and the entry re-resolves on the next run because its lock no longer satisfies the range:

```yaml
version: ^0.2.0
```

Restoring rewrites `resolved` to whatever the range came to. Commit that line with the change to the range: it is what
your pipeline will take.

### Find out why a restore was refused

A refusal names the entry and exits `1`, and nothing is written. A package whose stamped shortcode disagrees with your
declaration is one:

```text
restore: the package for 'example-engineering' is cited as 'eng:' by its own manifest, and this corpus declares it as 'gov:'. The producer owns the spelling, so change the declaration to match it.
```

A range no published version satisfies is another, and it names what the registry does hold:

```text
restore: 'example-engineering' has no version matching '^2.0.0' at https://nuget.pkg.github.com/OWNER/index.json. It holds 0.1.0.
```

Two entries claiming one shortcode are refused together, because both would restore into one folder and the fix is to
change one of them. So are two entries consuming one corpus, because there is one entry to write its resolved version
onto.

## Known limits

**It does not tell you a newer version exists.** A lock the range still admits is taken without asking the registry.
Whether the producer has released since is a different question, and one this command never asks.

**A folder source cannot serve a version it no longer holds.** A registry keeps every version published to it. A
folder holds the last one packed, so a consumer locked to an earlier one is refused rather than served something else.

**It refuses everything or fetches everything.** A run that fetched two corpora and then refused the third would leave
`.imports/` describing a graph your descriptor does not, so a problem anywhere stops the whole run.

**A range says `1.2.0` or `^1.2.0`, and nothing else.** Comparators, unions and wildcards are refused where they are
written rather than quietly matching nothing. So is a caret over a prerelease, because no caret takes one: name
`0.2.0-rc.1` exactly to opt in.

**A package unpacks to 256MB at most, and a single entry to 16MB.** A corpus past either is refused rather than read
into memory. Both caps count the bytes read rather than the size an entry declares, because that size is the package's
own claim about itself.

**It reports nothing about a corpus that has not restored.** Whether the imports a corpus declares are on disk is
[`validate`](validate.md)'s question, and it fails and names this command where one is not.

[The corpus descriptor](../corpus-descriptor.md) is the page for the rest of `.corpus.yaml`, including the `upstream:`
block, which is a different relationship: one framework flowing down to you as files, rather than a graph of records
you read.
