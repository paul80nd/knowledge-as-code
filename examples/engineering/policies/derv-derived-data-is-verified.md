---
id: pol-DERV
tier: normative
category: delivery
status: draft
aligns-with:
review-by: "2027-08-09"
owner: paul.law
tags: [ data-quality, integrity, provenance ]
---

# Derived data is verified before it is trusted

`Policy: pol-DERV` `DRAFT`

## Purpose

Where our code computes a result, rather than storing what it was given, we check that result before anyone relies on
it. A run that finished is not evidence that what it produced is right.

Everywhere else here, verifying the code is enough. The code determines the output, so a test that exercises the code
exercises the answer. That stops being true when correctness depends on data we did not author. A transformation can be
exactly right and still produce a wrong answer, because a source arrived truncated or changed meaning without telling
us. No test catches it, because the thing that varied is not in the test. The failure is silent, and the cost lands on
whoever acted on the number.

## Scope

Any process that produces data by computing over other data rather than recording what a user gave us: batch jobs,
transformations, aggregations, extracts, and report and feature builds. It binds once the output leaves the process that
made it.

_Boundary: [pol-AUTV] verifies the code of such a process and [pol-PIPE] governs how it reaches production. This policy
is about its output. Protecting that data, rather than checking it, is [pol-DATA]'s._

## Clauses

| Id        | Clause                                                                                           | Alignment                                      |
|-----------|--------------------------------------------------------------------------------------------------|------------------------------------------------|
| `EXPECT`  | **MUST** state what a correct output looks like, in terms that can be checked automatically      | [Azure WAF].reliability                        |
| `CHECK`   | **MUST** check a production output against those expectations before anything downstream uses it | [UK GDPR].Art.5(1)(d), [Azure WAF].reliability |
| `RUNLOG`  | **MUST** keep, for each production run, a record of its inputs, its output and its check result  | [Azure WAF].operational-excellence             |
| `FAILED`  | **MUST NOT** use a derived output whose checks did not pass                                      |                                                |
| `LINEAGE` | SHOULD be able to trace a derived value back through the runs that produced it to its source     |                                                |

## Exceptions

Exploratory analysis that nobody else acts on falls outside the Scope above rather than being excepted. It comes into
scope the moment its output informs a decision or feeds another system, which is usually before anyone notices.

Relying on a derived output that has no stated expectations requires a recorded deviation under [pol-DEVI], naming who
accepts a wrong answer reaching a decision. "The numbers have always looked about right" is not a check.

## Notes

[ISO 27001:2022] treats integrity as protection against unauthorised alteration, and no Annex A control asks whether a
computation was correct. `A.8.15` covers the logging behind `RUNLOG` and is claimed by [pol-OBSV]. Citing it here as
well would overstate what this policy adds.

[pol-AUTV]: autv-automated-verification.md
[pol-DATA]: data-data-protection.md
[pol-DEVI]: devi-deviations-are-recorded.md
[pol-OBSV]: obsv-observability.md
[pol-PIPE]: pipe-pipeline-to-production.md
[Azure WAF]: ../frameworks.md#azure-well-architected-framework
[ISO 27001:2022]: ../frameworks.md#iso-27001
[UK GDPR]: ../frameworks.md#uk-gdpr
