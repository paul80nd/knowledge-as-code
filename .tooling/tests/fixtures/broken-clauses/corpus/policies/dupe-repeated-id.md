---
id: pol-DUPE
tier: normative
category: governance
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# One clause id on two obligations

`Policy: pol-DUPE` `DRAFT`

## Purpose

Both rows are well-formed on their own, and the document reads correctly. What breaks is anything citing it:
`pol-DUPE.SAME` names two obligations, and the reader has no way to know which was meant.

## Scope

This fixture only. It exists so `clause-id-unique` is exercised.

## Clauses

| Id     | Clause                                               | Alignment |
|--------|------------------------------------------------------|-----------|
| `SAME` | **MUST** trigger `clause-id-unique` and nothing else |           |
| `SAME` | **MUST NOT** carry a second id of its own            |           |