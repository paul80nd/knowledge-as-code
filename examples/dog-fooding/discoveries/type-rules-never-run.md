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

# Three knowledge types hold no record anywhere, so their rules have never run

`Discovery: dsc-type-rules-never-run` `OPEN`

## What I saw

Seven adopted type folders across `examples/` contain no record. Four of the seven are types this corpus has records
for, and `.schema/` is authored once at the root, so those rules do run somewhere. Three are types no corpus here has
a record for: `capabilities`, `data` and `integrations`, all adopted by Example Library. Every rule those three
declare has run against no record at all.

## Context

Counted from the files on 2026-09-08, while adopting `discoveries` in this corpus. `part-ref` failed the first record
of that type on its first run. PR #212 and PR #492 each fixed a check that had never run against a record.

## Why it might matter

A clean `kac validate` is evidence about the records a corpus has. It says nothing about a rule that has never
run against a record. Writing a first record into `capabilities`, `data` or `integrations` may turn a green run red.
