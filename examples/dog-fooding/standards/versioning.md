---
id: std-VERS
type: standard
tier: normative
status: active
implements: [ eng:pol-KNOW.DOCS, eng:pol-KNOW.SYNC, eng:pol-PIPE.REVERT, eng:pol-TRUS.MUTATE ]
verified-by: [ ctl-0007 ]
applies-to:
  - all
review-by: "2027-09-07"
owner: paul.law
tags: [ compatibility, imports, versioning ]
---

# A version stamp says what it versions, and a move says what changed

`Standard: std-VERS` `ACTIVE`

## Summary

The stamps on this repository's files decide what it publishes and what a corpus consuming it may take. Two are
semantic versions a person moves: `<Version>` in `tooling/kac/kac.csproj`, and `content-version` in a corpus's
`.corpus.yaml`. The rest are counts a tool writes and a reader compares for equality. A count moves where something
reading the file has to respond, so a reader meeting a count above its own stops on it. Every consumer in this
repository takes a producer's move in the same pull request.

## Rules

### A stamp is a semantic version, a count, or a range

- `content-version` in a corpus's `.corpus.yaml` and `<Version>` in `tooling/kac/kac.csproj` **MUST** be written and
  read under [Semantic Versioning 2.0.0].
- `descriptor-version`, `upstream.template-version`, `version:` in `manifest.yaml`, `version:` under `export:` in a
  `.schema/<type>.yaml`, `formatVersion`, `mechanismVersion` and a type's `shapeVersion` **MUST** each be a whole
  number that only increases.
- A count **MUST NOT** be read as a major, a minor or a patch.
- A key spelled `version:` **MUST** version the file or the block holding it.
- `minimum-tool:` in `manifest.yaml` **MUST** name a `<Version>` nuget.org has published.
- A `version:` under `consumes:` **MUST** be an exact version or a caret over one, and no other form.
- A caret **MUST NOT** be read as admitting a prerelease.

_**Covers:** `eng:pol-KNOW.DOCS`_

### What a move of each stamp means

- `content-version` **MUST** be read as a statement about the records rather than about the file.
- Its major **MUST** mark a meaning that changed or a published URL that broke, its minor a record or a rule added,
  and its patch a change of wording.
- A move of `<Version>` **MUST** name a change to `kac` that a user can observe.
- `descriptor-version` **MUST** move where `.corpus.yaml` gains a key, loses one, or has one read differently.
- `version:` in `manifest.yaml` **MUST** move where a corpus has to respond: a file added, removed, renamed, or moved
  between layers.
- `upstream.template-version` **MUST** hold the `version:` of the manifest that corpus last took.
- `formatVersion` and a type's `shapeVersion` **MUST** move where a reader written against the shape before it would
  now be wrong.
- Adding a key to a line, or a file to a type's directory, **MUST NOT** move either of those two.
- `mechanismVersion` in an export's `manifest.json` **MUST** hold that corpus's `upstream.template-version`, and
  **MUST** be null where the corpus states none.
- A rename of `mechanismVersion` **MUST** move `formatVersion`.

_**Covers:** `eng:pol-KNOW.DOCS`_

### A reader meeting a stamp it does not know stops

- `kac bundle` **MUST** refuse an export whose `formatVersion` is not the one that build reads.
- `kac bundle` **MUST** leave out a component whose type the export carries at a `shapeVersion` the component did not
  name.
- A reader that does not know one type's `shapeVersion` **MUST** leave that type alone and read the rest of the export.
- A `kac` older than `minimum-tool:` **MUST** stop on the manifest and name it.
- A `content-version` a reader does not know **MUST NOT** stop that reader.

_**Covers:** `eng:pol-KNOW.DOCS`_

### A producer's move obliges every consumer in the same pull request

- A consuming corpus **MUST** move the `resolved:` lock of its `consumes:` entry in the pull request moving the
  producer's `content-version`.
- That consumer **MUST** move its `version:` range as well wherever the producer sits below 1.0.0 and its minor moved.
- A corpus here **MUST NOT** publish a `content-version` that a committed range in this repository refuses.
- You **MUST** delete `.imports/` in every consumer before you open the pull request.
- You **MUST** repack the producer after that delete.
- You **MUST** run `kac restore` again in every consumer, and read what it reports.
- A `kac restore` over an `.imports/` you did not delete **MUST NOT** be offered as proof.

_**Covers:** `eng:pol-KNOW.SYNC`_

### Which stamps move together, and which move alone

- `content-version` and the `resolved:` lock of every consumer of that corpus **MUST** move in one pull request.
- `version:` in `manifest.yaml` and the `upstream.template-version` of every corpus here **MUST** move in one pull
  request.
- `version:` under `export:` in a `.schema/<type>.yaml` and the `shapeVersion` an export publishes for that type
  **MUST** be one number.
