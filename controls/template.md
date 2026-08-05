---
id: ctl-NNNN
tier: normative
status: planned
verifies:
mechanism:
frequency:
evidence:
applies-to:
owner:
tags: [ a, b ]
---

# Control: <Title>

_(Frontmatter notes — delete this block. **`status`**: `active` · `planned` · `retired`. **`verifies`** lists standard
ids, ideally rule-level anchors. **`mechanism`**: `ci` · `review-checklist` · `manual-periodic` · `runtime-alert` ·
`not-enforced` — pick the real one. `not-enforced` is a first-class value and the whole point of the coverage report; do
not invent a mechanism to avoid using it. **`frequency`**: `per-pr` · `per-deploy` · `daily` · `monthly` ·
`quarterly` · `annual`.)_

One sentence: what this control proves.

## What it checks

The rules this control verifies, quoted or anchored:

* [std-NNNN](/standards/…#anchor) — "Services **MUST** …"
* [std-NNNN](/standards/…#anchor) — "Responses **MUST NOT** …"

_(Name rules, not intentions. "We review carefully" is not a control; "the PR template requires a tick against each
conformance checklist item" is.)_

## How it works

The mechanism, concretely. Which pipeline step, which tool, which checklist, which alert rule. Enough that someone can
find it and confirm it is running.

## Evidence

Where the proof lives — the build log, the audit note, the dashboard. Someone asking "how do you know?" should be able
to follow this to an artefact.

## Coverage and gaps

What this control does **not** catch. Most controls are partial, and a stated gap is far more useful than an implied
guarantee.

_(If `mechanism` is `not-enforced`, this is the section that matters: say what would need to exist, and what the
exposure is meanwhile.)_

## Owner

Who is answerable for this control continuing to run, and who notices when it stops.
