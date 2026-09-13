---
id: dsc-shared-clause-ids-unchecked
type: discovery
tier: observed
status: open
source: session
confidence: unverified
expires: "2026-12-07"
provenance: An agent session adopting this type, reading the clause tables under examples/engineering/policies.
applies-to:
  - svc-kac
promoted-to:
owner: human:paul.law
tags: [ citations, clauses, conventions ]
---

# A shared clause carries the same id on both sides, and nothing checks it

`Discovery: dsc-shared-clause-ids-unchecked` `OPEN`

## What I saw

The policy template says a shared obligation is marked on both sides, closing each clause with `. See [pol-OTHR]`.
Eighteen clauses in Example Engineering do that. Sixteen of them form eight pairs, and each pair uses one id on both
sides. Two are one-way: `eng:pol-ENVS.CREDS` cites `eng:pol-SCRT`, and `eng:pol-OBSV.SECRETS` cites `eng:pol-SCRT`
and `eng:pol-DATA`. Neither target has a clause with that id.

## Context

Read while adopting this type in this corpus. The policy template states the convention, in both trees that have a
copy of it. `kac checks` lists nothing that enforces it.

## Why it might matter

A citation is built from a clause id. Where one side of a pair renames its clause, the pair splits and both sides
still read as correct. The two one-way references may be deliberate, because telemetry and a log are not obviously one
obligation. Nobody has confirmed either way.
