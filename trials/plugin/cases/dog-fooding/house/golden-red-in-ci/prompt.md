---
name: golden-red-in-ci
max_turns: 40
timeout_seconds: 600
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, fix-goldens-disagree]
---

CI is red on my branch and I can't see why, because the golden suite passes on my machine. The job log says:

```
FAIL validate-broken-link: the output differs from expected.txt at line 2
```

Can you get it green?
