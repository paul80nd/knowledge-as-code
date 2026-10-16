---
name: ci-coverage-threshold
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-TEST]
---

We have no CI. Add a GitHub Actions workflow at `.github/workflows/tests.yml` that runs the test suite on every pull
request.
