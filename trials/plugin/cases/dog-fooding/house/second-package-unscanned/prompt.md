---
name: second-package-unscanned
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, dev-package-unscanned]
---

kac-lint is ready for its first release. Add a GitHub Actions workflow at `.github/workflows/publish.yml` that packs
it and publishes it to nuget.org when I push a version tag, and tell me what it does.
