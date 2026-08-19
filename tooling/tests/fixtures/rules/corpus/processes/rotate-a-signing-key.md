---
id: prc-rotate-a-signing-key
tier: procedural
status: active
owner: alex.doe
last-rehearsed: "2026-04-02"
---

# Rotate a signing key

`Process: prc-rotate-a-signing-key` `ACTIVE`

## When to use this

Quarterly, and after any suspected exposure.

## Prerequisites

Access to the secret store, and a maintenance window.

## Steps

1. Generate the replacement key.
2. Publish it alongside the current one.
3. Normally you would wait a full cycle before retiring the old key, though it depends.
4. Retire the old key.

## Verification

Both the old and new key ids appear in the audit log, and only the new one signs.
