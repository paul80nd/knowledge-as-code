---
id: fix-0001
type: fix
tier: normative
status: active
symptom-keywords: [ authorisation, "double charge", duplicate, idempotency-key, pending, retry, statement, timeout ]
applies-to:
  - svc-payment-api
  - svc-payment-ledger
verified:
  - { at: 2026-08-14T10:20:00Z, by: human:alex.doe }
review-by: "2027-03-09"
owner: human:alex.doe
tags: [ authorisation, idempotency, retries ]
---

# One order shows two authorisations on the cardholder's statement

`Fix: fix-0001` `ACTIVE`

## Symptom

A customer reports two pending amounts for one order. The ledger contains two authorisations against the same order
reference, seconds apart. Each has its own PSP reference and its own `Idempotency-Key`. Nothing failed: both calls
returned `201`.

## Cause

The caller generated a fresh `Idempotency-Key` for each attempt, so [svc-payment-api] took the second request for a new
payment. [std-IDEM] requires a key derived from the order and the operation. A fresh UUID per attempt fills the header
and breaks that rule.

## Resolution

1. Void the later authorisation with the PSP, which releases the hold on the cardholder's account.
2. Write the reversal to the ledger as its own entry, because [std-LEDGER] corrects an entry by another entry.
3. Change the caller to derive its key from the order and the operation, as `<order>:<operation>`.
4. Replay the retry against the PSP sandbox.
5. Check the second call returns the first outcome and not a second authorisation.

## Why it happens

A fresh UUID is the obvious way to fill a header that asks for a unique value. The request then passes every check the
API makes: the header is present, well formed and unique. Only the caller can apply the rule that a retry reuses its
key, and [svc-payment-api] cannot tell a retry from a new order.

## How we found it

Match the ledger entries on the order reference, not on the key. Two entries seconds apart with different keys are a
caller problem. Two with the same key are a fault in these services.

## Related

* [std-IDEM] is the rule the caller broke.
* [std-LEDGER] says how the reversal is written.
* [svc-payment-api] takes the authorisation request.

[std-IDEM]: ../standards/authorisation/idempotency.md
[std-LEDGER]: ../standards/ledger/entries.md
[svc-payment-api]: ../services/payment-api.md
