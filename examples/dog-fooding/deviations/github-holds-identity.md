---
id: dev-github-holds-identity
type: deviation
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
owner: human:paul.law
tags: [ access, github, identity ]
---

# GitHub decides who anybody here is

`Deviation: dev-github-holds-identity` `ACTIVE`

Every account, password and second factor used here belongs to GitHub, and no standard here says what any of them has
to be.

## What we are doing instead

A change arrives under a named GitHub account, and the commit records that account's name and address. A workflow acts
as a token GitHub issues for one run. Both satisfy the clauses in practice. Both are GitHub's arrangement, and no
record here states them.

Nothing here sets a password rule, a second-factor rule or a session length. The maintainer's account has a passkey
and a hardware key because GitHub offers them, not because a record asks for them.

## Why we need it

Writing a standard for authentication means writing rules this repository cannot enforce. GitHub owns the login
screen, so a rule about it would have no check behind it. [std-CI] states what a workflow's identity may do, which is
the half this repository controls.

## What compensates

* Every commit, review and merge is attributable to a GitHub account, and the history shows it.
* [std-CI] gives each workflow the least permission it needs, so a compromised account gets only what its job
  declares.
* Publishing to nuget.org waits for an approval on a named environment, so a stolen session still needs a person.
* The repository is public and stores nothing private, so a lost account threatens the history's integrity and nothing
  else.

## How it closes

Somebody writes down what this repository requires of an account used here: a second factor, a hardware key for the
maintainer, and what happens when either is lost. That belongs in a standard, and the standard closes this record.

Where the answer stays "whatever GitHub does", write that in a standard and close this record on it.

## Scope

Every account and token used on the repository, its packages, its pages and its marketplace branch.

## Related

* [std-CI] states what a workflow's identity may do.

[std-CI]: ../standards/workflows.md
