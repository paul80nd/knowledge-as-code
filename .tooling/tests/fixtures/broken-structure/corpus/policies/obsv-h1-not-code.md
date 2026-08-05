---
id: pol-OBSV
tier: normative
category: operations
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# pol-OBSV An id written as prose rather than as code

## Purpose

The right id in the right place, written without the backticks that make it read as a handle. `Md.PlainText` flattens
a code span to its content, so the H1 pattern sees the same characters either way — only the AST node tells them apart.

## Scope

This fixture only. It exists so the missing-code-span branch of `h1-matches-id` is exercised as well as the
wrong-id branch, which `recv-h1-disagrees.md` covers.

## Commitments

* We **will** trigger `h1-matches-id` and nothing else.
