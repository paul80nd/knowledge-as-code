---
id: dev-departure-needs-none
type: deviation
tier: normative
status: active
departs-from:
  - pol-ADVS.NAME
  - pol-ADVS.PREFIX
risk: low
accepted-on: "2020-01-01"
review-by: "2040-01-01"
owner: human:alex.doe
---

# The ledger store keeps its old name

`Deviation: dev-departure-needs-none` `ACTIVE`

## What we are doing instead

The ledger store keeps the name it was created with, which carries no service prefix and is missing from the estate
register.

## Why we need it

Renaming the store means a migration window the ledger cannot take this quarter.

## What the risk is

An operator looking for the ledger's store does not find it by its service's name.

## What compensates

The ledger's runbook names the store, and the on-call operator reads that runbook first.

## How it closes

The store is renamed and registered in the next migration window.
