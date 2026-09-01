# The plugin bundle

How an export becomes something a consumer can install, what decides which parts of it ship, and why the assembly reads
the export rather than the corpus.

A **corpus** is one repository of knowledge records kept in git. An **export** is what
[`export`](../cli/export.md) writes into `.dist/export/`, as data an agent reads. A **plugin tree** is the skills and
hooks a corpus keeps beside its records, at `.plugin/`. A **component** is one skill or hook inside that tree.

## `bundle` never loads the corpus

`Corpus.Load` is never called. A run reads one key out of `.corpus.yaml`, saying where the plugin tree is. Everything
else a bundle decides is a fact about the export it was handed.

That is why the two are separate commands. They fail differently: an export is wrong about the corpus, and a bundle is
wrong about what it shipped. They are proved differently too. The export has a committed golden of its output, and the
bundle has an assertion that it did not touch that output.

## Where the plugin tree is read from

`.plugin/` at the corpus root, unless `plugin.from` in [`.corpus.yaml`](../corpus-descriptor.md) names another folder.
Several corpora in one repository can then share a single tree instead of holding a copy each, and `update` withholds
the shared half from every corpus that says so.

The manifest is the exception. It carries the name the plugin installs under, so it is read from the corpus and never
from the shared tree, and a corpus holding none is refused. A file the corpus writes beside it wins over the shared
tree's copy of the same path, which is how one skill is overridden without giving up the rest.

## What stops a run before it writes

Each of these ends the run with the reason and nothing written:

* a plugin tree with no manifest
* a `plugin.from` naming a folder that is not there
* a manifest that is not JSON, or that states no name, or no `corpusRoot`, or a `corpusRoot` the plugin tree already
  uses
* an export with no manifest
* an export whose `formatVersion` is not the one this build reads
* a component reading a type at a shape version the export does not carry

The alternative to stopping is a plugin assembled around a missing answer, which installs and fails later somewhere
less obvious.

### A `corpusRoot` collision loses a file in silence

The export lands under that directory inside the plugin, so a clash has one side overwrite the other. Whichever won,
the loser would be missing from an artefact nobody reviews.

### Both format versions are named, because the next move differs

An export behind the tool is rebuilt. An export ahead of it says the tool is the stale half. `.dist/export/` is
untracked and outlives the run that wrote it, so a bundle built after a pull is the ordinary way to meet an export this
tool did not ship beside.

### A shape mismatch means the component's keys have moved

The type is there and its files have moved, so the component would ship, install, and read keys that are gone. Trimming
would hide that behind a plugin doing less. [The export format](export.md) says what a shape version covers and what
moves it.

## A component travels only where the export carries its types

A component declares under `metadata.components` which record types it reads, and it travels only where the export
carries every one of them. What that catches is the skill that finds nothing. To whoever asked it a question, such a
skill reads exactly like a corpus that does not define the term.

An entry may name the shape it reads the type at, as `glossary@1`. A bare `glossary` asks for the type and opens none
of its files, which is what the breadcrumb hook does. Both trim the same way where the export carries no glossary at
all.

### `requires` is answered by the merged export

An export carries what its corpus consumes, so a type can reach it from a corpus above this one.
`requires` asks what the export carries and never what the corpus adopted, so a corpus holding no glossary of its own,
consuming one that does, ships the glossary skill and the breadcrumb hook.

That is the wanted answer. The terms are in the export, and a skill that reads them finds them.
Nothing special-cases it, because the trim already reads the artefact rather than the folder.

### What a trim takes with it

A trimmed component's files leave with it. A path under no component's is unconditional and travels whatever the corpus
adopted, so a README or a licence in the plugin tree needs no declaration. A component may be a directory or a single
file, so the trim matches the declared path itself as well as anything beneath it, on whole segments: `skills/a` does
not take `skills/ab` with it.

### Each component reads its own type's parts file

A corpus adopting several types that export ships a skill for each, over one export. The export writes one parts file
per type, and each skill addresses the one belonging to the type it declares in `requires`. A skill naming another
type's parts file answers from one written for a different question, and whoever asked cannot tell. `bundle` does not
read a component's files, so nothing here catches that. The round trip does, over the installed copy.

### Trimming everything warns and still builds

Refusing would leave a corpus unable to produce the thing that would have told it why. The empty plugin is itself the
report: it installs, does nothing, and `bundle.json` beside it names each component that was dropped and the type it
needed.

## Each command replaces its own directory whole

`export` owns `.dist/export/`, `bundle` owns `.dist/plugin/` and [`pack`](../cli/pack.md) owns `.dist/package/`. Each
deletes before it writes, because an artefact nobody reviews must not keep a file that nothing backs any more. That
rule stays local only while no command deletes another's tree, which is why the export is a named subtree and not the
root. `Dist` is the one statement of the layout.

The export is copied in, and therefore exists twice. Once at `.dist/export/` as `export` wrote it, and once inside the
plugin under the directory `metadata.corpusRoot` names. That duplication is intended. An installed plugin is copied into
a cache of its own, so a path outside the plugin root resolves nowhere at runtime, and `export` has to stay runnable and
proved without `bundle` having run. The copy is what keeps them independent.

