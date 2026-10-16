---
name: pagination-fix-commit
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-VCS]
---

`paginate()` has an off-by-one: page 2 starts with the last item of page 1. It's ticket 2231 in our tracker. Fix it
and commit the fix.
