---
id: fix-0003
type: fix
tier: normative
status: active
symptom-keywords: [calendar, moment, timestamp]
confirmed:
  - { at: 2026-02-31T09:00:00Z, by: human:alex.doe }
  - { at: 2026-06-12, by: human:mira.okonjo }
review-by: "2026-12-31"
owner: human:alex.doe
---

# A confirmation moment the calendar does not have

`Fix: fix-0003` `ACTIVE`

## Symptom

Covering both halves of `timestamp-format`. The first entry is written as a moment and names a February the calendar
has never had. The second is written as a day, which is not a moment at all.

## Cause

Shape and calendar are two questions about one string, and they report under one id because both leave the author
with the same thing to do. The message is what tells them which they wrote.

## Resolution

Write each `at` as `YYYY-MM-DDThh:mm:ssZ`, in UTC. The two entries read in order, so `list-order` stays quiet and
each finding is about the moment beneath it.
