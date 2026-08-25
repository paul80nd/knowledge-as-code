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

| Type                        | Nearest prior art                                                                                                                                                                                                                           | Alignment                                                                                                                 | Divergence                                                                                                                                            |
|-----------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------|
| [NFR](../nfrs.md)           | [ISO/IEC 25010:2023](https://www.iso.org/standard/78176.html) product quality model                                                                                                                                                         | Quality characteristics that exist to be specified, measured and evaluated. The measurement obligation is 25010's premise | 25010 is a taxonomy of characteristics, not a document type. RPO and RTO come from continuity practice, and per-service budgets from SLO practice     |
| [Service](../services.md)   | [Backstage software catalog](https://backstage.io/docs/features/software-catalog/system-model/), `Component`                                                                                                                                | A deployable unit carrying owner, dependencies and consumed APIs, acting as the hub of the entity graph                   | Backstage models APIs, resources, systems and domains as separate kinds. We fold dependencies, data stores and environments into the service document |
| [Standard](../standards.md) | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Standard layer, and [BCP 14](https://www.rfc-editor.org/rfc/rfc8174) for the grammar | Granular and prescriptive requirements, the RFC 2119 keyword set, and the all-capitals rule                               | We compose standards by union across the axes the type declares. HCGF has no composition model                                                        |

<!-- END GENERATED: types-lineage -->

## Collisions

Some of these words already mean something else to a reader arriving from another framework. Where the collision is
severe, the type's own root page repeats the warning, because a reader who never opens this page still needs it.

<!-- BEGIN GENERATED: types-collisions -->

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
