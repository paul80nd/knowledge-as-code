# The knowledge of Example Dogfooding

This is what the corpus records, type by type. Each heading gives the id to cite it by.

## standards

### An interface is described by a contract in the repository (`std-API`)

Status: draft.

#### Summary

Every interface we publish has a machine-readable contract, committed beside the code. The build checks the implementation against that contract. A change a consumer cannot absorb ships as a new version.

#### A change carries a version and a notice (`eng:std-API.a-change-carries-a-version-and-a-notice`)

obligations:
- A published interface **MUST** have a version a consumer can pin to.
- An HTTP interface **SHOULD** put its major version in the path, as `/v2/`, unless the contract states another scheme.
- A breaking change **MUST** ship as a new version, with the previous version still serving requests.
- A team **MUST** give notice before it removes a version, on the timescale the deprecation approach states.
- A deprecated version **SHOULD** respond with a `Deprecation` header and a `Sunset` header, so a consumer sees the notice in the response.

#### Every endpoint authenticates and validates (`eng:std-API.every-endpoint-authenticates-and-validates`)

obligations:
- A contract **MUST** state the authentication each endpoint takes.
- A contract **MUST** declare an endpoint that takes no authentication as public.
- An endpoint **MUST** authenticate the caller before it does any work, unless the contract declares it public.
- An endpoint **MUST** authorise the operation before it does any work, unless the contract declares it public.
- An endpoint **MUST** validate each field it receives against the contract, and reject a request that does not match.
- An interface **MUST NOT** serve personal data, a secret, or an operation that changes state, to an unauthenticated caller.

#### The contract is the source of truth (`eng:std-API.the-contract-is-the-source-of-truth`)

obligations:
- A service **MUST** publish a machine-readable contract for every interface it exposes, committed to the repository that builds the service.
- Where the implementation and the contract disagree, a team **MUST** treat the implementation as the defect.
- A build **MUST** fail where the implementation no longer matches the contract.
- A consumer **SHOULD** test its use of an interface against the published contract, so the test fails on what the provider promised.
- A consumer **MAY** also test against a running instance of the provider.

### A container image is pinned by digest and runs as a non-root user (`std-CONT`)

Status: draft.

#### Summary

An image is built from a base pinned by digest. The pipeline tags it with the build's own version. That tag never moves. The container runs as a non-root user and opens one port, the one the service listens on.

#### A published tag never moves (`eng:std-CONT.a-published-tag-never-moves`)

obligations:
- A pipeline **MUST** tag an image with the build's own version.
- A registry **MUST** refuse a second push to a tag that already exists.
- A deployment **MUST** name an image by digest, so the running container is the one the pipeline built.

#### The base image is chosen and pinned (`eng:std-CONT.the-base-image-is-chosen-and-pinned`)

obligations:
- A Dockerfile **MUST** take its base image from the organisation's registry.
- A Dockerfile **MUST** pin the base image by digest rather than by a moving tag.
- A repository **MUST** rebuild against a refreshed base at least monthly, so a patched base is deployed to the running service.

#### The container holds only what the service needs (`eng:std-CONT.the-container-holds-only-what-the-service-needs`)

obligations:
- An image **MUST** declare a non-root `USER`.
- A container **MUST** run as that user.
- A container **MUST** run with a read-only root filesystem, writing only to a declared volume.
- An image **MUST** be built from a runtime base rather than an SDK base, so no compiler ships to production.
- An image **MUST NOT** expose a management, debug or metrics port to anything outside the cluster.

### C# follows the runtime team's conventions, and the compiler enforces them (`std-CSSTY`)

Status: draft.

#### Summary

C# in this estate is written to the .NET runtime team's own coding style, set out as an `.editorconfig` in the repository. Analysers run as part of the build. A warning is a defect.

#### A suppression is local and says why (`eng:std-CSSTY.a-suppression-is-local-and-says-why`)

obligations:
- A suppression **MUST** sit on the member it applies to, with a `Justification` giving the reason.
- A repository **MUST NOT** disable a rule in `.editorconfig` to clear a warning in one file.

#### The conventions come from one file (`eng:std-CSSTY.the-conventions-come-from-one-file`)

obligations:
- A repository holding C# **MUST** have an `.editorconfig` at its root.
- That `.editorconfig` **SHOULD** be the .NET runtime team's own.
- A repository **SHOULD** state each departure from the runtime team's file in a comment beside it.
- A project **MUST** set `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild`, so the build checks the style.
- A project **MUST** enable nullable reference types.
- Code **MUST NOT** silence a nullability warning with `!`, except in a test asserting that a null argument is refused.
- A project **MUST** enable the .NET analysers at the `latest` analysis level.

