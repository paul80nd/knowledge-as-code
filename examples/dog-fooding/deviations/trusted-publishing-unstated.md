---
id: dev-trusted-publishing-unstated
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-SCRT.ZEROSEC
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
applies-to:
  - svc-kac
owner: human:paul.law
tags: [ publishing, secrets, workload-identity ]
---

# No standard says this repository holds no static key

`Deviation: dev-trusted-publishing-unstated` `ACTIVE`

Trusted publishing removed the last long-lived credential, and nothing states that as the rule that keeps it removed.

## What we are doing instead

The publish workflow authenticates to nuget.org by exchanging a GitHub OIDC token, so no API key exists in a secret
store or anywhere else. [std-CI] states that a workflow has no credential of its own, and cites the clauses about
storing and rotating a secret.

`eng:pol-SCRT.ZEROSEC` is the clause asking for a secretless path where one is available. No rule here cites it, so
somebody adding a second publish target could use an API key without failing a check.

## Why we need it

The rule and the practice arrived together, and the practice was easier to write. [std-CI] grew from what the
workflows already did, and a clause about a credential that does not exist reads as a clause about nothing.

## What compensates

* The workflows have no key. Nothing checks that: [ctl-0003] says actionlint reports nothing about a credential
  written in the wrong place.
* [std-CI] already refuses a workflow with a credential of its own, which gives the same outcome from the other side.
* Publishing waits for an approval on the `nuget.org` environment, so a token exchange still needs a person.
* `publish-tool.yml` is the only thing here that contacts nuget.org. It checks the version, exchanges the token and
  pushes the package, in three steps of one file anybody can read.

## How it closes

[std-CI] gains a rule saying a publish authenticates by workload identity where the registry offers it, and cites
`eng:pol-SCRT.ZEROSEC` on its `Covers` line. That is one rule and one line, and this record closes on it.

## Scope

The publish workflows, and any registry this repository publishes to.

## Related

* [std-CI] is where the rule closing this belongs.

[ctl-0003]: ../controls/0003-actionlint.md
[std-CI]: ../standards/workflows.md
