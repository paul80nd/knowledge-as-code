---
id: ofr-borrowing
type: offering
tier: descriptive
status: live
implemented-by:
  - svc-catalogue-api
  - svc-catalogue-web
  - svc-lending
  - svc-notices
feature-files:
  - lending/features/loan-lifecycle.feature
  - lending/features/renewal.feature
  - notices/features/overdue-sweep.feature
  - platform/tests/Catalogue.Acceptance/my-loans.feature
owner: human:dev.raman
tags: [ loans, renewals ]
---

# Borrow and return an item

`Offering: ofr-borrowing` `LIVE`

A borrower takes an item out, sees what they have on loan, renews it, and hears from the library before it falls due.

## What it does

One card borrows at every branch, and an item borrowed at one branch is returned at any other.

Within this offering [svc-notices] writes to the borrower at three moments: a loan approaching its due date, a loan
already overdue, and a membership due for renewal.

## Who it is for

Cardholders of any library in the consortium, and the counter staff who borrow on their behalf.

## Why it exists

Lending is what the consortium is for. Every branch, every item and every card exists to support this one exchange.

## Surfaces

The counter and the self-service terminals in each branch, and the borrower's own loans page in the public catalogue.

## Where the detail lives

* **Implemented by**: [svc-catalogue-api], [svc-catalogue-web], [svc-lending], [svc-notices]
* **Specified in**: [ADO#1150], [ADO#1287], [ADO#1401]

## Known limitations

A loan made at the counter while the legacy system is down is recorded on paper and keyed in afterwards.
[svc-lending] is the only route to that system.

[ADO#1150]: https://dev.azure.com/example-libraries/consortium/_workitems/edit/1150
[ADO#1287]: https://dev.azure.com/example-libraries/consortium/_workitems/edit/1287
[ADO#1401]: https://dev.azure.com/example-libraries/consortium/_workitems/edit/1401
[svc-catalogue-api]: ../services/catalogue-api.md
[svc-catalogue-web]: ../services/catalogue-web.md
[svc-lending]: ../services/lending.md
[svc-notices]: ../services/notices.md
