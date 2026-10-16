---
name: cover-timeout-pr
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-PR]
---

`CoverClient.fetch` has no timeout, so a slow upstream hangs the product page. Bound it at 2 seconds and treat a
timeout as no cover. Then write the pull request description for the change into `PR_BODY.md`; I'll open the PR
myself.
