# The plugin bundle

`kac bundle` assembles a corpus's export and its plugin tree into a Claude Code plugin under `.dist/plugin/`, and the
marketplace that offers it under `.dist/`. This page lists what the bundle is built to do, and the scenario proving
each behaviour. [`bundle`](../use/cli/bundle.md) owns the full statement of what a run reads, ships and refuses.

A **corpus** is one repository of knowledge records kept in git. An **export** is what [`export`](../use/cli/export.md)
writes into `.dist/export/`, as data an agent reads. A **plugin tree** is the skills and hooks a corpus keeps beside its
records, at `.plugin/`. A **component** is one skill or hook inside that tree.

## What it does

* It reads the export, the plugin tree and one key of `.corpus.yaml`, and never loads the corpus.
  [What it does](../use/cli/bundle.md#what-it-does)
* It reads the plugin tree from `plugin.from` where the descriptor states one, and keeps this corpus's own manifest. A
  file this corpus keeps under `.plugin/` wins over the shared tree's copy.
  [`plugin`](../use/corpus-descriptor.md#plugin)
* It writes the plugin, `bundle.json` and the marketplace. [A built plugin](../use/cli/bundle.md#a-built-plugin)
* It copies the export into the plugin under `metadata.corpusRoot`, byte for byte.
  [What it does](../use/cli/bundle.md#what-it-does)
* It trims a component whose `requires` names a type the export lacks, with every file under its path, on whole
  segments. The export includes what the corpus consumes, so a type only a consumed corpus wrote meets a `requires`.
  [What decides whether a component ships](../use/cli/bundle.md#what-decides-whether-a-component-ships)
* It ships a component reading no type while a component reading one survives, and trims it with the last of them,
  unless the component is standalone.
  [What decides whether a component ships](../use/cli/bundle.md#what-decides-whether-a-component-ships)
* It ships a file no component claims, outside `skills/` and `hooks/`.
  [What decides whether a component ships](../use/cli/bundle.md#what-decides-whether-a-component-ships)
* It builds a plugin with every component trimmed, and warns.
  [What decides whether a component ships](../use/cli/bundle.md#what-decides-whether-a-component-ships)
* It fills the plugin manifest's identity from the descriptor, through the export, with `homepage` and `repository`
  from `publishing.base`. It removes a key the corpus declared nothing for, bar `author`, `description` and `version`.
  [The plugin manifest](../use/cli/bundle.md#the-plugin-manifest)
* It cuts the plugin manifest's components to the ones that survived, and keeps every key it does not know.
  [The plugin manifest](#the-plugin-manifest)
* It renders the breadcrumb the hook prints, and keeps the hook's permission bit. [The breadcrumb](#the-breadcrumb)
* It renders what the build-index hook prints, where the build index ships, and warns where the index is over the
  hook's cap. [The build-index hook](#the-build-index-hook)
* It writes each view a surviving component declares, from the export alone. [Views](#views)
* It reads a type under a component's `optional` where the export has it, and trims nothing for its absence.
  [Views](../use/cli/bundle.md#views)
* It replaces `.dist/plugin/` whole. [The `.dist/` layout](#the-dist-layout)

## What it refuses

Each refusal exits `1` and writes nothing. [Known limits](../use/cli/bundle.md#known-limits)

* no plugin tree, or a `plugin.from` naming a folder that is not there
* a plugin tree with no manifest, or a manifest that is not JSON
* a manifest stating no `metadata.corpusRoot`, or one the plugin tree already uses
* no export, an export with no manifest, or one naming no corpus
* an export at a format version this build does not read
* a component reading a type at a shape version the export does not have, or at a shape that is not a whole number
* a file under `skills/` or `hooks/` that no component claims
* a view this build does not write, or a view whose `path` the plugin tree or `corpusRoot` already uses

## What it promises a consumer

* The export inside the plugin is the export `export` wrote, byte for byte. [The `.dist/` layout](#the-dist-layout)
* The plugin's version is the corpus's content version. [The plugin's version](#the-plugins-version)
* `bundle.json` lists every component included and every one trimmed, with the reason. [`bundle.json`](#bundlejson)
* No shipped skill reads a type the export does not carry.
  [What decides whether a component ships](../use/cli/bundle.md#what-decides-whether-a-component-ships)

## The `.dist/` layout

Each command replaces its own directory whole. `export` owns `.dist/export/`, `bundle` owns `.dist/plugin/` and
[`pack`](../use/cli/pack.md) owns `.dist/package/`. No command deletes another's directory. `Dist` is the one statement
of the layout.

The export exists twice: at `.dist/export/` as `export` wrote it, and inside the plugin under the directory
`metadata.corpusRoot` names. An installed plugin is copied into a cache of its own, so a path outside the plugin root
resolves nowhere. `bundle` copies bytes, never text, so the copy cannot change an encoding or a line ending.

## The breadcrumb

A `SessionStart` hook prints a few lines into every session. They say which corpus is installed, how many entries it
holds, which records cover them, which skill to ask, and how to report a change the records no longer describe.

* **A line names at most six records.** A type over the bound has its first names given and the rest as a count.
* **A type has a line per corpus that wrote some of it.** The installing corpus has one where it wrote any, and each
  corpus it consumes has one carrying its shortcode:

```text
standards. 24 entries across 8 records: Card details reach the PSP and never reach us, One order pays once, …
standards (from eng). 37 entries across 12 records: An interface is described by a contract in the repository, …
```

* **The closing lines name each surviving skill by the role its entry announces.** They open by saying the records
  are the organisation's recorded knowledge, and warn against answering from memory. A skill earns a role where a
  session would never think to ask it, and a role whose skill was trimmed prints nothing. The table below lists the
  roles.
* **It is rendered at build time.** Everything it states is a fact about the export inside the plugin, so `bundle`
  writes `hooks/breadcrumb.txt` once and the hook is one `cat`. The consumer's machine needs no parser and no runtime.
* **It travels with the hook directory.** `bundle` asks which files survived, so a trimmed hook takes the breadcrumb
  with it.
* **It names no record type.** Counts, names, shortcodes and skills are read off the export and the surviving
  components, so a type this tool has never heard of gets a line.

| `announce`     | The breadcrumb asks a session to load the skill                | In the template   |
|----------------|----------------------------------------------------------------|-------------------|
| `"router"`     | for a question whose record the session cannot place           | `ask-the-corpus`  |
| `"lookup"`     | for a known need, named as an example of a lookup              | `glossary-lookup` |
| `"write-back"` | when the session's change alters something a record describes  | `raise-finding`   |

`true` reads as `"lookup"`, so a manifest written before the roles keeps its line. Any other value announces nothing.

The breadcrumb hook ships as a POSIX script and a `.cmd` twin. `bundle` keeps the permission bit on every script, so
each runs at the first session.

## The build-index hook

A `PreToolUse` hook on every tool prints the build index once a session, at the session's first tool call. Claude
Code passes a `PreToolUse` hook's `additionalContext` to the session beside that tool's result, so the session never
reads it before the call runs. The matcher decides only how early the index arrives. Matching every tool delivers it
with the first result, whether the session starts with `Read`, `Bash` or the Windows `PowerShell` tool. A session whose
first call is an edit has the index once that edit returns.

* **It is rendered at build time.** `BuildIndexHook` writes `hooks/build-index.json`: one line of JSON whose
  `additionalContext` is the build index as `View` rendered it. The script prints that file and parses nothing.
* **It travels with the hook directory and the build index.** `bundle` writes the file only where both survived the
  trim. Without it the script prints nothing, so a corpus with no standards ships a silent script.
* **It fires once a session.** The script cuts `session_id` out of its input. A JSON string escapes every quote inside
  it, so the key with bare quotes is the only match. It then makes a directory named after the session under
  `${CLAUDE_PLUGIN_DATA}/build-index/`. `mkdir` succeeds for one caller only, so two parallel calls print once.
* **A call after the first starts one `cat` and nothing else.** Every tool call runs the script. It reads its input
  with `cat`, finds the session id with shell built-ins, and exits when the session's directory exists. Only a first
  call runs `mkdir`, `find` and a second `cat`.
* **It prunes its own markers.** Each first call deletes marker directories older than 7 days.
* **Over 10,000 characters it points instead.** Claude Code passes a hook's context to the session up to that length,
  and over it shows a file path and the first 2,000 characters. `bundle` measures the whole build index and, where it
  is over, writes the rule and standard counts and the view's path in its place, and warns.
* **It needs only a POSIX shell.** It has no `.cmd` twin, because Claude Code runs a hook under `sh`, Git Bash or
  PowerShell.

The build index states facts and gives no instruction, because Claude Code passes it to the session as context, and
the hook's documentation asks for factual statements there. The view and the hook share that text.

## Views

A view is a file `View` renders into the plugin from the export, after the trim. A component declares one with a
`view` key, and `bundle` writes it at the component's `path`. Nothing in the plugin tree sits at that path.

* **It reads the export inside the plugin, and nothing else.** A fresh bundle of one export writes every view byte for
  byte, so a view is never a copy somebody keeps beside the export.
* **It reads part lines at the shape its component pins.** `standards@2` fixes the keys of a rule line, so a view reads
  `title`, `obligations` and `status` by name.
* **It names an inherited record as the export does.** A consumed corpus's control sits under its shortcode, and its
  id and `verifies` are read with that shortcode, so a control from `eng` checks `eng:std-DEPLOY`.
* **It states a standard's status and controls once, in or under the standard's heading.** A rule line repeats its
  standard's status, so a view reads it from the first rule. The build index puts the status in the heading, to keep
  the file short. A control is listed under each standard its `verifies` names, or whose rule it names.
* **The review view names the status in force from the manifest.** It reads `inForce` on the standards entry and on
  the controls entry. Where an entry states none, as an export from an older `kac` does, the view says the export does
  not state it. For controls, it then says no control is known to check a rule.
* **The review view opens on facts.** It says what each keyword means, and that a control checks only the obligations
  its record's What it checks section names. A control checks them only at the status in force, and only where it runs
  as `ci` or `runtime-alert`. The `standards-review` skill reads the view whole in a forked subagent, and gives the
  instructions itself.
* **The build index names a skill only where one survived.** It names the surviving lookup skill that reads standards,
  and the rules file inside the plugin where none did.

## The plugin's version

It is the corpus content version, read off the export's manifest. The export's format version stays inside the export.
A corpus stating no content version keeps the version its plugin manifest states, and the run warns.

## The plugin manifest

`bundle` edits `plugin.json` as a DOM. It sets the identity keys, the version and the surviving components, and writes
every other key back exactly as the corpus wrote it. [`bundle`](../use/cli/bundle.md#the-plugin-manifest) lists which
key comes from where.

## `bundle.json`

At the plugin root, it records the plugin, its version, the export it was built around, every component included, and
every one trimmed with the type it needed and the reason. It states no timestamp and no commit, because the export
inside the plugin states both.

## The marketplace

`.dist/` is the marketplace. A marketplace resolves each plugin's source against the directory holding
`.claude-plugin/marketplace.json`, and refuses a source containing `..`, so it sits at the root above the plugin.
`marketplace.json` takes the plugin's name, names the plugin's author as its owner, and offers the one plugin from
`./plugin` with the plugin's description.

## `corpusRoot`

`bundle` reads `metadata.corpusRoot` and never defaults it. The plugin's skills address the export as
`${CLAUDE_PLUGIN_ROOT}/<corpusRoot>/…`, so the name has to be the one the corpus wrote in its skills.

## Why not

**Why not ship `harvest-findings` as a component.** It triages the issues already filed and writes records, so its
reader is the corpus's own maintainer holding the source. [`new`](../use/cli/new.md) writes it into the working tree
under `.claude/skills/` and [`update`](../use/cli/update.md) keeps it current. A consumer install would ship a skill
that can only refuse.

**Why not trim `raise-finding` with the lookups.** It supports nothing and serves the reader. A corpus with no records
is the one where a session most needs a route to report, so it is standalone. Standalone does not exempt it from the
type trim.

**Why not an archive.** A marketplace points at a path, so a directory needs nothing unpacked before a question.

## Specified by

[`Bundle.feature`][Bundle.feature]:

* A bundle writes the plugin, its account of itself and the marketplace
* A bundle reads the export and never the corpus
* A bundle reads a shared plugin tree and keeps this corpus's own manifest
* A file this corpus keeps wins over the shared tree's copy
* A bundle rebuilds .dist/plugin/ whole
* A component reading a type the export lacks is trimmed with its files
* A trim takes whole path segments
* A component reading a type only a consumed corpus exports ships
* A component reading no type ships beside one that reads a type
* A component reading no type leaves with the last component reading one
* A standalone component always ships
* A file outside the skill and hook directories ships with no component
* The plugin contains the export unchanged
* The plugin manifest takes who the plugin is from the descriptor
* A key the descriptor gives nothing for is removed, bar the author and the description
* The plugin manifest takes its homepage and repository from the publishing base
* The plugin manifest lists the components that survived, and keeps every key the tool does not know
* A corpus stating no content version keeps the version its plugin manifest states
* A bundle that trims every component still builds
* bundle.json states the plugin and the export it was built around
* The marketplace offers the plugin from the directory beneath it
* The breadcrumb names at most six records of a type
* The breadcrumb gives a type a line for each corpus that wrote some of it
* The breadcrumb asks for each announced skill that survived
* The breadcrumb names each surviving skill by the role it announces
* The breadcrumb leaves with a trimmed hook
* The hook keeps its permission bit
* A bundle writes each view the plugin manifest declares
* A view whose types the export lacks is trimmed, and written nowhere
* A view reads an optional type only where the export carries it
* The build-index hook prints the build index where the hook and the index both ship
* The build-index hook prints the build index once a session
* The build-index hook prints nothing where no build index ships
* A bundle with no export is refused
* A bundle with no plugin tree is refused
* A bundle reading the plugin tree from a folder that is not there is refused
* A bundle whose plugin tree has no manifest is refused
* A bundle whose plugin manifest is not JSON is refused
* A bundle whose plugin manifest states no corpusRoot is refused
* A bundle whose corpusRoot the plugin tree already uses is refused
* A bundle over an export with no manifest is refused
* A bundle over an export naming no corpus is refused
* A bundle over an export at another format is refused
* A bundle whose component reads a shape the export does not have is refused
* A bundle whose component reads a shape that is not a whole number is refused
* A bundle whose view reads a shape the export does not have is refused
* A bundle declaring a view this tool does not write is refused
* A bundle whose plugin tree already holds a view's file is refused
* A bundle whose plugin tree holds a skill no component declares is refused

## Unspecified

* A skill reading the parts file of the type it requires. `bundle` never reads a component's files, and
  `tooling/tests/round-trip.sh` checks it over the installed copy.
* The `.cmd` half of the breadcrumb hook running on Windows. A scenario runs on the machine that runs the suite.
* The build-index hook firing once a session. A scenario reads what `bundle` wrote and runs no script, so
  `tooling/tests/round-trip.sh` runs the installed script twice.
* The warning over the build-index hook's cap. A scenario cannot read the output, so `BundlerTests` asserts it.
* What the run prints: each trim, the closing count, the install hint and every refusal's reason. A scenario cannot
  read the output.

## Where to go next

[`bundle`](../use/cli/bundle.md) is the command that assembles this.

[Bundle.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Bundle.feature
