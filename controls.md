# Controls

How we know the standards are actually being followed.

**[→ Index](controls/INDEX.md)**

## What is a control?

A control binds a rule to its enforcement. It names the standards rules it verifies, the mechanism that does the
verifying, how often it runs, and where the evidence lives.

## Why we use them

A rulebook nobody checks becomes fiction slowly and without anyone noticing. Controls make the gap between *rule* and
*enforcement* visible and countable: what proportion of our standards are actually real?

That number is useful at three engineers and at three hundred. It is also the honest answer when someone asks whether we
follow our own standards — rather than pointing at the standards and hoping.

The most valuable value in the `mechanism` enum is **`not-enforced`**. It converts an aspiration into a visible number
instead of letting it hide. Do not invent a mechanism to avoid using it.

## Scope

A control is **not** the rule. The rule lives in a [standard](/standards); the control says how it is checked.

| Standard                                                   | Control                                                                  |
|------------------------------------------------------------|--------------------------------------------------------------------------|
| "Secrets **MUST** come from Key Vault."                    | "CI runs secret scanning on every PR; failures block merge."             |
| "Every public endpoint **MUST** carry a conformance test." | "Quarterly manual audit of the OpenAPI document against the test suite." |

If it can fail a build, block a merge, raise an alert or produce an audit artefact, it is a control. If it tells you
what to do, it is a standard.

One control may verify several rules, and one rule may need several controls. Controls apply to
[services](/services) — a control with no scope is a control nobody owns.

## Metadata

<!-- BEGIN GENERATED: schema-controls -->

| Field       | Req | Type   | Notes                                                                            |
|-------------|-----|--------|----------------------------------------------------------------------------------|
| `status`    | ●   | enum   | `active` · `planned` · `retired`                                                 |
| `verifies`  | ●   | list   | Standard ids, ideally rule-level anchors                                         |
| `mechanism` | ●   | enum   | `ci` · `review-checklist` · `manual-periodic` · `runtime-alert` · `not-enforced` |
| `frequency` |     | enum   | `per-pr` · `per-deploy` · `daily` · `monthly` · `quarterly` · `annual`           |
| `evidence`  |     | string | Where the proof lives                                                            |

<!-- END GENERATED: schema-controls -->

## Adding a control

1. Copy [`template.md`](controls/template.md) to `NNNN-kebab-slug.md`.
2. Name the rules it verifies in `verifies` — rule-level anchors where the standard has them, the standard id otherwise.
3. Pick the real `mechanism`. If nothing currently checks the rule, that is `not-enforced` and the control still gets
   written — an unenforced rule you know about is worth more than one you don't.
4. Record where the evidence lives: the pipeline step, the checklist, the alert rule, the audit note.

**Conventions**

* **A control names rules, not intentions.** "We review carefully" is not a control; "the PR template requires a tick
  against each conformance checklist item" is.
* **`not-enforced` is a first-class value**, not a failure state. The coverage report is only useful if it is honest.
* **Controls follow the tooling.** When enforcement changes, update the control rather than the standard — the rule
  didn't change, the way we check it did.

## What CI checks

<!-- BEGIN GENERATED: checks-controls -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-controls -->
