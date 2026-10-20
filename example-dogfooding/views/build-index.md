# Rules the standards of example-dogfooding state

example-dogfooding 0.71.2 states 82 rules for code in this repository, under the 19 standards below. Each rule binds the code its title bears on. The standards-lookup skill returns a rule's obligations by its standard's id.

Each heading gives a standard's id, title and status. Each line under it gives a rule's title.

## std-A11Y: The documentation site and kac are usable without sight, a mouse or colour (active)

- A change that makes either surface worse needs a deviation
- A Markdown author owns the structure of a page
- A pinned component carries an accessibility assessment
- kac carries no meaning in colour alone
- The theme owns contrast, focus and keyboard reach
- What falls short is written down where a reader finds it

## std-CI: Every check and every publish runs from a workflow in this repository (active)

- A job declares the permission it needs
- A version moves by hand and publishes once
- A workflow holds no credential of its own
- Every action is pinned to a commit
- One job holds the write permission
- The gate runs on every pull request into main

## std-CONFIG: Configuration is committed, pinned and explained (active)

- A file says what it is for, and why each value is what it is
- A pin names an exact version
- A value living in more than one tree is copied and proved
- Every value the build reads is committed
- The shape of a YAML file is yamllint's to decide

## std-PLUGIN: The plugin carries the corpus, and a skill answers from what travelled (active)

- A deviation request an agent files asks for the owner it cannot name
- A finding an agent files is an observation, and proposes no record
- A framework finding travels stripped, and only by hand
- A hook runs in a POSIX shell, and pushes a view once a session
- A skill assumes only that the session can read a file
- A skill names the type of every field it describes
- A skill reads a rule's keyword as what departing from it costs
- A skill says what its corpus did not carry
- A skill states what it is for, and the reason for each rule it gives
- A skill writes back to the tracker the corpus states
- plugin.json declares every component and the types it reads
- The bundle carries a frozen export, and nothing writes to it
- Triage routes a finding with a label, and says why in a comment

## std-PROSE: Prose reads the same way whoever wrote it (active)

- Commit messages and pull requests
- The base style
- What prose must not change
- What stays true
- Where this repository differs from Microsoft

## std-VERS: A version stamp says what it versions, and a move says what changed (active)

- A producer's move obliges every consumer in the same pull request
- A published version is never replaced
- A reader meeting a stamp it does not know stops
- A stamp is a semantic version, a count, or a range
- What a move of each stamp means
- Which stamps move together, and which move alone

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
