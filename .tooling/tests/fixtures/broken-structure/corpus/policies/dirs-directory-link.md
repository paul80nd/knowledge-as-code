---
id: pol-DIRS
tier: normative
category: security
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# A link to a directory does not resolve

`Policy: pol-DIRS` `DRAFT`

## Purpose

A bare directory is not a link target. This corpus holds `policies/` and `adrs/` as folders and neither as a page, so
[the policies folder](/policies) and [the adrs folder](/adrs) both fail — where accepting a directory would have let
them pass.

## Scope

This fixture only. It guards the case that made link resolution depend on the machine: git cannot track an empty
directory, so a link to one passed wherever the folder had been created by hand and failed everywhere else.

## Clauses

| Id     | Clause                                                    | Alignment |
|--------|-----------------------------------------------------------|-----------|
| `DIRS` | **MUST** trigger `link-resolves` twice and nothing else   |           |
