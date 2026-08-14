---
id: pol-COLN
tier: normative
category: governance
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# A citation that addresses its clause with a colon

`Policy: pol-COLN` `DRAFT`

## Purpose

A colon-separated citation such as `pol-COLN:CLEAN` resolves to nothing. The parser reads a citation by its
separator, so the one `clause-ref` would have judged is never collected, and the document passes carrying a
reference the reader has every reason to trust.

## Scope

This fixture only. It exercises `clause-ref` on a separator, where the clause named is otherwise real.

## Clauses

| Id      | Clause                                         | Alignment |
|---------|------------------------------------------------|-----------|
| `CLEAN` | **MUST** trigger `clause-ref` and nothing else |           |
