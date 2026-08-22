---
id: int-{{slug}}
tier: descriptive
status: active
vendor:
used-by:
criticality:
their-sla:
owner:
tags: [ a, b ]
---

# {{Integration name}}

`Integration: int-{{slug}}` `ACTIVE`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md)** — where a document goes, how it is written, and how it
is reviewed. What is below is only what an integration adds to that.

**Frontmatter.** Delete this block once the fields above are filled in.

* **`status`** — `active` · `trial` · `retired`.
* **`used-by`** — The service ids that call it. An integration nothing uses is a candidate for retirement.
* **`criticality`** — Judged by what breaks for a customer when it is unavailable.
* **`their-sla`** — What the contract actually says, not what the marketing page implies.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what this external system does for us.

## What it does

The role it plays, and which parts of the product would stop working without it.

## Contract

|              |                                         |
|--------------|-----------------------------------------|
| **Protocol** | {{REST / SFTP / webhook / GraphQL / …}} |
| **Endpoint** |                                         |
| **Auth**     | {{mechanism — never the credentials}}   |
| **Docs**     | {{link to their documentation}}         |

_(Nothing secret goes in this corpus. Name the mechanism and where the credential is stored, not the credential.)_

## Failure modes

| Failure | How it presents | Our fallback |
|---------|-----------------|--------------|
|         |                 |              |

_("It goes down sometimes" is not a failure mode. "Returns 503 during their Sunday maintenance window; we queue and
retry with backoff" is. **Every integration names a fallback, or explicitly states there isn't one** — an undocumented
single point of failure is the most expensive kind.)_

## Their SLA

What they commit to, as written in the contract, and what happens when they miss it. Note where this caps one of our
own [NFRs](../nfrs.md).

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

* [svc-{{a}}] — services that depend on this.
* [rbk-{{a}}] — what to do when it is down.
* [adr-{{a}}] — why we chose it.

[adr-{{a}}]: ../adrs/{{a}}.md
[rbk-{{a}}]: ../runbooks/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
