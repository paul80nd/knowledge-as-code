# The plugin bundle

This page is for whoever changes `kac bundle`. It says how the command lays out what it writes, what the breadcrumb
hook says, how the plugin manifest is edited, and why a run trims or stops where it does.
[`bundle`](../use/cli/bundle.md) is the reference for what a run reads, ships and refuses.

A **corpus** is one repository of knowledge records kept in git. An **export** is what [`export`](../use/cli/export.md)
writes into `.dist/export/`, as data an agent reads. A **plugin tree** is the skills and hooks a corpus keeps beside its
records, at `.plugin/`. A **component** is one skill or hook inside that tree.

## What `bundle` reads

It never loads the corpus. `Corpus.Load` is not called. A run reads one key out of `.corpus.yaml`, saying where the
plugin tree is. Everything else a bundle decides is a fact about the export it was handed.

That is why the two are separate commands. They fail differently: an export is wrong about the corpus, and a bundle is
wrong about what it shipped. They are proved differently too. The export has a committed golden of its output, and the
bundle has an assertion that it did not touch that output.

## Why a run stops

Stopping is the alternative to a plugin assembled around a missing answer, which installs and fails later somewhere
less obvious.

* **A `corpusRoot` collision.** The export lands under that directory inside the plugin, so a clash has one side
  overwrite the other. Whichever won, the loser would be missing from an artefact nobody reviews.
* **Two format versions.** `.dist/export/` is untracked and outlives the run that wrote it, so a bundle built after a
  pull is the ordinary way to meet an export this tool did not ship beside. The message names both, because the next
  move differs.
* **A shape mismatch.** The component would ship, install, and read keys that are gone. Trimming would hide that behind
  a plugin doing less.

## Why a component is trimmed

A skill that finds nothing reads, to whoever asked it a question, exactly like a corpus that does not define the term.
The trim is what keeps such a skill out. It reads the artefact, not the folder, so a type reaching the export from a
consumed corpus needs no special case.

### Which parts file a skill reads

A corpus adopting several types that export ships a skill for each, over one export. Where that type declares parts, the
export writes one parts file for it and the skill addresses that file. A skill naming another type's parts file answers
from one written for a different question, and whoever asked cannot tell. `bundle` does not read a component's files, so
nothing here catches that. The round trip does, over the installed copy.

A type declaring no parts writes no such file, and its records travel one JSON apiece. `processes` and `controls` are
both that case, each for a reason of its own. A process is followed whole and in order, so a step addressed alone would
be quoted out of the sequence that makes it safe. A control has no repeated heading to lift out: it names the rules it
checks, says what performs the check, and says where the check stops. Those are three different answers, where a part is
one answer repeated. Each skill reads its own directory, and `manifest.json` reports `partsFile` as null for both.

### A component naming no type

`corpus-retrieval` and `request-deviation` each need a record a lookup found, and no single type owns either job. A
plugin shipping either alone would carry a skill supporting nothing a reader can reach, and `bundle.json` would report a
component included where the run left nothing to answer a question.

`raise-finding` supports nothing and serves the reader, so it is standalone. A corpus with no records at all is the one
a session most needs a route to report, so trimming that skill would remove the route where a session most needs it.
Standalone says nothing about the type trim. A standalone component naming a type the export left out is trimmed like
any other.

`harvest-findings` is the other half of that round trip and is not a component. It triages the issues already filed and
drafts the record one of them asks for, and both invocations write: to the tracker, and to the records themselves. The
reader is the corpus's own maintainer, holding the source. So the skill travels into the corpus's working tree under
`.claude/skills/`, where [`new`](../use/cli/new.md) writes it and [`update`](../use/cli/update.md) keeps it current, and
no plugin ships it. A consumer install would ship a skill that can only refuse.

### When every component is trimmed

The run builds, because refusing would leave a corpus unable to produce the thing that would have told it why. The
empty plugin is itself the report.

## The `.dist/` layout

Each command replaces its own directory whole. `export` owns `.dist/export/`, `bundle` owns `.dist/plugin/` and
[`pack`](../use/cli/pack.md) owns `.dist/package/`. Each deletes before it writes, because an artefact nobody reviews
must not keep a file that nothing backs any more. That rule stays local only while no command deletes another's tree,
which is why the export is a named subtree and not the root. `Dist` is the one statement of the layout.

