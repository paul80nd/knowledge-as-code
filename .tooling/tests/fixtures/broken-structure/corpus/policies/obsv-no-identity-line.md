---
id: pol-OBSV
tier: normative
category: operations
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# A document with no identity line

## Purpose

The H1 is followed straight by a section heading. Nothing beneath the title says what this document is, which one it
is, or whether it is in force — the three things a reader arriving from a citation checks first.

## Scope

This fixture only. It exists so the absent-line branch of `identity` is exercised as well as the malformed-line branch,
which `agnt-identity-malformed.md` covers.

## Clauses

| Id      | Clause                                       |
|---------|----------------------------------------------|
| `CLEAN` | **MUST** trigger `identity` and nothing else |

