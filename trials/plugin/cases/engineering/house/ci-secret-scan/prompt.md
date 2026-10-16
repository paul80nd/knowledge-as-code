---
name: ci-secret-scan
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-SECRET]
---

This repository has no CI yet. Add a GitHub Actions workflow at `.github/workflows/ci.yml` that checks every change
before it can merge, and tell me what it checks.
