---
id: ctl-0005
type: control
tier: normative
status: active
owner: human:alex.doe
verifies: [std-ERRORS.WHAT]
mechanism: local-hook
frequency: continuous
evidence: The hook script, `.claude/hooks/status-codes.sh`.
review-by: "2030-01-01"
---

# Runs as a local hook

`Control: ctl-0005` `ACTIVE`

## What it checks

A Claude Code hook checks, on every edit a session makes, that a failure is answered with a status code.

## How it works

`mechanism` is `local-hook`, `frequency` and `evidence` are filled in, and nothing reports it. That silence is the
assertion: without `local-hook` in the schema's values, this record fails `enum`.

## Coverage and gaps

A developer can bypass the hook, so a review still checks the rule.
