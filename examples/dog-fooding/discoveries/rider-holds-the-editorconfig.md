---
id: dsc-rider-holds-the-editorconfig
type: discovery
tier: observed
status: open
source: session
confidence: unverified
expires: "2026-12-07"
provenance: An agent session editing .editorconfig from a shell while the developer had the repository open in Rider.
applies-to:
promoted-to:
owner: human:paul.law
tags: [ editorconfig, formatting, rider ]
---

# Rider does not re-read an .editorconfig changed from a shell

`Discovery: dsc-rider-holds-the-editorconfig` `OPEN`

## What I saw

A key edited in `.editorconfig` from the terminal changed nothing about how Rider formatted a file. Opening
`.editorconfig` in the IDE made the same change take effect. Flipping a key with an obvious effect was the control.

## Context

Seen once, on one machine, and the Rider version was not recorded. This repository holds three `.editorconfig`
files. The four corpora under `examples/` read the one at the root, `tooling/` layers a second over it, and
`template/` holds its own with `root = true`.

## Why it might matter

A session changes `.editorconfig`, sees no difference in the IDE, and concludes the edit did nothing. The next move is
usually to change something else that was already right.
