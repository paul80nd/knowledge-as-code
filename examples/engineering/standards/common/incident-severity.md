---
id: std-INCSEV
type: standard
tier: normative
status: draft
implements: [ pol-INCR.COMMS, pol-INCR.LEARN, pol-INCR.TRIAGE ]
applies-to:
  - all
review-by: "2027-10-19"
owner: human:paul.law
tags: [ incidents, on-call, postmortems, severity ]
---

# Every incident is handled at a stated severity

`Standard: std-INCSEV` `DRAFT`

## Summary

An incident takes one of three severities when it is declared. The severity decides who is paged, how often the people
affected hear about it, and whether it gets a postmortem. Wherever a policy here says a significant incident, it means
sev1 or sev2.

## Rules

### Every incident has a severity

- Whoever declares an incident **MUST** set its severity at declaration.
- An incident **MUST** be sev1 where a `critical` service is unavailable, or where customers' money has been taken,
  refunded or paid out wrongly, or their personal data is wrong or exposed, at any scale.
- An incident **MUST** be sev2 where a service is degraded or a fault reaches some customers, and a workaround exists or
  the harm is contained.
- An incident **MUST** be sev3 where no customer is affected, or one customer is affected with no lasting harm.
- An incident meeting none of these **MUST** be sev2 until the incident lead sets another level.

_**Covers:** [pol-INCR].TRIAGE_

### The severity moves with what is known

- The incident lead **MUST** raise the severity as soon as the incident meets a higher level.
- The incident lead **SHOULD** lower the severity once the incident no longer meets its level, because a sev1 response
  held open costs the people paged for it.
- A lowered severity **MUST** have its reason recorded in the incident record.
- A postmortem **MUST** state the highest severity the incident reached.

_**Covers:** [pol-INCR].TRIAGE_

### The severity sets the response

- A sev1 or sev2 **MUST** have a named incident lead.
- A sev1 **MUST** page the on-call engineer as soon as it is declared.
- A sev1 **MUST** tell the people it affects its status at least every 30 minutes.
- A sev2 **MUST** page the on-call engineer.
- A sev2 **MUST** tell the people it affects its status at least every 2 hours.
- A sev3 **MAY** be handled as routine work, in working hours.

_**Covers:** [pol-INCR].COMMS, [pol-INCR].TRIAGE_

### A significant incident is reviewed

- A sev1 or sev2 incident **MUST** be treated as significant wherever a policy here names one.
- A sev1 or sev2 incident **MUST** have a postmortem.
- A personal data breach **MUST** have a postmortem, whatever its severity.

_**Covers:** [pol-INCR].LEARN_

## Examples

```
Good
  21:16  declared sev1: ledger writes failing, and customers are charged for orders the ledger does not record
  21:16  incident lead named, on-call engineer paged
  21:46  status page updated for the customers affected

Avoid
  21:16  declared sev3: only twelve minutes so far
```

The second sets the level by how long the fault has lasted. A customer charged for an order the ledger does not record
has had money taken wrongly, which makes it sev1 from the first minute.

## Conformance checklist

- [ ] Every incident record states the severity it was declared at.
- [ ] Every sev1 and sev2 paged the on-call engineer.
- [ ] Every sev1 and sev2 names its incident lead.
- [ ] Every sev1 posted its status at least every 30 minutes, and every sev2 at least every 2 hours.
- [ ] Every lowered severity has its reason in the incident record.
- [ ] Every sev1 and sev2, and every personal data breach, has a postmortem stating the highest severity reached.

## Rationale and provenance

A scale applied the same way each time is what makes the absence of a postmortem mean something. The levels turn on
harm to customers first, because a short incident that took money wrongly matters more than a long one that only
slowed a page. A card hold released before capture takes no money, so it is sev2 where nothing else makes it sev1.

## Sources and further reading

- **Informative.** [PagerDuty's postmortem process] writes a postmortem for every SEV-1 and SEV-2, which is the
  threshold this standard uses.

## Changelog

- 2026-10-19: initial version.

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[PagerDuty's postmortem process]: https://response.pagerduty.com/after/post_mortem_process/
[pol-INCR]: ../../policies/operations/incr-incident-response.md#clauses
