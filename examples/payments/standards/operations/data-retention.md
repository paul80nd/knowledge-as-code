---
id: std-RETAIN
tier: normative
status: active
implements:
  - eng:pol-DATA.DELETE
  - eng:pol-DATA.LINGER
  - eng:pol-DATA.RIGHTS
  - eng:pol-OBSV.RETAIN
applies-to:
  - svc-payment-api
  - svc-payment-ledger
review-by: "2027-08-31"
owner: paul.law
tags: [ erasure, retention, tokens ]
---

# A payment record is kept for seven years and nothing else is

`Standard: std-RETAIN` `ACTIVE`

## Summary

The ledger keeps its entries for seven years because tax law asks for them. Everything around a payment goes sooner:
tokens at 13 months, logs at 90 days, and the customer's contact details when the order closes.

## Rules

### Each store has one period

- The ledger **MUST** keep an entry for seven years from the date it was written (`eng:pol-DATA.LINGER`).
- A service **MUST** delete a card token 13 months after its last use (`eng:pol-DATA.LINGER`).
- The telemetry platform **MUST** delete a payment record after 90 days (`eng:pol-OBSV.RETAIN`).
- A store **MUST NOT** hold personal data with no stated period against it (`eng:pol-DATA.LINGER`).

### Deletion happens without anyone asking

- A scheduled job **MUST** delete data that has passed its period (`eng:pol-DATA.DELETE`).
- That job **MUST** run at least weekly (`eng:pol-DATA.DELETE`).
- The job **MUST** record what it deleted, by store and by count (`eng:pol-DATA.DELETE`).
- A backup policy **MUST** age a backup out on the same period as the store it came from (`eng:pol-DATA.DELETE`).

### An erasure request is answered

- A service **MUST** remove the customer's contact details on an erasure request (`eng:pol-DATA.RIGHTS`).
- A service **MUST** replace the customer's name in the ledger with the order reference (`eng:pol-DATA.RIGHTS`).
- A service **MUST NOT** delete a ledger entry on an erasure request (`eng:pol-DATA.RIGHTS`).
- A service **MUST** answer an erasure request within one month of receiving it (`eng:pol-DATA.RIGHTS`).

A ledger entry survives an erasure request because a tax record is a legal obligation, and the order reference is
enough to keep the books without keeping the name.

## Examples

```
Good
  entries       7 years    tax record
  card_tokens   13 months  chargeback window
  telemetry     90 days    investigation window
  contacts      on order close

Avoid
  entries       7 years
  card_tokens   -
  telemetry     -
```

The avoided table states one period and leaves two stores keeping everything forever, which is the state a store
reaches when nobody chooses.

## Conformance checklist

- [ ] Every store holding payment data has a period written against it in the repository.
- [ ] The retention job has run in the last week, and its record says what it deleted.
- [ ] A query for tokens last used over 13 months ago returns nothing.
- [ ] The telemetry platform is configured to 90 days.
- [ ] An erasure request run against a test customer leaves the ledger entry and removes the name.
- [ ] Backup lifecycle rules match the periods of the stores they hold.

## Rationale and provenance

Data we no longer need is data we can still lose. Seven years covers the six-year tax record with a year in hand. The
chargeback window sets 13 months, and an investigation rarely reaches past 90 days.

- `eng:pol-DATA` commits us to deleting personal data once its purpose ends, and to answering the rights people hold
  over it.
- `eng:pol-OBSV` commits us to retaining telemetry long enough to investigate and no longer.

## Sources and further reading

- **Normative.** [HMRC record-keeping for VAT] sets the six-year floor the seven-year ledger period is taken from.
- **Informative.** [The ICO guide to the right to erasure] covers the exemption a legal obligation gives.

## Changelog

- 2026-08-31: initial version.

[HMRC record-keeping for VAT]: https://www.gov.uk/vat-record-keeping
[The ICO guide to the right to erasure]: https://ico.org.uk/for-organisations/uk-gdpr-guidance-and-resources/individual-rights/individual-rights/right-to-erasure/
