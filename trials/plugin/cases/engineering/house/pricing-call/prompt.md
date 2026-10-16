---
name: pricing-call
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-OBS]
---

Orders shouldn't trust the unit price the client sends. When an order is created, fetch the price from the pricing
service instead: `GET {PRICING_URL}/v1/prices/<sku>` returns `{"sku": ..., "unit_price": ...}`, and `PRICING_URL`
is in the environment. Tell me what you changed.
