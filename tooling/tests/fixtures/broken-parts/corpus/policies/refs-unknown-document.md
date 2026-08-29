---
id: pol-REFS
tier: normative
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# A citation of a document that does not exist

`Policy: pol-REFS` `DRAFT`

## Purpose

`pol-ZZZZ.ANY` fails on its first half rather than its second. Written as a code span it is not a link, so no
link check reaches it — which is why the citation form is validated in its own right rather than left to
`link-resolves`.

## Scope

This fixture only. It exists so `part-ref` is exercised on its unknown-document branch.

## Clauses

| Id      | Clause                                       | Alignment |
|---------|----------------------------------------------|-----------|
| `CLEAN` | **MUST** trigger `part-ref` and nothing else |           |