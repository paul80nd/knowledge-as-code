# Integrations

The external systems the platform depends on.

**[→ Index](integrations/_index.md)**

## What is an integration?

One document per third-party or external system we depend on but do not deploy: payment gateways, tax-calculation
services, email delivery, accounting systems, and the Azure platform services themselves. The record covers what the
system does for us, the contract we call it under, how it authenticates and how it fails. It also carries what the
vendor promises us, what we do when they break that promise, and who to contact.

## Why we use them

Some incidents start outside our estate. Whoever picks one up needs to know which vendor runs the system, what the
contract says, what SLA they signed and who answers the phone when we call. None of that sits in our code or our logs.

An integration also caps what we can promise. Where an [NFR](nfrs.md) targets more availability than the vendor's SLA
supports, the vendor's bad day breaks our target. Whoever sets that target can read the vendor's number in `their-sla`
first. Otherwise the gap turns up during an incident.

## Scope

**The line is whether we deploy it.** If we deploy it, it is a [service](services.md). If someone else runs it and we
call it, it is an integration.

A library or package we depend on is a [tool](tools.md), because it reaches us as code in our own build and not as a
system somebody else runs.

Not the place for:

* **How to configure it.** That is a [process](processes.md).
* **What to do when it's down.** That is a [runbook](runbooks.md), which this document links to.
* **Why we chose it.** That is an [ADR](adrs.md).

## Metadata

<!-- BEGIN GENERATED: schema-integrations -->

| Field           | Value                               | Notes                                                                               |
|-----------------|-------------------------------------|-------------------------------------------------------------------------------------|
| `id` *†         | string                              | Stable, unique across the corpus, never reused. Format set by the type.             |
| `tier` *†       | `descriptive`                       | Fixed for the type. A trust signal for the reader. CI checks it matches the folder. |
| `status` *†     | `active` `trial` `retired`          | Whether the integration is live, on trial, or retired.                              |
| `owner` *†      | string                              | A named person, never a team alias.                                                 |
| `tags` †        | list                                | Free-form, lowercase, hyphenated. Used for cross-cutting search.                    |
| `vendor` *      | string                              | Who supplies it.                                                                    |
| `used-by` *     | list                                | An integration nothing uses is a candidate for retirement.                          |
| `criticality` * | `critical` `important` `supporting` | Judged by what a customer experiences when it is unavailable.                       |
| `their-sla`     | string                              | What the contract actually says, not what the marketing page implies.               |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-integrations -->

## Adding an integration

1. Copy [`_template.md`](integrations/_template.md) to `<slug>.md`. Integrations use slug ids: `int-sendgrid`.
2. Record the contract and how the system authenticates us. Name where the credential is held. Nothing secret goes in
   this corpus.
3. Write down each way the system fails and what we do instead. "It goes down sometimes" is not a failure mode; "returns
   503 during their maintenance window, we queue and retry" is.
4. Copy `their-sla` from the contract, word for word.
5. Name who to contact when the vendor misses that SLA.

**Conventions**

* **Every integration names a fallback**, or states plainly that it has none. Where the document says neither, whoever
  is on call works it out during the incident.
* **Record the commercial facts**: cost model, renewal date, account owner. Nobody else writes them down, and you want
  them on the day the vendor raises the price or stops answering.
* **`used-by` names the services that call this system**, by id. CI fails an id that names no service.

## What CI checks

<!-- BEGIN GENERATED: checks-integrations -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `fallback-required`         | warning | The Failure modes section mentions a fallback somewhere, or says there is none.                                 |
| `no-credentials`            | error   | Nothing reads as a token, key, password or connection string.                                                   |

<!-- END GENERATED: checks-integrations -->
