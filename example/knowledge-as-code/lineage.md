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

| Type                              | Nearest prior art                                                                                                                                                                                                                           | Alignment                                                                                                                                 | Divergence                                                                                                                                                                       |
|-----------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [ADR](../adrs.md)                 | [Nygard, *Documenting Architecture Decisions*](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) (2011)                                                                                                             | Context / Decision / Consequences, and the rule that a reversed decision is superseded rather than rewritten                              | Nygard scopes decisions to one codebase. Ours must affect more than one repository. Mandatory *Alternatives* is ours, not his                                                    |
| [Capability](../capabilities.md)  | None that fits. See [Capability](#capability) below                                                                                                                                                                                         | —                                                                                                                                         | —                                                                                                                                                                                |
| [Control](../controls.md)         | [NIST SP 800-53A Rev. 5](https://csrc.nist.gov/pubs/sp/800/53/a/r5/final), the assessment-procedure construct                                                                                                                               | Objective, method and expected evidence, bound to the requirement it tests                                                                | 800-53A procedures assess a federal catalogue inside an authorisation boundary. Ours verify internal standards, with no catalogue and no accreditation                           |
| [Data](../data.md)                | [GDPR Article 30](https://eur-lex.europa.eu/eli/reg/2016/679/oj), records of processing activities                                                                                                                                          | Categories of data, recipients, transfers, erasure time limits, security measures                                                         | Article 30 is organised by processing activity and by controller. Ours is organised by data domain. **A Data document is not a ROPA and does not satisfy Article 30**            |
| [Discovery](../discoveries.md)    | None. See [What has no precedent](https://paul80nd.github.io/knowledge-as-code/framework/lineage/#what-has-no-precedent)                                                                                                                    | —                                                                                                                                         | —                                                                                                                                                                                |
| [Explanation](../explanations.md) | [Diátaxis](https://diataxis.fr/explanation/), Explanation                                                                                                                                                                                   | Name for name. Understanding-oriented, discursive, gives background and considers alternatives                                            | Diátaxis constrains explanation only against its three siblings. We also forbid it being normative or a catalogue entry, because we have types for those                         |
| [FAQ](../faqs.md)                 | ITIL Problem Management, the *known error* construct                                                                                                                                                                                        | A problem whose cause is understood and whose fix is documented, and the observed → analysed → promoted lifecycle                         | ITIL binds a known error to a formal problem record and an owning practice. Ours is promoted by any human verification, from anything noticed                                    |
| [Glossary](../glossary.md)        | Evans, *Domain-Driven Design*: ubiquitous language                                                                                                                                                                                          | A rigorous shared vocabulary maintained against ambiguity, scoped to a bounded context and used identically within it                     | Evans' language is spoken as much as written, and lives in the model and the code. Ours is a set of documents, and nothing holds the code to agreeing with them                  |
| [Integration](../integrations.md) | [Nygard, *Release It!* (2nd ed.)](https://pragprog.com/titles/mnee2/release-it-second-edition/): integration points                                                                                                                         | That every integration point needs a deliberate failure mode and fallback, so the type requires both                                      | A pattern book, not a document type, and prior art for only half the fields. The commercial ones come from supplier management                                                   |
| [NFR](../nfrs.md)                 | [ISO/IEC 25010:2023](https://www.iso.org/standard/78176.html) product quality model                                                                                                                                                         | Quality characteristics that exist to be specified, measured and evaluated. The measurement obligation is 25010's premise                 | 25010 is a taxonomy of characteristics, not a document type. RPO and RTO come from continuity practice, and per-service budgets from SLO practice                                |
| [Policy](../policies.md)          | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Policy layer                                                                         | Non-prescriptive statements of intent, with the mandatory language pushed down to the layer below                                         | HCGF policy is security-scoped and externally driven. Ours is inward-facing engineering intent, and states its obligations as an addressable clause table rather than as prose   |
| [Postmortem](../postmortems.md)   | [Google SRE, *Postmortem Culture*](https://sre.google/sre-book/postmortem-culture/)                                                                                                                                                         | The field set (impact, actions, root cause, follow-up) and blamelessness defined as causes without indictment                             | Google reviews before publication and treats the document as living. Ours is immutable once published. We separate root cause from contributing factors                          |
| [Process](../processes.md)        | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Procedure layer, and [Diátaxis how-to guide](https://diataxis.fr/how-to-guides/)     | Steps that operationalise the standard above them                                                                                         | Splitting the procedural layer by reading conditions is ours. See [What has no precedent](https://paul80nd.github.io/knowledge-as-code/framework/lineage/#what-has-no-precedent) |
| [Runbook](../runbooks.md)         | [Google SRE](https://sre.google/sre-book/introduction/), the playbook                                                                                                                                                                       | Recording the response ahead of time rather than improvising it. Google measures roughly a threefold improvement in mean time to recovery | Google specifies no form. We mandate one: terse, imperative, structured as a decision tree                                                                                       |
| [Service](../services.md)         | [Backstage software catalog](https://backstage.io/docs/features/software-catalog/system-model/), `Component`                                                                                                                                | A deployable unit carrying owner, dependencies and consumed APIs, acting as the hub of the entity graph                                   | Backstage models APIs, resources, systems and domains as separate kinds. We fold dependencies, data stores and environments into the service document                            |
| [Standard](../standards.md)       | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Standard layer, and [BCP 14](https://www.rfc-editor.org/rfc/rfc8174) for the grammar | Granular and prescriptive requirements, the RFC 2119 keyword set, and the all-capitals rule                                               | We compose standards by union across the axes the type declares. HCGF has no composition model                                                                                   |
| [Tool](../tools.md)               | [Thoughtworks Technology Radar](https://www.thoughtworks.com/radar)                                                                                                                                                                         | Named technologies sorted by stance, with rejections recorded as first-class content                                                      | The Radar is industry opinion, published periodically and explicitly non-binding. Ours is an internal register with binding version ranges                                       |

<!-- END GENERATED: types-lineage -->

## Collisions

Some of these words already mean something else to a reader arriving from another framework. Where the collision is
severe, the type's own root page repeats the warning, because a reader who never opens this page still needs it.

<!-- BEGIN GENERATED: types-collisions -->

### Capability

**The weakest row in the set, and the one carrying two collisions at once.**

[ArchiMate](https://pubs.opengroup.org/architecture/archimate3-doc/ch-Strategy-Layer.html) defines a capability as an
*ability an organisation possesses*. Ours is what the organisation *offers a customer*, which in ArchiMate's own
vocabulary is a Product, a Business Service, or a Value Stream. The concepts are adjacent but not the same, and
ArchiMate's Value Stream is the closer fit.

[SAFe](https://framework.scaledagile.com/features-and-capabilities) defines a capability as large solution functionality
spanning multiple trains within a programme increment, which sits **below** an epic. Ours sits **above** the epic layer.
The hierarchy is inverted relative to the framework in which most engineering organisations will have met the word.

Neither mapping holds. This type is best read as unrelated to both, and the name is a live question rather than a
settled one.

### Control

**The most dangerous word in the taxonomy.** In [NIST SP 800-53](https://csrc.nist.gov/pubs/sp/800/53/r5/upd1/final),
ISO/IEC 27001 Annex A, ISO/IEC 27002 and ComplianceForge, a *control* **is the safeguard itself**: the technical,
administrative or physical measure that reduces risk. Here, a control is the **verification that a rule is being
followed**, which those frameworks call an assessment procedure, a test, or a metric.

A reader with a governance background will misread this type on sight, and will read a coverage report as claiming
safeguards exist when it claims only that checks exist. Say which sense is meant whenever the word crosses into a
compliance conversation.

### Policy

In ISO/IEC 27001 an information security policy is a mandatory, auditable artefact. Here, a policy is the deliberately
**non-binding** layer: it states intent, and the standard beneath it carries the obligation. A reader arriving from an
information security management system will read our policies as binding and be wrong about it.

### Runbook

The industry uses *runbook* and *playbook* interchangeably, and Google's SRE material uses the latter. We use *runbook*
because *playbook* also names an executable artefact in configuration-management tooling, and a word that means both a
document and a program is a word that will be misread in a repository holding both.

### Standard

In ordinary engineering conversation a *standard* is something an external body publishes: an ISO, an IEEE, an RFC. Here
it is an internal rulebook. The collision is unavoidable, since the governance frameworks we take the layering from have
the same problem, but it is worth saying aloud.

The grammar is worth being precise about too. The keyword set comes from **BCP 14**, which is
[RFC 2119](https://www.rfc-editor.org/rfc/rfc2119) *and* [RFC 8174](https://www.rfc-editor.org/rfc/rfc8174) together.
RFC 8174 establishes that the keywords carry their normative meaning **only when written in capitals**, and that is the
rule this type depends on. Citing RFC 2119 alone leaves it unsupported.

<!-- END GENERATED: types-collisions -->

[lineage]: https://paul80nd.github.io/knowledge-as-code/framework/lineage/
