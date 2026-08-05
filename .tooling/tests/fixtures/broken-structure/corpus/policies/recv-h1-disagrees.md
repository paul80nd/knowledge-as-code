---
id: pol-RECV
tier: normative
category: operations
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# POL-OBSV: H1 mnemonic disagreeing with the filename

## Purpose

A well-formed H1 carrying a mnemonic that names a different policy than the file it sits in. The id and the filename
agree, so only the H1 is wrong — which is the half `id-matches-filename` cannot see.

## Scope

This fixture only. It exists so `h1-matches-id` is exercised on a mnemonic type as well as a numbered one; the two
compare against different halves of the filename.

## Commitments

* We **will** trigger `h1-matches-id` and nothing else.