#### The security rules the analysers cover (`eng:std-CSSTY.the-security-rules-the-analysers-cover`)

obligations:
- Code **MUST** build a SQL command through a parameter rather than by joining strings.
- Code **MUST** use the framework's cryptography.
- Code **MUST NOT** call an algorithm the analysers report as broken.
- Code **MUST** compare a secret with `CryptographicOperations.FixedTimeEquals` rather than with `==`.

### One artefact is built once and promoted to production (`std-DEPLOY`)

Status: draft.

#### Summary

The pipeline builds an artefact once, then promotes that same artefact through each environment. Configuration comes from outside the artefact. The pipeline is the only route into production.

#### A way back exists before the change goes (`eng:std-DEPLOY.a-way-back-exists-before-the-change-goes`)

obligations:
- A release **MUST** have a stated rollback or recovery path before it starts.
- A database migration **MUST** leave the previous version of the service able to run against the new schema, unless the migration removes data the previous version reads.

#### Configuration sits outside the artefact (`eng:std-DEPLOY.configuration-sits-outside-the-artefact`)

obligations:
- A service **MUST** read environment-specific configuration at start-up, from outside the artefact.
- An artefact **MUST NOT** contain a hostname, connection string or feature setting that differs between environments.

#### Production changes through the pipeline alone (`eng:std-DEPLOY.production-changes-through-the-pipeline-alone`)

obligations:
- A change **MUST** reach production through the automated pipeline.
- A change **MUST** reach each environment below production by promotion from the one before it.
- Every environment **MUST** be provisioned from the same definition, parameterised per environment.
- The pipeline definition **MUST** be committed to the repository, and reviewed like the code.
- A team **MUST NOT** hand-edit production code, configuration, infrastructure or schema, other than a flag change made under [pol-PIPE].FLAGS.
- A team changing production by hand to restore service, where the pipeline is unavailable or its delay would extend an outage, **MUST** record a deviation at the time.
- A team **MUST** commit a hand change made to restore service to the repository before the incident closes.

#### The artefact is built once (`eng:std-DEPLOY.the-artefact-is-built-once`)

obligations:
- A pipeline **MUST** build the artefact once, and promote that build to every environment.
- An artefact **MUST** record the commit it was built from, so a running version identifies the change it came from.
- A deployment record **MUST** name the artefact, the change and the approval behind it.
- A pipeline **MUST NOT** deploy an artefact built anywhere but the pipeline.

### A dependency is pinned, screened and named in the inventory (`std-DEPS`)

Status: draft.

#### Summary

Every third-party package a build pulls in is pinned to a version and comes from a feed we control. Somebody screens it for vulnerabilities and licence terms before anyone adopts it. A critical finding stops the release.

#### What happens afterwards (`eng:std-DEPS.what-happens-afterwards`)

obligations:
- A pipeline **MUST** scan the dependency tree on every build and on a weekly schedule.
- A team **MUST** rank a finding by its severity and by whether the affected path runs.
- A team **MUST** close a critical finding within 7 days and a high finding within 14.
- A team **MUST NOT** release with an open critical finding, absent a recorded deviation naming who accepts the risk.

#### What happens before adoption (`eng:std-DEPS.what-happens-before-adoption`)

obligations:
- Somebody **MUST** screen a new package for known vulnerabilities before the pull request that adds it merges.
- Somebody **MUST** check a new package's licence against the allowed list before that pull request merges.
- A pull request adding a package **MUST** say what the package is for, so a reviewer can weigh it against writing the code.

#### Where a package comes from (`eng:std-DEPS.where-a-package-comes-from`)

obligations:
- A build **MUST** resolve packages from the organisation's feed, which proxies the public registries.
- A repository **MUST** commit a lockfile pinning every direct and transitive dependency to a version.
- A build **MUST** install from the lockfile, and fail where the lockfile and the manifest disagree.
- A build **MUST** publish its artefacts to the managed repository, versioned and retained.
- A team **MUST NOT** add a package from a personal feed, a git URL or an archive downloaded by hand.

### A failing check blocks the merge (`std-GATES`)

Status: draft.

#### Summary

Every push builds the code and runs the test suite automatically. A check that fails stops the change. Turning a check off takes a recorded deviation with a named owner.

#### A failing check stops the change (`eng:std-GATES.a-failing-check-stops-the-change`)

