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
* **An entry may name alternatives, as `standards|policies`.** Any one of them meets it, and the component is trimmed
  only where the export has none. `request-deviation` declares this, because a clause it asks about is a standard's
  rule or a policy's clause.
* **A trimmed component's files leave with it.** The trim matches the declared path and everything beneath it, on whole
  segments, so `skills/a` does not take `skills/ab`. A component declared inside a trimmed one leaves too.
* **Every skill and hook needs a component.** A run stops on a file under `skills/` or `hooks/` that no component
  claims, because nothing would trim it. A file anywhere else, such as a README or a licence, ships whatever the corpus
  adopted.
* **A component naming no type supports the ones that do, unless it is standalone.** `ask-the-corpus` and the hooks
  ship wherever a component reading a type shipped, and leave with the last of them.
  The trim reports that as `no component it supports survived`. `raise-finding` declares `"standalone": true` and
  always ships.
* **A run that trims every component warns and still builds.** The empty plugin installs, and `bundle.json` lists each
  component it dropped and the type that component needed.

### A skill's reference files

A reference file is a Markdown file inside a skill's directory that the skill reads for one type, such as
`skills/ask-the-corpus/runbooks.md`. Each is declared as its own component requiring that type, so a corpus exporting
no runbooks ships the router without its runbook file.

Each reference file opens with a `trigger:` in its frontmatter, naming the question that sends a session to it.
`bundle` writes the trigger of each reference file that shipped into its skill's `when_to_use:`, which Claude Code
lists beside the description, so the skill offers only what its plugin can answer. Two lengths stop the run:

