# `bundle` assemble the export into an installable agent plugin

<!-- BEGIN GENERATED: usage-bundle -->

```text
kac bundle [--no-color]
```

| Option       | What it does                                                |
|--------------|-------------------------------------------------------------|
| `--no-color` | Turn colour off. NO_COLOR in the environment does the same. |

<!-- END GENERATED: usage-bundle -->

## What it is for

An export is data, and data has to be handed to something. `bundle` assembles what `export` wrote, plus the `.plugin/`
tree, into a Claude Code plugin directory under `.dist/plugin/`. It writes the marketplace that offers it into `.dist/`
above, so the result can be installed. What ends up in the plugin is a function of what the export carried: a corpus
that ships no
glossary ships no glossary skill either.

## What it is not

**It is not [`export`](export.md).** `export` reads the corpus and writes data. `bundle` reads that data and writes a
package. They
are two commands because they fail differently: an export is wrong about the corpus, and a bundle is wrong about what
it shipped. They are proved differently too. The export has a committed golden of its output, and the bundle has an
assertion that it did not touch that output.

**It does not publish.** It writes a directory and a marketplace that names it, both untracked. Pushing either anywhere
is CI's job (on GitHub, `publish-plugin.yml`), and so is running `claude plugin validate` over the result. What that
division buys is a command a reader runs on a laptop, without credentials and without a branch to write to. It produces
there exactly what CI publishes.

**It does not read the corpus.** `Corpus.Load` is never called. Everything a bundle decides is a fact about the export
it was handed, and the export is the only thing that travels.

## How it works

A run reads the export `export` wrote and the corpus's own `.plugin/` tree. It decides which components that export can
support, copies what survives into `.dist/plugin/`, renders the breadcrumb, and writes a manifest naming what shipped.

### What stops a run before it writes

Each of these ends the run with the reason and nothing written:

* a plugin tree with no manifest
* a manifest that is not JSON, or that states no name, or no `corpusRoot`, or a `corpusRoot` the plugin tree already
  uses
* an export with no manifest
* an export whose `formatVersion` is not the one this build reads

Stopping is the point in every one of them. The alternative is a plugin assembled around a missing answer, which
installs and fails later somewhere less obvious. The `corpusRoot` collision is the one worth naming twice. The export
lands under that directory inside the plugin, so a clash would have one side silently overwrite the other. Whichever
won, the loser would be missing from an artefact nobody reviews.

The `formatVersion` refusal is where the export format version is finally held to account. Both numbers are named,
because the reader's next move differs. An export behind the tool is rebuilt, and an export ahead of it says the tool
is the stale half. `.dist/export/` is untracked and outlives the run that wrote it, so a bundle built after a pull is
the ordinary way to meet an export this tool did not ship beside. That case is what the field exists for.

### Two directories under one root, and each command replaces its own whole

`export` owns `.dist/export/` and `bundle` owns `.dist/plugin/`. Both delete before they write, for the reason `export`
already had: an artefact nobody reviews must not keep a file that nothing backs any more. That rule only stays local if
neither command deletes the other's tree, which is why the export is a named subtree rather than the root. `Dist` is the
one statement of the layout, and `.gitignore` covers all of it in a line.

**The export is copied in, and therefore exists twice.** Once at `.dist/export/` as `export` wrote it, and once inside
the plugin under the directory `metadata.corpusRoot` names. **That duplication is intended.** An installed plugin is
copied into a cache of its own, so a path outside the plugin root does not travel. It resolves nowhere at runtime. And
`export` has to stay independently runnable and independently proved without `bundle` having run. The copy is the seam
between the two commands.

**`bundle` never edits what it copies.** The export travels byte for byte. The plan carries bytes rather than text, so
a copy cannot acquire an opinion about encoding or line endings on the way through. A difference between the two copies
is a defect, and the golden fixture asserts their equality directly.

### Trimming reads the export, not the corpus

A component declares under `metadata.components` which record types it
reads, and it travels only where the export carries every one of them. What that catches is the skill that finds
nothing. To whoever asked it a question, that skill reads exactly like a corpus that does not define the term.

Reading the export is what makes the criterion the same in both states `.corpus.yaml` can be in. A corpus that declared
`types:` and one whose adoption is inferred from its folders both reach the export through `Corpus.Adopted`. And a type
that contributed no record is absent from the export's manifest either way.

A trimmed component's files leave with it. A path under no component's is unconditional and travels whatever the corpus
adopted, so a README or a licence in the plugin tree needs no declaration.

**A component may be a directory or a single file.** So the trim matches the declared path itself as well as anything
beneath it. The match is on a whole segment, so `skills/a`
does not take `skills/ab` with it.

**Trimming everything warns and still builds.** Refusing would leave a corpus unable to produce the thing that would
have told it why. The empty plugin is itself the report: it installs, does nothing, and `bundle.json` beside it names
each component that was dropped and the type it needed.

### The breadcrumb

#### What it says, and why it stops there

