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

The policy template says a shared obligation is signposted from both sides, closing each clause with
`. See [pol-OTHR]`. Eighteen clauses in Example Engineering do that. Sixteen of them form eight pairs, and each pair
carries one id on both sides. Two point one way: `eng:pol-ENVS.CREDS` names `eng:pol-SCRT`, and `eng:pol-OBSV.SECRETS`
names `eng:pol-SCRT` and `eng:pol-DATA`. Neither target holds a clause of that id.

## Context

Read while adopting this type in this corpus. The convention lives in `examples/engineering/policies/_template.md`
alone. `kac checks` lists nothing for it, and the phrase appears nowhere under `tooling/`.

## Why it might matter

A citation names a clause id. Where one side of a pair renames its clause, the pair splits and both sides still read
as correct. The two one-way references may be deliberate, because telemetry and a log are not obviously one
obligation. Nobody has said which.
