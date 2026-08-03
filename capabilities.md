# Capabilities

What the product offers its customers, and why.

**[→ Index](capabilities/INDEX.md)**

## What is a capability?

A single document per product surface — one per customer-visible area of the product — describing what it does, why it
exists, which services implement it, and where the detail lives.

A capability is a **hub**. It links to the epics that specify it, the feature files that test it, the services that
implement it and the NFRs that constrain it. It does not restate any of them.

## Why we use them

Functional detail lives in ADO epics, features and stories. What ADO cannot hold — without disturbing a work-item
hierarchy that exists for delivery, not documentation — is the layer *above* the epic: the holistic account of what the
product actually offers and why.

That gap is why nobody can answer "what does the product do?" from a single place today, and why the same context gets
reconstructed at the start of every significant piece of work.

## Scope

One document per **customer-visible surface**, not per epic and not per service. One capability typically spans several
services; one service often contributes to several capabilities.

**Capabilities link rather than restate.** The moment a capability document starts specifying behaviour, it has begun to
drift from the ADO items it should be pointing at — and a drifted capability is worse than none, because sessions will
trust it. If you are writing acceptance criteria, they belong in ADO.

Related but different:

* **Spec** — the per-feature application of standards to a concrete contract lives in the repository that owns the
  feature, alongside its OpenAPI document and feature files. Same central-vs-local rule as [ADRs](/adrs): cross-repo
  synthesis lives here, feature-level detail lives with the code.
* **[Service](/services)** — a thing we deploy. A capability is a thing a customer gets.
* **[Explanation](/explanations)** — how something works internally. A capability is what it does externally.

## Metadata

<!-- BEGIN GENERATED: schema-capabilities -->

| Field            | Req | Type | Notes                                          |
|------------------|-----|------|------------------------------------------------|
| `status`         | ●   | enum | `planned` · `building` · `live` · `deprecated` |
| `implemented-by` | ●   | list | Service ids                                    |
| `ado-epics`      |     | list | Work item ids                                  |
| `feature-files`  |     | list | Repo-relative paths — CI checks they exist     |
| `nfrs`           |     | list | NFR ids                                        |

<!-- END GENERATED: schema-capabilities -->

## Adding a Capability

Capabilities are **hubs**. They link to epics, services, feature files and NFRs; they do not restate them. If a
capability document starts to read like a specification, it has begun to drift from the ADO items it should be pointing
at — and a drifted capability document is worse than none, because sessions will trust it.

## Adding a capability

1. Copy [`template.md`](capabilities/template.md) to `<slug>.md`. Capabilities use slug ids — `cap-<name>`.
2. Write the *what* and the *why* in prose. Two or three paragraphs is usually enough.
3. Fill in `implemented-by`, `ado-epics` and `feature-files` — these are the links that make it a hub.
4. Resist the urge to explain how it works. Link to the services and explanations that already do.

**Conventions**

* **Hub, not specification.** If a section is longer than the list of links around it, ask whether it belongs in ADO.
* **Every feature file path is checked** by CI, in both directions — a path that doesn't exist fails, and a feature file
  claimed by no capability is reported.

## What CI checks

<!-- BEGIN GENERATED: checks-capabilities -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-capabilities -->
