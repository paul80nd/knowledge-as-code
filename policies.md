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

## Categories

Every policy carries a `category`: **security**, **delivery**, **operations** or **governance**. It answers *why this
policy exists* — the broad area of the commitment — where `tags` answer *what topics it touches*. Two different
questions, so two fields: a secrets policy is `category: security` and `tags: [credentials, key-management, secrets]`.

The set is closed and deliberately small. Four categories group twenty-one policies into groups worth navigating; a
fifth would have to earn its place by making one of these too crowded to scan, and the pressure for that is easier to
judge once there are enough policies to feel it.

Category is metadata, not folder structure. `policies/` stays flat, which means recategorising a policy is a one-line
edit rather than a file move that rewrites every document linking to it — and the awkward calls here (accessibility
under governance is the clearest) are the ones most likely to be revisited.

## Metadata

<!-- BEGIN GENERATED: schema-policies -->

| Field            | Req | Type   | Notes                                                                                |
|------------------|-----|--------|--------------------------------------------------------------------------------------|
| `id` †           | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                |
| `tier` †         | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder. |
| `status` †       | ●   | enum   | `draft` until agreed; `retired` rather than deleted.                                 |
| `owner` †        | ●   | string | A named person, never a team alias.                                                  |
| `tags` †         |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                     |
| `category`       | ●   | enum   | The broad area the commitment belongs to. Controlled, and deliberately few.          |
| `aligns-with`    |     | list   | e.g. `ISO27001:2022 A.8.25`. Alignment, not compliance.                              |
| `implemented-by` |     | list   | Standard ids.                                                                        |
| `review-by`      | ●   | date   | Quoted. Annual is usually right for a policy.                                        |

**Enum values**

| Field      | Values                                                              |
|------------|---------------------------------------------------------------------|
| `tier`     | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status`   | `draft` · `active` · `retired`                                      |
| `category` | `security` · `delivery` · `operations` · `governance`               |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-policies -->

## Adding a policy

1. Apply the test above. Most things that feel like policies are standards.
2. Choose a four-character mnemonic for the policy's *concept* — `VURM` for vulnerability remediation, `PIPE` for
   pipeline-to-production. Start it with the same letter as the slug, so the folder still reads alphabetically.
3. Copy [`template.md`](policies/template.md) to `mnem-kebab-slug.md`, lower-case, and set `id` to `pol-MNEM`,
   upper-case. The H1 opens with that same id as a code span — ``# `pol-MNEM` The commitment`` — and CI checks the two
   agree.
4. Set `category` to whichever of the four the commitment belongs to. If two fit, pick the one a reader looking for this
   policy would try first; if none does, that is a taxonomy conversation, not a fifth category invented in passing.
5. State the commitment, the scope it applies to, and any explicit exceptions. Exceptions stated up front are honest;
   exceptions discovered later are erosion.
6. Set `aligns-with` where an ISO 27001 Annex A area corresponds. Use `aligns-with`, never wording that implies
   compliance or certification.
7. Set `review-by`. Policies change rarely, so an annual review is usually right.

**Conventions**

* **Never say "compliant" or "certified".** We align. The distinction matters if anyone ever reads this externally.
* **Every policy should have at least one implementing standard**, or be explicitly marked aspirational. A policy
  nothing implements is a statement of intent, and should say so.
* **A policy id is immutable once the policy is active.** Rewrite the title, sharpen the commitments, correct the
  scope — the id does not move. Standards, controls and processes cite policies by id, and a mnemonic that is
  reassigned turns every one of those citations into a quiet lie: the reference still resolves, so nothing fails, and
  the reader is simply told something untrue.

  This is why the mnemonic comes from the concept rather than the wording. A policy whose *meaning* has changed enough
  to invalidate its mnemonic has not been edited — it has been replaced. **Retire the old policy and write a new one**,
  so the record shows the position we used to hold, the position we hold now, and that they are different positions.
  Retirement is cheap and keeps the history honest; an id quietly meaning something new destroys it.

## What CI checks

<!-- BEGIN GENERATED: checks-policies -->

| Check                       | Level   | What it verifies                                                                                   |
|-----------------------------|---------|----------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                     |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                  |
| `required-field`            | error   | Required and conditionally-required fields are present.                                            |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                     |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                               |
| `enum`                      | error   | Enum values are in range and lowercase.                                                            |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                       |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                         |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                         |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                      |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                              |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                    |
| `h1`                        | error   | The document has an H1 matching the title pattern, opening with its id where the type carries one. |
| `required-section`          | error   | Every required section heading is present.                                                         |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                     |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                    |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                           |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                   |
| `unused-definition`         | warning | A link definition that nothing references.                                                         |

<!-- END GENERATED: checks-policies -->
