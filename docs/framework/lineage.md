# Lineage

The knowledge types are derived from first principles, meaning what kinds of knowledge an engineering organisation
actually holds and how each behaves. Most of them still have a close analogue in an established framework. Lineage is
the register of those analogues: what each one lends, and where this framework parts company with it.

Three reasons the register is worth keeping. An auditor arriving cold recognises the concepts faster for being told what
the concepts are near. A future naming decision answers to precedent. And several of these words already mean something
else to a reader with a governance or agile background, which is a defect the register exists to close.

**This is alignment, not compliance.** Naming a framework in a lineage row says the thinking is near theirs. It does not
claim conformance, certification, or that a record would satisfy an assessment against theirs. Where a corpus has a
*standing* against an external framework, meaning obligations it accepts and is measured on, that corpus records it in
its own `frameworks.md` and nowhere else. The two registers are kept apart deliberately. Standing belongs wholly to the
corpus holding it.

## What a row says

A type declares its lineage in its own `.schema/<type>.yaml`, beside the fields it declares. Most carry three parts, and
a type with no ancestor carries the first alone. The ADR is the clearest to read:

**`prior-art`.** [Nygard, *Documenting Architecture Decisions*][nygard] (2011).

**`alignment`**, which is what the framework took: "Context / Decision / Consequences, and the rule that a reversed
decision is superseded rather than rewritten".

**`divergence`**, which is where it parted company: "Nygard scopes decisions to one codebase. Ours must affect more than
one repository. Mandatory *Alternatives* is ours, not his".

A type carrying a fourth part, `collision`, means the word already denotes something else to a reader arriving from
another framework. Five of them do, and where the collision is severe the type's own root page repeats the warning,
because a reader who never opens the lineage page still needs it.

**The rows themselves live in the corpus, not here.** Each corpus's lineage page renders them from the schema it holds,
so it shows the types that corpus adopted. A corpus that declares a type of its own writes that type's lineage too, and
owns the sourcing behind it exactly as the framework owns the sourcing below.

## What has no precedent

Three things here have no useful ancestor, and claiming one would be worse than admitting none.

**Discovery.** Nothing established defines a document type that is low-ceremony, explicitly unverified,
confidence-scored and self-expiring, with a defined promotion path into a reviewed type. A lab notebook, a fleeting note
and an agile spike each share the provisional quality and none of the rest. The combination of an expiry and a
confidence level is the most novel thing in this taxonomy.

**The Process / Runbook split.** No framework found here divides its procedural layer by *reading conditions*: planned
and deliberate against incident-time and under pressure. Both HCGF and [Diátaxis][diataxis] have one procedural type.
The split exists because the two are written differently, rehearsed differently, and fail differently.

**The tiers.** Classifying by obligation and decay rather than by subject matter has precedent in shape.
[Diátaxis][diataxis] also classifies behaviourally, by user need. The axes are the framework's own.

## Language

The `technical-writing` skill, and the tier rules in `writing-a-record`, draw on two bodies of work.

### ASD-STE100

**[Simplified Technical English][ste]**, Issue 9 (2025), published by the Aerospace, Security and Defence Industries
Association of Europe and maintained by its Simplified Technical English Maintenance Group. A controlled language for
aerospace maintenance documentation. It is a closed dictionary of approved words, each carrying one meaning and one part
of speech, plus writing rules covering sentence length, one instruction per sentence, permitted verb forms and
noun-cluster length. Its influence here is direct, and heaviest on the procedural tier, which is the material it was
built for.

### ISO 24495-1

**[ISO 24495-1:2023][iso24495]**, plain language governing principles: that readers get what they need, can find it, can
understand it, and can use it. These are principles, and no machine can check them. They are still the right statement
of what the mechanics are for.

### How they are cited, and why it matters

ASD-STE100 is free to obtain and is not freely licensed. Reproduction or publication in whole or in part requires
written authority from ASD, unauthorised redistribution is prohibited, and ASD does not endorse third-party compliance
claims. ISO 24495-1 is sold and is likewise all rights reserved.

So this framework cites both, learns from both, and reproduces neither. The authoring rules are written in its own words
and set at its own limits. They are not an implementation of either document, and no output of a corpus is described as
STE-compliant or as conforming to ISO 24495-1. A corpus wanting genuine conformance should obtain the specifications
directly. This is the same alignment-not-compliance stance the rest of this page takes, applied to the one framework
that also constrains how it may be quoted.

The same care applies to [Diátaxis][diataxis] above, licensed CC BY-SA, whose share-alike condition does not sit
comfortably with this repository's MIT licence. It is linked and attributed, and not quoted at length.

## What is not verified

Honesty about sourcing is part of the point of the register. Every row the framework declares was checked against a
primary source, except these:

* **ITIL** publications are paywalled, so the known-error definition behind the FAQ row rests on the publisher's own
  summary.
* **ISO standards** are paywalled, so 25010's characteristics and 24495-1's principles come from the issuing body's
  public descriptions.
* **ArchiMate** is licence-gated beyond its public specification pages.
* **ASD-STE100's** licensing terms and issue details come from the specification's own front matter and the maintenance
  group's published material. The numeric writing rules are corroborated from specialist secondary sources rather than
  from the rule text, so the framework states its own limits instead of quoting theirs.

Where a claim is later found wrong, correct the row. Do not soften it.

[diataxis]: https://diataxis.fr/
[nygard]: https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions
[iso24495]: https://www.iso.org/standard/78907.html
[ste]: https://www.asd-ste100.org/
