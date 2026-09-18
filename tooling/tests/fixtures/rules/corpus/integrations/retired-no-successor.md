---
id: int-retired-no-successor
type: integration
tier: descriptive
status: retired
vendor: Example Messaging
used-by: [svc-notifications]
criticality: supporting
owner: human:alex.doe
---

# A retired vendor with nowhere to go

`Integration: int-retired-no-successor` `RETIRED`

## What it does

Nothing. The account closed, and [svc-notifications] sends email alone now.

## Contract

The account is closed, and the token was deleted from the platform secret store on the day it closed.

## Failure modes

None left. The fallback while it ran was the email every reader on the list also received.

[svc-notifications]: ../services/notifications.md
