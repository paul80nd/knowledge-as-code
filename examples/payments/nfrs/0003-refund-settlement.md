---
id: nfr-0003
type: nfr
tier: normative
status: agreed
applies-to:
  - ofr-refund
target: 95% of refunds submitted to the PSP within one hour of the return being accepted
measured-by: Application Insights, on the gap between the return event and the refund request at [svc-payment-api]
review-by: "2027-08-28"
owner: human:paul.law
tags: [ psp, refunds, settlement ]
---

# A refund reaches the card scheme within the hour

`NFR: nfr-0003` `AGREED`

A refund is submitted to the PSP within an hour of the return being accepted.

## Target

95% of refunds within one hour, measured over a rolling day, from the return being accepted to
[svc-payment-api] submitting the refund to the PSP.

The money arriving in the customer's account is not committed. The card scheme decides that, and it
takes between three and five working days whatever this estate does.

## How it is measured

Application Insights records both moments against the payment id. The payments dashboard shows the
rolling day, and the same query runs in the monthly service review.

## Current actual

98.2% over August 2026. The failures are all the same shape: a return accepted while
[svc-payment-ledger] is mid-deployment waits for the next sweep.

## If it is breached

An alert fires where the figure falls below 95% for two consecutive hours and pages the on-call
engineer. A customer who has returned goods and seen nothing calls the contact centre on the second
day, so a sustained breach reaches support before it reaches finance.

## Constraints

The PSP accepts a refund only against a captured payment, so a return accepted before capture waits
for it. [ofr-card-payment] captures on shipment, which is why the hour is measured from acceptance of
the return and not from the return itself.

[ofr-card-payment]: ../offerings/card-payment.md
[svc-payment-api]: ../services/payment-api.md
[svc-payment-ledger]: ../services/payment-ledger.md
