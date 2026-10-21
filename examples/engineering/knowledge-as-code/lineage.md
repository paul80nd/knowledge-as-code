# Lineage

> Where this corpus's type names came from.

The types are derived from first principles, and most still have a close analogue in an established framework. The rows
below name each analogue. What a type takes from it and where the type differs stay in the schema, as `alignment` and
`divergence` under `lineage:` in `.schema/<type>.yaml`. The rows are generated from the schema, so they cover the types
this corpus adopted and no others.

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

| Type                        | Nearest prior art                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
|-----------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [ADR](../adrs.md)           | [Nygard, *Documenting Architecture Decisions*](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) (2011), the ADR as a short record of one decision and what followed from it. [Zimmermann's Y-statement](https://ozimmer.ch/practices/2020/04/27/ArchitectureDecisionMaking.html), a decision written as one sentence in six moves                                                                                                                                                                                                                                        |
| [Control](../controls.md)   | [NIST SP 800-53A Rev. 5](https://csrc.nist.gov/pubs/sp/800/53/a/r5/final), the assessment-procedure construct                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| [Glossary](../glossary.md)  | Evans, *Domain-Driven Design*, the ubiquitous language a team speaks, models and codes in. [SKOS](https://www.w3.org/TR/skos-reference/), a vocabulary for concept schemes. [DITA `glossentry`](https://docs.oasis-open.org/dita/dita/v1.3/os/part3-all-inclusive/langRef/technicalContent/glossentry.html), a markup element for one term and its definition. [ISO/IEC Directives, Part 2](https://www.iso.org/sites/directives/current/part2/index.xhtml), clause 16.5, the drafting rules a definition follows                                                                                 |
| [Policy](../policies.md)    | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Policy layer                                                                                                                                                                                                                                                                                                                                                                                                                               |
| [Report](../reports.md)     | [Open Knowledge Format](https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md) v0.2, the generated-document provenance keys                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| [Standard](../standards.md) | [ComplianceForge HCGF](https://complianceforge.com/start-here/governance-risk-compliance-grc-content/hierarchical-cybersecurity-governance-framework), Standard layer, the mandatory requirements beneath a policy. [BCP 14](https://www.rfc-editor.org/rfc/rfc8174), the requirement keywords and the rule that only capitals bind. [ISO/IEC Directives, Part 2](https://www.iso.org/sites/directives/current/part2/index.xhtml), ninth edition, the elements of a normative document. [W3C QA Framework: Specification Guidelines](https://www.w3.org/TR/qaframe-spec/), the conformance clause |

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

### Policy

In ISO/IEC 27001 an information security policy is one approved document, and the topic-specific policies sit beneath
it. Here every policy is topic-specific, and nothing sits above them. A reader arriving from an information security
management system will look for the overarching policy and find none.

### Report

A report reads like an explanation, and a reader skimming one will file it as the other. An explanation survives every
record being deleted. A report does not.

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
