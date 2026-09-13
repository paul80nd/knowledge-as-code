---
id: dev-security-design-unstated
type: deviation
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
owner: human:paul.law
tags: [ design, security, threat-model ]
---

# A security requirement arrives as a review comment

`Deviation: dev-security-design-unstated` `ACTIVE`

Nothing here sorts a change by risk before it is built, and no standard states the secure-by-default arrangement the
workflows already have.

## What we are doing instead

The workflows fail closed and deny by default: a job declares the permission it needs, and has none it did not ask
for. `.github/SECURITY.md` says how CI contains what it runs, and states what is in scope for a report. Both are
accurate, and no standard cites either.

A security requirement arrives as a comment on a pull request, from the maintainer who also wrote the change. Nothing
identifies a change as higher risk before the work starts.

## Why we need it

Threat modelling a documentation tool that reads a repository and writes files back into it produces a short list, and
`.github/SECURITY.md` already contains it. Turning that into a standard, a risk triage and a design review is real
work. One person in all three roles gets little from the separation.

## What compensates

* [std-CI] limits each job to the least permission it needs. [ctl-0003] says actionlint checks no such thing, so a
  reviewer does.
* `.github/SECURITY.md` is public, so a reader can see what CI contains and where a report is in scope.
* `eng:std-CSSTY` turns the analysers on, so their security rules fail a build without a reviewer looking.

## How it closes

A standard states what this repository already does: a workflow denies by default, a job declares its permission, and
the tool touches nothing outside the corpus it was given. It cites `.github/SECURITY.md` as the threat statement.

Risk triage closes separately, when a second person makes the sorting worth doing.

## Scope

The `kac` tool, the workflows, and the documentation site's build.

## Related

* [dev-one-maintainer] is why one person fills the author, reviewer and approver roles.
* [std-CI] states the permission rules a standard would cite.

[ctl-0003]: ../controls/0003-actionlint.md
[dev-one-maintainer]: one-maintainer.md
[std-CI]: ../standards/workflows.md
