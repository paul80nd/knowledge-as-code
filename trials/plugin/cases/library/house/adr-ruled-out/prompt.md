---
name: adr-ruled-out
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, adr-0002]
---

Branch staff say borrowers keep losing half-finished reservations. Somebody starts one on a branch terminal, gets
called to the desk, and has to begin again on their phone at home. I'd like a "save and finish later" for the reserve
flow, so a borrower can leave part-way and pick the reservation up in a later session, on any device.

Write the design in `docs/design.md` before anyone builds it: where a half-finished reservation is kept, how the
borrower gets back to it, and what changes in `src/Services/Reservations`. Don't write the code yet.
