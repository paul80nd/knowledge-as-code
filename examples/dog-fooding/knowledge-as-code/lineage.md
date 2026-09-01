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

| Type                        | Nearest prior art                                                                                                                                                                                                                           | Alignment                                                                                                                                 | Divergence                                                                                                                                             |
|-----------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Control](../controls.md)   | [NIST SP 800-53A Rev. 5](https://csrc.nist.gov/pubs/sp/800/53/a/r5/final), the assessment-procedure construct                                                                                                                               | Objective, method and expected evidence, bound to the requirement it tests                                                                | 800-53A procedures assess a federal catalogue inside an authorisation boundary. Ours verify internal standards, with no catalogue and no accreditation |
| [Runbook](../runbooks.md)   | [Google SRE](https://sre.google/sre-book/introduction/), the playbook                                                                                                                                                                       | Recording the response ahead of time rather than improvising it. Google measures roughly a threefold improvement in mean time to recovery | Google specifies no form. We mandate one: terse, imperative, structured as a decision tree                                                             |
| [Service](../services.md)   | [Backstage software catalog](https://backstage.io/docs/features/software-catalog/system-model/), `Component`                                                                                                                                | A deployable unit carrying owner, dependencies and consumed APIs, acting as the hub of the entity graph                                   | Backstage models APIs, resources, systems and domains as separate kinds. We fold dependencies, data stores and environments into the service document  |
| [Standard](../standards.md) | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Standard layer, and [BCP 14](https://www.rfc-editor.org/rfc/rfc8174) for the grammar | Granular and prescriptive requirements, the RFC 2119 keyword set, and the all-capitals rule                                               | We compose standards by union across the folders that apply. HCGF has no composition model                                                             |
| [Tool](../tools.md)         | [Thoughtworks Technology Radar](https://www.thoughtworks.com/radar)                                                                                                                                                                         | Named technologies sorted by stance, with rejections recorded as first-class content                                                      | The Radar is industry opinion, published periodically and explicitly non-binding. Ours is an internal register with binding version ranges             |

<!-- END GENERATED: types-lineage -->

## Collisions

Some of these words already mean something else to a reader arriving from another framework. Where the collision is
severe, the type's own root page repeats the warning, because a reader who never opens this page still needs it.

<!-- BEGIN GENERATED: types-collisions -->

### Control

**The most dangerous word in the taxonomy.** In [NIST SP 800-53](https://csrc.nist.gov/pubs/sp/800/53/r5/upd1/final),
ISO/IEC 27001 Annex A, ISO/IEC 27002 and ComplianceForge, a *control* **is the safeguard itself**: the technical,
administrative or physical measure that reduces risk. Here, a control is the **verification that a rule is being
followed**, which those frameworks call an assessment procedure, a test, or a metric.

A reader with a governance background will misread this type on sight, and will read a coverage report as claiming
safeguards exist when it claims only that checks exist. Say which sense is meant whenever the word crosses into a
compliance conversation.

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
