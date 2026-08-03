---
id: int-<slug>
tier: descriptive
status: active
vendor:
used-by:
criticality:
their-sla:
owner:
tags:
  - a
  - b
---

# <Integration name>

_(Frontmatter notes — delete this block. **`status`**: `active` · `trial` · `retired`. **`used-by`** lists the service
ids that call it — an integration nothing uses is a candidate for retirement. **`criticality`** is judged by what breaks
for a customer when it is unavailable. **`their-sla`** is what the contract actually says, not what the marketing page
implies.)_

One sentence: what this external system does for us.

## What it does

The role it plays, and which parts of the product would stop working without it.

## Contract

|              |                                     |
|--------------|-------------------------------------|
| **Protocol** | REST / SFTP / webhook / GraphQL / … |
| **Endpoint** |                                     |
| **Auth**     | <mechanism — never the credentials> |
| **Docs**     | <link to their documentation>       |

_(Nothing secret goes in this wiki. Name the mechanism and where the credential is stored, not the credential.)_

## Failure modes

| Failure | How it presents | Our fallback |
|---------|-----------------|--------------|
|         |                 |              |

_("It goes down sometimes" is not a failure mode. "Returns 503 during their Sunday maintenance window; we queue and
retry with backoff" is. **Every integration names a fallback, or explicitly states there isn't one** — an undocumented
single point of failure is the most expensive kind.)_

## Their SLA

What they commit to, as written in the contract, and what happens when they miss it. Note where this caps one of our
own [NFRs](/nfrs).

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

* [svc-example](/services/example.md) — services that depend on this.
* [rbk-example](/runbooks/example.md) — what to do when it is down.
* [ADR-NNNN] — why we chose it.

[ADR-NNNN]: /adrs/NNNN-kebab-slug.md
