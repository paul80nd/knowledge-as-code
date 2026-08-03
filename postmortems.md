# Postmortems

What actually happened, and why.

**[→ Index](postmortems/INDEX.md)**

## What is a postmortem?

A blameless account of an incident: the timeline, the impact, the root cause, the contributing factors, what went well,
and the actions that came out of it.

Immutable once published, like an [ADR](/adrs) — a postmortem is a record of what was understood at the time. New
understanding produces a new document, not a rewrite.

## Why we use them

The ADR log records what we intended. Postmortems record what the estate did about it — and the gap between the two is
where most real learning lives.

They are also the richest source of other knowledge in the corpus. A single incident routinely produces an
[FAQ](/faqs), a [runbook](/runbooks), a revised [NFR](/nfrs) and occasionally an [ADR](/adrs). And the pattern across
several postmortems — the recurring root cause nobody noticed was recurring — is the highest-signal thing this wiki can
tell you.

## Scope

**Blameless, always.** The output is a system that fails less, not a person who feels worse. Write about decisions and
conditions, not individuals — "the deploy ran before the migration completed", not "X deployed too early".

Boundaries:

* **[Runbook](/runbooks)** — instructions for an incident that might happen. A postmortem is an account of one that did.
* **[FAQ](/faqs)** — a reusable fix. A postmortem is a specific narrative, and often produces an FAQ as a by-product.
* **A work item** — actions belong in ADO. The postmortem links to them; it is not a tracker.

Not every incident needs one. Use severity as the trigger and be consistent about it, so the absence of a postmortem
means something.

## Metadata

<!-- BEGIN GENERATED: schema-postmortems -->

| Field         | Req | Type   | Notes                                     |
|---------------|-----|--------|-------------------------------------------|
| `status`      | ●   | enum   | `draft` · `published`                     |
| `occurred-on` | ●   | date   | Quoted                                    |
| `detected-on` | ●   | date   | Quoted                                    |
| `duration`    | ●   | string | e.g. `3h 20m`                             |
| `severity`    | ●   | enum   | `sev1` · `sev2` · `sev3`                  |
| `affected`    | ●   | list   | Service or capability ids                 |
| `prompted`    |     | list   | ADR / runbook / NFR ids that came from it |

<!-- END GENERATED: schema-postmortems -->

## Adding a postmortem

1. Copy [`template.md`](postmortems/template.md) to `NNNN-kebab-slug.md`, named for the symptom rather than the cause.
2. Write the timeline first, from the evidence, before anyone theorises. `occurred-on` and `detected-on` are separate
   fields for a reason — the gap between them is often the finding.
3. State the impact in customer terms, not system terms.
4. Separate root cause from contributing factors. There is usually one of the first and several of the second.
5. Include **what went well**. A postmortem that only lists failures teaches half the lesson.
6. Record actions as links to work items, and fill `prompted` with anything this incident caused to be written.
7. `status: draft` while it is being assembled; `published` freezes it.

**Conventions**

* **Immutable once published.** Corrections are limited to typos. A materially different understanding is a new
  postmortem that references this one.
* **No names in causal statements.** Roles and systems, not people.
* **Measure against the [NFRs](/nfrs)** where they exist. If the incident breached a target, say so; if there was no
  target, that is itself a finding.

## What CI checks

<!-- BEGIN GENERATED: checks-postmortems -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-postmortems -->
