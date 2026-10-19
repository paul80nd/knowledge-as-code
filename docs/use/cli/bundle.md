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

Run [`export`](export.md) first. `bundle` reads that output, plus the plugin tree and one key of `.corpus.yaml`. It
never loads the corpus itself. The plugin tree is `.plugin/` at the corpus root, unless
[`plugin.from`](../corpus-descriptor.md#plugin) points at a shared one.

### What decides whether a component ships

A component is one skill or hook in the plugin tree. Each declares under `metadata.components` which record types it
reads, as `requires`, and ships only where the export contains every one of them.

* **`requires` reads the export.** An export includes what its corpus consumes, so a corpus with no
  glossary of its own ships the glossary skill where it consumes a corpus that has one.
* **An entry may name a shape, as `glossary@1`.** The component then reads that type's files at shape version 1. A bare
  `glossary` asks only that the type is present. Both are trimmed where the export has no glossary.
* **A trimmed component's files leave with it.** The trim matches the declared path and everything beneath it, on whole
  segments, so `skills/a` does not take `skills/ab`.
* **Every skill and hook needs a component.** A run stops on a file under `skills/` or `hooks/` that no component
  claims, because nothing would trim it. A file anywhere else, such as a README or a licence, ships whatever the corpus
  adopted.
* **A component naming no type supports the lookups, unless it is standalone.** `corpus-retrieval` and
  `request-deviation` ship wherever a component reading a type shipped, and leave with the last of them. The trim
  reports that as `no component it supports survived`. `raise-finding` declares `"standalone": true` and always ships.
* **A run that trims every component warns and still builds.** The empty plugin installs, and `bundle.json` lists each
  component it dropped and the type that component needed.

### Views

A view is a Markdown file `bundle` writes into the plugin from the export, shaped for one moment of the work. You
declare one as a component whose `view` key names it. `bundle` writes it at the component's `path`, which nothing in
the plugin tree may already use.

| `view`        | What it lists, under each standard's id, title and status                             | Its component reads                           |
|---------------|---------------------------------------------------------------------------------------|-----------------------------------------------|
| `build-index` | each rule by title and id                                                             | `standards@2`                                 |
| `review`      | the controls checking the standard, with their mechanism, and each rule's obligations | `standards@2`, and `controls@1` where present |

A view is trimmed like any other component. A type under `optional` is read where the export has it, and its absence
trims nothing, so a corpus with no controls still gets the review view. The build index names the lookup skill that
reads standards where one survived the trim.
[The export format](../../reference/export.md#views-generated-from-the-export) says what a view reads.

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

A trimmed component is not an error. Its record type is one this corpus does not export.

### An install

```bash
claude plugin marketplace add ./.dist
```

Both directories are untracked, so nothing here needs a branch or a credential.

### The components that shipped

`example-engineering` exports controls, glossary, policies and standards, so it ships a lookup skill for each, beside
the three skills that name no type of their own. It also ships both views, because its export has standards. It has
no fixes and no processes, so `fix-lookup` and `process-lookup` are trimmed. The closing line counts what survived:

```text
bundle: wrote 71 file(s) to .dist/plugin/ as example-engineering 0.36.1. 10 component(s) included, 2 trimmed.
```

`bundle.json` lists them, and it travels inside the plugin:

```bash
jq -c '{kept: [.included[].path], trimmed: [.trimmed[].path]}' .dist/plugin/bundle.json
```

```text
{"kept":["skills/corpus-retrieval","skills/glossary-lookup","skills/raise-finding","skills/request-deviation","skills/controls-lookup","skills/policy-lookup","skills/standards-lookup","views/build-index.md","views/review.md","hooks"],"trimmed":["skills/fix-lookup","skills/process-lookup"]}
```

To see which assembled skill reads the file one type exports, search for it:

```bash
grep -l clauses.jsonl .dist/plugin/skills/*/SKILL.md
```

```text
.dist/plugin/skills/policy-lookup/SKILL.md
```

### A pipeline check

```bash
npm install -g @anthropic-ai/claude-code
claude plugin validate ./.dist/plugin --strict
claude plugin validate ./.dist --strict
```

`bundle` validates nothing it assembles, so this is the layer that does.

### The plugin manifest

`.plugin/.claude-plugin/plugin.json` declares what the plugin ships: `metadata.corpusRoot`, and the components under
`metadata.components`. `bundle` fills in who the plugin is from [`.corpus.yaml`](../corpus-descriptor.md), through the
export:

| Manifest key                                      | Comes from                   |
|---------------------------------------------------|------------------------------|
| `name`                                            | `corpus`                     |
| `version`                                         | `content-version`            |
| `displayName`, `description`, `license`, `author` | the four keys beside them    |
| `homepage`, `repository`                          | `publishing.base`            |
| `keywords`                                        | the types the export carried |

**`bundle` removes a key the corpus declared nothing for.** Three keys are the exceptions:

* `author`: a corpus that named nobody is filed under its own name, because `claude plugin validate --strict` fails a
  manifest with no `author`.
* `description`: a corpus with no description gets one built from its name.
* `version`: a corpus with no `content-version` keeps the version the manifest states.

Every other key in the manifest survives untouched.

## Known limits

**It refuses a run it cannot finish.** Each of these prints a reason, writes nothing and exits `1`:

* a plugin tree with no manifest
* a `plugin.from` naming a folder that is not there
* a manifest that is not JSON, or that states no `corpusRoot`, or a `corpusRoot` the plugin tree already uses
* an export with no manifest, or one naming no corpus because `.corpus.yaml` states no `corpus`
* an export whose `formatVersion` is not the one this build reads
* a component reading a type at a shape version the export does not carry, or at one that is not a whole number
* a file under `skills/` or `hooks/` that no component claims
* a view `bundle` does not write, or a view whose `path` the plugin tree or `corpusRoot` already uses

Where the format versions differ, the message gives both. Rebuild an export that is behind the tool with
[`export`](export.md). An export ahead of the tool means your `kac` is the older of the two. A shape mismatch means the
type's files have changed. [The export format](../../reference/export.md) says what moves a shape version.

**It validates nothing it assembles.** Run `claude plugin validate`, as [a pipeline check](#a-pipeline-check) shows.

**It does not publish.** Pushing the result anywhere is a separate job.

**The hook has been proved on macOS only.** Whether Claude Code runs the `.cmd` half of the hook on Windows is not yet
known.

**A component's `requires` is not checked against the schema.** A misspelt type trims its component with the same
message as a type this corpus declined.

**A view has no `paths:` and loads nowhere by itself.** Delivering it to an agent is a hook's or a skill's job.

**The export is copied whole.** A component that survives the trim brings the entire export, including types no
surviving component requires.

[`export`](export.md) is what writes the data this assembles.
