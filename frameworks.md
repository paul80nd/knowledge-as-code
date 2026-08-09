# Frameworks

The external frameworks this corpus refers to, and what each one obliges us to.

A [policy](/policies) maps its clauses to a framework's controls in the `Alignment` column of its clause table, as
`[ISO 27001:2022].A.8.24`. Those references resolve here. This page is the only place that says what the relationship
is — bound, self-held, or merely borrowed — because that standing changes on its own schedule and would otherwise need
correcting in twenty policies at once.

Maintained by hand: no frontmatter, no id, no index. It exists so the references in the corpus have somewhere honest to
land.

## The three postures

| Posture            | What it means                                                                                                                     |
|--------------------|-----------------------------------------------------------------------------------------------------------------------------------|
| **Obliged**        | Something outside us requires it — certification we hold, or law that applies to us. Not optional.                                |
| **Self-obligated** | Nothing external compels it; a policy of ours does. Binding on us because we said so, and revocable only by changing that policy. |
| **Inspiration**    | We took ideas from it. It shapes our thinking and binds nothing. A clause may cite it for provenance, not for obligation.         |

"Because we are certified against it" and "because it seemed sensible" are different answers to *why does this clause
exist*, and only one of them survives a change of mind.

## Obliged

### ISO/IEC 27001:2022 {#iso27001-2022}

**Registered.** We hold certification and are audited against it. Annex A is the control set our security, delivery and
operations policies map to, and that mapping is the evidence trail an auditor follows from a control back to the
commitment implementing it.

Registration is why the mapping has to be honest rather than generous. A clause mapped to a control it does not really
implement is worse than no mapping at all — the gap is now hidden behind a reference that looks like coverage. Where
nothing genuinely maps, the control is absent from the corpus and that absence is the finding.

**What this corpus is responsible for.** Annex A has 93 controls and the Statement of Applicability covers all of them;
this corpus is the engineering function's share and no more. Roughly a third of Annex A is deliberately answered
elsewhere in the management system:

| Area                                                      | Owned by                                  |
|-----------------------------------------------------------|-------------------------------------------|
| Physical controls — `A.7.1`–`A.7.14` entire               | Facilities                                |
| People controls — `A.6.1`–`A.6.7`                         | HR, with the security awareness programme |
| The management system itself — `A.5.1`, `A.5.2`, `A.5.35` | The ISMS owner                            |
| Organisational reach — `A.5.5`, `A.5.6`, `A.5.31`         | Legal and the ISMS owner                  |
| Asset handling — `A.5.10`, `A.5.11`, `A.5.13`             | IT operations                             |
| Supplier contracting — `A.5.20`                           | Procurement and legal                     |
| Corporate IT — `A.8.1`, `A.8.23`, `A.8.34`                | IT operations                             |

Without saying so, a reader who takes the rule above at face value counts every uncited control as a finding, and most
of them are not ours to answer. The ones that *are* ours and still uncited are the real ones, and they are worth naming
as gaps rather than leaving in a crowd of controls that were never in scope.

Referenced by: most policies under `category: security`, `delivery` and `operations`.

### UK GDPR and the Data Protection Act 2018 {#uk-gdpr}

**Obliged in law**, wherever we process personal data. The EU GDPR applies in parallel for people in the EU and shares
the article numbering, so a clause citing `Art.5(1)(e)` cites both; only the supervisory authority differs.

This is where [pol-DATA]'s obligations originate. ISO/IEC 27001:2022 covers the same ground in a single control —
A.5.34, *privacy and protection of personal data* — but a control that says "comply with the applicable law" is a
pointer, not the duty. The practical difference is who can waive it: an Annex A control is ours to scope out with a
documented justification, and an article is not. A recorded deviation ([pol-DEVI]) against one of these clauses is a
decision to break the law rather than a risk to accept.

Referenced by: [pol-DATA], [pol-DERV].

### Public Sector Bodies Accessibility Regulations 2018 {#psbar-2018}

**Obliged in law**, in the markets we serve. Sets the accessibility duty on public sector websites and mobile
applications, including the requirement to publish and maintain an accessibility statement.

Referenced by: [pol-A11Y].

### EN 301 549 {#en-301-549}

**Obliged in law**, as the harmonised European standard the accessibility regulations point at. It is the technical
expression of the duty [PSBAR 2018] creates, and it incorporates [WCAG 2.2 AA] by reference for web content.

Referenced by: [pol-A11Y].

## Self-obligated

### WCAG 2.2 AA {#wcag-22-aa}

**Self-obligated**, under [pol-A11Y]. We target level AA because the policy says we do, not because a particular market
has yet required that version of it. Where law obliges an older or narrower target, this is the higher bar and the one
we hold.

Referenced by: [pol-A11Y].

## Inspiration

Nothing here binds. A clause cites one of these to say where the thinking came from — that we are restating what the
industry already knows rather than inventing it. Citing one is never an argument for keeping a clause we would otherwise
drop.

### DORA metrics {#dora-metrics}

**Inspiration.** The four delivery-performance measures from the DevOps Research and Assessment programme: deployment
frequency, lead time for changes, change failure rate, and time to recover from a failed deployment.

These are outcomes rather than practices, which makes them an unusual thing to align a clause with. What we borrow is
the causal direction: certain capabilities move certain measures, so a clause citing `lead-time` is claiming to be one
of the things that moves it — a testable claim about our own delivery rather than a control we satisfy.

Not the **Digital Operational Resilience Act**, the EU regulation for financial entities, which shares the acronym and
nothing else. If that ever applies to us it goes under Obliged, under its full name.

Referenced by: [pol-PIPE], [pol-AUTV].

### Azure Well-Architected Framework {#azure-waf}

