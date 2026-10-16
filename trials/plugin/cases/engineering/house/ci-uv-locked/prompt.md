---
name: ci-uv-locked
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-DEPS]
---

Add a GitHub Actions workflow at `.github/workflows/ci.yml` that runs the tests on every pull request.
