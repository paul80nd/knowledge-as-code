# Policies

The engineering commitments we hold ourselves to — the *what* and the *why*.

**[→ Index](policies/INDEX.md)**

## What is a policy?

A high-level, durable statement of what we commit to and why. Policies are principle-level and largely stack-agnostic:
*"secrets are never stored in source control"*, *"quality checks are gates, not advisories"*, *"changes to
non-development environments go through the pipeline"*.

They sit at the top of the normative hierarchy. A policy says what we hold true; a [standard](/standards) says what to
do about it; a [control](/controls) says how we know it happened; a [process](/processes) says how to do it.

## Why we use them

Standards change with the stack. Policies don't — and separating the two means a framework migration doesn't
accidentally relitigate a security commitment.

They also give the standards somewhere to point. A standard that cites no ADR and no policy has no provenance, which is
usually a sign it is either guidance in disguise or a decision nobody has actually made.

Policies record alignment with **ISO/IEC 27001:2022** Annex A where relevant. This is **alignment, not certification** —
we are not registered, and the alignment exists because the framework covers the right ground, not because anyone is
auditing against it. The `aligns-with` field makes that coverage reportable.

## Scope

**The test:** would this still be true after replacing the entire technology estate? If yes, it is a policy. If it names
a tool, a framework or a protocol, it is a [standard](/standards).

| Policy                                          | Standard                                                               |
|-------------------------------------------------|------------------------------------------------------------------------|
| "Secrets are never stored in source control."   | "Services **MUST** read secrets from Key Vault via workload identity." |
| "Quality checks are gates that fail the build." | "ESLint **MUST** run with `--max-warnings 0`."                         |

A policy is not a [control](/controls) — it commits, it does not verify. And it is not an [ADR](/adrs): an ADR records a
specific decision with the alternatives that were weighed; a policy states a position we hold regardless.

## Metadata

<!-- BEGIN GENERATED: schema-policies -->

| Field            | Req | Type | Notes                                                   |
|------------------|-----|------|---------------------------------------------------------|
| `status`         | ●   | enum | `draft` · `active` · `retired`                          |
| `aligns-with`    |     | list | e.g. `ISO27001:2022 A.8.25`. Alignment, not compliance. |
| `implemented-by` |     | list | Standard ids                                            |
| `review-by`      | ●   | date | Quoted                                                  |

<!-- END GENERATED: schema-policies -->

## Adding a policy

1. Apply the test above. Most things that feel like policies are standards.
2. Copy [`template.md`](policies/template.md) to `NNNN-kebab-slug.md`.
3. State the commitment, the scope it applies to, and any explicit exceptions. Exceptions stated up front are honest;
   exceptions discovered later are erosion.
4. Set `aligns-with` where an ISO 27001 Annex A area corresponds. Use `aligns-with`, never wording that implies
   compliance or certification.
5. Set `review-by`. Policies change rarely, so an annual review is usually right.

**Conventions**

* **Never say "compliant" or "certified".** We align. The distinction matters if anyone ever reads this externally.
* **Every policy should have at least one implementing standard**, or be explicitly marked aspirational. A policy
  nothing implements is a statement of intent, and should say so.

## What CI checks

<!-- BEGIN GENERATED: checks-policies -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-policies -->
