---
id: pol-KNOW
tier: normative
category: governance
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# An identity line with the status in the wrong case

`Policy: pol-KNOW` `Draft`

## Purpose

The right status, written the way the frontmatter writes it rather than the way the line does. Frontmatter is
lower-case because it is read by a machine; the line is upper-case because it is read as a stamp.

## Scope

This fixture only. It exists so the wrong-case branch of `identity-status` is exercised as well as the
disagreeing-status branch, which `envs-identity-status.md` covers.

## Clauses

| Id      | Clause                                              | Alignment |
|---------|-----------------------------------------------------|-----------|
| `CLEAN` | **MUST** trigger `identity-status` and nothing else |           |

