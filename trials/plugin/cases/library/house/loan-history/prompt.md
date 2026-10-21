---
name: loan-history
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, dat-loans]
---

Borrowers want to see what they have read. Add `GET /borrowers/{card}/loans/history` to `src/Lending.Api`, returning
the loans that borrower has returned, newest first. Write the code, and tell me what you added.
