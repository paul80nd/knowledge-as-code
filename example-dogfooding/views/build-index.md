# Rules the standards of example-dogfooding state

Before you write or change code, read each rule whose title bears on the change. Ask the standards-lookup skill for a rule's obligations, by its id.

Each heading gives a standard's id, title and status. Each line under it gives a rule's title, then its id.

## std-A11Y: The documentation site and kac are usable without sight, a mouse or colour (active)

- A change that makes either surface worse needs a deviation (`std-A11Y.a-change-that-makes-either-surface-worse-needs-a-deviation`)
- A Markdown author owns the structure of a page (`std-A11Y.a-markdown-author-owns-the-structure-of-a-page`)
- A pinned component carries an accessibility assessment (`std-A11Y.a-pinned-component-carries-an-accessibility-assessment`)
- kac carries no meaning in colour alone (`std-A11Y.kac-carries-no-meaning-in-colour-alone`)
- The theme owns contrast, focus and keyboard reach (`std-A11Y.the-theme-owns-contrast-focus-and-keyboard-reach`)
- What falls short is written down where a reader finds it (`std-A11Y.what-falls-short-is-written-down-where-a-reader-finds-it`)

## std-CI: Every check and every publish runs from a workflow in this repository (active)

- A job declares the permission it needs (`std-CI.a-job-declares-the-permission-it-needs`)
- A version moves by hand and publishes once (`std-CI.a-version-moves-by-hand-and-publishes-once`)
- A workflow holds no credential of its own (`std-CI.a-workflow-holds-no-credential-of-its-own`)
- Every action is pinned to a commit (`std-CI.every-action-is-pinned-to-a-commit`)
- One job holds the write permission (`std-CI.one-job-holds-the-write-permission`)
- The gate runs on every pull request into main (`std-CI.the-gate-runs-on-every-pull-request-into-main`)

## std-CONFIG: Configuration is committed, pinned and explained (active)

- A file says what it is for, and why each value is what it is (`std-CONFIG.a-file-says-what-it-is-for-and-why-each-value-is-what-it-is`)
- A pin names an exact version (`std-CONFIG.a-pin-names-an-exact-version`)
- A value living in more than one tree is copied and proved (`std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved`)
- Every value the build reads is committed (`std-CONFIG.every-value-the-build-reads-is-committed`)
- The shape of a YAML file is yamllint's to decide (`std-CONFIG.the-shape-of-a-yaml-file-is-yamllints-to-decide`)

## std-PLUGIN: The plugin carries the corpus, and a skill answers from what travelled (active)

- A deviation request an agent files asks for the owner it cannot name (`std-PLUGIN.a-deviation-request-an-agent-files-asks-for-the-owner-it-cannot-name`)
- A finding an agent files is an observation, and proposes no record (`std-PLUGIN.a-finding-an-agent-files-is-an-observation-and-proposes-no-record`)
- A framework finding travels stripped, and only by hand (`std-PLUGIN.a-framework-finding-travels-stripped-and-only-by-hand`)
- A skill assumes only that the session can read a file (`std-PLUGIN.a-skill-assumes-only-that-the-session-can-read-a-file`)
- A skill names the type of every field it describes (`std-PLUGIN.a-skill-names-the-type-of-every-field-it-describes`)
- A skill reads a rule's keyword as what departing from it costs (`std-PLUGIN.a-skill-reads-a-rules-keyword-as-what-departing-from-it-costs`)
- A skill says what its corpus did not carry (`std-PLUGIN.a-skill-says-what-its-corpus-did-not-carry`)
- A skill writes back to the tracker the corpus states (`std-PLUGIN.a-skill-writes-back-to-the-tracker-the-corpus-states`)
- plugin.json declares every component and the types it reads (`std-PLUGIN.pluginjson-declares-every-component-and-the-types-it-reads`)
- The bundle carries a frozen export, and nothing writes to it (`std-PLUGIN.the-bundle-carries-a-frozen-export-and-nothing-writes-to-it`)
- Triage routes a finding with a label, and says why in a comment (`std-PLUGIN.triage-routes-a-finding-with-a-label-and-says-why-in-a-comment`)

## std-PROSE: Prose reads the same way whoever wrote it (active)

- Commit messages and pull requests (`std-PROSE.commit-messages-and-pull-requests`)
- The base style (`std-PROSE.the-base-style`)
- What prose must not change (`std-PROSE.what-prose-must-not-change`)
- What stays true (`std-PROSE.what-stays-true`)
- Where this repository differs from Microsoft (`std-PROSE.where-this-repository-differs-from-microsoft`)

## std-VERS: A version stamp says what it versions, and a move says what changed (active)

- A producer's move obliges every consumer in the same pull request (`std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request`)
- A published version is never replaced (`std-VERS.a-published-version-is-never-replaced`)
- A reader meeting a stamp it does not know stops (`std-VERS.a-reader-meeting-a-stamp-it-does-not-know-stops`)
- A stamp is a semantic version, a count, or a range (`std-VERS.a-stamp-is-a-semantic-version-a-count-or-a-range`)
- What a move of each stamp means (`std-VERS.what-a-move-of-each-stamp-means`)
- Which stamps move together, and which move alone (`std-VERS.which-stamps-move-together-and-which-move-alone`)