obligations:
- Branch policy **MUST** block a merge while any required check is failing.
- A team **MUST NOT** merge over a failing check without a recorded deviation.
- A team **MUST NOT** skip, silence or suppress a check without a recorded deviation.
- A deviation covering a suppressed check **MUST** name the individual who accepts the risk.
- That deviation **MUST** have a review date.

#### Every change is built and tested automatically (`eng:std-GATES.every-change-is-built-and-tested-automatically`)

obligations:
- A push to any branch **MUST** trigger a build and the test suite, without anyone asking for it.
- The build **MUST** run from a clean checkout, on an agent that is not a person's own machine.
- The agent **SHOULD** be provisioned from a definition in the repository, because a change to it is then reviewed like the code.
- A fix for a defect **MUST** arrive with a test that fails without it.

### Every incident is handled at a stated severity (`std-INCSEV`)

Status: draft.

#### Summary

An incident takes one of three severities when it is declared. The severity decides who is paged, how often the people affected hear about it, and whether it gets a postmortem. Wherever a policy here says a significant incident, it means sev1 or sev2.

#### Every incident has a severity (`eng:std-INCSEV.every-incident-has-a-severity`)

obligations:
- Whoever declares an incident **MUST** set its severity at declaration.
- An incident **MUST** be sev1 where a `critical` service is unavailable, or where customers' money has been taken, refunded or paid out wrongly, or their personal data is wrong or exposed, at any scale.
- An incident **MUST** be sev2 where a service is degraded or a fault reaches some customers, and a workaround exists or the harm is contained.
- An incident **MUST** be sev3 where no customer is affected, or one customer is affected with no lasting harm.
- An incident meeting none of these **MUST** be sev2 until the incident lead sets another level.

### A .NET test project runs under one framework, and an integration test owns its dependencies (`std-NETTST`)

Status: draft.

#### Summary

A repository's C# tests use one test framework and one assertion style. A unit test crosses no process boundary. An integration test starts the dependencies it needs, and shares none of them with another run.

#### A unit test crosses no boundary (`eng:std-NETTST.a-unit-test-crosses-no-boundary`)

obligations:
- A unit test **MUST NOT** open a socket, read the filesystem, or start a container.
- A test **MUST NOT** call `Thread.Sleep` or `Task.Delay` to wait for something.
- A test **SHOULD** take its time from an injected clock, such as `TimeProvider`.
- A test **MUST** await every asynchronous call, so the test that caused a failure is the test that reports it.

#### An integration test starts what it needs (`eng:std-NETTST.an-integration-test-starts-what-it-needs`)

obligations:
- An integration test **MUST** start the dependencies it needs.
- An integration test **MUST NOT** share an instance of a dependency with another run.
- An integration test **SHOULD** start those dependencies with Testcontainers.
- An integration test for an ASP.NET service **SHOULD** host it in the test process with `WebApplicationFactory`.
- An integration test **MUST** leave no state behind, so the suite passes when it runs twice.

#### Coverage is collected by the build (`eng:std-NETTST.coverage-is-collected-by-the-build`)

obligations:
- A build **MUST** collect coverage from its test run and publish the report.
- A build **SHOULD** collect it with `dotnet test --collect:"XPlat Code Coverage"`, which writes a Cobertura report.
- A test project **MUST** be excluded from its own coverage figure.

#### The framework and the project layout (`eng:std-NETTST.the-framework-and-the-project-layout`)

obligations:
- A repository **MUST** use one test framework across its test projects.
- Every test project **MUST** run under `dotnet test`.
- A test project **SHOULD** use xUnit, because the estate's examples and controls are written against it.
- A test project **MUST** be named for the project it tests, with a `.Tests` or `.IntegrationTests` suffix.
- A project **MUST** use one assertion style throughout its tests.
- A project **SHOULD** assert with the test framework's own `Assert`.
- A project **MAY** assert with AwesomeAssertions, or with another library whose licence is on the allowed list [std-DEPS.what-happens-before-adoption] checks a new package against.

### A service ships its telemetry to one place, keyed by one id (`std-OBS`)

Status: draft.

#### Summary

Every service emits structured logs, traces and metrics to the central platform. Each record has a timestamp stating its offset from UTC, and a trace id that follows the request across services. An alert names an owner and the first action to take.

#### Everything lands in the central store (`eng:std-OBS.everything-lands-in-the-central-store`)

obligations:
- A service **MUST** emit its logs, traces and metrics to the central platform.
- A service **MUST NOT** keep the only copy of a log, so that losing the host still leaves the evidence.
- A service **MUST NOT** go into production without health monitoring and at least one alert on it.