- `descriptor-version`, `formatVersion` and a `shapeVersion` **MUST NOT** oblige any other stamp to move.
- `<Version>` **MUST NOT** move because a corpus's records changed.
- A `content-version` **MUST NOT** move because `kac` changed.

_**Covers:** `eng:pol-KNOW.SYNC`_

### A published version is never replaced

- A published `<Version>` or `content-version` **MUST NOT** be pushed again, replaced or deleted.
- A correction **MUST** ship as a new version.
- The bytes a published version carries **MUST** stay the bytes it published.

_**Covers:** `eng:pol-PIPE.REVERT`, `eng:pol-TRUS.MUTATE`_

## Examples

```
✅ Good
# examples/engineering/.corpus.yaml
content-version: "0.11.0"

# examples/dog-fooding/.corpus.yaml, in the same pull request
consumes:
  - corpus: example-engineering
    version: ^0.11.0
    resolved: "0.11.0"

❌ Avoid
# examples/engineering/.corpus.yaml
content-version: "0.11.0"

# examples/dog-fooding/.corpus.yaml, left alone
consumes:
  - corpus: example-engineering
    version: ^0.10.0
    resolved: "0.10.0"
```

Below 1.0.0 a caret stops at the next minor, so `^0.10.0` admits nothing from 0.11.0 upward. `kac restore` on a clean
checkout then fails, naming a version it could not find. The local run passes, because `.imports/` is untracked and a
restore keeps a folder already holding 0.10.0.

## Conformance checklist

- [ ] Every corpus whose records changed has moved its `content-version`, at the major, minor or patch that change
      earns.
- [ ] `<Version>`, where it moved, names a change to `kac` a user can observe, and did not move because a record
      changed.
- [ ] Every consumer of a corpus whose `content-version` moved has moved its `resolved:` lock.
- [ ] Every consumer of a producer below 1.0.0 whose minor moved has moved its `version:` range as well.
- [ ] `.imports/` is deleted in `examples/payments` and `examples/dog-fooding`, the producer repacked, and
      `kac restore` run again. That is what CI sees, because `.imports/` is untracked.
- [ ] `version:` in `manifest.yaml` has moved where a corpus has to respond, and every `upstream.template-version`
      holds it.
- [ ] A type whose exported files changed shape has moved `version:` under `export:` in its `.schema/<type>.yaml`.
- [ ] `formatVersion` has moved where a reader of the export envelope would now be wrong.
- [ ] No published version has been replaced, and every correction ships as a new version.

## Rationale and provenance

`content-version` is a notification and never a gate. Nothing refuses to load because it moved, so a reader that does
not know the number reads on. The counts are the opposite: each moves exactly where the shape a reader was written
against changed meaning underneath it, and reading on would be reading something else. `docs/design/export.md` carries
the argument for splitting `formatVersion` from a `shapeVersion`, which is that one number across every type would stop
a reader over a change to a type it never opens.

The caret is where this bites. `kac` takes two range forms and no more: an exact version, and a caret over one. Above
1.0.0 a caret runs to the next major, because a major above zero promises that nothing below it changed meaning. Below
one there is no such promise, so the minor carries it. A producer that moves its minor and leaves its consumers alone
publishes something they have already said they will not take.

`mechanismVersion` is the descriptor's `upstream.template-version` published under the name that key used to carry. It
adds nothing a corpus does not already state, and renaming it in the export is what `formatVersion` exists to announce.

When a stamp moves in the delivery flow, and how a version reaches a registry, are [std-CI]'s. This standard reaches
what the move means. The `glossary@1` a component names in `plugin.json` is [std-PLUGIN]'s, and a pin on a package
`kac` itself takes is [std-CONFIG]'s. The version on an assembled plugin is the export's `contentVersion` read back, so
it states nothing the rules above have not already bound.

**A reviewer reads most of the rules above.** [ctl-0007] runs `kac validate`, which fails a declared import that was
never restored and warns where a newer version sits inside the declared range. The caret trap surfaces there as
information rather than a warning, because a corpus that capped itself on purpose is reporting a decision. Nothing
counts a stamp that should have moved and did not, nothing compares a `shapeVersion` against the files it stamps, and
nothing reads `<Version>` against what a user of `kac` can observe.

## Sources and further reading

- **Normative.** [Semantic Versioning 2.0.0] is the grammar `<Version>` and `content-version` are written under, and
  the ordering a range resolves by.
- **Informative.** [npm semver ranges] defines the caret this repository borrowed two forms from, including where it
  stops below 1.0.0.

## Changelog

- 2026-09-07: initial version, taking the stamp semantics and the consumer-repointing rules from [std-CI].

[Semantic Versioning 2.0.0]: https://semver.org
[ctl-0007]: ../controls/0007-corpus-validation.md
[npm semver ranges]: https://github.com/npm/node-semver#ranges
[std-CI]: workflows.md
[std-CONFIG]: configuration.md
[std-PLUGIN]: plugin.md
