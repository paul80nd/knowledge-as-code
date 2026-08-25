# Policies

The engineering commitments we hold ourselves to: the *what* and the *why*.

**[→ Index](policies/_index.md)**

## What is a policy?

A policy states what we commit to and why. It is durable, it is written at the level of principle, and it rarely names a
technology: *"secrets are never stored in source control"*, *"quality checks are gates, not advisories"*, *"changes to
non-development environments go through the pipeline"*.

Policies sit at the top of the normative hierarchy. A policy says what we hold true, a [standard](standards.md) says
what to do about it, a [control](controls.md) says how we know it happened, and a process says how to do it.

## Why we use them

Standards change with the stack and policies do not. Keep the two apart, and replacing a web framework never reopens the
question of where secrets live.

Policies also give the standards somewhere to point. A standard citing no ADR and no policy has no provenance, and that
usually means it is guidance in disguise or a decision nobody made.

Each clause maps to external frameworks in the `Alignment` column of the clause table, most often to **ISO/IEC 27001:
2022** Annex A. The mapping sits on the clause because a policy meets any one control through a single obligation. Leave
the cell empty where no genuine mapping exists, since an empty cell tells the reader more than an invented one.

A framework can bind us by certification, bind us because we chose to hold ourselves to it, or serve as something we
borrow from. [Frameworks](frameworks.md) records which of the three applies, and no policy repeats it. Our standing
changes on a schedule of its own, so a policy that carried it would need correcting alongside twenty others.

## Scope

**The test:** would this still be true after replacing the entire technology estate? If yes, it is a policy. If it names
a tool, a framework or a protocol, it is a [standard](standards.md).

| Policy                                          | Standard                                                               |
|-------------------------------------------------|------------------------------------------------------------------------|
| "Secrets are never stored in source control."   | "Services **MUST** read secrets from Key Vault via workload identity." |
| "Quality checks are gates that fail the build." | "ESLint **MUST** run with `--max-warnings 0`."                         |

A policy commits and a [control](controls.md) verifies, so the two are never the same document. An [ADR](adrs.md)
records one decision and the alternatives weighed against it, where a policy states a position we hold across all such
decisions.

## Categories

Every policy carries a `category`: **security**, **delivery**, **operations** or **governance**. It answers *why this
policy exists* (the broad area of the commitment), where `tags` answer *what topics it touches*. Two questions need two
fields: a secrets policy is `category: security` and `tags: [credentials, key-management, secrets]`.

The set is closed and deliberately small. Four categories cut the policies into groups worth navigating. A fifth would
have to earn its place by making one of these too crowded to scan. That pressure is easier to judge once there are
enough policies to feel it.

Category is metadata and `policies/` stays flat. Recategorising a policy is then a one-line edit. A file move would
rewrite every document linking to it. The awkward calls are the ones most likely to be revisited, and accessibility
under governance is the clearest of them.

## Metadata

<!-- BEGIN GENERATED: schema-policies -->

| Field         | Value                                           | Notes                                                                               |
|---------------|-------------------------------------------------|-------------------------------------------------------------------------------------|
| `id` *†       | string                                          | Stable, unique across the corpus, never reused. Format set by the type.             |
| `tier` *†     | `normative`                                     | Fixed for the type. A trust signal for the reader. CI checks it matches the folder. |
| `status` *†   | `draft` `active` `retired`                      | `draft` until agreed. `retired` rather than deleted.                                |
| `owner` *†    | string                                          | A named person, never a team alias.                                                 |
| `tags` †      | list                                            | Free-form, lowercase, hyphenated. Used for cross-cutting search.                    |
| `category` *  | `security` `delivery` `operations` `governance` | The broad area the commitment belongs to. Controlled, and deliberately few.         |
| `aligns-with` | list                                            | e.g. `ISO27001:2022 A.8.25`. The document-level roll-up of what its clauses map to. |
| `review-by` * | date                                            | Quoted. Annual is usually right for a policy.                                       |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-policies -->

## Adding a policy

1. Apply the test above. Most things that feel like policies are standards.
2. Choose a four-character mnemonic for the policy's *concept*: `VURM` for vulnerability remediation, `PIPE` for
   pipeline-to-production. Start it with the same letter as the slug, so the folder still reads alphabetically.
3. Copy [`_template.md`](policies/_template.md) to `mnem-kebab-slug.md`, lower-case, and set `id` to `pol-MNEM`,
   upper-case. The H1 states the commitment in plain words. The identity line beneath it carries the id, written
   ``` `Policy: pol-MNEM` `DRAFT` ```, and CI checks it against the frontmatter.
