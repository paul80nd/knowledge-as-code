---
id: std-PLUGIN
type: standard
tier: normative
status: active
implements: [ eng:pol-AGNT.ACCESS, eng:pol-AGNT.CONFID, eng:pol-AGNT.UNPROV, eng:pol-KNOW.COPY ]
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

## Conformance checklist

- [ ] Every instruction a skill gives can be followed by a session that can read a file and nothing else.
- [ ] Every path a skill names opens on `${CLAUDE_PLUGIN_ROOT}`.
- [ ] Every field a skill describes names its JSON type, and agrees with a line of a real export.
- [ ] Every skill names the parts file of the type it requires, and no other.
- [ ] Every skill says what its corpus did not carry, from `types` in `manifest.json`.
- [ ] Every skill directory and hook directory in the plugin tree is declared under `metadata.components`.
- [ ] Every `requires` entry names a type at the shape version it reads, bare where it opens none of that type's
      files, and empty where the component reads no export.
- [ ] No skill offers to write to the export, and each names the issue tracker instead.
- [ ] `bundle.json` names every component that left, and the type that left it out.

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
reads no export at all declares nothing, and travels with the components it supports.
`docs/design/plugin.md` carries how `kac bundle` acts on all three.

What CI reaches is narrow. `round-trip.sh` installs the plugin, asks each skill the question that skill describes, and
greps each `SKILL.md` for the parts file its component requires. Everything else above is a reviewer's, and an
undeclared component directory is the gap worth naming: `bundle.json` lists what was declared, so nothing looks for
what was not.

## Sources and further reading

- **Normative.** [Claude Code plugin manifest] is the schema `plugin.json` is validated against, and `metadata` is
  where this repository's own keys sit inside it.

## Changelog

- 2026-09-07: initial version.

[Claude Code plugin manifest]: https://json.schemastore.org/claude-code-plugin-manifest.json
