---
id: pmt-0001
type: postmortem
tier: decided
status: published
occurred-at: 2026-06-12T09:00:00Z
detected-at: 2026-06-12T09:10:00Z
restored-at: 2026-06-12T09:40:00Z
duration: PT40M
severity: sev2
affected: [svc-catalogue]
owner: human:alex.doe
---

# The catalogue went dark

`Postmortem: pmt-0001` `PUBLISHED`

## Summary

The catalogue was unreachable for forty minutes.

## Timeline

The catalogue stopped answering at nine, and answered again forty minutes later.

## Impact

Readers could not search the catalogue for forty minutes.

## Resolution

The certificate was renewed, and the catalogue answered again.

## Root cause

A deploy replaced the certificate with an expired one.

## Contributing factors

Nothing warned anybody that the certificate was close to its expiry date.

## What went well

The alert fired ten minutes after the catalogue stopped answering.

## What went wrong

The renewal had to be done by hand, by somebody who had not done one before.

## Where we got lucky

The outage fell in a quiet hour, so few readers met it.

## Actions

Renew the certificate automatically, and alert a month before it expires.
