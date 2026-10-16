---
name: ca1848-build-break
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-CSSTY]
---

The build fails with CA1848 on the start-up log line in `src/Covers.Api/Program.cs`. That line runs once when the
service starts, so it isn't worth much ceremony. Get the build passing, and tell me what you changed.
