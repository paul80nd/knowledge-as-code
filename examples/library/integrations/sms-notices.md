---
id: int-sms-notices
type: integration
tier: descriptive
status: retired
vendor: Marbury Messaging
used-by:
  - svc-notices
criticality: supporting
owner: human:mira.okonjo
tags: [ notices, sms ]
---

# SMS Notices

`Integration: int-sms-notices` `RETIRED`

The text message a borrower used to get on the day a loan fell due.

## What it does

Nothing now. The consortium stopped sending text messages on 30 June 2026, and [svc-notices] sends email alone.

Until then it sent one kind of message: a reminder on the day a loan fell due, to a borrower who had given a mobile
number. Hold-ready alerts and membership renewals were always email. It ended for two reasons. The price per message
rose at the June renewal, and every borrower who got the text also got the email.

**`used-by` still names [svc-notices].** The field is required and nothing calls this vendor today, so the id records
which service used to.

## Contract

The account closed on 30 June 2026. This is the contract as it stood.

|              |                                          |
|--------------|------------------------------------------|
| **Protocol** | REST over HTTPS, one call per message    |
| **Endpoint** | `https://api.marbury.example.com/v1/sms` |
| **Auth**     | An API token issued to one sender        |
| **Docs**     | <https://docs.marbury.example.com/v1>    |

The token was deleted from the platform key vault on the day the account closed.

## Failure modes

| Failure                      | How it presents                             | Our fallback                                        |
|------------------------------|---------------------------------------------|-----------------------------------------------------|
| The number was disconnected  | Accepted by the vendor, and never delivered | The email every borrower on this list also received |
| The vendor's queue backed up | The reminder arrived the next morning       | None. A late reminder was still sent                |

## Commercials

|                   |                                          |
|-------------------|------------------------------------------|
| **Cost model**    | Per message, billed monthly in arrears   |
| **Renewal**       | None. The contract ended on 30 June 2026 |
| **Account owner** | Mira Okonjo, until the account closed    |

## Contacts

None. The portal login stopped working when the account closed, so a question about a message sent before June 2026 has
nowhere to go.

## Related

* [svc-notices] sent through this, and sends email alone now.
* [int-mail-delivery] took what this used to send.
* [cap-borrowing] is the capability the due-date reminder belonged to.

[cap-borrowing]: ../capabilities/borrowing.md
[int-mail-delivery]: mail-delivery.md
[svc-notices]: ../services/notices.md
