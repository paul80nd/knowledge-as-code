# Rules the standards of example-engineering state

Before you write or change code, read each rule whose title bears on the change. Ask the standards-lookup skill for a rule's obligations, by its id.

Each heading gives a standard's id, title and status. Each line under it gives a rule's title, then its id.

## std-API: An interface is described by a contract in the repository (draft)

- A change carries a version and a notice (`std-API.a-change-carries-a-version-and-a-notice`)
- Every endpoint authenticates and validates (`std-API.every-endpoint-authenticates-and-validates`)
- The contract is the source of truth (`std-API.the-contract-is-the-source-of-truth`)

## std-CONT: A container image is pinned by digest and runs as a non-root user (draft)

- A published tag never moves (`std-CONT.a-published-tag-never-moves`)
- The base image is chosen and pinned (`std-CONT.the-base-image-is-chosen-and-pinned`)
- The container holds only what the service needs (`std-CONT.the-container-holds-only-what-the-service-needs`)

## std-CSSTY: C# follows the runtime team's conventions, and the compiler enforces them (draft)

- A suppression is local and says why (`std-CSSTY.a-suppression-is-local-and-says-why`)
- The conventions come from one file (`std-CSSTY.the-conventions-come-from-one-file`)
- The security rules the analysers cover (`std-CSSTY.the-security-rules-the-analysers-cover`)

## std-DEPLOY: One artefact is built once and promoted to production (draft)

- A way back exists before the change goes (`std-DEPLOY.a-way-back-exists-before-the-change-goes`)
- Configuration sits outside the artefact (`std-DEPLOY.configuration-sits-outside-the-artefact`)
- Production changes through the pipeline alone (`std-DEPLOY.production-changes-through-the-pipeline-alone`)
- The artefact is built once (`std-DEPLOY.the-artefact-is-built-once`)

## std-DEPS: A dependency is pinned, screened and named in the inventory (draft)

- What happens afterwards (`std-DEPS.what-happens-afterwards`)
- What happens before adoption (`std-DEPS.what-happens-before-adoption`)
- Where a package comes from (`std-DEPS.where-a-package-comes-from`)

## std-GATES: A failing check blocks the merge (draft)

- A failing check stops the change (`std-GATES.a-failing-check-stops-the-change`)
- Every change is built and tested automatically (`std-GATES.every-change-is-built-and-tested-automatically`)

## std-INCSEV: Every incident is handled at a stated severity (draft)

- A significant incident is reviewed (`std-INCSEV.a-significant-incident-is-reviewed`)
- Every incident has a severity (`std-INCSEV.every-incident-has-a-severity`)
- The severity moves with what is known (`std-INCSEV.the-severity-moves-with-what-is-known`)
- The severity sets the response (`std-INCSEV.the-severity-sets-the-response`)

## std-NETTST: A .NET test project runs under one framework, and an integration test owns its dependencies (draft)

- A unit test crosses no boundary (`std-NETTST.a-unit-test-crosses-no-boundary`)
- An integration test starts what it needs (`std-NETTST.an-integration-test-starts-what-it-needs`)
- Coverage is collected by the build (`std-NETTST.coverage-is-collected-by-the-build`)
- The framework and the project layout (`std-NETTST.the-framework-and-the-project-layout`)

## std-OBS: A service ships its telemetry to one place, keyed by one id (draft)

- Everything lands in the central store (`std-OBS.everything-lands-in-the-central-store`)
- One request reads as one timeline (`std-OBS.one-request-reads-as-one-timeline`)
- Somebody acts on every alert (`std-OBS.somebody-acts-on-every-alert`)
- Telemetry carries no personal data (`std-OBS.telemetry-carries-no-personal-data`)

## std-PR: A pull request carries its reasoning and one accountable approval (draft)

- A change arrives as a pull request (`std-PR.a-change-arrives-as-a-pull-request`)
- Agent-produced work says what produced it (`std-PR.agent-produced-work-says-what-produced-it`)
- Somebody other than the author approves it (`std-PR.somebody-other-than-the-author-approves-it`)

## std-SECRET: A secret is read from the store at run time (draft)

- A secret comes from the store (`std-SECRET.a-secret-comes-from-the-store`)
- Every secret rotates (`std-SECRET.every-secret-rotates`)
- Nowhere else holds a secret (`std-SECRET.nowhere-else-holds-a-secret`)

## std-TEST: A test runs at the lowest level that can catch the fault (draft)

- Know what the suite reaches (`std-TEST.know-what-the-suite-reaches`)
- Pick the level from the fault (`std-TEST.pick-the-level-from-the-fault`)
- The data is never a real customer's (`std-TEST.the-data-is-never-a-real-customers`)

## std-VCS: Everything a service needs is committed to its repository (draft)

- Every change is attributable and reviewed (`std-VCS.every-change-is-attributable-and-reviewed`)
- The repository holds everything the service needs (`std-VCS.the-repository-holds-everything-the-service-needs`)
