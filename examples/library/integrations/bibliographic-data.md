---
id: itg-bibliographic-data
type: integration
tier: descriptive
status: active
vendor: Pennine Bibliographic Data
used-by:
  - svc-catalogue-api
criticality: supporting
their-sla: 99.5% monthly availability on the API
owner: human:robin.hale
tags: [ cataloguing, jackets ]
---

# Bibliographic Data

`Integration: itg-bibliographic-data` `ACTIVE`

The supplier of the catalogue's bibliographic descriptions and the jacket images that go with them.

## What it does

Supplies the description of a title: author, edition, subject headings and a jacket image. That description is what
this consortium calls a record. See [gls-example-libraries.record].

Two paths bring it in.

* **The API.** [svc-catalogue-api] calls it when branch staff catalogue an item no other branch already holds.
* **The nightly feed.** `covers-import` collects the jacket images and publishes them to the `covers` container.
  [svc-thumbnailer] reads that container, and [svc-covers-cdn] serves what it returns. The pipeline is not a service
  in this catalogue, so `used-by` omits it.

The public catalogue calls neither path. An API outage stops branch staff cataloguing, and a missed feed leaves a
placeholder where a new title's jacket belongs.

## Contract

|              |                                                                            |
|--------------|----------------------------------------------------------------------------|
| **Protocol** | REST over HTTPS for the API, SFTP for the nightly feed                     |
| **Endpoint** | `https://api.pennine.example.com/v3` and `sftp://feed.pennine.example.com` |
| **Auth**     | An API key in a request header, and an SSH key pair for the feed           |
| **Docs**     | <https://docs.pennine.example.com/v3>                                      |

Both credentials sit in the platform key vault, and the catalogue reads them at start-up.

## Failure modes

| Failure                               | How it presents                          | Our fallback                                 |
|---------------------------------------|------------------------------------------|----------------------------------------------|
| The API returns 503                   | Cataloguing a new item fails at the desk | Staff type a short description by hand       |
| The nightly feed does not arrive      | A new title shows a placeholder jacket   | None. The catalogue serves the images it has |
| A feed row names a title nobody holds | `covers-import` skips it and logs it     | Re-run the import after the vendor resends   |

## Their SLA

The vendor measures availability itself and reports it monthly. A month below the target earns a service credit
against the next invoice.

**The contract sets no target for the nightly feed.** A late feed breaches nothing, so a week of missing jackets earns
no credit.

## Commercials

|                   |                                                       |
|-------------------|-------------------------------------------------------|
| **Cost model**    | Annual subscription, banded by the number of branches |
| **Renewal**       | 1 April, with 90 days' notice to cancel               |
| **Account owner** | Robin Hale                                            |

## Contacts

Support is a web portal, and a standard ticket gets a reply within two working days. Escalation is a telephone call to
the account manager, whose number is in the contract. There is no out-of-hours channel, and the SLA does not offer one.

## Related

* [svc-catalogue-api] calls the API and owns what it returns.
* [svc-thumbnailer] reads the container the nightly feed writes to, and [svc-covers-cdn] serves the result.
* [ofr-discovery] records the placeholder a missing jacket leaves.

[ofr-discovery]: ../offerings/discovery.md
[gls-example-libraries.record]: ../glossary/example-libraries.md#record
[svc-catalogue-api]: ../services/catalogue-api.md
[svc-covers-cdn]: ../services/covers-cdn.md
[svc-thumbnailer]: ../services/thumbnailer.md
