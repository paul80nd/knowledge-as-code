---
id: dev-security-design-unstated
tier: normative
status: active
departs-from:
  - eng:pol-SECD.DESIGN
  - eng:pol-SECD.HIRISK
  - eng:pol-SECD.REQS
  - eng:pol-SECD.THREAT
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
applies-to:
owner: paul.law
tags: [ design, security, threat-model ]
---

# Security arrives in review rather than in a requirement

`Deviation: dev-security-design-unstated` `ACTIVE`

Nothing here sorts a change by risk before it is built, and no standard states the secure-by-default arrangement the
workflows already have.

## What we are doing instead

The workflows fail closed and deny by default: a job declares the permission it needs, and holds none it did not ask
for. `.github/SECURITY.md` says how CI contains what it runs, and names what is in scope for a report. Both are
accurate and neither is named by a standard.

A security requirement arrives as a comment on a pull request, from the maintainer who also wrote the change. Nothing
identifies a change as higher risk before the work starts.

## Why we need it

Threat modelling a documentation tool that reads a repository and writes files back into it produces a short list, and
`.github/SECURITY.md` already carries it. The work of turning that into a standard, a risk triage and a design review
is real, and one person doing all three roles gets little from the separation.

## What compensates

* [std-CI] holds each job to the least permission it needs, and ctl-0003 reads the workflow files for it.
* `.github/SECURITY.md` states the containment CI relies on, and it is public.
* `eng:std-CSSTY` turns the analysers on, so the security rules they carry fail a build rather than a reviewer.
* The tool reaches the filesystem it was pointed at and the network never, which is a small surface to reason about.

## How it closes

A standard states what this repository already does: a workflow denies by default, a job declares its permission, and
the tool touches nothing outside the corpus it was given. It cites `.github/SECURITY.md` as the threat statement.

Risk triage closes separately, when a second person makes the sorting worth doing.

## Scope

The `kac` tool, the workflows, and the documentation site's build.

## Related

* `eng:pol-SECD.DESIGN`, `eng:pol-SECD.HIRISK`, `eng:pol-SECD.REQS` and `eng:pol-SECD.THREAT` are the clauses this
  departs from.
* [std-CI] carries the permission rules a standard would cite.

[std-CI]: ../standards/workflows.md
