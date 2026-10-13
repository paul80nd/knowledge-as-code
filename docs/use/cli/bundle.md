# `bundle` assemble the export into an installable agent plugin

<!-- BEGIN GENERATED: usage-bundle -->

```text
kac bundle [--no-color]
```

| Option       | What it does                                                |
|--------------|-------------------------------------------------------------|
| `--no-color` | Turn colour off. NO_COLOR in the environment does the same. |

<!-- END GENERATED: usage-bundle -->

## What it does

`bundle` assembles what [`export`](export.md) wrote, plus the `.plugin/` tree your corpus keeps beside its records, into
a Claude Code plugin directory under `.dist/plugin/`. A plugin is the corpus packaged so an agent can install it.
`bundle` also writes the marketplace that offers it into `.dist/` above, so you can install the result.

What ends up in the plugin depends on what the export carried. A corpus that ships no glossary ships no glossary skill
either.

Run [`export`](export.md) first. `bundle` reads that output, plus the plugin tree and one key of `.corpus.yaml`. It
never loads the corpus itself.

### Where the plugin tree is read from

`bundle` reads `.plugin/` at the corpus root, unless `plugin.from` in [`.corpus.yaml`](../corpus-descriptor.md) points
at another folder. Several corpora in one repository can then share one tree, and [`update`](update.md) withholds the
shared half from every corpus that points at it.

The plugin manifest is always read from the corpus. A file the corpus keeps at the same path as one in the shared tree
wins, so you can override one skill and keep the rest.

### What decides whether a component ships

A component is one skill or hook in the plugin tree. Each declares under `metadata.components` which record types it
reads, as `requires`, and ships only where the export contains every one of them.

* **`requires` reads the export.** An export includes what its corpus consumes, so a corpus with no
  glossary of its own ships the glossary skill where it consumes a corpus that has one.
* **An entry may name a shape, as `glossary@1`.** The component then reads that type's files at shape version 1. A bare
  `glossary` asks only that the type is present. Both are trimmed where the export has no glossary.
* **A trimmed component's files leave with it.** The trim matches the declared path and everything beneath it, on whole
  segments, so `skills/a` does not take `skills/ab`. A file no component claims, such as a README or a licence, ships
  whatever the corpus adopted.
* **A component naming no type supports the lookups, unless it is standalone.** `corpus-retrieval` and
  `request-deviation` ship wherever a component reading a type shipped, and leave with the last of them. The trim
  reports that as `no component it supports survived`. `raise-finding` declares `"standalone": true` and always ships.
* **A run that trims every component warns and still builds.** The empty plugin installs, and `bundle.json` lists each
  component it dropped and the type that component needed.

## Examples

### A built plugin

```bash
kac export
kac bundle
```

`bundle` prints each file as it writes it. The run closes with what shipped, what was trimmed and why, and how to
install it:

```text
wrote .dist/plugin/hooks/breadcrumb
wrote .dist/plugin/hooks/hooks.json
wrote .dist/plugin/skills/corpus-retrieval/SKILL.md
wrote .dist/plugin/skills/glossary-lookup/SKILL.md
wrote .dist/plugin/skills/raise-finding/SKILL.md
wrote .dist/plugin/skills/request-deviation/SKILL.md
bundle: trimmed skills/controls-lookup: the export carries no controls.
bundle: trimmed skills/fix-lookup: the export carries no fixes.
bundle: trimmed skills/policy-lookup: the export carries no policies.
bundle: trimmed skills/process-lookup: the export carries no processes.
bundle: trimmed skills/standards-lookup: the export carries no standards.
bundle: wrote 26 file(s) to .dist/plugin/ as example-libraries 0.5.0. 5 component(s) included, 5 trimmed.
bundle: .dist/ is a marketplace holding it. Install it from a path with:  claude plugin marketplace add ./.dist
```

A trimmed component is not an error. It is a skill whose record type this corpus does not export.

### An install

```bash
claude plugin marketplace add ./.dist
```

Both directories are untracked, so nothing here needs a branch or a credential. What you get on a laptop is exactly what
CI publishes.

### The components that shipped

A corpus whose export includes three record types ships a lookup skill for each, beside the three that name no type of
their own. Adoption is not the test. `example-engineering` adopted `controls` and has no record of one, so that skill is
trimmed alongside the type it never carried. The closing line counts what survived:

