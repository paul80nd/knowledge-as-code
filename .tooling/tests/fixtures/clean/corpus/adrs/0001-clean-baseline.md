---
id: adr-0001
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# ADR-0001: Clean baseline

> **In the context of** the kac test suite, **facing** the need for a known-good corpus, **we decided** to keep one
> fully valid ADR here, **rather than** pointing the tests at the live wiki, **to achieve** a stable zero-findings
> baseline that catches false positives, **accepting** that this fixture must be maintained alongside the rules.

## Context

The suite needs a document that passes every check, so that a rule which starts firing on valid input is caught as a
regression rather than slipping through unnoticed.

## Decision

Keep exactly one valid ADR in this fixture and assert that `kac validate` reports no findings against it.

## Alternatives Considered

* **Point the tests at the live corpus** — rejected: it drifts as real ADRs are added, so the baseline would not be
  stable and updates would be noisy.
* **Have no clean baseline at all** — rejected: false positives in the rules would then go undetected until they broke
  real authoring.

## Consequences

Any new rule must leave this document finding-free, or the fixture is updated deliberately with the rule change in the
same commit.
