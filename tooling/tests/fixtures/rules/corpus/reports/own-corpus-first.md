---
id: rpt-own-corpus-first
type: report
tier: descriptive
status: draft
owner: human:alex.doe
generated: { at: 2026-09-08T10:00:00Z, by: kac/0.24.0 }
sources:
  - { resource: fixture-corpus, version: "0.2.0" }
  - { resource: alpha-imports, version: "1.0.0" }
---

# A report listing its own corpus ahead of an import

`Report: rpt-own-corpus-first` `DRAFT`

`kac report` writes `sources` with the corpus the report answers for first and each import after it. Here the import
sorts ahead of the corpus, so a list held to alphabetical order would warn. `sources` declares its order as written,
and `list-order` reports nothing.
