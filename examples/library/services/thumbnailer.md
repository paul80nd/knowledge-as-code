---
id: svc-thumbnailer
type: service
tier: descriptive
status: live
repo: thumbnailer
platform: dotnet-web
criticality: critical
owner: human:dev.raman
tags: [ internal ]
---

# Thumbnailer

`Service: svc-thumbnailer` `LIVE`

Resizes and re-encodes jacket images on demand from blob storage. It is the origin behind the covers CDN.

## What it does

Reads a jacket image from blob storage and returns it at the dimensions and quality the request asks for. It stores no
state and caches nothing. The edge does the caching.

**Every request arrives through the edge.** [svc-covers-cdn] forwards to this service as its origin, and the edge
caches the resized result. Nothing calls this service directly.

## Where it lives

* **Repository**: [`thumbnailer`](https://git.example.com/example-libraries/thumbnailer)
* **Platform**: ASP.NET Core (.NET 10)
* **Deployed as**: App Service `app-thumbnailer-<env>`

## Environments

| Environment | URL                                      | Notes                      |
|-------------|------------------------------------------|----------------------------|
| Development | https://app-thumbnailer-dev.example.net  | Reached via the covers CDN |
| Test        | https://app-thumbnailer-test.example.net | Reached via the covers CDN |
| Production  | https://app-thumbnailer-prd.example.net  | Reached via the covers CDN |

No custom domain. The covers CDN hostname is the public one. In normal use nothing calls these URLs directly.

## Dependencies

None. It reads blob storage and returns an image.

## Data

Reads the `covers` container in the shared storage account, configured as `ConnectionStrings__CoverStorage`. It owns
nothing in that container: the nightly import pipeline and branch staff uploads fill it, and [svc-covers-cdn] records
both.

## Operational notes

* **TLS terminates at the edge.** This app service accepts plain HTTP, where every other application service in
  the estate is configured to refuse it. The origin is unreachable from outside the platform network, which may be the
  reasoning. Nothing records that, so it stays an open question.
* **Criticality**: `critical`. Every jacket image on every catalogue page is served through this service. If it fails,
  the covers CDN has no origin to forward to, and every uncached image breaks across the estate.

[svc-covers-cdn]: covers-cdn.md
