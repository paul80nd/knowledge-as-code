---
name: authorisation-extra-call
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, nfr-authorisation-latency]
---

Finance wants every card authorisation screened for fraud before we send it to the PSP. The screening vendor takes
`POST https://api.fraudscreen.example.com/v1/score` with the order, the amount and the card token, and returns a score
from 0 to 100. We'd decline anything over 80 without calling the PSP. Their documentation quotes a p95 of 350ms.

Write the design in `docs/design.md`: where the call goes in `POST /authorisations`, what happens when the vendor is
slow or down, and what changes in `src/PaymentApi`. Don't write the code yet.
