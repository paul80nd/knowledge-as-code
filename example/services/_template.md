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
facets: [ public ]
tags:
---

# {{Service name}}

`Service: svc-{{slug}}` `LIVE`

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written,
and how it is reviewed. Below is only what a service adds to that.

**Frontmatter**

* **`id`**: `svc-` plus the name of the **deployable**, not the repository it lives in. A repository shipping three app
  services yields three ids, and none of them is the repository's name.
* **`status`**: `live` · `building` · `deprecated` · `retired`.
* **`repo`**: the repository a change to *this service* is made in. Where the content it serves comes from somewhere
  else, say so in the body. The field takes one value even though services may need more.
* **`platform`**: what it is **built on**, not what deploys it. [The type page](../services.md) derives the values from
  the estate, so read the list there before you pick one.
* **`criticality`**: `critical` if a reader sees the failure, `important` if service degrades, `supporting` if the
  impact is internal only. It drives runbook and NFR priority, so grade it honestly.
* **`depends-on`**: other service ids, pointing downward only (this service is **configured to reach** that one).
  Messages over a bus are not a dependency.
* **`owner`**: the named person answerable for the service, never a team alias.
* **`facets`**: one exposure, then any traits that apply. These slice the catalogue, so a value earns its place by
  grouping several services (CI will warn on one that does not). [The type page](../services.md) carries the vocabulary
  and the reasoning behind it, derived from this estate, so read it before you invent a facet.
* **`tags`**: words a reader would search for that this service does not otherwise say. One service may be the only
  one carrying a tag, which is what separates a tag from a facet. Never restate another field.

**The identity line.** Beneath the title come the type, the `id`, then the `status` in upper case. It is the first thing
a reader arriving from a citation sees, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences: what this service is for, in terms someone unfamiliar with it would understand.

## What it does

The responsibilities this service owns. Keep it to what is true today. This is a descriptive document, and a catalogue
that disagrees with the estate is worse than none.

**Say where a claim came from.** "Taken from the application settings the infrastructure declares" and "its own README
says" are worth the words. They tell the next reader how much weight to give the line, and where to look when it goes
stale. Where something is genuinely not established write it down as an open question. A guess here will spread with
everything that reads this page.

## Where it lives

* **Repository**: [`{{repo-name}}`]({{url}}), at `{{path within the repo, where it is not the root}}`
* **Platform**: {{runtime and framework}}
* **Deployed as**: {{app service, function app, CDN assets, …}}

_(Presented as a list because these are three unlike facts: no column to scan down, and nothing to re-align when one of
them grows. Environments below has columns and stays a table.)_

## Environments

| Environment | URL | Notes |
|-------------|-----|-------|
| Development |     |       |
| Test        |     |       |
| Production  |     |       |

_(Where a service has no published URL for an environment say so in Notes. An empty row may imply one exists. "No public
URL" and "does not exist" are different facts, and both are worth recording.)_

## Dependencies

* **[svc-{{a}}]**: what this service calls it for, and the setting that configures it.

_(Downward only. Where nothing is listed, write "none". An empty section reads as unfinished, and "none in this
catalogue" is a different statement from "none at all".)_

## Data

What this service owns, and where it stores it. Link the [data](../data.md) document for the domain, which is where a
schema is described.

**"None of its own", when verified, is a finding worth recording.** It is a different fact from nobody having looked.

## Operational notes

_(Write only what is true of **this** service. A health-check endpoint every service exposes, or a runbook nobody has
written yet, is estate-wide background: recording it on every record adds length and no information. What earns a bullet
is the exception: the service hosted differently from its neighbours, the one with no test environment, the setting that
disagrees with the others, the criticality that wants defending. A service with no notes needs no notes section, so
delete this one.)_

* **Runbooks**: any that exist. Where the type is unadopted, name it in words. Its folder is empty and untracked, so a
  link to it resolves on the machine that created it and nowhere else.
* **NFRs**: where any exist.
* **Consumers**: nothing generates the reverse view, so a list here is maintained by hand and goes stale. Say so.

[svc-{{a}}]: {{a}}.md
