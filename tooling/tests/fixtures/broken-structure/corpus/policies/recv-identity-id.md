---
id: pol-RECV
tier: normative
category: operations
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# An identity line naming a different policy

`Policy: pol-OBSV` `DRAFT`

## Purpose

A well-formed identity line carrying a mnemonic that names a different policy than the file it sits in. The frontmatter
id and the filename agree, so only the line is wrong — which is the half `id-matches-filename` cannot see.

## Scope

This fixture only. It exists so `identity-id` is exercised on a mnemonic type as well as a numbered one; the two
compare against different halves of the filename.

## Clauses

| Id      | Clause                                          | Alignment |
|---------|-------------------------------------------------|-----------|
| `CLEAN` | **MUST** trigger `identity-id` and nothing else |           |

