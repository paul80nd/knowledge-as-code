# Landscape: decision records and compliance as code (issue #1160)

Every web source below was read on **2026-10-27**. Each kac claim cites a file in this repository at commit `375411d43`
on branch `measure-surfaces-1162`. A claim I drew as a conclusion rather than read is marked **(inferred)**.

## kac, in brief (the baseline)

* A record is Markdown with YAML frontmatter, and its type is declared in `.schema/<type>.yaml`. `kac validate` checks
  fields, sections, ids, links and reciprocal edges, and fails CI (`README.md`, `docs/framework/principles.md`).
* The chain is policy → standard → control. A policy's clauses are table rows with ids such as `pol-SCRT.STORE`.
  A standard's `implements:` cites clause ids, and its rules are H3 parts such as `std-SECRET.NOWHERE`. A control's
  `verifies:` cites rule ids and reciprocates `verified-by` (`.schema/policies.yaml`, `.schema/standards.yaml`,
  `.schema/controls.yaml`).
* A standard can also be `derived-from:` an ADR. An ADR is immutable once `accepted` or `rejected`, and the change is
  made by `supersedes`/`superseded-by`, which are checked both ways (`.schema/adrs.yaml`).
* A policy's `Alignment` column cites external framework references such as `[ISO 27001:2022].A.5.17`, which roll up
  into `aligns-with:`. `frameworks.md` records the posture: Obliged, Self-obligated or Inspiration
  (`.schema/policies.yaml`, `examples/engineering/frameworks.md`).
* For layering, `consumes:` names a producer corpus, a shortcode, a semver range and a resolved lock. The producer
  runs `export`, then `pack` into a `.nupkg`. The consumer runs `restore` into `.imports/`, and citations are written
  `eng:pol-VURM.TIMEBOX`. A check reads only an import's id, type, path and parts. An export re-publishes what the
  producer itself consumed, at any depth (`docs/use/imports.md`, `examples/payments/.corpus.yaml`).
* An agent reaches a corpus through `kac bundle`, which builds a Claude Code plugin whose lookup skills read the
  export (`README.md`, `docs/framework/intent.md`).

---

## 1. MADR (Markdown Architectural Decision Records)

### Problem and audience

