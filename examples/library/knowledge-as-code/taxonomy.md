# Taxonomy

> The kinds of knowledge this corpus holds, what each is for, and what each is not.

The [decision table](#where-does-this-go) below is the quickest route to the right answer, and the
[disambiguations](#disambiguations) explain the calls that are genuinely close. Both cover the types this corpus adopted
and no others.

[Taxonomy][taxonomy] states what is true of every corpus: what a tier is and what the four ask, the shape a type takes
on disk, and what changing a taxonomy costs.

## Where does this go?

Scan the left column for what you are holding.

<!-- BEGIN GENERATED: types-placement -->

| You have…                                                        | It goes in                         |
|------------------------------------------------------------------|------------------------------------|
| A decision affecting more than one repository, and its reasoning | [ADRs](../adrs.md)                 |
| A step-by-step for a planned task                                | [Processes](../processes.md)       |
| A step-by-step for when something is broken                      | [Runbooks](../runbooks.md)         |
| A term whose meaning is local, or not obvious                    | [Glossaries](../glossary.md)       |
| A third-party or external system the estate depends on           | [Integrations](../integrations.md) |
| How something works, or why it is shaped that way                | [Explanations](../explanations.md) |
| What a deployable component is and does                          | [Services](../services.md)         |
| What the organisation offers a customer, and why                 | [Offerings](../offerings.md)       |
| Where data lives, how long it is kept, and its sensitivity       | [Data](../data.md)                 |

<!-- END GENERATED: types-placement -->

If nothing fits, raise it. A missing type is a taxonomy conversation, and the answer is sometimes a type this corpus has
not adopted.

## The types

The types are grouped by [tier][tiers]. Each type's own page lists the records filed under it.

<!-- BEGIN GENERATED: types-detail -->

### Decided: immutable once accepted, except to correct it

A decided record is about one moment: a decision, or an incident. Once accepted, it changes only to correct what it
misstates about that moment, or to fix its prose and spelling. It never follows what changed afterwards, and a changed
decision is a new record that supersedes it. A decided record states the constraints it asserts. It cites a descriptive
record for background, and never as the authority behind a constraint.

**[ADRs](../adrs.md).** An architecturally significant decision affecting more than one repository, and the reasoning
behind it. The context, the choice, the alternatives weighed, and the consequences. An accepted ADR is immutable, so a
later ADR supersedes it. A decision that affects only one repository belongs in that repository.

### Descriptive: living, must mirror reality

CI can check these against the estate itself. They also fall out of date fastest.

**[Data](../data.md).** Which service owns which data, how long it is kept, how sensitive it is, and where personal data
flows. One document per data domain, written for an engineer. It records the entities in the domain, the store they live
in, the service that owns them, how sensitive they are, and how long they are kept.

**[Explanations](../explanations.md).** Narrative that helps you understand how something works, or why it is shaped the
way it is. Architecture overviews, conceptual walkthroughs, and how the pieces fit together. An explanation points at
the documents with the detail and does not repeat it. One that accumulates facts of its own falls out of date as soon as
those facts move.

**[Glossaries](../glossary.md).** The ubiquitous language. Terms with a meaning specific to the organisation, or easily
confused with another. One glossary per bounded context, each small enough to read end to end. A term that needs
explaining every time it appears belongs in the most general glossary that admits it, and the narrower glossaries link
to that entry.

**[Integrations](../integrations.md).** An external system the estate depends on: its contract, auth, failure modes, SLA
and fallback. One document per external system. It records the contract, how a caller authenticates, what happens when
the system is down, and who to call about it.

**[Offerings](../offerings.md).** What the organisation offers a customer, and why, with links to the services and NFRs
behind it. An offering sits above the work items. It links to the ones that detail it, the services that implement it,
the feature files that test it, and the NFRs that constrain it. An offering that accumulates detail of its own has
stopped being one.

**[Services](../services.md).** One deployable component: purpose, repos, platform, environments, dependencies, data
stores, owner. The record most other types point at. Without it, a cross-reference has nothing to resolve against.

### Procedural: living, must be rehearsed

Each records when it was last rehearsed. An unrehearsed process is annoying. An unrehearsed runbook is dangerous.

**[Processes](../processes.md).** A planned procedure (releasing, onboarding, provisioning, rotating a secret). Write
each one for somebody who has not done it before.

**[Runbooks](../runbooks.md).** An incident-time procedure read under pressure: terse, imperative, structured as a
decision tree. Disaster recovery and estate rebuild are runbooks.

<!-- END GENERATED: types-detail -->

## How the types relate

Every edge below is a cross-reference field the schema declares, so CI checks that it resolves to a document that
exists.

<!-- BEGIN GENERATED: types-graph -->

```mermaid
graph LR;
  t_adrs[ADR];
  t_data[Data];
  t_explanations[Explanation];
  t_glossary[Glossary];
  t_integrations[Integration];
  t_offerings[Offering];
  t_processes[Process];
  t_runbooks[Runbook];
  t_services[Service];
  t_adrs -- related --> t_adrs;
  t_adrs -- superseded-by / supersedes --> t_adrs;
  t_data -- flows-to --> t_services;
  t_data -- flows-to --> t_integrations;
  t_data -- owned-by --> t_services;
  t_explanations -- explains --> t_services;
  t_explanations -- explains --> t_offerings;
  t_explanations -- explains --> t_processes;
  t_explanations -- explains --> t_adrs;
  t_glossary -- narrows --> t_glossary;
  t_integrations -- replaces / successor --> t_integrations;
  t_integrations -- used-by --> t_services;
  t_offerings -- implemented-by --> t_services;
  t_processes -- applies-to --> t_services;
  t_runbooks -- applies-to --> t_services;
  t_services -- data-stores --> t_data;
  t_services -- depends-on --> t_services;
```

<!-- END GENERATED: types-graph -->

An edge labelled with two fields, such as `verifies / verified-by`, is a reciprocal pair. A pair written on one side
only fails the build. An edge with one field has no counterpart to keep in step.

Not every edge is a pair. A standard's `implements` points up at a policy, and the policy never points back. A
downstream corpus inherits both the policies and the standards above it. It also writes standards of its own, so
nobody sitting at the policy can know what implements it. An inherited standard stays as its own corpus wrote it, and
no corpus below rewrites it.

Nor does every edge leave from a whole document. A policy aligns with a framework through a single **clause** rather
than in its entirety, so the edge leaves the clause table and lands on a control: `pol-SCRT.KEYS` to Annex A A.8.24.
[Frameworks](../frameworks.md) is the far end of every one of those edges, and the only page that records this
corpus's standing against a framework. It has no `ref:`, so the graph above does not draw it.

## Disambiguations

The calls that are actually close. Each is written once, on the type its heading names first, and appears only where
this corpus holds both sides of it.

<!-- BEGIN GENERATED: types-versus -->

**Explanation vs ADR.** An explanation describes the shape something has. An ADR records the choice that gave it that
shape, and is frozen at the moment of choosing.

**Explanation vs Process.** An explanation says how something works. A process says how to perform a task. If a reader
is meant to follow it step by step, it is a process.

**Explanation vs Service.** An explanation covers how the pieces fit together. A service document describes one
deployable component. If it is about a single component, it is a service.

**Offering vs Service.** An offering is what a customer gets. A service is something the organisation deploys. One
offering usually spans several services. One service usually contributes to several offerings.

**Process vs Runbook.** Are you doing this because you planned to, or because something is broken? Planned is a process.
Broken is a runbook.

<!-- END GENERATED: types-versus -->

[taxonomy]: https://paul80nd.github.io/knowledge-as-code/framework/taxonomy/
[tiers]: https://paul80nd.github.io/knowledge-as-code/framework/taxonomy/#the-four-tiers
