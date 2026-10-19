# Rules the standards of example-payments state

Before you write or change code, read each rule whose title bears on the change. Ask the standards-lookup skill for a rule's obligations, by its id.

Each heading gives a standard's id, title and status. Each line under it gives a rule's title, then its id.

## std-CARD: Card details reach the PSP and never reach us (active)

- Nothing we own stores a card (`std-CARD.nothing-we-own-stores-a-card`)
- The card goes straight to the PSP (`std-CARD.the-card-goes-straight-to-the-psp`)
- The split of responsibility is written down (`std-CARD.the-split-of-responsibility-is-written-down`)

## std-IDEM: One order pays once, however many times the request arrives (active)

- An in-flight repeat waits or is told to wait (`std-IDEM.an-in-flight-repeat-waits-or-is-told-to-wait`)
- The caller chooses the key (`std-IDEM.the-caller-chooses-the-key`)
- The service answers a repeat from its record (`std-IDEM.the-service-answers-a-repeat-from-its-record`)

## std-LEDGER: A ledger entry is written once and corrected by another entry (active)

- An entry is a balanced pair (`std-LEDGER.an-entry-is-a-balanced-pair`)
- An entry says what produced it (`std-LEDGER.an-entry-says-what-produced-it`)
- Nothing amends an entry (`std-LEDGER.nothing-amends-an-entry`)

## std-PSPOUT: A call to the PSP is bounded, and an unknown outcome is resolved (active)

- A retry is bounded and backs off (`std-PSPOUT.a-retry-is-bounded-and-backs-off`)
- An unknown outcome is resolved (`std-PSPOUT.an-unknown-outcome-is-resolved`)
- Every call has a deadline (`std-PSPOUT.every-call-has-a-deadline`)

## std-RECON: The ledger is reconciled against the PSP every day (active)

- A break is named and owned (`std-RECON.a-break-is-named-and-owned`)
- Nothing downstream reads an unreconciled day (`std-RECON.nothing-downstream-reads-an-unreconciled-day`)
- The run happens every day (`std-RECON.the-run-happens-every-day`)

## std-RETAIN: A payment record is kept for seven years and nothing else is (active)

- An erasure request is answered (`std-RETAIN.an-erasure-request-is-answered`)
- Deletion happens without anyone asking (`std-RETAIN.deletion-happens-without-anyone-asking`)
- Each store has one period (`std-RETAIN.each-store-has-one-period`)

## std-SCA: The PSP decides whether the cardholder is challenged (active)

- The challenge is somebody else's page (`std-SCA.the-challenge-is-somebody-elses-page`)
- The outcome travels with the payment (`std-SCA.the-outcome-travels-with-the-payment`)
- The PSP runs the authentication (`std-SCA.the-psp-runs-the-authentication`)

## std-TELEM: A payment leaves an order reference in the logs and nothing else (active)

- A PSP key is a secret and behaves like one (`std-TELEM.a-psp-key-is-a-secret-and-behaves-like-one`)
- The redaction runs before the sink (`std-TELEM.the-redaction-runs-before-the-sink`)
- What a log line may carry (`std-TELEM.what-a-log-line-may-carry`)

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
