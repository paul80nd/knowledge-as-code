# Discoveries

Things we noticed and haven't verified yet.

**[→ Index](discoveries/INDEX.md)**

## What is a discovery?

A short, unreviewed note of something observed during work. *"The build fails silently if X."* *"The legacy API returns
200 with an empty body when Y."* Possibly wrong, possibly already fixed, possibly situational.

Deliberately low-ceremony: a title, what you saw, the context you were in, and why it might matter. Nothing more.

## Why we use them

Capture has to be nearly free or it doesn't happen. Nobody writes up a gotcha if doing so requires a template, an owner
and two reviewers — so observations are recorded with **no review at all**, marked unverified, and expire on their own
if nothing promotes them.

The rigour lives at promotion, not capture. That gradient — cheap in, deliberate out — is what lets the corpus grow
without the average trustworthiness falling.

This is also where AI sessions contribute. A session that discovers something useful has somewhere to put it, and the
discovery outlives the session.

## Scope

Discoveries are **perishable and carry no authority**. They expire after 90 days by default, and that is a feature: an
observation nobody has needed in three months was probably situational.

Boundaries:

* **[FAQ](/faqs)** — confirmed, general, current, and carries authority. That is what a discovery is promoted *to*.
* **Session state** — where a piece of work got to. That is personal handover and is **not stored in this repository**.
* **A bug** — if it is broken and should be fixed, raise a work item. A discovery records something surprising, not
  something owed.

## Metadata

<!-- BEGIN GENERATED: schema-discoveries -->

| Field         | Req | Type   | Notes                                                    |
|---------------|-----|--------|----------------------------------------------------------|
| `status`      | ●   | enum   | `open` · `promoted` · `expired` · `rejected`             |
| `source`      | ●   | enum   | `human` · `session` · `dreamed`                          |
| `confidence`  | ●   | enum   | `unverified` · `corroborated` · `confirmed`              |
| `expires`     | ●   | date   | Quoted. Default: 90 days from capture.                   |
| `provenance`  |     | string | Where it came from — required when `source` is `dreamed` |
| `applies-to`  |     | list   | Service ids                                              |
| `promoted-to` |     | id     | Set when promoted                                        |

<!-- END GENERATED: schema-discoveries -->

## Capturing a discovery

Low ceremony on purpose. Title, what you observed, why it might matter, and the context you were in. Set
`confidence: unverified` unless you've genuinely proven it. Don't tidy it up; don't verify it first; don't write it as
an FAQ.

Discoveries expire after 90 days by default. That's a feature — an observation nobody has needed in three months was
probably situational.

## Promoting a discovery to an FAQ

The one flow that crosses tiers, and the one worth getting right.

1. A human confirms the observation is real, general, and still current.
2. Create the FAQ with `promoted-from`, `confirmed-by` and `confirmed-on`.
3. Set the discovery's `status: promoted` and `promoted-to`.
4. If the underlying issue is actually a rule people should follow, the promotion target is a **standard**, not an FAQ —
   and that needs an ADR first.

Promotions proposed automatically arrive as PRs carrying `provenance` back to the passage that produced them. Review
that provenance; it's the whole reason the field exists. An unverifiable proposal is a rejected proposal.

_(The automatic half is not built yet — see [Automation](/knowledge-as-code/automation.md).)_

## What CI checks

<!-- BEGIN GENERATED: checks-discoveries -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-discoveries -->
