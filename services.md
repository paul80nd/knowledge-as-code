# Services

The catalogue of deployable components that make up the platform.

**[→ Index](services/_index.md)**

> **The records in `services/` are an example estate, not your estate.** They describe a fictional public-library
> consortium, and they give this type something to demonstrate: a dependency graph, a criticality gradient, and the
> awkward cases that shaped the schema. **Delete them before you add your first real service.** The two sections below
> that derive a convention from that estate — the `platform` enum and the facet vocabulary — show a method for reaching
> your own values.

## What is a service?

One document per independently deployable component — a web app, an API, a function app, a CDN asset bundle. It records
what the component is for, where its code lives, what it runs on, what it depends on, and what data it owns.

## Why we use them

Services are the **anchor** the rest of the wiki points at. An NFR, a control and an FAQ each name the service they
concern, and a capability names the services that implement it. Nobody can check those references, or write them the
same way twice, without one canonical list of what a service is and what it is called.

A new contributor or an AI session asks one question more than any other: *which repository does this thing live in, and
what talks to it?* A service document answers it.

## Scope

One document per **deployable unit**, not per repository and not per feature. A repository containing three
independently deployed apps gets three documents; a capability spanning six services gets a
[capability](/capabilities) document that links to all six.

A service document is **descriptive**: it mirrors what is actually deployed. It is not the place for:

* **How to deploy it** — that is a [process](/processes).
* **How to fix it when it breaks** — that is a runbook.
* **Why it is shaped the way it is** — that is an [ADR](/adrs) or an [explanation](/explanations).
* **What it promises** — availability and latency targets are NFRs.

A third-party system we depend on is an integration; the line is whether we deploy it. Infrastructure-as-code stays out
of the catalogue as well, because it deploys services.

## Metadata

<!-- BEGIN GENERATED: schema-services -->

| Field         | Req | Type   | Notes                                                                                |
|---------------|-----|--------|--------------------------------------------------------------------------------------|
| `id` †        | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                |
| `tier` †      | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder. |
| `status` †    | ●   | enum   | Where the service is in its life.                                                    |
| `owner` †     | ●   | string | A named person, never a team alias.                                                  |
| `tags` †      |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                     |
| `repo`        | ●   | string | Where the code lives.                                                                |
| `platform`    | ●   | enum   | What it is built on. Drives which standards apply.                                   |
| `criticality` | ●   | enum   | Judged by what a customer experiences when it is unavailable.                        |
| `depends-on`  |     | list   | Downward only — what this service calls.                                             |
| `data-stores` |     | list   | Data ids this service owns or reads.                                                 |
| `facets`      |     | list   | Slices the catalogue — one exposure, then any traits. Each value groups services.    |

**Enum values**

| Field         | Values                                                                                           |
|---------------|--------------------------------------------------------------------------------------------------|
| `tier`        | `decided` · `normative` · `descriptive` · `procedural` · `observed`                              |
| `status`      | `live` · `building` · `deprecated` · `retired`                                                   |
| `platform`    | `dotnet-web` · `dotnet-api` · `azure-function` · `static` · `typescript` · `terraform` · `mixed` |
| `criticality` | `critical` · `important` · `supporting`                                                          |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-services -->

## Adding a service

1. Copy [`_template.md`](services/_template.md) to `<slug>.md`. Services take a **slug id**, `svc-<name>`, because they
   have natural stable names.
2. Fill in the frontmatter. `depends-on` names other service ids; the [index](services/_index.md) is where to find them.
3. Record environments and URLs, and the data stores it owns.
4. Keep it current. Everything else in the wiki trusts this catalogue, so an entry that has drifted from the estate
   sends the next reader to the wrong repository.

**Conventions**

* **The id names the deployable, not the repository.** A repository shipping three independently deployed apps yields
  three ids, and none of them carries the repository's name. `repo` carries it, and that is where to look for the code.
  Contributors get this wrong more often than anything else here. A repository list is the easiest list to reach for,
  and it is the wrong one.
* **`repo` takes one value, and sometimes one value is not enough.** It names the repository you work in when you change
  *this service*. Where the content a service serves comes from elsewhere, the field cannot say so and the body must —
  an asset surface can be filled by two pipelines, or by another service at runtime.
* **Criticality** — `critical` means a customer-facing failure, `important` means degraded service, and `supporting`
  means internal impact only. Criticality drives runbook and NFR prioritisation, so grade a service honestly. A service
  graded above one of its own dependencies is not automatically wrong, but it is worth defending in the record.
