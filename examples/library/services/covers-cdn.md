---
id: svc-covers-cdn
type: service
tier: descriptive
status: live
repo: infrastructure
platform: static
criticality: critical
depends-on:
  - svc-thumbnailer
owner: human:dev.raman
facets: [ public ]
tags: [ jackets ]
---

# Covers CDN

`Service: svc-covers-cdn` `LIVE`

Book-jacket imagery, resized on demand and served from the edge.

## What it does

Serves every jacket image that appears in a result list, on an item page or on a branch terminal. Each one is resized
and re-encoded to the dimensions the request asks for. The `?width=320&quality=80` form appears throughout the estate.

**[svc-thumbnailer] is the origin, and blob storage is behind that service.** The thumbnailer reads the underlying
container and resizes the image, so the edge caches the resized result. This service therefore has an endpoint of its
own, separate from the estate's static one.

Content arrives in the underlying container from two places:

* `covers-import` brings jacket imagery from the bibliographic data supplier. Its own pipeline publishes it nightly.
* [svc-catalogue-api] writes jackets uploaded by branch staff at runtime, for items the supplier has no image for.

**`repo` is `infrastructure`.** That repository defines the edge, and a change to this service is made there. What this
service serves is changed in `covers-import` and [svc-catalogue-api], and neither of those is `infrastructure`.

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

The `?width=` and `?quality=` parameters make the check exercise [svc-thumbnailer] behind the edge, and not the cache
alone. The container root does not list its contents, so a known object is the only way to confirm the surface is
serving.

## Dependencies

* [svc-thumbnailer] is the origin the edge forwards to. The CDN origin host sets it, so this dependency is recorded in
  the routing and in neither service's application settings.

## Data

Backed by the `covers` container in the shared storage account. The thumbnailer reads it on the edge's behalf.

## Operational notes

* **Caching.** The edge keys its cache on the resize parameters alone, and no other query string affects the key.
  Compression is enabled at the edge.
* **Consumers.** [svc-catalogue-web] is the only service configured with this URL. The branch terminals load from it
  directly. They are not services, so they appear nowhere in the graph.

[svc-catalogue-api]: catalogue-api.md
[svc-catalogue-web]: catalogue-web.md
[svc-thumbnailer]: thumbnailer.md
