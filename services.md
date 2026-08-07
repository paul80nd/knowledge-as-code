# Services

The catalogue of deployable components that make up the platform.

**[→ Index](services/INDEX.md)**

## What is a service?

One document per independently deployable component — a web app, an API, an Azure Function, a CDN asset bundle, an
infrastructure stack. It records what the component is for, where its code lives, what it runs on, what it depends on,
what data it owns, and who is answerable for it.

## Why we use them

This is the **anchor** the rest of the wiki points at. NFRs apply to services. Controls apply to services. Capabilities
are implemented by services. FAQs affect services. None of those cross-references can be validated — or even written
consistently — without one canonical list of what a service is and what it is called.

It is also the answer to the question new contributors and AI sessions ask most often: *which of the seventeen
repositories does this thing live in, and what talks to it?*

## Scope

One document per **deployable unit**, not per repository and not per feature. A repository containing two independently
deployed apps gets two documents; a capability spanning six services gets a
[capability](/capabilities) document that links to all six.

A service document is **descriptive**: it mirrors what is actually deployed. It is not the place for:

* **How to deploy it** — that is a [process](/processes).
* **How to fix it when it breaks** — that is a [runbook](/runbooks).
* **Why it is shaped the way it is** — that is an [ADR](/adrs) or an [explanation](/explanations).
* **What it promises** — availability and latency targets are [NFRs](/nfrs).

Third-party systems we depend on are [integrations](/integrations), not services — the line is whether we deploy it.

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
| `depends-on`  |     | list   | Downward only — what this service calls. The reverse view is generated.              |
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

1. Copy [`template.md`](services/template.md) to `<slug>.md`. Services use a **slug id**, not a number —
   `svc-<name>` — because they have natural stable names.
2. Fill in the frontmatter. `depends-on` names other service ids; CI checks they resolve.
3. Record environments and URLs, the data stores it owns, and a named owner.
4. Keep it current. This is a descriptive document — a service catalogue that disagrees with the estate is worse than
   none, because everything else trusts it.

**Conventions**

* **Slug** — matches the repository or component name where one exists (`<component>` → `svc-<component>`).
* **Criticality** — `critical` means customer-facing failure; `important` means degraded service; `supporting` means
  internal impact only. It drives runbook and NFR prioritisation, so be honest rather than generous.
* **Dependencies point downward only.** Record what this service calls; the reverse view is generated.

## What CI checks

<!-- BEGIN GENERATED: checks-services -->

| Check                       | Level   | What it verifies                                                                                         |
|-----------------------------|---------|----------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                      |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                           |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                        |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                  |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                           |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                                     |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                  |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                             |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                               |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                               |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                            |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                    |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                          |
| `h1`                        | error   | The document has an H1.                                                                                  |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter. |
| `required-section`          | error   | Every required section heading is present.                                                               |
| `clause-ref`                | error   | A `pol-XXXX.CLAUSE` citation names a clause that exists.                                                 |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                           |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                          |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                 |
| `unused-definition`         | warning | A link definition that nothing references.                                                               |

<!-- END GENERATED: checks-services -->
