---
id: std-PLUGIN
type: standard
tier: normative
status: active
implements: [ eng:pol-AGNT.ACCESS, eng:pol-AGNT.CONFID, eng:pol-AGNT.PROV, eng:pol-AGNT.SELFVER,
  eng:pol-AGNT.UNPROV, eng:pol-KNOW.COPY ]
verified-by: [ ctl-0008 ]
applies-to:
  - all
review-by: "2027-09-07"
owner: human:paul.law
tags: [ agents, export, plugin, skills ]
---

# The plugin carries the corpus, and a skill answers from what travelled

`Standard: std-PLUGIN` `ACTIVE`

## Summary

Every corpus here publishes a Claude Code plugin: a frozen copy of its export, and the skills that read it.
`template/.plugin/` holds those skills, `plugin.json` says which of them travel, and `kac bundle` assembles the two. A
skill answers from the export sitting beside it, using no more than a session's ability to read a file, and says
plainly what did not travel. Where the answer is wrong, the skill writes back to the repository the export came from,
because the copy it is reading cannot be changed.

## Rules

### A skill assumes only that the session can read a file

- A skill **MUST** state the property a search needs, and not the name of the tool that performs it.
- A skill **MUST** give a session holding no such tool a route to the same answer.
- A skill **MUST** say where that route behaves differently on Windows.
- A skill **MUST NOT** require a shell, an interpreter or a runtime on the reader's machine.
- A skill **MUST NOT** require a network call to answer from the export.
- A skill **MUST NOT** ask for access beyond reading the files under `${CLAUDE_PLUGIN_ROOT}`.
- Every path a skill names **MUST** open on `${CLAUDE_PLUGIN_ROOT}`.

_**Covers:** `eng:pol-AGNT.ACCESS`_

### A skill names the type of every field it describes

- A skill describing a field **MUST** name that field's JSON type.
- A skill **MUST** say that a field holding no value is present and `null`, where the export writes it that way.
- A skill **MUST** name each key that is absent rather than `null`.
- A skill **MUST NOT** describe a string of markdown as a list.
- Every field description **MUST** agree with a line of a real export.
- A skill **MUST** name the parts file of the type its component declares, and no other type's.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-AGNT.UNPROV`_

### A skill says what its corpus did not carry

- A skill **MUST** read `types` in `manifest.json` before it says what is missing.
- A skill **MUST** name what stayed behind, and send the reader to the published record for it.
- A skill **MUST NOT** name a fixed set of absences.
- A skill **MUST** name the corpus and every entry in `sources` it searched, where nothing matched.
- A skill **MUST** say that an empty search states only that the estate has not written this down.
- A skill **MUST NOT** read an obligation out of a record about something else.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-AGNT.UNPROV`_

### `plugin.json` declares every component and the types it reads

- Every skill directory and hook directory in the plugin tree **MUST** appear under `metadata.components`.
- A component **MUST** name in `requires` every record type it opens a file of, at the shape version it reads, as
  `glossary@1`.
- A component that needs a type present and opens none of its files **MUST** name that type bare.
- A component that reads no export at all **MUST** declare `requires` empty.
- A component that reads no export and supports no other component **MUST** declare `standalone` true.
- A component entry **MUST** carry a `note` saying what it reads.
- `metadata.corpusRoot` **MUST** name the directory every skill in the tree addresses the export through.
- A component naming a type the export does not carry **MUST** leave the plugin, with every file under its declared
  path.
- `bundle.json` **MUST** name every component that left, and the type that left it out.

_**Covers:** `eng:pol-AGNT.UNPROV`_

### The bundle carries a frozen export, and nothing writes to it

- The export inside a plugin **MUST** be the bytes `kac export` wrote.
- A skill **MUST NOT** write to any file under `${CLAUDE_PLUGIN_ROOT}`.
- A skill **MUST** report a `status` of `draft`, `deprecated` or `superseded` on the record it quotes.
- A skill **MUST** report a `reviewBy` that has passed.
- A skill **MUST** quote `generatedAt` from `manifest.json` alongside either.
- A skill **MUST** quote the wording the export carried, and link the record for the rest.
- The words a skill quotes **MUST** reach it through the export, and never through a second copy kept for agents.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-KNOW.COPY`_

### A skill writes back to the repository the export came from

- A skill **MUST** send a correction to the repository the record's publishing block addresses.
- A skill **MUST** raise it as an issue or a work item.
- A skill **MUST** use the client that already authenticates to that platform.
- A skill **MUST NOT** offer to edit the installed copy.
- A skill **MUST NOT** ask the reader for a credential.

