---
id: dev-github-holds-identity
tier: normative
status: active
departs-from:
  - eng:pol-ACCS.AUTHN
  - eng:pol-ACCS.NAMED
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on:
applies-to:
  - svc-docs-site
  - svc-kac
  - svc-marketplace
owner: paul.law
tags: [ access, github, identity ]
---

# GitHub decides who anybody here is

`Deviation: dev-github-holds-identity` `ACTIVE`

Every account, password and second factor reaching this repository belongs to GitHub, and no standard here says what
any of them has to be.

## What we are doing instead

A change arrives under a named GitHub account, and the commit carries that account's name and address. A workflow acts
as a token GitHub mints for one run. Both satisfy the clauses in practice, and both are GitHub's arrangement rather
than one this repository states.

Nothing here sets a password rule, a second-factor rule or a session length. The maintainer's account carries a
passkey and a hardware key because GitHub offers them, not because a record asks for them.

## Why we need it

Writing a standard for authentication means writing rules this repository cannot enforce. GitHub owns the login screen,
and a rule about it would be a wish with no check behind it. [std-CI] states what a workflow's identity may do, which
is the half that is ours to hold.

## What compensates

* Every commit, review and merge is attributable to a GitHub account, and the history shows it.
* [std-CI] gives each workflow the least permission it needs, so a compromised account reaches only what its job
  declares.
* Publishing to nuget.org waits for an approval on a named environment, so a stolen session still meets a person.
* The repository is public and holds nothing private, so a lost account costs the history's integrity rather than its
  confidentiality.

## How it closes

Somebody writes down what this repository requires of an account reaching it: a second factor, a hardware key for the
maintainer, and what happens when either is lost. That belongs in a standard, and the standard closes this record.

Where the answer stays "whatever GitHub does", say so in a standard rather than in this record, and close it that way.

## Scope

Every account and token reaching the repository, its packages, its pages and its marketplace branch.

## Related

* [std-CI] states what a workflow's identity may do.

[std-CI]: ../standards/workflows.md
