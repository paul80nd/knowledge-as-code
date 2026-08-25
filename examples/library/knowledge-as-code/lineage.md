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

| Type                              | Nearest prior art                                                                                                                                                                                                                       | Alignment                                                                                                                                 | Divergence                                                                                                                                                                       |
|-----------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [ADR](../adrs.md)                 | [Nygard, *Documenting Architecture Decisions*](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) (2011)                                                                                                         | Context / Decision / Consequences, and the rule that a reversed decision is superseded rather than rewritten                              | Nygard scopes decisions to one codebase. Ours must affect more than one repository. Mandatory *Alternatives* is ours, not his                                                    |
| [Capability](../capabilities.md)  | None that fits. See [Capability](#capability) below                                                                                                                                                                                     | —                                                                                                                                         | —                                                                                                                                                                                |
| [Data](../data.md)                | [GDPR Article 30](https://eur-lex.europa.eu/eli/reg/2016/679/oj), records of processing activities                                                                                                                                      | Categories of data, recipients, transfers, erasure time limits, security measures                                                         | Article 30 is organised by processing activity and by controller. Ours is organised by data domain. **A Data document is not a ROPA and does not satisfy Article 30**            |
| [Glossary](../glossary.md)        | Evans, *Domain-Driven Design*: ubiquitous language                                                                                                                                                                                      | A rigorous shared vocabulary maintained against ambiguity, scoped to a bounded context and used identically within it                     | Evans' language is spoken as much as written, and lives in the model and the code. Ours is a set of documents, and nothing holds the code to agreeing with them                  |
| [Integration](../integrations.md) | [Nygard, *Release It!* (2nd ed.)](https://pragprog.com/titles/mnee2/release-it-second-edition/): integration points                                                                                                                     | That every integration point needs a deliberate failure mode and fallback, so the type requires both                                      | A pattern book, not a document type, and prior art for only half the fields. The commercial ones come from supplier management                                                   |
| [Process](../processes.md)        | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Procedure layer, and [Diátaxis how-to guide](https://diataxis.fr/how-to-guides/) | Steps that operationalise the standard above them                                                                                         | Splitting the procedural layer by reading conditions is ours. See [What has no precedent](https://paul80nd.github.io/knowledge-as-code/framework/lineage/#what-has-no-precedent) |
| [Runbook](../runbooks.md)         | [Google SRE](https://sre.google/sre-book/introduction/), the playbook                                                                                                                                                                   | Recording the response ahead of time rather than improvising it. Google measures roughly a threefold improvement in mean time to recovery | Google specifies no form. We mandate one: terse, imperative, structured as a decision tree                                                                                       |
| [Service](../services.md)         | [Backstage software catalog](https://backstage.io/docs/features/software-catalog/system-model/), `Component`                                                                                                                            | A deployable unit carrying owner, dependencies and consumed APIs, acting as the hub of the entity graph                                   | Backstage models APIs, resources, systems and domains as separate kinds. We fold dependencies, data stores and environments into the service document                            |

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

### Runbook

The industry uses *runbook* and *playbook* interchangeably, and Google's SRE material uses the latter. We use *runbook*
because *playbook* also names an executable artefact in configuration-management tooling, and a word that means both a
document and a program is a word that will be misread in a repository holding both.

<!-- END GENERATED: types-collisions -->

[lineage]: https://paul80nd.github.io/knowledge-as-code/framework/lineage/
