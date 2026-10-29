---
id: dev-{{slug}}
type: deviation
tier: normative
status: draft
departs-from:
risk:
accepted-on:
review-by:
closed-on:
owner:
tags: [ a, b ]
---

# {{Title}}

`Deviation: dev-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a deviation adds to it. Add an
optional field from [the type page](../deviations.md#metadata) where you have a value for it.

**Write one deviation per departure.** A page collecting every exception to one policy loses the owner, the date and the
id that a citation needs. Write it before you depart, or immediately afterwards where an incident left no time.

**Frontmatter**

Keep the slug to 30 characters at most: the filename without `.md` and without any number or mnemonic at its
head. Write each list field's entries in alphabetical order.

* **`id`**: `dev-` plus a slug naming the departure, as `dev-legacy-report-secrets`. Save the file as `<slug>.md`.
* **`status`**: a deviation past its review date is still `active`, because that is what is true.
* **`departs-from`**: the policy or standard clauses this departs from, one entry each. This corpus states no
  policies, so a policy clause is inherited and written `eng:pol-{{MNEM}}.{{CLAUSE}}`. A standard's rule is
  `std-{{MNEM}}.{{rule-heading-slug}}`, and an inherited one takes the same `eng:` prefix. A bare id is refused,
  because it claims a departure from every clause the rule carries. Name only a `MUST` or a `MUST NOT` the practice
  breaks: departing from a `SHOULD` takes a reason in the change, and no deviation. Write `none` for a shortcut that
  breaks no rule, which is technical debt and is recorded the same way.
* **`risk`**: how bad the risk is once what compensates is working, and never how bad the rule's authors thought it
  was. A `high` rating brings `review-by` inside six months, which `high-risk-review-window` warns on. Name the risk
  plainly in `What the risk is`, because a record that reads as a defence of the departure hides what the reviewer
  needs.
* **`accepted-on`**: the day the owner accepted the risk. Required once the status leaves `draft`.
* **`review-by`**: the day somebody has to look at this again. Every deviation has one, a draft included. Once it
  passes, `expiry` warns on an active record and `draft-expiry` on a draft.
* **`closed-on`**: the day the gap was fixed, or the risk re-accepted with the same scrutiny as the first time. Move
  the status in the same change. Required when the status is `closed`.
* **`owner`**: the individual who accepts the risk, and has the authority to accept it. Write `human:alex.doe`. This
  type refuses `role:`, where every other type admits it.
* **`assigned-to`**: who does the work that closes this, where that is somebody else. A post may do the work, so this
  one takes `role:head-of-engineering` as well as `human:alex.doe`. Leave it out where `owner` does both.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what rule we are not following, and for how long.

## What we are doing instead

What actually happens today, in place of what the rule asks for. State it plainly enough that somebody can check
whether it is still true.

## Why we need it

The reason this was worth accepting. Say what the alternative cost, and who it would have cost.

_(A reason a reviewer can weigh. "It was quicker" is a reason. "Business need" is not.)_

## What the risk is

What can still go wrong while this stands, and who it costs. State the risk left after what compensates, not the risk
the rule was written against.

_(This is the section `risk` rates. A record whose rating and whose section disagree is one a reviewer cannot weigh.)_

## What compensates

What makes the risk survivable while this stands: an alert, a manual check, a narrower scope, a shorter retention.

_(A deviation with nothing here is an unmanaged risk. Say so if that is the truth, rather than inventing a control.)_

## How it closes

What has to be true for this to end, and who does it. Say what closing it costs: the staff time, and anything that has
to be bought. Where closing means re-accepting the risk instead, say what that review has to weigh.

_(A deviation nobody has priced is one nobody can choose to close.)_

## Scope

Which services, environments and data this reaches. A deviation with no boundary is one nobody can close.

## Related

* [std-{{MNEM}}] is the standard here that would close it.
* [svc-{{a}}] is the service it applies to.

_(`departs-from` already names the clause, so no bullet here repeats it. An inherited record lives in `.imports/`, so
where the prose above names a clause it uses a code span rather than a link.)_

[std-{{MNEM}}]: ../standards/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
