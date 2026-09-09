---
id: rpt-producer-without-a-version
type: report
tier: descriptive
status: draft
owner: human:alex.doe
generated: { at: 2026-09-08T10:00:00Z, by: kac }
sources:
  - { resource: fixture-corpus, version: "0.2.0" }
confirmed:
  - { at: 2026-09-08T11:00:00Z, by: human:alex.doe }
---

# A producer named without its version

`Report: rpt-producer-without-a-version` `DRAFT`

`generated.by` names `kac` and stops. A reader cannot tell which version of the tool wrote the report, so a run whose
output changed reads exactly like one whose output did not, which is what `generated-by-a-producer` reports.

The `event` shape leaves `by` a plain string, because what counts as an actor differs by what the event is. This type
holds it to a producer and its version, and the fix type holds the same key to a person.
