---
id: std-PR
tier: normative
status: draft
implements: [ pol-AGNT.ACCEPT, pol-AGNT.EQUAL, pol-AGNT.PROV, pol-AGNT.SELFVER, pol-EVER.BRANCH, pol-EVER.INTENT,
  pol-SECD.CODEREV ]
applies-to:
  - all
review-by: "2027-08-26"
owner: paul.law
tags: [ agents, code-review, pull-requests ]
---

# A pull request carries its reasoning and one accountable approval

`Standard: std-PR` `DRAFT`

## Summary

Every change to the default branch arrives as a pull request that says what it does and why, and a named person other
than the author approves it before it merges.

## Rules

### A change arrives as a pull request

- A pull request **MUST** merge into the default branch rather than reaching it by direct push.
- A pull request description **MUST** link the work item that asked for the change.

_**Covers:** [pol-EVER].BRANCH, [pol-EVER].INTENT_

### Somebody other than the author approves it

- A pull request **MUST** carry at least one approval from somebody other than the author before it merges.
- A reviewer **MUST** check the change against the standards that apply to it, and not read for style alone.
- A reviewer **MUST** state what they checked when they approve.

_**Covers:** [pol-SECD].CODEREV_

### Agent-produced work says what produced it

- A pull request holding agent-produced work **MUST** name the agent, the model and the prompt or task that produced it.
- Agent-produced work **MUST** carry the approval of a named person, who owns the change afterwards.
- A reviewer **MUST NOT** accept an agent's own summary of a change as evidence that the change is correct.
- Branch policy **MUST** apply to agent-produced work unchanged, with no exemption and no second route in.

_**Covers:** [pol-AGNT].ACCEPT, [pol-AGNT].EQUAL, [pol-AGNT].PROV, [pol-AGNT].SELFVER_

## Examples

```
Good
  ## What
  Bounds the covers lookup at 2s, so a slow upstream degrades the page rather than hanging it.

  ## Why
  #4812. The 14 March incident traced back to an unbounded call.

  ## Produced by
  Agent: claude-opus-5. Task: "add a timeout to CoverClient".
  Accepted by: r.okafor, who ran the integration suite against a stubbed slow upstream.

Avoid
  ## What
  Small fix.

  ## Produced by
  AI. The agent confirmed the change is correct and the tests pass.
```

The second names no work item, no model and no task, so a reviewer cannot tell what was asked for. It then offers the
agent's own account in place of a check.

## Conformance checklist

- [ ] Branch policy requires a pull request, and requires at least one approval.
- [ ] Branch policy excludes the author from satisfying that approval.
- [ ] The description links a work item.
- [ ] Where an agent produced any part of the change, the description names the agent, the model and the task.
- [ ] The reviewer states what they checked, rather than restating what the change claims.

## Rationale and provenance

A review is the last point at which a person sees the change before it becomes ours. An agent can produce a change far
faster than it can be checked, so the record of what produced it is what a reviewer works from.

## Sources and further reading

- **Informative.** [Google's Code Review Developer Guide] covers what a reviewer looks for and how they say it. These
  rules say who reviews and what they record, and leave the reading itself to that guide.

[Google's Code Review Developer Guide]: https://google.github.io/eng-practices/review/
[pol-AGNT]: ../../policies/governance/agnt-agents-propose-people-decide.md#clauses
[pol-EVER]: ../../policies/delivery/ever-everything-in-version-control.md#clauses
[pol-SECD]: ../../policies/security/secd-security-by-design.md#clauses
