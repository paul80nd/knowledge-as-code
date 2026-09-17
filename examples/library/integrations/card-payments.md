---
id: int-card-payments
type: integration
tier: descriptive
status: trial
vendor: Kestrel Payments
used-by:
  - svc-reservations
criticality: important
their-sla: 99.9% monthly availability on the hosted payment page
owner: human:mira.okonjo
tags: [ fees, payments ]
---

# Card Payments

`Integration: int-card-payments` `TRIAL`

The card payment a borrower makes for the fee a hold attracts.

## What it does

Takes one card payment on a page the vendor hosts. [svc-reservations] redirects the borrower there, the borrower pays,
and the vendor returns them to the service against the hold reference. No card detail passes through any service this
consortium deploys.

**The status is `trial` because [cap-reservations] is still building.** Development and test run against the vendor's
sandbox. Production points at the live endpoint and has taken no payment from a borrower yet.

## Contract

|              |                                                                    |
|--------------|--------------------------------------------------------------------|
| **Protocol** | An HTTPS redirect out and back, and an API to query one payment    |
| **Endpoint** | `https://pay.kestrel.example.com/checkout/v2`                      |
| **Auth**     | A merchant id, and a signature on the return the service verifies  |
| **Docs**     | <https://developer.kestrel.example.com>                            |

The signing material sits in the platform key vault. A borrower's card number never arrives here to be stored.

## Failure modes

| Failure                        | How it presents                                      | Our fallback                               |
|--------------------------------|------------------------------------------------------|--------------------------------------------|
| The hosted page is unavailable | The flow stops before the hold is created            | Branch staff place the hold at the desk    |
| The borrower closes the browser | The payment is taken, and the hold is not confirmed | An hourly job re-queries by hold reference |
| The vendor declines the card   | The borrower sees the decline on the vendor's page   | None. A decline is the system working      |

## Their SLA

99.9% monthly availability on the hosted payment page, measured over a calendar month. Settlement into the consortium's
account is three working days.

**The contract covers the hosted page alone.** The hourly re-query is there for a borrower who pays and never comes
back, and not for a promise the vendor has missed.

## Commercials

|                   |                                                      |
|-------------------|------------------------------------------------------|
| **Cost model**    | A fixed fee per transaction, with no monthly minimum |
| **Renewal**       | Rolling, with 30 days' notice on either side         |
| **Account owner** | Mira Okonjo                                          |

## Contacts

A payment incident goes to a 24-hour support line. Everything else goes to the portal, which answers within one working
day. The trial has no named technical contact, so a question about the sandbox goes through the account manager.

## Related

* [svc-reservations] runs the redirect and confirms the hold.
* [cap-reservations] is the capability that cannot go live without this.

[cap-reservations]: ../capabilities/reservations.md
[svc-reservations]: ../services/reservations.md
