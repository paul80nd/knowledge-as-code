---
id: svc-{{slug}}
tier: descriptive
status: live
repo:
platform:
criticality:
depends-on:
  - svc-{{a}}
  - svc-{{b}}
data-stores:
owner:
tags: [ public ]
---

# {{Service name}}

`Service: svc-{{slug}}` `LIVE`

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Read with [contributing](/knowledge-as-code/contributing.md)** — how links and ids are written, and how a contribution
is reviewed — and [authoring](/knowledge-as-code/authoring.md), where the prose rules follow the document's tier. What
is below is only what a service adds to those.

**Frontmatter.** Delete this block once the fields above are filled in.

* **`id`** — `svc-` plus the name of the **deployable**, not of the repository it lives in. A repository shipping three
  app services yields three ids and none of them is the repository's name.
* **`status`** — `live` · `building` · `deprecated` · `retired`.
* **`repo`** — the repository a change to *this service* is made in. Where content it serves comes from somewhere else,
  say so in the body — the field takes one value and some services need more.
* **`platform`** — what it is **built on**, not what deploys it. The values are on [the type page](/services) and are
  derived from the estate, so check the list there rather than assuming this one.
* **`criticality`** — `critical` if a reader sees the failure, `important` if service degrades, `supporting` if the
  impact is internal only. It drives runbook and NFR priority, so be honest rather than generous.
* **`depends-on`** — other service ids, pointing downward only. An edge means this service is **configured to reach**
  that one. Messages over a bus are not an edge.
* **`owner`** — the named person answerable for the service, never a team alias.
* **`tags`** — one exposure tag, then any traits that apply. The vocabulary and the reasoning behind it are on
  [the type page](/services); it is derived from the estate, so read it before inventing a tag. Never restate another
  field, and never add one used by a single service.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences: what this component is for, in terms someone unfamiliar with it would understand.

## What it does

The responsibilities this service owns. Keep it to what is true today — this is a descriptive document and a catalogue
that disagrees with the estate is worse than none.

**Say where a claim came from.** "Taken from the application settings the infrastructure declares" and "its own README
says" are worth the words: they tell the next reader how much weight to give the line, and where to look when it goes
stale. Where something is genuinely not established, write that down as an open question rather than filling the gap
with a plausible guess — a guess here is repeated by everything that reads this page.

## Where it lives

* **Repository** — [`{{repo-name}}`]({{url}}) — `{{path within the repo, where it is not the root}}`
* **Platform** — {{runtime / framework}}
* **Deployed as** — {{app service / function app / CDN assets / …}}

_(A list rather than a table: three unlike facts, no column to scan down, and nothing to re-align when one of them
grows. Environments below is the other way round and stays a table.)_

## Environments

| Environment | URL | Notes |
|-------------|-----|-------|
| Development |     |       |
| Test        |     |       |
| Production  |     |       |

_(Where a service has no published URL for an environment, say so in Notes rather than leaving the row to imply one
exists. "No public URL" and "does not exist" are different facts and both are worth recording.)_

## Dependencies

* **[svc-{{a}}]** — what this service calls it for, and the setting that configures it.

_(Downward only. If nothing is listed, say "none" rather than leaving it blank — an empty section reads as unfinished,
and "none in this catalogue" is a different statement from "none at all".)_

## Data

What this service owns, and where it stores it. Link to the [data](/data) document for the domain rather than describing
schemas here. **"None of its own", verified, is a finding worth recording** — it is not the same as nobody having
looked.

## Operational notes

_(Only what is true of **this** service. A health-check endpoint every service exposes, or a runbook nobody has written
yet, is estate-wide background: recording it on every record adds length and no information. What earns a bullet is the
exception — the service hosted differently from its neighbours, the one with no test environment, the setting that
disagrees with the others, the criticality that wants defending. A service with no exceptions needs no section, so
delete this one.)_

* **Runbooks** — where any exist. Name an unadopted type rather than linking its folder: an empty folder is untracked,
  so a link to it resolves on the machine that created it and nowhere else.
* **NFRs** — where any exist.
* **Consumers** — the reverse view is not generated, so any list here is maintained by hand and will go stale. Say so.

[svc-{{a}}]: {{a}}.md
