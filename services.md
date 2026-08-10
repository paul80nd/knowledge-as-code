# Services

The catalogue of deployable components that make up the platform.

**[→ Index](services/_index.md)**

> **The records in `services/` are an example estate, not your estate.** They describe a fictional public-library
> consortium, and they are here so this type has something to demonstrate — a dependency graph, a criticality
> gradient, and the awkward cases the schema was shaped by. **Delete them before you add your first real service.**
> The two conventions marked below as *derived from the estate* — the `platform` enum and the tag vocabulary — are
> illustrations of a method, not defaults to inherit.

## What is a service?

One document per independently deployable component — a web app, an API, a function app, a CDN asset bundle. It records
what the component is for, where its code lives, what it runs on, what it depends on, and what data it owns.

## Why we use them

This is the **anchor** the rest of the wiki points at. NFRs apply to services. Controls apply to services. Capabilities
are implemented by services. FAQs affect services. None of those cross-references can be validated — or even written
consistently — without one canonical list of what a service is and what it is called.

It is also the answer to the question new contributors and AI sessions ask most often: *which repository does this thing
live in, and what talks to it?*

## Scope

One document per **deployable unit**, not per repository and not per feature. A repository containing three
independently deployed apps gets three documents; a capability spanning six services gets a
[capability](/capabilities) document that links to all six.

A service document is **descriptive**: it mirrors what is actually deployed. It is not the place for:

* **How to deploy it** — that is a [process](/processes).
* **How to fix it when it breaks** — that is a runbook.
* **Why it is shaped the way it is** — that is an [ADR](/adrs) or an [explanation](/explanations).
* **What it promises** — availability and latency targets are NFRs.

Third-party systems we depend on are integrations, not services — the line is whether we deploy it.
Infrastructure-as-code is not a service either: it deploys services rather than being one.

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

1. Copy [`_template.md`](services/_template.md) to `<slug>.md`. Services use a **slug id**, not a number —
   `svc-<name>` — because they have natural stable names.
2. Fill in the frontmatter. `depends-on` names other service ids; the [index](services/_index.md) is where to find them.
3. Record environments and URLs, and the data stores it owns.
4. Keep it current. This is a descriptive document — a service catalogue that disagrees with the estate is worse than
   none, because everything else trusts it.

**Conventions**

* **The id names the deployable, not the repository.** A repository shipping three independently deployed apps yields
  three ids, and the repository's name is in none of them — it is carried by `repo`, which is where to look for the
  code. This is the single most common thing to get wrong, because a repository list is usually the easiest thing to
  hand and it is the wrong list.
* **`repo` takes one value, and sometimes one value is not enough.** It names the repository a change to *this service*
  is made in. Where the content a service serves comes from elsewhere — an asset surface filled by two pipelines, or by
  another service at runtime — the field cannot say so and the body must.
* **Criticality** — `critical` means a customer-facing failure; `important` means degraded service; `supporting`
  means internal impact only. It drives runbook and NFR prioritisation, so be honest rather than generous. A service
  graded above one of its own dependencies is not automatically wrong, but it is worth defending in the record.
* **Dependencies point downward only.** Record what this service calls. There is no generated reverse view today, so a
  "depended on by" list is maintained by hand and should be assumed stale unless it says otherwise.
* **`depends-on` records calls, not messages.** An edge means this service is configured to reach that one — a URL in
  its application settings, or a route pointing at it. Publish/subscribe coupling over a message bus is deliberately
  **not** an edge, because it is not a call and the publisher does not know its consumers. So the event-driven services
  look unconnected in the graph while being anything but; their topics and queues are recorded in their own
  `## Operational notes` instead.
* **A bare field means "nothing in this catalogue", not "nothing at all".** A service that calls a legacy system, a
  third-party integration or anything else outside this catalogue carries a bare `depends-on`, because none of those is
  an edge. The same holds for `data-stores`. The field records what is here.
* **Sourcing.** Say where a claim came from — "taken from the application settings the infrastructure declares"
  weighs differently from "the README says", and a reader deserves to know which they have. Park what is not established
  as an open question rather than guessing; an unanswered question is cheap and a wrong answer is not.

### Deriving the `platform` enum

`platform` says what a service is **built on**, because that is what decides which standards apply to it. It does not
say what deploys it: a service deployed by Terraform is not a Terraform service, and infrastructure-as-code is out of
this catalogue entirely.

**Derive the values from the estate you have, then close the list.** Walk the deployables, group them by the runtime and
framework a contributor would need to know, and let that be the enum. Do not inherit a list from elsewhere: a value
nobody can use is offered to an author at exactly the moment they are least able to judge it, and a value the estate
needs but the enum lacks sends someone to `mixed` who should not be there.

The values shipped in `.schema/services.yaml` are the example estate's, and the example estate exercises all but one of
them — `terraform` is the value nothing can carry, kept here deliberately so that the case is visible.

### Deriving the tag vocabulary

The **method** below is portable and worth keeping. The **seven-or-so tags** any given estate ends up with are not —
replace them.

One **exposure** tag, then zero or more **traits**:

| Tag            | Means                                                                                          |
|----------------|------------------------------------------------------------------------------------------------|
| `public`       | Has an inbound surface a customer or member of the public can reach. Exactly one of these two. |
| `internal`     | Inbound only from staff or from other services in this catalogue.                              |
| `event-driven` | Publishes to or consumes from the message bus.                                                 |
| `scheduled`    | Runs on a timer as well as, or instead of, on request.                                         |

Three rules make the vocabulary small enough to be searchable, and they are the transferable part:

1. **Exposure is about the inbound surface, not the firewall.** A staff portal on the public internet is `internal`,
   because only staff are meant to reach it.
2. **Never restate another field.** There is no `cdn` tag, because `platform: static` says it; no `monorepo` tag,
   because `repo` says it. A tag that duplicates a field can only ever disagree with it.
3. **A tag used exactly once belongs in prose.** The example estate considered `email`, `payments` and
   `legacy-facing` and dropped all three for this reason — each would have described a single service, which is not a
   search term, it is a sentence. Those facts are recorded in the bodies of the services concerned. In a larger estate
   the same three might each earn their place; that is a judgement to make against your own catalogue, not to inherit
   from this one.

Nothing enforces the vocabulary. A `values:` list is read from an `enum` field and nowhere else, and one written on
`tags` is rejected when the schema loads rather than quietly ignored — a vocabulary the validator cannot apply does not
get to look like one it can. It is prose here for that reason.

## What CI checks

<!-- BEGIN GENERATED: checks-services -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                                            |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                           |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `required-section`          | error   | Every required section heading is present.                                                                      |
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
