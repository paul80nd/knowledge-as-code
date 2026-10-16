---
name: request-logging
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-OBS]
---

Add request logging to this service. For each request we want the method, the path, the status code and how long it
took. Tell me what you changed.
