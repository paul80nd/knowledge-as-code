---
id: dev-unsigned-commits
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-EVER.SIGNED
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on:
owner: human:paul.law
tags: [ commits, provenance, version-control ]
---

# Commits are not signed

`Deviation: dev-unsigned-commits` `ACTIVE`

No commit in this repository carries a signature, and nothing rejects one that does not.

## What we are doing instead

A commit carries the name and address git was configured with, and GitHub shows the account that pushed it. A merge
into `main` goes through a pull request, so the account behind a change is recorded twice: on the commit and on the
merge.

An agent-written commit adds a `Co-Authored-By` trailer and a session link. That says what wrote the change, and it is
no more verifiable than the author line above it.

## Why we need it

Signing means a key on the maintainer's machine, a key in every agent's environment, and a branch rule that rejects an
unsigned push. The middle one is the problem: much of this repository is written by agents running in sandboxes, and
handing each a signing key spreads the key rather than protecting the history.

## What compensates

* Every merge into `main` is a pull request from a named account, and the branch rule refuses a direct push.
* GitHub records the account that pushed each commit, separately from the author line the commit carries.
* The repository is public, so a rewritten history is visible to anybody holding a clone.
* Nothing here is deployed from a checkout. What ships is built by a workflow from the commit on `main`.

## How it closes

GitHub can sign on the merge, which puts one key in one place and asks nothing of an agent's machine. Turning that on
and requiring signed commits on the branch rule closes this record.

## Scope

Every commit in this repository, on `main` and on every branch.

## Related

* [std-CI] states what the branch rule requires today.

[std-CI]: ../standards/workflows.md
