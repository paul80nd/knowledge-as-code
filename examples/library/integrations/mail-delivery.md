---
id: int-mail-delivery
type: integration
tier: descriptive
status: active
vendor: Hartwell Mail
used-by:
  - svc-notices
  - svc-shelf-audit
criticality: important
their-sla: 99.9% monthly availability, and 95% of accepted messages delivered within 60 seconds
owner: human:mira.okonjo
tags: [ email, templates ]
---

# Mail Delivery

`Integration: int-mail-delivery` `ACTIVE`

Every email the consortium sends a borrower, and the nightly report the shelf audit posts to a mailbox.

## What it does

Accepts a message over an API and delivers it. [svc-notices] sends overdue reminders, hold-ready alerts and membership
renewals through it. [svc-shelf-audit] sends its nightly report the same way.

**The vendor stores the message wording.** [svc-notices] selects a template by event type and sends the values to fill
it, so changing what a borrower reads is a change made in the vendor's console. That wording is outside version
control, and this corpus cannot tell you what a borrower received last month.

## Contract

|              |                                            |
|--------------|--------------------------------------------|
| **Protocol** | REST over HTTPS, one call per message      |
| **Endpoint** | `https://api.hartwell.example.com/v2/send` |
| **Auth**     | A bearer token issued per sending service  |
| **Docs**     | <https://docs.hartwell.example.com/v2>     |

Each service has a token of its own, and both sit in the platform key vault.

## Failure modes

| Failure                                            | How it presents                               | Our fallback                                     |
|----------------------------------------------------|-----------------------------------------------|--------------------------------------------------|
| The API returns 429 during the overdue sweep       | Notices arrive late                           | The queue keeps the work, and the sender retries |
| A template is edited and will not render           | Every message of that type fails              | None. The vendor keeps no version history        |
| A borrower's domain is put on the suppression list | The borrower gets nothing, and nothing errors | None. Somebody reads that list once a week       |

## Their SLA

99.9% monthly availability on the sending API, and 95% of accepted messages delivered within 60 seconds. Delivery is
measured to the receiving server, so a message a borrower's provider then files as junk counts as delivered.

## Commercials

|                   |                                                  |
|-------------------|--------------------------------------------------|
| **Cost model**    | Per thousand messages, billed monthly in arrears |
| **Renewal**       | 1 September, with 60 days' notice to cancel      |
| **Account owner** | Mira Okonjo                                      |

## Contacts

Support is a portal, and a ticket gets a reply within one working day. Deliverability is a separate team, reached
through the same portal, and a suppression question takes longer. There is no telephone escalation on this contract.

## Related

* [svc-notices] sends every borrower-facing message through this.
* [svc-shelf-audit] sends its nightly report through it, and a mailbox is the only reader of that report.

[svc-notices]: ../services/notices.md
[svc-shelf-audit]: ../services/shelf-audit.md
