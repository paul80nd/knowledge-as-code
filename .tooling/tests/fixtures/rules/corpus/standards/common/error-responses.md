---
id: std-0001
tier: normative
status: active
owner: alex.doe
axis: common
applies-to: [svc-catalogue]
review-by: "2026-12-31"
---

# Error responses

`Standard: std-0001` `ACTIVE`

## Summary

How a service reports a failure to its caller. Valid in every respect except the one this fixture is for:
it cites no authority, so nothing says who decided any of it.

## Rules

A failure is reported with the status code that describes it and a body the caller can act on.

## Examples

A request for a record that does not exist is answered `404`, not `200` with an empty body.

## Conformance checklist

* Does every failure path return a status code that matches what happened?

## Rationale and provenance

There is none, which is the point — `derived-from` and `implements` are both absent, so this is guidance
wearing a standard's frontmatter.
