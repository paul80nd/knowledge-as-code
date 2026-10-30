---
id: ctl-runs-as-a-local-hook
type: control
tier: normative
status: active
owner: human:alex.doe
verifies: [std-ERRORS.WHAT]
mechanism: local-hook
frequency: per-commit
evidence: The hook script, `.githooks/pre-commit`.
review-by: "2030-01-01"
---

# Runs as a local hook

`Control: ctl-runs-as-a-local-hook` `ACTIVE`

## What it checks

A pre-commit hook checks, on every commit, that a failure is answered with a status code.

## How it works

`mechanism` is `local-hook`, `frequency` is `per-commit`, `evidence` is filled in, and nothing reports it. That silence
is the assertion: without either value in the schema, this record fails `enum`.

## Coverage and gaps

A developer can bypass the hook, so a review still checks the rule.
