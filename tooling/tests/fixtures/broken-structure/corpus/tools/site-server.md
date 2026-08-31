---
id: tol-Site_Server
tier: descriptive
status: approved
owner: alex.doe
---

# Slug id in the wrong alphabet

`Tool: tol-Site_Server` `APPROVED`

## What we use it for

Covering `id-format` on a slug id. The prefix is right, so the prefix check passes and the shape check is the only
thing left to catch the capitals and the underscore.

## Status

Intentionally broken. The identity line repeats the id as written, so this document trips the shape check and nothing
else.
