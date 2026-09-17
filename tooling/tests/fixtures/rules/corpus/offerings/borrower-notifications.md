---
id: ofr-borrower-notifications
type: offering
tier: descriptive
status: live
owner: human:alex.doe
implemented-by: [svc-notifications]
feature-files:
  - notifications/features/hold-available.feature
  - qa-pack/features/overdue.feature
nfrs: [nfr-0001]
---

# Borrower notifications

`Offering: ofr-borrower-notifications` `LIVE`

## What it does

Sends borrowers a message when a hold becomes available, when a loan is close to falling due, and again
once it has. Each message is composed from a template, addressed through whichever channel the borrower
has chosen, and recorded so that a later question about whether it was sent can be answered.

## Why it exists

A hold nobody collects is a book nobody reads, and a fine nobody expected is a complaint.

## Where the detail lives

* **Implemented by**: [svc-notifications]
* **Constrained by**: [nfr-0001]

Two links against all the prose above, which is what `hub-not-specification` measures. The second feature
file names a repository no service here claims, which is what `feature-file-repo` reports.

[nfr-0001]: ../nfrs/0001-hedged-measurement.md
[svc-notifications]: ../services/notifications.md
