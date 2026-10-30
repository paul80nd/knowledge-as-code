---
name: two-pending-amounts
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, fix-duplicate-authorisation]
---

A customer rang support: their card shows two pending amounts for one order. Support says the first payment attempt
timed out and the checkout retried it. The call to the payment API is in `src/Checkout`. Please fix it, and tell me
what you changed.
