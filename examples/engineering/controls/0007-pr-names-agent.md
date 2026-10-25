---
id: ctl-0007
type: control
tier: normative
status: active
verifies: [ std-PR.AGENT ]
mechanism: not-enforced
applies-to:
  - all
review-by: "2027-10-17"
owner: human:paul.law
tags: [ agents, provenance, pull-requests ]
---

# Nothing checks that a pull request names the agent that produced it

`Control: ctl-0007` `ACTIVE`

No check fails an agent-produced pull request whose description leaves out the agent, the model or the task.

## What it checks

* [std-PR.AGENT] says a pull request containing agent-produced work "**MUST** name
  the agent, the model and the prompt or task that produced it".

## How it works

Nothing runs. The check this rule needs is a pipeline job on each pull request. It reads the description and the
commit messages. Where a commit has a `Co-Authored-By:` trailer naming an agent, or the description has a
`## Produced by` heading, the job fails unless that section has an `Agent:`, a `Model:` and a `Task:` line.

The description is written after the code, so neither a pre-commit check nor a Claude Code hook on `Write` can see it.

## Coverage and gaps

A reviewer reading the description is the only thing that notices a missing model or task today.

The check finds agent-produced work only where a trailer or a heading declares it. Whether a change is agent-produced
at all is a reviewer's judgement. The standard fixes no layout for the three facts, so the check needs one agreed
first. The rules about approval and self-merge belong to branch policy, and this check reads none of them.

[std-PR.AGENT]: ../standards/common/pull-requests.md#agent-agent-produced-work-says-what-produced-it
