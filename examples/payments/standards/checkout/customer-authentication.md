---
id: std-SCA
tier: normative
status: active
implements:
  - eng:pol-INTC.SECURE
  - eng:pol-SECD.DESIGN
  - eng:pol-SECD.REQS
applies-to:
  - svc-payment-api
review-by: "2027-08-31"
owner: paul.law
tags: [ 3ds, checkout, strong-customer-authentication ]
---

# The PSP decides whether the cardholder is challenged

`Standard: std-SCA` `ACTIVE`

## Summary

Every authorisation goes through the payment service provider's (PSP) 3-D Secure flow. The PSP decides whether the
cardholder is challenged, and our services carry the outcome rather than deciding it.

## Rules

### The PSP runs the authentication

- The checkout **MUST** start authentication through the PSP's 3-D Secure flow before it asks for an authorisation
  (`eng:pol-SECD.DESIGN`).
- A service **MUST** treat an authorisation without an authentication outcome as declined, so a failure denies rather
  than allows (`eng:pol-SECD.DESIGN`).
- A service **MUST NOT** decide by itself that a payment is exempt from Strong Customer Authentication, which is the
  cardholder proving who they are with two independent factors (`eng:pol-SECD.REQS`).
- A service **MUST** record the exemption the PSP applied, where the PSP applied one (`eng:pol-SECD.REQS`).

### The outcome travels with the payment

- An authorisation request **MUST** carry the PSP's authentication reference (`eng:pol-INTC.SECURE`).
- The ledger entry for an authorisation **MUST** record the authentication outcome and who bears the liability for a
  chargeback (`eng:pol-SECD.REQS`).
- A service **MUST NOT** retry a declined authorisation with the authentication step left out (`eng:pol-SECD.DESIGN`).

### The challenge is somebody else's page

- The checkout **MUST** hand the cardholder to the PSP's challenge page or frame (`eng:pol-SECD.DESIGN`).
- The checkout **MUST NOT** collect a one-time passcode, a password or a biometric prompt on a page we serve
  (`eng:pol-SECD.DESIGN`).
- The checkout **MUST** hold the order for the length of the challenge, and release it on a stated timeout rather than
  waiting forever (`eng:pol-SECD.REQS`).

## Examples

```
Good
  checkout  --> psp 3ds        the PSP returns frictionless, or challenges
  checkout  --> payment-api    { order, token, psp_auth_ref }
  payment-api --> psp          authorise, quoting psp_auth_ref

Avoid
  checkout  --> payment-api    { order, token }
  payment-api --> psp          authorise, no authentication reference
```

The avoided flow asks the PSP to move money without telling it who checked the cardholder. The PSP either declines it
or accepts it with the liability on us.

## Conformance checklist

- [ ] Every authorisation request our services send carries a PSP authentication reference.
- [ ] An authorisation with no authentication outcome is declined, confirmed by a test.
- [ ] The challenge page in the browser's network trace is served by the PSP.
- [ ] No page we serve has a field for a one-time passcode.
- [ ] Every ledger entry for an authorisation records the outcome and the liability holder.
- [ ] The checkout releases a held order when the challenge passes its timeout.

## Rationale and provenance

Authentication decides who pays when a payment turns out to be fraudulent. The PSP holds the card scheme's rules and
the issuer's response, so a decision we make here would be a second, worse copy of theirs.

- `eng:pol-INTC` commits us to interfaces that authenticate and authorise before they act.
- `eng:pol-SECD` commits us to secure defaults, to failing closed, and to capturing security requirements up front.

## Sources and further reading

- **Normative.** [EMV 3-D Secure] defines the authentication flow and the fields this standard passes through. Our
  services fill none of them directly.
- **Informative.** [The PSD2 regulatory technical standards on SCA] set the exemptions the PSP applies on our behalf.

## Changelog

- 2026-08-31: initial version.

[EMV 3-D Secure]: https://www.emvco.com/emv-technologies/3-d-secure/
[The PSD2 regulatory technical standards on SCA]: https://www.handbook.fca.org.uk/techstandards/PS/2018/2018_389.pdf
