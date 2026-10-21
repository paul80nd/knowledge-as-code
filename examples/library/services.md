# Services

The catalogue of deployable components that make up the platform.

**[→ Index](services/_index.md)**

## What is a service?

One document per independently deployable component: a web app, an API, a serverless function, a CDN asset bundle. It
records what the component is for, where its code lives, what it runs on, what it depends on, and what data it owns.

## Why we use them

Services are the **anchor** the rest of the corpus points at. An NFR, a control and a fix each name the service they
concern, and an offering names the services that implement it. Nobody can check those references, or write them the
same way twice, without one canonical list of what a service is and what it is called.

A new contributor or an AI session asks one question more than any other: *which repository does this thing live in, and
what talks to it?* A service document answers it.

## Scope

One document per **deployable unit**, not per repository and not per feature. A repository containing three
independently deployed apps gets three documents. An offering spanning six services gets an
[offering](offerings.md) document that links to all six.

**Where one pipeline publishes several artefacts that differ only in the content they carry, that pipeline is the
unit.** A feed of six packages, built from six folders by one workflow, is one document. Its artefacts are one build
run over different content, so splitting it would repeat one repository, one owner and one platform six times. Where
each artefact is built from source of its own, they are separate services however few pipelines publish them.

A service document is **descriptive**: it mirrors what is actually deployed. It is not the place for:

* **How to deploy it.** That is a [process](processes.md).
* **How to fix it when it breaks.** That is a runbook.
* **Why it is shaped the way it is.** That is an [ADR](adrs.md) or an explanation.
* **What it promises.** Availability and latency targets are NFRs.

A third-party system we depend on is an integration. The line is whether we deploy it. Infrastructure-as-code stays out
of the catalogue as well, because it deploys services.

## Level of detail and cadence

One record describes one deployable component as it runs today. A fact the component's own repository already states,
such as a setting or a list of endpoints, stays there, and the record links to it.

A record changes when the component's repositories, environments or dependencies change. New records arrive with each
new component.

## Metadata

<!-- BEGIN GENERATED: schema-services -->

| Field               | Value                                                   | Notes                                                                                                |
|---------------------|---------------------------------------------------------|------------------------------------------------------------------------------------------------------|
| `tier` *†           | `descriptive`                                           | The record's trust level, fixed for the type and checked against the folder.                         |
| `status` *†         | `live` `building` `deprecated` `retired`                | Where the service is in its life. In force at `live`.                                                |
| `component-type` *  | `api` `asset` `website` `worker`                        | The sort of component this is, which `platform` does not say.                                        |
| `repos` *           | list                                                    | Repositories a change to this service is made in.                                                    |
| `platform` *        | `azure-function` `dotnet` `mixed` `static` `typescript` | What it is built on. Drives which standards apply.                                                   |
| `criticality` *     | `critical` `important` `supporting`                     | Judged by what a customer experiences when it is unavailable.                                        |
| `monitoring-output` | `alert` `ticket` `log` `none`                           | How a live problem with this service reaches a person. Required when `status in [live, deprecated]`. |
| `interfaces`        | list                                                    | The interfaces this service offers, with the contract for each.                                      |
| `depends-on`        | list                                                    | What this service calls, downward only.                                                              |
| `data-stores`       | list                                                    | Data ids this service owns or reads.                                                                 |
| `nfrs`              | list                                                    | Ids of the NFRs this service must meet.                                                              |
| `facets`            | list                                                    | Slices the catalogue: one exposure value, then any traits.                                           |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-services -->

## Adding a service

Copy [`_template.md`](services/_template.md). Its guidance block states how to write the id, each field and where a
claim came from. The prompt under each section states how to write that section.

### Deriving the `platform` enum

`platform` says what a service is **built on**, because that is what decides which standards apply to it. It says
neither what deploys it nor what sort of component it is. A service deployed by Terraform is not a Terraform service,
and infrastructure-as-code is out of this catalogue entirely. `component-type` says the sort, so a value here naming a
sort belongs in the other list.

**Derive the values from the estate you have, then close the list.** Walk the deployables, group them by the runtime and
framework a contributor would need to know, and let that be the range. Do not inherit a list from elsewhere. A value no
service can carry reaches an author at exactly the moment they are least able to judge it.

The list lives under `enums:` in [`.corpus.yaml`](.corpus.yaml), which is this catalogue's own file.
`.schema/services.yaml` declares `values: $corpus.platform` and states no values, so one schema can sit above several
catalogues and stand behind the list each of them wrote. `corpus-enum-undeclared` reports a record that arrives before
the list does.