The export is copied in, and therefore exists twice. Once at `.dist/export/` as `export` wrote it, and once inside the
plugin under the directory `metadata.corpusRoot` names. That duplication is intended. An installed plugin is copied into
a cache of its own, so a path outside the plugin root resolves nowhere at runtime, and `export` has to stay runnable and
proved without `bundle` having run. The copy is what keeps them independent.

`bundle` never edits what it copies. The export travels byte for byte. The plan carries bytes rather than text, so a
copy cannot acquire an opinion about encoding or line endings on the way through. A difference between the two copies is
a defect, and the golden fixture asserts their equality directly.

## The breadcrumb

### What the breadcrumb says

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

That last line warns against answering from memory and states no question of its own. Each skill that can announce asks
a different one, so naming a question would name a record type, and a corpus shipping the standards skill and no
glossary would be warned about words alone.

### A type line

It names the corpus whose records it counts. An export carries what its corpus consumes, so one type can hold several
corpora's records. Each type gets a line for the installing corpus's own records and another for each corpus it
consumes, carrying that corpus's shortcode and that corpus's count:

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
read it, and a corpus whose hook was trimmed would otherwise keep a file describing a component that left. Both fall out
of asking which files survived, so no corpus has to declare that a generated file belongs to a component.

### No record type in the renderer

The counts, the record names, the shortcodes and the skill to ask are read off the export and off the surviving
components. So a corpus adopting a type this tool has never heard of gets a breadcrumb about it with no line changing,
and so does a corpus consuming one.

The record names do a job a count alone cannot. A corpus keeps one glossary per bounded context, so three names say
which contexts are covered where the number three says only that there are some.

## The hook's permission bit

One file in the plugin tree is run rather than read. A command copied without that bit is a plugin that installs and
then fails at the first session, with a message about permissions rather than about the corpus.

The bit does not exist on Windows and is not asked for there. A hook ships as a POSIX script and a `.cmd` twin, so a
Windows shell has something to reach. [`bundle`](../use/cli/bundle.md#known-limits) says what is still unproved about
that half.

## The plugin's version

It is the corpus content version, read off the export's manifest rather than out of `.corpus.yaml`, so the number on the
plugin is the number of the data inside it. The export **format** version stays where it is, inside the export's own
manifest. One says what the corpus knows and the other says which parser will read it, so stamping the second onto the
plugin would describe the wrong thing confidently.

A corpus stating no content version keeps the version its plugin manifest had, and the run says so.

## The plugin manifest

`bundle` edits it rather than rebuilding it. `plugin.json` is forked in the portability manifest, so each corpus owns
its own. `bundle` reads it as a DOM, gives it a new version and the surviving components, then writes it back with every
other key exactly as the corpus wrote it. Mapping it onto a shape known here would delete whatever the corpus had added,
without a word.

## `bundle.json`

At the plugin root, it records what shipped: the plugin, its version, the export it was built around, every component
included and every one trimmed with the reason.

Two corpora running one plugin name may ship different component sets. That is correct, and it makes "does this plugin
do X" unanswerable from outside unless the plugin says. `bundle.json` carries no timestamp and no commit. The export
inside the plugin states both.

## The marketplace

`.dist/` is it. A marketplace is a directory holding `.claude-plugin/marketplace.json`, and it resolves each plugin's
source against that directory. A source containing `..` is refused. So a marketplace cannot sit beside the plugin and
point sideways at it, and the root is where it goes.

## The output directory and `corpusRoot`

**The output is a directory rather than an archive.** A marketplace can point at a path, so there is nothing to unpack
between building and asking the plugin a question.

**`corpusRoot` is read rather than defaulted.** The plugin's skills address the export as
`${CLAUDE_PLUGIN_ROOT}/<corpusRoot>/…` by that name. A default here would be the tool quietly disagreeing with words the
corpus wrote in its own skill, and the disagreement would surface only when somebody asked the installed plugin a
question.

## Where to go next

[`bundle`](../use/cli/bundle.md) is the command that assembles this.
