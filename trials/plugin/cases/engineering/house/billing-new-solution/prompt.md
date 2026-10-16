---
name: billing-new-solution
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-CSSTY]
---

Start the billing code: create a solution with a class library `Billing.Core` under `src/`, holding an `Invoice`
type with a number, an issue date and a list of lines (description, quantity, unit price), and a `Total`. Tell me
what you created.