#### One request reads as one timeline (`eng:std-OBS.one-request-reads-as-one-timeline`)

obligations:
- A service **MUST** stamp every record with a timestamp taken from a synchronised clock, stating its offset from UTC.
- A service **SHOULD** write that timestamp in UTC, because records from several services then sort without conversion.
- A service **MUST** accept an inbound `traceparent`.
- A service **MUST** pass that `traceparent` to everything it calls.
- A log line **MUST** include the trace id, so a search on one request returns every service that handled it.
- A service **MUST** emit structured fields, not a formatted sentence, so a search can filter on a value.

#### Somebody acts on every alert (`eng:std-OBS.somebody-acts-on-every-alert`)

obligations:
- A service **MUST** publish availability and latency objectives.
- A service **MUST** alert when it is on course to miss one of those objectives.
- An alert **MUST** name the accountable owner and the first action to take.
- A team **MUST** delete or fix an alert nobody acts on, and not leave it to be filtered.
- A team **MUST** retain telemetry for the period the repository records against the service.

#### Telemetry carries no personal data (`eng:std-OBS.telemetry-carries-no-personal-data`)

obligations:
- A service **MUST** redact unmasked personal data before the record is written.
- A service **MUST** keep every secret, credential and token out of a log line, a trace attribute and a metric label. [std-SECRET.nowhere-else-holds-a-secret] states where a secret may appear.

### A pull request carries its reasoning and one accountable approval (`std-PR`)

Status: draft.

#### Summary

Every change to the default branch arrives as a pull request that says what it does and why. A named person other than the author approves it before it merges.

#### A change arrives as a pull request (`eng:std-PR.a-change-arrives-as-a-pull-request`)

obligations:
- A change **MUST** arrive on the default branch as a merged pull request, not as a direct push.
- A pull request description **MUST** link the work item that asked for the change.

#### Agent-produced work says what produced it (`eng:std-PR.agent-produced-work-says-what-produced-it`)

obligations:
- A pull request containing agent-produced work **MUST** name the agent, the model and the prompt or task that produced it.
- Agent-produced work **MUST** have the approval of a named person, who owns the change afterwards.
- A reviewer **MUST NOT** accept an agent's own summary of a change as evidence that the change is correct.
- Branch policy **MUST** apply to agent-produced work unchanged, with no exemption and no second route in.
- The identity that opened an agent-produced pull request **MUST NOT** approve or merge it.

#### Somebody other than the author approves it (`eng:std-PR.somebody-other-than-the-author-approves-it`)

obligations:
- A pull request **MUST** have at least one approval from somebody other than the author before it merges.
- A reviewer **MUST** check the change against the standards that apply to it, and not read for style alone.
- A reviewer **MUST** state what they checked when they approve.

### A secret is read from the store at run time (`std-SECRET`)

Status: draft.

#### Summary

A service reads every secret from the managed store, at start-up or when it needs one. It authenticates with an identity granted to that service in that environment. Nothing else keeps a secret.

#### A secret comes from the store (`eng:std-SECRET.a-secret-comes-from-the-store`)

obligations:
- A service **MUST** read every secret from the managed store, through an identity granted to the workload.
- An environment below production **MUST** have its own secrets, distinct from production's.

#### Every secret rotates (`eng:std-SECRET.every-secret-rotates`)

obligations:
- A secret **MUST** be replaceable in the store without a code change or a rebuild.
- Every secret **MUST** rotate on a period recorded against it in the store.

#### Nowhere else holds a secret (`eng:std-SECRET.nowhere-else-holds-a-secret`)

obligations:
- A repository **MUST NOT** contain a secret, in source, in a configuration file, in a pipeline definition or in a test fixture.
- A build **MUST NOT** embed a secret in an artefact or an image.
- A service **MUST NOT** write a secret to a log, to a console, to an error message or to a support ticket.
- A pipeline **MUST** run a secret scanner over the repository and its history, and fail on a finding.

### A test runs at the lowest level that can catch the fault (`std-TEST`)

Status: draft.

#### Summary

A team writes each test at the cheapest level that can fail on the fault it looks for. The whole suite runs on data that was never a real customer's.

#### Know what the suite reaches (`eng:std-TEST.know-what-the-suite-reaches`)

obligations:
- A build **MUST** report line coverage.
- A build **MUST** fail where coverage falls below the figure the repository records.
- A team **MUST** read a coverage drop as a gap in the tests rather than as a threshold to lower.

#### Pick the level from the fault (`eng:std-TEST.pick-the-level-from-the-fault`)

