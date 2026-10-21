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

| Type                              | Nearest prior art                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
|-----------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [ADR](../adrs.md)                 | [Nygard, *Documenting Architecture Decisions*](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) (2011), the ADR as a short record of one decision and what followed from it. [Zimmermann's Y-statement](https://ozimmer.ch/practices/2020/04/27/ArchitectureDecisionMaking.html), a decision written as one sentence in six moves                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| [Data](../data.md)                | [GDPR Article 30](https://eur-lex.europa.eu/eli/reg/2016/679/oj), records of processing activities. [Open Data Contract Standard 3.2](https://github.com/bitol-io/open-data-contract-standard): a contract for one dataset                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| [Explanation](../explanations.md) | [Diátaxis](https://diataxis.fr/explanation/), Explanation                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| [Glossary](../glossary.md)        | Evans, *Domain-Driven Design*, the ubiquitous language a team speaks, models and codes in. [SKOS](https://www.w3.org/TR/skos-reference/), a vocabulary for concept schemes. [DITA `glossentry`](https://docs.oasis-open.org/dita/dita/v1.3/os/part3-all-inclusive/langRef/technicalContent/glossentry.html), a markup element for one term and its definition. [ISO/IEC Directives, Part 2](https://www.iso.org/sites/directives/current/part2/index.xhtml), clause 16.5, the drafting rules a definition follows                                                                                                                                                                                                                                                                                |
| [Integration](../integrations.md) | [Nygard, *Release It!* (2nd ed.)](https://pragprog.com/titles/mnee2/release-it-second-edition/): integration points. [DORA](https://eur-lex.europa.eu/eli/reg/2022/2554/oj), Articles 28 and 30: the register of ICT third-party arrangements. [OpenAPI 3.1](https://spec.openapis.org/oas/v3.1.0) and [AsyncAPI 3.0](https://www.asyncapi.com/docs/reference/specification/v3.0.0): the description an API publishes of itself                                                                                                                                                                                                                                                                                                                                                                  |
| [Offering](../offerings.md)       | ITIL 4 Foundation (2019), 2.3.2: the service offering                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| [Process](../processes.md)        | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Procedure layer. [Diátaxis how-to guide](https://diataxis.fr/how-to-guides/), the shape of a task document. [EPA QA/G-6](https://www.epa.gov/sites/default/files/2015-06/documents/g6-final.pdf), the elements of a written procedure                                                                                                                                                                                                                                                                                                                                                                                                     |
| [Runbook](../runbooks.md)         | [Google SRE, *Introduction*](https://sre.google/sre-book/introduction/), the playbook. [Google SRE Workbook, *On-Call*](https://sre.google/workbook/on-call/), what an on-call engineer is handed with an alert. [AWS Well-Architected OPS07-BP03](https://docs.aws.amazon.com/wellarchitected/latest/operational-excellence-pillar/ops_ready_to_support_use_runbooks.html), the runbook that achieves an outcome, and [OPS07-BP04](https://docs.aws.amazon.com/wellarchitected/latest/operational-excellence-pillar/ops_ready_to_support_use_playbooks.html), the playbook that investigates an issue. [Google SRE, *Accelerating SREs to On-Call and Beyond*](https://sre.google/sre-book/accelerating-sre-on-call/), Disaster Role Playing, the walk of a document against the running system |
| [Service](../services.md)         | [Backstage software catalog](https://backstage.io/docs/features/software-catalog/system-model/), `Component`, [OpsLevel `opslevel.yml`](https://docs.opslevel.com/docs/opslevel-yml) and the [Cortex entity descriptor](https://docs.cortex.io/ingesting-data-into-cortex/entities-overview/entities/yaml): three machine-readable catalogues of an estate's deployables                                                                                                                                                                                                                                                                                                                                                                                                                         |

<!-- END GENERATED: types-lineage -->

## Collisions

Some of these words already mean something else to a reader arriving from another framework.

<!-- BEGIN GENERATED: types-collisions -->

### Process

ITIL and BPMN use *process* for a chain of activities across an organisation. Both reserve *procedure* for the document
that says how one is carried out. ComplianceForge HCGF names its layer Procedures on the same reading. Here a process is
that document. A reader arriving from any of them will look for the wider thing and find one procedure.

### Runbook

The industry does not agree on *runbook* and *playbook*. Google's SRE material says *playbook* for investigating a
failure and for resolving it. AWS splits the two: a runbook achieves a known outcome, and a playbook investigates an
issue. This framework says *runbook* for the incident-time document and *process* for the planned one. *Playbook* also
means an executable artefact in configuration-management tooling, and a word meaning both a document and a program gets
misread in a repository holding both.

<!-- END GENERATED: types-collisions -->

[lineage]: https://paul80nd.github.io/knowledge-as-code/framework/lineage/
