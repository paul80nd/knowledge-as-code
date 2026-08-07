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

A bare directory is not a link target. This corpus holds `media/` as a folder and no `media.md` beside it, so
[the media folder](/media) fails — where accepting a directory would have let it pass.

## Scope

This fixture only. It guards the case that made link resolution depend on the machine: git cannot track an empty
directory, so a link to one passed wherever the folder had been created by hand and failed everywhere else. `media/`
is also a folder no schema declares, which is allowed and checked by nothing.

## Clauses

| Id     | Clause                                                  | Alignment |
|--------|----------------------------------------------------------|-----------|
| `DIRS` | **MUST** trigger `link-resolves` once and nothing else  |           |
