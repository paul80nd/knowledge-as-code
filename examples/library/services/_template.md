---
id: svc-{{slug}}
type: service
tier: descriptive
status: live
component-type:
repos:
platform:
criticality:
monitoring-output:
depends-on:
  - svc-{{a}}
  - svc-{{b}}
owner:
facets: [ public ]
---

# {{Service name}}

`Service: svc-{{slug}}` `LIVE`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a service adds to it. Add an
optional field from [the type page](../services.md#metadata) where you have a value for it.

**Say where a claim came from.** "Taken from the application settings the infrastructure declares" and "its own README
says" weigh differently, and the next reader needs to know which one they have. Where something is not established,
write it down as an open question, because nobody reading later can tell your guess from a fact.

**Frontmatter**

* **`id`**: `svc-` plus the name of the **deployable**, not the repository it lives in. A repository shipping three app
  services yields three ids, and none of them is the repository's name. `repos` states the repository. Contributors get
  this wrong more often than anything else here. Save the file as `<slug>.md`.
* **`component-type`**: the sort of component this is, such as an API, a website, a worker or a command line tool.
  [The type page](../services.md) derives the values from the estate, so read the list there before you pick one.
* **`repos`**: every repository a change to *this service* is made in. Most services name one. An asset surface names
  the repository defining it alongside the repository publishing what it serves. Where another service fills that
  surface at runtime, that service is not a repository, and the body says so.
* **`platform`**: what it is **built on**, not what deploys it and not what sort of thing it is. [The type
  page](../services.md) derives the values from the estate, so read the list there before you pick one.
* **`criticality`**: `critical` if a reader sees the failure, `important` if service degrades, `supporting` if the
  impact is internal only. It drives runbook and NFR priority, so grade it honestly. A service graded above one of its
  own dependencies is not automatically wrong, but defend the grade in the record.
* **`monitoring-output`**: what a live problem with this service raises. `alert` if a person must act now, `ticket` if
  the system files one and a person acts later, `log` if it is recorded and nobody reads it, `none` if nothing is
  emitted. Required once the service is `live` or `deprecated`. Write `none` where that is the truth: CI warns on a
  `live` or `deprecated` service graded `critical` that emits nothing, and that warning stands until something does.
* **`depends-on`**: the ids of other services this service is **configured to reach**, such as by a URL in its
  application settings or a route pointing at it. Point downward only. The services index lists the ids. Publish and
  subscribe over a message bus is not an edge, because the publisher does not know its consumers. The graph then shows
  an event-driven service as unconnected, so list its topics and queues under `Operational notes`.
* **`interfaces`**: each interface the service offers, with its contract format and the address the contract is
  published at.
* **`owner`**: who is answerable for the service, as `human:alex.doe` or `role:head-of-engineering`. Never a team
  alias.
* **`facets`**: one exposure, then any traits that apply. These slice the catalogue, so a value earns its place by
  grouping several services (CI will warn on one that does not). [The type page](../services.md) carries the vocabulary
  and the reasoning behind it, derived from this estate, so read it before you invent a facet.
* **`tags`**: words a reader would search for that this service does not otherwise say. One service may be the only one
  carrying a tag, which is what separates a tag from a facet. Never restate another field.

**A field left out means "nothing in this catalogue", not "nothing at all".** A service that calls only a legacy system,
a third-party integration or anything else outside this catalogue writes no `depends-on`. The same applies to
`data-stores`. The field records what is here, and the body records the rest.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences: what this service is for, in terms someone unfamiliar with it would understand.

## What it does

The responsibilities this service owns. Keep it to what is true today. This is a descriptive document, and a catalogue
that disagrees with the estate is worse than none.

## Where it lives

* **Repository**: [`{{repo-name}}`]({{url}}), at `{{path within the repo, where it is not the root}}`
* **Platform**: {{runtime and framework}}
* **Deployed as**: {{web app, serverless function, CDN assets, …}}

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
