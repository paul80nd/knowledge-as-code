# Explanations

Narrative documents that help you *understand* how the platform works and why it is shaped the way it is.

**[→ Index](explanations/INDEX.md)**

## What is an explanation?

An explanation is prose whose job is comprehension rather than instruction. An architecture overview, a walkthrough of
how a request flows through the estate, an account of why the testing approach is structured as it is. It is the
document you read to build a mental model, not the one you consult mid-task.

Every other type in this wiki answers a narrow question — what was decided, what must I do, how do I do it, what is this
component. Explanations answer "how does this hang together", which none of the others can without becoming something
they shouldn't.

## Why we use them

Without a home for narrative, this kind of content either doesn't get written or accumulates at the top level with no
owner and no review date — which is how it goes stale. Typing it means an explanation has an owner and a `review-by`
like anything else.

## Scope

An explanation is **not**:

* **Normative** — if it says what you must do, it is a [standard](/standards).
* **Procedural** — if it says how to perform a task, it is a [process](/processes) or a
  [runbook](/runbooks).
* **A catalogue entry** — if it describes one component, it is a [service](/services).
* **A decision** — if it records what was chosen and why, it is an [ADR](/adrs).

**Explanations link rather than restate.** An architecture overview points at the services, capabilities and ADRs that
hold the detail; it does not duplicate them. An explanation that starts accumulating facts of its own has become a
maintenance liability, and its facts will be the ones that go stale first.

If a document could plausibly be an explanation *or* something else, it is the something else. This type is the
residual, and residual categories become dumping grounds unless the bar for entry is kept high.

## Metadata

<!-- BEGIN GENERATED: schema-explanations -->

| Field       | Req | Type | Notes                                           |
|-------------|-----|------|-------------------------------------------------|
| `status`    | ●   | enum | `draft` · `active` · `stale`                    |
| `explains`  | ●   | list | Service or capability ids                       |
| `review-by` | ●   | date | Quoted. The field that stops this type rotting. |

<!-- END GENERATED: schema-explanations -->

## Adding an explanation

1. Check it isn't one of the four things above.
2. Copy [`template.md`](explanations/template.md) to a kebab-case filename — no number prefix; explanations are named,
   not sequenced.
3. Set `explains` to the services or capabilities it covers, and `review-by`.
4. Write it as prose. Link out for every concrete fact you're tempted to state.

## What CI checks

<!-- BEGIN GENERATED: checks-explanations -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-explanations -->
