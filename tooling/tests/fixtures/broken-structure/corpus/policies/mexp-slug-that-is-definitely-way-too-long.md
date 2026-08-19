---
id: pol-MEXP
tier: normative
category: security
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# An over-long slug behind a mnemonic

`Policy: pol-MEXP` `DRAFT`

## Purpose

The slug is measured with the `mexp-` prefix excluded, exactly as `NNNN-` is excluded for a numbered type. This pins
that the mnemonic prefix is stripped before the limit is applied — were it counted, the reported length would be five
characters higher.

## Scope

This fixture only.

## Clauses

| Id      | Clause                                          | Alignment |
|---------|-------------------------------------------------|-----------|
| `CLEAN` | **MUST** trigger `slug-length` and nothing else |           |

