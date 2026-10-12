---
id: dev-draft-review-date-passed
type: deviation
tier: normative
status: draft
departs-from:
  - std-ERRORS.a-failure-says-what-happened
risk: low
review-by: "2020-06-01"
owner: human:alex.doe
---

# The bulk import endpoint reports one failure for the whole batch

`Deviation: dev-draft-review-date-passed` `DRAFT`

## What we are doing instead

The bulk import endpoint answers a failed batch with one error. It does not say which rows failed or why.

## Why we need it

The importer that calls it retries the whole batch on any failure, so a per-row answer would go unread.

## What the risk is

A batch that fails on every retry is explained from a log the caller cannot read.

## What compensates

The endpoint writes each failed row and its reason to its own log, and the owner reads it when a batch keeps failing.

## How it closes

The endpoint names each failed row once the importer can retry a row on its own.
