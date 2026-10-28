# Getting started

## Before you start

`kac` is a dotnet tool, so it needs a **.NET 10 SDK**. Any release of it runs `kac`. Check what you have:

```bash
dotnet --list-sdks
```

You want a line opening `10.`, whatever follows it:

```text
10.0.113 [/usr/lib/dotnet/sdk]
```

If there is none, install one from
[dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0). An older SDK beside it
is fine. On Linux, [Install .NET on Linux](https://learn.microsoft.com/dotnet/core/install/linux) says which package
feed has the SDK for your distribution. Ubuntu 24.04 has it in Ubuntu's own feed, which also serves Arm:

```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

You also need **git**. `kac` lists a corpus with `git ls-files`, so a corpus outside version control is read a narrower
way. [Discovery](discovery.md) says what changes.

## Install the tool

`kac` is published as the dotnet tool `KnowledgeAsCode.Tool`. Installing it globally puts `kac` on your `PATH`:

```bash
dotnet tool install --global KnowledgeAsCode.Tool
kac --version
```

`--version` prints the release and the commit it was built from:

```text
0.41.0+c1c9621bc64317da9642f942d804d7dd22e41644
```

If the shell cannot find `kac` after a global install, add `~/.dotnet/tools` to your `PATH` and open a new shell.
[Troubleshooting](troubleshooting.md) covers that and the other first-run faults.

## Start a corpus

`kac new` turns the folder you are standing in into a **corpus**: one repository of knowledge records kept in git. It
takes the framework from the knowledge-as-code repository, and writes the files that framework says a corpus receives.
It also writes the files no template can supply: `.corpus.yaml`, which names your corpus, `.config/dotnet-tools.json`,
which pins this `kac`, and a `README.md` to rewrite.

```bash
mkdir my-corpus && cd my-corpus
kac new
```

It asks what the corpus is called, which types it adopts, where it publishes, what builds it and who owns the records it
seeds, with a default for each. The owner defaults to `human:` and the part of your git email before the `@`. Name a
publishing target and it asks one more: the URL a person opens to browse the corpus. Answer nothing at all and you still
end with a corpus that validates, holding every type the framework declares. `--yes` takes every default and asks
nothing, which is what a pipeline runs.

The folder does not have to be a repository yet. `new` offers to run `git init` where there is none. It finishes by
running `generate`, then `validate`, then `git add -A`:

```text
new: did not write .github/dependabot.yml, .github/workflows/kac.yml, azure-pipelines.yml: this corpus is built by none.
new: wrote 124 file(s) for my-corpus, taken from https://github.com/paul80nd/knowledge-as-code at c1c9621.
new: wrote human:alex.doe as the owner of adrs/0001-knowledge-as-code.md, policies/devi-deviations.md.
wrote README.md
updated 1 of 40 generated file(s).
validated 3 document(s) and 18 template(s), skipped 0 without frontmatter. 0 error(s), 0 warning(s)
validate: skipped immutable-after-accepted, clause-id-kept, immutable-after-published, rule-id-kept and retire-not-delete, which compare each record with an earlier revision. Name one with `--since <ref>` to run them.
new: staged. `git status` shows everything this wrote, and the first commit is yours.
new: this corpus has no tracker to file on, so raise-finding and request-deviation will print each issue for you to file. Add a `tracker:` block to .corpus.yaml to set one.
```

The first commit is yours to make, once you have read what is staged.

!!! note "Tell git who you are before the first commit"

    On a machine where git has never committed, `git commit` stops with `Author identity unknown`. Set your name and
    email once:

    ```bash
    git config --global user.name "Your Name"
    git config --global user.email "you@example.com"
    ```

```bash
git commit -m "Start a corpus"
```

!!! note "Look at `.corpus.yaml` before you commit"

    `new` writes it from your answers. [The corpus descriptor](corpus-descriptor.md) says what every key in it means,
    and which ones you move by hand afterwards.

You arrive with ignore rules, editor conventions and a wiki ordering. Name a CI system and its starter pipeline comes
too. `new` writes that one system's and no other, so a corpus is never handed a workflow for a host it does not build
on. [`new`](cli/new.md) covers every flag, the order it asks in, and what stops it.

If you declined some types, the run ends by naming links the type pages make to types you did not take. Those pages are
yours from here, so edit the links out.

## Run the tool against your corpus

`kac` finds a corpus by walking up from the working directory looking for `.corpus.yaml`. Run it from inside your
corpus.

```bash
cd path/to/your/corpus

kac validate            # frontmatter, links, structure, clauses and the graph
kac generate            # rewrite the indexes and the tables inside the markers
```

A clean `validate` prints the counts and exits `0`. Its second line names the checks that compare each record with an
earlier revision, which run only when you name one with `--since`:

```text
validated 13 document(s) and 8 template(s), skipped 0 without frontmatter. 0 error(s), 0 warning(s)
validate: skipped immutable-after-accepted and retire-not-delete, which compare each record with an earlier revision. Name one with `--since <ref>` to run them.
```

A `generate` with nothing to do says so:

```text
generated files already up to date; nothing written.
```

Anything else is a finding naming the file, the check and the line. [Troubleshooting](troubleshooting.md) covers the
ones you meet first.

Two more commands write your corpus out for an agent to read: `kac export` and `kac bundle`. Together they build a
plugin, which is your corpus packaged for Claude Code.

!!! note "Set a tracker before you publish a plugin"

    The plugin's `raise-finding` and `request-deviation` skills file an issue about your corpus on its tracker. A corpus
    published to `mkdocs` or `none` has no tracker, so they print the issue for you to file instead. Add a
    [`tracker`](corpus-descriptor.md#tracker) block to `.corpus.yaml` naming where your issues go:

    ```yaml
    tracker:
      target: github
      base: https://github.com/your-org/your-corpus
    ```

    `kac bundle` warns while the plugin ships either skill and the corpus has no tracker.

* [Installing the plugin you built](cli/bundle.md#an-install) takes it into Claude Code on your own machine.
* [Building the plugin](ci.md#building-the-plugin) runs both commands in CI once a record of yours passes.
* [Installing a corpus plugin](installing-a-plugin.md) says how your readers install it and take each new version.

Every command takes the same few options, and each answers with one of three exit codes.
[The CLI reference](cli/index.md) covers both, and gives a page to every command.

## Add your first record

Your corpus arrives with two records that are its own, each with you as its `owner`. Accept each one, rewrite it, or
delete it:

* **`adr-0001`**, under `adrs/`, records the decision to keep knowledge as code. It arrives `proposed`, because accepting
  it is your decision. If your corpus consumes a parent corpus, delete it: the parent already records that decision.
  [Imports](imports.md) says how one corpus consumes another.
* **`pol-DEVI`**, under `policies/`, is the policy the deviation mechanism rests on: how a team records a knowing
  departure from a rule, who accepts the risk, and when it is reviewed. It arrives `draft`.

The glossary under `glossary/` is the framework's own vocabulary, inherited word for word. Write your own glossaries
beside it and leave that one as it is.

1. **Pick the type.** `knowledge-as-code/taxonomy.md` in your own corpus has a decision table saying where a record
   goes, covering the types that corpus adopted. [The default types](../framework/types.md) introduces every one of
   them.
2. **Copy that type's `_template.md`** to a new file in the same folder. It marks what you supply as `{{placeholder}}`
   and fences its own guidance between `DELETE FROM HERE` and `DELETE TO HERE` comments. A finished record has neither
   left in it.
3. **Run `kac validate`, then `kac generate`.** The first names what is still missing. The second writes your record
   into the folder's index.

[Metadata](../framework/metadata.md) says how an id is formed and how a citation reaches a part of a record.
[Running it in CI](ci.md) is what to read once a record of your own passes locally.
