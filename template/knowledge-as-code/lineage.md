# Lineage

> Where this corpus's type names came from.

The types are derived from first principles, and most still have a close analogue in an established framework. The rows
below record each analogue, what it lends, and where this framework parts company with it. They are generated from the
schema, so they cover the types this corpus adopted and no others.

**This is alignment, not compliance.** Naming a framework in a row says the thinking is near theirs. It does not claim
conformance, certification, or that a record of ours would satisfy an assessment against theirs. Where this corpus has a
*standing* against an external framework, meaning obligations it accepts and is measured on,
[`frameworks.md`](../frameworks.md) records it and nowhere else does.

[Lineage][lineage] carries the rest, and the argument does not vary by corpus. Why the register is worth keeping, what
has no precedent at all, the language work the writing rules draw on, and which of the framework's own rows rest on a
secondary source. A type this corpus declared for itself is this corpus's to source honestly, on the same terms.

## The types

A type with no useful ancestor says so, and claiming one would be worse than admitting none.

<!-- BEGIN GENERATED: types-lineage -->

| Type                              | Nearest prior art                                                                                                                                                                                                                           | Alignment                                                                                                                                             | Divergence                                                                                                                                                                                       |
|-----------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [ADR](../adrs.md)                 | [Nygard, *Documenting Architecture Decisions*](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) (2011)                                                                                                             | Context / Decision / Consequences, and the rule that a reversed decision is superseded and left standing                                              | Nygard scopes a decision to one codebase. An ADR here affects more than one repository, and the mandatory *Alternatives Considered* section is an addition to his shape                          |
| [Capability](../capabilities.md)  | None that fits. See [Capability](#capability) below                                                                                                                                                                                         | —                                                                                                                                                     | —                                                                                                                                                                                                |
| [Control](../controls.md)         | [NIST SP 800-53A Rev. 5](https://csrc.nist.gov/pubs/sp/800/53/a/r5/final), the assessment-procedure construct                                                                                                                               | Objective, method and expected evidence, bound to the requirement it tests                                                                            | 800-53A procedures assess a federal catalogue inside an authorisation boundary. A control here verifies an internal standard, with no catalogue and no accreditation                             |
| [Data](../data.md)                | [GDPR Article 30](https://eur-lex.europa.eu/eli/reg/2016/679/oj), records of processing activities                                                                                                                                          | Categories of data, recipients, transfers, erasure time limits, security measures                                                                     | Article 30 is organised by processing activity and by controller. This type is organised by data domain. **A Data document is not a ROPA and does not satisfy Article 30**                       |
| [Deviation](../deviations.md)     | [NIST SP 800-37 Rev. 2](https://csrc.nist.gov/pubs/sp/800/37/r2/final), the plan of action and milestones a named official accepts risk against                                                                                             | A named individual accepts a residual risk for a bounded period, and the remediation stays tracked                                                    | A plan of action and milestones sits inside a system authorisation and feeds an accreditation. A deviation here departs from one internal rule, and nothing accredits it                         |
| [Discovery](../discoveries.md)    | None. See [What has no precedent](https://paul80nd.github.io/knowledge-as-code/framework/lineage/#what-has-no-precedent)                                                                                                                    | —                                                                                                                                                     | —                                                                                                                                                                                                |
| [Explanation](../explanations.md) | [Diátaxis](https://diataxis.fr/explanation/), Explanation                                                                                                                                                                                   | Name for name. Understanding-oriented, discursive, gives background and considers alternatives                                                        | Diátaxis constrains explanation only against tutorial, how-to and reference. This framework also forbids an explanation being normative or a catalogue entry, because separate types cover both  |
| [Fix](../fixes.md)                | ITIL Problem Management, the *known error* construct                                                                                                                                                                                        | A problem whose cause is understood and whose fix is documented, along with the observed → analysed → promoted lifecycle                              | ITIL binds a known error to a formal problem record and an owning practice. A fix here is promoted by any verification, from anything somebody noticed                                           |
| [Glossary](../glossary.md)        | Evans, *Domain-Driven Design*: ubiquitous language                                                                                                                                                                                          | A rigorous shared vocabulary maintained against ambiguity, scoped to a bounded context and used identically within it                                 | Evans' language is spoken as much as written, and lives in the model and the code. A glossary here is a set of documents, and nothing checks the code against them                               |
| [Integration](../integrations.md) | [Nygard, *Release It!* (2nd ed.)](https://pragprog.com/titles/mnee2/release-it-second-edition/): integration points                                                                                                                         | Every integration point needs a deliberate failure mode and a fallback, so the type requires both                                                     | *Release It!* is a pattern book and prior art for only half the fields. The commercial fields come from supplier management                                                                      |
| [NFR](../nfrs.md)                 | [ISO/IEC 25010:2023](https://www.iso.org/standard/78176.html) product quality model                                                                                                                                                         | Quality characteristics written so that each one can be specified, measured and evaluated                                                             | 25010 lists the characteristics and stops. This type is a document. RPO and RTO come from continuity practice, and per-service budgets from SLO practice                                         |
| [Policy](../policies.md)          | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Policy layer                                                                         | Statements of intent, with the mandatory language pushed down to the layer below                                                                      | HCGF policy is security-scoped and externally driven. A policy here states inward-facing engineering intent, and puts its obligations in a clause table, so each row can be cited                |
| [Postmortem](../postmortems.md)   | [Google SRE, *Postmortem Culture*](https://sre.google/sre-book/postmortem-culture/)                                                                                                                                                         | The field set (impact, actions, root cause, follow-up), and blamelessness defined as naming causes without naming people                              | Google reviews before publication and treats the document as living. This type freezes a postmortem once it is published, and separates root cause from contributing factors                     |
| [Process](../processes.md)        | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Procedure layer, and [Diátaxis how-to guide](https://diataxis.fr/how-to-guides/)     | Steps that operationalise the standard above them                                                                                                     | Splitting the procedural layer by reading conditions is this framework's own. See [What has no precedent](https://paul80nd.github.io/knowledge-as-code/framework/lineage/#what-has-no-precedent) |
| [Report](../reports.md)           | [Open Knowledge Format](https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md) v0.2, the generated-document provenance keys                                                                                        | `generated` states the producer and the moment, in OKF's `<producer>/<version>` form. `sources` lists each body of knowledge the document answers for | OKF puts no version on a source. This type does, because a reader needs to know which content the report is true of. It also requires a `verified` entry, which OKF leaves to the publisher      |
| [Runbook](../runbooks.md)         | [Google SRE](https://sre.google/sre-book/introduction/), the playbook                                                                                                                                                                       | Writing the response down before the incident. Google measures roughly a threefold improvement in mean time to recovery                               | Google specifies no form. This type mandates one: terse, imperative, structured as a decision tree                                                                                               |
| [Service](../services.md)         | [Backstage software catalog](https://backstage.io/docs/features/software-catalog/system-model/), `Component`                                                                                                                                | A deployable unit with its owner, dependencies and consumed APIs, at the centre of the entity graph                                                   | Backstage models APIs, resources, systems and domains as separate kinds. This type folds dependencies, data stores and environments into one service document                                    |
| [Standard](../standards.md)       | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Standard layer, and [BCP 14](https://www.rfc-editor.org/rfc/rfc8174) for the grammar | Granular and prescriptive requirements, the RFC 2119 keyword set, and the all-capitals rule                                                           | Standards here compose by union across the folders that apply. HCGF has no composition model                                                                                                     |
| [Tool](../tools.md)               | [Thoughtworks Technology Radar](https://www.thoughtworks.com/radar)                                                                                                                                                                         | Named technologies sorted by stance, with rejections recorded beside adoptions                                                                        | The Radar is industry opinion, published periodically and explicitly non-binding. This type is an internal register with binding version ranges                                                  |

<!-- END GENERATED: types-lineage -->

## Collisions

Some of these words already mean something else to a reader arriving from another framework. Where the collision is
severe, the type's own root page repeats the warning, because a reader who never opens this page still needs it.

<!-- BEGIN GENERATED: types-collisions -->

### Capability

[ArchiMate](https://pubs.opengroup.org/architecture/archimate3-doc/ch-Strategy-Layer.html) uses the word for an ability
an organisation possesses, and [SAFe](https://framework.scaledagile.com/features-and-capabilities) uses it for solution
functionality that sits below an epic. Here a capability is what the organisation offers a customer, and it sits above
the epic layer. Say which sense you mean wherever the word crosses into an architecture conversation.

### Control

In [NIST SP 800-53](https://csrc.nist.gov/pubs/sp/800/53/r5/upd1/final), ISO/IEC 27001 Annex A, ISO/IEC 27002 and
ComplianceForge, a *control* **is the safeguard itself**: the technical, administrative or physical measure that reduces
risk. Here, a control is the **verification that a rule is being followed**, which those frameworks call an assessment
procedure, a test, or a metric.

A reader with a governance background will misread this type on sight, and will read a coverage report as a claim that
safeguards exist. It claims only that checks exist. Say which sense you mean wherever the word crosses into a compliance
conversation.

### Deviation

In audit and quality practice a **deviation** is a finding. An auditor records a departure from a procedure after it
happened, and a departure agreed in advance is called a waiver, an exception or a concession. Here the record *is* the
agreement, written before the departure, or straight after one an incident left no time for.

A reader arriving from that background will read this folder as a list of findings against the estate, and a growing
count as the estate getting worse. The count says how much of the estate somebody wrote down. Say which sense you mean
wherever the word crosses into an audit conversation.

### Policy

In ISO/IEC 27001 an information security policy is a mandatory, auditable artefact. Here a policy is the deliberately
**non-binding** layer: it states intent, and the standard beneath it states the obligation. A reader arriving from an
information security management system will read a policy here as binding, and be wrong.

### Report

A report reads like an explanation, and a reader skimming one will file it as the other. An explanation survives every
record being deleted. A report does not.

### Runbook

The industry uses *runbook* and *playbook* interchangeably, and Google's SRE material says *playbook*. This framework
says *runbook*, because *playbook* also means an executable artefact in configuration-management tooling. A word meaning
both a document and a program gets misread in a repository holding both.

### Standard

In ordinary engineering conversation a *standard* is something an external body publishes: an ISO, an IEEE, an RFC. Here
it is an internal rulebook. The governance frameworks this layering comes from have the same collision, so no name
avoids it. Say which sense you mean where both are in play.

The grammar is worth being precise about too. The keyword set comes from **BCP 14**, which is
[RFC 2119](https://www.rfc-editor.org/rfc/rfc2119) *and* [RFC 8174](https://www.rfc-editor.org/rfc/rfc8174) together.
RFC 8174 is what makes the keywords normative **only when written in capitals**, and that is the rule this type depends
on. Cite BCP 14, because RFC 2119 alone leaves it unsupported.

<!-- END GENERATED: types-collisions -->

[lineage]: https://paul80nd.github.io/knowledge-as-code/framework/lineage/
