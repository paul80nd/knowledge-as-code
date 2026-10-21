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

| Type                            | Nearest prior art                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
|---------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Fix](../fixes.md)              | ITIL Problem Management, the *known error* construct, and [the KCS v6 article](https://library.serviceinnovation.org/KCS/KCS_v6/KCS_v6_Practices_Guide/030/040/010/020)                                                                                                                                                                                                                                                                                                                                                                                                                           |
| [NFR](../nfrs.md)               | [ISO/IEC 25010:2023](https://www.iso.org/standard/78176.html): the product quality model. [Google SRE, *Implementing SLOs*](https://sre.google/workbook/implementing-slos/): the service level objective. [OpenSLO](https://github.com/OpenSLO/OpenSLO): the machine-readable objective, and its alert condition                                                                                                                                                                                                                                                                                  |
| [Offering](../offerings.md)     | ITIL 4 Foundation (2019), 2.3.2: the service offering                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| [Postmortem](../postmortems.md) | [Google SRE, *Postmortem Culture*](https://sre.google/sre-book/postmortem-culture/), the blameless postmortem written after an incident                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| [Service](../services.md)       | [Backstage software catalog](https://backstage.io/docs/features/software-catalog/system-model/), `Component`, [OpsLevel `opslevel.yml`](https://docs.opslevel.com/docs/opslevel-yml) and the [Cortex entity descriptor](https://docs.cortex.io/ingesting-data-into-cortex/entities-overview/entities/yaml): three machine-readable catalogues of an estate's deployables                                                                                                                                                                                                                          |
| [Standard](../standards.md)     | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Standard layer, the mandatory requirements beneath a policy. [BCP 14](https://www.rfc-editor.org/rfc/rfc8174), the requirement keywords and the rule that only capitals bind. [ISO/IEC Directives, Part 2](https://www.iso.org/sites/directives/current/part2/index.xhtml), ninth edition, the elements of a normative document. [W3C QA Framework: Specification Guidelines](https://www.w3.org/TR/qaframe-spec/), the conformance clause |

<!-- END GENERATED: types-lineage -->

## Collisions

Some of these words already mean something else to a reader arriving from another framework.

<!-- BEGIN GENERATED: types-collisions -->

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
