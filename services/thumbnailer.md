---
id: svc-thumbnailer
tier: descriptive
status: live
repo: thumbnailer
platform: dotnet-web
criticality: critical
depends-on:
data-stores:
owner: dev.raman
tags: [ internal ]
---

# Thumbnailer

`Service: svc-thumbnailer` `LIVE`

Resizes and re-encodes jacket images on demand from blob storage — the origin behind the covers CDN.

## What it does

Reads a jacket image from blob storage and returns it at the dimensions and quality the request asks for. It holds no
state and caches nothing itself; the caching is the edge's job.

It is **not reached directly**. [svc-covers-cdn] forwards to it as its origin, so every request arrives through the
edge and the edge caches the resized result rather than the original. That is why the covers surface has an endpoint
of its own while the estate's other static content shares one.

## Where it lives

* **Repository** — [`thumbnailer`](https://git.example.com/example-libraries/thumbnailer)
* **Platform** — ASP.NET Core (.NET 10)
* **Deployed as** — App Service `app-thumbnailer-<env>`

## Environments

| Environment | URL                                     | Notes                      |
|-------------|-----------------------------------------|----------------------------|
| Development | https://app-thumbnailer-dev.example.net  | Reached via the covers CDN |
| Test        | https://app-thumbnailer-test.example.net | Reached via the covers CDN |
| Production  | https://app-thumbnailer-prd.example.net  | Reached via the covers CDN |

No custom domain — the public face of this service is the covers CDN hostname, not these. In normal use nothing should
call these URLs directly.

## Dependencies

None. It reads blob storage and returns an image.

## Data

Reads the `covers` container in the shared storage account, configured as `ConnectionStrings__CoverStorage`. It reads
rather than owns: the container is filled by the nightly import pipeline and by branch staff uploads, both recorded
against [svc-covers-cdn].

## Operational notes

* **TLS terminates at the edge**, so this app service accepts plain HTTP where every other application service in the
  estate is configured to refuse it. The origin is not reachable from outside the platform network, which is
  presumably the reasoning, but it is a deliberate difference from its neighbours and worth confirming rather than
  inheriting.
* **Criticality** — `critical`. Every jacket image on every catalogue page is served through this service; if it
  fails, the covers CDN has no origin to forward to and imagery breaks estate-wide, cached content aside.

[svc-covers-cdn]: covers-cdn.md
