---
name: new-command-line-verb
max_turns: 30
timeout_seconds: 300
allowed_tools: [Read, Glob, Grep, Skill]
tags: [house, tol-system-commandline]
---

I want to stop scoring the plugin trial in shell. `tooling/kac-trials` is a new .NET console project beside kac, and
it needs its first verb: `kac-trials score <result.json> [--adjudication <file>] [--out <dir>]`. It reads the result
file `claude plugin eval` writes, and writes `sessions.tsv` and `aggregate.md` into the out folder. More verbs will
follow.

Write the design in `docs/design.md` first: what parses the command line, how the verb and its options are declared,
and what the verb writes. Don't write the code yet.
