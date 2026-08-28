---
id: std-0001
tier: normative
status: active
axis: domain
implements:
  - eng:pol-DATA.MINIMAL
  - eng:pol-MEXP.PEERID
  - eng:pol-TRUS.CLOUD
applies-to:
  - svc-payment-api
review-by: "2027-08-28"
owner: paul.law
tags: [ cards, pci-dss, tokenisation ]
---

# Card details reach the PSP and never reach us

`Standard: std-0001` `ACTIVE`

## Summary

The browser sends card details to the payment service provider (PSP) and receives a token. Our services handle the
token. No card number, expiry date or security code crosses a boundary we own.

## Rules

### The card goes straight to the PSP

- A checkout page **MUST** collect card details in a frame the PSP serves (`eng:pol-DATA.MINIMAL`).
- A checkout page **MUST NOT** post a card field to a host we operate (`eng:pol-DATA.MINIMAL`).
- A service **MUST** accept a card token where it needs to charge a card.
- A service **MUST NOT** accept a card number on any endpoint (`eng:pol-DATA.MINIMAL`).
- A service **MUST** verify the PSP's certificate against the PSP's own published chain before it sends anything
  (`eng:pol-MEXP.PEERID`).

### Nothing we own stores a card

- A datastore **MUST NOT** hold a card number, an expiry date or a security code, in any column, document or blob.
- A support tool **MUST NOT** offer a field that accepts a card number, so that a customer reading one out over the
  telephone has nowhere for it to land.
- A token **MAY** be stored and reused, because it is worthless to anyone but us and the PSP.

### The split of responsibility is written down

- The PSP contract **MUST** name which PCI DSS requirements the PSP answers for and which we do
  (`eng:pol-TRUS.CLOUD`).
- A change to how the checkout collects a card **MUST** be reviewed against that split before it ships.

## Examples

```
Good
  browser      --card-->   psp.example.com           (the frame the PSP serves)
  browser      --token-->  payment-api.example.com
  payment-api  --token-->  api.psp.example.com

Avoid
  browser      --card-->   payment-api.example.com
  payment-api  --card-->   api.psp.example.com
```

The avoided form puts a card number in our request logs, our memory dumps and our PCI DSS scope, and the token it
eventually gets is no safer for the detour.

## Conformance checklist

- [ ] The checkout page posts card fields to a PSP hostname, confirmed in the browser's network trace.
- [ ] No request body reaching our estate carries a field named for a card number, expiry or security code.
- [ ] A search of every schema in the estate for a card-number column returns nothing.
- [ ] The support tool has no free-text field a card number could be typed into.
- [ ] The PSP contract's responsibility matrix is current, and someone here has read it this year.

## Rationale and provenance

A card number we never receive is one we cannot leak, cannot be asked to produce, and cannot leave in a log. It also
keeps us to the smallest PCI DSS assessment available to a merchant, because the systems in scope are the systems that
touch card data.

- `eng:pol-DATA` commits us to collecting only the personal data that is needed.
- `eng:pol-MEXP` commits us to verifying who is at the far end of a connection leaving our estate.
- `eng:pol-TRUS` commits us to establishing which security responsibilities a provider holds before we depend on them.

## Changelog

- 2026-08-28: initial version.
