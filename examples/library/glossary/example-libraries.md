---
id: gls-example-libraries
type: glossary
tier: descriptive
status: draft
owner: human:robin.hale
review-by: "2027-08-12"
tags: [ catalogue, lending ]
---

# Example Libraries

`Glossary: gls-example-libraries` `DRAFT`

The words the consortium uses across its systems.

## Scope

The vocabulary every team here shares: readers, what they borrow, and where they borrow it from. A word one system
uses differently belongs in that system's glossary, and the entry here points at it.

## Terms

### Borrower

A person with a library card. One branch issues the card and every branch accepts it.

**Not:** a reader. Anyone may read in a branch without a card.

### Branch

One physical library building, with its own opening hours, staff and shelves.

**Not:** a branch in source control. Both senses appear in the same sentence often enough to need separating.

### Item

One physical copy of a title, with its own barcode, shelved at one branch.

**Not:** a title. A popular title is one work and thirty items. Only an item can be lent.

### Record

The bibliographic description of a title (author, edition, subject headings), stored once and used by every branch.

**Not:** a knowledge record in this repository, which is a document filed under a type. See
[gls-knowledge-as-code.record].

Owned by [svc-catalogue-api].

### Title

A work the consortium has catalogued. The copies of it on the shelves are items.

**Not:** the indexed field of the same name. See [gls-search.title].

Owned by [svc-catalogue-api].

[gls-knowledge-as-code.record]: knowledge-as-code.md#record
[gls-search.title]: search.md#title
[svc-catalogue-api]: ../services/catalogue-api.md
