---
id: std-0002
tier: normative
status: active
implements:
  - eng:pol-DATA.LOGS
  - eng:pol-SCRT.LOGS
  - eng:pol-SCRT.STORE
applies-to:
  - svc-payment-api
  - svc-payment-ledger
review-by: "2027-08-28"
owner: paul.law
tags: [ logging, observability, redaction ]
---

# A payment leaves an order reference in the logs and nothing else

`Standard: std-0002` `ACTIVE`

## Summary

A payment is traced by its order reference and by the PSP's own reference. A card token, a PSP key and a customer's
name and address stay out of every log, trace and error report.

## Rules

### What a log line may carry

- A log line **MUST** identify a payment by its order reference, its PSP reference, or both
  (`eng:pol-DATA.LOGS`).
- A log line **MUST NOT** carry a card token, in a message, a property or a structured field
  (`eng:pol-SCRT.LOGS`).
- A log line **MUST NOT** carry a customer's name, address, email address or telephone number
  (`eng:pol-DATA.LOGS`).

### The redaction runs before the sink

- A service **MUST** redact against a list of field names held in code and covered by a test, rather than against a
  pattern applied where the log is read.
- A service **MUST** redact a request or response body before it is written.
- A service **MUST NOT** leave redaction to the log platform, which sees the line only after every copy of it exists.
- An unhandled exception **MUST** be reported without the request body that caused it.

### A PSP key is a secret and behaves like one

- A service **MUST** read the PSP's secret key from the managed store at start-up, through the identity granted to that
  workload (`eng:pol-SCRT.STORE`).
- A service **MUST NOT** write a PSP key to a log, a console, an error report or a support ticket
  (`eng:pol-SCRT.LOGS`).

## Examples

```
Good
  Authorised order ORD-4417 as psp_ref=ch_9Kx2 in 612ms

Avoid
  Authorised order ORD-4417 for Alex Fenwick, token tok_live_9Kx2QpR, key sk_live_8fB2
```

The avoided line puts a customer's name, a reusable token and a live secret key into every place the logs are shipped,
searched and backed up. Rotating the key does not reach any of those copies.

## Conformance checklist

- [ ] The redaction field list is in the repository and a test asserts each entry is removed.
- [ ] A search of the last 30 days of logs for `tok_` and for `sk_` returns nothing.
- [ ] A search of the last 30 days of logs for a known test customer's surname returns nothing.
- [ ] Error reports carry a request id and no request body.
- [ ] The PSP key resolves from the managed store at start-up, and appears in no configuration file.

## Rationale and provenance

Logs are shipped, searched, exported and kept longer than anything else we run. A token written into one is a token in
every copy of it, and a copy is not covered by rotating the token at the PSP.

`eng:pol-SCRT` binds every secret and this standard says what its prohibition on logging means for a payment. That
obligation is discharged twice on purpose: `eng:pol-SCRT.LOGS` reaches the whole estate through the governance layer's
own secret-handling standard, and reaches a PSP key and a card token through this one.

- `eng:pol-DATA` commits us to keeping unmasked personal data out of logs and telemetry.
- `eng:pol-SCRT` commits us to holding secrets in a controlled store, and to keeping them out of logs.

## Changelog

- 2026-08-28: initial version.