obligations:
- A team **MUST** cover a rule about one unit's behaviour with a test on that unit alone.
- A team **MUST** cover an agreement between two components with a test that exercises both.
- A team **MUST** keep the tests that run on every push under ten minutes, so the answer arrives while the work is still open.
- A test **MUST** fail for one reason.
- A test's name **MUST** say what that reason is.
- A test **MUST NOT** depend on the order the suite runs in, on the wall clock, or on a service somebody else deploys.

#### The data is never a real customer's (`eng:std-TEST.the-data-is-never-a-real-customers`)

obligations:
- A test **MUST** run against generated data, or against production data that a masking step has already processed.
- A team **MUST NOT** copy unmasked production data into a test fixture, a seed script or a local database.
- A team **MUST NOT** run a test, a script or a debugger against production.

### Everything a service needs is committed to its repository (`std-VCS`)

Status: draft.

#### Summary

One repository contains everything that builds, deploys, runs and recovers a service. Every change to it arrives by reviewed merge from an identified author.

#### Every change is attributable and reviewed (`eng:std-VCS.every-change-is-attributable-and-reviewed`)

obligations:
- The default branch **MUST** refuse a direct push, so that every change arrives as a merge.
- A commit **MUST** name the work item that asked for the change.
- A commit **SHOULD** name it in the subject as `#<id>`, because the forge then links the commit to the item.
- A commit **MUST** be made under the author's own verified identity.
- A commit **MUST NOT** be authored by a shared or generic account.

#### The repository holds everything the service needs (`eng:std-VCS.the-repository-holds-everything-the-service-needs`)

obligations:
- A repository **MUST** contain the source, the build definition, the infrastructure definition, the configuration templates and the recovery scripts for the services it owns.
- A repository **MUST NOT** depend on an asset kept only on a workstation, in a cloud portal or in an unversioned share.
- Infrastructure, schema and configuration changes **MUST** go through the same review as application code.

## glossary

### Engineering (`gls-engineering`)

#### Could (`eng:gls-engineering.could`)

definition: The weakest obligation level a clause takes, below `SHOULD`, `MUST` and `MUST NOT`. It marks something we would like and have not committed to. A team asking to take one on under continuous improvement is told yes, and that it is worth a little time.
not: RFC 2119's MAY, which expresses no preference either way. A `COULD` here expresses a preference for doing it.

#### May (`eng:gls-engineering.may`)

definition: The level a rule in a standard takes to permit something a reader might otherwise think is ruled out. A policy grants no permission, so a policy clause never takes it.
not: a `COULD`, which states something we would like and permits nothing.

#### Must (`eng:gls-engineering.must`)

definition: The level a clause takes where a framework we are obliged to requires it, or where not doing it would be reckless. A
`MUST` binds us, where `SHOULD` and `COULD` state good practice. No team negotiates one down.

#### Must not (`eng:gls-engineering.must-not`)

definition: A `MUST` written as a prohibition, with the same weight and the same test. Some prohibitions admit no deviation at all. A policy lists those in its Exceptions section.
not: a firmly worded `SHOULD`.

#### Personal data (`eng:gls-engineering.personal-data`)

definition: Information about a living person who is identified, or who could be identified from it. Identification counts any means reasonably likely to be used, by us or by another person, as [UK GDPR] Article 4(1) and recital 26 set out.
avoid: PII, a US term with a different scope. Write personal data.
not: sensitive personal data, which is a narrower set inside it. A name, a postal address and an email address are personal data, and none of them is sensitive.

#### Secret (`eng:gls-engineering.secret`)

definition: A value that grants access, so a leak lets somebody in: a password, an API key, a session token, a certificate private key, a connection string. [pol-SCRT] states where one is stored.
not: sensitive data. A secret protects data. It is not the data it protects.

#### Sensitive personal data (`eng:gls-engineering.sensitive-personal-data`)

definition: The special categories [UK GDPR] sets apart: health, sex life, sexual orientation, racial or ethnic origin, political opinion, religious or philosophical belief, trade union membership, genetic data, and biometric data used to identify someone. This estate treats criminal offence data the same way.
also: special category data, which is what [UK GDPR] calls it.
not: personal data, which is the wider class. [pol-DATA].LOGS bars only this narrower set from a log line.

#### Should (`eng:gls-engineering.should`)

definition: The level a clause takes where a capable engineering function would do this, and we would spend time to meet it. A team asking to take one on under continuous improvement is told yes, and that it is worth real time.
not: a `MUST` we are being polite about, or the `SHOULD` in a standard. Nothing in a policy obliges us to a
`SHOULD`. A team that has not met one yet is maturing, and is not in breach. In a [standard](../standards.md), a
`SHOULD` names the usual way to reach that standard's `MUST`, and whoever departs from it says why in the change.

