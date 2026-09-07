---
id: dev-review-before-acceptance
tier: normative
status: active
departs-from:
  - std-ERRORS.a-failure-says-what-happened
accepted-on: "2030-05-01"
review-by: "2030-04-01"
owner: alex.doe
---

# The batch importer returns bare status codes

`Deviation: dev-review-before-acceptance` `ACTIVE`

## What we are doing instead

The batch importer answers a failed request with a status code and an empty body.

## Why we need it

Its only caller is a scheduled job that reads the status code, and the error shape would be built for nobody.

## What compensates

Every failure is written to the run log with the detail the error body would have carried.

## How it closes

A second caller appears, or the importer is folded into the reports service.
