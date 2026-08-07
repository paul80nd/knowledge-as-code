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

| Field         | Req | Type   | Notes                                                                                |
|---------------|-----|--------|--------------------------------------------------------------------------------------|
| `id` †        | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                |
| `tier` †      | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder. |
| `status` †    | ●   | enum   | Whether the integration is live, on trial, or retired.                               |
| `owner` †     | ●   | string | A named person, never a team alias.                                                  |
| `tags` †      |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                     |
| `vendor`      | ●   | string | Who supplies it.                                                                     |
| `used-by`     | ●   | list   | An integration nothing uses is a candidate for retirement.                           |
| `criticality` | ●   | enum   | Judged by what a customer experiences when it is unavailable.                        |
| `their-sla`   |     | string | What the contract actually says, not what the marketing page implies.                |

**Enum values**

| Field         | Values                                                              |
|---------------|---------------------------------------------------------------------|
| `tier`        | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status`      | `active` · `trial` · `retired`                                      |
| `criticality` | `critical` · `important` · `supporting`                             |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

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

| Check                       | Level   | What it verifies                                                                                         |
|-----------------------------|---------|----------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                      |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                           |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                        |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                  |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                           |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                                     |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                  |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                             |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                               |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                               |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                            |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                    |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                          |
| `h1`                        | error   | The document has an H1.                                                                                  |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter. |
| `required-section`          | error   | Every required section heading is present.                                                               |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                           |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                          |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                 |
| `unused-definition`         | warning | A link definition that nothing references.                                                               |

<!-- END GENERATED: checks-integrations -->
