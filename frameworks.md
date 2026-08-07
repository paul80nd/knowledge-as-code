# Frameworks

The external frameworks this corpus refers to, and what each one obliges us to.

A [policy](/policies) maps its clauses to a framework's controls in the `Alignment` column of its clause table, as
`[ISO 27001:2022].A.8.24`. Those references resolve here, and this page is the only place that says what the
relationship actually is — whether we are bound to a framework, hold ourselves to it, or simply learned from it. A
policy states obligations; it does not state our standing against a framework, because that standing changes on its own
schedule and would otherwise have to be corrected in twenty places at once.

This page is maintained by hand. It is not generated, and it is not a knowledge record — there is no frontmatter, no id
and no index. It exists because the references in the corpus need somewhere honest to land.

## How to read the three postures

| Posture            | What it means                                                                                                                     |
|--------------------|-----------------------------------------------------------------------------------------------------------------------------------|
| **Obliged**        | Something outside us requires it — certification we hold, or law that applies to us. Not optional.                                |
| **Self-obligated** | Nothing external compels it; a policy of ours does. Binding on us because we said so, and revocable only by changing that policy. |
| **Inspiration**    | We took ideas from it. It shapes our thinking and binds nothing. A clause may cite it for provenance, not for obligation.         |

The distinction matters when someone asks *why* a clause exists. "Because we are certified against it" and "because it
seemed sensible" are different answers, and only one of them survives a change of mind.

## Obliged

### ISO/IEC 27001:2022 {#iso27001-2022}

**Registered.** We hold certification against ISO/IEC 27001:2022 and are audited against it. Annex A is the control set
our security, delivery and operations policies map their clauses to, and the mapping is the evidence trail an auditor
follows from a control back to the commitment it implements.

Registration is why the mapping has to be honest rather than generous. A clause mapped to a control it does not really
implement is worse than an unmapped clause: the gap is now hidden behind a reference that looks like coverage. Where no
clause genuinely maps to a control, the control is absent from the corpus and that absence is the finding.

Referenced by: most policies under `category: security`, `delivery` and `operations`.

### UK GDPR and the Data Protection Act 2018 {#uk-gdpr}

**Obliged in law**, wherever we process personal data — which is everywhere we hold a customer, an employee or a user
record. The EU GDPR applies in parallel for people in the EU; the article numbering is the same, so a clause citing
`Art.5(1)(e)` cites both, and only the supervisory authority differs.

This is where [pol-DATA]'s obligations actually originate. ISO/IEC 27001:2022 acknowledges the same ground in one
control — A.5.34, *privacy and protection of personal data* — but a control that says "comply with the applicable law"
is a pointer, not the duty. Lawful basis, minimisation, storage limitation and the rights of a data subject are duties
in their own right, owed to people rather than to an auditor, and they survive any decision to stop being certified.

The practical difference is who can waive it. An Annex A control is ours to scope out with a documented justification;
an article of the UK GDPR is not, and a recorded deviation ([pol-DEVI]) against one of these clauses is a decision to
break the law rather than a risk to accept.

Referenced by: [pol-DATA].

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

Nothing here binds. A clause cites one of these to say where the thinking came from — that we are restating something
the industry already knows rather than inventing it — and citing one is never an argument for keeping a clause we would
otherwise drop.

### DORA metrics {#dora-metrics}

**Inspiration.** The four delivery-performance measures from the DevOps Research and Assessment programme and the
*Accelerate* research behind it: deployment frequency, lead time for changes, change failure rate, and the time to
recover from a failed deployment.

These are outcomes rather than practices, which makes them an unusual thing to align a clause with. What the research
claims — and what we are borrowing — is the causal direction: certain capabilities move certain measures, so a clause
citing `lead-time` is claiming to be one of the things that moves it. That is a testable claim about our own delivery,
not a control we satisfy.

Not to be confused with the **Digital Operational Resilience Act**, the EU regulation for financial entities, which
shares the acronym and nothing else. If that ever applies to us it goes under Obliged, under its full name.

Referenced by: [pol-PIPE], [pol-AUTV].

### Azure Well-Architected Framework {#azure-waf}

**Inspiration.** Microsoft's five pillars for designing and operating a workload: Reliability, Security, Cost
Optimization, Operational Excellence and Performance Efficiency.

Vendor-published rather than standards-body, and we cite Azure's because Azure is what we run on. AWS and Google
publish near-identical pillars, so a move would be a relabelling rather than a re-mapping. Clauses cite the **pillar**
and not the individual checklist recommendation: the recommendations are renumbered as the framework is revised, and a
citation that rots quietly is worse than one that is a little coarse.

This is the only framework that covers [pol-COST], and it is worth being plain about why. No external body will ever
oblige us to manage cloud spend. The Cost Optimization pillar is a genuine influence on how that policy is written, and
recording it is the difference between a policy that borrows from established practice and one that appears to have
been invented in a meeting.

Referenced by: [pol-COST], [pol-RECV], [pol-PERF], [pol-OBSV].

### OWASP ASVS 4.0 {#owasp-asvs-4}

**Inspiration.** The Open Worldwide Application Security Project's Application Security Verification Standard — a
catalogue of application security requirements organised into chapters (`V1` architecture and threat modelling, `V2`
authentication, `V4` access control, `V5` validation and encoding, `V13` APIs and web services), each at three levels
of rigour.

The version is pinned deliberately. ASVS re-chapters between major versions, so `V13` means something specific only
alongside the version it was written against. Moving to a later major version is a re-mapping exercise rather than an
edit to this heading, and doing it as an edit would silently repoint every citation.

Referenced by: [pol-INTC], [pol-SECD], [pol-ACCS].

## Adding a framework

1. Decide the posture first. If you cannot say which of the three it is, the corpus is not ready to reference it.
2. Add a heading under that posture with an explicit anchor — `{#iso27001-2022}`, the version folded in with a hyphen
   rather than a colon, since `:` scopes ids inside the corpus.
3. Say what it is, what it obliges, and what changed if the posture is new. A framework we have just become obliged by
   reads differently from one we have been certified against for years.
4. In the citing policy, define the link at the foot — `[ISO 27001:2022]: /frameworks.md#iso27001-2022` — below the
   corpus references, and cite it per clause rather than per document.

A framework nothing references does not belong here. If we have stopped using one, say so and when, rather than deleting
the entry: a policy clause written under its influence is easier to read with the history intact.

[pol-A11Y]: policies/a11y-accessibility.md
[pol-ACCS]: policies/accs-access-by-identity.md
[pol-AUTV]: policies/autv-automated-verification.md
[pol-COST]: policies/cost-cost-as-an-nfr.md
[pol-DATA]: policies/data-data-protection.md
[pol-DEVI]: policies/devi-deviations-are-recorded.md
[pol-INTC]: policies/intc-interface-contracts.md
[pol-OBSV]: policies/obsv-observability.md
[pol-PERF]: policies/perf-performance-targets.md
[pol-PIPE]: policies/pipe-pipeline-to-production.md
[pol-RECV]: policies/recv-recoverability.md
[pol-SECD]: policies/secd-security-by-design.md
[PSBAR 2018]: #psbar-2018
[WCAG 2.2 AA]: #wcag-22-aa
