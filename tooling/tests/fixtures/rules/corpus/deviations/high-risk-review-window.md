---
id: dev-high-risk-review-window
type: deviation
tier: normative
status: active
departs-from:
  - std-ERRORS.a-failure-says-what-happened
risk: high
accepted-on: "2030-01-01"
review-by: "2031-01-01"
owner: human:alex.doe
---

# The admin console shows a raw stack trace on failure

`Deviation: dev-high-risk-review-window` `ACTIVE`

## What we are doing instead

The admin console renders whatever the service returned, so a failure reaches the operator as a stack trace.

## Why we need it

The console was built for one operator during a migration, and shaping the failure was cut to meet the cutover date.

## What the risk is

A stack trace names internal hosts and query text, and the console is reachable from the office network.

## What compensates

The console sits behind the same sign-in as the service, and only three accounts can open it.

## How it closes

The console reads the error shape the standard names, and renders the message alone.
