---
id: dev-{{slug}}
type: deviation
tier: normative
status: draft
departs-from:
accepted-on:
review-by:
closed-on:
applies-to:
owner:
tags: [ a, b ]
---

# {{Title}}

`Deviation: dev-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a deviation adds to that.

**Frontmatter**

* **`status`**: `active` · `draft` · `closed`. A deviation past its review date is still `active`, because that is what
  is true.
* **`departs-from`**: the policy or standard clauses this departs from, one entry each. This corpus states no
  policies, so a policy clause is inherited and written `eng:pol-{{MNEM}}.{{CLAUSE}}`. A standard's rule is
  `std-{{MNEM}}.{{rule-heading-slug}}`, and an inherited one takes the same `eng:` prefix. A bare id is refused,
  because it claims a departure from every clause the rule carries. Write `none` for a shortcut that breaks no rule,
  which is technical debt and is recorded the same way.
* **`accepted-on`**: the day the owner accepted the risk. Required once the status leaves `draft`.
* **`review-by`**: the day somebody has to look at this again. Every deviation carries one.
* **`closed-on`**: the day the gap was fixed, or the risk consciously re-accepted. Required when the status is
  `closed`.
* **`owner`**: who accepts the risk, and has the authority to accept it. Write `human:alex.doe`, or
  `role:head-of-engineering` where the authority belongs to the post. Never a team.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what rule we are not following, and for how long.

## What we are doing instead

What actually happens today, in place of what the rule asks for. State it plainly enough that somebody can check
whether it is still true.

## Why we need it

The reason this was worth accepting. Say what the alternative cost, and who it would have cost.

_(A reason a reviewer can weigh. "It was quicker" is a reason. "Business need" is not.)_

## What compensates

What makes the risk survivable while this stands: an alert, a manual check, a narrower scope, a shorter retention.

_(A deviation with nothing here is an unmanaged risk. Say so if that is the truth, rather than inventing a control.)_

## How it closes

What has to be true for this to end, and who does it. Where closing means re-accepting the risk instead, say what that
review has to weigh.

## Scope

Which services, environments and data this reaches. A deviation with no boundary is one nobody can close.

## Related

* [std-{{MNEM}}] is the standard here that would close it.
* [svc-{{a}}] is the service it applies to.

_(`departs-from` already names the clause, so no bullet here repeats it. An inherited record lives in `.imports/`, so
where the prose above names a clause it uses a code span rather than a link.)_

[std-{{MNEM}}]: ../standards/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
