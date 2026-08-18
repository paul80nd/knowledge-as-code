# Capabilities

What the product offers its customers, and why.

**[→ Index](capabilities/_index.md)**

## What is a capability?

One document per customer-visible surface of the product. It records what the surface does, why it exists, which
services implement it, and where the detail lives.

A capability is a **hub**. It links to the epics that specify it, the feature files that test it, the services that
implement it and the NFRs that constrain it. It does not restate any of them.

## Why we use them

Functional detail lives in ADO epics, features and stories. ADO holds nothing above the epic: no account of what the
product offers a customer and why. It could not hold one without disturbing a work-item hierarchy that exists to run
delivery.

Without that account, nobody can answer "what does the product do?" from one place, and everyone starting a significant
piece of work rebuilds the same context.

## Scope

One document per **customer-visible surface**, not per epic and not per service. One capability typically spans several
services; one service often contributes to several capabilities.

**Capabilities link rather than restate.** A capability that specifies behaviour has begun to drift from the ADO items
it should point at. The next session to read it will trust it anyway, which makes a drifted capability worse than none.
Acceptance criteria go in ADO.

Related but different:

* **Spec** — how standards apply to one concrete contract. It lives in the repository that owns the feature, beside its
  OpenAPI document and feature files. [ADRs](/adrs) split the same way: cross-repo synthesis here, feature-level detail
  with the code.
* **[Service](/services)** — a thing we deploy. A capability is a thing a customer gets.
* **[Explanation](/explanations)** — how something works internally. A capability is what it does externally.

## Metadata

<!-- BEGIN GENERATED: schema-capabilities -->

| Field              | Value                                    | Notes                                                                                     |
|--------------------|------------------------------------------|-------------------------------------------------------------------------------------------|
| `id` *†            | string                                   | Stable, unique across the corpus, never reused. Format set by the type.                   |
| `tier` *†          | `descriptive`                            | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.      |
| `status` *†        | `planned` `building` `live` `deprecated` | Lifecycle of the capability, not of the services behind it.                               |
| `owner` *†         | string                                   | A named person, never a team alias.                                                       |
| `tags` †           | list                                     | Free-form, lowercase, hyphenated. Used for cross-cutting search.                          |
| `implemented-by` * | list                                     | Service ids. A capability no service implements is a plan.                                |
| `ado-epics`        | list                                     | ADO work item ids. Not resolvable by CI; recorded for humans and for the reverse harvest. |
| `feature-files`    | list                                     | Repo-relative paths. Nothing resolves them today.                                         |
| `nfrs`             | list                                     | NFR ids — the targets this capability is held to.                                         |

\* Field is required  
† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-capabilities -->

## Adding a capability

1. Copy [`_template.md`](capabilities/_template.md) to `<slug>.md`. Capability ids are slugs: `cap-<name>`.
2. Write the *what* and the *why* in prose. Two or three paragraphs is usually enough.
3. Fill in `implemented-by`, `ado-epics` and `feature-files`. Those links make it a hub.
4. Do not explain how it works. Link to the services and explanations that already do.

**Conventions**

* **Hub, not specification.** `hub-not-specification` weighs the whole document against its outbound links, at roughly
  forty words each, so a capability that grows a section of its own trips it. Where a section runs longer than the links
  around it, ask whether the detail belongs in ADO.
* **Keep the feature file paths honest yourself.** Nothing resolves them: the field holds plain strings, so
  `ref-resolves` never sees it, and `feature-file-orphans` is declared and does not run. A path that goes stale here
  goes stale quietly.

## What CI checks

<!-- BEGIN GENERATED: checks-capabilities -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has — `YYYY-MM-DD`.                                         |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `hub-not-specification`     | warning | A capability's prose stays proportionate to its links — a hub, not a specification.                             |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                   | What it would verify                                                                                                     |
|------------------------|--------------------------------------------------------------------------------------------------------------------------|
| `feature-file-orphans` | Scheduled. Reports feature files in the code repositories claimed by no capability, and paths here that no longer exist. |

<!-- END GENERATED: checks-capabilities -->
