---
id: pol-ENVS
tier: normative
category: operations
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# An identity line claiming the wrong status

`Policy: pol-ENVS` `ACTIVE`

## Purpose

The line says the policy is in force; the frontmatter says it is still a draft. Of the three things the identity line
restates this is the one that moves, so it is the one that drifts.

## Scope

This fixture only. It exists so the disagreeing-status branch of `identity-status` is exercised as well as the
wrong-case branch, which `know-identity-case.md` covers.

## Clauses

| Id      | Clause                                              | Alignment |
|---------|-----------------------------------------------------|-----------|
| `CLEAN` | **MUST** trigger `identity-status` and nothing else |           |

