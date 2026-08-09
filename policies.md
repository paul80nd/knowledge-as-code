# Policies

The engineering commitments we hold ourselves to — the *what* and the *why*.

**[→ Index](policies/_index.md)**

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

Policies map their clauses to external frameworks — **ISO/IEC 27001:2022** Annex A most often — in the `Alignment`
column of the clause table. Per clause, because a policy aligns with a control through one obligation rather than
through all of them, and an empty cell where no genuine mapping exists is more use than an invented one.

What our standing against a framework actually is — bound by certification, self-obligated by a policy of our own, or
simply borrowed from — is recorded once in [Frameworks](/frameworks.md) and nowhere else. A policy states obligations;
it does not state our standing, which changes on its own schedule and would otherwise have to be corrected in twenty
places at once.

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

| Field         | Req | Type   | Notes                                                                                |
|---------------|-----|--------|--------------------------------------------------------------------------------------|
| `id` †        | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                |
| `tier` †      | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder. |
| `status` †    | ●   | enum   | `draft` until agreed; `retired` rather than deleted.                                 |
| `owner` †     | ●   | string | A named person, never a team alias.                                                  |
| `tags` †      |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                     |
| `category`    | ●   | enum   | The broad area the commitment belongs to. Controlled, and deliberately few.          |
| `aligns-with` |     | list   | e.g. `ISO27001:2022 A.8.25`. The document-level roll-up of what its clauses map to.  |
| `review-by`   | ●   | date   | Quoted. Annual is usually right for a policy.                                        |

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
3. Copy [`_template.md`](policies/_template.md) to `mnem-kebab-slug.md`, lower-case, and set `id` to `pol-MNEM`,
   upper-case. The H1 is the commitment in plain words; the identity line beneath it carries the id —
   ``` `Policy: pol-MNEM` `DRAFT` ``` — and CI checks it against the frontmatter.
4. Set `category` to whichever of the four the commitment belongs to. If two fit, pick the one a reader looking for this
   policy would try first; if none does, that is a taxonomy conversation, not a fifth category invented in passing.
5. State the scope it binds, then the commitment itself as clauses — one obligation per row, each with a short
   upper-case id, ordered **MUST**, **MUST NOT**, SHOULD, COULD. Add any explicit exceptions beneath them. Exceptions
   stated up front are honest; exceptions discovered later are erosion.
6. Map clauses to framework controls in the `Alignment` column where a genuine mapping exists, and roll the references
   up into `aligns-with`. A framework cited for the first time gets an entry in [Frameworks](/frameworks.md) — decide
   its posture there before citing it here.
7. Set `review-by`. Policies change rarely, so an annual review is usually right.

**Conventions**

* **A policy does not state our standing against a framework.** Not "compliant", not "certified", not "registered" —
  a policy maps its clauses to controls and says nothing about what that mapping is worth. Standing is recorded once,
  in [Frameworks](/frameworks.md), so that a change of certification is one edit rather than twenty.
* **A policy names no implementers.** The reference points up: a standard declares the policy it puts into practice,
  and a policy says nothing about what implements it. A downstream corpus inherits these policies and writes its own
  standards against them, so the set of implementers is not knowable from here — and a policy nothing in *this* wiki
  implements is the normal state rather than a gap to be explained.
* **A clause is the unit anything else cites.** Written `pol-VURM.TIMEBOX` — the policy id, then the clause id — so a
  standard, a control or a deviation names the obligation it answers rather than the whole document. Clause ids are
  immutable for the same reason policy ids are: removing or renaming one silently breaks every citation of it.
* **A policy id is immutable once the policy is active.** Rewrite the title, sharpen the commitments, correct the
  scope — the id does not move. Standards, controls and processes cite policies by id, and a mnemonic that is reassigned
  turns every one of those citations into a quiet lie: the reference still resolves, so nothing fails, and the reader is
  simply told something untrue.

  This is why the mnemonic comes from the concept rather than the wording. A policy whose *meaning* has changed enough
  to invalidate its mnemonic has not been edited — it has been replaced. **Retire the old policy and write a new one**,
  so the record shows the position we used to hold, the position we hold now, and that they are different positions.
  Retirement is cheap and keeps the history honest; an id quietly meaning something new destroys it.

## What CI checks

<!-- BEGIN GENERATED: checks-policies -->

| Check                            | Level   | What it verifies                                                                                                                                                                                              |
|----------------------------------|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`             | error   | Frontmatter is present and is a valid YAML mapping.                                                                                                                                                           |
| `unknown-key`                    | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                                                                                                                |
| `key-order`                      | error   | Key order is a topological extension of the schema's field order.                                                                                                                                             |
| `required-field`                 | error   | Required and conditionally-required fields are present.                                                                                                                                                       |
| `bare-key`                       | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                                                                                                                |
| `date-quoted / date-format`      | error   | Date fields are quoted `YYYY-MM-DD`.                                                                                                                                                                          |
| `enum`                           | error   | Enum values are in range and lowercase.                                                                                                                                                                       |
| `field-pattern`                  | error   | Values match the pattern their field declares (e.g. `tags`).                                                                                                                                                  |
| `list-order`                     | warning | List entries read in alphabetical order, with numbers compared as numbers.                                                                                                                                    |
| `tier-matches-type`              | error   | `tier` matches the tier the type declares.                                                                                                                                                                    |
| `id`                             | error   | `id` carries the type's prefix and matches the filename's number, mnemonic or slug.                                                                                                                           |
| `id-unique`                      | error   | `id` is unique across the whole wiki.                                                                                                                                                                         |
| `filename / slug-length`         | error   | Filename matches the pattern; the slug is within 30 characters.                                                                                                                                               |
| `h1`                             | error   | The document has an H1.                                                                                                                                                                                       |
| `identity`                       | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.                                                                                                      |
| `required-section`               | error   | Every required section heading is present.                                                                                                                                                                    |
| `clauses`                        | error   | The clause section is a table of `Id \| Clause` rows, each id a unique code span and each clause opening with its modal.                                                                                      |
| `clause-order / clause-compound` | warning | Clause rows are grouped by binding level, and each carries a single obligation.                                                                                                                               |
| `clause-ref`                     | error   | A `pol-XXXX.CLAUSE` citation names a clause that exists.                                                                                                                                                      |
| `link-resolves`                  | error   | Every internal link resolves (all link forms, `.md` optional).                                                                                                                                                |
| `undefined-label`                | error   | Every shortcut reference has a link definition.                                                                                                                                                               |
| `label-canonical`                | error   | A shortcut label that names a document is written as that document's id.                                                                                                                                      |
| `unused-definition`              | warning | A link definition that nothing references.                                                                                                                                                                    |
| `posture-belongs-to-frameworks`  | warning | Flags the words "compliant", "compliance", "certified" and "registered" near a framework reference. A policy maps its clauses to controls; what our standing is against a framework belongs in frameworks.md. |

<!-- END GENERATED: checks-policies -->
