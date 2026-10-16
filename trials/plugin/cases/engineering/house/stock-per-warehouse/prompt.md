---
name: stock-per-warehouse
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-API]
---

Checkout wants to see stock per warehouse, not one total. Change `GET /v1/stock/<sku>` so that `on_hand` is an
object of warehouse to quantity instead of a single number. Tell me what you changed.
