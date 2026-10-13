# Use it

This section is for the `kac` user: whoever installs the tool, runs it over a corpus, wires it into CI, and builds and
installs the plugin. A corpus is one repository of knowledge records kept in git. The plugin is that corpus packaged
for Claude Code.

## The pages here

* **[Getting started](getting-started.md)** installs the tool and runs it against a corpus.
* **[One corpus or several](one-corpus-or-several.md)** helps you decide how many corpora to keep, and how they share
  what they know.
* **[Running it in CI](ci.md)** wires the commands that check a corpus into a pull request, on GitHub Actions or Azure
  Pipelines, and says what those checks ask.
* **[Troubleshooting](troubleshooting.md)** is what `kac` prints when something is wrong, and what to do about it.
* **[The corpus descriptor](corpus-descriptor.md)** covers `.corpus.yaml`, the one file a corpus writes for itself.
* **[Discovery](discovery.md)** says which files `kac` opens, which of those count as records, and which it reads for
  something narrower.
* **[Layers](layers.md)** says which files in a corpus belong to the framework, which belong to the corpus, and what
  happens to each when a newer framework arrives.
* **[Imports](imports.md)** says how one corpus cites records another corpus published.
* **[Skills](skills.md)** says which skills `kac` ships, where each one lives, and who can read it.
* **[CLI reference](cli/index.md)** gives a page to each command, saying what it does, what it refuses and what it
  leaves alone.

## Where to go next

[Getting started](getting-started.md) is the first page to read if you have not installed `kac`.
