# Rules the standards of example-payments state

example-payments 0.45.0 states 66 rules for code in this repository, under the 21 standards below. Each rule binds the code its title bears on. The standards-lookup skill returns a rule's obligations by its standard's id.

Each heading gives a standard's id, title and status. Each line under it gives a rule's title.

## std-CARD: Card details reach the PSP and never reach us (active)

- Nothing we own stores a card
- The card goes straight to the PSP
- The split of responsibility is written down

## std-IDEM: One order pays once, however many times the request arrives (active)

- An in-flight repeat waits or is told to wait
- The caller chooses the key
- The service answers a repeat from its record

## std-LEDGER: A ledger entry is written once and corrected by another entry (active)

- An entry is a balanced pair
- An entry says what produced it
- Nothing amends an entry

## std-PSPOUT: A call to the PSP is bounded, and an unknown outcome is resolved (active)

- A retry is bounded and backs off
- An unknown outcome is resolved
- Every call has a deadline

## std-RECON: The ledger is reconciled against the PSP every day (active)

- A break is named and owned
- Nothing downstream reads an unreconciled day
- The run happens every day

## std-RETAIN: A payment record is kept for seven years and nothing else is (active)

- An erasure request is answered
- Deletion happens without anyone asking
- Each store has one period

## std-SCA: The PSP decides whether the cardholder is challenged (active)

- The challenge is somebody else's page
- The outcome travels with the payment
- The PSP runs the authentication

## std-TELEM: A payment leaves an order reference in the logs and nothing else (active)

- A PSP key is a secret and behaves like one
- The redaction runs before the sink
- What a log line may carry

## eng:std-API: An interface is described by a contract in the repository (draft)

- A change carries a version and a notice
- Every endpoint authenticates and validates
- The contract is the source of truth

## eng:std-CONT: A container image is pinned by digest and runs as a non-root user (draft)

- A published tag never moves
- The base image is chosen and pinned
- The container holds only what the service needs

## eng:std-CSSTY: C# follows the runtime team's conventions, and the compiler enforces them (draft)

- A suppression is local and says why
- The conventions come from one file
- The security rules the analysers cover

## eng:std-DEPLOY: One artefact is built once and promoted to production (draft)

- A way back exists before the change goes
- Change approval
- Configuration sits outside the artefact
- Production changes through the pipeline alone
- The artefact is built once

## eng:std-DEPS: A dependency is pinned, screened and named in the inventory (draft)

- What happens afterwards
- What happens before adoption
- Where a package comes from

## eng:std-GATES: A failing check blocks the merge (draft)

- A failing check stops the change
- Every change is built and tested automatically

## eng:std-INCSEV: Every incident is handled at a stated severity (draft)

- A significant incident is reviewed
- Every incident has a severity
- The severity moves with what is known
- The severity sets the response

## eng:std-NETTST: A .NET test project runs under one framework, and an integration test owns its dependencies (draft)

- A unit test crosses no boundary
- An integration test starts what it needs
- Coverage is collected by the build
- The framework and the project layout

## eng:std-OBS: A service ships its telemetry to one place, keyed by one id (draft)

- Everything lands in the central store
- One request reads as one timeline
- Somebody acts on every alert
- Telemetry carries no personal data

## eng:std-PR: A pull request carries its reasoning and one accountable approval (draft)

- A change arrives as a pull request
- Agent-produced work says what produced it
- Somebody other than the author approves it

## eng:std-SECRET: A secret is read from the store at run time (draft)

- A secret comes from the store
- Every secret rotates
- Nowhere else holds a secret

## eng:std-TEST: A test runs at the lowest level that can catch the fault (draft)

- Know what the suite reaches
- Pick the level from the fault
- The data is never a real customer's

## eng:std-VCS: Everything a service needs is committed to its repository (draft)

- Every change is attributable and reviewed
- The repository holds everything the service needs
