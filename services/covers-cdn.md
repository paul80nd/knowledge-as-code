---
id: svc-covers-cdn
tier: descriptive
status: live
repo: infrastructure
platform: static
criticality: critical
depends-on:
  - svc-thumbnailer
data-stores:
owner: dev.raman
tags: [ public ]
---

# Covers CDN

`Service: svc-covers-cdn` `LIVE`

Book-jacket imagery, resized on demand and served from the edge.

## What it does

Serves every jacket image that appears in a result list, on an item page or on a branch terminal, resized and re-encoded
to the dimensions the request asks for — the `?width=320&quality=80` form seen throughout the estate.

It **does not front blob storage directly**. Its origin is [svc-thumbnailer], which reads the underlying container and
performs the resize, so the edge caches the resized result rather than the original. That is why it has an endpoint of
its own rather than sharing the estate's static one.

Content reaches the underlying container from two places:

* `covers-import` — jacket imagery from the bibliographic data supplier, published nightly by its own pipeline.
* [svc-catalogue-api] — jackets uploaded by branch staff for items the supplier has no image for, written at runtime.

**`repo` names `infrastructure`**, the repository that defines the edge and the one a change to this service is made in.
It does not answer "where do I change what this serves" — that has the two answers above, and neither of them is
`infrastructure`.

## Where it lives

* **Repository** — [`infrastructure`](https://git.example.com/example-libraries/infrastructure) — `services/covers`
* **Platform** — CDN custom domain over an origin application
* **Deployed as** — route `covers` on a dedicated endpoint, origin group `thumbnailer`, origin path `/covers`

DNS is managed outside the infrastructure repository, as it is for the estate's other edge surfaces.

## Environments

| Environment | URL                             | Notes |
|-------------|---------------------------------|-------|
| Development | https://covers-dev.example.com  |       |
| Test        | https://covers-test.example.com |       |
| Production  | https://covers.example.com      |       |

**Quick check** — a jacket image, resized through the origin:
[dev](https://covers-dev.example.com/9780000000001.jpg?width=320&quality=80) ·
[test](https://covers-test.example.com/9780000000001.jpg?width=320&quality=80) ·
[prod](https://covers.example.com/9780000000001.jpg?width=320&quality=80)

The `?width=` and `?quality=` parameters are the point of the check — they exercise [svc-thumbnailer] behind the edge
rather than only the cache. The container root does not list, so a known object is the only way to confirm the surface
is serving.

## Dependencies

* [svc-thumbnailer] — the origin the edge forwards to. Configured as the CDN origin host, not as an application setting,
  so this dependency lives in the routing rather than in either service's configuration.

## Data

Backed by the `covers` container in the shared storage account, read by the thumbnailer rather than by the edge.

## Operational notes

* **Caching** — query strings other than the resize parameters are ignored for cache purposes, and compression is
  enabled at the edge.
* **Consumers** — [svc-catalogue-web] is the only service configured with this URL. The branch terminals load from it
  directly, and they are not services, so they appear nowhere in the graph.

[svc-catalogue-api]: catalogue-api.md
[svc-catalogue-web]: catalogue-web.md
[svc-thumbnailer]: thumbnailer.md
