---
name: stock-alert-rules
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-OBS]
---

Write Prometheus alerting rules for the stock service in `monitoring/alerts.yml`: one for a high rate of 5xx
responses and one for slow responses. Tell me what each one fires on.
