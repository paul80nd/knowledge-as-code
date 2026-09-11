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

| Type                              | Nearest prior art                                                                                                                                                                                                                       | Alignment                                                                                                               | Divergence                                                                                                                                                                                       |
|-----------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [ADR](../adrs.md)                 | [Nygard, *Documenting Architecture Decisions*](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) (2011)                                                                                                         | Context / Decision / Consequences, and the rule that a reversed decision is superseded and left standing                | Nygard scopes a decision to one codebase. An ADR here affects more than one repository, and the mandatory *Alternatives Considered* section is an addition to his shape                          |
| [Capability](../capabilities.md)  | None that fits. See [Capability](#capability) below                                                                                                                                                                                     | —                                                                                                                       | —                                                                                                                                                                                                |
| [Data](../data.md)                | [GDPR Article 30](https://eur-lex.europa.eu/eli/reg/2016/679/oj), records of processing activities                                                                                                                                      | Categories of data, recipients, transfers, erasure time limits, security measures                                       | Article 30 is organised by processing activity and by controller. This type is organised by data domain. **A Data document is not a ROPA and does not satisfy Article 30**                       |
| [Glossary](../glossary.md)        | Evans, *Domain-Driven Design*: ubiquitous language                                                                                                                                                                                      | A rigorous shared vocabulary maintained against ambiguity, scoped to a bounded context and used identically within it   | Evans' language is spoken as much as written, and lives in the model and the code. A glossary here is a set of documents, and nothing checks the code against them                               |
| [Integration](../integrations.md) | [Nygard, *Release It!* (2nd ed.)](https://pragprog.com/titles/mnee2/release-it-second-edition/): integration points                                                                                                                     | Every integration point needs a deliberate failure mode and a fallback, so the type requires both                       | *Release It!* is a pattern book and prior art for only half the fields. The commercial fields come from supplier management                                                                      |
| [Process](../processes.md)        | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Procedure layer, and [Diátaxis how-to guide](https://diataxis.fr/how-to-guides/) | Steps that operationalise the standard above them                                                                       | Splitting the procedural layer by reading conditions is this framework's own. See [What has no precedent](https://paul80nd.github.io/knowledge-as-code/framework/lineage/#what-has-no-precedent) |
| [Runbook](../runbooks.md)         | [Google SRE](https://sre.google/sre-book/introduction/), the playbook                                                                                                                                                                   | Writing the response down before the incident. Google measures roughly a threefold improvement in mean time to recovery | Google specifies no form. This type mandates one: terse, imperative, structured as a decision tree                                                                                               |
| [Service](../services.md)         | [Backstage software catalog](https://backstage.io/docs/features/software-catalog/system-model/), `Component`                                                                                                                            | A deployable unit with its owner, dependencies and consumed APIs, at the centre of the entity graph                     | Backstage models APIs, resources, systems and domains as separate kinds. This type folds dependencies, data stores and environments into one service document                                    |

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

### Runbook

The industry uses *runbook* and *playbook* interchangeably, and Google's SRE material says *playbook*. This framework
says *runbook*, because *playbook* also means an executable artefact in configuration-management tooling. A word meaning
both a document and a program gets misread in a repository holding both.

<!-- END GENERATED: types-collisions -->

[lineage]: https://paul80nd.github.io/knowledge-as-code/framework/lineage/