A `SessionStart` hook injects a few lines into every session. They say which corpus is installed, how many entries it
holds, which records cover them, and which skill to ask. That is the whole of its job. An agent never asks for a
glossary, because it does not know a word is ambiguous, so the breadcrumb exists to create the question rather than to
answer it. A longer one would be paid for by every session that had none to ask.

#### It is rendered here, and not computed at runtime

Everything it states is a fact about the export sitting inside the plugin, and an installed plugin's export does not
change between builds. So `bundle` writes the text once and the hook is one `cat`. Nothing on the consumer's machine is
asked for a JSON parser, an interpreter or a runtime. The corpus takes the same position about every other generated
projection: compute it once into an artefact rather than have each reader recompute it.

#### It travels with the hook that prints it

The rendered file travels with the directory that prints it and nowhere else. A corpus shipping no hook has nothing to
read it. A corpus whose hook was trimmed would otherwise keep a file describing a component that left. Asking the
surviving files rather than the components settles both without a corpus having to declare that a generated file belongs
to one.

#### Nothing in the render names a record type

The counts, the record names and the skill to ask are read off the export and off the surviving components. So a corpus
adopting a type this tool has never heard of gets a breadcrumb about it with no line changing.

The record names are the point of the line a count alone cannot make. A corpus keeps one glossary per bounded context.
Three names say which contexts are covered, where the number three says only that there are some.

#### A line names at most six things

The names do a job that stops at a handful. The length of the breadcrumb is what every session pays at start, resume,
clear and compact. The bound is therefore a number the renderer holds rather than however many records a corpus turns
out to keep.

A type over the bound has its first names carried and the rest given as a count, which is the last of the six. A list
cut short in silence would read as the whole of what the type covers, and the count is what the line was for.

### A hook is copied with its permission bit

One file in the plugin tree is run rather than read. A command copied without
that bit is a plugin that installs and then fails at the first session, with a message about permissions rather than
about the corpus.

The bit does not exist on Windows and is not asked for there. A hook ships as a POSIX script and a `.cmd` twin, the
same pair and the same reasoning as `kac` and `kac.cmd` at a corpus's root.

### The plugin's version is the corpus content version

It is read off the export's manifest rather than out of
`.corpus.yaml`, so the number on the plugin is the number of the data inside it. The export **format** version stays
where it is, inside the export's own manifest. One says what the corpus knows and the other says which parser will read
it, so stamping the second onto the plugin would describe the wrong thing confidently. A corpus stating no content
version keeps the version its plugin manifest carried, and the run says so rather than stamping nothing.

### The plugin manifest is edited, not rebuilt

`plugin.json` is forked in the portability manifest, so each corpus owns its own. `bundle` reads it as a DOM. It gives
the manifest a new version and the surviving components, then writes it back with every other key exactly as the corpus
wrote it. Mapping it onto a shape known here would delete whatever the corpus had added, without a word.

### The bundle records what it shipped

`bundle.json` at the plugin root carries the plugin, its version, the export it was built
around, every component included and every one trimmed with the reason. Two corpora running one plugin name may ship
different component sets. That is correct, and it makes "does this plugin do X" unanswerable from outside unless the
plugin says. `bundle.json` carries no timestamp and no commit: the export inside the plugin states both, and a second
clock would be a second answer to one question.

### `.dist/` is the marketplace

A marketplace is a directory holding `.claude-plugin/marketplace.json`, and it resolves
each plugin's source against that directory. A source containing `..` is refused. So a marketplace cannot sit beside the
plugin and point sideways at it, and the root is where it goes. `claude plugin marketplace add ./.dist` installs what
was just built.

## Decisions

**The output is a directory rather than an archive.** A marketplace can point at a path. There is nothing to unpack
between building and asking the plugin a question.

**`corpusRoot` is read rather than defaulted.** The plugin's skills address the export as
`${CLAUDE_PLUGIN_ROOT}/<corpusRoot>/…` by that name. A default here would be the tool quietly disagreeing with words
the corpus wrote in its own skill. The disagreement would surface only when someone asked the installed plugin a
question.

## Known limits

**This command validates nothing it assembles.** `claude plugin validate` is a CI step rather than part of the build.
So a component misplaced inside `.claude-plugin/` leaves here unreported, and is caught one layer out. That is the same
division as publishing: the build stays runnable without the CLI installed.

**The hook has been proved on macOS only.** The assembled plugin installs from the marketplace beside it. Its
`SessionStart` command reaches the breadcrumb through `${CLAUDE_PLUGIN_ROOT}`, and lands it in a session opened in an
unrelated directory.

What no run yet says is which shell Claude Code reaches a hook command with on Windows. So nothing yet says whether the
`.cmd` half of the pair is ever the one that runs. The round-trip installs the plugin on a Windows runner but opens no
session, so it cannot answer this.

**A component's `requires` is not held against the schema.** A component naming a type no schema declares is trimmed
with the same message as one naming a type this corpus declined. The two mean different things: one is a typo and the
other is a decision. Nothing reports the first.

**The export is copied whole.** A component surviving trimming pulls in the entire export, including types no surviving
component names. That costs nothing today, because the trim and the export are driven by the same adoption. It is worth
reopening for a corpus exporting many types where a plugin reads one.
