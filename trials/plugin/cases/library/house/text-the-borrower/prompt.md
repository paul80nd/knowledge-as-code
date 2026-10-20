---
name: text-the-borrower
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, itg-sms-notices]
---

Borrowers keep missing the hold-ready email, and their holds go back to the shelf uncollected. Can we text them as
well when a hold is ready to collect?

Write the design in `docs/design.md`: who sends the text, where the borrower's mobile number comes from, and what
changes in `src/Notices.FunctionApp`. Don't write the code yet.
