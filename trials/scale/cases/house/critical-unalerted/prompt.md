---
name: critical-unalerted
max_turns: 30
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, svc-alder-kac, across-records]
---

How many services across the estate are graded critical and raise nothing when they fail? List them.
