---
id: pmt-0002
type: postmortem
tier: decided
status: published
occurred-at: 2026-06-14T09:00:00Z
detected-at: 2026-06-14T09:05:00Z
restored-at: 2026-06-14T09:25:00Z
duration: PT25M
severity: sev3
affected: [svc-search]
owner: human:alex.doe
---

# Searches ran slow

`Postmortem: pmt-0002` `PUBLISHED`

## Summary

[svc-search] answered slowly for twenty-five minutes.

## Timeline

Searches slowed at nine, and were fast again twenty-five minutes later.

## Impact

Readers waited several seconds for each search.

## Resolution

The index was rebuilt, and searches were fast again.

## Root cause

The index had grown past the memory the service was given.

## Contributing factors

Nothing measured the size of the index.

## What went well

The alert fired five minutes after searches slowed.

## What went wrong

The rebuild took longer than anybody expected.

## Where we got lucky

The slowdown fell in a quiet hour, so few readers met it.

## Actions

Alert when the index nears the memory the service is given.

[svc-search]: ../services/search.md
