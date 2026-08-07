---
id: dsc-pool-exhaustion-under-load
tier: observed
status: open
owner: alex.doe
source: human
confidence: unverified
expires: "2026-12-31"
---

# Pool exhaustion under load

`Discovery: dsc-pool-exhaustion-under-load` `OPEN`

## What I saw

Under sustained load the connection pool emptied and requests began to queue behind it, and the queue did
not drain until the load stopped. It was reproducible three times running on the same afternoon, and each
time the recovery took several minutes longer than the incident that caused it, which suggests something
holds connections after the load has gone rather than releasing them as it eases.

## Context

It was seen while running a load profile that nobody has run before, against an environment that is not
quite production and against data that is a copy several weeks old. None of those differences obviously
explains what happened, but any of them might, and until somebody has looked properly this is worth no
more than the paragraph above.

## Why it might matter

If it is real, it is the shape of an outage rather than a slow afternoon: the system does not degrade and
recover, it degrades and stays there. That is worth knowing before somebody meets it at a time of their
choosing rather than of ours. If it is not real, this expires and nobody has lost anything but the
minutes it took to write, which is the whole point of capturing at this tier rather than writing it up
properly somewhere that would demand a reviewer and a decision.
