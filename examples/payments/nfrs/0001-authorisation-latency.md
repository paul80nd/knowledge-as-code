---
id: nfr-0001
tier: normative
status: agreed
applies-to:
  - svc-payment-api
target: p95 under 800ms, measured over a rolling hour
measured-by: Application Insights, on the server duration of POST /authorisations
constrained-by:
review-by: "2027-08-28"
owner: paul.law
tags: [ checkout, latency, psp ]
---

# A customer waits under a second to hear whether their card was accepted

`NFR: nfr-0001` `AGREED`

An authorisation answers in under 800ms at the 95th percentile.

## Target

p95 under 800ms, over a rolling hour, on `POST /authorisations` at [svc-payment-api]. Measured from the request
arriving to the response leaving, so the PSP's own time is inside the budget.

The 99th percentile is deliberately not committed. A card issuer can take several seconds to answer a step-up
challenge, and a target covering that would be a target about somebody else's system.

## How it is measured

Application Insights records the server duration of every request to that route. The payments dashboard carries the
rolling hour, and the on-call engineer reads it during an incident. Nobody reads it otherwise, and the alert below is
what brings it to attention.

## Current actual

p95 of 610ms over August 2026, against an average of 240ms. The gap is the PSP: a request it answers slowly is slow
here, and the spread of its response times is most of the spread of ours.

## If it is breached

An alert fires at 800ms sustained for fifteen minutes and pages the on-call engineer. Beyond about two seconds
customers begin abandoning the checkout, which finance sees as a drop in conversion the same day.

## Constraints

The PSP's contract promises a p95 of 500ms on its authorisation endpoint. The 800ms here is that plus what
[svc-payment-api] spends on either side of the call, so a faster target needs a conversation with the PSP rather than a
change to our code. The `integrations` type is not adopted here, so `constrained-by` carries no id and the cap is
recorded in these words.

[svc-payment-api]: ../services/payment-api.md
