---
id: exp-covers-path
type: explanation
tier: descriptive
status: active
owner: human:dev.raman
explains:
  - ofr-discovery
  - svc-catalogue-api
  - svc-covers-cdn
  - svc-thumbnailer
review-by: "2027-03-31"
tags: [ jackets ]
---

# How a jacket image reaches a page

`Explanation: exp-covers-path` `ACTIVE`

The path one jacket image takes, from the container it is stored in to the result list a borrower sees. Each leg of it
belongs to a different record.

## The request path

A catalogue page asks [svc-covers-cdn] for an image at a size. The edge serves it from cache, or forwards to
[svc-thumbnailer]. That service reads the original from blob storage, resizes it, and returns it. The edge caches the
resized result, so the requested size is part of the cache key.

Nothing calls [svc-thumbnailer] directly. [svc-covers-cdn] is the public hostname, and the origin sits behind the
platform network.

## How images get into storage

Two writers fill the container, and [svc-thumbnailer] is neither of them. The `covers-import` pipeline brings imagery
from the bibliographic data supplier each night. [svc-catalogue-api] writes the jackets branch staff upload for items
the supplier has no image for. [svc-covers-cdn] describes both.

## The repositories behind the path

No single service record can describe this path, because each leg is changed in a different repository. The edge is
defined in `infrastructure`, the origin in `thumbnailer`, and the staff uploads in `platform`. [Services](../services.md)
says why `repo` takes one value, and why the body has to state the rest.

## What a borrower sees when a leg fails

[ofr-discovery] states the ordinary case: a title the supplier sent no image for renders a placeholder. A failure of
[svc-thumbnailer] is worse, because [svc-covers-cdn] then has no origin and every uncached image breaks across the
estate.

## Where the detail lives

* [svc-covers-cdn] describes the edge, its routes and its cache key.
* [svc-thumbnailer] describes the origin and the container it reads.
* [svc-catalogue-api] describes the gateway that writes staff uploads.
* [ofr-discovery] lists what a borrower uses to find a title, and what the placeholder costs them.
* [exp-estate-overview] puts this path beside the rest of the estate.

[ofr-discovery]: ../offerings/discovery.md
[exp-estate-overview]: estate-overview.md
[svc-catalogue-api]: ../services/catalogue-api.md
[svc-covers-cdn]: ../services/covers-cdn.md
[svc-thumbnailer]: ../services/thumbnailer.md
