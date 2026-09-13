---
id: fix-0002
type: fix
tier: normative
status: active
symptom-keywords: [ editorconfig, formatting, ide, indent, reformat, rider, wrapping ]
promoted-from: dsc-rider-holds-the-editorconfig
verified:
  - { at: 2026-09-09T08:14:30Z, by: human:paul.law }
review-by: "2027-03-09"
owner: human:paul.law
tags: [ editorconfig, formatting, rider ]
---

# An .editorconfig key edited from a shell changes nothing in the IDE

`Fix: fix-0002` `ACTIVE`

## Symptom

A key edited in `.editorconfig` from the terminal makes no difference to how Rider formats a file. Reformatting the
file, and reopening it, both leave the old behaviour in place. Nothing is reported. The edit is valid, and Rider keeps
using the settings it already read.

## Cause

Rider reads an `.editorconfig` when the file is opened in the IDE. A write it never saw leaves the cached settings in
place.

## Resolution

1. Open `.editorconfig` in Rider, which reads it on open.
2. Reformat a file the key governs.
3. Check the result changed.

Where the result is unchanged, flip a key with an obvious effect as a control. `max_line_length` works well, because a
reformat rewraps the paragraph you are looking at.

## Why it happens

An `.editorconfig` is layered. The corpora under `examples/` read the one at the repository root, `tooling/` adds a
second over it, and `template/` has its own with `root = true`. A key that looks inert therefore has two possible
causes: another file overrides it, or Rider has not read the edit. The control above tells an override apart from a
cached setting.

## How we found it

A session edited the file from a shell while the developer had the repository open in Rider, and the two then
disagreed about the same key. That disagreement is the signal. The question that settles it is which of them has
opened `.editorconfig` since the edit.

## Related

* [dsc-rider-holds-the-editorconfig] is the observation this was promoted from.

[dsc-rider-holds-the-editorconfig]: ../discoveries/rider-holds-the-editorconfig.md
