# FAQs

Problems we have hit before, with the fix that worked.

**[→ Index](faqs/INDEX.md)**

## What is an FAQ?

One document per problem: the symptom as you would actually encounter it, what causes it, how to fix it, and why it
happens. Confirmed by a human, so it carries authority.

Add one when an investigation cost real time. Future you will be grateful, and so will the next session that hits it.

## Why we use them

The same problems resurface, and the cost is paid again by whoever hits them next. An FAQ converts two hours of
debugging into a thirty-second search — provided it is findable, which is what `symptom-keywords` is for.

They are also the destination for the promotion path: a [discovery](/discoveries) that turns out to be real, general and
current becomes an FAQ.

## Scope

An FAQ is **confirmed**. A human has verified the problem is real, the fix works, and both are still current. That is
what separates it from a [discovery](/discoveries), which is captured cheaply and might be wrong or already fixed.

**Never write straight to an FAQ from a session.** Capture as a discovery and let promotion do the work — an agent
cannot confirm its own observations.

Other boundaries:

* **[Runbook](/runbooks)** — if it needs a diagnosis tree and an escalation path, it is a runbook. An FAQ has a known
  fix, not a decision procedure.
* **[Standard](/standards)** — if the real answer is "people should stop doing the thing that causes this", the fix is a
  rule, and that needs an [ADR](/adrs) first.
* **One problem per document.** A page of assorted gotchas cannot be found by symptom, which defeats the purpose.

<!-- BEGIN GENERATED: schema-faqs -->

| Field              | Req | Type   | Notes                                      |
|--------------------|-----|--------|--------------------------------------------|
| `status`           | ●   | enum   | `active` · `superseded` · `fixed-upstream` |
| `symptom-keywords` | ●   | list   | Be generous — this is what people grep     |
| `applies-to`       |     | list   | Service ids                                |
| `promoted-from`    |     | id     | Discovery id                               |
| `confirmed-by`     | ●   | string | The human who verified it                  |
| `confirmed-on`     | ●   | date   | Quoted                                     |
| `review-by`        | ●   | date   | Quoted                                     |

<!-- END GENERATED: schema-faqs -->

## Adding an FAQ

1. Copy [`template.md`](faqs/template.md) to `<slug>.md`, named for the symptom rather than the cause — that is what
   people search for.
2. Make the H1 the symptom as encountered, in the words the error message or the user would use.
3. Be generous with `symptom-keywords`. Include the literal error text, the service names, and the words someone would
   type who doesn't yet know what is wrong.
4. Record `confirmed-by` and `confirmed-on` — a named person and a real date. An FAQ nobody confirmed is a discovery.
5. Set `review-by`. Fixes go stale when the thing they fix gets rewritten.

**Conventions**

* **Symptom first, cause second, fix third.** The reader arrives with a symptom and nothing else.
* **Record how you found it**, not just what it was. The diagnostic route is often more reusable than the fix.
* **If the root cause is still open**, say so, and raise it somewhere it can be tracked. An FAQ is not a place to park
  unowned work.

## What CI checks

<!-- BEGIN GENERATED: checks-faqs -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-faqs -->