---
id: std-LEDGER
tier: normative
status: active
implements:
  - eng:pol-DERV.EXPECT
  - eng:pol-DERV.LINEAGE
applies-to:
  - svc-payment-ledger
review-by: "2027-08-31"
owner: paul.law
tags: [ append-only, double-entry, ledger ]
---

# A ledger entry is written once and corrected by another entry

`Standard: std-LEDGER` `ACTIVE`

## Summary

Every movement of money is a balanced pair of entries, written once and never amended. A mistake is corrected by a
further pair, so the sequence finance read on the day is the sequence they read a year later.

## Rules

### An entry is a balanced pair

- A movement of money **MUST** be written as a debit and a credit of the same amount (`eng:pol-DERV.EXPECT`).
- An entry **MUST** carry the amount in minor units as an integer, with its ISO 4217 currency code
  (`eng:pol-DERV.EXPECT`).
- An entry **MUST NOT** carry an amount in a floating-point type, at rest or in transit (`eng:pol-DERV.EXPECT`).
- A pair of entries **MUST** be written in one transaction, so no reader ever sees one half (`eng:pol-DERV.EXPECT`).

### Nothing amends an entry

- The ledger **MUST** refuse an update or a delete on a written entry (`eng:pol-DERV.LINEAGE`).
- A correction **MUST** be a reversing pair followed by the intended pair, each naming the entry it corrects
  (`eng:pol-DERV.LINEAGE`).
- An entry **MUST** carry the time the event happened and the time it was written, where the two differ
  (`eng:pol-DERV.LINEAGE`).

### An entry says what produced it

- An entry **MUST** name the order, the PSP reference and the idempotency key from [std-IDEM]
  (`eng:pol-DERV.LINEAGE`).
- An entry **MUST** name the event that produced it: an authorisation, a capture, a refund or a chargeback
  (`eng:pol-DERV.LINEAGE`).
- The ledger **MUST** be able to answer a payment's full history from its entries alone (`eng:pol-DERV.LINEAGE`).

## Examples

```
Good
  ORD-4417  capture   debit  psp_receivable   2599 GBP   entry 90114
  ORD-4417  capture   credit revenue          2599 GBP   entry 90115
  ORD-4417  reversal  credit psp_receivable   2599 GBP   entry 90230  reverses 90114
  ORD-4417  reversal  debit  revenue          2599 GBP   entry 90231  reverses 90115

Avoid
  UPDATE entries SET amount = 2499 WHERE id = 90114
```

The avoided statement leaves no record that the figure ever was 2599, so the reconciliation that already matched the
old figure now disagrees with the ledger and nothing explains why.

## Conformance checklist

- [ ] Every amount column is an integer type, and every entry carries a currency code.
- [ ] The database grants the service no UPDATE or DELETE on the entries table.
- [ ] Summing debits and credits across the whole table returns zero.
- [ ] Every correction in the last month appears as a reversal naming the entry it reverses.
- [ ] Each entry names an order, a PSP reference and an idempotency key.
- [ ] A payment's history can be rebuilt from the entries with no other source.

## Rationale and provenance

Finance reconciles against this table, and an auditor reads it. An amended row makes both of them wrong about the past,
and neither can tell that it happened.

- `eng:pol-DERV` commits us to stating what a correct output looks like, and to tracing a value back to its source.

## Sources and further reading

- **Normative.** [ISO 4217] sets the currency codes and the minor-unit count each entry uses.

## Changelog

- 2026-08-31: initial version.

[ISO 4217]: https://www.iso.org/iso-4217-currency-codes.html
[std-IDEM]: ../authorisation/idempotency.md
