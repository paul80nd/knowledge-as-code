---
id: pmt-{{nnnn}}
type: postmortem
tier: decided
status: draft
occurred-at:
detected-at:
restored-at:
duration:
severity:
affected:
owner:
tags: [ a, b ]
---

# {{The symptom, as customers experienced it}}

`Postmortem: pmt-{{nnnn}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a postmortem adds to that.

**Frontmatter**

**The three moments.** Each is a UTC timestamp, as `2026-09-07T20:18:00Z`, and each names a different point.

| Field         | The moment to pick                                                              |
|---------------|----------------------------------------------------------------------------------|
| `occurred-at` | The impairment began. Not when the change that caused it shipped.                |
| `detected-at` | A person or an alert first knew. The gap from `occurred-at` is often the finding. |
| `restored-at` | Service was back for users. Not when the cause was fixed.                        |

**`restored-at` is the one to get wrong.** MTTR names four different measures: time to respond, to repair, to recover
and to resolve. This field is *recover*: service back for users, which is what an availability budget counts. An
incident whose actions run for weeks still has a `restored-at` on the day.

**`detected-at` may fall after `restored-at`.** An incident can recover before anybody notices, and one found later in
the logs is written that way. Only `restored-at` before `occurred-at` is refused.

* **`duration`**: the span from `occurred-at` to `restored-at`, as an ISO 8601 duration: `PT12M`, `PT4H20M`. Hours,
  minutes and seconds, never days. `kac validate` prints the right value where yours disagrees.
* **`severity`**: `sev1` · `sev2` · `sev3`.
* **`affected`**: service or capability ids.
* **`prompted`**: the NFRs, fixes and standards this incident caused to be written.
* **`status`**: `draft` while it is being assembled; `published` freezes it. Postmortems are **immutable once
  published**. A materially different understanding becomes a new document that references this one.

**Fields this template leaves out.** This type takes optional fields the frontmatter above does not carry. Add a key
where you have a value for it, and leave it out where you do not. [The type page](../postmortems.md#metadata) lists
every field and says what each one holds.

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

| Action | Work item    | Owner |
|--------|--------------|-------|
|        | gh#{{item}}  |       |

_(Actions live in GitHub issues; this links to them. A postmortem is not a tracker.)_

## Related

* [fix-{{a}}] is the fix this incident produced.
* [nfr-{{a}}] is the target this incident breached.

---

_(**Blameless, always.** Write about decisions and conditions, not individuals: "the deploy ran before the migration
completed", not "X deployed too early". The output is a system that fails less, not a person who feels worse.)_

[fix-{{a}}]: ../fixes/{{a}}.md
[nfr-{{a}}]: ../nfrs/{{a}}.md