* **Dependencies point downward only.** Record what this service calls. Nothing generates a reverse view today, so
  anyone who writes a "depended on by" list maintains it by hand. Assume such a list is stale unless it says otherwise.
* **`depends-on` records calls, not messages.** An edge means this service is configured to reach that one — a URL in
  its application settings, or a route pointing at it. Publish/subscribe coupling over a message bus is deliberately not
  an edge, because it is not a call and the publisher does not know its consumers. The graph therefore shows an
  event-driven service as unconnected when it is not, so its topics and queues sit in its own `## Operational notes`.
* **A bare field means "nothing in this catalogue", not "nothing at all".** A service that calls a legacy system, a
  third-party integration or anything else outside this catalogue carries a bare `depends-on`. None of those is an edge.
  The same holds for `data-stores`. The field records what is here.
* **Sourcing.** Say where a claim came from. "Taken from the application settings the infrastructure declares" weighs
  differently from "the README says", and the reader needs to know which one they have. Where you cannot establish
  something, write it down as an open question. Nobody reading later can tell your guess from a fact.

### Deriving the `platform` enum

`platform` says what a service is **built on**, because that is what decides which standards apply to it. It does not
say what deploys it: a service deployed by Terraform is not a Terraform service, and infrastructure-as-code is out of
this catalogue entirely.

**Derive the values from the estate you have, then close the list.** Walk the deployables, group them by the runtime and
framework a contributor would need to know, and let that be the enum. Do not inherit a list from elsewhere. A value no
service can carry reaches an author at exactly the moment they are least able to judge it. A value the estate needs and
the enum lacks sends someone to `mixed` who does not belong there.

The values in `.schema/services.yaml` are the example estate's, and its services exercise all but one. Nothing can carry
`terraform`, which stays in the list so that the case is visible.

### Deriving the facet vocabulary

A **facet** slices the catalogue, so it earns its place by grouping: one nothing else carries has failed at the only
thing it was for. `facets` declares `min-records: 2` and CI warns on a value a single service carries. The **method**
below is portable and worth keeping. The handful of values an estate ends up with is not: replace them with your own.

One **exposure**, then zero or more **traits**:

| Facet          | Means                                                                                          |
|----------------|------------------------------------------------------------------------------------------------|
| `public`       | Has an inbound surface a customer or member of the public can reach. Exactly one of these two. |
| `internal`     | Inbound only from staff or from other services in this catalogue.                              |
| `event-driven` | Publishes to or consumes from the message bus.                                                 |
| `scheduled`    | Runs on a timer as well as, or instead of, on request.                                         |

Two rules keep the vocabulary small enough to browse, and they are the transferable part:

1. **Exposure describes the inbound surface.** A staff portal on the public internet is `internal`, because only staff
   are meant to reach it.
2. **Never restate another field.** There is no `cdn` facet, because `platform: static` says it; no `monorepo` facet,
   because `repo` says it. A value that duplicates a field can only ever disagree with it.

Membership stays a judgement. The floor holds the shape of the vocabulary — that every value in it groups — and no
declaration anywhere says which words this estate chose. That is the corpus's to decide and this page's to record.

### What a tag is for instead

A **tag** brings a reader's word to a service that does not use it. `payments` reaches Reservations, `renewals` reaches
Notices, `legacy` reaches Lending — each carried by one document, which is exactly right: a searcher arriving with that
word wanted that service. Tags are free-form, and CI holds them to nothing beyond their shape.

So the two tests pull opposite ways, and that is the whole reason for two fields. A word matching a single service fails
as a facet and succeeds as a tag; a word several services share divides the catalogue and belongs in `facets`, where the
floor holds it to doing so. Judged as one field, the second kind reads as a vocabulary that never converged, and the
words worth keeping are the ones thrown away.

## What CI checks

<!-- BEGIN GENERATED: checks-services -->

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
| `min-records`               | warning | A value in a grouping field is carried by at least as many records as the schema asks for.                      |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                           |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists.                                        |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                   | What it would verify                                                           |
|------------------------|--------------------------------------------------------------------------------|
| `no-dependency-cycles` | A cycle in the dependency graph is reported, not failed — some are legitimate. |
| `drift-against-repos`  | The catalogue against the real repository list, in both directions.            |

<!-- END GENERATED: checks-services -->
