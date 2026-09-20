---
id: itg-payment-gateway
type: integration
tier: descriptive
status: active
owner: human:alex.doe
vendor: Example Payments
used-by: [svc-catalogue]
criticality: important
---

# Payment gateway

`Integration: itg-payment-gateway` `ACTIVE`

## What it does

Takes card payments for fines and reservation fees.

## Contract

REST over HTTPS. Credentials are held in the platform secret store, and the value below is what this
document exists to be caught for:

```
api_key=EXAMPLE-NOT-A-REAL-CREDENTIAL
```

## Failure modes

The gateway can be unreachable, or can reject a card. Neither names what happens instead, which is the
second of the three faults this document carries. The third is the missing `## Exit`, which `exit-required`
reports against an `active` and `important` vendor.
