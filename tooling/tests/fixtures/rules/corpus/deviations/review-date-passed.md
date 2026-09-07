---
id: dev-review-date-passed
tier: normative
status: active
departs-from:
  - std-ERRORS.a-failure-says-what-happened
accepted-on: "2020-01-01"
review-by: "2020-06-01"
owner: alex.doe
---

# The batch export endpoint returns a bare status code

`Deviation: dev-review-date-passed` `ACTIVE`

## What we are doing instead

The batch export endpoint answers a failure with a status code and an empty body. It does not send the error shape the
standard names.

## Why we need it

The one caller reads the status code and discards the body, so shaping the body would have delayed the release for
nobody's benefit.

## What compensates

The endpoint writes the reason to its own log, and the owner reads that log when the caller reports a failed run.

## How it closes

The endpoint returns the standard shape when its caller is next changed.