## policies

### Access is by individual identity, on least privilege (`pol-ACCS`)

#### `eng:pol-ACCS.NAMED`

clause: MUST grant access to a named individual or a distinctly identified workload, never to a shared persona
level: MUST

#### `eng:pol-ACCS.AUTHN`

clause: MUST require more than one factor wherever a person authenticates to our systems, our code or our data
level: MUST

#### `eng:pol-ACCS.PERSIST`

clause: MUST NOT leave standing production access in place beyond what the role genuinely requires
level: MUST NOT

### Agents propose, people decide (`pol-AGNT`)

#### `eng:pol-AGNT.UNPROV`

clause: MUST NOT accept a proposal we cannot trace back to what produced it
level: MUST NOT

#### `eng:pol-AGNT.ACCESS`

clause: MUST NOT grant an agent access, privilege or a route to production that an individual doing the same work would not be granted
level: MUST NOT

### Every change is verified automatically, and failures block (`pol-AUTV`)

#### `eng:pol-AUTV.REPRO`

clause: MUST be able to reproduce a build from version control alone, on any machine set up to do so
level: MUST

#### `eng:pol-AUTV.MAPPED`

clause: MUST be able to trace every stated requirement to the test that validates it
level: MUST

#### `eng:pol-AUTV.BROKEN`

clause: MUST treat a broken default branch as the team's first priority
level: MUST

#### `eng:pol-AUTV.OFTEN`

clause: SHOULD integrate to the default branch often enough that any one change is small enough to reason about
level: SHOULD

### Data is protected according to its sensitivity (`pol-DATA`)

#### `eng:pol-DATA.CLASS`

clause: MUST classify data by sensitivity and handle each class according to that classification
level: MUST

#### `eng:pol-DATA.MASK`

clause: MUST mask, pseudonymise or anonymise personal data wherever the full value is not needed to do the work
level: MUST

#### `eng:pol-DATA.CRYPTO`

clause: MUST protect sensitive data in transit and at rest using current, well-regarded cryptographic algorithms
level: MUST

#### `eng:pol-DATA.LAWFUL`

clause: MUST handle personal data on a lawful basis
level: MUST

#### `eng:pol-DATA.MINIMAL`

clause: MUST limit personal data to what is needed, wherever it is collected, copied or derived
level: MUST

#### `eng:pol-DATA.RIGHTS`

clause: MUST be able to find, export, correct, delete and restrict one person's data in every store that contains it
level: MUST

#### `eng:pol-DATA.DELETE`

clause: MUST delete data when its defined retention period ends
level: MUST

#### `eng:pol-DATA.BULK`

clause: MUST control bulk copying or export of sensitive data, even through a channel authorised for other use
level: MUST

#### `eng:pol-DATA.SHARE`

clause: MUST NOT send personal data to a third party before a written processing agreement covers it
level: MUST NOT

#### `eng:pol-DATA.LINGER`

clause: MUST NOT retain sensitive or personal data beyond its defined lifetime without a recorded deviation (pol-DEVI)
level: MUST NOT

#### `eng:pol-DATA.REVIVE`

clause: MUST NOT let personal data erased on request return through a restore
level: MUST NOT

#### `eng:pol-DATA.LOGS`

clause: MUST NOT write unmasked sensitive personal data into logs or telemetry (pol-SCRT.LOGS states the same for secrets)
level: MUST NOT

### Derived data is verified before it is trusted (`pol-DERV`)

#### `eng:pol-DERV.EXPECT`

clause: MUST state what a correct output looks like, in terms that can be checked automatically
level: MUST

#### `eng:pol-DERV.CHECK`

clause: MUST check a production output against those expectations before anything downstream uses it
level: MUST

#### `eng:pol-DERV.RUNLOG`

clause: MUST keep, for each production run, a record of its inputs, its output and its check result
level: MUST

#### `eng:pol-DERV.FAILED`

clause: MUST NOT use a derived output whose checks did not pass
level: MUST NOT

### Deviations are recorded, owned and time-bound (`pol-DEVI`)

#### Exceptions

None. Departing from this policy means not recording a deviation, which is the one thing it exists to stop.

Some commitments admit no deviation at all, and no record makes any of them acceptable:

