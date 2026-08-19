---
id: exp-how-holds-are-queued
tier: descriptive
status: active
owner: alex.doe
explains: [cap-borrower-notifications]
review-by: "2026-12-31"
---

# How holds are queued

`Explanation: exp-how-holds-are-queued` `ACTIVE`

## Where the detail lives

It lives here, which is the fault this document exists to demonstrate. A hold joins the queue for a title
rather than for a copy, so any copy returned to any branch can satisfy the oldest waiting hold. The queue
is ordered by the moment the hold was placed, except that a borrower who has already had the title within
the season is moved behind anyone who has not. Staff holds do not jump the queue, and a hold placed on a
title with no circulating copies is accepted but never becomes collectable until one is acquired. None of
that is linked to anywhere: every rule above is restated here rather than cited, which is exactly how an
explanation becomes the copy that goes stale first.
