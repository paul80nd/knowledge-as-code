---
id: dat-loans
type: data
tier: descriptive
status: active
owned-by: svc-lending
classification: confidential
personal-data: personal
data-subjects:
  - borrowers
retention: The borrower link is cleared two years after the item comes back.
region: UK South
flows-to:
  - svc-notices
  - svc-reservations
review-by: "2027-03-31"
owner: human:dev.raman
tags: [ holds, loans ]
---

# Loans and Holds

`Data: dat-loans` `ACTIVE`

What a borrower has out, what they have reserved, and what they owe on it.

## Purpose

* **Issuing and returning an item.** A loan row is what says an item is out and when it is due.
* **Placing a borrower in a queue.** A hold row sets the order in which an item goes out next.
* **Charging for an overdue or damaged item.** A fine row is the debt [dat-payments] settles.
* **Reporting use to the consortium.** Loan counts by item and branch go to the annual return, read after the card
  number has been cleared.

The lawful basis for all four is public task, the same as [dat-borrowers].

## Entities

* **Loan**: one item issued to one borrower. The item barcode, the card number, the branch, the date out, the date due
  and the date back.
* **Renewal**: an extension of a loan. Who asked, when, and the new due date.
* **Hold**: a request for an item that is out. The card number, the title wanted, the collection branch and the queue
  position.
* **Fine**: money owed on an overdue or damaged item. The amount, the reason and whether it is paid.

_(Names and meanings, not schemas. Schemas live with the code that owns them.)_

## Where it lives

|                    |                                                 |
|--------------------|-------------------------------------------------|
| **Owning service** | [svc-lending]                                   |
| **Store**          | The legacy database, in the `CIRC` table family |

The legacy circulation API writes here as well, so a counter issue and an API issue reach the same rows by two routes.

## Classification

`confidential`, and access to a historic loan is limited to the branch that issued the item. `personal-data` is
`personal`, because a loan says what a named person read and when.

A loan arrives here as a card number, and [dat-borrowers] says who that number belongs to.

`personal-data` is `personal` and not `special-category`, and the field cannot express what sits between them. A
borrowing record can reveal health, religion or sexuality without holding a single Article 9 field. The consortium
records the category GDPR gives it and protects the data as though it were more than that.

## Retention

A returned loan keeps its card number for two years, then the link is cleared and the loan survives as a count against
the item and the branch. An open loan is kept until the item comes back or is written off.

A fine is kept for six years after it is paid or written off, because it is a debt.

**Two years is the policy and no job enforces it.** The clearing script is run by hand when somebody remembers, and
nothing records when that last happened.

A borrower may ask for their history to be cleared early, and [svc-lending] has no route for it. The request is carried
out against the database directly.

## Flows

| Goes to            | Why                                | What is shared                       | Where they process it |
|--------------------|------------------------------------|--------------------------------------|-----------------------|
| [svc-notices]      | Overdue, due-soon and hold notices | Card number, title, due date, branch | UK South              |
| [svc-reservations] | Showing a queue position           | Card number, title, queue position   | UK South              |

Nothing here leaves the estate. A notice that reaches a borrower by email includes the title, so the title crosses to
[int-mail-delivery] through [dat-borrowers] and not through this domain.

## Related

* [dat-borrowers] says who a card number belongs to.
* [svc-lending] owns this data and is the only route to it.

---

_(**Never put actual data here**: no sample records, no identifiers, no connection strings. This corpus is broadly
readable.)_

[dat-borrowers]: borrowers.md
[dat-payments]: payments.md
[int-mail-delivery]: ../integrations/mail-delivery.md
[svc-lending]: ../services/lending.md
[svc-notices]: ../services/notices.md
[svc-reservations]: ../services/reservations.md
