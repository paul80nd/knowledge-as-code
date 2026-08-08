---
id: svc-notices
tier: descriptive
status: live
repo: notices
platform: azure-function
criticality: important
depends-on:
data-stores:
owner: mira.okonjo
tags: [ event-driven, internal, scheduled ]
---

# Notices

`Service: svc-notices` `LIVE`

Overdue reminders, hold-ready alerts and membership renewals — the email a reader receives from the library.

## What it does

Sends the templated email the library owes a reader. It runs on a schedule and reacts to events: timers sweep for loans
falling due and loans already overdue, and three bus topics carry the events that need an immediate message.

The message bodies live with the mail provider as templates selected by event type, so changing the wording is a
provider change rather than a deployment. The trade is deliberate: the people accountable for the wording can change it
without a developer, and the wording is not in version control.

## Where it lives

* **Repository** — [`notices`](https://git.example.com/example-libraries/notices) — `src/Notices.FunctionApp`
* **Platform** — Azure Functions, .NET isolated worker
* **Deployed as** — Function App `func-notices-<env>`

## Environments

| Environment | URL | Notes                               |
|-------------|-----|-------------------------------------|
| Development |     | `func-notices-dev` — no public URL  |
| Test        |     | `func-notices-test` — no public URL |
| Production  |     | `func-notices-prd` — no public URL  |

It publishes no HTTP surface in any environment. There is no URL to record, rather than one nobody has written down.

## Dependencies

None. It consumes from the bus and calls the mail provider, which is an integration rather than a service.

## Data

* A table recording what has been sent to whom, so a reader is not reminded twice for the same loan.
* A blob container carrying message bodies too large to travel on the bus.

Both are in the shared storage account.

## Operational notes

* **Messaging** — three queues, each fed by a topic subscription: `notices-hold-ready` from `lending.hold_ready`,
  `notices-loan-due` from `lending.loan_due`, and `notices-membership-expiring` from `members.membership_expiring`.
* **Schedule** — the due-soon sweep runs daily at 06:00 and the overdue sweep at 07:00, in that order, so a loan that
  became overdue overnight is not also reminded about as due soon.
* **Criticality** — `important`. It is queue-fed, so an outage delays notices rather than losing them: work resumes from
  where it stopped when the service returns.
