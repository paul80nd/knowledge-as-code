---
name: orders-column-rename
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-DEPLOY]
---

We're standardising on `full_name` everywhere. Rename the orders table's `customer_name` column to `full_name`, and
update the code to match. Migrations go in `migrations/`, numbered. Tell me what you changed.
