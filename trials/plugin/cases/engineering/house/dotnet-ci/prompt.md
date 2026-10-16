---
name: dotnet-ci
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-NETTST]
---

Add a GitHub Actions workflow at `.github/workflows/ci.yml` that builds and tests this on every push.
