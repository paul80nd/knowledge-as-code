# Lineage

Where this taxonomy's names came from.

The types were derived from first principles — what kinds of knowledge an engineering organisation actually holds, and
how each behaves — but most of them have close analogues in established frameworks. This page records those analogues,
what each one lends, and where we deliberately part company with it.

Three reasons it is worth writing down. An auditor arriving cold recognises the concepts faster if they are told what
the concepts are near. A future naming decision is constrained by precedent rather than argued from scratch. And several
of these words already mean something else to a reader with a governance or agile background, which is a defect this
page exists to close.

**This is alignment, not compliance.** Naming a framework here says our thinking is near theirs. It does not claim
conformance, certification, or that a document of ours would satisfy an assessment against theirs. Where this corpus has
a *standing* against an external framework — obligations it accepts and is measured on — that is recorded in
[`frameworks.md`](/frameworks.md) and nowhere else. The two are different registers and are deliberately kept apart:
lineage is the framework's own intellectual debt and is identical in every corpus; standing belongs wholly to the corpus
holding it.

## The types

| Type                         | Nearest prior art                                                                    | Alignment                                                                                                                                 | Divergence                                                                                                                                                                     |
|------------------------------|--------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [ADR](/adrs)                 | [Nygard, *Documenting Architecture Decisions*][nygard] (2011)                        | Context / Decision / Consequences, and the rule that a reversed decision is superseded rather than rewritten                              | Nygard scopes decisions to one codebase; ours must affect more than one repository. Mandatory *Alternatives* is ours, not his                                                  |
| [Postmortem](/postmortems)   | [Google SRE, *Postmortem Culture*][sre-pm]                                           | The field set — impact, actions, root cause, follow-up — and blamelessness defined as causes without indictment                           | Google reviews before publication and treats the document as living; ours is immutable once published. We separate root cause from contributing factors                        |
| [Policy](/policies)          | [ComplianceForge HCGF][hcgf], Policy layer                                           | Non-prescriptive statements of intent, with the mandatory language pushed down to the layer below                                         | HCGF policy is security-scoped and externally driven. Ours is inward-facing engineering intent, and states its obligations as an addressable clause table rather than as prose |
| [Standard](/standards)       | [ComplianceForge HCGF][hcgf], Standard layer; [BCP 14][rfc8174] for the grammar      | Granular, prescriptive requirements; the RFC 2119 keyword set and the all-capitals rule                                                   | We compose standards by union across three axes — common, platform, interface. HCGF has no composition model                                                                   |
| [Control](/controls)         | [NIST SP 800-53A Rev. 5][80053a], the assessment-procedure construct                 | Objective, method and expected evidence, bound to the requirement it tests                                                                | 800-53A procedures assess a federal catalogue inside an authorisation boundary. Ours verify internal standards, with no catalogue and no accreditation                         |
| [NFR](/nfrs)                 | [ISO/IEC 25010:2023][25010] product quality model                                    | Quality characteristics that exist to be specified, measured and evaluated — the measurement obligation is 25010's premise                | 25010 is a taxonomy of characteristics, not a document type. RPO and RTO come from continuity practice, and per-service budgets from SLO practice                              |
| [FAQ](/faqs)                 | ITIL Problem Management — the *known error* construct                                | A problem whose cause is understood and whose fix is documented; the observed → analysed → promoted lifecycle                             | ITIL binds a known error to a formal problem record and an owning practice. Ours is promoted by any human verification, from anything noticed                                  |
| [Service](/services)         | [Backstage software catalog][backstage] — `Component`                                | A deployable unit carrying owner, dependencies and consumed APIs, acting as the hub of the entity graph                                   | Backstage models APIs, resources, systems and domains as separate kinds. We fold dependencies, data stores and environments into the service document                          |
| [Capability](/capabilities)  | None that fits. See [Capability](#capability) below                                  | —                                                                                                                                         | —                                                                                                                                                                              |
| [Tools](/tools)              | [Thoughtworks Technology Radar][radar]                                               | Named technologies sorted by stance, with rejections recorded as first-class content                                                      | The Radar is industry opinion, published periodically and explicitly non-binding. Ours is an internal register with binding version ranges                                     |
| [Integration](/integrations) | [Nygard, *Release It!* (2nd ed.)][releaseit] — integration points                    | That every integration point needs a deliberate failure mode and fallback, which is why those are required fields                         | A pattern book, not a document type — and prior art for only half the fields. The commercial ones come from supplier management                                                |
| [Data](/data)                | [GDPR Article 30][gdpr], records of processing activities                            | Categories of data, recipients, transfers, erasure time limits, security measures                                                         | Article 30 is organised by processing activity and by controller. Ours is organised by store. **A Data document is not a ROPA and does not satisfy Article 30**                |
| [Glossary](/glossary)        | Evans, *Domain-Driven Design* — ubiquitous language                                  | A rigorous shared vocabulary maintained against ambiguity, used identically by everyone                                                   | DDD scopes the language to a bounded context and expects a word to differ across contexts. Ours is corpus-wide and single-valued                                               |
| [Explanation](/explanations) | [Diátaxis][diataxis-exp] — Explanation                                               | Name for name. Understanding-oriented, discursive, gives background and considers alternatives                                            | Diátaxis constrains explanation only against its three siblings. We also forbid it being normative or a catalogue entry, because we have types for those                       |
| [Process](/processes)        | [ComplianceForge HCGF][hcgf], Procedure layer; [Diátaxis how-to guide][diataxis-how] | Steps that operationalise the standard above them                                                                                         | Splitting the procedural layer by reading conditions is ours — see [First principles](#first-principles)                                                                       |
| [Runbook](/runbooks)         | [Google SRE][sre-intro] — the playbook                                               | Recording the response ahead of time rather than improvising it; Google measures roughly a threefold improvement in mean time to recovery | Google specifies no form. We mandate one: terse, imperative, structured as a decision tree                                                                                     |
| [Discovery](/discoveries)    | None. See [First principles](#first-principles)                                      | —                                                                                                                                         | —                                                                                                                                                                              |

## Collisions

Four of these words already mean something else to a reader arriving from another framework. Where the collision is
severe, the type's own root page repeats the warning — a reader who never opens this page still needs it.

### Control

**The most dangerous word in the taxonomy.** In [NIST SP 800-53][80053], ISO/IEC 27001 Annex A, ISO/IEC 27002 and
ComplianceForge, a *control* **is the safeguard itself** — the technical, administrative or physical measure that
reduces risk. Here, a control is the **verification that a rule is being followed**, which those frameworks call an
assessment procedure, a test, or a metric.

A reader with a governance background will misread this type on sight, and will read a coverage report as claiming
safeguards exist when it claims only that checks exist. Say which sense is meant whenever the word crosses into a
compliance conversation.

### Capability

**The weakest row in the set, and the one carrying two collisions at once.**

[ArchiMate][archimate] defines a capability as an *ability an organisation possesses*. Ours is what the organisation
*offers a customer* — which in ArchiMate's own vocabulary is a Product, a Business Service, or a Value Stream. The
concepts are adjacent but not the same, and ArchiMate's Value Stream is the closer fit.

[SAFe][safe] defines a capability as large solution functionality spanning multiple trains within a programme
increment — which sits **below** an epic. Ours sits **above** the epic layer. The hierarchy is inverted relative to the
framework in which most engineering organisations will have met the word.

Neither mapping holds. This type is best read as unrelated to both, and the name is a live question rather than a
settled one.

### Standard

In ordinary engineering conversation a *standard* is something an external body publishes — an ISO, an IEEE, an RFC.
Here it is an internal rulebook. The collision is unavoidable, since the governance frameworks we take the layering from
have the same problem, but it is worth saying aloud.

The grammar is worth being precise about too. The keyword set comes from **BCP 14**, which is [RFC 2119][rfc2119]
*and* [RFC 8174][rfc8174] together. RFC 8174 is what establishes that the keywords carry their normative meaning **only
when written in capitals** — which is the rule the standards type actually depends on. Citing RFC 2119 alone leaves that
rule unsupported.

### Policy

In ISO/IEC 27001 an information security policy is a mandatory, auditable artefact. Here, a policy is the deliberately
**non-binding** layer: it states intent, and the standard beneath it carries the obligation. A reader arriving from an
information security management system will read our policies as binding and be wrong about it.

### Runbook

The industry uses *runbook* and *playbook* interchangeably, and Google's SRE material uses the latter. We use *runbook*
because *playbook* also names an executable artefact in configuration-management tooling, and a word that means both a
document and a program is a word that will be misread in a repository holding both.

## First principles

Three things here have no useful ancestor, and claiming one would be worse than admitting none.

**[Discovery](/discoveries).** Nothing established defines a document type that is low-ceremony, explicitly unverified,
confidence-scored, self-expiring, and carries a defined promotion path into a reviewed type. A lab notebook, a fleeting
note and an agile spike each share the provisional quality and none of the rest. The combination of an expiry and a
confidence level is the most novel thing in this taxonomy.

**The Process / Runbook split.** No framework we found divides its procedural layer by *reading conditions* — planned
and deliberate against incident-time and under pressure. Both HCGF and Diátaxis have one procedural type. The split
exists because the two are written differently, rehearsed differently, and fail differently.

**The tiers.** Classifying by obligation and decay rather than by subject matter is not unprecedented in shape —
[Diátaxis][diataxis] also classifies behaviourally, by user need — but the axes are ours.

## Language

The [authoring rules](authoring.md) are informed by two bodies of work.

**[ASD-STE100 Simplified Technical English][ste]**, Issue 9 (2025), published by the Aerospace, Security and Defence
Industries Association of Europe and maintained by its Simplified Technical English Maintenance Group. A controlled
language for aerospace maintenance documentation: a closed dictionary of approved words each carrying one meaning and
one part of speech, plus writing rules covering sentence length, one instruction per sentence, permitted verb forms and
noun-cluster length. Its influence here is direct, and heaviest on the procedural tier — which is the material it was
built for.

**[ISO 24495-1:2023][iso24495]**, plain language governing principles: that readers get what they need, can find it, can
understand it, and can use it. Principles rather than mechanics, and not machine-checkable, but the right statement of
what the mechanics are for.

**How we cite them, and why it matters.** ASD-STE100 is free to obtain and is not freely licensed. Reproduction or
publication in whole or in part requires written authority from ASD, unauthorised redistribution is prohibited, and ASD
does not endorse third-party compliance claims. ISO 24495-1 is sold and is likewise all rights reserved.

So this corpus **cites** both, **learns** from both, and **reproduces** neither. The authoring rules are written in our
own words and set at our own limits; they are not an implementation of either document, and no output of this corpus is
described as STE-compliant or as conforming to ISO 24495-1. A corpus wanting genuine conformance should obtain the
specifications directly. This is the same alignment-not-compliance stance the rest of this page takes, applied to the
one framework that also constrains how it may be quoted.

The same care applies to two sources above: [Diátaxis][diataxis] and arc42 are licensed CC BY-SA, whose share-alike
condition does not sit comfortably with this repository's MIT licence. Both are linked and attributed. Neither is quoted
at length.

## What is not verified

Honesty about sourcing is part of the point of this page.

Every row above was checked against a primary source except these, which could not be: **ITIL** publications are
paywalled, so the known-error definition behind the FAQ row rests on the publisher's own summary rather than the text.
**ISO standards** are paywalled, so 25010's characteristics and 24495-1's principles are taken from the issuing body's
public descriptions rather than from the standards. **ArchiMate and TOGAF** are licence-gated beyond their public
specification pages. **ASD-STE100's** licensing terms and issue details are read from the specification's own front
matter and the maintenance group's published material; the numeric writing rules are corroborated from specialist
secondary sources rather than from the rule text, which is why this corpus states its own limits rather than quoting
theirs.

Where a claim here is later found wrong, correct the row. Do not soften it.

[80053]: https://csrc.nist.gov/pubs/sp/800/53/r5/upd1/final
[80053a]: https://csrc.nist.gov/pubs/sp/800/53/a/r5/final
[25010]: https://www.iso.org/standard/78176.html
[archimate]: https://pubs.opengroup.org/architecture/archimate3-doc/ch-Strategy-Layer.html
[backstage]: https://backstage.io/docs/features/software-catalog/system-model/
[diataxis]: https://diataxis.fr/
[diataxis-exp]: https://diataxis.fr/explanation/
[diataxis-how]: https://diataxis.fr/how-to-guides/
[gdpr]: https://eur-lex.europa.eu/eli/reg/2016/679/oj
[hcgf]: https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework
[iso24495]: https://www.iso.org/standard/78907.html
[nygard]: https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions
[radar]: https://www.thoughtworks.com/radar
[releaseit]: https://pragprog.com/titles/mnee2/release-it-second-edition/
[rfc2119]: https://www.rfc-editor.org/rfc/rfc2119
[rfc8174]: https://www.rfc-editor.org/rfc/rfc8174
[safe]: https://framework.scaledagile.com/features-and-capabilities
[sre-intro]: https://sre.google/sre-book/introduction/
[sre-pm]: https://sre.google/sre-book/postmortem-culture/
[ste]: https://www.asd-ste100.org/
