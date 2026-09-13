---
id: svc-notices
type: service
tier: descriptive
status: live
repo: notices
platform: azure-function
criticality: important
owner: human:mira.okonjo
facets: [ event-driven, internal, scheduled ]
tags: [ email, renewals ]
---

# Notices

`Service: svc-notices` `LIVE`

Overdue reminders, hold-ready alerts and membership renewals: the emails a reader receives from the library.

## What it does

Sends templated emails to a reader on behalf of the library. It runs on a schedule and reacts to events. Timers sweep
for loans falling due and for loans already overdue. Three bus topics deliver the events that need an immediate message.

The message bodies are templates stored with the mail provider, selected by event type. Changing the wording is a
provider change and needs no deployment. The trade is deliberate: the people accountable for the wording can change it
without a developer, and the wording is outside version control.

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

It publishes no HTTP surface in any environment. The URL cells above are empty because there is no URL, not because
nobody has filled them in.

## Dependencies

None. It consumes from the bus and calls the mail provider. The library does not deploy that provider, so it is an
integration.

## Data

* A table recording what has been sent to whom, so a reader is not reminded twice for the same loan.
* A blob container storing message bodies too large for the bus.

Both are in the shared storage account.

## Operational notes

* **Messaging.** Three queues, each fed by a topic subscription.
  * `lending.hold_ready` feeds `notices-hold-ready`.
  * `lending.loan_due` feeds `notices-loan-due`.
  * `members.membership_expiring` feeds `notices-membership-expiring`.
* **Schedule.** The due-soon sweep runs daily at 06:00, and the overdue sweep runs at 07:00. A loan that became overdue
  overnight therefore gets the overdue notice and no due-soon notice.
* **Criticality**: `important`. It is queue-fed, so an outage delays notices and loses none. Work resumes from where it
  stopped when the service returns.
