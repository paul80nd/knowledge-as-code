# Running it in CI

Two commands check a corpus against its schema on every pull request, so a broken cross-reference fails CI. A corpus is
one repository of knowledge records kept in git, and a record is one Markdown document in it, filed under a type. Every
command here runs from inside the corpus.

A corpus that ships an agent plugin runs two more, so a change that breaks either one fails the pull request too. The
plugin is the corpus packaged so an AI agent can install it, and the export is the corpus written out as data for that
packaging to read.

| Command                | Fails when                                                                  |
|------------------------|-----------------------------------------------------------------------------|
| `kac validate`         | a record breaks a check the schema declares                                 |
| `kac generate --check` | a generated file no longer matches the records and schema it was built from |
| `kac export`           | the corpus can no longer be written out as data                             |
| `kac bundle`           | that export can no longer be assembled into a plugin                        |

An error fails the build. A warning and an info are printed and leave the exit code alone.
[What the checks ask](#what-the-checks-ask) says what they cover.

## Pin the tool first

Run the version the corpus was written against. Put `kac` in a tool manifest and commit it. Do this once, on your own
machine:

```bash
dotnet new tool-manifest
dotnet tool install KnowledgeAsCode.Tool
```

That writes `.config/dotnet-tools.json` with the version in it. CI restores from that file, so every machine runs the
same `kac`.

```bash
dotnet tool restore
dotnet tool run kac validate
```

## Read-only permission

**CI never commits.** `generate --check` recomputes every generated file, lists the ones that differ, and exits `1`. It
writes nothing. Give the job read-only permission. Run `kac generate` on your own machine and commit what it writes.

```text
generated files are stale. These differ from the schema/frontmatter:
  glossary/_index.md
run:  kac generate
```

## Checking out with git

Check out with git, not a tarball. `kac` lists a corpus with `git ls-files`, so `.gitignore` and the other exclude files
count. A working tree with no `.git/` falls back to a directory walk, which obeys none of them. Every standard checkout
action is fine. A downloaded archive is not.

## GitHub Actions

```yaml
name: kac

on:
  pull_request:
    branches:
      - main

# Nothing here writes back to the repository.
permissions:
  contents: read

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1
        with:
          persist-credentials: false

      - uses: actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68 # v6.0.0
        with:
          dotnet-version: 10.0.x

      - name: Install the pinned kac
        run: dotnet tool restore

      - name: Validate frontmatter, links and structure
        run: dotnet tool run kac validate

      - name: Check generated output is fresh
        run: dotnet tool run kac generate --check
```

Each action is pinned to a commit, because a tag can be moved to other code. A `.github/dependabot.yml` that tracks
`github-actions` moves each pin and the version comment beside it. The one `kac new` writes waits seven days after a
release before it proposes it, so a compromised release has time to be withdrawn. The checkout sets
`persist-credentials: false`, so no later step can push with the job's token.

## Azure Pipelines

```yaml
trigger: none      # PR validation comes from an Azure Repos branch policy

pr:
  branches:
    include:
      - main

pool:
  vmImage: ubuntu-latest

steps:
  - task: UseDotNet@2
    displayName: Install .NET 10 SDK
    inputs:
      packageType: sdk
      version: 10.0.x

  - script: dotnet tool restore
    displayName: Install the pinned kac

  - script: dotnet tool run kac validate
    displayName: Validate frontmatter, links and structure

  - script: dotnet tool run kac generate --check
    displayName: Check generated output is fresh
```

!!! warning "Azure Repos ignores the `pr:` trigger in this file"

    Wire the pipeline up as a branch policy, or it never runs on a pull request and nothing says so.

    Project settings, then Repositories, then your repository, then Policies, then Branch Policies, then `main`, then
    Build Validation, then **+**. Select this pipeline and mark it Required.

The `pr:` block states the intent, and it works as written if the repository is ever mirrored to GitHub.

## Restoring what a corpus consumes

A corpus that reads another corpus's records declares them in `consumes:`, and fetches them before anything is
validated:

```bash
dotnet tool run kac restore     # each consumed corpus, into .imports/<shortcode>/
```

`.imports/` is not committed, so run [`restore`](cli/restore.md) as the first step of the job.

A private feed needs a token, which the job reads from `KAC_REGISTRY_TOKEN`. In GitHub Actions the built-in
`github.token` reaches GitHub Packages in the same organisation, and the job needs `packages: read`.

A `source:` naming a folder needs no token. Run `export` and `pack` in the producer before this step.

**Restore before you validate.** A declared import that is not on disk fails [`validate`](cli/validate.md). A corpus
that consumes nothing skips the step.

## Building the plugin

A corpus that publishes an agent plugin runs two more commands, in this order:

```bash
dotnet tool run kac export      # the corpus as data, into .dist/export/
dotnet tool run kac bundle      # that export plus .plugin/, into .dist/plugin/
```

Each replaces its own directory under `.dist/`, which the corpus's `.gitignore` lists. Running them proves the corpus
still exports and still assembles, and publishes nothing.

[`bundle`](cli/bundle.md) validates nothing it assembles, so validate both the plugin and the marketplace above it. The
Claude Code CLI has to be on the runner first:

```bash
npm install -g @anthropic-ai/claude-code
claude plugin validate ./.dist/plugin --strict
claude plugin validate ./.dist --strict
```

## Publishing the corpus as a package

A corpus another corpus consumes runs one more command, and it reads the same export:

```bash
dotnet tool run kac pack       # the export, sealed into .dist/package/
```

[`pack`](cli/pack.md) writes a `.nupkg`, which GitHub Packages and Azure DevOps Artifacts both store. Run `pack` in
the gate. Push it from a separate job, which has the credential the gate does not.

A registry refuses a version it already has, so a push fails where somebody forgot to move `content-version`:

```bash
VERSION=$(jq -r '.contentVersion' .dist/export/manifest.json)
ID=$(jq -r '.corpus' .dist/export/manifest.json)

dotnet nuget push ".dist/package/$ID.$VERSION.nupkg" \
  --source "https://nuget.pkg.github.com/OWNER/index.json" \
  --api-key "$GITHUB_TOKEN"
```

The job needs `packages: write` in GitHub Actions. A re-run whose version has not moved fails on the push.
[`publish-corpus.yml`](https://github.com/paul80nd/knowledge-as-code/blob/main/.github/workflows/publish-corpus.yml)
asks the registry what it has first, and skips a version already there.

## What the checks ask

Every check reads the corpus, and none reads the estate the corpus describes. A service deleted last month still
validates cleanly, so a green build says the corpus is consistent, not that it is right.

`validate` asks four questions of each record:

* Does it parse, and does it declare itself as its type and folder say?
* Does every reference resolve, and does each reciprocal pair agree in both directions?
* Does it do what its tier asks, such as citing a source or stating when it was last rehearsed?
* Is it safe to publish, with nothing reading as a credential or as real data?

`kac checks` lists every check the validator can report, and [Checks](../reference/checks.md) says where each one comes
from. [Discovery](discovery.md) says which files are records, and `types:` in
[`.corpus.yaml`](corpus-descriptor.md#types) bounds both validation and generation.
