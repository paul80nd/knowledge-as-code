# Landscape review (#1160): developer portals and docs-as-code catalogues

Family: Backstage (Software Catalog, `catalog-info.yaml`, TechDocs, MCP actions), Cortex, OpsLevel, and Port in brief.

Every web source was read on **2026-10-27** unless stated. Claims about kac come from the files in this repository,
cited by path. "(inferred)" marks a conclusion I drew and did not read.

## Where kac already stands relative to this family

kac's `services` type names Backstage, OpsLevel `opslevel.yml` and the Cortex entity descriptor as its prior art. Its
`divergence` already states the main differences for a *service* record: a document with required prose against a row
of metadata, `status`/`criticality` against lifecycle/tier, `monitoring-output` against a paging tool's opaque id, and
`interfaces` pointing at a contract against Backstage's `spec.definition` embedding it
(`.schema/services.yaml`, `lineage:` block). This review does not redo that. It
covers what lies beyond the service record: the rest of each product's model, validation, scorecards, documentation,
agent access and distribution.

`docs/compared.md` does not list any developer portal, although the services lineage cites three of them
(`docs/compared.md`).

---

## 1. Backstage

### Problem and audience

The catalog is "a centralized system that keeps track of ownership and metadata for all the software in your
ecosystem". It serves teams seeing what they own, and anyone finding software and its owner
(<https://backstage.io/docs/features/software-catalog/>). Open source, self-hosted.

### Data model

* **Unit.** An entity: `apiVersion`, `kind`, `metadata` (required `name`, optional `namespace`, `title`, `description`,
  `labels`, `annotations`, `tags`, `links`) and a kind-specific `spec`
  (<https://backstage.io/docs/features/software-catalog/descriptor-format/>).
* **Kinds.** Component, API, Resource, System, Domain, Group, User, Template, Location. Component requires `type`,
  `lifecycle`, `owner`. API additionally requires `definition`, the machine-readable contract itself
  (<https://backstage.io/docs/features/software-catalog/descriptor-format/>,
  <https://backstage.io/docs/features/software-catalog/system-model/>).
* **Storage.** Authored as YAML "stored in source control", which the docs call the source of truth
  (<https://backstage.io/docs/features/software-catalog/>). The catalog itself is a database: providers ingest raw
  entities, processors validate and emit relations, a stitcher assembles the final entity the API serves
  (<https://backstage.io/docs/features/software-catalog/life-of-an-entity/>). So git holds the input and a database
  holds the knowledge an agent or UI reads.
* **Validation, and when.** Ingestion applies only coarse checks (a `kind` and `metadata.name`). "The more precise
  validation rules ... are not yet applied at this stage." Processing validates later, and errors land in a `status`
  field on the entity and in events (<https://backstage.io/docs/features/software-catalog/life-of-an-entity/>). Name
  rules reject registration; enumerated values do not: "The software catalog accepts any lifecycle value"
  (<https://backstage.io/docs/features/software-catalog/descriptor-format/>). Built-in kinds are checked against a fixed
  schema; a custom kind needs a TypeScript type, a JSON Schema and a custom processor module; unknown `spec` fields
  are stored ("the catalog will happily store whatever is in it")
  (<https://backstage.io/docs/features/software-catalog/extending-the-model/>).
* **Pre-merge validation.** `backstage-cli catalog validate <file>` checks a YAML file "against the catalog schema" and
  exits 1 on an invalid entity. It requires authenticating with a Backstage instance first, and reports "connection and
  authentication problems" as errors (<https://backstage.io/docs/tooling/cli/module-catalog>), so it needs a running
  instance (inferred: not an offline CI check).

### Ownership, lifecycle, relations

* `spec.owner` on Component, API, Resource, System, Domain. Group and User are first-class kinds, so an owner resolves
  to an entity with members and a parent
  (<https://backstage.io/docs/features/software-catalog/descriptor-format/>).
* `lifecycle` well-known values `experimental`, `production`, `deprecated`, unenforced (same page).
* **Relations are derived, not authored.** Seven paired relations (`ownedBy`/`ownerOf`, `providesApi`/`apiProvidedBy`,
  `consumesApi`/`apiConsumedBy`, `dependsOn`/`dependencyOf`, `parentOf`/`childOf`, `memberOf`/`hasMember`,
  `partOf`/`hasPart`), each "commonly generated" from a spec field. The author writes one side; the inverse appears on
  the other entity (<https://backstage.io/docs/features/software-catalog/well-known-relations/>). Descriptor files
  "should not contain" `relations`, and produced relations are "the authoritative source"
  (<https://backstage.io/docs/features/software-catalog/descriptor-format/>).
* **Opaque ids.** `annotations` are "often pointing to external systems", and `links` "don't drive functionality"
  (<https://backstage.io/docs/features/software-catalog/descriptor-format/>). An annotation such as a PagerDuty id is
  a key a plugin dereferences, not a fact for a reader; kac's services lineage already says so.

### TechDocs

* "Spotify's homegrown docs-like-code solution". Markdown "live[s] together with their code"
  (<https://backstage.io/docs/features/techdocs/>), built with MkDocs from an `mkdocs.yml` beside `catalog-info.yaml`,
  wired by the annotation `backstage.io/techdocs-ref: dir:.`. Recommended: generate in CI and publish to S3, GCS or
  Azure Blob, with `techdocs.builder: external` (<https://backstage.io/docs/features/techdocs/how-to-guides/>).
* **No content validation.** The overview describes none; a "Trust Card with a Trust Score" sits under "Someday/Maybe"
  on the roadmap (<https://backstage.io/docs/features/techdocs/>). A TechDocs page has no type, owner field, status or
  review date of its own (inferred: it inherits the owning entity's owner through the annotation).

### Reaching an AI agent

* `@backstage/plugin-mcp-actions-backend` "exposes Actions registered with the Actions Registry as MCP tools", over
  streamable HTTP at `/api/mcp-actions/v1`, behind Backstage auth (OAuth via CIMD recommended, static tokens a
  "temporary workaround") (<https://backstage.io/docs/ai/mcp-actions>).
* Well-known actions include `catalog.get-catalog-entity`, `catalog.query-catalog-entities`, `catalog.register-entity`,
  `catalog.validate-entity`, `catalog.get-catalog-model-description` (a Markdown description of kinds, annotations,
  labels, tags and relations), `search.query`, `techdocs.get-techdocs-metadata` and scaffolder actions
  (<https://backstage.io/docs/ai/well-known-actions>). The list is called non-exhaustive.

### Distribution

One instance per organisation. `namespace` "only bounds name uniqueness"; cross-namespace refs use `<namespace>/<name>`
(<https://backstage.io/docs/features/software-catalog/descriptor-format/>). I found no mechanism by which one
organisation's catalogue binds another's (inferred from absence across the pages read).

---

## 2. Cortex

### Problem and audience

A commercial internal developer portal: catalog plus Scorecards and Initiatives for engineering standards
(<https://docs.cortex.io/standardize/scorecards>).

### Data model

* **Unit.** An entity defined by a "Cortex entity descriptor", an OpenAPI 3 document with `x-cortex-*` extensions under
  `info`. Required: `title`, `x-cortex-tag`, `x-cortex-type`. Others: `x-cortex-owners` (typed `group` or `email`),
  `x-cortex-groups`, `x-cortex-custom-metadata`, `x-cortex-link`, `x-cortex-dependency`, `x-cortex-parents`/
  `x-cortex-children`, integration blocks such as `x-cortex-oncall` and `x-cortex-git`
  (<https://docs.cortex.io/ingesting-data-into-cortex/entities-overview/entities/yaml>).
* **Storage.** "Cortex stores descriptors as JSON in the database", so YAML comments are lost on round trip (same page).
  GitOps reads `cortex.yaml` from the default branch, or a `.cortex/` directory holding `catalog`, `domains`, `teams`,
  `scorecards`, `workflows` in one repository. An entity type can be set so that after GitOps creation "Cortex doesn't
  process edits you make through GitOps" and the UI becomes the source of truth. Catalogs "cannot be created via
  GitOps" (<https://docs.cortex.io/configure/gitops>).
* **Custom types.** An entity type carries a JSON Schema; its `required` attributes "must be defined for entities of
  that type". Types are created in the UI or API, not GitOps
  (<https://docs.cortex.io/ingesting-data-into-cortex/entities/adding-entities/entity-types>).
* **Validation, and when.** A linter runs on pull requests through the Cortex GitHub app. It checks YAML structure,
  required fields and the format of Cortex blocks. It "does not verify that referenced data is correct", for example
  that a group or monitor id exists, and a problem in a Cortex block is a warning that still passes
  (<https://docs.cortex.io/ingesting-data-into-cortex/entities-overview/entities/yaml/linter>). Misplaced root-level
  blocks are ignored and "the linter doesn't flag them"
  (<https://docs.cortex.io/ingesting-data-into-cortex/entities-overview/entities/yaml>).

### Ownership, lifecycle, relations, standards

* Owners, parents/children hierarchy, dependencies by `tag` (same page). `x-cortex-oncall` and `x-cortex-git` are
  integration pointers a Cortex integration dereferences (inferred from their being named "integration blocks").
* **Scorecards.** Rules that "either passes or fails for each entity", drawing on catalog metadata and "live data from
  connected integrations", graded by levels or points, written in a form builder or Cortex Query Language. Re-evaluated
  "every four (4) hours" by default. "Scorecards as code" via GitOps (<https://docs.cortex.io/standardize/scorecards>).
* **Rule exemptions.** Any user requests; an admin or a holder of "Configure Scorecard Exemptions" approves. Permanent or
  for a set number of days; statuses PENDING, APPROVED, REJECTED, REVOKED, EXPIRED; a revoked exemption keeps its
  approval history (<https://docs.cortex.io/scorecards/rule-exemptions>, read via search index, 2026-10-27).

### Reaching an AI agent

* Cortex MCP queries "across your catalog, Scorecards, metrics, and Initiatives" and can "Create and update entities,
  Scorecards, Initiatives, Workflows, and more"; the remote version adds `query_docs` over Cortex's own documentation
  (<https://docs.cortex.io/get-started/cortex-ai-assistant/mcp>). Local Docker image `ghcr.io/cortexapps/cortex-mcp`
  with a personal access token; README calls it "an early version" (<https://github.com/cortexapps/cortex-mcp>). Remote
  endpoints `https://mcp.cortex.io/mcp` and an EU one, per the search index for
  <https://docs.cortex.io/get-started/cortex-ai-assistant/mcp/configuring-cortex-mcp> (not fetched directly).
* Caution: "Cortex MCP Server" pages at cortex-docs.paloaltonetworks.com belong to Palo Alto's unrelated Cortex XSIAM.

### Distribution

One workspace per customer (inferred: no page read describes one workspace's scorecards binding another's).

---

## 3. OpsLevel

### Problem and audience

A commercial catalog with a maturity rubric, scorecards and campaigns (<https://docs.opslevel.com/docs/scorecards>).

### Data model

* **Unit.** A component (or a repository) described by `opslevel.yml`: required `name`, `type`; `owner`, `lifecycle`,
  `tier` take OpsLevel-generated aliases; `system`, `product`, `language`, `framework`, `description` free text; lists
  of `aliases`, `tags` (key/value), `tools` (`category`, `url`), `dependencies` (component aliases with `notes`),
  `repositories` (`name` as org/repo plus `provider`); and `properties` keyed by Property Definition alias
  (<https://docs.opslevel.com/docs/opslevel-yml>).
* **Storage.** OpsLevel's own store, fed by UI, GraphQL API and the file. A file-managed component locks name,
  description, owner, tier, system, lifecycle, language and framework; deleting the file unlocks it and keeps the data.
  Errors surface through `configErrors` on the API (same page).
* **Validation.** A published JSON Schema (`opslevel.schema.yml`) that "you can automatically validate ... as part of
  your CI", e.g. with `ajv` (same page). Schema validation only (inferred: nothing documented resolves a `dependencies`
  alias before merge).

### Standards and checks

* Rubric of levels (Bronze, Silver, Gold by default) and categories; scorecards are team-owned "lightweight Rubrics"
  that do not move a service's overall level (<https://docs.opslevel.com/docs/scorecards>).
* Check types: Ownership, Property, Service Configuration, Repository Integrated, Tool Usage, Tag Defined, Repo File,
  Repo Search, Custom Event, Manual. A check has an owning team and Markdown notes explaining why it matters; a Manual
  check can "auto-expire" for periodic audits (<https://docs.opslevel.com/docs/checks-and-filters>).

### Documentation

Tech Docs is "a docs-as-code solution that retrieves documentation from component repositories": OpsLevel scans a
mapped repository for Markdown, syncs on change, searches by file name, path or content. Nothing is said of
frontmatter, ownership per page, or content validation (<https://docs.opslevel.com/docs/tech-docs>).

### Reaching an AI agent

Hosted MCP at `https://app.opslevel.com/mcp`, OAuth, "only uses read-only access", reading Actions, Checks, Components,
"Documentation (API & Tech Docs)", Domains, Filters, Infrastructure, Repositories, Systems, Teams, Users
(<https://docs.opslevel.com/docs/mcp>). The self-hosted `opslevel-mcp` is deprecated and the repository was archived
on 2026-09-10 (<https://github.com/opslevel/opslevel-mcp>).

### Distribution

One account per customer (inferred, as for Cortex).

---

## 4. Port (brief)

* A blueprint is "represented by a Json schema" with `properties`, `required` and `relations`; entities are instances.
  Blueprints are edited by UI, API, Terraform, Pulumi, Port AI or the MCP server
  (<https://docs.port.io/context-lake/data-model/setup-blueprint/overview/>). `required` is described as making a field
  required "in the UI form"; the page says nothing of write-time validation (same page).
* Remote MCP at `https://mcp.port.io/v1` (and a US endpoint), OAuth or client-credentials; "blueprint, entity,
  scorecard, and action tools"; `x-read-only-mode: 1` hides write tools
  (<https://docs.port.io/ai-interfaces/port-mcp-server/overview-and-installation/>).
* **Closest to kac's own ground, but unverified.** Port's guide "Centralize AI coding rules with Port" models one AI
  coding rule per entity with priority, status (active, draft, deprecated), version, scope and category, linked to a
  company, team or service; an agent resolves precedence (service over team over company) and generates `AGENTS.md`
  per repository. Both <https://docs.port.io/guides/all/centralize-ai-coding-rules-with-port/> and
  <https://docs.port.io/guides/all/manage-ai-instructions/> returned **HTTP 404** on 2026-10-27; these claims rest on
  the search engine's index of those pages only, so treat them as unverified until re-read.

---

## Same, differs, lacks

### Same

* **YAML in git, reviewed in a pull request, as the authored form.** All four take a descriptor from the repository
  (Backstage `catalog-info.yaml`, `cortex.yaml`, `opslevel.yml`). kac's record frontmatter is the same move.
* **An owner on every unit, a lifecycle/status, a dependency edge.** kac's `_universal.yaml` makes `owner` and `status`
  required on every record (`.schema/_universal.yaml`).
* **Schema-declared types the adopter can extend.** Backstage custom kinds via JSON Schema and a processor, Cortex
  custom entity types via JSON Schema, Port blueprints. kac's type is a YAML file in `.schema/`
  (`docs/framework/types.md`).
* **Docs as Markdown beside the code** (TechDocs, OpsLevel Tech Docs). kac's records are Markdown too.
* **A description of the model for agents.** Backstage's `catalog.get-catalog-model-description` returns the model as
  Markdown; kac's type pages and generated schema tables do the same job for a reader (inferred equivalence).

### Differs

* **Git versus the database as what is read.** The products ingest git into a database and serve that; Cortex and
  OpsLevel let the UI override or lock against the file. kac's git tree *is* the corpus, and the export is built from it
  (`README.md`).
* **Validation timing and depth.** Backstage validates after ingestion and reports on the entity's `status`; its CLI
  check needs a live instance; enums are unenforced. Cortex's PR linter does not check that a referenced entity exists.
  OpsLevel offers JSON Schema only. kac fails CI on an unresolved reference and a one-sided reciprocal edge, offline
  (`docs/framework/principles.md`, "Knowledge is a graph").
* **Inverse edges.** Backstage derives the inverse of each relation; the author writes one side. kac requires both
  sides to agree and fails the build otherwise (same principles section). This is a cost kac takes for a record that
  reads correctly away from the tool (inferred rationale; not stated as such).
* **Unit of knowledge.** Catalogues model the estate (components, APIs, teams). Standards, decisions, runbooks and
  fixes are not catalogue kinds; at best they sit as TechDocs pages or links with no type of their own. kac types
  eighteen kinds of knowledge (`docs/framework/types.md`).
* **Agent delivery.** Products serve live queries over MCP to any MCP client. kac bundles a frozen export into a
  Claude Code plugin (`docs/compared.md`, "What KaC does not do"; `CLAUDE.md`: "The export is frozen at bundle time").
* **Standards as data versus standards as documents.** Cortex and OpsLevel encode a standard as an executable rule
  (CQL, repo-file, grep). kac encodes it as a graded clause with an id and a control *record* saying what checks it.

### Lacks (what kac lacks that these have)

1. **Evaluation against the estate.** Cortex scorecards run every four hours against catalog metadata and live
   integration data; OpsLevel checks read repository files and events. kac records what checks a rule (controls) and
   does not run anything against code; `drift-against-repos` is declared and "not running"
   (`.schema/services.yaml`). The nearest kac has is CI on the corpus itself.
2. **A workflow around exemptions.** Cortex's exemption has request, approve, reject, revoke and auto-expiry with
   statuses and history, in the product. kac's deviation record carries the accepted risk and a review date
   (`docs/framework/types.md`), but the request-approve loop runs through an issue and a pull request (inferred from
   the `request-deviation` skill description), and nothing expires one automatically (inferred; not checked in code).
3. **Live, agent-agnostic access with write tools.** Backstage, Cortex, OpsLevel and Port each ship an MCP server any
   MCP client can call; Cortex and Port add write tools. kac's plugin is Claude Code only and read-only, writing back
   through an issue (`README.md`).
4. **Organisation entities.** Backstage Group and User are kinds with membership and hierarchy; Cortex owners can be
   a Cortex team. kac's `owner` is a string `human:` or `role:` and admits no team (`_universal.yaml`). Whether that
   is a lack or a choice is for Paul; `_universal.yaml` says it is deliberate.
5. **Ingestion from the estate.** Backstage providers, OpsLevel Git integrations and Cortex integrations discover
   entities. kac's service records are written by hand.
6. **Self-service scaffolding.** Backstage Templates and `scaffolder.execute-template`. Nothing in kac corresponds.
7. **A rendered portal.** All four give a UI over the graph. kac publishes a wiki (Azure DevOps first) from the same
   files (`README.md`, "Opinions").

Not counted as lacks: `pagerduty.com/service-id`, `x-cortex-oncall`, OpsLevel `alert_sources` and `tools` URLs are
opaque ids or pointers a product dereferences. A reader holding only the value has nothing; kac's
`monitoring-output` deliberately states a commitment instead (`.schema/services.yaml`, `monitoring-output.notes`).
Backstage `spec.definition` embeds an API contract; kac deliberately points at it.

### Where kac is genuinely ahead

* **Referential integrity before merge, offline.** None of the four documents a pre-merge check that a reference
  resolves; Cortex says outright that its linter does not.
* **Typed knowledge beyond the estate**, with ids per rule (`std-SECRET.STORE`) that an agent and a reviewer can cite.
  A Cortex or OpsLevel check is a pass/fail on an entity, not a citable clause with a MUST/SHOULD grade.
* **Policy to standard to control traceability** as checked edges (`docs/compared.md`, "What KaC adds").
* **Cross-organisation consumption.** A team corpus consumes an organisation corpus and fails a citation a new version
  broke (`README.md`, `docs/compared.md`). No product read offers one tenant's content binding another's.
* **Exit cost.** "The Markdown outlives the tool" (`docs/framework/principles.md`); Cortex stores descriptors as JSON
  and drops comments, and OpsLevel lets the UI become the source of truth.
* **Ownership and review dates on documentation.** TechDocs and OpsLevel Tech Docs pages have no status, owner field or
  review date; every kac record does.

## What kac's docs left unclear

* `docs/compared.md` omits this whole family, though `services.yaml` lineage cites three members. A reader holding
  Backstage will ask the comparison first.
* `docs/framework/principles.md` says both a corpus is "one bounded context" where "a session clones the repository and
  has the whole graph, with nobody to ask for the rest", and (README) that a team corpus consumes an organisation's.
  How a consumed graph squares with "the whole graph" in one clone is not stated on the pages read.
* I could not establish from the docs read whether kac ever derives an inverse edge, or always requires the author to
  write both sides; the principle reads as the latter.
* Nothing says how a corpus coexists with an existing catalogue: whether `svc-` records should be generated from
  `catalog-info.yaml`, or link to its entity, or replace it.
