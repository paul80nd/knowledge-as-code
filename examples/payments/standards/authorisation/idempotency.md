---
id: std-IDEM
tier: normative
status: active
implements:
  - eng:pol-INTC.SPEC
  - eng:pol-RECV.IDEMPOT
applies-to:
  - svc-payment-api
  - svc-payment-ledger
review-by: "2027-08-31"
owner: paul.law
tags: [ authorisation, idempotency, retries ]
---

# One order pays once, however many times the request arrives

`Standard: std-IDEM` `ACTIVE`

## Summary

Every request that moves money carries an idempotency key chosen by the caller. A repeat of that key returns the first
outcome and charges nobody a second time.

## Rules

### The caller chooses the key

- A request that authorises, captures or refunds **MUST** carry an `Idempotency-Key` header
  (`eng:pol-RECV.IDEMPOT`).
- The caller **MUST** derive the key from the order and the operation, so a retry of one attempt produces the same key
  (`eng:pol-RECV.IDEMPOT`).
- A service **MUST** reject a request with no key, rather than treating it as a new payment
  (`eng:pol-RECV.IDEMPOT`).
- The contract **MUST** state the key's format and how long it is honoured (`eng:pol-INTC.SPEC`).

### The service answers a repeat from its record

- A service **MUST** store the key with the outcome before it answers the first request (`eng:pol-RECV.IDEMPOT`).
- A service **MUST** return the stored outcome for a repeated key, with the same status and body
  (`eng:pol-RECV.IDEMPOT`).
- A service **MUST** honour a key for at least 24 hours (`eng:pol-RECV.IDEMPOT`).
- A service **MUST** answer `422` where a repeated key arrives with a different amount or a different order, rather
  than charging either one (`eng:pol-RECV.IDEMPOT`).
- A service **MUST** pass the key to the PSP as the PSP's own idempotency key, so a retry stops at whichever hop
  already answered (`eng:pol-RECV.IDEMPOT`).

### An in-flight repeat waits or is told to wait

- A service **MUST** answer `409` where the key is recorded and the first request has not finished
  (`eng:pol-RECV.IDEMPOT`).
- A service **MUST NOT** start a second call to the PSP while the first is in flight for the same key
  (`eng:pol-RECV.IDEMPOT`).

## Examples

```
Good
  POST /v1/authorisations
  Idempotency-Key: ORD-4417:authorise
  { "order": "ORD-4417", "amount": 2599, "token": "tok_9Kx2" }

Avoid
  POST /v1/authorisations
  Idempotency-Key: 6f1c8a52-4d9e-4c3b-b0a1-2f77ce9d1a04     # new for every attempt
```

The avoided key is fresh on each retry, so the customer is charged once per timeout the network produces.

## Conformance checklist

- [ ] The contract documents the header, its format and its lifetime.
- [ ] Sending the same request twice returns one payment and the same response body.
- [ ] Sending the same key with a different amount returns `422` and charges nothing.
- [ ] A request with no key is refused.
- [ ] The key reaching the PSP is the key the caller sent.
- [ ] The stored keys survive a restart of the service.

## Rationale and provenance

A timeout tells the caller nothing about whether the money moved. Without a key the safe action is to give up, and with
one the safe action is to retry, which is the difference between a lost payment and a slow one.

- `eng:pol-INTC` commits us to publishing the contract a caller writes against.
- `eng:pol-RECV` commits us to making an operation that may be retried safe to re-run.

## Sources and further reading

- **Normative.** [The IETF Idempotency-Key header field] defines the header, its semantics and the `409` and `422`
  responses these rules require.

## Changelog

- 2026-08-31: initial version.

[The IETF Idempotency-Key header field]: https://datatracker.ietf.org/doc/draft-ietf-httpapi-idempotency-key-header/
