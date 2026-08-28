# Frameworks

The external frameworks this corpus refers to, and what each one obliges us to.

A policy maps its clauses to a framework's controls in the `Alignment` column of its clause table, as
`[ISO 27001:2022].A.8.24`. Those references resolve here. This page is the only place that says what the relationship
is, because that standing changes on its own schedule and would otherwise need correcting in every policy that cites it
at once.

Maintained by hand: no frontmatter, no id, no index. It exists so the references in the corpus have somewhere honest to
land.

## The three postures

| Posture            | What it means                                                                                                                     |
|--------------------|-----------------------------------------------------------------------------------------------------------------------------------|
| **Obliged**        | Something outside us requires it: certification we hold, or law that applies to us. Not optional.                                 |
| **Self-obligated** | Nothing external compels it; a policy of ours does. Binding on us because we said so, and revocable only by changing that policy. |
| **Inspiration**    | We took ideas from it. It shapes our thinking and binds nothing. A clause may cite it for provenance, not for obligation.         |

"Because we are certified against it" and "because it seemed sensible" are different answers to *why does this clause
exist*, and only one of them survives a change of mind.

Write one `###` heading per framework, and say plainly what binds this organisation to it: the certification held, the
law that applies, the contract that requires it. Say which parts of it this corpus answers for, and where the rest is
answered, so a reader cannot mistake a partial mapping for full coverage.

### PCI DSS v4.0

The card schemes require it of any merchant taking card payments, through the acquiring bank's contract. It binds this
corpus because payments is where cards are taken.

This corpus answers for the merchant's own systems, which is a small part of the standard: the card details never reach
them, so the assessment available to us is the shortest one. What the payment service provider answers for is in their
own attestation, and [std-0001] holds the contract to naming which requirements sit on which side.

The engineering corpus maps its policies to ISO 27001 and to UK GDPR rather than to this. A policy stating a commitment
for the whole organisation is not the place for a standard binding one bounded context.

[std-0001]: standards/domain/card-data.md

## Self-obligated

_Nothing recorded yet._

A framework belongs here when a policy of ours is the only thing holding us to it. Name that policy, so the obligation
can be traced to the decision that created it and dropped by changing that decision.

## Inspiration

Frameworks that shaped the thinking and bind nothing. Keep the section, even when it is the longest: an idea taken
openly is easier to argue with than one absorbed silently.

_Nothing recorded yet._