## eng:std-API: An interface is described by a contract in the repository (draft)

- A change carries a version and a notice (`eng:std-API.a-change-carries-a-version-and-a-notice`)
- Every endpoint authenticates and validates (`eng:std-API.every-endpoint-authenticates-and-validates`)
- The contract is the source of truth (`eng:std-API.the-contract-is-the-source-of-truth`)

## eng:std-CONT: A container image is pinned by digest and runs as a non-root user (draft)

- A published tag never moves (`eng:std-CONT.a-published-tag-never-moves`)
- The base image is chosen and pinned (`eng:std-CONT.the-base-image-is-chosen-and-pinned`)
- The container holds only what the service needs (`eng:std-CONT.the-container-holds-only-what-the-service-needs`)

## eng:std-CSSTY: C# follows the runtime team's conventions, and the compiler enforces them (draft)

- A suppression is local and says why (`eng:std-CSSTY.a-suppression-is-local-and-says-why`)
- The conventions come from one file (`eng:std-CSSTY.the-conventions-come-from-one-file`)
- The security rules the analysers cover (`eng:std-CSSTY.the-security-rules-the-analysers-cover`)

## eng:std-DEPLOY: One artefact is built once and promoted to production (draft)

- A way back exists before the change goes (`eng:std-DEPLOY.a-way-back-exists-before-the-change-goes`)
- Configuration sits outside the artefact (`eng:std-DEPLOY.configuration-sits-outside-the-artefact`)
- Production changes through the pipeline alone (`eng:std-DEPLOY.production-changes-through-the-pipeline-alone`)
- The artefact is built once (`eng:std-DEPLOY.the-artefact-is-built-once`)

## eng:std-DEPS: A dependency is pinned, screened and named in the inventory (draft)

- What happens afterwards (`eng:std-DEPS.what-happens-afterwards`)
- What happens before adoption (`eng:std-DEPS.what-happens-before-adoption`)
- Where a package comes from (`eng:std-DEPS.where-a-package-comes-from`)

## eng:std-GATES: A failing check blocks the merge (draft)

- A failing check stops the change (`eng:std-GATES.a-failing-check-stops-the-change`)
- Every change is built and tested automatically (`eng:std-GATES.every-change-is-built-and-tested-automatically`)

## eng:std-INCSEV: Every incident is handled at a stated severity (draft)

- A significant incident is reviewed (`eng:std-INCSEV.a-significant-incident-is-reviewed`)
- Every incident has a severity (`eng:std-INCSEV.every-incident-has-a-severity`)
- The severity moves with what is known (`eng:std-INCSEV.the-severity-moves-with-what-is-known`)
- The severity sets the response (`eng:std-INCSEV.the-severity-sets-the-response`)

## eng:std-NETTST: A .NET test project runs under one framework, and an integration test owns its dependencies (draft)

- A unit test crosses no boundary (`eng:std-NETTST.a-unit-test-crosses-no-boundary`)
- An integration test starts what it needs (`eng:std-NETTST.an-integration-test-starts-what-it-needs`)
- Coverage is collected by the build (`eng:std-NETTST.coverage-is-collected-by-the-build`)
- The framework and the project layout (`eng:std-NETTST.the-framework-and-the-project-layout`)

## eng:std-OBS: A service ships its telemetry to one place, keyed by one id (draft)

- Everything lands in the central store (`eng:std-OBS.everything-lands-in-the-central-store`)
- One request reads as one timeline (`eng:std-OBS.one-request-reads-as-one-timeline`)
- Somebody acts on every alert (`eng:std-OBS.somebody-acts-on-every-alert`)
- Telemetry carries no personal data (`eng:std-OBS.telemetry-carries-no-personal-data`)

## eng:std-PR: A pull request carries its reasoning and one accountable approval (draft)

- A change arrives as a pull request (`eng:std-PR.a-change-arrives-as-a-pull-request`)
- Agent-produced work says what produced it (`eng:std-PR.agent-produced-work-says-what-produced-it`)
- Somebody other than the author approves it (`eng:std-PR.somebody-other-than-the-author-approves-it`)

## eng:std-SECRET: A secret is read from the store at run time (draft)

- A secret comes from the store (`eng:std-SECRET.a-secret-comes-from-the-store`)
- Every secret rotates (`eng:std-SECRET.every-secret-rotates`)
- Nowhere else holds a secret (`eng:std-SECRET.nowhere-else-holds-a-secret`)

## eng:std-TEST: A test runs at the lowest level that can catch the fault (draft)

- Know what the suite reaches (`eng:std-TEST.know-what-the-suite-reaches`)
- Pick the level from the fault (`eng:std-TEST.pick-the-level-from-the-fault`)
- The data is never a real customer's (`eng:std-TEST.the-data-is-never-a-real-customers`)

## eng:std-VCS: Everything a service needs is committed to its repository (draft)

- Every change is attributable and reviewed (`eng:std-VCS.every-change-is-attributable-and-reviewed`)
- The repository holds everything the service needs (`eng:std-VCS.the-repository-holds-everything-the-service-needs`)
