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

| Field            | Req | Type   | Notes                                                                                                                                                     |
| ---------------- | --- | ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `id` †           | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                                                                                     |
| `tier` †         | ●   | enum   | `decided` · `normative` · `descriptive` · `procedural` · `observed`. Fixed for the type — a trust signal for the reader. CI checks it matches the folder. |
| `status` †       | ●   | enum   | `draft` · `active` · `retired`                                                                                                                            |
| `owner` †        | ●   | string | A named person, never a team alias.                                                                                                                       |
| `tags` †         |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                                                                          |
| `aligns-with`    |     | list   | e.g. `ISO27001:2022 A.8.25`. Alignment, not compliance.                                                                                                   |
| `implemented-by` |     | list   | Standard ids.                                                                                                                                             |
| `review-by`      | ●   | date   | Quoted. Annual is usually right for a policy.                                                                                                             |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

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

| Check                       | Level   | What it verifies                                                                             |
| --------------------------- | ------- | -------------------------------------------------------------------------------------------- |
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                          |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                               |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                            |
| `required-field`            | error   | Required and conditionally-required fields are present.                                      |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                               |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                         |
| `enum`                      | error   | Enum values are in range and lowercase.                                                      |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                 |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                   |
| `id`                        | error   | `id` carries the type's prefix and, where the type is numbered, matches the filename number. |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                        |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                              |
| `h1`                        | error   | The document has an H1 and, where the type declares one, it matches the title pattern.       |
| `required-section`          | error   | Every required section heading is present.                                                   |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                               |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                              |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                             |
| `unused-definition`         | warning | A link definition that nothing references.                                                   |

<!-- END GENERATED: checks-policies -->
