---
id: pmt-{{nnnn}}
tier: decided
status: draft
occurred-on:
detected-on:
duration:
severity:
affected:
prompted:
owner:
tags: [ a, b ]
---

# {{The symptom, as customers experienced it}}

`Postmortem: pmt-{{nnnn}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a postmortem adds to that.

**Frontmatter**

* **`occurred-on` / `detected-on`**: separate quoted dates. The gap between them is often the finding.
* **`severity`**: `sev1` · `sev2` · `sev3`.
* **`affected`**: service or capability ids.
* **`prompted`**: the ADRs, runbooks, NFRs and FAQs this incident caused to be written.
* **`status`**: `draft` while it is being assembled; `published` freezes it. Postmortems are **immutable once
  published**. A materially different understanding becomes a new document that references this one.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## Summary

Three or four sentences: what broke, who was affected, how long, and what fixed it. Written so someone can decide in
fifteen seconds whether to read the rest.

## Timeline

| Time (UTC) | Event |
|------------|-------|
|            |       |

_(Assemble the timeline from logs, alerts and messages before anyone theorises. Include when it started, when it was
detected, when it was understood, and when it was resolved; those are four different moments.)_

## Impact

In customer terms, not system terms. How many, for how long, what they could not do. Include revenue or contractual
consequence where it is known, and say so where it isn't.

Measured against [nfr-{{a}}]: {{met / breached}}. _(If no NFR covered this, that is itself a finding.)_

## Root cause

The one thing that, had it been different, would have prevented this. Resist listing several. Contributing factors are
the next section.

## Contributing factors

* {{Condition that made it more likely, or harder to detect, or slower to fix.}}

_(There is usually one root cause and several contributing factors. The factors are where most of the improvement
lives.)_

## What went well

Genuinely. Detection that worked, a rollback that held, a runbook that was accurate. A postmortem that only lists
failures teaches half the lesson and makes the next one harder to write honestly.

## Actions

| Action | Work item | Owner |
|--------|-----------|-------|
|        | #{{item}} |       |

_(Actions live in ADO; this links to them. A postmortem is not a tracker.)_

## Related

* [rbk-{{a}}] was written or revised as a result.
* [adr-{{a}}] is the decision this prompted.

---

_(**Blameless, always.** Write about decisions and conditions, not individuals: "the deploy ran before the migration
completed", not "X deployed too early". The output is a system that fails less, not a person who feels worse.)_

[adr-{{a}}]: ../adrs/{{a}}.md
[nfr-{{a}}]: ../nfrs/{{a}}.md
[rbk-{{a}}]: ../runbooks/{{a}}.md
