---
id: adr-0001
type: adr
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: human:alex.doe
---

# A decision resting on a service

`ADR: adr-0001` `ACCEPTED`

> **In the context of** the `decisions-cite-no-descriptive-record` rule, **facing** an ADR whose constraint rests on a
> living record, **we decided** to link a service from Decision, **rather than** stating the constraint, **to achieve**
> coverage of the warning, **accepting** one finding against this record.

## Context

[Notifications](../services/notifications.md) sends every borrower message. A link here is background, and passes.

## Decision

Send every message through [svc-notifications], which the rule reports. A reference-style link is judged where it is
used, so the definition below sits outside this section and the finding still lands here.

## Alternatives Considered

* **Link the service from Context alone** — rejected; the rule would have nothing to report.

## Consequences

The golden pins one warning for this document.

[svc-notifications]: ../services/notifications.md