* [pol-SCRT].EMBED, [pol-SCRT].REUSE and [pol-SCRT].LOGS. The operational obligations beside them are deviable.
* [pol-DATA].LAWFUL. Its other obligations are deviable.
* [pol-KNOW].DOCS, at an effort proportionate to the system. The commitment does not vary with the effort.
* [pol-AGNT].ACCEPT. Being in a hurry does not make agent output authoritative.
* [pol-INCR].EVIDENC, whether or not a breach is notifiable.
* This policy, for the reason given above.

#### `eng:pol-DEVI.RECORD`

clause: MUST record a deviation before departing from a MUST or MUST NOT in a policy or a standard, or immediately afterwards where an incident left no time
level: MUST

#### `eng:pol-DEVI.CONTENT`

clause: MUST state what the deviation is, why it is needed and what compensates for it
level: MUST

### Environments are separated, and production stays in production (`pol-ENVS`)

#### `eng:pol-ENVS.SPLIT`

clause: MUST separate production from non-production, with distinct access controls at each tier
level: MUST

#### `eng:pol-ENVS.CREDS`

clause: MUST keep production credentials and secrets unreachable from any lower environment (pol-SCRT owns the exceptions)
level: MUST

### Incidents are managed and learned from (`pol-INCR`)

#### `eng:pol-INCR.RECOVER`

clause: MUST invoke the recovery path defined for the affected system rather than improvising one
level: MUST

#### `eng:pol-INCR.HOLD`

clause: MUST keep security controls in force during an incident, or record what compensates for one that is set aside
level: MUST

### Interfaces are contracts we honour (`pol-INTC`)

#### `eng:pol-INTC.DEPREC`

clause: MUST publish a deprecation approach, saying how much notice a consumer gets
level: MUST

#### `eng:pol-INTC.COMPAT`

clause: SHOULD compare a change to a published contract against the version consumers hold, so a break is found before they find it
level: SHOULD

### Knowledge is written down and kept with what it describes (`pol-KNOW`)

#### `eng:pol-KNOW.SYNC`

clause: MUST change documentation alongside the code, configuration or process it describes, in the same review
level: MUST

#### `eng:pol-KNOW.DECIDE`

clause: MUST record architecturally significant decisions, with the reasoning and the alternatives weighed, so a future reader can tell a considered choice from an accident
level: MUST

#### `eng:pol-KNOW.HEADS`

clause: MUST NOT allow knowledge that is critical to operating or recovering a system to exist only in someone's head or in an ephemeral conversation
level: MUST NOT

#### `eng:pol-KNOW.BEHAVE`

clause: SHOULD state a new feature's expected behaviour as scenarios the tests can run, before it is built
level: SHOULD

#### `eng:pol-KNOW.RUNBOOK`

clause: SHOULD write the steps for each operational scenario we can foresee, before the day we need them
level: SHOULD

### Exposure is minimised and traffic is controlled (`pol-MEXP`)

#### `eng:pol-MEXP.DENY`

clause: MUST deny by default, and expose a service only through an intended, controlled route
level: MUST

#### `eng:pol-MEXP.TRANSIT`

clause: MUST protect traffic crossing a trust boundary
level: MUST

#### `eng:pol-MEXP.PEERID`

clause: MUST verify the identity presented at the far end of a connection that crosses a trust boundary, and refuse it where that fails
level: MUST

#### `eng:pol-MEXP.EGRESS`

clause: MUST control what leaves our systems, not only what arrives
level: MUST

#### `eng:pol-MEXP.LATERAL`

clause: MUST NOT leave traffic between our own systems unrestricted where they have no need to talk to each other
level: MUST NOT

#### `eng:pol-MEXP.WEAKEN`

clause: MUST NOT disable, bypass or weaken identity verification at a trust boundary
level: MUST NOT

#### `eng:pol-MEXP.PRIVATE`

clause: SHOULD use private paths rather than public ones for access between our own services
level: SHOULD

### Software stays easy to change (`pol-MNTN`)

#### `eng:pol-MNTN.DEAD`

clause: MUST remove code, configuration and definitions that nothing uses
level: MUST

#### `eng:pol-MNTN.HARDER`

clause: MUST NOT knowingly make a component harder to change without recording why (pol-DEVI)
level: MUST NOT

### Systems are observable and actively monitored (`pol-OBSV`)

#### `eng:pol-OBSV.USAGE`

clause: MUST monitor resource use against the capacity a system has, and act before it runs out
level: MUST

#### `eng:pol-OBSV.SECMON`

clause: MUST monitor for security-relevant events, not only for availability
level: MUST

#### `eng:pol-OBSV.TAMPER`

