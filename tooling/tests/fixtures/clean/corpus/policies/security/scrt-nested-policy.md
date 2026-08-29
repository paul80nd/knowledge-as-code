---
id: pol-SCRT
tier: normative
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# A policy filed one folder down

`Policy: pol-SCRT` `DRAFT`

## Purpose

The happy path for a nested record: the file sits in `policies/security/`, and the category the tool reads from that
path is `security`. Nothing in the frontmatter says so.

## Scope

This fixture only. It sits beside a policy filed flat, so the golden pins both a derived category and an empty one.

## Clauses

| Id      | Clause                                                       | Alignment |
|---------|--------------------------------------------------------------|-----------|
| `NEST`  | **MUST** stay free of findings from one folder down          |           |
| `LOCAL` | **MUST NOT** link anywhere, for the reason `pol-VURM` gives  |           |
