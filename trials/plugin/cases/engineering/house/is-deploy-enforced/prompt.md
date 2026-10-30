---
name: is-deploy-enforced
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, ctl-health-reports-commit]
---

Does anything actually check that a running build reports the commit it was built from? We want to rely on that when
we trace a bad release, so I need to know whether it is checked or just written down.
