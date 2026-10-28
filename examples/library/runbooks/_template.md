---
id: rbk-{{slug}}
type: runbook
tier: procedural
status: draft
severity:
last-rehearsed:
rehearsal-frequency:
owner:
tags: [ a, b ]
---

# {{What is broken}}

`Runbook: rbk-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a runbook adds to it. Add an
optional field from [the type page](../runbooks.md#metadata) where you have a value for it.

**Frontmatter**

Keep the slug to 30 characters at most: the filename without `.md` and without any number or mnemonic at its
head. Write each list field's entries in alphabetical order.

Title this document for the **failure**, not the fix. A reader at 2am searches for the failure.

* **`id`**: `rbk-` plus a slug naming the failure, as `rbk-estate-rebuild`. Save the file as `<slug>.md`.
* **`severity`**: how urgent. The `Impact` section says who is affected.
* **`last-rehearsed`**: the last walk end to end, as `{ at: "2026-10-20", by: human:alex.doe, method: tabletop }`.
  `at` is the day, quoted. `by` is a person as `human:alex.doe`, or an agent with its version. `method` is `live` where
  you caused the fault, and `tabletop` where you read the steps against the running system. Write `never` where nobody
  has walked it, which is what you want to know before the incident rather than during it.
* **`rehearsal-frequency`**: pick `on-change` where a change to the system is what breaks the steps. `staleness-loud`
  warns at 92 days whichever value you pick.
* **`requires-tools`**: list every tool the reader installs before starting.
* **`requires-access`**: list every permission and role the runbook needs. Discovering you lack one mid-incident is its
  own outage. Where a step needs somebody else's approval, name who it is and how to reach them at 2am.

**The steps.** Write short sentences in the imperative. Leave out background and rationale. Link an explanation in
`Related` for a reader who wants the theory afterwards.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## Symptoms

* What you are seeing: alerts, error messages and customer reports, in the words they appear in.
* What you are **not** seeing, where its absence is diagnostic.

_(Symptoms come first because that is how the reader finds this document.)_

## Impact

Who cannot do what while this is broken. Name the users, the service or the team, and what stops for them.

_(One or two lines. A reader decides here whether to wake somebody.)_

## Immediate actions

1. {{Stop the bleeding.}}
2. Tell the people in [Communication](#communication).

_(Before diagnosis. Contain first, understand afterwards.)_

## Diagnosis

**Is {{condition}}?**

* **Yes** → {{action}}, then go to [Resolution](#resolution).
* **No** → continue.

**Is {{next condition}}?**

* **Yes** → {{action}}, then go to [Resolution](#resolution).
* **No** → [escalate](#escalation).

_(A tree, not prose. Each branch links to [Resolution](#resolution) or to [escalate](#escalation), or writes
`continue` to fall through to the next question. The last question has nothing to fall through to, so close
every branch of it with a link. One branch of the tree reaches the escalation.)_

## Resolution

1. {{Restore service.}}
2. {{The next action.}}

Confirmed when {{what tells you service is back}}. If a step does not do what it says, [escalate](#escalation).

_(Numbered and imperative. End with the `Confirmed when` line, so the reader knows when to stop. Close that line with a
link to the escalation, so a reader whose step did not work knows where to go.)_

## Escalation

| When          | Who             | How         |
|---------------|-----------------|-------------|
| {{condition}} | {{name / role}} | {{channel}} |

_(If this document is long, put this table near the top of the page. A reader has to find it without scrolling.)_

## Communication

| Who             | What they need to know | How often     |
|-----------------|------------------------|---------------|
| {{audience}}    | {{the message}}        | {{how often}} |

_(Escalation wakes the people who can help. This tells the people who are waiting.)_

## Afterwards

* Raise a postmortem if severity warrants it.
* Update this runbook with anything that was wrong or missing.

## Related

* [svc-{{a}}] is the service this covers.
* [exp-{{a}}] explains how the system works. Read it afterwards, not now.

[exp-{{a}}]: ../explanations/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