* a `description:` over 1,024 characters, the [Agent Skills specification](https://agentskills.io/specification)'s
  maximum
* a `description:` and `when_to_use:` over 1,536 characters together, past which Claude Code truncates its listing

### Views

A view is a Markdown file `bundle` writes into the plugin from the export, shaped for one moment of the work. You
declare one as a component whose `view` key names it. `bundle` writes it at the component's `path`, which nothing in
the plugin tree may already use.

| `view`        | What it lists, under each standard's id, title and status                       | Its component reads                           |
|---------------|---------------------------------------------------------------------------------|-----------------------------------------------|
| `build-index` | each rule by title                                                              | `standards@3`                                 |
| `review`      | each rule's obligations, and the controls checking it, with their mechanism     | `standards@3`, and `controls@3` where present |

A view is trimmed like any other component. A type under `optional` is read where the export has it, and its absence
trims nothing, so a corpus with no controls still gets the review view. The build index opens on how many rules the
corpus states, and names the skill that reads standards where one survived the trim.
[The export format](../../reference/export.md#views-generated-from-the-export) says what a view reads.
[Copying the review view](../skills.md#copying-the-review-view-to-coding_standardsmd) says how another review tool
can read it.

### The build-index hook

Where the `hooks` component and the build index both ship, `bundle` writes `hooks/build-index.json`. The hook runs
before every tool call. It prints the build index at the first call of the main session, at the first call of each
subagent, and again after the session compacts. Claude Code passes the build index to the agent with that call's
result, whichever tool the call used. An agent whose first call is an edit has the build index once that edit returns.

Claude Code passes a hook at most 10,000 characters of context. Over that, `bundle` writes the rule and standard counts
and the build index's path in its place, and warns.

### A corpus with no tracker

`raise-finding` and `request-deviation` file an issue on the [`tracker`](../corpus-descriptor.md#tracker) the export
states. Where that tracker has no backlog to file on, `bundle` warns and still builds. The warning names each of the two
skills the plugin ships:

<!-- transcript: illustration, kac bundle -->

```text
bundle: this corpus has no tracker to file on, so raise-finding and request-deviation will print each issue for you to file. Add a `tracker:` block to .corpus.yaml and export again to set one.
```

A corpus publishing to `none` has no tracker until you add the block. [`validate`](validate.md) says nothing about it,
because a corpus that never bundles never files.

## Examples

### A built plugin

```bash
kac export
kac bundle
```

`bundle` prints each file as it writes it. The run closes with what shipped, what was trimmed and why, and how to
install it:

<!-- transcript: examples/library, kac bundle -->

```text
wrote .dist/plugin/hooks/breadcrumb
wrote .dist/plugin/hooks/breadcrumb.cmd
wrote .dist/plugin/hooks/breadcrumb.txt
wrote .dist/plugin/hooks/build-index
wrote .dist/plugin/hooks/hooks.json
wrote .dist/plugin/skills/ask-the-corpus/SKILL.md
wrote .dist/plugin/skills/ask-the-corpus/adrs.md
wrote .dist/plugin/skills/ask-the-corpus/data.md
wrote .dist/plugin/skills/ask-the-corpus/explanations.md
wrote .dist/plugin/skills/ask-the-corpus/glossary.md
wrote .dist/plugin/skills/ask-the-corpus/integrations.md
wrote .dist/plugin/skills/ask-the-corpus/offerings.md
wrote .dist/plugin/skills/ask-the-corpus/retrieval.md
wrote .dist/plugin/skills/ask-the-corpus/services.md
wrote .dist/plugin/skills/raise-finding/SKILL.md
wrote .dist/plugin/skills/raise-finding/azure-devops.md
wrote .dist/plugin/skills/raise-finding/github.md
bundle: trimmed skills/request-deviation: the export carries no standards or policies.
bundle: trimmed skills/ask-the-corpus/standards.md: the export carries no standards.
bundle: trimmed skills/ask-the-corpus/policies.md: the export carries no policies.
bundle: trimmed skills/ask-the-corpus/controls.md: the export carries no controls.
bundle: trimmed skills/ask-the-corpus/processes.md: the export carries no processes.
bundle: trimmed skills/ask-the-corpus/fixes.md: the export carries no fixes.
bundle: trimmed skills/ask-the-corpus/deviations.md: the export carries no deviations.
bundle: trimmed skills/ask-the-corpus/reports.md: the export carries no reports.
bundle: trimmed skills/ask-the-corpus/runbooks.md: the export carries no runbooks.
bundle: trimmed skills/ask-the-corpus/nfrs.md: the export carries no nfrs.
bundle: trimmed skills/ask-the-corpus/postmortems.md: the export carries no postmortems.
bundle: trimmed skills/ask-the-corpus/tools.md: the export carries no tools.
bundle: trimmed skills/standards-review: the export carries no standards.
bundle: wrote 50 files to .dist/plugin/ as example-libraries 0.37.1. 10 components included, 13 trimmed.
bundle: .dist/ is a marketplace holding it. Install it from a path with:  claude plugin marketplace add ./.dist
```

A trimmed component is not an error. Its record type is one this corpus does not export. This corpus exports no
standards, so the build-index hook ships with no `hooks/build-index.json` and prints nothing.

### An install

```bash
claude plugin marketplace add ./.dist                       # register the marketplace bundle wrote
claude plugin install example-libraries@example-libraries   # install the plugin from it
```

`.dist/` and the plugin inside it are untracked, so nothing here needs a branch or a credential.

The install takes `<plugin>@<marketplace>`. `bundle` names both after `corpus` in `.corpus.yaml`, so write your
corpus's name on each side of the `@`. [The names an install uses](../installing-a-plugin.md#the-names-an-install-uses)
says where each name comes from.

### The components that shipped

`example-engineering` exports ADRs, controls, glossary, policies, reports and standards. It ships `ask-the-corpus` with
the reference file for each of those types. Its export has standards, so it also ships both views and
`standards-review`, which reads the review view. The reference file for every other type is trimmed. The closing line
counts what survived:

<!-- transcript: examples/engineering, kac bundle -->

```text
bundle: wrote 81 files to .dist/plugin/ as example-engineering 0.53.1. 13 components included, 12 trimmed.
```

`bundle.json` lists them, and it travels inside the plugin:

```bash
jq -c '{kept: [.included[].path], trimmed: [.trimmed[].path]}' .dist/plugin/bundle.json
```

<!-- transcript: examples/engineering, jq -c '{kept: [.included[].path], trimmed: [.trimmed[].path]}' .dist/plugin/bundle.json -->

```text
{"kept":["skills/raise-finding","skills/request-deviation","skills/ask-the-corpus","skills/ask-the-corpus/standards.md","skills/ask-the-corpus/policies.md","skills/ask-the-corpus/controls.md","skills/ask-the-corpus/glossary.md","skills/ask-the-corpus/reports.md","skills/ask-the-corpus/adrs.md","skills/standards-review","views/build-index.md","views/review.md","hooks"],"trimmed":["skills/ask-the-corpus/processes.md","skills/ask-the-corpus/fixes.md","skills/ask-the-corpus/deviations.md","skills/ask-the-corpus/runbooks.md","skills/ask-the-corpus/data.md","skills/ask-the-corpus/explanations.md","skills/ask-the-corpus/integrations.md","skills/ask-the-corpus/nfrs.md","skills/ask-the-corpus/offerings.md","skills/ask-the-corpus/postmortems.md","skills/ask-the-corpus/services.md","skills/ask-the-corpus/tools.md"]}
```

To see which assembled file reads the parts file one type exports, search for it:

```bash
grep -l clauses.jsonl .dist/plugin/skills/*/*.md
```

<!-- transcript: examples/engineering, grep -l clauses.jsonl .dist/plugin/skills/*/*.md -->

```text
.dist/plugin/skills/ask-the-corpus/policies.md
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

| Manifest key                                      | Comes from                                                                              |
|---------------------------------------------------|-----------------------------------------------------------------------------------------|
| `name`                                            | `corpus`                                                                                |
| `version`                                         | `content-version`                                                                       |
| `displayName`, `description`, `license`, `author` | the four keys beside them                                                               |
| `homepage`                                        | `publishing.base`                                                                       |
| `repository`                                      | the Git repository of an `azure-devops-wiki` corpus, and `publishing.base` on any other |
| `keywords`                                        | the types the export carried                                                            |

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
* a `requires` entry naming an empty alternative, as `standards|`
* a skill whose description passes 1,024 characters, or whose description and listed triggers pass 1,536
* a file under `skills/` or `hooks/` that no component claims
* a component, other than a view, whose `path` has no file in the plugin tree
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

**A view has no `paths:` and loads nowhere by itself.** The build-index hook pushes the build index. The review view
reaches an agent only where a skill reads it.

**The build-index hook marks each agent done in the plugin's data directory.** It makes one directory per agent under
`build-index/`, and deletes any older than 7 days each time it makes one. A compaction deletes the session's
directories. A session whose id the hook cannot read gets the build index on every call.

**The export is copied whole.** A component that survives the trim brings the entire export, including types no
surviving component requires.

[`export`](export.md) is what writes the data this assembles.
