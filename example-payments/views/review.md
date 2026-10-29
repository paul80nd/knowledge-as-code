# The rules a change is reviewed against, in the standards of example-payments

Each rule below gives the id a finding cites, then its obligations, one to a bullet.

- **MUST** and **MUST NOT** state a requirement. A change that breaks one is a violation.
- **SHOULD** and **SHOULD NOT** state the usual way. A change that departs from one and gives its reason conforms, and the departure is a judgement call.
- **MAY** permits, and a change cannot break it.
- A standard is in force at status `active`. A finding under any other status is not in force.
- Each rule's controls are listed under it, with each one's mechanism and status. A control checks a rule only where its status is `active` and its mechanism is `ci` or `runtime-alert`, and then only the obligations named in its record's What it checks section. `corpus/controls/` inside this plugin has each control's record. A person checks every other obligation in the review.

## std-CARD: Card details reach the PSP and never reach us

Status: active.

### Nothing we own stores a card

Cite as `std-CARD.NOSTORE`.

Controls: none.

- A datastore **MUST NOT** keep a card number, an expiry date or a security code, in any column, document or blob.
- A support tool **MUST NOT** offer a field that accepts a card number, even where a customer reads one out over the telephone.
- A token **MAY** be stored and reused, because only we and the PSP can use it.

### The card goes straight to the PSP

Cite as `std-CARD.DIRECT`.

Controls: none.

- A checkout page **MUST** collect card details in a frame the PSP serves.
- A checkout page **MUST NOT** post a card field to a host we operate.
- A service **MUST** accept a card token where it needs to charge a card.
- A service **MUST NOT** accept a card number on any endpoint.
- A service **MUST** verify the PSP's certificate before it sends anything.
- A service **SHOULD** verify that certificate against the PSP's own published chain.

### The split of responsibility is written down

Cite as `std-CARD.SPLIT`.

Controls: none.

- The PSP contract **MUST** state which PCI DSS requirements the PSP answers for and which we do.
- A change to how the checkout collects a card **MUST** be reviewed against that split before it ships.

## std-IDEM: One order pays once, however many times the request arrives

Status: active.

### An in-flight repeat waits or is told to wait

Cite as `std-IDEM.WAIT`.

Controls: none.

- A service **MUST** return `409` where the key is recorded and the first request has not finished.
- A service **MUST NOT** start a second call to the PSP while the first is in flight for the same key.

### The caller chooses the key

Cite as `std-IDEM.KEY`.

Controls: none.

- A request that authorises, captures or refunds **MUST** include an `Idempotency-Key` header.
- The caller **MUST** send the same key on every retry of one attempt.
- The caller **SHOULD** derive the key from the order and the operation.
- A service **MUST** reject a request with no key, rather than treating it as a new payment.
- The contract **MUST** state the key's format and how long it is honoured.

### The service answers a repeat from its record

Cite as `std-IDEM.REPEAT`.

Controls: none.

- A service **MUST** store the key with the outcome before it responds to the first request.
- A service **MUST** return the stored outcome for a repeated key, with the same status and body.
- A service **MUST** honour a key for the period the contract states.
- That period **SHOULD** be at least 24 hours.
- A service **MUST** return `422` where a repeated key arrives with a different amount or a different order, rather than charging either one.
- A service **MUST** pass the key to the PSP as the PSP's own idempotency key, so a retry stops at whichever service has already responded.

## std-LEDGER: A ledger entry is written once and corrected by another entry

Status: active.

### An entry is a balanced pair

Cite as `std-LEDGER.BALANCE`.

Controls: none.

- The ledger **MUST** write a movement of money as a debit and a credit of the same amount.
- An entry **MUST** state the amount in minor units as an integer, with its ISO 4217 currency code.
- An entry **MUST NOT** use a floating-point type for an amount, at rest or in transit.
- The ledger **MUST** write a pair of entries in one transaction, so no reader ever sees one half.

### An entry says what produced it

Cite as `std-LEDGER.SOURCE`.

Controls: none.

