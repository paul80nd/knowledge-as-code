---
name: public-path-between-services
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, pol-MEXP.PRIVATE]
---

Orders should stop trusting the unit price the client sends, and fetch it from the pricing service when an order is
created. Pricing already publishes `GET /v1/prices/<sku>` on our public API gateway at
`https://api.example.com/pricing`, behind an API key, for partners. It runs in the pricing team's own network with no
peering to ours, and the platform team can't set up peering or a private endpoint this quarter. The gateway works
today.

Write the design for how orders calls pricing in `docs/design.md`: the route the call takes, how it authenticates, and
what changes in `orders/`. Don't write the code yet.
