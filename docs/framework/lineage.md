# Lineage

The knowledge types are derived from first principles, meaning what kinds of knowledge an engineering organisation
actually holds and how each behaves. Most of them still have a close analogue in an established framework. Lineage is
the register of those analogues: what each one lends, and where this framework parts company with it.

## Why the register is kept

Three reasons. An auditor arriving cold recognises the concepts faster for being told what they are near. A future
naming decision answers to precedent. And several of these words already mean something else to a reader with a
governance or agile background, which is a defect the register exists to close.

## Alignment, not compliance

Naming a framework in a lineage row says the thinking is near theirs. It does not claim conformance, certification, or
that a record would satisfy an assessment against theirs.

A corpus may also have a *standing* against an external framework, meaning obligations it accepts and is measured on. It
records that in its own `frameworks.md` and nowhere else. The two registers are kept apart deliberately, because
standing belongs wholly to the corpus holding it.

## What a row says

A type declares its lineage in its own `.schema/<type>.yaml`, beside the fields it declares. Most have three parts, and
a type with no ancestor has the first alone. The ADR is the clearest to read:

**`prior-art`.** [Nygard, *Documenting Architecture Decisions*][nygard] (2011).

**`alignment`**, which is what the framework took: "Context / Decision / Consequences, and the rule that a reversed
decision is superseded rather than rewritten".

**`divergence`**, which is where it parted company: "Nygard scopes a decision to one codebase. An ADR here affects more
than one repository, and the mandatory *Alternatives Considered* section is an addition to his shape".

A type with a fourth part, `collision`, means the word already denotes something else to a reader arriving from another
framework. Where the collision is severe the type's own root page repeats the warning, because a reader who never opens
the lineage page still needs it.

**The rows themselves live in the corpus, not here.** Each corpus's lineage page renders them from the schema it holds,
so it shows the types that corpus adopted. A corpus declaring a type of its own writes that type's lineage too. It owns
the sourcing behind it exactly as the framework owns the sourcing below.

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

## Where the language rules come from

The `technical-writing` skill, and the tier rules in `writing-a-record`, follow the
[Microsoft Writing Style Guide][mswsg] with a short list of local changes. It is the base because it is public,
complete, and known in depth by the models that write here. `std-PROSE` in this repository's own corpus is the record
that states the same rules for the estate.

The rules are written in this framework's own words and set at its own limits. They are not an implementation of the
Microsoft guide, and no output of a corpus is described as conforming to it.

## What a licence permits

[Diátaxis][diataxis] is licensed CC BY-SA, and that share-alike condition does not sit comfortably with this
repository's MIT licence. It is linked and attributed, and not quoted at length.

The same care applies to every row of the register. This framework cites its prior art, learns from it, and reproduces
none of it. That is [Alignment, not compliance](#alignment-not-compliance) again, read as a rule about quoting.

## What is not verified

Honesty about sourcing is part of the point of the register. Every row the framework declares was checked against a
primary source, except these:

* **ITIL** publications are paywalled, so the known-error definition behind the fix row rests on the publisher's own
  summary.
* **ISO standards** are paywalled, so 25010's characteristics come from the issuing body's public descriptions.
* **ArchiMate** is licence-gated beyond its public specification pages.

Where a claim is later found wrong, correct the row. Do not soften it.

[The default types](types.md) is the page for what each type actually holds, once you know where its name came from.

[diataxis]: https://diataxis.fr/
[mswsg]: https://learn.microsoft.com/style-guide/welcome/
[nygard]: https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions
