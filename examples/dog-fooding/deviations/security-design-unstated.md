---
id: dev-security-design-unstated
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-SECD.HIRISK
  - eng:pol-SECD.REQS
  - eng:pol-SECD.THREAT
risk: low
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
owner: human:paul.law
tags: [ design, security, threat-model ]
---

# A security requirement arrives as a review comment

`Deviation: dev-security-design-unstated` `ACTIVE`

Nothing here sorts a change by risk before it is built, or records its security requirements before the work starts.

## What we are doing instead

A security requirement arrives as a comment on a pull request, from the maintainer who also wrote the change. Nothing
identifies a change as higher risk before the work starts, and nothing records how a change could be attacked.

`.github/SECURITY.md` says how CI contains what it runs, and states what is in scope for a report. It covers the
repository as a whole, and no change is weighed against it.

## Why we need it

Threat modelling a documentation tool that reads a repository and writes files back into it produces a short list, and
`.github/SECURITY.md` already contains it. Turning that into a risk triage and a design review for each change is real
work. One person in the author, reviewer and approver roles gets little from the separation.

## What the risk is

A change ships with a security fault that nobody sorted it for. The workflows deny by default and the analysers fail a
build, so the arrangement is right and unstated. What slips through is a fault the same person misses twice, as author
and as reviewer. A triage step filled in by that person would miss it a third time.

## What compensates

* [std-CI] limits each job to the least permission it needs. [ctl-0003] says actionlint checks no such thing, so a
  reviewer does.
* `.github/SECURITY.md` is public, so a reader can see what CI contains and where a report is in scope.
* `eng:std-CSSTY` turns the analysers on, so their security rules fail a build without a reviewer looking.

## How it closes

A standard says which changes count as high risk here. It requires such a change to record its security requirements,
and how it could be attacked, before it is built. It cites `.github/SECURITY.md` as the threat statement for the
repository. This record closes on that standard.

## Scope

The `kac` tool, the workflows, and the documentation site's build.

## Related

* [dev-one-maintainer] is why one person fills the author, reviewer and approver roles.
* [std-CI] states the permission rules a standard would cite.

[ctl-0003]: ../controls/0003-actionlint.md
[dev-one-maintainer]: one-maintainer.md
[std-CI]: ../standards/workflows.md
