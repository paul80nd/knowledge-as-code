---
id: dev-standing-publish-rights
tier: normative
status: active
departs-from:
  - eng:pol-ACCS.PERSIST
  - eng:pol-ACCS.ZERO
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on:
applies-to:
  - svc-kac
  - svc-marketplace
owner: paul.law
tags: [ access, publishing, standing-grant ]
---

# The maintainer's publish rights never go away

`Deviation: dev-standing-publish-rights` `ACTIVE`

The one maintainer holds admin on the repository, on the nuget.org package and on the pages site, and nothing takes any
of it back.

## What we are doing instead

Publishing `kac` waits for an approval on the `nuget.org` environment, so a release is two acts rather than one. The
rights behind that approval are held all the time, by the person who also writes the change and approves the pull
request.

Nothing grants those rights for a window and removes them afterwards, and nothing reviews whether they are still
needed.

## Why we need it

A just-in-time grant needs somebody to grant it. With one maintainer, the approver and the requester are the same
person, so the grant would be a form filled in by whoever wanted it. That is ceremony rather than control.

## What compensates

* The `nuget.org` environment approval is a deliberate second act, recorded against the run that asked for it.
* Trusted publishing means no long-lived key exists to steal, so the rights are useless without the account.
* Every publish leaves a workflow run naming the commit it built, and ctl-0008 checks the round trip.
* nuget.org refuses a second push to a version it already holds, so a stolen session cannot replace what shipped.

## How it closes

A second maintainer makes just-in-time access mean something: one person asks, another grants, and the grant expires.
This record closes on the pull request that writes that down.

Where this repository still has one maintainer at the review date, the question is whether standing rights on a public
repository with no data behind it are worth the machinery to remove them.

## Scope

The GitHub organisation, the nuget.org package, the pages site and the marketplace branch.

## Related

* `eng:pol-ACCS.PERSIST` and `eng:pol-ACCS.ZERO` are the clauses this departs from.
* [dev-one-maintainer] is the condition underneath it.

[dev-one-maintainer]: one-maintainer.md