**Inspiration.** Microsoft's five pillars for designing and operating a workload: Reliability, Security, Cost
Optimization, Operational Excellence and Performance Efficiency. Vendor-published rather than standards-body, and
Azure's because Azure is what we run on — AWS and Google publish near-identical pillars, so a move would be a
relabelling rather than a re-mapping.

Clauses cite the **pillar**, not the individual checklist recommendation: recommendations are renumbered as the
framework is revised, and a citation that rots quietly is worse than one that is a little coarse.

This is the only framework covering [pol-COST], and worth being plain about why. No external body will ever oblige us to
manage our own cloud spend. Recording the Cost Optimization pillar as a real influence is the difference between a
policy that borrows from established practice and one that appears to have been invented in a meeting.

Referenced by: [pol-COST], [pol-RECV], [pol-PERF], [pol-OBSV], [pol-DERV].

### NIST SSDF 1.1 {#nist-ssdf-1-1}

**Inspiration.** The Secure Software Development Framework, NIST SP 800-218 — around forty practices grouped as `PO`
prepare the organisation, `PS` protect the software, `PW` produce well-secured software, and `RV` respond to
vulnerabilities. Cited as `PO.5`, `PW.7`, `RV.2`. Free, and not certifiable: there is no registering against SSDF.

The closest thing to a peer this corpus has. SSDF says what a practice must achieve and leaves the tooling to whoever
adopts it, which puts it at the same altitude as a policy clause and is why it reaches seven policies rather than one.
Several of our clauses turn out to be SSDF practices arrived at independently — [pol-ENVS] is `PO.5` nearly in its
entirety — and that cuts both ways: the corpus is conventional where it should be, and where it says something SSDF does
not is where it is worth defending.

NIST also publishes SP 800-218A, an SSDF profile for generative AI. It addresses producers of models rather than
consumers of coding agents, so it does not cover what [pol-AGNT] governs; the AI RMF below does.

Referenced by: [pol-SECD], [pol-AUTV], [pol-TRUS], [pol-VURM], [pol-EVER], [pol-PIPE], [pol-ENVS].

### NIST AI RMF 1.0 {#nist-ai-rmf-1-0}

**Inspiration.** The AI Risk Management Framework, NIST AI 100-1 — four functions: `GOVERN` (accountability and
oversight), `MAP` (context and provenance), `MEASURE` (evaluation and verification) and `MANAGE` (risk treatment). Cited
at function level, for the same reason the Well-Architected pillars are.

[pol-AGNT] is the policy this exists for. It previously had only `A.8.30`, *outsourced development* — a control written
for contracting out a system build, pressed into service for an agent proposing a change. It reads plausibly and is
wrong in the way that matters: an outsourced supplier is accountable for its work, and the premise of pol-AGNT is that
an agent is not, so accountability sits with the person who accepts the output. GOVERN says that directly.

**ISO/IEC 42001** is the certifiable sibling, an AI management system standard and the natural companion to our 27001
registration if the use of AI ever needs auditing rather than governing. It moves to Obliged the day we register and not
before — the same relationship SSDF has to 27001: the free framework describes the practice, the certifiable standard
makes someone check.

Referenced by: [pol-AGNT].

### OWASP ASVS 4.0 {#owasp-asvs-4}

**Inspiration.** The Open Worldwide Application Security Project's Application Security Verification Standard —
application security requirements in chapters (`V1` architecture and threat modelling, `V2` authentication, `V4` access
control, `V5` validation and encoding, `V13` APIs and web services), each at three levels of rigour.

The version is pinned deliberately: ASVS re-chapters between major versions, so `V13` means something specific only
alongside the version it was written against. Moving to a later one is a re-mapping exercise, and doing it as an edit to
this heading would silently repoint every citation.

Referenced by: [pol-INTC], [pol-SECD], [pol-ACCS].

## Adding a framework

1. Decide the posture first. If you cannot say which of the three it is, the corpus is not ready to reference it.
2. Add a heading under that posture with an explicit anchor — `{#iso27001-2022}`, the version folded in with a hyphen
   rather than a colon, since `:` scopes ids inside the corpus.
3. Say what it is, what it obliges, and what changed if the posture is new. A framework we have just become obliged by
   reads differently from one we have been certified against for years.
4. In the citing policy, define the link at the foot — `[ISO 27001:2022]: /frameworks.md#iso27001-2022` — below the
   corpus references, and cite it per clause rather than per document.

A framework nothing references does not belong here. If we stop using one, say so and when, rather than deleting the
entry: a policy clause written under its influence is easier to read with the history intact.

[pol-A11Y]: policies/a11y-accessibility.md
[pol-ACCS]: policies/accs-access-by-identity.md
[pol-AGNT]: policies/agnt-agents-propose-people-decide.md
[pol-AUTV]: policies/autv-automated-verification.md
[pol-COST]: policies/cost-cost-as-an-nfr.md
[pol-DATA]: policies/data-data-protection.md
[pol-DERV]: policies/derv-derived-data-is-verified.md
[pol-DEVI]: policies/devi-deviations-are-recorded.md
[pol-ENVS]: policies/envs-environment-separation.md
[pol-EVER]: policies/ever-everything-in-version-control.md
[pol-INTC]: policies/intc-interface-contracts.md
[pol-OBSV]: policies/obsv-observability.md
[pol-PERF]: policies/perf-performance-targets.md
[pol-PIPE]: policies/pipe-pipeline-to-production.md
[pol-RECV]: policies/recv-recoverability.md
[pol-SECD]: policies/secd-security-by-design.md
[pol-TRUS]: policies/trus-trusted-components.md
[pol-VURM]: policies/vurm-vulnerability-remediation.md
[PSBAR 2018]: #psbar-2018
[WCAG 2.2 AA]: #wcag-22-aa
