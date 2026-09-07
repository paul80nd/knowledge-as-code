---
id: dev-no-incident-process
tier: normative
status: active
departs-from:
  - eng:pol-INCR.ADHOC
  - eng:pol-INCR.COMMS
  - eng:pol-INCR.DRILL
  - eng:pol-INCR.PROCESS
  - eng:pol-INCR.RECOVER
  - eng:pol-INCR.REPORT
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
applies-to:
  - svc-docs-site
  - svc-kac
  - svc-marketplace
owner: paul.law
tags: [ incidents, response, runbooks ]
---

# There is no incident process, so everything is handled informally

`Deviation: dev-no-incident-process` `ACTIVE`

Three runbooks cover three failures. Every other failure is whatever the maintainer decides on the day, and nobody is
told.

## What we are doing instead

[`../runbooks/`](../runbooks/) carries a runbook for each of three failures somebody has met. Outside those three,
there is no declaration, no roles and no order of work. The maintainer finds the fault, fixes it, and ships.

`.github/SECURITY.md` gives the route in for a vulnerability, and says a report is acknowledged within about a week.
No record here names that route, and nothing covers an ordinary failure a user meets.

Whoever installed a bad version of `kac` hears nothing until the next version lands. There is no announcement, no
advisory and no place a user would look.

## Why we need it

`kac` is a command-line tool with no service behind it, so an incident here is a bad release rather than an outage. The
recovery is a new version, which [std-VERS] already governs. A declaration procedure with one person to declare it to
would be a form the maintainer fills in for themselves.

The gap that costs a user is the silence, and that one is real.

## What compensates

* A correction ships as a new version, which [std-VERS] states, and a published version is never replaced.
* ctl-0008 restores the published package on each release, so a broken publish is caught by CI rather than by a user.
* `.github/SECURITY.md` gives a private route for a vulnerability, and GitHub security advisories carry the thread.
* The three runbooks cover the three failures that have actually happened.

## How it closes

Two pieces close this. A process record says what happens when a released version is found broken: who decides, what
ships, and where users are told. A standard names `.github/SECURITY.md` as the reporting route and states the
acknowledgement the file already promises.

Telling users means a GitHub release note and a security advisory where the fault warrants one. This record closes when
both exist.

## Scope

`kac` as published to nuget.org, the documentation site, and the plugin served from the marketplace branch.

## Related

* `eng:pol-INCR.ADHOC`, `eng:pol-INCR.COMMS`, `eng:pol-INCR.DRILL`, `eng:pol-INCR.PROCESS`, `eng:pol-INCR.RECOVER` and
  `eng:pol-INCR.REPORT` are the clauses this departs from.
* [dev-nothing-learns-from-a-fault] carries what happens after the fix.
* [std-VERS] governs the version a correction ships as.

[dev-nothing-learns-from-a-fault]: nothing-learns-from-a-fault.md
[std-VERS]: ../standards/versioning.md
