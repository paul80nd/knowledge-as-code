# Integrations

The external systems the platform depends on.

**[→ Index](integrations/INDEX.md)**

## What is an integration?

One document per third-party or external system we depend on but do not deploy: payment gateways, tax-calculation
services, email delivery, accounting systems, the Azure platform services themselves. What it does, the contract, how it
authenticates, how it fails, what they promise us, what we do when it doesn't, and who to contact.

## Why we use them

A meaningful share of incidents originate outside our estate, and the knowledge needed to diagnose them — which vendor,
which contract, whose SLA, whose phone number — is exactly the knowledge nobody has written down.

Integrations also **cap** what we can promise. An [NFR](/nfrs) built on a dependency with a weaker SLA than the target
is not a commitment, it is a hope. Recording the vendor's SLA makes that visible before it is tested.

## Scope

**The line is whether we deploy it.** If we deploy it, it is a [service](/services). If we consume it and someone else
runs it, it is an integration.

A library or package we depend on is neither — that is a [tool](/tools). The distinction is a running system we call
versus code we ship.

Not the place for:

* **How to configure it** — that is a [process](/processes).
* **What to do when it's down** — that is a [runbook](/runbooks), which this document links to.
* **Why we chose it** — that is an [ADR](/adrs).

## Metadata

<!-- BEGIN GENERATED: schema-integrations -->

| Field         | Req | Type   | Notes                                   |
|---------------|-----|--------|-----------------------------------------|
| `status`      | ●   | enum   | `active` · `trial` · `retired`          |
| `vendor`      | ●   | string |                                         |
| `used-by`     | ●   | list   | Service ids                             |
| `criticality` | ●   | enum   | `critical` · `important` · `supporting` |
| `their-sla`   |     | string |                                         |

<!-- END GENERATED: schema-integrations -->

## Adding an integration

1. Copy [`template.md`](integrations/template.md) to `<slug>.md`. Integrations use slug ids — `int-sendgrid`.
2. Record the contract and the auth mechanism, but not the credentials. Nothing secret goes in this wiki.
3. Document the **failure modes** and our fallback for each. "It goes down sometimes" is not a failure mode; "returns
   503 during their maintenance window, we queue and retry" is.
4. Record `their-sla` as written in the contract, and who to contact when it is breached.
5. Set `criticality` by what breaks for a customer when it is unavailable.

**Conventions**

* **Every integration names a fallback**, or explicitly states there isn't one. An undocumented single point of failure
  is the most expensive kind.
* **Record the commercial facts** — cost model, renewal date, account owner. They are rarely written anywhere else and
  are needed at exactly the wrong moment.
* **`used-by` resolves to services.** An integration nothing uses is a candidate for retirement.

## What CI checks

<!-- BEGIN GENERATED: checks-integrations -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-integrations -->