**`mixed` means several of the other values in one deployable, and never *none of the above*.** A service released as
a function and a workflow app together carries it. Where the estate grows something no value names, add the value.

### Deriving the `component-type` vocabulary

`component-type` says what sort of thing a service is: an API, a website, a worker, a command line tool. It is what a
reader uses to tell two deployables apart before opening either. Backstage, OpsLevel and Cortex each require this word
and each leave the vocabulary to the adopter, and so does this type.

**Derive the values the way you derived `platform`, and keep the two lists apart.** A value belongs here where it
answers "what is it", and in `platform` where it answers "what is it built on". `dotnet-api` fails that test: `dotnet`
is the platform and `api` is the sort.

The list lives under `enums:` in [`.corpus.yaml`](.corpus.yaml), beside `platform`. `corpus-enum-undeclared` reports a
record that arrives before the list does.

This estate deploys APIs, websites and workers, and it serves one asset surface, so the list holds `api`, `asset`,
`website` and `worker`. `asset` covers files served over HTTP with no runtime of their own.

### Deriving the facet vocabulary

A **facet** slices the catalogue, so it earns its place by grouping: one nothing else carries has failed at the only
thing it was for. `facets` declares `min-records: 2` and CI warns on a value a single service carries. The **method**
below is portable and worth keeping. The handful of values an estate ends up with is not: replace them with your own.

One **exposure**, then zero or more **traits**:

| Facet          | Means                                                                                          |
|----------------|------------------------------------------------------------------------------------------------|
| `public`       | Has an inbound surface a customer or member of the public can reach. At most one of these two. |
| `internal`     | Inbound only from staff or from other services in this catalogue.                              |
| `event-driven` | Publishes to or consumes from the message bus.                                                 |
| `scheduled`    | Runs on a timer as well as, or instead of, on request.                                         |

Two rules keep the vocabulary small enough to browse, and they are the transferable part:

1. **Exposure describes the inbound surface.** A staff portal on the public internet is `internal`, because only staff
   are meant to reach it. A service reached by nobody carries neither value, and a command line tool somebody installs
   and runs is one.
2. **Never restate another field.** There is no `cdn` facet, because `platform: static` says it. There is no `monorepo`
   facet, because `repos` says it. A value that duplicates a field can only ever disagree with it.

Membership stays a judgement. The floor holds the shape of the vocabulary (that every value in it groups), and no
declaration anywhere says which words this estate chose. That is the corpus's to decide and this page's to record.
`platform` states its list in `.corpus.yaml` instead, because a value nothing recognises there detaches a service from
its standards. A facet nobody else carries only costs a grouping, and `min-records` reports it.

### What a tag is for instead

A **tag** brings a reader's word to a service that does not use it. `payments` reaches Reservations, `renewals` reaches
Notices, `legacy` reaches Lending. Each is carried by one document, which is exactly right: a searcher arriving with
that word wanted that service. Tags are free-form, and CI holds them to nothing beyond their shape.

So the two tests pull opposite ways, and that is the whole reason for two fields. A word matching a single service fails
as a facet and succeeds as a tag. A word several services share divides the catalogue and belongs in `facets`, where the
floor holds it to doing so. Judged as one field, the second kind reads as a vocabulary that never converged, and the
words worth keeping are the ones thrown away.

## What CI checks

<!-- BEGIN GENERATED: checks-services -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                         | Level   | What it verifies                                                                                      |
|-------------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `entry-shape / entry-key`     | error   | An object field, and each entry of an object list, carries the keys the field declares and no others. |
| `min-records`                 | warning | A value in a grouping field is carried by at least as many records as the schema asks for.            |
| `ref-resolves`                | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `reciprocal`                  | error   | A reciprocal field and its counterpart agree in both directions.                                      |
| `dependency-cycle`            | warning | A cycle in the dependency graph these records form, naming every record the loop runs through.        |
| `dependency-criticality`      | warning | No record is graded above the criticality of anything it depends on.                                  |
| `mirrors-repo-links`          | warning | Every `repos` entry is linked under `Where it lives`, and every repository linked there is stated.    |
| `critical-service-is-alerted` | warning | A service graded `critical` emits something when it fails.                                            |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                  | What it would verify                                                         |
|-----------------------|------------------------------------------------------------------------------|
| `drift-against-repos` | Every service has a repository, and every repository has a service document. |

<!-- END GENERATED: checks-services -->
