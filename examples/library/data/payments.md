---
id: dat-payments
type: data
tier: descriptive
status: active
owned-by: svc-lending
classification: confidential
personal-data: personal
data-subjects:
  - borrowers
  - staff
retention: Seven years from the end of the financial year the payment falls in.
region: UK South
flows-to:
  - int-card-payments
review-by: "2027-03-31"
owner: human:mira.okonjo
tags: [ fines, payments ]
---

# Payments

`Data: dat-payments` `ACTIVE`

What a borrower has paid, what it was for, and the receipt the consortium keeps for its auditor.

## Purpose

* **Taking money for a fine or a fee.** A payment row is what says a debt is settled.
* **Refunding a borrower.** A refund row is the evidence that money went back.
* **Proving the account to an auditor.** Seven years of payments and refunds are what the consortium's auditor reads.
* **Matching what the card provider settled.** A reconciliation line finds a payment the estate recorded and the
  provider did not, or the reverse.

The lawful basis for taking and refunding money is public task. For the audit trail and the reconciliation it is legal
obligation, which is why those two outlive the debt they record.

## Entities

* **Payment**: money taken from a borrower. The amount, the date, the branch or the website, and the borrower's card
  number.
* **Allocation**: which fine or fee a payment cleared. One payment can clear several.
* **Refund**: money returned. The original payment, the amount, the reason and who authorised it.
* **Reconciliation line**: what the card provider says it settled, matched against what the estate recorded.

_(Names and meanings, not schemas. Schemas live with the code that owns them.)_

## Where it lives

|                    |                                                |
|--------------------|------------------------------------------------|
| **Owning service** | [svc-lending]                                  |
| **Store**          | The legacy database, in the `FIN` table family |

**No payment card details are held here.** [int-card-payments] returns a token and the last four digits of the payment
card, and those two values are what a payment row keeps. The payment card number, its expiry date and its security
code never enter the estate.

## Classification

`confidential`, because the finance team's access rule governs these rows and nobody else reads them. `personal-data`
is `personal`, because a payment names a borrower through their library card number and says what they owed.

`data-subjects` names staff as well as borrowers, because a refund records who authorised it. That is the one place
this domain keeps data about somebody who is not a borrower.

The estate is the controller of these rows. [int-card-payments] is the processor for the card transaction itself, and
keeps the card details this domain never sees.

## Retention

Seven years from the end of the financial year the payment falls in, which is what the consortium's auditor requires.
A refund is kept for the same seven years from the refund date and not the payment date.

Deletion is implemented and runs annually against the finance tables. It has run every year since 2021.

A reconciliation line is kept for two years. That is shorter than the payment it matches, so an audit reaching back
more than two years reads the payment rows alone.

## Flows

| Goes to             | Why                        | What is shared                        | Where they process it |
|---------------------|----------------------------|---------------------------------------|-----------------------|
| [int-card-payments] | Taking and refunding money | Amount, payment reference, card token | UK and Ireland        |

The consortium never sends a borrower's name or address with a payment, so what crosses to Ireland is a pseudonymous
reference and an amount.

## Related

* [dat-loans] records the fine a payment clears.
* [int-card-payments] takes the money and keeps the card details.

---

_(**Never put actual data here**: no sample records, no identifiers, no connection strings. This corpus is broadly
readable.)_

[dat-loans]: loans.md
[int-card-payments]: ../integrations/card-payments.md
[svc-lending]: ../services/lending.md