- An entry **MUST** record the order, the PSP reference and the idempotency key from [std-IDEM.KEY](https://github.com/paul80nd/knowledge-as-code/blob/5cb4b7171b05fdf0c09136026ef499dcd3d36940/examples/payments/standards/authorisation/idempotency.md#key-the-caller-chooses-the-key).
- An entry **MUST** record the event that produced it: an authorisation, a capture, a refund or a chargeback.
- An entry for an authorisation **MUST** record the authentication outcome from [std-SCA.OUTCOME](https://github.com/paul80nd/knowledge-as-code/blob/5cb4b7171b05fdf0c09136026ef499dcd3d36940/examples/payments/standards/checkout/customer-authentication.md#outcome-the-outcome-travels-with-the-payment), and who bears the liability for a chargeback.
- The ledger **MUST** be able to reproduce a payment's full history from its entries alone.

### Nothing amends an entry

Cite as `std-LEDGER.NOAMEND`.

Controls: none.

- The ledger **MUST** refuse an update or a delete on a written entry.
- The ledger **MUST** record a correction as a reversing pair followed by the intended pair, each stating the entry it corrects.
- An entry **MUST** record the time the event happened and the time it was written, where the two differ.

## std-PSPOUT: A call to the PSP is bounded, and an unknown outcome is resolved

Status: active.

### A retry is bounded and backs off

Cite as `std-PSPOUT.RETRY`.

Controls: none.

- A service **SHOULD** retry a connection failure or a `5xx`.
- A service **MUST NOT** retry a decline, because a decline is the issuer's answer and the card schemes fine a merchant that keeps retrying one.
- A service **MUST** stop retrying at a limit set in its configuration.
- A service **MUST** back off between attempts.
- That limit **SHOULD** be two retries.
- The backoff **SHOULD** be exponential, with jitter.
- A service **MUST** stop calling the PSP for a period set in its configuration once the failure rate passes its configured threshold.
- That period **SHOULD** be 30 seconds.

### An unknown outcome is resolved

Cite as `std-PSPOUT.UNKNOWN`.

Controls: none.

- A service **MUST** record a timed-out authorisation as unknown.
- A service **MUST** query the PSP for the outcome of an unknown authorisation, quoting the idempotency key from [std-IDEM.KEY](https://github.com/paul80nd/knowledge-as-code/blob/5cb4b7171b05fdf0c09136026ef499dcd3d36940/examples/payments/standards/authorisation/idempotency.md#key-the-caller-chooses-the-key).
- A service **MUST** resolve every unknown outcome within a window set in its configuration.
- A service **MUST** raise an alert on an unknown outcome it has not resolved within that window.
- That window **SHOULD** be 15 minutes.
- The checkout **MUST** tell the customer the payment is being confirmed.

### Every call has a deadline

Cite as `std-PSPOUT.TIMEOUT`.

Controls: none.

- A call to the PSP **MUST** have a timeout set in its configuration, so it can change without a release.
- The authorisation call **SHOULD** time out at 5 seconds.

## std-RECON: The ledger is reconciled against the PSP every day

Status: active.

### A break is named and owned

Cite as `std-RECON.BREAK`.

Controls: none.

- The run **MUST** classify each break: in the file and not the ledger, in the ledger and not the file, or matched with a different amount.
- The run **MUST** raise one alert stating the count and the total value of the breaks.
- The run **MUST NOT** raise an alert for each break.
- The run **MUST** escalate a break over 30 days old to the finance owner set in its configuration.

### Nothing downstream reads an unreconciled day

Cite as `std-RECON.HOLD`.

Controls: none.

- A revenue report **MUST NOT** include a day whose reconciliation has not passed.
- A team **MUST** close a break with a ledger correction under [std-LEDGER.NOAMEND](https://github.com/paul80nd/knowledge-as-code/blob/5cb4b7171b05fdf0c09136026ef499dcd3d36940/examples/payments/standards/ledger/entries.md#noamend-nothing-amends-an-entry).
- A team **MUST NOT** close a break by editing the run's output.

### The run happens every day

Cite as `std-RECON.DAILY`.

Controls: none.

- A reconciliation **MUST** run once a day against the PSP's settlement file for the previous day.
- The run **MUST** match a settlement row to a ledger entry on the PSP reference.
- The run **MUST** compare the amount and the currency of a matched pair.
- The run **MUST** record its inputs, its output and its result, and keep that record for seven years.
- The run **MUST** raise an alert where the settlement file is missing.
- The run **MUST NOT** record a clean run for a day whose settlement file did not arrive.

## std-RETAIN: A payment record is kept for seven years and nothing else is

Status: active.

### An erasure request is answered

Cite as `std-RETAIN.ERASE`.

Controls: none.

- A service **MUST** remove the customer's contact details on an erasure request.
- A service **MUST** replace the customer's name in the ledger with the order reference.
- A service **MUST NOT** delete a ledger entry on an erasure request.
- A service **MUST** answer an erasure request within one month of receiving it.

### Deletion happens without anyone asking

Cite as `std-RETAIN.PURGE`.

Controls: none.

- A service **MUST** delete data that has passed its period, without anyone asking.
- A scheduled job **SHOULD** delete it, at least weekly, unless the store expires data itself.
- A service **MUST** record each deletion, by store and by count.
- A backup policy **MUST** expire a backup on the same period as the store it came from.

### Each store has one period

Cite as `std-RETAIN.PERIOD`.

Controls: none.

- The ledger **MUST** keep an entry for seven years from the date it was written.
- A service **MUST** delete a card token 13 months after its last use.
- The telemetry platform **MUST** delete a payment record after 90 days.
- A store **MUST NOT** keep personal data with no stated period against it.

## std-SCA: The PSP decides whether the cardholder is challenged

Status: active.

### The challenge is somebody else's page

Cite as `std-SCA.PAGE`.

Controls: none.

- The checkout **MUST** hand the cardholder to the PSP's challenge page or frame.
- The checkout **MUST NOT** collect a one-time passcode, a password or a biometric prompt on a page we serve.
- The checkout **MUST** hold the order for the length of the challenge, and release it on a stated timeout rather than waiting forever.

### The outcome travels with the payment

Cite as `std-SCA.OUTCOME`.

Controls: none.

- An authorisation request **MUST** include the PSP's authentication reference.
- A service **MUST** pass the authentication outcome to the ledger, which [std-LEDGER.SOURCE](https://github.com/paul80nd/knowledge-as-code/blob/5cb4b7171b05fdf0c09136026ef499dcd3d36940/examples/payments/standards/ledger/entries.md#source-an-entry-says-what-produced-it) requires to record it.
- A service **MUST NOT** retry a declined authorisation with the authentication step left out.

### The PSP runs the authentication

Cite as `std-SCA.PSP`.

Controls: none.

- The checkout **MUST** start authentication through the PSP's 3-D Secure flow before it asks for an authorisation.
- A service **MUST** treat an authorisation with no authentication outcome as declined.
- A service **MUST NOT** decide by itself that a payment is exempt from Strong Customer Authentication, which is the cardholder proving who they are with two independent factors.
- A service **MUST** record the exemption the PSP applied, where the PSP applied one.

## std-TELEM: A payment leaves an order reference in the logs and nothing else

Status: active.

### A PSP key is a secret and behaves like one

Cite as `std-TELEM.KEY`.

Controls: none.

- A service **MUST** read the PSP's secret key from the managed store at start-up, through the identity granted to that workload.
- A service **MUST NOT** write a PSP key to a log, a console, an error report or a support ticket.

### The redaction runs before the sink

Cite as `std-TELEM.REDACT`.

Controls: none.

- A service **SHOULD** redact against a list of field names kept in code and covered by a test, rather than against a pattern applied where the log is read.
- A service **MUST** redact a request or response body before it is written.
- A service **MUST NOT** leave redaction to the log platform, which sees the line only after every copy of it exists.
- An unhandled exception **MUST** be reported without the request body that caused it.

### What a log line may carry

Cite as `std-TELEM.FIELDS`.

Controls: none.

- A log line **MUST** identify a payment by its order reference, its PSP reference, or both.
- A log line **MUST NOT** include a card token, in a message, a property or a structured field.
- A log line **MUST NOT** include a customer's name, address, email address or telephone number.

## eng:std-API: An interface is described by a contract in the repository

Status: draft.

### A change carries a version and a notice

Cite as `eng:std-API.VERSION`.

Controls: `eng:ctl-0005` Nothing checks that an interface change carries a version and a notice (`not-enforced`, active).

- A published interface **MUST** have a version a consumer can pin to.
- An HTTP interface **SHOULD** put its major version in the path, as `/v2/`, unless the contract states another scheme.
- A breaking change **MUST** ship as a new version, with the previous version still serving requests.
- A team **MUST** give notice before it removes a version, on the timescale the deprecation approach states.
- A deprecated version **SHOULD** respond with a `Deprecation` header and a `Sunset` header, so a consumer sees the notice in the response.

### Every endpoint authenticates and validates

Cite as `eng:std-API.AUTH`.

Controls: none.

- A contract **MUST** state the authentication each endpoint takes.
- A contract **MUST** declare an endpoint that takes no authentication as public.
- An endpoint **MUST** authenticate the caller before it does any work, unless the contract declares it public.
- An endpoint **MUST** authorise the operation before it does any work, unless the contract declares it public.
- An endpoint **MUST** validate each field it receives against the contract, and reject a request that does not match.
- An interface **MUST NOT** serve personal data, a secret, or an operation that changes state, to an unauthenticated caller.

### The contract is the source of truth

Cite as `eng:std-API.SPEC`.

Controls: none.

- A service **MUST** publish a machine-readable contract for every interface it exposes, committed to the repository that builds the service.
- Where the implementation and the contract disagree, a team **MUST** treat the implementation as the defect.
- A build **MUST** fail where the implementation no longer matches the contract.
- A consumer **SHOULD** test its use of an interface against the published contract, so the test fails on what the provider promised.
- A consumer **MAY** also test against a running instance of the provider.

## eng:std-CONT: A container image is pinned by digest and runs as a non-root user

Status: draft.

### A published tag never moves

Cite as `eng:std-CONT.TAG`.

Controls: `eng:ctl-0001` Nothing checks that an image is named by digest (`not-enforced`, active).

- A pipeline **MUST** tag an image with the build's own version.
- A registry **MUST** refuse a second push to a tag that already exists.
- A deployment **MUST** name an image by digest, so the running container is the one the pipeline built.

### The base image is chosen and pinned

Cite as `eng:std-CONT.BASE`.

Controls: `eng:ctl-0001` Nothing checks that an image is named by digest (`not-enforced`, active).

- A Dockerfile **MUST** take its base image from the organisation's registry.
- A Dockerfile **MUST** pin the base image by digest rather than by a moving tag.
- A repository **MUST** rebuild against a refreshed base at least monthly, so a patched base is deployed to the running service.

### The container holds only what the service needs

Cite as `eng:std-CONT.MINIMAL`.

Controls: none.

- An image **MUST** declare a non-root `USER`.
- A container **MUST** run as that user.
- A container **MUST** run with a read-only root filesystem, writing only to a declared volume.
- An image **MUST** be built from a runtime base rather than an SDK base, so no compiler ships to production.
- An image **MUST NOT** expose a management, debug or metrics port to anything outside the cluster.

## eng:std-CSSTY: C# follows the runtime team's conventions, and the compiler enforces them

Status: draft.

### A suppression is local and says why

Cite as `eng:std-CSSTY.SUPPR`.

Controls: `eng:ctl-0009` Nothing checks that a suppression is local and justified (`not-enforced`, active).

- A suppression **MUST** sit on the member it applies to, with a `Justification` giving the reason.
- A repository **MUST NOT** disable a rule in `.editorconfig` to clear a warning in one file.

### The conventions come from one file

Cite as `eng:std-CSSTY.ONEFILE`.

Controls: none.

- A repository holding C# **MUST** have an `.editorconfig` at its root.
- That `.editorconfig` **SHOULD** be the .NET runtime team's own.
- A repository **SHOULD** state each departure from the runtime team's file in a comment beside it.
- A project **MUST** set `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild`, so the build checks the style.
- A project **MUST** enable nullable reference types.
- Code **MUST NOT** silence a nullability warning with `!`, except in a test asserting that a null argument is refused.
- A project **MUST** enable the .NET analysers at the `latest` analysis level.

### The security rules the analysers cover

Cite as `eng:std-CSSTY.SECURE`.

Controls: none.

- Code **MUST** build a SQL command through a parameter rather than by joining strings.
- Code **MUST** use the framework's cryptography.
- Code **MUST NOT** call an algorithm the analysers report as broken.
- Code **MUST** compare a secret with `CryptographicOperations.FixedTimeEquals` rather than with `==`.

## eng:std-DEPLOY: One artefact is built once and promoted to production

Status: draft.

### A way back exists before the change goes

Cite as `eng:std-DEPLOY.WAYBACK`.

Controls: none.

- A release **MUST** have a stated rollback or recovery path before it starts.
- A database migration **MUST** leave the previous version of the service able to run against the new schema, unless the migration removes data the previous version reads.

### Change approval

Cite as `eng:std-DEPLOY.APPROVE`.

Controls: none.

- A change **MUST** be approved by someone other than its author before it reaches production.
- The approval **MUST** be recorded where the change is, such as in branch protection.
- A change of a higher-risk kind **MUST** carry an approver with authority for that risk. The higher-risk kinds are a schema or data migration, a change to access or security configuration, and infrastructure that holds data.
- A change **MAY** bypass approval during an incident.
- A change that bypassed approval **MUST** be reviewed by a second person within two working days of the change reaching production.
- That review **MUST** be recorded against the change.

### Configuration sits outside the artefact

Cite as `eng:std-DEPLOY.CONFIG`.

Controls: none.

- A service **MUST** read environment-specific configuration at start-up, from outside the artefact.
- An artefact **MUST NOT** contain a hostname, connection string or feature setting that differs between environments.

### Production changes through the pipeline alone

Cite as `eng:std-DEPLOY.PIPE`.

Controls: none.

- A change **MUST** reach production through the automated pipeline.
- A change **MUST** reach each environment below production by promotion from the one before it.
- Every environment **MUST** be provisioned from the same definition, parameterised per environment.
- The pipeline definition **MUST** be committed to the repository, and reviewed like the code.
- A team **MUST NOT** hand-edit production code, configuration, infrastructure or schema, other than a flag change made under [pol-PIPE](https://github.com/paul80nd/knowledge-as-code/blob/5cb4b7171b05fdf0c09136026ef499dcd3d36940/examples/engineering/policies/delivery/pipe-pipeline-to-production.md#clauses).FLAGS.
- A team changing production by hand to restore service, where the pipeline is unavailable or its delay would extend an outage, **MUST** record a deviation at the time.
- A team **MUST** commit a hand change made to restore service to the repository before the incident closes.

### The artefact is built once

Cite as `eng:std-DEPLOY.ONCE`.

Controls: `eng:ctl-0002` Nothing checks that a running service reports its commit (`not-enforced`, active).

- A pipeline **MUST** build the artefact once, and promote that build to every environment.
- An artefact **MUST** record the commit it was built from, so a running version identifies the change it came from.
- A deployment record **MUST** name the artefact, the change and the approval behind it.
- A pipeline **MUST NOT** deploy an artefact built anywhere but the pipeline.

## eng:std-DEPS: A dependency is pinned, screened and named in the inventory

Status: draft.

### What happens afterwards

Cite as `eng:std-DEPS.UPKEEP`.

Controls: none.

- A pipeline **MUST** scan the dependency tree on every build and on a weekly schedule.
- A team **MUST** rank a finding by its severity and by whether the affected path runs.
- A team **MUST** close a critical finding within 7 days and a high finding within 14.
- A team **MUST NOT** release with an open critical finding, absent a recorded deviation naming who accepts the risk.

### What happens before adoption

Cite as `eng:std-DEPS.ADOPT`.

Controls: none.

- Somebody **MUST** screen a new package for known vulnerabilities before the pull request that adds it merges.
- Somebody **MUST** check a new package's licence against the licences the [conformance checklist](#conformance-checklist) allows before that pull request merges.
- A package a build ships under a review-tier licence **MUST** carry an accepted deviation.
- A build **MUST NOT** ship a package under a refused licence.
- A build **MUST NOT** ship a package with no licence.
- A pull request adding a package **MUST** say what the package is for, so a reviewer can weigh it against writing the code.

### Where a package comes from

Cite as `eng:std-DEPS.SOURCE`.

Controls: none.

- A build **MUST** resolve packages from the organisation's feed, which proxies the public registries.
- A repository **MUST** commit a lockfile pinning every direct and transitive dependency to a version.
- A build **MUST** install from the lockfile, and fail where the lockfile and the manifest disagree.
- A build **MUST** publish its artefacts to the managed repository, versioned and retained.
- A team **MUST NOT** add a package from a personal feed, a git URL or an archive downloaded by hand.

## eng:std-GATES: A failing check blocks the merge

Status: draft.

### A failing check stops the change

Cite as `eng:std-GATES.BLOCK`.

Controls: `eng:ctl-0004` Nothing checks that a skipped test cites a deviation (`not-enforced`, active).

- Branch policy **MUST** block a merge while any required check is failing.
- A team **MUST NOT** merge over a failing check without a recorded deviation.
- A team **MUST NOT** skip, silence or suppress a check without a recorded deviation.
- A deviation covering a suppressed check **MUST** name the individual who accepts the risk.
- That deviation **MUST** have a review date.
- A team **MAY** guard a test off a platform it cannot run on without a deviation, where a required check runs the test on a platform where it can, and the guard states why.

### Every change is built and tested automatically

Cite as `eng:std-GATES.BUILD`.

Controls: none.

- A push to any branch **MUST** trigger a build and the test suite, without anyone asking for it.
- The build **MUST** run from a clean checkout, on an agent that is not a person's own machine.
- The agent **SHOULD** be provisioned from a definition in the repository, because a change to it is then reviewed like the code.
- A fix for a defect **MUST** arrive with a test that fails without it.

## eng:std-INCSEV: Every incident is handled at a stated severity

Status: draft.

### A significant incident is reviewed

Cite as `eng:std-INCSEV.REVIEW`.

Controls: none.

- A sev1 or sev2 incident **MUST** be treated as significant wherever a policy here names one.
- A sev1 or sev2 incident **MUST** have a postmortem.
- A personal data breach **MUST** have a postmortem, whatever its severity.

### Every incident has a severity

Cite as `eng:std-INCSEV.ASSIGN`.

Controls: none.

- Whoever declares an incident **MUST** set its severity at declaration.
- An incident **MUST** be sev1 where a `critical` service is unavailable, or where customers' money has been taken, refunded or paid out wrongly, or their personal data is wrong or exposed, at any scale.
- An incident **MUST** be sev2 where a service is degraded or a fault reaches some customers, and a workaround exists or the harm is contained.
- An incident **MUST** be sev3 where no customer is affected, or one customer is affected with no lasting harm.
- An incident meeting none of these **MUST** be sev2 until the incident lead sets another level.

### The severity moves with what is known

Cite as `eng:std-INCSEV.REVISE`.

Controls: none.

- The incident lead **MUST** raise the severity as soon as the incident meets a higher level.
- The incident lead **SHOULD** lower the severity once the incident no longer meets its level, because a sev1 response held open costs the people paged for it.
- A lowered severity **MUST** have its reason recorded in the incident record.
- A postmortem **MUST** state the highest severity the incident reached.

### The severity sets the response

Cite as `eng:std-INCSEV.RESPOND`.

Controls: none.

- A sev1 or sev2 **MUST** have a named incident lead.
- A sev1 **MUST** page the on-call engineer as soon as it is declared.
- A sev1 **MUST** tell the people it affects its status at least every 30 minutes.
- A sev2 **MUST** page the on-call engineer.
- A sev2 **MUST** tell the people it affects its status at least every 2 hours.
- A sev3 **MAY** be handled as routine work, in working hours.

## eng:std-NETTST: A .NET test project runs under one framework, and an integration test owns its dependencies

Status: draft.

### A unit test crosses no boundary

Cite as `eng:std-NETTST.UNIT`.

Controls: none.

- A unit test **MUST NOT** open a socket, read the filesystem, or start a container.
- A test **MUST NOT** call `Thread.Sleep` or `Task.Delay` to wait for something.
- A test **SHOULD** take its time from an injected clock, such as `TimeProvider`.
- A test **MUST** await every asynchronous call, so the test that caused a failure is the test that reports it.

### An integration test starts what it needs

Cite as `eng:std-NETTST.INTEG`.

Controls: none.

- An integration test **MUST** start the dependencies it needs.
- An integration test **MUST NOT** share an instance of a dependency with another run.
- An integration test **SHOULD** start those dependencies with Testcontainers.
- An integration test for an ASP.NET service **SHOULD** host it in the test process with `WebApplicationFactory`.
- An integration test **MUST** leave no state behind, so the suite passes when it runs twice.

### Coverage is collected by the build

Cite as `eng:std-NETTST.COVER`.

Controls: `eng:ctl-0008` Nothing checks that a .NET build collects coverage (`not-enforced`, active).

- A build **MUST** collect coverage from its test run and publish the report.
- A build **SHOULD** write the report in Cobertura format: with `--collect:"XPlat Code Coverage"` under VSTest, or with `--coverage --coverage-output-format cobertura` under Microsoft.Testing.Platform, with `Microsoft.Testing.Extensions.CodeCoverage` referenced.
- A test project **MUST** be excluded from its own coverage figure.

### The framework and the project layout

Cite as `eng:std-NETTST.LAYOUT`.

Controls: none.

- A repository **MUST** use one test framework across its test projects.
- Every test project **MUST** run under `dotnet test`.
- A test project **SHOULD** use xUnit, because the estate's examples and controls are written against it.
- A test project **MUST** be named for the project it tests, with a `.Tests` or `.IntegrationTests` suffix.
- A project **MUST** use one assertion style throughout its tests.
- A project **SHOULD** assert with the test framework's own `Assert`.
- A project **MAY** assert with AwesomeAssertions, or with another library whose licence is one of the allowed ids in [std-DEPS's conformance checklist](https://github.com/paul80nd/knowledge-as-code/blob/5cb4b7171b05fdf0c09136026ef499dcd3d36940/examples/engineering/standards/common/dependencies.md#conformance-checklist).

## eng:std-OBS: A service ships its telemetry to one place, keyed by one id

Status: draft.

### Everything lands in the central store

Cite as `eng:std-OBS.CENTRAL`.

Controls: none.

- A service **MUST** emit its logs, traces and metrics to the central platform.
- A service **MUST NOT** keep the only copy of a log, so that losing the host still leaves the evidence.
- A service **MUST NOT** go into production without health monitoring and at least one alert on it.

### One request reads as one timeline

Cite as `eng:std-OBS.TRACE`.

Controls: none.

- A service **MUST** stamp every record with a timestamp taken from a synchronised clock, stating its offset from UTC.
- A service **SHOULD** write that timestamp in UTC, because records from several services then sort without conversion.
- A service **MUST** accept an inbound `traceparent`.
- A service **MUST** pass that `traceparent` to everything it calls.
- A log line **MUST** include the trace id, so a search on one request returns every service that handled it.
- A service **MUST** emit structured fields, not a formatted sentence, so a search can filter on a value.

### Somebody acts on every alert

Cite as `eng:std-OBS.ALERT`.

Controls: `eng:ctl-0003` Nothing checks that an alert names an owner and a first action (`not-enforced`, active).

- A service **MUST** publish availability and latency objectives.
- A service **MUST** alert when it is on course to miss one of those objectives.
- An alert **MUST** name the accountable owner and the first action to take.
- A team **MUST** delete or fix an alert nobody acts on, and not leave it to be filtered.
- A team **MUST** retain telemetry for the period the repository records against the service.

### Telemetry carries no personal data

Cite as `eng:std-OBS.PII`.

Controls: none.

- A service **MUST** redact unmasked personal data before the record is written.
- A service **MUST** keep every secret, credential and token out of a log line, a trace attribute and a metric label. [std-SECRET.NOWHERE](https://github.com/paul80nd/knowledge-as-code/blob/5cb4b7171b05fdf0c09136026ef499dcd3d36940/examples/engineering/standards/common/secret-handling.md#nowhere-nowhere-else-holds-a-secret) states where a secret may appear.

## eng:std-PR: A pull request carries its reasoning and one accountable approval

Status: draft.

### A change arrives as a pull request

Cite as `eng:std-PR.ARRIVE`.

Controls: none.

- A change **MUST** arrive on the default branch as a merged pull request, not as a direct push.
- A pull request description **MUST** link the work item that asked for the change.

### Agent-produced work says what produced it

Cite as `eng:std-PR.AGENT`.

Controls: `eng:ctl-0007` Nothing checks that a pull request names the agent that produced it (`not-enforced`, active).

- A pull request containing agent-produced work **MUST** name the agent, the model and the prompt or task that produced it.
- Agent-produced work **MUST** have the approval of a named person, who owns the change afterwards.
- A reviewer **MUST NOT** accept an agent's own summary of a change as evidence that the change is correct.
- Branch policy **MUST** apply to agent-produced work unchanged, with no exemption and no second route in.
- The identity that opened an agent-produced pull request **MUST NOT** approve or merge it.

### Somebody other than the author approves it

Cite as `eng:std-PR.APPROVE`.

Controls: none.

- A pull request **MUST** have at least one approval from somebody other than the author before it merges.
- A reviewer **MUST** check the change against the standards that apply to it, and not read for style alone.
- A reviewer **MUST** state what they checked when they approve.

## eng:std-SECRET: A secret is read from the store at run time

Status: draft.

### A secret comes from the store

Cite as `eng:std-SECRET.STORE`.

Controls: none.

- A service **MUST** read every secret from the managed store, through an identity granted to the workload.
- An environment below production **MUST** have its own secrets, distinct from production's.

### Every secret rotates

Cite as `eng:std-SECRET.ROTATE`.

Controls: none.

- A secret **MUST** be replaceable in the store without a code change or a rebuild.
- Every secret **MUST** rotate on a period recorded against it in the store.

### Nowhere else holds a secret

Cite as `eng:std-SECRET.NOWHERE`.

Controls: `eng:ctl-0006` Nothing checks a repository for a committed secret (`not-enforced`, active).

- A repository **MUST NOT** contain a secret, in source, in a configuration file, in a pipeline definition or in a test fixture.
- A build **MUST NOT** embed a secret in an artefact or an image.
- A service **MUST NOT** write a secret to a log, to a console, to an error message or to a support ticket.
- A pipeline **MUST** run a secret scanner over the repository and its history, and fail on a finding.

## eng:std-TEST: A test runs at the lowest level that can catch the fault

Status: draft.

### Know what the suite reaches

Cite as `eng:std-TEST.REACH`.

Controls: none.

- A build **MUST** report line coverage.
- A build **MUST** fail where coverage falls below the figure the repository records.
- A team **MUST** read a coverage drop as a gap in the tests rather than as a threshold to lower.

### Pick the level from the fault

Cite as `eng:std-TEST.LEVEL`.

Controls: none.

- A team **MUST** cover a rule about one unit's behaviour with a test on that unit alone.
- A team **MUST** cover an agreement between two components with a test that exercises both.
- A team **MUST** keep the tests that run on every push under ten minutes, so the answer arrives while the work is still open.
- A test **MUST** fail for one reason.
- A test's name **MUST** say what that reason is.
- A test **MUST NOT** depend on the order the suite runs in, on the wall clock, or on a service somebody else deploys.

### The data is never a real customer's

Cite as `eng:std-TEST.DATA`.

Controls: none.

- A test **MUST** run against generated data, or against production data that a masking step has already processed.
- A team **MUST NOT** copy unmasked production data into a test fixture, a seed script or a local database.
- A team **MUST NOT** run a test, a script or a debugger against production.

## eng:std-VCS: Everything a service needs is committed to its repository

Status: draft.

### Every change is attributable and reviewed

Cite as `eng:std-VCS.ATTRIB`.

Controls: none.

- The default branch **MUST** refuse a direct push, so that every change arrives as a merge.
- A commit **MUST** name the work item that asked for the change.
- A commit **SHOULD** name it in the subject as `#<id>`, because the forge then links the commit to the item.
- A commit **MUST** be made under the author's own verified identity.
- A commit **MUST NOT** be authored by a shared or generic account.

### The repository holds everything the service needs

Cite as `eng:std-VCS.REPO`.

Controls: none.

- A repository **MUST** contain the source, the build definition, the infrastructure definition, the configuration templates and the recovery scripts for the services it owns.
- A repository **MUST NOT** depend on an asset kept only on a workstation, in a cloud portal or in an unversioned share.
- Infrastructure, schema and configuration changes **MUST** go through the same review as application code.
