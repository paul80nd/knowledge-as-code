# Taxonomy

The kinds of knowledge this wiki holds, what each is for, and — more usefully — what each is *not*.

Most mistakes here are placement mistakes, not writing mistakes. Someone writes a good document and puts it in the wrong
folder, where it either duplicates something or is never found. The [decision table](#where-does-this-go) below is the
quickest route to the right answer; the [disambiguations](#disambiguations) explain the calls that are genuinely close.

## Where does this go?

| You have…                                                            | It goes in                      |
|----------------------------------------------------------------------|---------------------------------|
| A decision that affects more than one repo, and its reasoning        | [ADRs](/adrs)                   |
| A rule people must follow when building                              | [Standards](/standards)         |
| A commitment about how we engineer, at principle level               | [Policies](/policies)           |
| A check that proves a rule is being followed                         | [Controls](/controls)           |
| A target for speed, uptime, or recovery                              | [NFRs](/nfrs)                   |
| A step-by-step for a planned task                                    | [Processes](/processes)         |
| A step-by-step for when something is broken                          | [Runbooks](/runbooks)           |
| A narrative of how something works or why it's shaped that way       | [Explanations](/explanations)   |
| A description of what a deployable component is and does             | [Services](/services)           |
| A description of what we offer a customer, and why                   | [Capabilities](/capabilities)   |
| A tool or package we've approved, rejected, or are trialling         | [Tools](/tools)                 |
| A third-party or external system we depend on                        | [Integrations](/integrations)   |
| Where data lives, how long we keep it, and how sensitive it is       | [Data](/data)                   |
| A term whose meaning isn't obvious, or that we use in a specific way | [Glossary](/glossary)           |
| A problem with a known, confirmed fix                                | [FAQs](/faqs)                   |
| Something surprising you noticed and haven't verified                | [Discoveries](/discoveries)     |
| An account of an incident and what caused it                         | [Postmortems](/postmortems)     |
| Where you got to, mid-piece-of-work                                  | Session state (local, not here) |

If nothing fits, raise it rather than improvising. A missing type is a taxonomy conversation; a `misc/` folder is a
slow-motion failure.

## The types

Grouped by [tier](../knowledge-as-code.md#tiers), because tier determines how each behaves.

### Decided — immutable once accepted

**[ADRs](/adrs)** — an architecturally significant decision affecting more than one repository: the context, the
choice, the alternatives weighed, the consequences. Immutable once accepted; superseded by a new ADR rather than
rewritten. Decisions local to a single repo belong in that repo, not here.

**[Postmortems](/postmortems)** — what actually happened during an incident: timeline, impact, root cause, contributing
factors, actions. Blameless. Immutable once published. The honest counterpart to the ADR log — ADRs record what we
intended, postmortems record what the estate did about it.

### Normative — living, owned, reviewed

**[Policies](/policies)** — a high-level engineering commitment. The *what* and *why*, largely stack-agnostic, changing
rarely. Aligned to ISO/IEC 27001:2022 Annex A areas where relevant, as alignment rather than certification.

**[Standards](/standards)** — the rulebook. Imperative, RFC 2119 (**MUST** / **SHOULD** / **MAY**), with concrete
examples and a conformance checklist. Organised along three axes — common, platform, interface/domain — and composed:
the rules for a piece of work are the union of the layers that apply to it.

**[Controls](/controls)** — how a standard's rules are actually verified: the mechanism, the frequency, the evidence.
Every control names the rules it covers. A rule with no control is recorded as `not-enforced`, which is the honest state
and the number worth watching.

**[NFRs](/nfrs)** — non-functional requirements: availability, latency budgets, RPO, RTO, capacity assumptions. Each
states how it is measured. An NFR with no measurement method is an aspiration, not a requirement.

**[FAQs](/faqs)** — a problem with a confirmed fix. Promoted from a [discovery](/discoveries) once a human has verified
it, and carrying provenance back to the observation it came from.

### Descriptive — living, must mirror reality

These are the types CI can check against the estate rather than merely against themselves, which matters because they
rot faster than anything else.

**[Service](/services)** — one document per deployable component: purpose, repo, platform, environments, dependencies,
data stores, criticality, owner. This is the **anchor** most other types point at; without it, cross-references can't be
validated.

**[Capability](/capabilities)** — what we offer and why, at the level above ADO epics. A *hub* document: it links to
the epics that detail it, the services that implement it, the feature files that test it, and the NFRs that constrain
it. It does not restate them.

**[Tools](/tools)** — the approved-software register: what we've chosen, what we've rejected, what's deprecated, and the
version ranges we stand behind.

**[Integration](/integrations)** — an external system we depend on: the contract, auth, failure modes, their SLA, our
fallback, who to contact.

**[Data](/data)** — which store owns which entities, retention periods, classification, and where personal data flows.

**[Glossary](/glossary)** — the ubiquitous language. Terms whose meaning is specific to us, or which are easily
confused with each other. Small, high-value, and the one document worth loading into every session.

**[Explanations](/explanations)** — narrative that helps you *understand* how something works or why it is shaped the way
it is. Architecture overviews, conceptual walkthroughs, "how the pieces fit together". Descriptive tier: it must mirror
reality, and it carries an owner and a review date like anything else.

An explanation is **not**:

* normative — if it says what you must do, it's a [standard](/standards)
* procedural — if it says how to perform a task, it's a [process](/processes)
* a catalogue entry — if it describes one component, it's a [service](/services)
* a decision — if it records what was chosen and why, it's an [ADR](/adrs)

Explanations **link rather than restate**. An architecture overview points at the services, capabilities and ADRs that
hold the detail; it does not duplicate them. An explanation that starts accumulating facts of its own has become a
maintenance liability.

### Procedural — living, must be rehearsed

**[Process](/processes)** — a planned procedure followed deliberately: releasing, onboarding, provisioning, rotation.

**[Runbook](/runbooks)** — an incident-time procedure, read under pressure. Terse and imperative, structured as a
decision tree. Disaster recovery and estate rebuild live here.

Both record when they were last rehearsed. An unrehearsed process is annoying; an unrehearsed runbook is dangerous.

### Observed — perishable, unreviewed until promoted

**[Discovery](/discoveries)** — something noticed during work and not yet verified. "The build fails silently if X."
Deliberately low-ceremony: a title, an observation, why it might matter. Carries a confidence level and expires by
default if nothing promotes it.

**Session state** — where a piece of work got to, for handover between sessions. **Not stored in this repo.** Session
logs routinely contain stack traces, connection strings and customer identifiers; they stay local. Only distilled,
reviewed discoveries reach the wiki.

## How the types relate

The edges carry as much value as the nodes, and they are the part that breaks silently. CI validates that every
reference resolves to a document that exists and is not superseded.

```
Policy ──implemented-by──> Standard ──verified-by──> Control ──applies-to──> Service
   │                          │                                                ▲
   │                          └──derived-from──> ADR                           │
   │                                              ▲                            │
   └──aligns-with──> ISO/IEC 27001:2022           │                            │
                     Annex A                 prompted-by                       │
                                              Postmortem                       │
Capability ──implemented-by────────────────────────────────────────────────────┘
    │
    ├──detailed-by──────> ADO epics & features
    ├──tested-by────────> feature files
    └──constrained-by───> NFR

Discovery ──promoted-to──> FAQ ──relates-to──> Service | Capability
```

Reciprocal pairs must agree in both directions: `supersedes`/`superseded-by`,
`implements`/`implemented-by`, `promoted-from`/`promoted-to`. A one-sided link fails the build.

## Layout

Each type follows the same shape:

```
<type>.md              # what it is, why, how to contribute — human-written
<type>/
  ├── INDEX.md        # index — GENERATED
  ├── template.md      # what humans and agents copy
  └── <records>.md
```

Alongside the types:

```
README.md              # orientation
CLAUDE.md              # rules digest — GENERATED
glossary.md
knowledge-as-code.md   # the approach
knowledge-as-code/     # the system's own documentation — outside the taxonomy
  manifest.yaml        # which files are shared and which are local
  mechanism.lock       # this corpus's sync state
.claude/skills/        # agent machinery — SYNCED
.tooling/                   # validators and generators — SYNCED
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

**Capability vs Spec.** A *capability* is the product surface — Billing, Search, Notifications — described once,
above the epic layer, as a hub of links. A *spec* is the per-feature application of standards to a concrete contract,
and it belongs in the repo that owns the feature, next to the OpenAPI document and the feature files it describes. This
follows the same central-vs-local rule as ADRs: cross-repo synthesis lives here, feature-level detail lives with the
code.

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

Not all types exist yet. The current state and sequencing live in `_plan/backlog.md`, which is temporary scaffolding and
will be deleted once the migration is complete.

Changing the taxonomy — adding a type, merging two, moving a type between tiers — is a larger act than editing any
document within it, and should be recorded as an ADR superseding or amending
[ADR-0001](/adrs/0001-knowledge-as-code.md).
