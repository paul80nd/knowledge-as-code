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
      - uses: actions/checkout@v7

      - uses: actions/setup-dotnet@v6
        with:
          dotnet-version: 10.0.x

      - name: Install the pinned kac
        run: dotnet tool restore

      - name: Validate frontmatter, links and structure
        run: dotnet tool run kac validate

      - name: Check generated output is fresh
        run: dotnet tool run kac generate --check
```

A tag is mutable, so pin each action to a commit where your organisation asks for it. This repository's own workflows
pin that way and let Dependabot move them.

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

`.imports/` is not committed, so a fresh checkout holds none of it. Run [`restore`](cli/restore.md) as the first step of
the job, exactly as a build restores packages.

A private feed needs a token, which the job reads from `KAC_REGISTRY_TOKEN`. In GitHub Actions the built-in
`github.token` reaches GitHub Packages in the same organisation, and the job needs `packages: read`.

A `source:` naming a folder rather than a registry needs no token, and the producer has to have packed into that folder
before this step runs. Two corpora in one repository take that form: build the producer's package first, in its own
directory, and the consumer reads it from the path its descriptor names.

**Restore before you validate, not after.** A declared import that is not on disk fails [`validate`](cli/validate.md)
naming this command. A corpus that consumes nothing skips the step, and standing alone is the ordinary case.

## Building the plugin

A corpus that publishes an agent plugin runs two more commands, in this order:

```bash
dotnet tool run kac export      # the corpus as data, into .dist/export/
dotnet tool run kac bundle      # that export plus .plugin/, into .dist/plugin/
```

Each replaces its own directory under `.dist/` and leaves the other alone, so a `.gitignore` listing `.dist/` keeps both
out of the tree. Running them in CI proves the corpus still exports and still assembles. It publishes nothing. Pushing
the result anywhere is a separate job, and one that needs credentials this one should not have.

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

[`pack`](cli/pack.md) writes a `.nupkg`, which is a zip a registry stores under the corpus name and its
`content-version`. Both GitHub Packages and Azure DevOps Artifacts take one. Nothing that reads the result needs a NuGet
client.

Run `pack` in the gate, where it proves the corpus can still be packaged. Push it from a separate job, because that job
needs a credential the gate should not hold.

A published version is never replaced, so pushing one the registry already holds fails rather than overwriting it. That
failure means a `content-version` somebody forgot to bump:

```bash
VERSION=$(jq -r '.contentVersion' .dist/export/manifest.json)
ID=$(jq -r '.corpus' .dist/export/manifest.json)

dotnet nuget push ".dist/package/$ID.$VERSION.nupkg" \
  --source "https://nuget.pkg.github.com/OWNER/index.json" \
  --api-key "$GITHUB_TOKEN"
```

The job needs `packages: write` in GitHub Actions. That block pushes and asks the registry nothing, so a re-run of a
corpus whose version has not moved fails on the push.
[`publish-corpus.yml`](https://github.com/paul80nd/knowledge-as-code/blob/main/.github/workflows/publish-corpus.yml)
is the whole pipeline this repository publishes from. It asks the registry what it holds first and skips a version
already there, which is what keeps that re-run green.

## What the checks ask

Every check reads the corpus, and none reads the estate the corpus describes. A service deleted last month still
validates cleanly, so a green build says the corpus is consistent, not that it is right. `kac checks` lists every check
the validator can report against the corpus it runs in.

### What validation asks

Validation asks four questions. `kac checks` prints the checks themselves, and [Checks](../reference/checks.md) says
where each one comes from.

* **Does the record parse and declare itself?** Frontmatter is YAML. Required fields are present. Enum values are ones
  the type declares, and dates are quoted. An `id` is unique across the corpus, and it agrees with both its type's
  prefix and the folder it sits in. A document claiming a tier its folder does not have is reported as a placement
  error.
* **Do the references resolve?** Every id a cross-reference states resolves to a document that exists, and every
  relative link resolves. A reciprocal pair agrees in both directions, so a one-sided link fails. `implements` is not
  reciprocal: it points up from a standard to a policy clause, and a clause lists nothing that implements it.
* **Does the record do what its tier asks?** A tier says how far a record may be trusted, and how it must be written.
  A standard cites what it derives from. A procedural record states when it was last rehearsed.
* **Is it safe to publish?** Nothing in a corpus may read as a credential or as real data. The rule is declared on the
  types where that risk sits.

A type may declare a rule with a description and no severity. `validate` skips it, and the type's own page lists it
under **Declared, not yet enforced**. The schema refuses a severity with nothing behind it.

### What generation protects

Generated content sits between markers inside otherwise hand-written files.
[Generation](../reference/generation.md#the-region-between-the-markers) says what each block contains. `validate` checks
that both markers of every block are still there.

`kac` generates only the types a corpus adopted, so every generated list points at pages that corpus has.
[`generate`](cli/generate.md) says which blocks exist and what each one is built from. A pipeline never commits what it
generates. [Contributing](../framework/contributing.md#what-a-pipeline-will-not-do) says why.

### What is not a record

No schema is applied to these:

* the framework's own documents
* the scaffolding folders
* a type's `_template.md`
* a corpus's root pages

Each path is listed one by one. The `_` prefix is the one glob, and `kac` matches it wherever it appears in a path.

A file excluded as a record is still checked in other ways. `validate` resolves the links in the framework's own
documents like any page's, and checks a template as a template. [`validate`](cli/validate.md) lists each of those
passes.

### What the corpus decides

`types:` in [`.corpus.yaml`](corpus-descriptor.md) states the knowledge types a corpus has adopted, and it bounds both
validation and generation. `skip:` states each file the corpus keeps differently on purpose.
[`update`](cli/update.md) reads both, and checks a copy of the framework against what that corpus declared.
