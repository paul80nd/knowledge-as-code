---
id: fix-0002
type: fix
tier: normative
status: active
symptom-keywords: [editorconfig, formatting, ide, indent, reformat, rider, wrapping]
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
file, and reopening it, both leave the old behaviour in place. Nothing is reported: the edit is valid and the IDE
carries on with the settings it already had.

## Cause

Rider reads an `.editorconfig` when the file is opened in the IDE, and a write it never saw leaves the cached settings
standing.

## Resolution

1. Open `.editorconfig` in Rider. The IDE reads it on open, and the edit takes effect.
2. Reformat a file the key governs, and check the result changed.

Where the result is unchanged, flip a key with an obvious effect as a control. `max_line_length` is one, because a
reformat rewraps the paragraph in front of you.

## Why it happens

This repository holds three `.editorconfig` files. The four corpora under `examples/` read the one at the root,
`tooling/` layers a second over it, and `template/` holds its own with `root = true`. So a key that looks inert may be
overridden rather than unread, and the control above is what separates the two.

## How we found it

A session edited the file from a shell while the developer had the repository open. The give-away is a second machine,
or a fresh `dotnet format` run, honouring a key the IDE ignores.

## Related

* [dsc-rider-holds-the-editorconfig] is the observation this was promoted from.

[dsc-rider-holds-the-editorconfig]: ../discoveries/rider-holds-the-editorconfig.md
