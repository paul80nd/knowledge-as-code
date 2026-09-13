---
id: nfr-0001
type: nfr
tier: normative
status: agreed
applies-to:
  - svc-payment-api
target: p95 under 800ms, measured over a rolling hour
measured-by: Application Insights, on the server duration of POST /authorisations
review-by: "2027-08-28"
owner: human:paul.law
tags: [ checkout, latency, psp ]
---

# A customer waits under a second to hear whether their card was accepted

`NFR: nfr-0001` `AGREED`

An authorisation returns in under 800ms at the 95th percentile.

## Target

p95 under 800ms, over a rolling hour, on `POST /authorisations` at [svc-payment-api]. The measurement runs from the
request arriving to the response leaving, so the PSP's own time counts towards the 800ms.

The 99th percentile is deliberately not committed. A card issuer can take several seconds to answer a step-up
challenge, and that time belongs to the issuer's system.

## How it is measured

Application Insights records the server duration of every request to that route. The payments dashboard shows the
rolling hour. The on-call engineer reads the dashboard during an incident and at no other time, so the alert below is
what raises a breach.

## Current actual

p95 of 610ms over August 2026, against an average of 240ms. The PSP accounts for the gap. A slow reply from the PSP
is a slow response here, and its response times account for most of the spread.

## If it is breached

An alert fires at 800ms sustained for fifteen minutes and pages the on-call engineer. Beyond about two seconds
customers begin abandoning the checkout, and finance sees the drop in conversion the same day.

## Constraints

The PSP's contract promises a p95 of 500ms on its authorisation endpoint. The 800ms here is that figure plus the time
[svc-payment-api] spends on either side of the call. A faster target needs a conversation with the PSP, not a change to
[svc-payment-api]. The `integrations` type is not adopted here, so `constrained-by` has no id and this section records
the cap in words.

[svc-payment-api]: ../services/payment-api.md
