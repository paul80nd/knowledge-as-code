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

Your corpus can then cite another corpus's records without keeping a copy of them. What `restore` fetches is the
package [`pack`](pack.md) builds, so you take a released version. [Imports](../imports.md) describes the round trip,
from the producer's export to the citation your build resolves.

**Do not commit `.imports/`.** The `.gitignore` a corpus arrives with already excludes it, and CI restores before it
validates.

A corpus that consumes nothing prints `this corpus consumes nothing, so there is nothing to restore.` and exits `0`.

### The `consumes:` block

Each entry declares one corpus. [`consumes`](../corpus-descriptor.md#consumes) gives the five keys. You write the first
four, and `restore` writes `resolved`.

```yaml
consumes:
  - corpus: example-engineering
    shortcode: eng
    version: ^0.16.0
    source: https://nuget.pkg.github.com/OWNER/index.json
    resolved: "0.16.0"
```

`version` is `1.2.0` for one version, or `^1.2.0` for the newest with the same major version. Below `1.0.0` it admits
the same minor version instead, so `^0.16.0` never takes `0.17.0`. The producer owns the spelling of `shortcode`.

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

The range resolves against the versions in the folder, so a corpus consuming a sibling in its own repository needs no
registry, no token and no release. Run `kac export` and then `kac pack` in the producer first.

#### A private feed

A registry serving a private feed refuses an anonymous read. GitHub Packages and Azure Artifacts are two. Put a token
in the environment, where nothing echoes it into a log. In a pipeline, pass the job's own token, such as
`github.token` or `$(System.AccessToken)`, and `kac` sends it as a bearer token:

```bash
export KAC_REGISTRY_TOKEN="$GITHUB_TOKEN"
kac restore
```

On your own machine with Azure Artifacts, start with a Microsoft Entra token, because there is no token to create or
rotate. Sign in with `az login`, then ask the Azure CLI for a token to Azure DevOps. Leave `KAC_REGISTRY_USER` unset,
so `kac` sends it as a bearer token:

```bash
unset KAC_REGISTRY_USER                                   # a name would send the token as Basic auth
export KAC_REGISTRY_TOKEN=$(az account get-access-token \
  --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv)
kac restore
```

The token lasts about an hour, so set it again in each session and do not store it.
[Issue Entra tokens with Azure CLI](https://learn.microsoft.com/azure/devops/cli/entra-tokens) gives the resource id
and the lifetime.

Otherwise, use a personal access token, and set `KAC_REGISTRY_USER` to any name as well. `kac` then sends the name and
the token as Basic auth, which is how Azure Artifacts takes a personal access token. There, the token needs the
**Packaging (Read)** scope:

```bash
export KAC_REGISTRY_USER=kac                              # any name
read -rs KAC_REGISTRY_TOKEN && export KAC_REGISTRY_TOKEN  # paste the token, which is not echoed
kac restore
```

One token covers every entry. `kac` sends it to each `https://` source your descriptor declares, and never over plain
HTTP, so declare only a source you are willing to hand the token to.

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

Where the lock still satisfies the range, `kac` never asks the registry, so two restores of an unchanged `.corpus.yaml`
write the same bytes.

### A move to a newer version

Edit `version`. The entry re-resolves on the next run, because its lock no longer satisfies the range:

```yaml
version: ^0.17.0
```

`restore` rewrites `resolved` to whatever the range came to. Commit that line with the change to the range.

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

Two entries claiming one shortcode are refused, and so are two entries consuming one corpus.

## Known limits

**It does not tell you a newer version exists.** `restore` takes a lock the range still admits without asking the
registry.

**A folder source cannot serve a version it no longer contains.** [`pack`](pack.md) rebuilds its output directory whole.
A folder contains the last one packed, so `restore` refuses a consumer locked to an earlier one. Move the lock
with the range when the producer's `content-version` moves, or delete `resolved:` and restore again.

**It refuses everything or fetches everything.** A problem with any entry stops the whole run.

**A range is `1.2.0` or `^1.2.0`, and nothing else.** `restore` refuses a comparator, a union or a wildcard. It refuses
a caret over a prerelease too. Name `0.2.0-rc.1` exactly to opt in.

**A package unpacks to 256MB at most, and a single entry to 16MB.** `restore` refuses a corpus past either cap, counting
the bytes actually read.

**It reports nothing about a corpus that has not restored.** [`validate`](validate.md) fails where a declared import is
not on disk, and tells you to run this command.

[Imports](../imports.md) says how a citation reaches a record another corpus keeps.