`bundle` never edits what it copies. The export travels byte for byte. The plan carries bytes rather than text, so a
copy cannot acquire an opinion about encoding or line endings on the way through. A difference between the two copies is
a defect, and the golden fixture asserts their equality directly.

## The breadcrumb

### What the breadcrumb says, and why it stops there

A `SessionStart` hook injects a few lines into every session. They say which corpus is installed, how many entries it
holds, which records cover them, and which skill to ask. That is the whole of its job.

An agent never asks for a glossary, because it does not know a word is ambiguous. So the breadcrumb exists to create the
question rather than to answer it. A longer one would be paid for by every session that had none to ask. That is also
why a line names at most six things: the renderer holds the bound itself, so a corpus keeping three hundred records pays
what one keeping three pays. A type over the bound has its first names carried and the rest given as a count, because a
list cut short in silence would read as the whole of what the type covers.

A component is named in the last line where its manifest entry says `"announce": true`, and left out otherwise. The
default is out, because most skills need no introduction: you already know to ask what a policy commits you to. What
earns the line is a skill whose question a session would never think to put.

That last line warns against answering from memory and states no question of its own. Each skill that can announce
asks a different one, so naming a question would name a record type, and a corpus shipping the standards skill and no
glossary would be warned about words alone.

### A type line names the corpus whose records it counts

An export carries what its corpus consumes, so one type can hold several corpora's records. Each type gets a line for
the installing corpus's own records and another for each corpus it consumes, carrying that corpus's shortcode and that
corpus's count:

```text
standards. 24 entries across 8 records: Card details reach the PSP and never reach us, One order pays once, …
standards (from eng). 37 entries across 12 records: An interface is described by a contract in the repository, …
```

The installing corpus gets no line of its own where it wrote none of the type, which is what a corpus adopting a type
purely to inherit it looks like. One count under the installing corpus's name would send a reader looking for records
nobody there wrote, and a payments developer told the session holds 202 policy clauses goes looking for a payments
policy.

### The breadcrumb is rendered at build time

Everything it states is a fact about the export sitting inside the plugin, and an installed plugin's export does not
change between builds. So `bundle` writes the text once and the hook is one `cat`. Nothing on the consumer's machine is
asked for a JSON parser, an interpreter or a runtime.

The rendered file travels with the directory that prints it and nowhere else. A corpus shipping no hook has nothing to
read it, and a corpus whose hook was trimmed would otherwise keep a file describing a component that left. Both fall
out of asking which files survived, so no corpus has to declare that a generated file belongs to a component.

### The render names no record type

The counts, the record names, the shortcodes and the skill to ask are read off the export and off the surviving
components. So a corpus adopting a type this tool has never heard of gets a breadcrumb about it with no line changing,
and so does a corpus consuming one.

The record names do a job a count alone cannot. A corpus keeps one glossary per bounded context, so three names say
which contexts are covered where the number three says only that there are some.



## A hook is copied with its permission bit

One file in the plugin tree is run rather than read. A command copied without that bit is a plugin that installs and
then fails at the first session, with a message about permissions rather than about the corpus.

The bit does not exist on Windows and is not asked for there. A hook ships as a POSIX script and a `.cmd` twin, so a
Windows shell has something to reach. [`bundle`](../cli/bundle.md#known-limits) carries what is still unproved about
that half.

## The plugin's version is the corpus content version

It is read off the export's manifest rather than out of `.corpus.yaml`, so the number on the plugin is the number of
the data inside it. The export **format** version stays where it is, inside the export's own manifest. One says what
the corpus knows and the other says which parser will read it, so stamping the second onto the plugin would describe
the wrong thing confidently.

A corpus stating no content version keeps the version its plugin manifest carried, and the run says so.

## The plugin manifest is edited, not rebuilt

`plugin.json` is forked in the portability manifest, so each corpus owns its own. `bundle` reads it as a DOM, gives it
a new version and the surviving components, then writes it back with every other key exactly as the corpus wrote it.
Mapping it onto a shape known here would delete whatever the corpus had added, without a word.

## The bundle records what it shipped

`bundle.json` at the plugin root carries the plugin, its version, the export it was built around, every component
included and every one trimmed with the reason.

Two corpora running one plugin name may ship different component sets. That is correct, and it makes "does this plugin
do X" unanswerable from outside unless the plugin says. `bundle.json` carries no timestamp and no commit: the export
inside the plugin states both.

## `.dist/` is the marketplace

A marketplace is a directory holding `.claude-plugin/marketplace.json`, and it resolves each plugin's source against
that directory. A source containing `..` is refused. So a marketplace cannot sit beside the plugin and point sideways
at it, and the root is where it goes.

## Why the output is a directory, and why `corpusRoot` is read

**The output is a directory rather than an archive.** A marketplace can point at a path, so there is nothing to unpack
between building and asking the plugin a question.

**`corpusRoot` is read rather than defaulted.** The plugin's skills address the export as
`${CLAUDE_PLUGIN_ROOT}/<corpusRoot>/…` by that name. A default here would be the tool quietly disagreeing with words
the corpus wrote in its own skill, and the disagreement would surface only when somebody asked the installed plugin a
question.

## Where to go next

[`bundle`](../cli/bundle.md) is the command that assembles this.
