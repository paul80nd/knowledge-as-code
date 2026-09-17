---
id: exp-estate-overview
type: explanation
tier: descriptive
status: active
owner: human:robin.hale
explains:
  - cap-borrowing
  - cap-discovery
  - cap-reservations
  - svc-catalogue-api
  - svc-catalogue-web
  - svc-covers-cdn
  - svc-lending
  - svc-notices
  - svc-reservations
  - svc-search
  - svc-shelf-audit
review-by: "2027-03-31"
tags: [ architecture, onboarding ]
---

# How the estate fits together

`Explanation: exp-estate-overview` `ACTIVE`

A map of the consortium's systems, for somebody meeting them for the first time. Every fact below is stated in the
record it links to.

## What a borrower can do

The consortium offers a [borrower](../glossary/example-libraries.md#borrower) three things. They find a title, they
borrow it, and they reserve one held at another [branch](../glossary/example-libraries.md#branch). [cap-discovery],
[cap-borrowing] and [cap-reservations] each list the services behind one of them. Read those before any service
record, because a service on its own does not say what it is for.

## The public surfaces

[svc-catalogue-web] and [svc-reservations] are the only services a borrower reaches directly. The branch terminals run
those same applications, so no separate service exists for them.

## The gateway and the services behind it

[svc-catalogue-api] is the route from the public site into the estate, and the services behind it stay off the public
internet. Two services face outward beside the gateway. [svc-search] serves the search box and faceted browse.
[svc-covers-cdn] serves jacket imagery from the edge, and [exp-covers-path] walks that path.

## The legacy system

[svc-lending] wraps the legacy library management system and maps its database. Loans, holds and borrower records are
read and written through it, so both [cap-borrowing] and [cap-reservations] depend on it.

## What runs without a borrower

[svc-notices] emails a borrower on a schedule and on events from the message bus. [svc-search] rebuilds its index from
that same bus. [svc-shelf-audit] checks the catalogue from outside once a night, and the platform's own synthetic
monitoring is replacing it.

## What the service graph leaves out

`depends-on` records a call, and a bus message is not a call. [svc-search] and [svc-notices] therefore appear
unconnected in the graph while being coupled to the estate as tightly as anything in it. [Services](../services.md)
states that convention, and each service lists its own topics under operational notes.

## Where the detail lives

* [svc-catalogue-web] and [svc-catalogue-api] describe the public site and its gateway.
* [svc-lending] describes the strangler wrapper and the legacy database behind it.
* [svc-search], [svc-notices] and [svc-shelf-audit] describe the work that runs on a schedule or on an event.
* [Services](../services.md) states the conventions this catalogue follows, and why some coupling has no edge.

[cap-borrowing]: ../capabilities/borrowing.md
[cap-discovery]: ../capabilities/discovery.md
[cap-reservations]: ../capabilities/reservations.md
[exp-covers-path]: covers-path.md
[svc-catalogue-api]: ../services/catalogue-api.md
[svc-catalogue-web]: ../services/catalogue-web.md
[svc-covers-cdn]: ../services/covers-cdn.md
[svc-lending]: ../services/lending.md
[svc-notices]: ../services/notices.md
[svc-reservations]: ../services/reservations.md
[svc-search]: ../services/search.md
[svc-shelf-audit]: ../services/shelf-audit.md
