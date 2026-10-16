---
name: order-repository-tests
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-NETTST]
---

Add integration tests for `OrderRepository` against Postgres: saving an order and reading it back, and reading an
id that doesn't exist. Tell me how to run them.
