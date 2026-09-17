---
id: pmt-0002
type: postmortem
tier: decided
status: published
occurred-at: 2026-08-12T18:05:00Z
detected-at: 2026-08-12T18:12:00Z
restored-at: 2026-08-12T22:25:00Z
duration: PT4H20M
severity: sev2
affected:
  - cap-card-payment
  - svc-payment-api
  - svc-payment-ledger
prompted:
  - fix-0001
owner: human:alex.doe
tags: [ authorisation, idempotency, psp ]
---

# One order left two pending amounts on the card

`Postmortem: pmt-0002` `PUBLISHED`

## Summary

The payment service provider (PSP) slowed down for 4 hours 20 minutes, and authorisation calls began hitting their 5
second deadline. The checkout retried each timed-out call with a fresh idempotency key, so the PSP took the retry as a
second payment. 31 orders ended with two authorisations. Nothing detected the duplicates, because both calls
succeeded. A customer counted the holds on their statement the next morning, and the duplicates were released that
afternoon.

## Timeline

| Time (UTC)       | Event                                                                                                             |
|------------------|-------------------------------------------------------------------------------------------------------------------|
| 2026-08-12 18:05 | PSP authorisation latency rose from a 240ms average to over 4 seconds.                                            |
| 2026-08-12 18:12 | The p95 alert on [nfr-0001] fired and paged the on-call engineer.                                                 |
| 2026-08-12 18:20 | Calls began timing out at the 5 second deadline [std-PSPOUT] sets. The checkout retried each one.                 |
| 2026-08-12 18:20 | Each retry used a new `Idempotency-Key`, so the PSP authorised it as a new payment. Both calls returned `201`. |
| 2026-08-12 22:25 | PSP latency returned to its usual range. The on-call engineer closed the incident.                                |
| 2026-08-13 09:30 | Two customers reported two pending amounts for one order.                                                         |
| 2026-08-13 11:40 | Matching ledger entries on the order reference found 31 orders with a second authorisation.                  |
| 2026-08-13 14:10 | The later authorisation on each of the 31 orders was voided, and each void was written to the ledger.             |
| 2026-08-14 10:20 | [fix-0001] was written and verified against the PSP sandbox.                                                      |

## Impact

31 customers saw two pending amounts for one order, each for the basket value, for up to 20 hours. The duplicate holds
totalled £1,847. Two customers rang to ask whether they had been charged twice.

No customer was charged twice. Capture happens when an order ships, and every duplicate authorisation was voided before
any of the 31 orders reached despatch. [cap-card-payment] kept taking payments throughout, at the slower speed.

Measured against [nfr-0001]: breached. The p95 held at about 4.2 seconds against a target of 800ms, for 4 hours 20
minutes.

## Root cause

The checkout generated a fresh idempotency key for each attempt at one payment. [std-IDEM] requires a key derived from
the order and the operation, so that a retry arrives as the same payment. A fresh key makes every retry a new one.

## Contributing factors

* [svc-payment-api] cannot tell a retry from a new order. The header was present, well formed and unique, so every check
  the service makes passed.
* Nothing alerted on two authorisations against one order reference, and both calls returned `201`. The duplicates were
  invisible until a customer counted the holds on their statement.
* The page said latency, so the response was to watch the PSP recover. Nothing prompted anyone to ask what the retries
  were doing while it was slow.

## What went well

The deadline [std-PSPOUT] sets worked. Every call stopped at 5 seconds, so no request hung and the checkout stayed
usable at the slower speed.

The ledger made the duplicates findable. One immutable entry per event meant that matching on the order reference
returned all 31 pairs in a single query. Each void went in as its own entry under [std-LEDGER].

## Actions

| Action                                                             | Work item | Owner         |
|--------------------------------------------------------------------|-----------|---------------|
| Derive the checkout's idempotency key from the order and operation | [gh#3402] | Checkout team |
| Void the 31 later authorisations                                   | [gh#3403] | Payments team |
| Alert on a second authorisation against one order reference        | [gh#3404] | Payments team |
| Write to the 31 customers explaining the second hold               | [gh#3405] | Payments team |

## Related

* [fix-0001] is the fix this incident produced.
* [std-IDEM] is the rule the checkout broke.
* [nfr-0001] is the target the slowdown breached.

---

[cap-card-payment]: ../capabilities/card-payment.md
[fix-0001]: ../fixes/duplicate-authorisation.md
[gh#3402]: https://git.example.com/example-payments/payment-api/issues/3402
[gh#3403]: https://git.example.com/example-payments/payment-api/issues/3403
[gh#3404]: https://git.example.com/example-payments/payment-ledger/issues/3404
[gh#3405]: https://git.example.com/example-payments/payment-api/issues/3405
[nfr-0001]: ../nfrs/0001-authorisation-latency.md
[std-IDEM]: ../standards/authorisation/idempotency.md
[std-LEDGER]: ../standards/ledger/entries.md
[std-PSPOUT]: ../standards/authorisation/psp-timeouts.md
[svc-payment-api]: ../services/payment-api.md
