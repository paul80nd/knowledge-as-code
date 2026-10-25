---
id: prc-followed-by-a-process
type: process
tier: procedural
status: active
owner: human:alex.doe
last-rehearsed: { at: "2020-01-01", by: process:release-bot }
rehearsal-frequency: on-change
---

# Cut a release

`Process: prc-followed-by-a-process` `ACTIVE`

`last-rehearsed` names a process as `by`, so `followed-by-a-person-or-agent` reports it. A procedure a process follows
is automation. `on-change` gives `staleness` no window, so the date reports nothing.

## When to use this

When the changelog has an entry under its unreleased heading.

## Prerequisites

Write access to the repository.

## Steps

1. Move the version in the project file.
2. Tag the commit.

## Verification

The tag names the version the project file states.

## If it goes wrong

Delete the tag and move the version back.
