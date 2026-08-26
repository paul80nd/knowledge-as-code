# Running it in CI

Two commands hold a corpus to its schema on every pull request, so a broken cross-reference fails CI. A corpus is one
repository of knowledge records kept in git, and a record is one Markdown document in it, filed under a type. Both
commands run from inside the corpus.

| Command                | Fails when                                                                  |
|------------------------|-----------------------------------------------------------------------------|
| `kac validate`         | a record breaks a check the schema declares                                 |
| `kac generate --check` | a generated file no longer matches the records and schema it was built from |

[Checks](design/checks.md) is the page for adding a check. A corpus that ships an agent plugin adds
[`export`](cli/export.md) and [`bundle`](cli/bundle.md) beside the two, so a change that breaks the export or the bundle
fails the pull request. [Automation](framework/automation.md) says what the checks are for.

## Pin the tool first

Run the version the corpus was written against. Put `kac` in a tool manifest and commit it. Do this once, on your own
machine:

```bash
dotnet new tool-manifest
dotnet tool install KnowledgeAsCode.Tool
```

That writes `.config/dotnet-tools.json` and names the version in it. CI restores from that file, so every
machine runs the same `kac`.

```bash
dotnet tool restore
dotnet tool run kac validate
```

## CI never commits, and checks out with git

**CI never commits.** `generate --check` recomputes every generated file, names the ones that differ, and exits `1`. It
writes nothing. Give the job read-only permission. Run `kac generate` on your own machine and commit what it writes.

```text
generated files are stale. These differ from the schema/frontmatter:
  glossary/_index.md
run:  kac generate
```

**Check out with git, not a tarball.** `kac` lists a corpus with `git ls-files`, so `.gitignore` and the other exclude
files are honoured. A working tree with no `.git/` falls back to a directory walk, which honours none of them. Every
standard checkout action is fine. A downloaded archive is not.

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

## Building the plugin

A corpus that publishes an agent plugin runs two more commands, in this order:

```bash
dotnet tool run kac export      # the corpus as data, into .dist/export/
dotnet tool run kac bundle      # that export plus .plugin/, into .dist/plugin/
```

Each replaces its own directory under `.dist/` and leaves the other alone, so a `.gitignore` holding `.dist/` keeps both
out of the tree. Running them in CI proves the corpus still exports and still assembles. It publishes nothing:
pushing the result anywhere is a separate job, and one that needs credentials this one should not have.

[`bundle`](cli/bundle.md) validates nothing it assembles, so validate both the plugin and the marketplace above it. The
Claude Code CLI has to be on the runner first:

```bash
npm install -g @anthropic-ai/claude-code
claude plugin validate ./.dist/plugin --strict
claude plugin validate ./.dist --strict
```