```text
bundle: wrote 57 file(s) to .dist/plugin/ as example-engineering 0.19.0. 7 component(s) included, 3 trimmed.
```

`bundle.json` lists them, and it travels inside the plugin:

```bash
jq -c '{kept: [.included[].path], trimmed: [.trimmed[].path]}' .dist/plugin/bundle.json
```

```text
{"kept":["skills/corpus-retrieval","skills/glossary-lookup","skills/raise-finding","skills/request-deviation","skills/policy-lookup","skills/standards-lookup","hooks"],"trimmed":["skills/controls-lookup","skills/fix-lookup","skills/process-lookup"]}
```

To see which assembled skill reads the file one type exports, search for it:

```bash
grep -l clauses.jsonl .dist/plugin/skills/*/SKILL.md
```

```text
.dist/plugin/skills/policy-lookup/SKILL.md
```

[What decides whether a component ships](#what-decides-whether-a-component-ships) says what a component declares.

### A pipeline check

```bash
npm install -g @anthropic-ai/claude-code
claude plugin validate ./.dist/plugin --strict
claude plugin validate ./.dist --strict
```

`bundle` validates nothing it assembles, so this is the layer that does.

### The plugin manifest

`.plugin/.claude-plugin/plugin.json` is where a corpus declares what its plugin ships: `metadata.corpusRoot`, and the
components under `metadata.components`. Who the plugin is comes from [`.corpus.yaml`](../corpus-descriptor.md) instead,
through the export, and `bundle` writes it into the manifest that travels.

| Manifest key                                      | Comes from                   |
|---------------------------------------------------|------------------------------|
| `name`                                            | `corpus`                     |
| `version`                                         | `content-version`            |
| `displayName`, `description`, `license`, `author` | the four keys beside them    |
| `homepage`, `repository`                          | `publishing.base`            |
| `keywords`                                        | the types the export carried |

**`bundle` removes a key the corpus declared nothing for.** It does not leave the key standing. A plugin manifest copied
from a template names somebody, licenses something and points at a repository. Every corpus copying that file would
publish under an identity it never chose.

`author` is the exception, because the format asks for one and `claude plugin validate --strict` fails a manifest with
none. A corpus that named nobody is filed under its own name, which says the corpus wrote its own plugin and says
nothing about a person. [`pack`](pack.md) files a package's `authors` the same way. Nothing asks for `license`, so a
corpus that chose none asserts none.

Every other key survives untouched, including one this tool has never heard of. What a corpus adds to its own manifest
is its own.

## Known limits

**It refuses a run it cannot finish.** Each of these prints a reason, writes nothing and exits `1`:

* a plugin tree with no manifest
* a `plugin.from` naming a folder that is not there
* a manifest that is not JSON, or that states no name, or no `corpusRoot`, or a `corpusRoot` the plugin tree already
  uses
* an export with no manifest
* an export whose `formatVersion` is not the one this build reads
* a component reading a type at a shape version the export does not carry

Where the format versions differ, the message gives both. Rebuild an export that is behind the tool with
[`export`](export.md). An export ahead of the tool means your `kac` is the older of the two. A shape mismatch means the
type is in the export and its files have changed. [The export format](../../reference/export.md) says what a shape
version covers and what moves it.

**It validates nothing it assembles.** A component misplaced inside `.claude-plugin/` leaves here unreported.
`claude plugin validate` runs one layer out, which keeps the build runnable without the Claude Code CLI installed.

**It does not publish.** Pushing the result anywhere is a separate job, and one that needs credentials this one should
not have.

**The hook has been proved on macOS only.** Nothing yet says which shell Claude Code reaches a hook command with on
Windows, so nothing yet says whether the `.cmd` half of the pair is ever the one that runs. The round-trip test installs
the plugin on a Windows runner but opens no session, so it cannot answer this.

**A component's `requires` is not checked against the schema.** `bundle` trims a component that requires a type no
schema declares, with the same message as one that requires a type this corpus declined. One is a typo and the other is
a decision, and nothing reports the first.

**The export is copied whole.** A component that survives the trim pulls in the entire export, including types no
surviving component requires. That costs nothing while the trim and the export are driven by the same adoption. It is
worth reopening for a corpus that exports many types where a plugin reads one.

[`export`](export.md) is what writes the data this assembles.
