---
id: std-CARD
type: standard
tier: normative
status: active
implements: [ eng:pol-DATA.MINIMAL, eng:pol-MEXP.PEERID, eng:pol-TRUS.CLOUD ]
applies-to:
  - svc-payment-api
review-by: "2027-08-28"
owner: human:paul.law
tags: [ cards, pci-dss, tokenisation ]
---

# Card details reach the PSP and never reach us

`Standard: std-CARD` `ACTIVE`

## Summary

The browser sends card details to the payment service provider (PSP) and receives a token. Our services work with the
token. No card number, expiry date or security code crosses a boundary we own.

## Rules

### The card goes straight to the PSP

- A checkout page **MUST** collect card details in a frame the PSP serves.
- A checkout page **MUST NOT** post a card field to a host we operate.
- A service **MUST** accept a card token where it needs to charge a card.
- A service **MUST NOT** accept a card number on any endpoint.
- A service **MUST** verify the PSP's certificate against the PSP's own published chain before it sends anything.

_**Covers:** `eng:pol-DATA.MINIMAL`, `eng:pol-MEXP.PEERID`_

### Nothing we own stores a card

- A datastore **MUST NOT** keep a card number, an expiry date or a security code, in any column, document or blob.
- A support tool **MUST NOT** offer a field that accepts a card number, even where a customer reads one out over the
  telephone.
- A token **MAY** be stored and reused, because only we and the PSP can use it.

### The split of responsibility is written down

- The PSP contract **MUST** state which PCI DSS requirements the PSP answers for and which we do.
- A change to how the checkout collects a card **MUST** be reviewed against that split before it ships.

_**Covers:** `eng:pol-TRUS.CLOUD`_

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

The avoided form puts a card number in our request logs, our memory dumps and our PCI DSS scope. The token it receives
at the end of the detour is no safer.

## Conformance checklist

- [ ] The checkout page posts card fields to a PSP hostname, confirmed in the browser's network trace.
- [ ] No request body sent to our estate has a field named for a card number, expiry or security code.
- [ ] A search of every schema in the estate for a card-number column returns nothing.
- [ ] The support tool has no free-text field a card number could be typed into.
- [ ] The PSP contract's responsibility matrix is current, and someone here has read it this year.

## Rationale and provenance

A card number we never receive cannot leak, cannot be demanded from us, and cannot be left in a log. It also keeps us
to the smallest PCI DSS assessment a merchant can take, because the systems in scope are the systems that handle card
data.

## Changelog

- 2026-08-28: initial version.
