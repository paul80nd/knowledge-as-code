---
id: dev-export-has-no-notice
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-INTC.DEPREC
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on:
applies-to:
  - svc-kac
owner: human:paul.law
tags: [ export, interfaces, notice ]
---

# A consumer of the export gets no notice period

`Deviation: dev-export-has-no-notice` `ACTIVE`

The export a consumer reads is a published interface, and no standard says how much warning a change to it gives.

## What we are doing instead

[std-VERS] states the stamps: `formatVersion` for the export as a whole, and a `shapeVersion` for each type. A reader
meeting a number it does not know leaves that type alone and reads the rest. That is graceful degradation. It is not
notice.

Nothing says how long a shape stays readable after a new one ships, and nothing announces a change before it lands.

## Why we need it

The only consumers of an export today are the corpora in this repository and the plugin `kac bundle` builds from one.
Both are rebuilt on the same commit, so a change arrives for them and for their producer at once. A notice period
would be a promise to nobody.

## What compensates

* [std-VERS] gives each shape a stamp, so a reader meeting an unknown stamp keeps working on the rest of the export.
* `kac bundle` refuses an export whose `formatVersion` is not the one that build reads.
* The changelog records every change a user can observe, and [ctl-0006] fails a version with no section.
* Every consumer is in this repository, so a shape change is a change to files a pull request already touches.

## How it closes

Somebody outside this repository reads an export. The standard then states how long a shape keeps answering after its
successor ships, and how a change is announced. This record closes on that standard.

## Scope

The export `kac export` writes, the package `kac pack` zips, and the plugin `kac bundle` assembles.

## Related

* [std-VERS] defines the version stamps a consumer reads in place of a notice period.

[ctl-0006]: ../controls/0006-changelog-tests.md
[std-VERS]: ../standards/versioning.md
