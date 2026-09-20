---
id: dat-borrowers
type: data
tier: descriptive
status: active
owned-by: svc-lending
classification: confidential
personal-data: personal
data-subjects:
  - borrowers
retention: Six years after the membership lapses, then the row is anonymised.
region: UK South
flows-to:
  - itg-identity
  - itg-mail-delivery
  - svc-notices
review-by: "2027-03-31"
owner: human:dev.raman
tags: [ borrowers, membership ]
---

# Borrowers

`Data: dat-borrowers` `ACTIVE`

Who holds a library card, how the consortium contacts them, and what their card entitles them to.

## Purpose

* **Running the loan service.** A card number identifies the borrower a loan, a hold or a fine belongs to.
* **Reaching a borrower.** An address, an email address and a telephone number let [svc-notices] send an overdue or a
  hold notice.
* **Setting an entitlement.** A category sets the loan allowance and the fine rate.
* **Counting use by branch.** Loan counts by branch and category go to the consortium's annual return, read from
  anonymised rows.

The lawful basis for all four is public task. The consortium runs a statutory library service, and membership is how
it does so.

## Entities

* **Borrower**: a person with a library card. Name, date of birth, postal address, email address and telephone number.
* **Membership**: the card itself. Card number, the branch that issued it, the start and expiry dates, and the
  category the borrower falls in.
* **Category**: adult, junior, housebound or staff. It sets the loan allowance and the fine rate.
* **Contact preference**: whether the borrower takes notices by email, by post, or not at all.

_(Names and meanings, not schemas. Schemas live with the code that owns them.)_

## Where it lives

|                    |                                                     |
|--------------------|-----------------------------------------------------|
| **Owning service** | [svc-lending]                                       |
| **Store**          | The legacy database, in the `BORROWER` table family |

[svc-lending] maps this data and did not design it. The legacy library management system still writes to the same
tables, so a change made at a branch counter appears here without the service seeing it. That second writer is the
reason a correction requested by a borrower has to be made in both places.

A copy of the name and the email address also sits in the [itg-identity] tenant, which the vendor runs.

## Classification

`confidential`, because a borrower's details are shown to the borrower and to the staff serving them, and to nobody
else. `personal-data` is `personal`: a name, an address and a date of birth identify a living person on their own.

The consortium keeps no staff or supplier data in this domain, and a staff member with a card appears here as a
borrower.

Date of birth is held to set the category and to separate two borrowers with the same name. `personal-data` is not
`special-category`: the housebound category records a delivery arrangement and never a medical reason.

## Retention

Six years after the membership lapses, which is the limitation period for a debt on an unreturned item. A membership
lapses three years after the last loan, so a dormant card is held for nine years in total.

Deletion is implemented as anonymisation. The row survives with the identifying fields cleared, because
[svc-lending] reports loan counts by branch and category and those counts must not move.

**The anonymisation job runs monthly and has never been checked against the legacy library management system.** That
system can create a borrower the job has not seen, and nothing reconciles the two.

## Flows

| Goes to             | Why                                    | What is shared                          | Where they process it |
|---------------------|----------------------------------------|-----------------------------------------|-----------------------|
| [itg-identity]      | Sign-in                                | Name, email address, card number        | UK                    |
| [svc-notices]       | Overdue and hold notices               | Name, email address, contact preference | UK South              |
| [itg-mail-delivery] | Sending the notice [svc-notices] wrote | Name, email address                     | Ireland               |

[itg-mail-delivery] is the only recipient outside the estate that receives an address, and the only one processing
outside the UK. A transfer out of the country takes place on every notice sent.

## Related

* [svc-lending] owns this data and is the only route to it.
* [dat-loans] refers to a borrower by card number.

---

_(**Never put actual data here**: no sample records, no identifiers, no connection strings. This corpus is broadly
readable.)_

[dat-loans]: loans.md
[itg-identity]: ../integrations/identity.md
[itg-mail-delivery]: ../integrations/mail-delivery.md
[svc-lending]: ../services/lending.md
[svc-notices]: ../services/notices.md
