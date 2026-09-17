---
id: int-identity
type: integration
tier: descriptive
status: active
vendor: Loxley Identity
used-by:
  - svc-catalogue-api
  - svc-catalogue-web
criticality: critical
their-sla: 99.95% monthly availability on the sign-in endpoint
owner: human:robin.hale
tags: [ sign-in, tokens ]
---

# Identity

`Integration: int-identity` `ACTIVE`

The sign-in a borrower completes before the catalogue shows them anything of their own.

## What it does

Authenticates a borrower and issues the token the estate trusts. [svc-catalogue-web] redirects to it at
`id.example.com`, and [svc-catalogue-api] validates the token it gets back against the same authority. The vendor runs
the tenant, so the hostname is the consortium's and the system behind it is not.

Browsing and search need no sign-in. A borrower opens their loans, renews an item and places a hold only once this
has issued a token, so an outage here stops all three.

## Contract

|              |                                                                                 |
|--------------|---------------------------------------------------------------------------------|
| **Protocol** | OpenID Connect, code flow with PKCE                                             |
| **Endpoint** | `https://id.example.com`, with discovery at `/.well-known/openid-configuration` |
| **Auth**     | A client id per application, and a client secret for the gateway                |
| **Docs**     | <https://docs.loxley.example.com/oidc>                                          |

The gateway's client secret sits in the platform key vault. [svc-catalogue-web] is a public client and has none.

## Failure modes

| Failure                                | How it presents                              | Our fallback                                          |
|----------------------------------------|----------------------------------------------|-------------------------------------------------------|
| The sign-in endpoint is unavailable    | Nobody can sign in, and open sessions run on | Branch staff do at the desk what nobody can do online |
| The published key set is unreachable   | The gateway rejects every token              | The gateway caches that key set for 24 hours          |
| The vendor rotates a signing key early | The same rejection, delayed by the cache     | None. The cache is flushed before sign-in recovers    |

## Their SLA

Planned maintenance runs on Sunday between 02:00 and 04:00 and is excluded from the measurement. A Sunday morning
outage in that window earns no credit.

**No target covers the administration console.** Branch staff unlock an account there, and the contract treats that as
a separate product.

## Commercials

|                   |                                         |
|-------------------|-----------------------------------------|
| **Cost model**    | Per active borrower per year, banded    |
| **Renewal**       | 1 April, with 90 days' notice to cancel |
| **Account owner** | Robin Hale                              |

## Contacts

A total outage goes to a 24-hour incident line named in the contract. Everything else goes to the portal, and a ticket
gets a reply within four working hours on a weekday.

## Related

* [svc-catalogue-web] redirects a borrower here and receives the token.
* [svc-catalogue-api] validates that token on every call behind the site.
* [ofr-borrowing] and [ofr-reservations] both start with a signed-in borrower.

[ofr-borrowing]: ../offerings/borrowing.md
[ofr-reservations]: ../offerings/reservations.md
[svc-catalogue-api]: ../services/catalogue-api.md
[svc-catalogue-web]: ../services/catalogue-web.md
