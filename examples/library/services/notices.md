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
facets: [ event-driven, internal, scheduled ]
tags: [ email, renewals ]
---

# Notices

`Service: svc-notices` `LIVE`

Overdue reminders, hold-ready alerts and membership renewals: the emails a reader receives from the library.

## What it does

Sends templated emails on behalf of the library to a reader. It runs on a schedule and reacts to events. Timers sweep
for loans falling due and for loans already overdue. Three bus topics carry the events that need an immediate message.

The message bodies live with the mail provider as templates, selected by event type. Changing the wording is a provider
change and takes no deployment. The trade is deliberate: the people accountable for the wording can change it without a
developer, and the wording is not in version control.

## Where it lives

* **Repository**: [`notices`](https://git.example.com/example-libraries/notices), at `src/Notices.FunctionApp`
* **Platform**: Azure Functions, .NET isolated worker
* **Deployed as**: Function App `func-notices-<env>`

## Environments

| Environment | URL | Notes                              |
|-------------|-----|------------------------------------|
| Development |     | `func-notices-dev`: no public URL  |
| Test        |     | `func-notices-test`: no public URL |
| Production  |     | `func-notices-prd`: no public URL  |

It publishes no HTTP surface in any environment. The blank cells above are a finding: there is no URL to record.

## Dependencies

None. It consumes from the bus and calls the mail provider. We do not deploy that provider, so it is an integration.

## Data

* A table recording what has been sent to whom, so a reader is not reminded twice for the same loan.
* A blob container carrying message bodies too large to travel on the bus.

Both are in the shared storage account.

## Operational notes

* **Messaging**: three queues, each fed by a topic subscription. `lending.hold_ready` feeds `notices-hold-ready`,
  `lending.loan_due` feeds `notices-loan-due`, and `members.membership_expiring` feeds `notices-membership-expiring`.
* **Schedule.** The due-soon sweep runs daily at 06:00 and the overdue sweep runs at 07:00, in that order. A loan that
  became overdue overnight is therefore not also reminded about as due soon.
* **Criticality**: `important`. It is queue-fed, so an outage delays notices and loses none. Work resumes from where it
  stopped when the service returns.