4. Set `category` to whichever of the four the commitment belongs to. If two fit, pick the one a reader looking for this
   policy would try first. If none does, that is a taxonomy question, and not a fifth category invented in passing.
5. State the scope it binds, then the commitment itself as clauses: one obligation per row, each with a short upper-case
   id, ordered **MUST**, **MUST NOT**, SHOULD, COULD. Write any explicit exceptions beneath the clauses, where a reader
   meets them before relying on the rule.
6. Map clauses to framework controls in the `Alignment` column where a genuine mapping exists, and roll the references
   up into `aligns-with`. A framework cited for the first time gets an entry in [Frameworks](frameworks.md). Decide its
   posture there before citing it here.
7. Set `review-by`. Policies change rarely, so an annual review is usually right.

**Conventions**

* **A policy does not state our standing against a framework.** A policy maps its clauses to controls and says nothing
  about what that mapping is worth. Leave out "compliant", "certified" and "registered". [Frameworks](frameworks.md)
  holds the standing, and holds it once.
* **A policy names no implementers.** The reference points up: a standard declares the policy it puts into practice, and
  a policy says nothing about what implements it. A downstream corpus inherits these policies and writes its own
  standards against them, so nobody writing here can know the full set of implementers. A policy that nothing in *this*
  corpus implements is the normal state rather than a gap to be explained.
* **A clause is the unit anything else cites.** A standard, a control or a deviation names the single obligation it
  answers, in the form [Metadata][referring] sets out. Clause ids are immutable for the same reason policy ids are:
  removing or renaming one breaks every citation of it.
* **A policy id is immutable once the policy is active.** Rewrite the title, sharpen the commitments, correct the scope.
  The id does not move. Standards, controls and processes cite policies by id. Reassign a mnemonic and every one of
  those citations tells the reader something untrue: the reference still resolves, so nothing fails and no check fires.

  This is why the mnemonic comes from the concept rather than the wording. A policy whose *meaning* has changed enough
  to invalidate its mnemonic has not been edited. It has been replaced. **Retire the old policy and write a new one**,
  so the record shows the position we used to hold, the position we hold now, and that the two differ. Retirement is
  cheap and keeps the history honest. An id that quietly means something new leaves every citation of it pointing at a
  position we abandoned.

## What CI checks

<!-- BEGIN GENERATED: checks-policies -->

| Check                            | Level   | What it verifies                                                                                                  |
|----------------------------------|---------|-------------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`             | error   | Frontmatter is present and is a valid YAML mapping.                                                               |
| `unknown-key`                    | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                    |
| `key-order`                      | error   | Key order is a topological extension of the schema's field order.                                                 |
| `required-field`                 | error   | Required and conditionally-required fields are present.                                                           |
| `bare-key`                       | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                    |
| `date-quoted / date-format`      | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                            |
| `enum`                           | error   | Enum values are in range and lowercase.                                                                           |
| `field-pattern`                  | error   | Values match the pattern their field declares (e.g. `tags`).                                                      |
| `list-order`                     | warning | List entries read in alphabetical order, with numbers compared as numbers.                                        |
| `tier-matches-type`              | error   | `tier` matches the tier the type declares.                                                                        |
| `id`                             | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename.   |
| `id-unique`                      | error   | `id` is unique across the whole corpus.                                                                           |
| `filename / slug-length`         | error   | Filename matches the pattern. The slug is within 30 characters.                                                   |
| `h1`                             | error   | The document has an H1.                                                                                           |
| `identity`                       | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.          |
| `sections`                       | error   | Every required section heading is present, and no declared section is left as a bare heading.                     |
| `placeholder-left`               | error   | No `{{…}}` from the template is left unfilled, outside code.                                                      |
| `clauses`                        | error   | The clause section is a table of `Id \| Clause` rows, each id a code span and each clause opening with its modal. |
| `clause-order / clause-compound` | warning | Clause rows are grouped by binding level, and each carries a single obligation.                                   |
| `part-id-unique / part-ref`      | error   | No two parts of a record share an address, and a `record-id.part` citation reaches the part it names.             |
| `link-resolves`                  | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.                |
| `undefined-label`                | error   | Every shortcut reference has a link definition.                                                                   |
| `label-canonical`                | error   | A shortcut label that names a document is written as that document's id.                                          |
| `unused-definition`              | warning | A link definition that nothing references.                                                                        |
| `posture-belongs-to-frameworks`  | warning | "compliant", "certified" or "registered" near a framework reference. Standing belongs in `frameworks.md`.         |

<!-- END GENERATED: checks-policies -->

[referring]: https://paul80nd.github.io/knowledge-as-code/framework/metadata/#referring-to-an-id
