---
id: pol-scrt
tier: normative
category: security
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# POL-SCRT: Lower-case mnemonic in the id

## Purpose

The mnemonic agrees with the filename but is lower-case. The id carries it upper-case; only the filename is
lower-case, so this is `id-format` rather than a mismatch.

## Scope

This fixture only.

## Commitments

* We **will** trigger `id-format` and nothing else.
