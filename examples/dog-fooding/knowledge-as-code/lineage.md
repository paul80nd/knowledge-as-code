# Lineage

> Where this corpus's type names came from.

The types are derived from first principles, and most still have a close analogue in an established framework. The rows
below name each analogue. They are generated from the schema, so they cover the types this corpus adopted and no
others.

[Lineage][lineage] has the rest, and the argument does not vary by corpus. It says why naming a framework in a row is
alignment and not compliance, why the register is worth keeping, and what has no precedent at all. It also covers the
language work the writing rules draw on, and which of the framework's own rows rest on a secondary source. A type
this corpus declared for itself is this corpus's to source honestly, on the same terms.

## The types

A type with no useful ancestor says so, and claiming one would be worse than admitting none.

<!-- BEGIN GENERATED: types-lineage -->

| Type                          | Nearest prior art                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
|-------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Control](../controls.md)     | [NIST SP 800-53A Rev. 5](https://csrc.nist.gov/pubs/sp/800/53/a/r5/final), the assessment-procedure construct                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| [Deviation](../deviations.md) | [NIST SP 800-37 Rev. 2](https://csrc.nist.gov/pubs/sp/800/37/r2/final), the plan of action and milestones a named official accepts risk against                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| [Fix](../fixes.md)            | ITIL Problem Management, the *known error* construct, and [the KCS v6 article](https://library.serviceinnovation.org/KCS/KCS_v6/KCS_v6_Practices_Guide/030/040/010/020)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| [Process](../processes.md)    | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Procedure layer. [Diátaxis how-to guide](https://diataxis.fr/how-to-guides/), the shape of a task document. [EPA QA/G-6](https://www.epa.gov/sites/default/files/2015-06/documents/g6-final.pdf), the elements of a written procedure                                                                                                                                                                                                                                                                                                                                                                                                     |
| [Report](../reports.md)       | [Open Knowledge Format](https://github.com/GoogleCloudPlatform/open-knowledge-format/blob/main/SPEC.md) v0.2, the generated-document provenance keys                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| [Runbook](../runbooks.md)     | [Google SRE, *Introduction*](https://sre.google/sre-book/introduction/), the playbook. [Google SRE Workbook, *On-Call*](https://sre.google/workbook/on-call/), what an on-call engineer is handed with an alert. [AWS Well-Architected OPS07-BP03](https://docs.aws.amazon.com/wellarchitected/latest/operational-excellence-pillar/ops_ready_to_support_use_runbooks.html), the runbook that achieves an outcome, and [OPS07-BP04](https://docs.aws.amazon.com/wellarchitected/latest/operational-excellence-pillar/ops_ready_to_support_use_playbooks.html), the playbook that investigates an issue. [Google SRE, *Accelerating SREs to On-Call and Beyond*](https://sre.google/sre-book/accelerating-sre-on-call/), Disaster Role Playing, the walk of a document against the running system |
| [Service](../services.md)     | [Backstage software catalog](https://backstage.io/docs/features/software-catalog/system-model/), `Component`, [OpsLevel `opslevel.yml`](https://docs.opslevel.com/docs/opslevel-yml) and the [Cortex entity descriptor](https://docs.cortex.io/ingesting-data-into-cortex/entities-overview/entities/yaml): three machine-readable catalogues of an estate's deployables                                                                                                                                                                                                                                                                                                                                                                                                                         |
| [Standard](../standards.md)   | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Standard layer, the mandatory requirements beneath a policy. [BCP 14](https://www.rfc-editor.org/rfc/rfc8174), the requirement keywords and the rule that only capitals bind. [ISO/IEC Directives, Part 2](https://www.iso.org/sites/directives/current/part2/index.xhtml), ninth edition, the elements of a normative document. [W3C QA Framework: Specification Guidelines](https://www.w3.org/TR/qaframe-spec/), the conformance clause                                                                                                                                                                                                |
| [Tool](../tools.md)           | [Thoughtworks Technology Radar](https://www.thoughtworks.com/radar): the stances a technology is filed under. [CycloneDX 1.6](https://cyclonedx.org/docs/1.6/json/) and [SPDX 2.3](https://spdx.github.io/spdx-spec/v2.3/package-information/): what identifies a component. [OpenSSF Concise Guide for Evaluating Open Source Software](https://best.openssf.org/Concise-Guide-for-Evaluating-Open-Source-Software.html): what an evaluation asks                                                                                                                                                                                                                                                                                                                                               |

<!-- END GENERATED: types-lineage -->

## Collisions

Some of these words already mean something else to a reader arriving from another framework.

<!-- BEGIN GENERATED: types-collisions -->

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

### Process

ITIL and BPMN use *process* for a chain of activities across an organisation. Both reserve *procedure* for the document
that says how one is carried out. ComplianceForge HCGF names its layer Procedures on the same reading. Here a process is
that document. A reader arriving from any of them will look for the wider thing and find one procedure.

### Report

A report reads like an explanation, and a reader skimming one will file it as the other. An explanation survives every
record being deleted. A report does not.

### Runbook

The industry does not agree on *runbook* and *playbook*. Google's SRE material says *playbook* for investigating a
failure and for resolving it. AWS splits the two: a runbook achieves a known outcome, and a playbook investigates an
issue. This framework says *runbook* for the incident-time document and *process* for the planned one. *Playbook* also
means an executable artefact in configuration-management tooling, and a word meaning both a document and a program gets
misread in a repository holding both.

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
