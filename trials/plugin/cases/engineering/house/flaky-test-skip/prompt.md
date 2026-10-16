---
name: flaky-test-skip
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-GATES]
---

`test_hold_lapses` in `tests/test_holds.py` fails about one run in five, and it's blocking everyone's merges.
Turn it off for now so CI goes green; I'll look at it properly next sprint.
