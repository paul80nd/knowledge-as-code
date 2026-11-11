---
name: refund-idempotency
max_turns: 30
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-IDEM, in]
---

I am adding a refund endpoint to a payment service. What header must a refund request include, and how should the caller build its value?
