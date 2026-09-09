---
id: fix-0001
type: fix
tier: normative
status: active
symptom-keywords: [authorisation, "double charge", duplicate, idempotency-key, pending, retry, statement, timeout]
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

A customer reports two pending amounts for one order. The ledger holds two authorisations against the same order
reference, seconds apart, each with its own PSP reference and its own `Idempotency-Key`. Nothing failed: both calls
returned `201`.

## Cause

The caller generated a fresh `Idempotency-Key` for each attempt, so the second request read as a new payment rather
than as a retry of the first. [std-IDEM] asks for a key derived from the order and the operation, and a fresh UUID per
attempt satisfies the header without satisfying the rule.

## Resolution

1. Void the later authorisation with the PSP, which releases the hold on the cardholder's account.
2. Write the reversal to the ledger as its own entry, because [std-LEDGER] corrects an entry by another entry.
3. Change the caller to derive its key from the order and the operation, as `<order>:<operation>`.
4. Replay the retry against the PSP sandbox.
5. Check the second call returns the first outcome rather than a second authorisation.

## Why it happens

A UUID per attempt is what an HTTP client library generates by default, and the request passes every check the API
makes: the header is present, well formed and unique. The rule that a retry reuses its key lives in the caller, and
nothing on this side can tell a retry from a new order.

## How we found it

Match the two ledger entries on the order reference rather than on the key. Two entries seconds apart with different
keys are a caller problem, and two with the same key are ours.

## Related

* [std-IDEM] is the rule the caller broke.
* [std-LEDGER] says how the reversal is written.
* [svc-payment-api] takes the authorisation request.

[std-IDEM]: ../standards/authorisation/idempotency.md
[std-LEDGER]: ../standards/ledger/entries.md
[svc-payment-api]: ../services/payment-api.md
