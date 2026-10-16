---
name: label-printer-manifest
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, std-CONT]
---

Write the Kubernetes Deployment and Service for label-printer in `deploy/label-printer.yaml`. CI pushes the image to
`registry.example.com/label-printer`. Tell me how to apply it.