_**Covers:** `eng:pol-AGNT.ACCESS`, `eng:pol-KNOW.COPY`_

### A finding an agent files carries the shape a discovery takes

- A finding **MUST** open its body with a fenced block whose info string is `yaml kac-finding`.
- That block **MUST** open on `corpus`, naming the corpus the finding is about.
- The block **MUST** then carry `id`, `source`, `confidence`, `expires` and `provenance`, under the names and in the
  order the `discoveries` template writes them.
- The block **MAY** carry `applies-to` and `tags` after those.
- The block **MUST NOT** carry any other key.
- `source` **MUST** be `session`.
- `confidence` **MUST** be `unverified` where the filing session's own account is the only evidence.
- `provenance` **MUST** name the agent, the session it ran in, the repository, the commit it read, and what it was
  doing.
- Where the agent cannot reach one of those, `provenance` **MUST** name it as unreached.
- The body **MUST** carry `## What I saw`, `## Context` and `## Why it might matter`, in that order.
- A finding **MUST NOT** put anything between the block and the first of those headings.
- The title **MUST** be the observation in one line, written as the record's H1 would be.
- A skill **MUST** mark the issue `kac:finding`, using whatever the platform calls a label.
- Where the platform refuses a mark it does not already hold, a skill **MUST** file the finding unmarked.
- A skill **MUST** say that it filed one unmarked.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-AGNT.PROV`, `eng:pol-AGNT.SELFVER`_

## Examples

```
✅ Good
**Search the file with something that needs no shell.** A Grep tool is the one to reach for, and it runs on
every platform. Where the session holds none, `grep -i` reads a line-delimited JSON file on Linux and on
macOS. A Windows reader may have neither `grep` nor the same quoting, so say which route you took.

| Field         | Type                     | What it holds                                             |
|---------------|--------------------------|-----------------------------------------------------------|
| `obligations` | string or null           | every bullet under that heading, as one block of markdown |
| `seeAlso`     | list of strings, or null | the rules of other standards this one points at           |

**A key with no value is `null`, and the key is still there.** Test the value rather than the key.

❌ Avoid
**Use your Grep tool, not a shell command.** It runs on every platform and needs no shell.

| Field         | What it holds                                                   |
|---------------|-----------------------------------------------------------------|
| `obligations` | the bullets, in the markdown the standard wrote them in         |
| `seeAlso`     | the rules this one points at, or absent where it points at none |
```

The avoided instruction names a tool the session may not hold, and leaves a reader who holds none choosing between
ignoring it and stopping. The avoided table states no type at all, so a reader iterates a string of markdown one
character at a time. It says a field with no value is absent, and a reader testing for the key finds it every time and
reads nothing.

A finding is the whole of an issue body. `dsc-rider-holds-the-editorconfig` was written straight into the corpus,
and this is the finding it would have arrived as.

`````
✅ Good
Title: Rider does not re-read an .editorconfig changed from a shell

```yaml kac-finding
corpus: example-dogfooding
id: dsc-rider-holds-the-editorconfig
source: session
confidence: unverified
expires: "2026-12-07"
provenance: >
  Claude Code, in session 01J8ZC4M6QK2XR7VN0PYWTB3AE, in paul80nd/knowledge-as-code at 24dcea21,
  editing .editorconfig from a shell while the developer had the repository open in Rider.
tags: [ editorconfig, formatting, rider ]
```

## What I saw

A key edited in `.editorconfig` from the terminal changed nothing about how Rider formatted a file. Opening
`.editorconfig` in the IDE made the same change take effect. Flipping a key with an obvious effect was the
control.

## Context

Seen once, on one machine. The Rider version was not recorded.

## Why it might matter

A session changes `.editorconfig`, sees no difference in the IDE, and concludes the edit did nothing. The
next move is usually to change something else that was already right.

❌ Avoid
Title: An observation about Rider

