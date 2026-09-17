---
id: pmt-0001
type: postmortem
tier: decided
status: published
occurred-at: 2026-06-03T21:14:00Z
detected-at: 2026-06-03T21:15:00Z
restored-at: 2026-06-03T21:26:00Z
duration: PT12M
severity: sev1
affected:
  - cap-card-payment
  - cap-refund
  - svc-payment-api
  - svc-payment-ledger
prompted:
  - nfr-0002
owner: human:paul.law
tags: [ failover, ledger, reconciliation ]
---

# The checkout refused payments the card had already authorised

`Postmortem: pmt-0001` `PUBLISHED`

## Summary

A maintenance failover moved the ledger database to a geo-replica that was 11 minutes behind. For 12 minutes
[svc-payment-api] could not write to the ledger. It told 86 customers their payment had failed, after the payment
service provider (PSP) had already authorised it. The replica came up without the 214 entries written in the 11
minutes before the failover. The next morning's reconciliation found every missing entry, and the entries were
rebuilt from the PSP's settlement file.

## Timeline

| Time (UTC)       | Event                                                                                                 |
|------------------|-------------------------------------------------------------------------------------------------------|
| 2026-06-03 21:03 | The last ledger entry that reached the geo-replica was written.                                       |
| 2026-06-03 21:14 | Platform maintenance failed `sql-payment-ledger-prod` over to the geo-replica.                        |
| 2026-06-03 21:14 | Ledger writes began failing. [svc-payment-api] returned an error to the checkout after each PSP call. |
| 2026-06-03 21:15 | The write-failure alert paged the on-call engineer.                                                   |
| 2026-06-03 21:26 | The replica accepted writes. The checkout took payments again.                                        |
| 2026-06-03 22:40 | The on-call engineer closed the incident, having seen writes recover.                                 |
| 2026-06-04 02:00 | The PSP's settlement file for 3 June arrived by SFTP.                                                 |
| 2026-06-04 03:10 | The reconciliation run reported 214 breaks, all of them in the file and not in the ledger.            |
| 2026-06-04 07:40 | The finance owner escalated the break alert, and the lost window was understood.                      |
| 2026-06-05 16:20 | The 214 entries were rebuilt from the settlement file, and the reconciliation ran clean.              |

## Impact

86 customers were told at the checkout that their payment had failed. The PSP held an authorisation against each of
those cards, so those customers saw a pending amount for an order they believed had not gone through. Some paid a second
time. [cap-card-payment] was unusable for the 12 minutes the ledger refused writes.

A further 214 customers paid successfully and left no ledger entry. Their orders showed as unpaid, and a refund request
against any of them could not be traced until 5 June, so [cap-refund] was unavailable for those orders for two days.

The value of the baskets abandoned during the 12 minutes is not known. Nothing records an attempt that never reached the
PSP.

`restored-at` marks 21:26, when the checkout took payments again. The data repair ran on past it to 5 June. The
outage lasted 12 minutes; the 214 orders were a defect behind a service that was already serving.

No recovery-point target covered the ledger on the day, so nobody could say on the night whether losing 11 minutes was
acceptable. That absence is the finding that produced [nfr-0002].

## Root cause

The failover promoted an asynchronous geo-replica whose lag nothing measured. Any entry written inside that lag was
lost the moment the failover completed.

## Contributing factors

* [svc-payment-api] treats a failed ledger write as a failed payment, and the PSP has already authorised by that point.
  The customer sees a refusal against a live hold.
* No alert compared the replica's position with the source's, so the size of the lost window was unknown during the
  incident.
* The maintenance window was scheduled at 21:00, inside the evening peak for card payments.
* The incident was closed as soon as writes recovered. Nothing prompted anyone to ask what the failover had cost.

## What went well

The reconciliation run found all 214 missing entries on its first pass after the incident. It classified them exactly
as [std-RECON] requires: in the file and not in the ledger. One alert stated the count and the total value, so the
finance owner escalated a single figure.

[std-LEDGER] made the repair safe. An entry is never amended, so the rebuild appended 214 entries and left the
reconciliation reading the same sequence finance had read.

## Actions

| Action                                                    | Work item | Owner         |
|-----------------------------------------------------------|-----------|---------------|
| Void the 86 authorisations the PSP holds against no order | [gh#3310] | Payments team |
| Rebuild the 214 lost entries from the settlement file     | [gh#3311] | Payments team |
| Alert when replica lag passes the recovery point          | [gh#3312] | Platform team |
| Hold the checkout open while a ledger write is retried    | [gh#3313] | Payments team |
| Move platform maintenance out of the evening peak         | [gh#3314] | Platform team |

## Related

* [nfr-0002] states the recovery point this incident caused to be written.
* [std-RECON] is the daily run that found the lost entries.
* [std-LEDGER] is why the repair appended entries.

---

[cap-card-payment]: ../capabilities/card-payment.md
[cap-refund]: ../capabilities/refund.md
[gh#3310]: https://git.example.com/example-payments/payment-api/issues/3310
[gh#3311]: https://git.example.com/example-payments/payment-ledger/issues/3311
[gh#3312]: https://git.example.com/example-payments/payment-ledger/issues/3312
[gh#3313]: https://git.example.com/example-payments/payment-api/issues/3313
[gh#3314]: https://git.example.com/example-payments/payment-ledger/issues/3314
[nfr-0002]: ../nfrs/0002-ledger-recovery-point.md
[std-LEDGER]: ../standards/ledger/entries.md
[std-RECON]: ../standards/ledger/reconciliation.md
[svc-payment-api]: ../services/payment-api.md
