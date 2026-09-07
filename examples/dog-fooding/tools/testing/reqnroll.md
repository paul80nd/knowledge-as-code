---
id: tol-reqnroll
type: tool
tier: descriptive
status: approved
versions: 3.3.x
licence: BSD-3-Clause
decided-in:
replaces:
successor:
owner: human:paul.law
tags: [ bdd, gherkin, testing ]
---

# Reqnroll

`Tool: tol-reqnroll` `APPROVED`

The framework that turns the Gherkin feature documents in `kac.features` into tests, bound to [tol-xunit] by the
`Reqnroll.xunit.v3` package.

## What we use it for

Each feature document states what a verb does in the words a reader of the documentation would use, and Reqnroll binds
each step to a method. A step that no longer matches fails the build, so the feature documents cannot describe a
command surface the tool has stopped offering.

## Status

**approved** since 2026-08-04.

## Where it is used

* [svc-kac] is tested with it.

## Alternatives considered

None recorded.

## Licence and obligations

BSD-3-Clause. It asks that the copyright notice travel with a redistribution, and forbids using the project's name to
endorse anything built on it. Nothing here redistributes it.

## Related

* [tol-xunit] is the runner it binds to.

[svc-kac]: ../../services/kac.md
[tol-xunit]: xunit.md
