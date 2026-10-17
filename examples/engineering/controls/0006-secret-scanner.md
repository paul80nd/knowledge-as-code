---
id: ctl-0006
type: control
tier: normative
status: active
verifies: [ std-SECRET ]
mechanism: not-enforced
applies-to:
  - all
review-by: "2027-10-17"
owner: human:paul.law
tags: [ credentials, scanning, secrets ]
---

# Nothing checks a repository for a committed secret

`Control: ctl-0006` `ACTIVE`

No secret scanner runs over a repository or its history.

## What it checks

* [std-SECRET.nowhere-else-holds-a-secret] says a pipeline "**MUST** run a secret scanner over the repository and its
  history, and fail on a finding".
* It says a repository "**MUST NOT** contain a secret, in source, in a configuration file, in a pipeline definition or
  in a test fixture".

## How it works

Nothing runs. The check these rules need is gitleaks, in three places:

* A pipeline job checks out the full history and runs `gitleaks git --redact`. gitleaks exits non-zero on a finding,
  which fails the job.
* A pre-commit check runs `gitleaks git --pre-commit --staged --redact`, which reads only what is about to be
  committed.
* A Claude Code hook on `Write` and `Edit` pipes the content an agent is about to write into `gitleaks stdin`.

## Coverage and gaps

A reviewer reading the diff is the only thing that notices a secret today.

gitleaks finds a secret that matches one of its patterns, so a credential in an unfamiliar format passes it. A scanner
job in the pipeline definition a team edits can be removed in the same change. It proves the rule only where branch
policy requires it as a check the team cannot remove. The rules about images, logs and support tickets go unchecked.

[std-SECRET.nowhere-else-holds-a-secret]: ../standards/common/secret-handling.md#nowhere-else-holds-a-secret
