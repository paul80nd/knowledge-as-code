---
id: pol-CREF
tier: normative
category: governance
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# A citation of a clause that does not exist

`Policy: pol-CREF` `DRAFT`

## Purpose

The document the citation names is real and resolves, so nothing about `pol-CREF.MISSING` looks wrong to a
reader: they follow it, find the policy, and are left to conclude they misread the clause table. This is the
failure clause ids exist to prevent, and the only one that needs the whole corpus to see.

## Scope

This fixture only. It exists so `clause-ref` is exercised where the document resolves and the clause does not.

## Clauses

| Id      | Clause                                             |
|---------|----------------------------------------------------|
| `CLEAN` | **MUST** trigger `clause-ref` and nothing else     |
