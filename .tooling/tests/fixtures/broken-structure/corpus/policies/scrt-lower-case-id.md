---
id: pol-scrt
tier: normative
category: security
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# Lower-case mnemonic in the id

`Policy: pol-scrt` `DRAFT`

## Purpose

The mnemonic agrees with the filename but is lower-case. The id carries it upper-case; only the filename is
lower-case, so this is `id-format` rather than a mismatch.

## Scope

This fixture only.

## Clauses

| Id      | Clause                                        |
|---------|-----------------------------------------------|
| `CLEAN` | **MUST** trigger `id-format` and nothing else |

