# Lineage

The knowledge types are derived from first principles, meaning what kinds of knowledge an engineering organisation
actually holds and how each behaves. Most of them still have a close analogue in an established framework. Lineage is
the register of those analogues: what each one lends, and where this framework parts company with it.

The register helps an auditor arriving cold recognise each concept by what it is near. It also warns a reader where one
of these words already means something else in governance or agile practice.

## Alignment, not compliance

Naming a framework in a lineage row says the thinking is near theirs. It does not claim conformance, certification, or
that a record would satisfy an assessment against theirs.

A corpus may also have a *standing* against an external framework, meaning obligations it accepts and is measured on. It
records that in its own `frameworks.md`, apart from lineage.

## What a row says

A type declares its lineage in its own `.schema/<type>.yaml`, beside the fields it declares. Most have three parts, and
a type with no ancestor has the first alone. The ADR row reads:

**`prior-art`**, the sources and what each one is. [Nygard, *Documenting Architecture Decisions*][nygard] (2011), the
ADR as a short record of one decision and what followed from it, and [Zimmermann's Y-statement][ystatement], a
decision written as one sentence in six moves.

**`alignment`**, where this framework went the same way. Nygard's Context / Decision / Consequences, his four status
values, and the rule that a reversed decision is superseded and left standing.

**`divergence`**, where it is notably different. An ADR here affects more than one repository, every alternative
states a verdict, and a fifth status, `rejected`, keeps a proposal the team turned down.

A type with a fourth part, `collision`, means the word already denotes something else to a reader arriving from another
framework. Where the collision is severe the type's own root page repeats the warning, because a reader who never opens
the lineage page still needs it.

**The rows themselves live in the corpus.** Each corpus's lineage page renders them from the schema it has, for the
types that corpus adopted. A corpus declaring a type of its own writes that type's lineage too.

**Lineage is not a record's `sources`.** A row says where the type came from. The `sources` field on a record says where
that one record's content came from. [Filling `sources`](metadata.md#filling-sources) says which of them a document
belongs in.

## What has no precedent

Two things here have no useful ancestor, and claiming one would be worse than admitting none.

**The Process / Runbook split.** No framework found here divides its procedural layer by *reading conditions*: planned
and deliberate against incident-time and under pressure. Both HCGF and [Diátaxis][diataxis] have one procedural type.
The split exists because the two are written differently, rehearsed differently, and fail differently.

**The tiers.** Classifying by obligation and decay rather than by subject matter has precedent in shape.
[Diátaxis][diataxis] also classifies behaviourally, by user need. The axes are the framework's own.

## Where the language rules come from

The `technical-writing` skill, and the tier rules in `writing-a-record`, follow the
[Microsoft Writing Style Guide][mswsg] with a short list of local changes. The rules are written in this framework's own
words, and no output of a corpus is described as conforming to that guide. Diátaxis is linked and attributed, and not
quoted at length.

## What is not verified

Every row the framework declares was checked against a primary source, except these:

* **ITIL** publications are paywalled, so the known-error definition behind the fix row and the service offering
  behind the offering row both rest on the publisher's own summary.
* **ISO standards** are paywalled, so the NFR row rests on 25010 at second hand. Its characteristics and
  subcharacteristics come from the issuing body's public description and from published summaries of the 2023
  edition, and not from the standard's text.

Where a claim is later found wrong, correct the row. Do not soften it.

[The default types](types.md) is the page for what each type actually holds, once you know where its name came from.

[diataxis]: https://diataxis.fr/
[mswsg]: https://learn.microsoft.com/style-guide/welcome/
[nygard]: https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions
[ystatement]: https://ozimmer.ch/practices/2020/04/27/ArchitectureDecisionMaking.html
