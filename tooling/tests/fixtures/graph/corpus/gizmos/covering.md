---
id: giz-covering
tier: descriptive
status: live
owner: alex.doe
depends-on:
covers:
  - giz-behaving.it-hums
  - giz-behaving.it-rattles
---

# Covering gizmo

`Gizmo: giz-covering` `LIVE`

## What it does

Answers two of another gizmo's behaviours, soundly. Each section closes on the footnote naming what that section
covers, and the union of those lines is exactly what `covers` lists. The first line also cites an ADR, which `covers`
could never carry: only the types the field points at are reconciled against it.

### The humming part

Sets the pitch, and holds it.

_**Covers:** [giz-behaving].it-hums, as [adr-0001](../adrs/0001-first.md) decided_

### The rattling part

Sheds the load, which is what stops the rattle.

_**Covers:** [giz-behaving].it-rattles_

[giz-behaving]: behaving.md
