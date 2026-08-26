# The default types

Seventeen knowledge types ship with the framework. A corpus adopts the ones it needs by naming them in `types:` in
[`.corpus.yaml`](../corpus-descriptor.md), and a declined type's schema file is never written, so nothing arrives to be
ignored.

 A corpus running one product might stand up four of these and never want the rest, and a corpus
can declare a type of its own that the framework has never heard of. What follows is what you get without deciding
anything.

They are grouped by [tier](taxonomy.md#the-five-tiers), because tier is what sets the rules each one answers to.

## Decided

Immutable once accepted. Superseded, never rewritten.

| Type            | Folder         | What it holds                                                                                           |
|-----------------|----------------|---------------------------------------------------------------------------------------------------------|
| **ADRs**        | `adrs/`        | An architecturally significant decision affecting more than one repository, and the reasoning behind it |
| **Postmortems** | `postmortems/` | What actually happened during an incident: timeline, impact, root cause, contributing factors, actions  |

## Normative

Living and owned. Edited in place, with a changelog.

| Type          | Folder       | What it holds                                                                                         |
|---------------|--------------|-------------------------------------------------------------------------------------------------------|
| **Policies**  | `policies/`  | A high-level engineering commitment: the what and the why, largely stack-agnostic and changing rarely |
| **Standards** | `standards/` | The rulebook, imperative, RFC 2119, with concrete examples and a conformance checklist                |
| **Controls**  | `controls/`  | How a standard's rules are verified: the mechanism, the frequency, and the evidence it leaves         |
| **NFRs**      | `nfrs/`      | A non-functional requirement (availability, latency, RPO, RTO) stated with how it is measured         |
| **FAQs**      | `faqs/`      | A problem with a confirmed fix, promoted from a discovery once a human has verified it                |

## Descriptive

Living, and must mirror reality. Verifiable against the estate.

| Type             | Folder          | What it holds                                                                                                   |
|------------------|-----------------|-----------------------------------------------------------------------------------------------------------------|
| **Capabilities** | `capabilities/` | What you offer a customer and why, as a hub linking to what implements, tests and constrains it                 |
| **Services**     | `services/`     | One deployable component: purpose, repo, platform, environments, dependencies, data stores, owner               |
| **Integrations** | `integrations/` | An external system you depend on: the contract, the auth, the failure modes, their SLA and your fallback        |
| **Data**         | `data/`         | Which service owns which data, how long it is kept, how sensitive it is, and where personal data flows          |
| **Tools**        | `tools/`        | The approved-software register: what is chosen, rejected or deprecated, and the version ranges you stand behind |
| **Glossaries**   | `glossary/`     | The ubiquitous language. Terms whose meaning is specific to you, or which are easily confused                   |
| **Explanations** | `explanations/` | Narrative that helps you understand how something works, or why it is shaped the way it is                      |

## Procedural

Living, and must be rehearsed to stay true.

| Type          | Folder       | What it holds                                                                                     |
|---------------|--------------|---------------------------------------------------------------------------------------------------|
| **Processes** | `processes/` | A planned procedure followed deliberately: releasing, onboarding, provisioning, rotating a secret |
| **Runbooks**  | `runbooks/`  | An incident-time procedure read under pressure: terse, imperative, structured as a decision tree  |

## Observed

Perishable. Unreviewed until promoted, and it expires by default.

| Type            | Folder         | What it holds                                                                                     |
|-----------------|----------------|---------------------------------------------------------------------------------------------------|
| **Discoveries** | `discoveries/` | Something noticed during work and not yet verified, captured cheaply and expiring unless promoted |

## The pairs that look alike

Several of these sit close enough to be confused, and the schema declares the distinction on the type it belongs to. An
ADR is the decision and its reasoning, frozen; a standard is the rule that results, kept current. A process is read at a
desk; a runbook is read at three in the morning. A discovery is cheap and unverified; an FAQ is what it becomes once
somebody confirms it.

A corpus's own taxonomy page renders every such pair it holds both sides of, so the distinctions you meet are the ones
your own types actually raise.

## Adding one of your own

A type is a YAML file in `.schema/` declaring its fields, its tier and its rules. Nothing in `kac` changes, and nothing
upstream has to agree. [Taxonomy](taxonomy.md#changing-the-taxonomy) says what that costs and where the decision
belongs.
