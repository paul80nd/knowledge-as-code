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
facets: [ public ]
tags: [ jackets ]
---

# Covers CDN

`Service: svc-covers-cdn` `LIVE`

Book-jacket imagery, resized on demand and served from the edge.

## What it does

Serves every jacket image that appears in a result list, on an item page or on a branch terminal. Each one is resized
and re-encoded to the dimensions the request asks for. The `?width=320&quality=80` form appears throughout the estate.

**It does not front blob storage directly.** Its origin is [svc-thumbnailer], which reads the underlying container and
resizes the image. The edge therefore caches the resized result. That is why it has an endpoint of its own, separate
from the estate's static one.

Content reaches the underlying container from two places:

* `covers-import` carries jacket imagery from the bibliographic data supplier, published nightly by its own pipeline.
* [svc-catalogue-api] writes jackets uploaded by branch staff at runtime, for items the supplier has no image for.

**`repo` names `infrastructure`** as the repository that defines the edge and the one a change to this service is made
in. It does not answer "where do I change what this serves". That question has the two answers above (neither of them is
`infrastructure`).

## Where it lives

* **Repository**: [`infrastructure`](https://git.example.com/example-libraries/infrastructure), at `services/covers`
* **Platform**: CDN custom domain over an origin application
* **Deployed as**: route `covers` on a dedicated endpoint, origin group `thumbnailer`, origin path `/covers`

DNS is managed outside the infrastructure repository, as it is for the estate's other edge surfaces.

## Environments

| Environment | URL                             | Notes |
|-------------|---------------------------------|-------|
| Development | https://covers-dev.example.com  |       |
| Test        | https://covers-test.example.com |       |
| Production  | https://covers.example.com      |       |

**Quick check**: a jacket image resized through the origin, in
[dev](https://covers-dev.example.com/9780000000001.jpg?width=320&quality=80) ·
[test](https://covers-test.example.com/9780000000001.jpg?width=320&quality=80) ·
[prod](https://covers.example.com/9780000000001.jpg?width=320&quality=80).

The `?width=` and `?quality=` parameters are the point of the check. They exercise [svc-thumbnailer] behind the edge,
not the cache alone. The container root does not list, so a known object is the only way to confirm the surface is
serving.

## Dependencies

* [svc-thumbnailer] is the origin the edge forwards to. The CDN origin host configures it, so this dependency lives in
  the routing and in neither service's application settings.

## Data

Backed by the `covers` container in the shared storage account. The thumbnailer reads it on the edge's behalf.

## Operational notes

* **Caching.** The edge keys its cache on the resize parameters alone, and ignores the other query strings for that
  purpose. Compression is enabled there.
* **Consumers.** [svc-catalogue-web] is the only service configured with this URL. The branch terminals load from it
  directly, and they are not services, so they appear nowhere in the graph.

[svc-catalogue-api]: catalogue-api.md
[svc-catalogue-web]: catalogue-web.md
[svc-thumbnailer]: thumbnailer.md
