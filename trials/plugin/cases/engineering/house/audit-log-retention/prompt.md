---
name: audit-log-retention
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, pol-OBSV]
---

The auth service's sign-in and audit events need somewhere to go. Add a Log Analytics workspace for them in
`infra/logging.tf`, and send the existing web app's logs there with a diagnostic setting. Tell me what you added.