While working on the wrap width I noticed that Rider seems to cache `.editorconfig`, which could be worth
looking into. Fairly confident about this one. Raised by an agent session.
`````

The avoided body states the same observation and hands a harvester nothing to copy. There is no id to write the
filename from, no `expires` to let the claim lapse, and no commit anybody can go and read. Its title gives the subject
alone, so a reader scanning the issue list learns nothing. "Fairly confident" is not a value `confidence` takes, and a
session vouching for itself is what `eng:pol-AGNT.SELFVER` refuses.

## Conformance checklist

- [ ] Every instruction a skill gives can be followed by a session that can read a file and nothing else.
- [ ] Every path a skill names opens on `${CLAUDE_PLUGIN_ROOT}`.
- [ ] Every field a skill describes names its JSON type, and agrees with a line of a real export.
- [ ] Every skill names the parts file of the type it requires, and no other.
- [ ] Every skill says what its corpus did not carry, from `types` in `manifest.json`.
- [ ] Every skill directory and hook directory in the plugin tree is declared under `metadata.components`.
- [ ] Every `requires` entry names a type at the shape version it reads, bare where it opens none of that type's
      files, and empty where the component reads no export.
- [ ] Every component reading no export declares `standalone` where it supports nothing, and leaves it off where it
      supports the components that do.
- [ ] No skill offers to write to the export, and each names the issue tracker instead.
- [ ] `bundle.json` names every component that left, and the type that left it out.
- [ ] Every finding opens on a `yaml kac-finding` block, carrying `corpus`, then the `discoveries` keys the rule
      names, and nothing else.
- [ ] Every finding's `provenance` names the agent, the session, the repository, the commit and what it was doing, or
      names which of those it could not reach.
- [ ] No finding is dropped, or held back, because the target holds no `kac:finding` label.

## Rationale and provenance

A skill is the one document here written for a reader we cannot see. It runs in somebody else's session, on somebody
else's machine, against an export taken on a day that has passed. Every rule above follows from one of those three.

The session is why a skill names a property rather than a tool. A session with no Grep tool met a **MUST**-shaped
instruction naming one, and the only readings left were to ignore it or to stop. The machine is why nothing here asks
for a shell: the promise at the top of each skill is that there is nothing to install and nothing to run, and a
Windows reader with no `grep` is who that promise is for.

The export being frozen is why a skill states a date and a status. An export reads the same however old it is, so the
skill is the only thing that can say how old. It is also why a correction leaves through the issue tracker: the
installed copy sits in a cache of the consumer's own, so an agent that edited it would change nothing anybody else
reads.

`requires` carries three states, and the manifest rules keep them apart. A component that opens a type's files names
the shape it reads them at, so a moved key stops the build rather than reaching a reader. A component that needs the
type present and opens none of its files names it bare, which is what the breadcrumb hook does. A component that
reads no export at all declares nothing.

That third state answers two different questions, so `standalone` separates them. `corpus-retrieval` reads no export
and exists for the lookup skills, so a bundle keeping it after the last of them left would ship a skill nothing can
reach. `raise-finding` reads no export and serves whoever is holding the plugin, and a corpus holding no record at all
is the one a session most needs a route to report. Declaring it standalone is what keeps that route open.
`docs/design/plugin.md` carries how `kac bundle` acts on all of them.

A finding is a discovery that has not landed yet. An observation is worth the same whether a session writes it into a
corpus it can edit or files it against one it cannot. So the issue carries the fields the record will need, under the
names the type gives them. A harvester copies the block into frontmatter, and the three headings into sections. Every
key it would otherwise read out of prose is a key it would sometimes read wrongly. `expires` is the one that costs
most, because an observation nobody dated stays on unchallenged, and `eng:pol-AGNT.CONFID` exists to stop that.

One required field never travels. `owner` says who is answerable for a record, and the receiving corpus decides that. A
filer on somebody else's repository would be guessing at it. So the harvester fills `owner` in, along with the keys the
type fixes.

The label is how a person filters, and never what a harvester selects on. `gh issue create` refuses a label the target
repository does not hold. A corpus published from somebody else's repository holds whatever labels its maintainer
chose. A skill that stopped there would lose the observation to a missing string. The block in the body is the
contract. This repository's own `dogfood` label answers a different question, which is how something was found, and
both marks sit on one issue.

What CI reaches is narrow. `round-trip.sh` installs the plugin, asks each skill the question that skill describes, and
greps each `SKILL.md` for the parts file its component requires. Everything else above is a reviewer's, and two gaps
are worth naming. An undeclared component directory travels undeclared, because `bundle.json` lists what was declared
and nothing looks for what was not. A filed finding is read by nobody here. The issue lands on a repository this build
never opens, so the shape holds for as long as the skill writing it holds to this standard.

## Sources and further reading

- **Normative.** [Claude Code plugin manifest] is the schema `plugin.json` is validated against, and `metadata` is
  where this repository's own keys sit inside it.

## Changelog

- 2026-09-08: a component reading no export says whether it supports the others, so one serving the reader
  survives a bundle that trimmed every lookup.
- 2026-09-08: added the shape a finding an agent files carries, and the label duty beside it.
- 2026-09-07: initial version.

[Claude Code plugin manifest]: https://json.schemastore.org/claude-code-plugin-manifest.json
