# Taxonomy

The kinds of knowledge this wiki holds, what each is for, and — more usefully — what each is *not*.

Most mistakes here are placement mistakes, not writing mistakes. Someone writes a good document and puts it in the wrong
folder, where it either duplicates something or is never found. The [decision table](#where-does-this-go) below is the
quickest route to the right answer; the [disambiguations](#disambiguations) explain the calls that are genuinely close.

## Where does this go?

The types **this corpus holds**, generated from the schema and ordered by what you are holding rather than by where it
ends up — so a corpus that has adopted five of them is offered five, and every row opens.

<!-- BEGIN GENERATED: types-placement -->

| You have…                                                            | It goes in                    |
|----------------------------------------------------------------------|-------------------------------|
| A check that proves a rule is being followed                         | [Controls](/controls)         |
| A commitment about how we engineer, at principle level               | [Policies](/policies)         |
| A decision that affects more than one repo, and its reasoning        | [ADRs](/adrs)                 |
| A description of what a deployable component is and does             | [Services](/services)         |
| A description of what we offer a customer, and why                   | [Capabilities](/capabilities) |
| A narrative of how something works or why it's shaped that way       | [Explanations](/explanations) |
| A problem with a known, confirmed fix                                | [FAQs](/faqs)                 |
| A rule people must follow when building                              | [Standards](/standards)       |
| A step-by-step for a planned task                                    | [Processes](/processes)       |
| A step-by-step for when something is broken                          | [Runbooks](/runbooks)         |
| A target for speed, uptime, or recovery                              | [NFRs](/nfrs)                 |
| A term whose meaning isn't obvious, or that we use in a specific way | [Glossary](/glossary)         |
| A third-party or external system we depend on                        | [Integrations](/integrations) |
| A tool or package we've approved, rejected, or are trialling         | [Tools](/tools)               |
| An account of an incident and what caused it                         | [Postmortems](/postmortems)   |
| Something surprising you noticed and haven't verified                | [Discoveries](/discoveries)   |
| Where data lives, how long we keep it, and how sensitive it is       | [Data](/data)                 |

<!-- END GENERATED: types-placement -->

Where you got to mid-piece-of-work is the one thing on nobody's list: session logs stay local and never reach the wiki.

If nothing fits, raise it rather than improvising. A missing type is a taxonomy conversation; a `misc/` folder is a
slow-motion failure. The framework declares more types than any one corpus stands up, so the answer may be to adopt one
rather than to invent one.

## The types

Grouped by [tier](../knowledge-as-code.md#tiers), because tier determines how each behaves, and generated from the same
schema as the table above. The fuller account of a type — what it looks like here, and the records already filed under
it — is on the type's own page.

<!-- BEGIN GENERATED: types-detail -->

### Decided — immutable once accepted

Superseded rather than rewritten, so what was thought at the time survives being wrong.

**[ADRs](/adrs)** — An architecturally significant decision affecting more than one repository, and the reasoning behind
it. The context, the choice, the alternatives weighed, the consequences. Immutable once accepted and superseded by a new
ADR rather than rewritten. A decision local to a single repository belongs in the repo that holds it, not here.

**[Postmortems](/postmortems)** — What actually happened during an incident — timeline, impact, root cause, contributing
factors, actions. Blameless, and immutable once published. The honest counterpart to the decision log: an ADR records
what was intended, a postmortem what the estate did about it.

### Normative — living, owned, reviewed

**[Controls](/controls)** — How a standard's rules are verified: the mechanism, the frequency, and the evidence it
leaves. Every control names the rules it covers. A rule no control claims is recorded as `not-enforced`, which is the
honest state and the number worth watching.

**[FAQs](/faqs)** — A problem with a confirmed fix, promoted from a discovery once a human has verified it. It carries
provenance back to the observation it came from, so the reader can see how far the fix has been taken on trust.

**[NFRs](/nfrs)** — A non-functional requirement — availability, latency, RPO, RTO — stated with how it is measured.
Capacity assumptions belong here too. An NFR with no measurement method is an aspiration, not a requirement.

**[Policies](/policies)** — A high-level engineering commitment: the what and the why, largely stack-agnostic and
changing rarely. Alignment to an external framework is stated clause by clause, as alignment rather than certification.

**[Standards](/standards)** — The rulebook — imperative, RFC 2119, with concrete examples and a conformance checklist.
Imperative throughout — **MUST**, **SHOULD**, **MAY**. Composed rather than read alone: the rules for a piece of work
are the union of the layers that apply to it.

### Descriptive — living, must mirror reality

These are the types CI can check against the estate rather than merely against themselves, which matters because they
rot faster than anything else.

**[Capabilities](/capabilities)** — What we offer a customer and why, as a hub linking to what implements, tests and
constrains it. A hub, sitting above the epic layer: it links to the work items that detail it, the services that
implement it, the feature files that test it, and the NFRs that constrain it. A capability that starts accumulating
detail of its own has stopped being one.

**[Data](/data)** — Which store owns which entities, how long they are kept, how sensitive they are, and where personal
data flows. Organised by store rather than by processing activity, which is what makes it useful to an engineer and
insufficient for a regulator.

**[Explanations](/explanations)** — Narrative that helps you understand how something works, or why it is shaped the way
it is. Architecture overviews, conceptual walkthroughs, how the pieces fit together. It links rather than restates: an
overview points at the documents holding the detail instead of repeating them. One that starts accumulating facts of its
own has become a maintenance liability.

**[Glossary](/glossary)** — The ubiquitous language — terms whose meaning is specific to us, or which are easily
confused. Small, high-value, and the one document worth loading into every session. A term that needs explaining every
time it appears belongs here once, and everything else links to it.

**[Integrations](/integrations)** — An external system we depend on: the contract, the auth, the failure modes, their
SLA and our fallback. Every integration point needs a deliberate failure mode and a fallback, which is why both are
required rather than optional, along with who to contact when it is down.

**[Services](/services)** — One deployable component: purpose, repo, platform, environments, dependencies, data stores,
owner. The anchor most other types point at. Without it, a cross-reference has nothing to resolve against.

**[Tools](/tools)** — The approved-software register — what is chosen, rejected or deprecated, and the version ranges we
stand behind. Rejections are first-class content. Knowing what was turned down, and why, saves the next person the
evaluation.

### Procedural — living, must be rehearsed

Each records when it was last rehearsed. An unrehearsed process is annoying; an unrehearsed runbook is dangerous.

**[Processes](/processes)** — A planned procedure followed deliberately — releasing, onboarding, provisioning, rotating
a secret. Written to be followed by someone who has not done it before.

**[Runbooks](/runbooks)** — An incident-time procedure read under pressure: terse, imperative, structured as a decision
tree. Disaster recovery and estate rebuild live here.

### Observed — perishable, unreviewed until promoted

The tier carrying the least authority is the one a corpus most depends on, because capture that is not free does not
happen.

**[Discoveries](/discoveries)** — Something noticed during work and not yet verified, captured cheaply and expiring
unless promoted. Deliberately low-ceremony — a title, an observation, why it might matter — and carrying a confidence
level, so that "the build fails silently if X" has somewhere to go the moment it is noticed.

<!-- END GENERATED: types-detail -->

**Session state** is the one thing with no type: where a piece of work got to, for handover between sessions, and
**not stored in this repo**. Session logs routinely contain stack traces, connection strings and customer identifiers,
so they stay local. Only distilled, reviewed discoveries reach the wiki.


## How the types relate

The edges carry as much value as the nodes, and they are the part that breaks silently. CI validates that every
reference resolves to a document that exists and is not superseded.

```
Policy <──implements── Standard ──verified-by──> Control ──applies-to──> Service
   │                      │                                                ▲
 clause                   └──derived-from──> ADR                           │
   │                                          ▲                            │
   └──aligns-with──> Framework                │                            │
                     (frameworks.md)     prompted-by                       │
                                          Postmortem                       │
Capability ──implemented-by────────────────────────────────────────────────┘
    │
    ├──detailed-by──────> ADO epics & features
    ├──tested-by────────> feature files
    └──constrained-by───> NFR

Discovery ──promoted-to──> FAQ ──relates-to──> Service | Capability
```

Reciprocal pairs must agree in both directions: `supersedes`/`superseded-by`, `verifies`/`verified-by`,
`promoted-from`/`promoted-to`. A one-sided link fails the build.

Not every edge is a pair. A standard's `implements` points up at a policy and is never answered from the policy side:
policies are the layer a downstream corpus inherits, standards the layer it writes for itself, so what implements a
policy is not knowable from where the policy sits.

Nor does every edge leave from a whole document. A policy aligns with a framework through a single **clause** rather
than in its entirety, so the edge leaves the clause table and lands on a control — `pol-SCRT.KEYS` to Annex A A.8.24.
[Frameworks](/frameworks.md) is the far end of every one of those edges, and the only place our standing against a
framework is recorded.

## Layout

Each type follows the same shape:

```
<type>.md              # what it is, why, how to contribute — human-written
<type>/
  ├── _index.md        # index — GENERATED
  ├── _template.md     # what humans and agents copy
  └── <records>.md
```

`_` is reserved. A leading underscore means the framework's own artefact rather than a knowledge record — the generated
index and the template inside a type folder, the scaffolding directories alongside them. The tool reads the prefix, not
the names, so anything under it is excluded from discovery and never validated as a record. A record must therefore not
take it. The prefix also sorts ahead of letters whether or not a listing folds case, which is what keeps the framework's
files together at the top of a folder someone is scanning for content.

Alongside the types:

```
README.md              # orientation
CLAUDE.md              # agent guidance, with the rules digest generated into it
glossary.md
frameworks.md          # external frameworks, and what each obliges us to
knowledge-as-code.md   # the approach
knowledge-as-code/     # the system's own documentation — outside the taxonomy
  manifest.yaml        # which files are shared and which are local
.mechanism.lock        # this corpus's sync state
.claude/skills/        # agent machinery — SYNCED
.schema/               # the machine-readable schema — SYNCED
.tooling/              # validators and generators — SYNCED
_plan/                 # migration scaffolding — temporary
_reports/              # GENERATED
```

## Disambiguations

The calls that are actually close.

**ADR vs Standard.** The ADR is the decision and its reasoning, frozen. The standard is the rule that results, kept
current. If you're writing "we considered X and rejected it", that's an ADR. If you're writing "you **MUST** do Y",
that's a standard. Most substantial changes produce both.

**Policy vs Standard.** A policy is true regardless of stack, framework or year — "we do not store secrets in source
control". A standard is specific enough to check — "use Key Vault via workload identity". If it would still be true
after replacing the entire technology estate, it's a policy.

**Standard vs Control.** The standard says what to do; the control says how we know it happened. `Secrets MUST come
from Key Vault` is a standard. `CI runs gitleaks on every PR` is a control. If it can fail a build, it's a control.

**Capability vs Spec.** A *capability* is the product surface — Billing, Search, Notifications — described once, above
the epic layer, as a hub of links. A *spec* is the per-feature application of standards to a concrete contract, and it
belongs in the repo that owns the feature, next to the OpenAPI document and the feature files it describes. This follows
the same central-vs-local rule as ADRs: cross-repo synthesis lives here, feature-level detail lives with the code.

**Capability vs Service.** A capability is what a customer gets. A service is a thing we deploy. One capability
typically spans several services; one service often contributes to several capabilities.

**Discovery vs FAQ.** A discovery is unverified and might be wrong or already fixed. An FAQ has been confirmed by a
human and carries authority. Never write straight to FAQ from a session — capture as a discovery and let promotion do
the work.

**Process vs Runbook.** Are you doing this because you planned to, or because something is broken? Planned is a process.
Broken is a runbook.

**Tools vs ADR.** Adopting a tool is often a decision worth an ADR *and* an entry in the register. The ADR carries the
reasoning; the register carries the current state and version range. Small, uncontroversial adoptions need only the
register.

**Glossary vs everything.** If a term needs explaining every time it appears, it belongs in the glossary — once — and
everything else links to it.

## Status of this taxonomy

Not all types are proven. Which are and which are drafts is recorded once, in the root
[README](../README.md#maturity).

Changing the taxonomy — adding a type, merging two, moving a type between tiers — is a larger act than editing any
document within it, and should be recorded as an ADR superseding or amending
[adr-0001](/adrs/0001-knowledge-as-code.md).
