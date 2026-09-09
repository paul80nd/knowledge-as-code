---
id: rpt-verified-by-its-own-producer
type: report
tier: descriptive
status: draft
owner: human:alex.doe
generated: { at: 2026-09-08T10:00:00Z, by: kac/0.24.0 }
sources:
  - { resource: fixture-corpus, version: "0.2.0" }
verified:
  - { at: 2026-09-08T11:00:00Z, by: coverage-sweep/1.2.0 }
  - { at: 2026-09-08T12:00:00Z, by: kac/0.24.0 }
---

# A report signed off by the run that wrote it

`Report: rpt-verified-by-its-own-producer` `DRAFT`

An agent may verify a report, which is what lets its read count for something. The first entry names one, carrying
its version the way the tool names its own, and fires nothing.

The second names `kac/0.24.0`, which is what `generated.by` names above. So the run is verifying its own output, and
`self-verification` reports the entry. The message names the line rather than the field, because the fix is to delete
one entry and leave the other alone.
