# Capabilities

What the product offers its customers, and why.

**[→ Index](capabilities/_index.md)**

## What is a capability?

A single document per product surface — one per customer-visible area of the product — describing what it does, why it
exists, which services implement it, and where the detail lives.

A capability is a **hub**. It links to the epics that specify it, the feature files that test it, the services that
implement it and the NFRs that constrain it. It does not restate any of them.

## Why we use them

Functional detail lives in ADO epics, features and stories. What ADO cannot hold — without disturbing a work-item
hierarchy that exists for delivery, not documentation — is the layer *above* the epic: the holistic account of what the
product actually offers and why.

That gap is why nobody can answer "what does the product do?" from a single place today, and why the same context gets
reconstructed at the start of every significant piece of work.

## Scope

One document per **customer-visible surface**, not per epic and not per service. One capability typically spans several
services; one service often contributes to several capabilities.

**Capabilities link rather than restate.** The moment a capability document starts specifying behaviour, it has begun to
drift from the ADO items it should be pointing at — and a drifted capability is worse than none, because sessions will
trust it. If you are writing acceptance criteria, they belong in ADO.

Related but different:

* **Spec** — the per-feature application of standards to a concrete contract lives in the repository that owns the
  feature, alongside its OpenAPI document and feature files. Same central-vs-local rule as [ADRs](/adrs): cross-repo
  synthesis lives here, feature-level detail lives with the code.
* **[Service](/services)** — a thing we deploy. A capability is a thing a customer gets.
* **[Explanation](/explanations)** — how something works internally. A capability is what it does externally.

## Metadata

<!-- BEGIN GENERATED: schema-capabilities -->

| Field            | Req | Type   | Notes                                                                                        |
|------------------|-----|--------|----------------------------------------------------------------------------------------------|
| `id` †           | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                        |
| `tier` †         | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.         |
| `status` †       | ●   | enum   | Lifecycle of the capability, not of the services behind it.                                  |
| `owner` †        | ●   | string | A named person, never a team alias.                                                          |
| `tags` †         |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                             |
| `implemented-by` | ●   | list   | Service ids. A capability no service implements is a plan.                                   |
| `ado-epics`      |     | list   | ADO work item ids. Not resolvable by CI; recorded for humans and for the reverse harvest.    |
| `feature-files`  |     | list   | Repo-relative paths. Checked both ways: a missing path fails, an unclaimed file is reported. |
| `nfrs`           |     | list   | NFR ids — the targets this capability is held to.                                            |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `planned` · `building` · `live` · `deprecated`                      |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-capabilities -->

## Adding a capability

1. Copy [`_template.md`](capabilities/_template.md) to `<slug>.md`. Capabilities use slug ids — `cap-<name>`.
2. Write the *what* and the *why* in prose. Two or three paragraphs is usually enough.
3. Fill in `implemented-by`, `ado-epics` and `feature-files` — these are the links that make it a hub.
4. Resist the urge to explain how it works. Link to the services and explanations that already do.

**Conventions**

* **Hub, not specification.** If a section is longer than the list of links around it, ask whether it belongs in ADO.
* **Every feature file path is checked** by CI, in both directions — a path that doesn't exist fails, and a feature file
  claimed by no capability is reported.

## What CI checks

<!-- BEGIN GENERATED: checks-capabilities -->

| Check                       | Level   | What it verifies                                                                                                                                                                                                                                             |
|-----------------------------|---------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                                                                                                                                                                          |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                                                                                                                                                               |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                                                                                                                                                                            |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                                                                                                                                                                      |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                                                                                                                                                               |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                                                                                                                                                                                         |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                                                                                                                                                                      |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                                                                                                                                                                 |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                                                                                                                                                                   |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                                                                                                                                                                   |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                                                                                                                                                                                |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                                                                                                                                                                        |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                                                                                                                                                              |
| `h1`                        | error   | The document has an H1.                                                                                                                                                                                                                                      |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.                                                                                                                                                     |
| `required-section`          | error   | Every required section heading is present.                                                                                                                                                                                                                   |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                                                                                                                                                                               |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                                                                                                                                                              |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                                                                                                                                                                     |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists.                                                                                                                                                                                     |
| `unused-definition`         | warning | A link definition that nothing references.                                                                                                                                                                                                                   |
| `hub-not-specification`     | warning | Reports a capability whose prose exceeds a threshold relative to its link count. A capability that specifies behaviour has begun to drift from the ADO items it should point at, and a drifted capability is worse than none because sessions will trust it. |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                   | What it would verify                                                                                                     |
|------------------------|--------------------------------------------------------------------------------------------------------------------------|
| `feature-file-orphans` | Scheduled. Reports feature files in the code repositories claimed by no capability, and paths here that no longer exist. |

<!-- END GENERATED: checks-capabilities -->
