---
id: itg-mail-delivery
type: integration
tier: descriptive
status: active
vendor: Hartwell Mail
used-by:
  - svc-notices
criticality: important
their-sla: 99.9% monthly availability, and 95% of accepted messages delivered within 60 seconds
replaces: itg-sms-notices
owner: human:mira.okonjo
tags: [ email, templates ]
---

# Mail Delivery

`Integration: itg-mail-delivery` `ACTIVE`

Every email the consortium sends a borrower.

## What it does

Accepts a message over an API and delivers it. [svc-notices] sends overdue reminders, hold-ready alerts and membership
renewals through it, and no other service in this catalogue calls the vendor.

**The vendor stores the message wording.** [svc-notices] selects a template by event type and sends the values that
fill it. Changing what a borrower reads is therefore a change made in the vendor's console. That wording sits outside
version control, so this corpus cannot tell you what a borrower received last month.

## Contract

|              |                                            |
|--------------|--------------------------------------------|
| **Protocol** | REST over HTTPS, one call per message      |
| **Endpoint** | `https://api.hartwell.example.com/v2/send` |
| **Auth**     | A bearer token issued to the sender        |
| **Docs**     | <https://docs.hartwell.example.com/v2>     |

The token sits in the platform key vault.

## Failure modes

| Failure                                            | How it presents                               | Our fallback                                     |
|----------------------------------------------------|-----------------------------------------------|--------------------------------------------------|
| The API returns 429 during the overdue sweep       | Notices arrive late                           | The queue keeps the work, and the sender retries |
| A template is edited and will not render           | Every message of that type fails              | None. The vendor keeps no version history        |
| A borrower's domain is put on the suppression list | The borrower gets nothing, and nothing errors | None. Somebody reads that list once a week       |

## Their SLA

Delivery is measured to the receiving server. A message a borrower's own provider then files as junk counts as
delivered, so the target says nothing about whether anybody read it.

## Exit

The API call is one HTTP request, and [svc-notices] would point it at another vendor in a configuration change.

**The wording is the lock-in.** The vendor stores every template, so a move means re-authoring each one in the new
vendor's console and checking what a borrower reads. Nothing in version control says what the current wording is, so
that re-authoring starts by copying it out of the console by hand.

The contract gives 60 days' notice. This estate has made the move once already: [itg-sms-notices] closed in June 2026
and its traffic came here.

## Commercials

|                   |                                                  |
|-------------------|--------------------------------------------------|
| **Cost model**    | Per thousand messages, billed monthly in arrears |
| **Renewal**       | 1 September, with 60 days' notice to cancel      |
| **Account owner** | Mira Okonjo                                      |

## Contacts

Support is a portal, and a ticket gets a reply within one working day. Deliverability is a separate team on the same
portal, and a suppression question takes longer. There is no telephone escalation on this contract.

## Related

* [svc-notices] sends every borrower-facing message through this.
* [itg-sms-notices] is the retired text-message vendor whose traffic moved here.

[itg-sms-notices]: sms-notices.md
[svc-notices]: ../services/notices.md
