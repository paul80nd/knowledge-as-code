---
id: dsc-type-rules-never-run
type: discovery
tier: observed
status: open
source: session
confidence: corroborated
expires: "2026-12-07"
provenance: An agent session adopting this type, counting the record files under every examples/ corpus.
applies-to:
  - svc-kac
promoted-to:
owner: human:paul.law
tags: [ coverage, schema, validation ]
---

# Seven adopted type folders hold no record, so their rules have never run

`Discovery: dsc-type-rules-never-run` `OPEN`

## What I saw

`controls` and `tools` in Example Engineering, and `capabilities`, `data`, `integrations`, `processes` and `runbooks`
in Example Library. Each is named in its corpus's `types:` and holds only `_index.md` and `_template.md`. Every rule
those seven types declare has been evaluated against no record at all.

## Context

Counted from the files on 2026-09-08. Two merged pull requests came out of this gap already. PR #212 fixed `present()`
failing to see a list, and `LooksLikeId` rejecting an upper-case mnemonic. PR #492 fixed `LooksLikeId` reading a
citation as a single string.

## Why it might matter

A clean `kac validate` is evidence about the records a corpus holds. It says nothing about a rule no record reaches.
Writing a first record into any of the seven folders may turn a green run red.
