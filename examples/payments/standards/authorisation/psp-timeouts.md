---
id: std-PSPOUT
type: standard
tier: normative
status: active
implements: [ eng:pol-PERF.TARGETS, eng:pol-RECV.DEGRADE, eng:pol-RECV.RETRY, eng:pol-RECV.TIMEOUT ]
applies-to:
  - svc-payment-api
review-by: "2027-08-31"
owner: human:paul.law
tags: [ psp, resilience, timeouts ]
---

# A call to the PSP is bounded, and an unknown outcome is resolved

`Standard: std-PSPOUT` `ACTIVE`

## Summary

Every call to the payment service provider (PSP) has a deadline. A call that times out leaves the payment in an unknown
state. The service resolves that state by asking the PSP what happened.

## Rules

### Every call has a deadline

- A call to the PSP **MUST** have a timeout set in its configuration, so it can change without a release.
- The authorisation call **SHOULD** time out at 5 seconds.

_**Covers:** `eng:pol-PERF.TARGETS`, `eng:pol-RECV.TIMEOUT`_

### A retry is bounded and backs off

- A service **SHOULD** retry a connection failure or a `5xx`.
- A service **MUST NOT** retry a decline, because a decline is the issuer's answer and the card schemes fine a
  merchant that keeps retrying one.
- A service **MUST** stop retrying at a limit set in its configuration.
- A service **MUST** back off between attempts.
- That limit **SHOULD** be two retries.
- The backoff **SHOULD** be exponential, with jitter.
- A service **MUST** stop calling the PSP for a period set in its configuration once the failure rate passes its
  configured threshold.
- That period **SHOULD** be 30 seconds.

_**Covers:** `eng:pol-RECV.DEGRADE`, `eng:pol-RECV.RETRY`_

### An unknown outcome is resolved

- A service **MUST** record a timed-out authorisation as unknown.
- A service **MUST** query the PSP for the outcome of an unknown authorisation, quoting the idempotency key from
  [std-IDEM.the-caller-chooses-the-key].
- A service **MUST** resolve every unknown outcome within a window set in its configuration.
- A service **MUST** raise an alert on an unknown outcome it has not resolved within that window.
- That window **SHOULD** be 15 minutes.
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

The avoided line records a fact nobody checked. The PSP may have taken the money, so the customer sees a failed order
and a debit on their statement.

## Conformance checklist

- [ ] Every PSP client in the repository is configured with a timeout.
- [ ] A decline is not retried, confirmed by a test.
- [ ] The retry policy stops at its configured limit, and backs off between attempts.
- [ ] Calls to the PSP stop for the configured period once the failure rate passes its threshold.
- [ ] A forced timeout in a test environment leaves the payment marked unknown.
- [ ] The resolver clears an unknown outcome within its configured window, and alerts when it cannot.
- [ ] The checkout shows a pending message while the outcome is unknown.

## Rationale and provenance

A timeout is the absence of an answer. Treating it as a decline invents an outcome, and the customer's bank has the
real one.

The 5-second default is well above [nfr-0001], which asks for an answer inside 800ms at the 95th percentile. The target
governs the ordinary call, and the timeout bounds the slowest one.

## Sources and further reading

- **Informative.** [Exponential backoff and jitter] covers the backoff shape these rules ask for.

## Changelog

- 2026-10-18: each bound stays a `MUST`, with its value set in configuration. The 5-second timeout, two retries, an
  exponential backoff with jitter, the 30-second pause and the 15-minute window become `SHOULD` defaults, and so does
  retrying a `5xx`.
- 2026-08-31: initial version.

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[Exponential backoff and jitter]: https://aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/
[nfr-0001]: ../../nfrs/0001-authorisation-latency.md
[std-IDEM.the-caller-chooses-the-key]: idempotency.md#the-caller-chooses-the-key
