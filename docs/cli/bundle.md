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

`bundle` assembles what [`export`](export.md) wrote, plus the `.plugin/` tree your corpus keeps beside its records,
into a Claude Code plugin directory under `.dist/plugin/`. It writes the marketplace that offers it into `.dist/`
above, so the result can be installed.

What ends up in the plugin is a function of what the export carried: a corpus shipping no glossary ships no glossary
skill either. [The plugin bundle](../design/plugin.md) says what decides that, and what stops a run.

Run `export` first. `bundle` reads that output and never the corpus.

## Examples

### Build the plugin

```bash
kac export
kac bundle
```

Each file is named as it is written. The run closes with what shipped, what was trimmed and why, and how to install it:

```text
wrote .dist/plugin/corpus/glossary/terms.jsonl
wrote .dist/plugin/corpus/manifest.json
wrote .dist/plugin/hooks/breadcrumb
wrote .dist/plugin/skills/glossary-lookup/SKILL.md
bundle: trimmed skills/policy-lookup: the export carries no policies.
bundle: wrote 13 file(s) to .dist/plugin/ as example-libraries 0.1.0. 2 component(s) included, 1 trimmed.
bundle: .dist/ is a marketplace holding it. Install it from a path with:  claude plugin marketplace add ./.dist
```

A trimmed component is not an error. It is a skill whose record type this corpus does not export.

### Install what you just built

```bash
claude plugin marketplace add ./.dist
```

Both directories are untracked, so nothing here needs a branch or a credential. What you get on a laptop is exactly
what CI publishes.

### Validate it in a pipeline

```bash
npm install -g @anthropic-ai/claude-code
claude plugin validate ./.dist/plugin --strict
claude plugin validate ./.dist --strict
```

`bundle` validates nothing it assembles, so this is the layer that does.

## Known limits

**This command validates nothing it assembles.** A component misplaced inside `.claude-plugin/` leaves here
unreported. `claude plugin validate` runs one layer out, which keeps the build runnable without the Claude Code CLI
installed.

**It does not publish.** Pushing the result anywhere is a separate job, and one needing credentials this one should not
have.

**The hook has been proved on macOS only.** Nothing yet says which shell Claude Code reaches a hook command with on
Windows, so nothing yet says whether the `.cmd` half of the pair is ever the one that runs. The round-trip test
installs the plugin on a Windows runner but opens no session, so it cannot answer this.

**A component's `requires` is not held against the schema.** A component naming a type no schema declares is trimmed
with the same message as one naming a type this corpus declined. One is a typo and the other is a decision, and nothing
reports the first.

**The export is copied whole.** A component surviving the trim pulls in the entire export, including types no surviving
component names. That costs nothing while the trim and the export are driven by the same adoption, and is worth
reopening for a corpus exporting many types where a plugin reads one.

[`export`](export.md) is what writes the data this assembles.
