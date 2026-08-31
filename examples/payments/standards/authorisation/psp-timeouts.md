---
id: std-PSPOUT
tier: normative
status: active
implements: [ eng:pol-PERF.TARGETS, eng:pol-RECV.DEGRADE, eng:pol-RECV.RETRY, eng:pol-RECV.TIMEOUT ]
applies-to:
  - svc-payment-api
review-by: "2027-08-31"
owner: paul.law
tags: [ psp, resilience, timeouts ]
---

# A call to the PSP is bounded, and an unknown outcome is resolved

`Standard: std-PSPOUT` `ACTIVE`

## Summary

Every call to the payment service provider (PSP) has a deadline. A call that times out leaves the payment in an unknown
state, and the service resolves that state by asking the PSP.

## Rules

### Every call has a deadline

- A call to the PSP **MUST** carry a timeout.
- A timeout **MUST** be set in configuration, so it moves without a release.
- The authorisation call **MUST** time out at 5 seconds.

_**Covers:** `eng:pol-PERF.TARGETS`, `eng:pol-RECV.TIMEOUT`_

### A retry is bounded and backs off

- A service **MUST** retry a connection failure or a `5xx`.
- A service **MUST NOT** retry a decline.
- A service **MUST** stop after two retries, with exponential backoff and jitter between them.
- A service **MUST** stop calling the PSP for 30 seconds once the failure rate passes the threshold configured against
  it.

_**Covers:** `eng:pol-RECV.DEGRADE`, `eng:pol-RECV.RETRY`_

### An unknown outcome is resolved

- A service **MUST** record a timed-out authorisation as unknown.
- A service **MUST** query the PSP for the outcome of an unknown authorisation, quoting the idempotency key from
  [std-IDEM.the-caller-chooses-the-key].
- A service **MUST** resolve every unknown outcome within 15 minutes, and raise an alert on one that is not.
- The checkout **MUST** tell the customer the payment is being confirmed.

_**Covers:** `eng:pol-RECV.DEGRADE`_

## Examples

```
Good
  authorise -> timeout at 5s
  record    -> ORD-4417 unknown
  poll      -> psp /charges?idempotency_key=ORD-4417:authorise
  record    -> ORD-4417 authorised, psp_ref=ch_9Kx2

Avoid
  authorise -> timeout at 5s
  record    -> ORD-4417 failed
```

The avoided line writes a fact nobody checked. The PSP may have taken the money, so the customer sees a failed order
and a debit on their statement.

## Conformance checklist

- [ ] Every PSP client in the repository is configured with a timeout.
- [ ] A decline is not retried, confirmed by a test.
- [ ] The retry policy stops after two attempts, and the backoff carries jitter.
- [ ] A forced timeout in a test environment leaves the payment marked unknown.
- [ ] The resolver clears an unknown outcome within 15 minutes, and alerts when it cannot.
- [ ] The checkout shows a pending message while the outcome is unknown.

## Rationale and provenance

A timeout is the absence of an answer. Treating it as a decline invents an outcome, and the customer's bank has the
real one.

The 5-second bound sits well above [nfr-0001], which asks for an answer inside 800ms at the 95th percentile. The
target governs the ordinary call and the timeout bounds the worst one.

## Sources and further reading

- **Informative.** [Exponential backoff and jitter] covers the backoff shape these rules require.

## Changelog

- 2026-08-31: initial version.

[Exponential backoff and jitter]: https://aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/
[nfr-0001]: ../../nfrs/0001-authorisation-latency.md
[std-IDEM.the-caller-chooses-the-key]: idempotency.md#the-caller-chooses-the-key