clause: MUST NOT let an administrator erase or deactivate logs of their own activity
level: MUST NOT

### Performance targets are stated and verified (`pol-PERF`)

#### `eng:pol-PERF.MEASURE`

clause: MUST validate against those targets before significant releases, under conditions representative of real load rather than convenient load
level: MUST

#### `eng:pol-PERF.PEAK`

clause: MUST understand behaviour at peak and under sustained load, not only at the average case
level: MUST

#### `eng:pol-PERF.NOTEST`

clause: MUST NOT release a significant change to a performance-sensitive system with no performance validation at all
level: MUST NOT

### Changes reach production through the pipeline (`pol-PIPE`)

#### `eng:pol-PIPE.FLAGS`

clause: MUST treat a flag that changes production behaviour as a controlled, auditable change
level: MUST

### Services and data are recoverable (`pol-RECV`)

#### `eng:pol-RECV.RTORPO`

clause: MUST define recovery-time and recovery-point objectives for critical systems, and design to meet them
level: MUST

#### `eng:pol-RECV.BACKUP`

clause: MUST back up critical data on a schedule that matches those objectives
level: MUST

#### `eng:pol-RECV.TIMEOUT`

clause: MUST bound every outbound call in time, so a slow dependency cannot become an unbounded wait
level: MUST

#### `eng:pol-RECV.DEGRADE`

clause: MUST design so that the failure of one dependency degrades function rather than taking the system down with it
level: MUST

#### `eng:pol-RECV.IDEMPOT`

clause: MUST make operations that may be retried safe to re-run
level: MUST

#### `eng:pol-RECV.UNTEST`

clause: MUST NOT rely on a backup that has never been test-restored
level: MUST NOT

#### `eng:pol-RECV.RETRY`

clause: MUST NOT retry indefinitely, without limit or backoff, against a failing dependency
level: MUST NOT

#### `eng:pol-RECV.SHED`

clause: SHOULD shed or slow work deliberately when load exceeds capacity, turning some requests away rather than failing unpredictably
level: SHOULD

### Secrets are managed, never embedded (`pol-SCRT`)

#### `eng:pol-SCRT.ENCRYPT`

clause: MUST encrypt the secret store, and any transfer of a secret out of it
level: MUST

#### `eng:pol-SCRT.KEYS`

clause: MUST protect the keys and certificates that protect our data through their full lifecycle: issue, storage, rotation, revocation
level: MUST

#### `eng:pol-SCRT.REUSE`

clause: MUST NOT use a production secret anywhere outside production (pol-ENVS.REUSE narrows this to environments below production)
level: MUST NOT

#### `eng:pol-SCRT.APART`

clause: MUST NOT transfer an encryption key via the same channel as the data it protects
level: MUST NOT

### Security is designed in, not added on (`pol-SECD`)

#### `eng:pol-SECD.REQS`

clause: MUST capture security and privacy requirements for new systems and features, as requirements rather than as afterthoughts
level: MUST

#### `eng:pol-SECD.DESIGN`

clause: MUST apply established secure-design principles: least privilege, defence in depth, secure defaults, and failing closed so that a failure denies rather than allows
level: MUST

#### `eng:pol-SECD.THREAT`

clause: MUST think through how a significant new system or high-risk change could be attacked, and record what we found
level: MUST

#### `eng:pol-SECD.HIRISK`

clause: MUST NOT take a high-risk change into build with no security requirements and no consideration of threat
level: MUST NOT

### Third-party services are chosen deliberately, and watched (`pol-SUPL`)

#### `eng:pol-SUPL.EXIT`

clause: MUST know how we would leave a service before we depend on it
level: MUST

### We ship only components we know and trust (`pol-TRUS`)

#### `eng:pol-TRUS.SUPPORT`

clause: MUST keep a component on a version the vendor still supports
level: MUST

#### `eng:pol-TRUS.PATCH`

clause: MUST update dependencies and runtimes on a routine cadence, independent of a specific vulnerability finding
level: MUST

#### `eng:pol-TRUS.MALWARE`

clause: MUST scan the artefacts we build for malicious content before we release them
level: MUST

### Vulnerabilities are found, prioritised and closed to a timeframe (`pol-VURM`)

#### `eng:pol-VURM.DISCLOS`

clause: MUST provide a route for people outside the organisation to report a vulnerability to us, and respond when they do
level: MUST

#### `eng:pol-VURM.REGRESS`

clause: MUST keep a regression test for a vulnerability we have fixed, so it cannot return unnoticed (pol-AUTV.REGRESS states the general duty)
level: MUST