MADR is a Markdown template for architectural decisions, aimed at development teams. The current version is
**MADR 4.0.0, released 2024-09-17** (https://adr.github.io/madr/). It is a format and ships no tool. The MADR page
sends readers to an external list of tooling (https://adr.github.io/madr/, https://adr.github.io/adr-tooling/).

### Data model

* One decision per file, named `NNNN-title-with-dashes.md` (https://adr.github.io/madr/).
* MADR 4 frontmatter is all optional ("These are optional metadata elements"): `status`, `date`, `decision-makers`,
  `consulted`, `informed`
  (https://raw.githubusercontent.com/adr/madr/main/template/adr-template.md).
* Sections: *Context and Problem Statement*, *Decision Drivers* (optional), *Considered Options*, *Decision
  Outcome*, with *Consequences* and *Confirmation* (both optional) beneath it, then *Pros and Cons of the Options*
  (optional) and *More Information* (optional) (same URL).
* The minimal template keeps only *Context and Problem Statement*, *Considered Options* and *Decision Outcome*, and
  *Consequences* is still marked optional there
  (https://raw.githubusercontent.com/adr/madr/main/template/adr-template-minimal.md).
* Validation is formatting only: a markdownlint configuration ships with it (https://adr.github.io/madr/). Nothing
  checks the frontmatter or the sections.

### Exact version history (relevant to kac's lineage)

From https://raw.githubusercontent.com/adr/madr/main/CHANGELOG.md:

* **3.0.0-beta**: added YAML front matter to the template.
* **3.0.0 (2022-10-09)**: merged *Positive Consequences* and *Negative Consequences* into *Consequences*, and moved
  *Validation* directly after *Decision Outcome*.
* **4.0.0-beta**: renamed *Deciders* to *Decision Maker(s)*, with the key `decision-makers:`. Renamed *Validation* to
  *Confirmation* and made it a sub-element of *Decision Outcome*. Quoted the value of `status:`. Removed the link from
  `status`, leaving the identifier alone (`superseded by ADR-0123`). Added the minimal and bare templates.
* **4.0.0 (2024-09-17)**: fixes only.

### Ids, status, lifecycle, links

* `status: "{proposed | rejected | accepted | deprecated | … | superseded by ADR-0123}"`
  (https://raw.githubusercontent.com/adr/madr/main/template/adr-template.md). **`rejected` is a MADR status.** It is
  also in the older MADR-style template Log4brains ships (see section 3).
* Supersession is a phrase inside `status`. Other links are free prose under *More Information* (same URL).
* Nothing enforces immutability (inferred: there is no tool to enforce it).

### Layering

None. MADR has no concept of one organisation's decisions being imported by another (inferred from the template and
the project page, which name none).

### Reaching an AI agent

Not mentioned on the MADR page, and the ADR tooling list names no MCP server or agent integration
(https://adr.github.io/madr/, https://adr.github.io/adr-tooling/). An agent reads MADR files as plain Markdown in a
checkout (inferred).

### Same / differs / lacks versus kac

* **Same.** One decision per numbered Markdown file, with YAML frontmatter, a set of options weighed, a decision and
  its consequences, and a status vocabulary that includes `rejected` and `superseded`.
* **Differs.**
  * kac requires `status` and `decided-on` (when `accepted` or `rejected`), where MADR's frontmatter is all optional.
  * kac requires *Consequences* and the Y-statement block-quote (a warning).
  * kac's supersession is a pair of id fields, checked both ways. MADR's is a string inside `status`.
  * kac's `deciders` keeps the MADR 3 name. MADR 4 calls it `decision-makers`.
  * kac's section names (*Context*, *Decision*, *Alternatives Considered*) are Nygard's plus one. They are not MADR 4's
    (*Context and Problem Statement*, *Considered Options*, *Decision Outcome*) (`.schema/adrs.yaml`).
* **Lacks.**
  * MADR has a *Decision Drivers* section and per-option *Pros and Cons* ("Good, because / Bad, because / Neutral,
    because"). kac has a single verdict bullet for each alternative.
  * MADR has `consulted` and `informed`, which kac drops on purpose.
  * MADR has *Confirmation* inside the ADR. kac moves it to a standard and a control.
* **Ahead.**
  * kac enforces the schema, immutability (`immutable-after-accepted`), reciprocal supersession and a decided-on date.
  * kac links an ADR to the standard derived from it, and to the control that checks that standard.
  * kac carries ADRs into the agent plugin.

### Where kac's lineage on MADR is wrong or loose

1. `.schema/adrs.yaml` says, under `divergence`: "A fifth status, `rejected`, keeps a proposal the team turned down."
   That is a divergence from Nygard, but MADR 4 already lists `rejected`. The `notes:` on `status` even says "the order
   is the one MADR 4 lists", and MADR 4's order is proposed, rejected, accepted, deprecated, superseded, which matches
   kac's values exactly. The divergence sentence should say "from Nygard".
2. `docs/framework/lineage.md` quotes the ADR row and omits every MADR mention the schema now carries. Its
   `divergence` text also differs from `.schema/adrs.yaml`. Either the page is stale or it abridges without saying so.
3. kac's field `deciders` is MADR's pre-4.0 name. The lineage does not say whether kac keeps it on purpose.

---

## 2. adr-tools (npryce)

### Problem and audience

adr-tools is "Command-line tools for working with Architecture Decision Records", written as Bash scripts. ADRs live by
default in `doc/adr` (https://github.com/npryce/adr-tools). The latest tag is `3.0.0`, and the last push was
2024-04-25. The repository is not archived, and its licence reads as "Other"
(https://api.github.com/repos/npryce/adr-tools/tags, https://api.github.com/repos/npryce/adr-tools).

### Data model

* The default template is Nygard's: `# NUMBER. TITLE`, `Date:`, then `## Status`, `## Context`, `## Decision` and
  `## Consequences` (https://raw.githubusercontent.com/npryce/adr-tools/master/src/template.md).
* There is no frontmatter: status is a section. A repository can supply its own template at `templates/template.md`
  (https://raw.githubusercontent.com/npryce/adr-tools/master/src/_adr_help_new).
* The commands are the scripts in `src/`: `adr-init`, `adr-new`, `adr-link`, `adr-list`, `adr-generate` (with
  `_adr_generate_toc` and `_adr_generate_graph`), `adr-config`, `adr-upgrade-repository`
  (https://api.github.com/repos/npryce/adr-tools/contents/src).
* There is no validation (inferred: no script for it is listed, and the README describes none).

### Ids, status, links

* Files are numbered sequentially.
* `adr new -s N` supersedes ADR N. It adds a link in the new ADR's Status section and updates the old ADR's status.
* `-l TARGET:LINK:REVERSE-LINK` writes a named link into both ADRs, such as "Amends" and "Amended by"
  (https://raw.githubusercontent.com/npryce/adr-tools/master/src/_adr_help_new).

### Layering and agents

Neither appears. An agent would run the shell commands or read the files (inferred).

### Versus kac

* **Same.** Nygard's sections, sequential numbers, and supersession kept in both directions.
* **Differs.** adr-tools *writes* both ends of a link when the ADR is created. kac *checks* that both ends agree on
  every build, so a hand edit that breaks one end fails.
* **Lacks.** kac has no command that creates a superseding ADR and stamps both files. kac also has no arbitrary
  bidirectional link type such as "Amends / Amended by": `related` is one-directional and kac checks it against the
  `## Related` section. Nor does kac draw a decision graph.
* **Ahead.** kac has schema validation, required fields and an immutability rule.

---

## 3. Log4brains

### Problem and audience

Log4brains is "a docs-as-code knowledge base for your development and infrastructure projects". It logs ADRs from the
IDE and publishes them as a static site with a timeline and search (https://github.com/thomvaill/log4brains). It is
licensed Apache-2.0. The latest release is **v1.1.0, 2024-12-17**, and the last push was 2024-12-17
(https://api.github.com/repos/thomvaill/log4brains/releases?per_page=3,
https://api.github.com/repos/thomvaill/log4brains).

### Data model

* Markdown in git. The template can be customised, and the default is MADR
  (https://raw.githubusercontent.com/thomvaill/log4brains/master/README.md).
* The template it ships is the **pre-3.0 MADR shape**. Its metadata is bullets (`- Status:`, `- Deciders:`, `- Date:`,
  `- Tags:`), not frontmatter. It keeps separate *Positive Consequences* and *Negative Consequences*, and ends with a
  `## Links` section
  (https://raw.githubusercontent.com/thomvaill/log4brains/master/packages/init/assets/template.md). That it is MADR 2.x
  is inferred from the MADR changelog above.
* The status list in that template is `draft | proposed | rejected | accepted | deprecated | … | superseded by` (same
  URL).
* "No enforced markdown structure: you are free to write however you want". Metadata is "automatically guessed from
  its raw text and git logs" (https://raw.githubusercontent.com/thomvaill/log4brains/master/README.md).
* The CLI runs `init`, `adr new`, `preview` and `build` (https://github.com/thomvaill/log4brains).

### Layering

"Multi-package projects support (mono or multi repo): notion of global and package-specific ADRs", configured in
`project.packages` in `.log4brains.yml`. The README also says "For the moment, all the ADRs have to be stored in a
central repository" (https://raw.githubusercontent.com/thomvaill/log4brains/master/README.md). This is scoping inside
one repository. It is not import across organisations.

### Reaching an AI agent

The README does not mention AI, an LLM or MCP (same URL). Its roadmap lists an `@adr` annotation to cite an ADR from
code, and a decision backlog (same URL).

### Versus kac

* **Same.** Markdown ADRs in git, published as a site for people to read.
* **Differs.** Log4brains infers metadata from text and git and enforces nothing. kac declares the fields and fails a
  record that breaks them.
* **Lacks.**
  * A rendered timeline and search UI. kac targets the Azure DevOps wiki and generated indexes.
  * Dates and authorship guessed from git with no metadata written.
  * A planned `@adr` code annotation.
* **Ahead.** kac validates records, imports across corpora and builds an agent plugin. Log4brains has done none of
  these, and has had no release since December 2024.

---

## 4. NIST OSCAL (Open Security Controls Assessment Language)

### Problem and audience

OSCAL is a NIST-led initiative giving "open, machine-readable formats available in XML, JSON, and YAML" for
control-based risk assessment (https://pages.nist.gov/OSCAL/). It is aimed at control catalogue publishers, system
owners, assessors and authorising officials, as FedRAMP does (inferred from the models). The latest release is
**v1.2.3, 2026-08-07**. v1.2.0 (2025-12-12) added the **Control Mapping** model
(https://api.github.com/repos/usnistgov/OSCAL/releases?per_page=4,
https://api.github.com/repos/usnistgov/OSCAL/releases/tags/v1.2.0).

### Data model and layers

* Lower layers "provide information structures that are referenced and used by each higher layer". The control layer
  holds the catalog and the profile. The implementation layer holds the component definition and the SSP. The
  assessment layer holds the assessment plan, the assessment results and the POA&M
  (https://pages.nist.gov/OSCAL/learn/concepts/layer/, search summary).
* **Catalog.** `catalog` has `uuid`, `metadata`, `params`, `controls`, `groups` and `back-matter`. A control has `id`
  (a required token), `class`, `title`, `params`, `props`, `links`, `parts` (each with a required `name` and optional
  `prose`) and nested `controls`. `metadata` requires `version`, `oscal-version` and `last-modified`, and takes
  `revisions[]` (https://pages.nist.gov/OSCAL-Reference/models/v1.2.3/catalog/json-outline/).
* **Profile.** `imports[]` each require an `href` and either `include-all` or `include-controls` (`with-ids`,
  `matching` pattern, `with-child-controls`), plus `exclude-controls`. `merge` takes `combine`, and one of `flat`,
  `as-is` or `custom`. `modify` takes `set-parameters[]` and `alters[]` (each by `control-id`, with `removes` and
  `adds`) (https://pages.nist.gov/OSCAL-Reference/models/v1.2.3/profile/json-outline/). A profile is "a baseline of
  selected controls from one or more control catalogs" (https://pages.nist.gov/OSCAL/learn/concepts/layer/control/profile/).
* **Component definition.** `import-component-definitions[]` (each an `href`). `components[]` each carry
  `control-implementations[]`, with a `source` (the profile or catalog) and `implemented-requirements[]`, each with a
  `control-id` and `statements[]` keyed by `statement-id`. `capabilities[]` group components
  (https://pages.nist.gov/OSCAL-Reference/models/v1.2.3/component-definition/json-outline/).
* **SSP.** `import-profile` (an `href`). `system-characteristics` carries `status` and `authorization-boundary`.
  `system-implementation` lists components. `control-implementation` has `implemented-requirements` with
  `by-components` (`component-uuid`, `implementation-status` with `state`, and `export`/`inherited`/`satisfied` with
  `provided` and `responsibilities`)
  (https://pages.nist.gov/OSCAL-Reference/models/v1.2.3/system-security-plan/json-outline/). Each
  `implemented-requirement` references a control by `@control-id`, which must match "a single target control with the
  matching id" in the baseline (https://pages.nist.gov/OSCAL/learn/concepts/layer/implementation/ssp/).
* **Control Mapping (1.2).** `mapping-collection` has a `provenance` (`method`, `matching-rationale`, `status`) and
  `mappings[]`. Each mapping has a `source-resource` and a `target-resource` (`type`, `href`) and `maps[]`, each with a
  `relationship`, `sources` and `targets`, which take `type` and `id-ref`
  (https://pages.nist.gov/OSCAL-Reference/models/v1.2.3/mapping/json-outline/).
* **Validation.** `oscal-cli` validates OSCAL content "to ensure it is well-formed and valid", converts between XML,
  JSON and YAML, and resolves profiles (https://github.com/usnistgov/oscal-cli). That validation covers both schema and
  Metaschema constraints is inferred: the page does not say which. It runs when somebody invokes a tool. OSCAL defines
  no CI.

### Ids, status, lifecycle

* Every object is identified by a UUID, and a control by a token `id` such as `ac-2`. "This document level UUID is the
  only UUID in OSCAL associated with version control"
  (https://pages.nist.gov/OSCAL/learn/concepts/layer/control/profile/).
* A document has a free-string `version` and `revisions[]`
  (https://pages.nist.gov/OSCAL-Reference/models/v1.2.3/catalog/json-outline/).
* A control's withdrawal is a convention in the content, not a field: `prop[@name='status']` "Withdrawn", with
  `rel="incorporated-into"` pointing at the controls that superseded it
  (https://github.com/usnistgov/oscal-content/tree/main/nist.gov/SP800-53/rev4).
* Component definitions carry no implementation-status field. Trestle stores status in props "because the schema
  lacks a status field"
  (https://oscal-compass.dev/compliance-trestle/latest/tutorials/Trestle_authoring/ssp_profile_catalog_authoring/).

### The chain compared with kac

| OSCAL                                                                        | kac                                                          | Comment                                                                                                                                                                                                                                                                                    |
|------------------------------------------------------------------------------|--------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **catalog** of controls, the requirements                                    | **policy** clauses, and the **standard** rules beneath them  | An OSCAL "control" is a safeguard requirement. kac's `controls.yaml` `collision:` already warns that kac's control is the *verification* instead.                                                                                                                                           |
| **profile**: select, tailor (`set-parameters`, `alters`) and resolve        | No equivalent. A standard *tightens* a policy clause by citing it with `implements:` | kac never re-publishes a tailored copy of another corpus's clauses. A consumer cites them and adds its own standards. kac lets a standard tighten a policy's figure, which is close to `set-parameters` in spirit (inferred), but nothing checks the tightening.                             |
| **component definition**: which controls a product or service can satisfy, statement by statement | **standard** `applies-to:` services, plus **service** records    | OSCAL's direction is component → control (`control-implementations.source` names the profile). kac's direction is standard → policy clause (`implements:`), and the policy lists nothing back.                                                                                              |
| **SSP**: how each control is implemented in one system, with `implementation-status` and responsibilities | No equivalent                                                | kac records the rules and their checks, never a per-system implementation statement.                                                                                                                                                                                                       |
| **assessment plan** (`tasks/timing/at-frequency`)                             | **control** `frequency`, `mechanism`, `evidence`             | Confirmed: `at-frequency` with `period` and `unit` (https://pages.nist.gov/OSCAL-Reference/models/v1.2.3/assessment-plan/json-outline/).                                                                                                                                                       |
| **assessment results** (`observations/origins/actors`)                        | **control** `last-verified.by`, for a manual check only       | kac keeps "the plan and never the result" (`.schema/controls.yaml`).                                                                                                                                                                                                                       |
| **POA&M**                                                                    | **deviation**                                                | kac's deviation lineage already cites SP 800-37's POA&M.                                                                                                                                                                                                                                   |
| **control mapping** (1.2)                                                    | the policy's `Alignment` column, `aligns-with:` and `frameworks.md` | OSCAL's mapping has a typed `relationship` and a `provenance.matching-rationale`. kac's alignment is an untyped citation, with posture held at framework level.                                                                                                                         |

### Layering across organisations (profile import compared with `consumes:`)

* **How a profile imports.** A profile imports by `href`: "an absolute or relative URL", or a back-matter resource
  whose `rlink` resolves. A relative reference resolves against the profile's own URI. A source that cannot be fetched
  stops processing with an error. A circular import is an error. Profiles can import profiles, so tailoring chains
  (https://pages.nist.gov/OSCAL/learn/concepts/processing/profile-resolution/). The output of resolution is a new
  catalog, stamped with `metadata:source-profile` and `metadata:resolution-tool` (same URL). The resolution
  specification is labelled "a work in progress and is subject to change" (same URL).
* **What is the same.** Both layer across organisations by reference rather than by copy. Both fail when the source
  cannot be fetched. Both inherit at depth: a profile of a profile, and kac's export re-publishing what it consumed.
* **Where they differ.**
  1. **Versioning.** OSCAL's `href` names one document and has no version range or lock. A document's `version` is a
     free string. kac's `consumes:` takes a semver range, resolves a lock, and reports `import-behind` and
     `import-capped` (`docs/use/imports.md`). kac is ahead here.
  2. **Selection and tailoring.** A profile chooses which controls apply (`include-controls`/`exclude-controls`) and
     rewrites them (`alters`, `set-parameters`). A kac consumer takes the producer's whole export and cannot exclude or
     amend an imported clause. It can only cite clauses or not cite them. Where a consumer departs from a clause it
     writes a deviation. kac lacks this.
  3. **Identity across layers.** OSCAL keeps the control's `id` through resolution. Telling two imports of the same
     control apart was a long-running problem, and it was closed by PR #1424, "Profile import identifier handling"
     (https://github.com/usnistgov/OSCAL/issues/538). kac's shortcode prefix (`eng:`), together with the rule that a
     record keeps its *author's* shortcode through a chain (`docs/use/imports.md`), settles the same question at the
     notation level. kac is arguably ahead here (inferred).
  4. **What a check may read.** A kac check reads only an import's id, type, path and parts, and never the producer's
     prose (`docs/use/imports.md`). An OSCAL resolver reads and rewrites the imported controls in full.
  5. **Packaging.** kac seals an export into a `.nupkg` registry package. OSCAL has no package or registry. Documents
     are fetched by URL.

### Reaching an AI agent

* NIST itself publishes no agent integration (inferred: none is named on https://pages.nist.gov/OSCAL/).
* AWS Labs published "OSCAL tools for AI agents", an MCP server with tools such as `get_oscal_schema`,
  `list_oscal_models` and `query_component_definition`. It serves bundled AWS component definitions, the OSCAL
  documentation and the schemas, and it can be pointed at local OSCAL JSON. The repository "was archived by the owner
  on Sep 2, 2026", and the README gives no reason (https://github.com/awslabs/mcp-server-for-oscal).
* Compliance Trestle's MCP server is covered in section 5.

### Versus kac

* **Same.**
  * Structured, machine-validated records with stable ids.
  * A layered chain from requirement to implementation to verification.
  * Import across organisations by reference.
  * A frequency on a verification activity.
  * A plan of action, where kac has the deviation.
* **Differs.**
  * The unit is a JSON, XML or YAML document holding many controls. kac's unit is one Markdown file per policy,
    standard or control.
  * OSCAL ids are UUIDs plus a control token. kac's are mnemonic and readable (`pol-SCRT.STORE`).
  * The word "control" means opposite things in the two (see `.schema/controls.yaml` `collision:`).
  * OSCAL is aimed at federal authorisation. kac is aimed at internal engineering rules.
* **Lacks** (what kac does not have):
  * Profile selection and tailoring of imported content.
  * Parameters (`params` and `set-parameters`) that make a figure in a clause a value a consumer can set. kac's
    policy clause "may state a figure", but it is prose.
  * Per-system implementation statements (the SSP), with implementation status and inherited or shared
    responsibility (`export`, `inherited`, `satisfied`).
  * Assessment results and findings kept as data.
  * A typed relationship and a rationale on each cross-framework mapping (Control Mapping).
  * XML, JSON and YAML equivalence, and interchange with GRC and authorisation tools.
  * A published deterministic resolution specification.
  * Existing machine-readable catalogues an organisation can import directly, such as NIST's own
    `usnistgov/oscal-content` (inferred: kac's `aligns-with` cites framework references as strings, with nothing to
    resolve them against).
* **Ahead** (kac):
  * Prose-first records that people read as a wiki.
  * Semver ranges with a lock.
  * Reciprocal edges checked both ways.
  * Rules that fail CI for a stale review date, a clause id dropped after activation, or an unproven MUST
    (`coverage-report`).
  * `not-enforced` as a first-class record of a gap.
  * ADRs and the rest of the engineering taxonomy, which OSCAL does not cover.
  * An agent plugin built from the same files.

### Notes on kac's own OSCAL claims (`.schema/controls.yaml`)

* "OSCAL schedules an assessment `at-frequency`" is **confirmed**.
* "OSCAL names who collected an observation in `origin-actor`" is **loosely worded**. The JSON path is
  `observations/origins/actors` (`type`, `actor-uuid`), and no key named `origin-actor` appears in the JSON outline
  (https://pages.nist.gov/OSCAL-Reference/models/v1.2.3/assessment-results/json-outline/). `origin-actor` is probably
  the Metaschema assembly name behind `actors` (inferred, not verified).
* The prior-art for controls is SP 800-53A. kac never mentions that its *policy* and *standard* layer plays OSCAL's
  *catalog* role, so a reader arriving from OSCAL has no map. The table above would supply one.

---

## 5. Compliance Trestle (OSCAL Compass)

### Problem and audience

Trestle is "an ensemble of tools that enable the creation, validation, and governance of documentation artifacts for
compliance needs", using OSCAL. It is "designed to operate as a CICD pipeline running on top of compliance artifacts in
`git`", and it splits large OSCAL documents so that changes go through pull request review, versioning and tagging.
"The current version of trestle 5.x supports NIST OSCAL 1.2.1" (https://oscal-compass.dev/compliance-trestle/latest/).
The latest release is **v5.2.0, 2026-10-05**
(https://api.github.com/repos/oscal-compass/compliance-trestle/releases?per_page=2). It belongs to OSCAL Compass, a
CNCF sandbox project that is not affiliated with NIST (https://github.com/oscal-compass/compliance-trestle-mcp).

### Data model: OSCAL as Markdown

The generate and assemble cycle runs JSON → Markdown → JSON
(https://oscal-compass.dev/compliance-trestle/latest/tutorials/Trestle_authoring/ssp_profile_catalog_authoring/):

* `catalog-generate` writes one Markdown file per control, in group folders, with the parameters in a YAML header.
  `catalog-assemble` can add controls but cannot remove them. `--version` sets a new version.
* `profile-generate` writes the resolved controls, with an "Editable Content" section where a profile adds parts.
  `--set-parameters` applies only the parameters marked `profile-values`.
* `component-generate` keeps rules as properties, which are read-only in the Markdown, and implementation status in
  properties as well.
* `ssp-generate` takes a profile and component definitions and writes a response for each statement and each
  component. A response for a part removed upstream is lost. `--leveraged-ssp` and `profile-inherit` handle inherited
  responsibility.
* `profile-assemble --required-sections / --allowed-sections` fails Markdown that breaks them. An assemble command
  writes no file when nothing changed, so a downstream CI step does not fire. A catalog change can cascade through CI
  to profiles and SSPs.

### Governed documents: the closest analogue to kac's own validator

`trestle author docs`, `folders` and `headers` validate arbitrary Markdown against a template
(https://oscal-compass.dev/compliance-trestle/latest/tutorials/Trestle_authoring/trestle_author/):

* **YAML header.** Every template key must be present and no key may be added. "we validate the structure and the
  presence of the "keys" and not the "values"."
* **Headings.** Every template heading must appear, in order and in its hierarchy. A document may not add a top-level
  heading, and fixed heading text must match unless the template marks it `{...}`.
* **Versioning.** `x-trestle-template-version` pins each document to a version of its template.
* **Governed headings.** `--governed-heading` confirms the key-value content of a named section did not change.
* The page's own example output is `VALID: decisions/decision_000.md`, so it is shown governing decision records.

### Reaching an AI agent

`compliance-trestle-mcp` is an "MCP server to easily use compliance-trestle (OSCAL tool) from Claude, Roo, or any
MCP-compliant client". It has nine tools, including `trestle_import`, `trestle_author_catalog_generate`,
`trestle_author_profile_resolve`, `trestle_author_profile_assemble` and `trestle_validate`. It runs with
`uvx --from compliance-trestle-mcp trestle-mcp` (https://github.com/oscal-compass/compliance-trestle-mcp). It hands an
agent the *tool's operations*, not a lookup of the content.

### Versus kac

* **Same.**
  * Markdown in git, reviewed by pull request and gated in CI.
  * A YAML header and a required set of sections, checked by a validator.
  * One file for each control.
  * An upstream change flows downstream.
* **Differs.**
  * Trestle's Markdown is a *projection* of OSCAL JSON, which stays the source of truth and is regenerated and
    reassembled. kac's Markdown *is* the record, and its JSON export is derived from it.
  * Trestle's governed-document check validates the presence of keys and not their values. kac validates types, enums,
    patterns, `required-when` and references.
  * Trestle's template is a sample document. kac's schema is a declarative YAML type file.
* **Lacks.**
  * Per-document template versioning (`x-trestle-template-version`), so an old record keeps validating against the
    template it was written to. kac validates every record against the current schema.
  * The governed-heading check that a section's content did not change. This is close to kac's
    `immutable-after-accepted`, which is declared with no `expr:` (inferred: whether C# enforces it was not checked).
  * Round-tripping to an interchange format a GRC tool or an authorising body accepts.
  * Inherited and leveraged responsibility across systems.
  * An official MCP server.
* **Ahead** (kac):
  * Value-level validation.
  * Cross-record reference and reciprocity checks.
  * Readable ids that can be cited.
  * Versioned packages consumed across organisations.
  * A taxonomy beyond compliance.
  * A plugin that serves the *content* to an agent at the moment it is used, rather than serving tool operations.

---

## 6. Closer analogues worth noting briefly

* **Trestle's governed documents** (section 5) are the nearest single feature to `kac validate` in this family:
  template-governed Markdown with a YAML header, checked in CI, and shown on `decisions/`. They are nearer than any ADR
  tool.
* **pyadr** is listed as covering the lifecycle of "proposal, acceptance, rejection, deprecation, and superseding"
  (https://adr.github.io/adr-tooling/). It is not researched further here.
* **Backstage ADR plugin**, which explores ADRs "across multiple orgs and repos" (https://adr.github.io/adr-tooling/),
  is the nearest ADR-world analogue to cross-corpus reading. It reads; it does not import or validate (inferred).

## Sources (all read 2026-10-27)

* https://adr.github.io/madr/
* https://raw.githubusercontent.com/adr/madr/main/template/adr-template.md
* https://raw.githubusercontent.com/adr/madr/main/template/adr-template-minimal.md
* https://raw.githubusercontent.com/adr/madr/main/CHANGELOG.md
* https://adr.github.io/adr-tooling/
* https://github.com/npryce/adr-tools
* https://api.github.com/repos/npryce/adr-tools and https://api.github.com/repos/npryce/adr-tools/tags
* https://api.github.com/repos/npryce/adr-tools/contents/src
* https://raw.githubusercontent.com/npryce/adr-tools/master/src/template.md
* https://raw.githubusercontent.com/npryce/adr-tools/master/src/_adr_help_new
* https://github.com/thomvaill/log4brains and
  https://raw.githubusercontent.com/thomvaill/log4brains/master/README.md
* https://raw.githubusercontent.com/thomvaill/log4brains/master/packages/init/assets/template.md
* https://api.github.com/repos/thomvaill/log4brains and its `/releases`
* https://pages.nist.gov/OSCAL/
* https://pages.nist.gov/OSCAL/learn/concepts/layer/control/profile/
* https://pages.nist.gov/OSCAL/learn/concepts/layer/implementation/component-definition/
* https://pages.nist.gov/OSCAL/learn/concepts/layer/implementation/ssp/
* https://pages.nist.gov/OSCAL/learn/concepts/processing/profile-resolution/
* https://pages.nist.gov/OSCAL-Reference/models/v1.2.3/{catalog,profile,component-definition,system-security-plan,assessment-plan,assessment-results,mapping}/json-outline/
* https://api.github.com/repos/usnistgov/OSCAL/releases and `/releases/tags/v1.2.0`
* https://github.com/usnistgov/oscal-cli
* https://github.com/usnistgov/oscal-content/tree/main/nist.gov/SP800-53/rev4
* https://github.com/usnistgov/OSCAL/issues/538
* https://github.com/awslabs/mcp-server-for-oscal
* https://oscal-compass.dev/compliance-trestle/latest/
* https://oscal-compass.dev/compliance-trestle/latest/tutorials/Trestle_authoring/ssp_profile_catalog_authoring/
* https://oscal-compass.dev/compliance-trestle/latest/tutorials/Trestle_authoring/trestle_author/
* https://api.github.com/repos/oscal-compass/compliance-trestle/releases
* https://github.com/oscal-compass/compliance-trestle-mcp
