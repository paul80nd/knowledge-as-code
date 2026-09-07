---
id: cap-ado-epics-that-are-not-numbers
type: capability
tier: descriptive
status: planned
implemented-by: [ svc-notifications ]
owner: human:alex.doe
ado-epics: [ 41, EPIC-7 ]
---

# ADO epics that are not numbers

`Capability: cap-ado-epics-that-are-not-numbers` `PLANNED`

## What it does

Names [svc-notifications], and carries an `ado-epics` entry that is not a number.

## Why it exists

`ado-epics` is the only field declared `of: int`, so it is where a fixture reaches [svc-notifications].

## Where the detail lives

Nowhere else.

[svc-notifications]: ../services/notifications.md
