# Glossary

The words we use, and what we mean by them.

**[→ Index](glossary/_index.md)**

> **`knowledge-as-code.md` is the only glossary that arrives, and it is not yours to edit.** It holds the framework's
> own vocabulary, it arrives synced, and a local change to it is drift rather than customisation. Write your estate's
> glossaries beside it.

## What is a glossary?

The ubiquitous language of one bounded context: one entry per term whose meaning is specific to us, or which is easily
confused with a neighbouring term.

One file per context, collected under `glossary/`, and each of them is read end to end. The index gives a row per
glossary, so a reader opens the context they are working in and reads its vocabulary whole.

## Why we use them

A glossary is the highest value-per-byte content in the corpus, and the corpus-wide one is what a contributor can read
in full before starting work.

The terms particular to the domain are often not interchangeable, and neighbouring terms are easily confused. A
contributor (human or agent) who doesn't know the distinctions will produce work that is plausible, confident and subtly
wrong, in code and in documentation alike. Every other document here assumes these terms mean something precise. This is
where that precision lives.

## Scope

A term belongs here if it is **specific to the domain, or easily confused with something else**. General industry
vocabulary does not: we are not writing a dictionary, and every entry lengthens a page somebody reads end to end.

Not the place for:

* **A component.** That is a [service](services.md). The glossary may define the *concept* the service is named after.
* **A rule about using the term.** That is a [standard](standards.md).
* **A full explanation of a pattern.** That is an [explanation](explanations.md). A glossary entry is a paragraph, and
  links out for the rest.

**A term goes in the most general glossary that admits it.** A glossary admits a term when the term belongs to its
context, so a word the whole estate uses sits in the corpus-wide file and a word only search uses sits with search. A
narrower glossary redefines a term the general one carries only where the meaning genuinely differs.

## Level of detail and cadence

Each term has one definition, with the words to avoid and what the term does not mean. How the thing it names works
belongs in an explanation.

A term changes rarely, once a team has agreed it. New terms arrive when a team finds a word it has been using two ways.

## Adding a term

1. Choose the most general glossary that admits the term.
2. Where no glossary covers the context, copy [`_template.md`](glossary/_template.md). Its guidance block states how to
   write the file, each field and each entry.
3. Add the entry under `Terms`, as the template's guidance block sets out.

**What ISO asks of a definition.** [ISO/IEC Directives, Part
2](https://www.iso.org/sites/directives/current/part2/index.xhtml), clause 16.5, states the drafting rules this type
follows. Two of them run as checks, and the table says what becomes of the rest.

| Part 2 asks                                         | Here                                                                    |
|-----------------------------------------------------|-------------------------------------------------------------------------|
| A definition does not repeat the term it defines    | `definition-circular` reports one that does.                            |
| A definition states no requirement                  | `entry-requirement` reports a bold RFC 2119 keyword inside an entry.    |
| One definition per entry                            | Yours. Nothing here counts the definitions in an entry.                 |
| Admitted and deprecated terms sit with the term     | `Also` gives an admitted term, and `Avoid` a deprecated one.            |
| A definition taken from a document cites it         | Yours. Link the source inside the entry.                                |
| Only terms the corpus uses are listed               | `unused-terms` states it and does not run.                              |
| A definition can be substituted for the term        | Yours. Read the entry back into a sentence that used the word.          |
| A term a qualified reader knows is not defined      | Yours. Scope above says which reader that is.                           |
| A definition opens with no article                  | Does not apply. An entry here is read as a sentence, not substituted.   |
| A definition closes with no full stop               | Does not apply, for the same reason.                                    |

## Metadata

<!-- BEGIN GENERATED: schema-glossary -->

| Field         | Value            | Notes                                                                        |
|---------------|------------------|------------------------------------------------------------------------------|
| `tier` *†     | `descriptive`    | The record's trust level, fixed for the type and checked against the folder. |
| `status` *†   | `draft` `active` | `draft` while the terms are still settling. In force at `active`.            |
| `narrows`     | id               | Id of the more general glossary this one narrows.                            |
| `review-by` * | date             | Quoted. The date by which the whole glossary is reviewed.                    |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-glossary -->

## What CI checks

<!-- BEGIN GENERATED: checks-glossary -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                       | Level   | What it verifies                                                                                      |
|-----------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `part-none / part-empty`    | error   | The parts section holds at least one heading, and each has something under it.                        |
| `part-id-unique / part-ref` | error   | No two parts of a record share an address, and a `record-id.part` citation reaches the part it names. |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `terms-alphabetical`        | warning | A glossary's entries read in alphabetical order.                                                      |
| `definition-circular`       | warning | A glossary entry's definition does not repeat the term it defines.                                    |
| `redefinition-unlinked`     | warning | Where two glossaries define one term, at least one of the entries links to the other's anchor.        |
| `entry-requirement`         | warning | No bold RFC 2119 keyword states a requirement inside a glossary entry.                                |
| `glossary-in-date`          | warning | A glossary names a review date that has not passed.                                                   |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule              | What it would verify                                                                   |
|-------------------|----------------------------------------------------------------------------------------|
| `undefined-terms` | Reports terms appearing more than N times across the corpus with no glossary entry.    |
| `unused-terms`    | Reports entries nothing uses, where a use inside the term's own context counts as one. |

<!-- END GENERATED: checks-glossary -->
