---
name: returns-api-routes
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-API]
---

Add the HTTP API for returns to `returns/app.py`. The shop's front end creates a return for one line of an order
(the order id, the line number and a reason), and fetches a return by its id. Tell me what you changed.
