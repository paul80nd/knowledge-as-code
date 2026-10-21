---
id: itg-{{slug}}
type: integration
tier: descriptive
status: active
vendor:
used-by:
criticality:
owner:
tags: [ a, b ]
---

# {{Integration name}}

`Integration: itg-{{slug}}` `ACTIVE`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what an integration adds to it. Add
an optional field from [the type page](../integrations.md#metadata) where you have a value for it.

**Frontmatter**

* **`id`**: `itg-` plus a slug naming the system, as `itg-email-provider`. Save the file as `<slug>.md`.
* **`used-by`**: the service ids that call it. CI fails an id that names no service. An integration nothing uses is a
  candidate for retirement.
* **`criticality`**: judged by what breaks for a customer when it is unavailable.
* **`their-sla`**: what the contract says, copied word for word, not what the marketing page implies.
* **`successor`**: what took over, once the status is `retired`. `replaces` is the same edge read the other way, and
  CI checks both ends, so a reader arriving from an old citation reaches the system that took the traffic.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what this external system does for us.

## What it does

The role it plays, and which parts of the product would stop working without it.

## Contract

|              |                                         |
|--------------|-----------------------------------------|
| **Protocol** | {{REST / SFTP / webhook / GraphQL / …}} |
| **Endpoint** |                                         |
| **Auth**     | {{mechanism, never the credentials}}    |
| **Docs**     | {{link to their documentation}}         |

_(Nothing secret goes in this corpus. Name the mechanism and where the credential is stored, not the credential.)_

## Failure modes

| Failure | How it presents | Our fallback |
|---------|-----------------|--------------|
|         |                 |              |

_("It goes down sometimes" is not a failure mode. "Returns 503 during their Sunday maintenance window; we queue and
retry with backoff" is. **Give each failure a row, and fill its fallback cell. Write `None.` where nothing takes
over.** An undocumented single point of failure is the most expensive kind.)_

## Trial criteria

_(Required while the integration's status is `trial`, and deleted once it is not.)_

What is being evaluated, what would settle it either way, and what has been measured so far. A trial with no decision
criteria stays a trial forever.

## Their SLA

What they commit to, as written in the contract, and what happens when they miss it. Note where this caps one of our
own [NFRs](../nfrs.md).

## Exit

_(Required while a `critical` or `important` integration is still in use.)_

How we would move to another supplier: what would have to be rebuilt, what data would have to move, and the notice the
contract needs. **Name what makes the move hard.** The wording, the history and the accounts a vendor keeps are the
usual answer, and none of them appear in the contract.

## Commercials

|                   | |
|-------------------|-|
| **Cost model**    | |
| **Renewal**       | |
| **Account owner** | |

_(Rarely written down anywhere else, and needed at exactly the wrong moment.)_

## Contacts

Support channel, escalation path, and the account contact. Include response-time expectations if they differ from the
SLA.

## Related

* [svc-{{a}}] depends on this.
* [rbk-{{a}}] says what to do when it is down.
* [adr-{{a}}] records why we chose it.

[adr-{{a}}]: ../adrs/{{a}}.md
[rbk-{{a}}]: ../runbooks/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
