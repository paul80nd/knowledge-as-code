---
id: std-VERS
type: standard
tier: normative
status: active
implements: [ eng:pol-KNOW.DOCS, eng:pol-KNOW.SYNC, eng:pol-PIPE.REVERT, eng:pol-TRUS.MUTATE ]
verified-by: [ ctl-content-version-tests, ctl-corpus-validation ]
applies-to:
  - all
review-by: "2027-09-07"
owner: human:paul.law
sources:
  - { resource: https://github.com/paul80nd/knowledge-as-code/issues/616 }
tags: [ compatibility, imports, versioning ]
---

# A version stamp says what it versions, and a move says what changed

`Standard: std-VERS` `ACTIVE`

## Summary

The stamps on this repository's files decide what it publishes and what a corpus consuming it may take. Two are
semantic versions a person moves: `<Version>` in `tooling/kac/kac.csproj`, and `content-version` in a corpus's
`.corpus.yaml`. The rest are counts a tool writes and a reader compares for equality. A count moves where something
reading the file has to respond, so a reader that finds a count above its own stops there. Every consumer in this
repository takes a producer's move in the same pull request.

## Rules

### FORMS: A stamp is a semantic version, a count, or a range

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

### MEANING: What a move of each stamp means

- `content-version` **MUST** be read as a statement about what the corpus publishes rather than about the file.
- What a corpus publishes **MUST** be read as its export, and never as the repository its records are readable in.
- A change confined to a section a record's type leaves out of `export:` **MUST NOT** move `content-version`.
- Its major **MUST** mark a meaning that changed or a published URL that broke, its minor a record or a rule added,
  and its patch a change of wording.
- `content-version` **MUST** move where a change reaches a skill, a hook or the manifest its bundle includes. Its patch
  **MUST** mark a change of wording, and its minor a change to what the component tells a reader to do. `kac bundle`
  stamps the plugin with it, so a reader takes a changed skill only where it moves.
- A change to a field's accepted value format, its name, or its removal **MUST** move `content-version`'s major where
  the change reaches a record whose type names that field under `export:`, and its patch where it reaches none.
- A corpus below 1.0.0 **MUST** move its minor where the rule above asks for its major, because reaching 1.0.0 is a
  statement about that corpus's stability rather than about the change in front of you.
- A field added **MUST** move `content-version`'s minor where a record's type names it under `export:`, and its patch
  where none does.
- A move of `<Version>` **MUST** name a change to `kac` that a user can observe.
- `descriptor-version` **MUST** move where `.corpus.yaml` gains a key, loses one, or has one read differently.
- `version:` in `manifest.yaml` **MUST** move where a corpus has to respond: a file added, removed, renamed, or moved
  between layers.
- `upstream.template-version` **MUST** be the `version:` of the manifest that corpus last took.
- `formatVersion` and a type's `shapeVersion` **MUST** move where a reader written against the shape before it would
  now be wrong.
- Adding a key to a line, or a file to a type's directory, **MUST NOT** move `formatVersion` or a `shapeVersion`.
- `mechanismVersion` in an export's `manifest.json` **MUST** be that corpus's `upstream.template-version`, and
  **MUST** be null where the corpus states none.
- A rename of `mechanismVersion` **MUST** move `formatVersion`.

_**Covers:** `eng:pol-KNOW.DOCS`_

### UNKNOWN: A reader meeting a stamp it does not know stops

- `kac bundle` **MUST** refuse an export whose `formatVersion` is not the one that build reads.
- `kac bundle` **MUST** leave out a component whose type the export includes at a `shapeVersion` the component did
  not name.
- A reader that does not know one type's `shapeVersion` **MUST** leave that type alone and read the rest of the export.
- A `kac` older than `minimum-tool:` **MUST** stop on the manifest and name it.
- A `content-version` a reader does not know **MUST NOT** stop that reader.

_**Covers:** `eng:pol-KNOW.DOCS`_

### CONSUME: A producer's move obliges every consumer in the same pull request

- A consuming corpus **MUST** move the `resolved:` lock of its `consumes:` entry in the pull request moving the
  producer's `content-version`.
- That consumer **MUST** move its own `content-version` wherever its `resolved:` lock moves, by the part the producer
  moved, because its export now publishes what the producer changed.
- That consumer **MUST** move its `version:` range as well wherever the producer is below 1.0.0 and its minor moved.
- A corpus here **MUST NOT** publish a `content-version` that a committed range in this repository refuses.
- You **SHOULD** delete `.imports/` in every consumer before you open the pull request, unless you leave the proof to
  the pull request's CI run.
- You **SHOULD** repack the producer after that delete, unless you leave the proof to the pull request's CI run.
- You **SHOULD** run `kac restore` again in every consumer, unless you leave the proof to the pull request's CI run.
- You **SHOULD** read what each of those restores reports.
- A `kac restore` over an `.imports/` you did not delete **MUST NOT** be offered as proof.

_**Covers:** `eng:pol-KNOW.SYNC`_

### TANDEM: Which stamps move together, and which move alone

- `content-version`, the `resolved:` lock of every consumer of that corpus, and each of those consumers' own
  `content-version` **MUST** move in one pull request.
- `version:` in `manifest.yaml` and the `upstream.template-version` of every corpus here **MUST** move in one pull
  request.
- `version:` under `export:` in a `.schema/<type>.yaml` and the `shapeVersion` an export publishes for that type
  **MUST** be one number.
- `descriptor-version`, `formatVersion` and a `shapeVersion` **MUST NOT** oblige any other stamp to move.
- `<Version>` **MUST NOT** move because a corpus's records changed.
- A `content-version` **MUST NOT** move because `kac` changed.

_**Covers:** `eng:pol-KNOW.SYNC`_

### FINAL: A published version is never replaced

- A published `<Version>` or `content-version` **MUST NOT** be pushed again, replaced or deleted.
- A correction **MUST** ship as a new version.
- The bytes a published version contains **MUST** stay the bytes it published.

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
restore keeps a folder that already contains 0.10.0.

## Conformance checklist

- [ ] Every corpus whose export changed, or whose bundle includes a skill, a hook or a manifest that changed, has
      moved its `content-version`, at the major, minor or patch that change earns.
- [ ] Where a field's value format or name changed, or a field was added or removed, the `export:` block of every type
      it reached decided which component that corpus moved.
- [ ] `<Version>`, where it moved, names a change to `kac` a user can observe, and did not move because a record
      changed.
- [ ] Every consumer of a corpus whose `content-version` moved has moved its `resolved:` lock.
- [ ] Every consumer whose `resolved:` lock moved has moved its own `content-version`, by the part its producer moved.
- [ ] Every consumer of a producer below 1.0.0 whose minor moved has moved its `version:` range as well.
- [ ] No `kac restore` over an `.imports/` nobody deleted is offered as proof.
- [ ] `version:` in `manifest.yaml` has moved where a corpus has to respond, and every `upstream.template-version`
      matches it.
- [ ] A type whose exported files changed shape has moved `version:` under `export:` in its `.schema/<type>.yaml`.
- [ ] `formatVersion` has moved where a reader of the export envelope would now be wrong.
- [ ] No published version has been replaced, and every correction ships as a new version.

## Rationale and provenance

`content-version` is a notification and never a gate. Nothing refuses to load because it moved, so a reader that does
not know the number reads on. A count works the other way. It moves exactly where the shape a reader was written against
changed meaning, and reading on would be reading something else. `docs/reference/export.md` gives the argument for
splitting `formatVersion` from a `shapeVersion`: one number across every type would stop a reader over a change to a
type it never opens.

A field that reaches the export is data a consumer parses. A field that stays behind is a fact about this corpus's
stewardship, and nothing downstream reads it. That is why the `export:` block decides which stamp a change to a field
moves, and why one such change lands differently in two corpora. `owner` is the case that shows it: `deviations` is the
one type whose export includes the field, for the reason `.schema/deviations.yaml` gives. Change how `owner` is
written, and a corpus with a deviation has moved something a consumer reads. A corpus with none has not.

The caret decides what a producer's move costs a consumer. `kac` takes two range forms and no more: an exact version,
and a caret over one. Above 1.0.0 a caret runs to the next major, because a major above zero promises that nothing
below it changed meaning. Below one there is no such promise, so the caret stops at the minor instead. A producer that
moves its minor and leaves its consumers alone publishes something they have already said they will not take.

A consumer's export includes what it inherits, so a lock that moves changes what the consumer publishes. The lock is
the trigger, and a change to the inherited records is not, so the rule and its test read the same line of
`.corpus.yaml`. Where the consumer's own records changed in the same pull request, one move covers both, at the larger
part. A consumer below 1.0.0 moves its minor where that part is a major, as it would for any other move. A producer
that moves for a bundled skill alone still obliges its consumers to move, with nothing new in their export. A
`content-version` a reader does not know never stops that reader.

`mechanismVersion` is the descriptor's `upstream.template-version` published under the name that key used to have. It
adds nothing a corpus does not already state, and renaming it in the export is what `formatVersion` exists to announce.

When a stamp moves in the delivery flow, and how a version gets to a registry, are [std-CI]'s. This standard covers
what the move means. The `glossary@1` a component names in `plugin.json` is [std-PLUGIN]'s, and a pin on a package
`kac` itself takes is [std-CONFIG]'s. The version on an assembled plugin is the export's `contentVersion` read back, so
it states nothing the rules above have not already bound.

**A reviewer reads most of the rules above.** [ctl-corpus-validation] runs `kac validate`, which fails a declared import
that was never restored and warns where a newer version sits inside the declared range. It reports the caret trap as
information, not as a warning, because a corpus that capped itself on purpose is reporting a decision.
[ctl-content-version-tests] fails a corpus whose `content-version` did not rise where its export changed, its
`resolved:` lock moved, or its bundle includes a skill, a hook or a manifest that changed. A reviewer judges whether
that last change was to wording or to what the component tells a reader to do. Nothing compares a `shapeVersion` against
the files it stamps, and nothing reads `<Version>` against what a user of `kac` can observe.

## Sources and further reading

- **Normative.** [Semantic Versioning 2.0.0] is the grammar `<Version>` and `content-version` are written under, and
  the ordering a range resolves by.
- **Informative.** [npm semver ranges] defines the caret this repository borrowed two forms from, including where it
  stops below 1.0.0.

## Changelog

- 2026-10-23: a consumer moves its own `content-version` wherever its `resolved:` lock moves, by the part its producer
  moved.
- 2026-10-23: `content-version` moves where a change reaches a skill, a hook or the manifest a bundle includes, at the
  patch for wording and the minor for what the component tells a reader to do.
- 2026-10-18: deleting `.imports/`, repacking the producer and restoring again become `SHOULD`, because the pull
  request's CI restores from a clean checkout. A restore over an `.imports/` you did not delete is still no proof.
- 2026-09-15: a change confined to a section the export leaves out moves no `content-version`.
- 2026-09-09: `content-version` moves where a bundled skill changes what it tells a reader to do.
- 2026-09-09: a field's value format, name, addition or removal moves the component the `export:` block decides.
- 2026-09-07: initial version, taking the stamp semantics and the consumer-repointing rules from [std-CI].

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[Semantic Versioning 2.0.0]: https://semver.org
[ctl-content-version-tests]: ../controls/content-version-tests.md
[ctl-corpus-validation]: ../controls/corpus-validation.md
[npm semver ranges]: https://github.com/npm/node-semver#ranges
[std-CI]: workflows.md
[std-CONFIG]: configuration.md
[std-PLUGIN]: plugin.md
