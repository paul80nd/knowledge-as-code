---
id: dev-open-ended-departure
type: deviation
tier: normative
status: active
departs-from:
  - std-ERRORS.a-failure-says-what-happened
accepted-on: "2030-01-01"
review-by: "2030-05-01"
owner: human:alex.doe
---

# The legacy reports endpoint keeps its own error shape

`Deviation: dev-open-ended-departure` `ACTIVE`

## What we are doing instead

The legacy reports endpoint returns the error shape it shipped with, rather than the one the standard names.

## Why we need it

Its remaining callers sit outside our estate, and none of them can be changed.

## What compensates

A smoke check runs the two calls those callers make, and pages the owner on failure.

## How it closes

It does not. The endpoint stays as it is indefinitely, because nobody left can change the callers.
