# What the cut left out

Fields dropped from every record kept in part, unless the row says otherwise: `Source:`, `tags:`, `implements:`,
`applies-to:`, `category:`, `depends-on:` and every clause's `covers:` line.

## controls

The file has two sets of control ids: the first fourteen are this corpus's, and the last nine are the engineering
layer's, which reuse `ctl-0001` to `ctl-0009`. The second set is marked (eng).

| Id             | Title                                                                   | Outcome                                                                                       |
|----------------|-------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------|
| ctl-0001       | A merge waits for the validate job                                      | Left out                                                                                      |
| ctl-0002       | Every job in kac.yml sits behind the gate                               | In part: How it works only. Status, verifies, mechanism, frequency, evidence, What it checks, Coverage and gaps went |
| ctl-0003       | Every workflow passes actionlint                                        | Left out                                                                                      |
| ctl-0004       | Every YAML file passes yamllint                                         | Left out                                                                                      |
| ctl-0005       | Every corpus holds the file the template sends it                       | Left out                                                                                      |
| ctl-0006       | A version carries the changelog section its release publishes          | In part: How it works only (as ctl-0002)                                                      |
| ctl-0007       | Every corpus validates and its generated output is fresh                | In part: How it works only (as ctl-0002)                                                      |
| ctl-0008       | Every corpus publishes, installs and is read back                       | Left out                                                                                      |
| ctl-0009       | Three test layers run before a merge                                    | In part: How it works only (as ctl-0002)                                                      |
| ctl-0010       | The documentation site builds with no dead link                         | Left out                                                                                      |
| ctl-0011       | Every corpus whose records changed has moved its content-version        | In part: How it works only (as ctl-0002)                                                      |
| ctl-0012       | Nothing checks a page or the output for accessibility                   | Left out                                                                                      |
| ctl-0013       | Nothing checks prose against the house style                            | Left out                                                                                      |
| ctl-0014       | A pinned component's assessment names the version it is pinned at       | Left out                                                                                      |
| ctl-0001 (eng) | Nothing checks that an image is named by digest                         | Left out                                                                                      |
| ctl-0002 (eng) | Nothing checks that a running service reports its commit                | Left out                                                                                      |
| ctl-0003 (eng) | Nothing checks that an alert names an owner and a first action          | Left out                                                                                      |
| ctl-0004 (eng) | Nothing checks that a skipped test cites a deviation                    | In part: What it checks only. Status, fields, How it works, Coverage and gaps went            |
| ctl-0005 (eng) | Nothing checks that an interface change carries a version and a notice | Left out                                                                                      |
| ctl-0006 (eng) | Nothing checks a repository for a committed secret                      | Left out                                                                                      |
| ctl-0007 (eng) | Nothing checks that a pull request names the agent that produced it     | In part: What it checks only (as ctl-0004 eng)                                                |
| ctl-0008 (eng) | Nothing checks that a .NET build collects coverage                      | In part: What it checks only (as ctl-0004 eng)                                                |
| ctl-0009 (eng) | Nothing checks that a suppression is local and justified                | In part: What it checks only (as ctl-0004 eng)                                                |

## deviations

| Id                               | Title                                                              | Outcome                                                                                                   |
|----------------------------------|--------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------|
| dev-decisions-live-in-commits    | The reasoning behind a decision lives in the commit that made it   | Left out                                                                                                  |
| dev-export-has-no-notice         | A consumer of the export gets no notice period                     | Left out                                                                                                  |
| dev-generated-output-ungoverned  | A generated file keeps no record of the run that wrote it          | Left out                                                                                                  |
| dev-github-concentration         | Everything here runs on GitHub, and nothing says how it would leave | Left out                                                                                                 |
| dev-no-incident-process          | There is no incident process, so everything is handled informally  | Left out                                                                                                  |
| dev-no-reproducible-build        | A build of kac takes whichever .NET 10 SDK the runner installs     | In part: Status, departs-from and What we are doing instead. risk, owner, accepted-on and the other sections went |
| dev-one-maintainer               | One person knows how this repository works                         | Left out                                                                                                  |
| dev-package-unscanned            | Nothing scans the package kac ships                                | Left out                                                                                                  |
| dev-security-design-unstated     | A security requirement arrives as a review comment                 | Left out                                                                                                  |
| dev-standing-publish-rights      | The maintainer's publish rights never go away                      | Left out                                                                                                  |
| dev-vulnerability-route-unstated | Nobody is accountable for a vulnerability left past its window     | Left out                                                                                                  |

## fixes

| Id       | Title                                                          | Outcome                                                                                  |
|----------|----------------------------------------------------------------|------------------------------------------------------------------------------------------|
| fix-0001 | pip cannot find the yamllint version the lint job pins         | In part: symptom-keywords, verified and the Why it happens section went                  |
| fix-0002 | An .editorconfig key edited from a shell changes nothing in the IDE | Left out                                                                            |

## processes

| Id                      | Title                                    | Outcome  |
|-------------------------|------------------------------------------|----------|
| prc-add-a-record        | Add a record to a corpus                 | Left out |
| prc-add-a-type          | Add a knowledge type                     | Left out |
| prc-change-a-skill      | Change a skill or a hook                 | Left out |
| prc-change-the-schema   | Change the schema                        | Left out |
| prc-change-the-tool     | Change the tool                          | Left out |
| prc-pull-request        | Open a pull request                      | Left out |
| prc-review-a-board-lane | Review and tidy a lane of the project board | Left out |
| prc-sweep-prose         | Sweep the writing rules across a folder  | Left out |
| prc-write-public-docs   | Write the public documentation           | Left out |

## reports

| Id                     | Title              | Outcome  |
|------------------------|--------------------|----------|
| rpt-framework-coverage | Framework coverage | Left out |
| rpt-clause-coverage    | Clause coverage    | Left out |

## runbooks

| Id                              | Title                                                     | Outcome  |
|---------------------------------|-----------------------------------------------------------|----------|
| rbk-goldens-disagree            | The golden suite reports a diff nobody meant              | Left out |
| rbk-nuget-404-on-publish        | nuget.org answers 404 for a version it has already accepted | Left out |
| rbk-restore-cannot-find-version | kac restore cannot find the version the lock names        | Left out |

## services

| Id              | Title               | Outcome  |
|-----------------|---------------------|----------|
| svc-corpus-feed | Corpus package feed | Left out |
| svc-docs-site   | Documentation site  | Left out |
| svc-kac         | kac                 | Left out |
| svc-marketplace | Plugin marketplace  | Left out |

## standards

| Id          | Title                                                                                  | Outcome                                                                                                       |
|-------------|----------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------|
| std-API     | An interface is described by a contract in the repository                              | Left out                                                                                                      |
| std-CONT    | A container image is pinned by digest and runs as a non-root user                      | Left out                                                                                                      |
| std-CSSTY   | C# follows the runtime team's conventions, and the compiler enforces them              | In part: Status, Summary and Conformance checklist kept; the common fields went                               |
| std-DEPLOY  | One artefact is built once and promoted to production                                  | Left out                                                                                                      |
| std-DEPS    | A dependency is pinned, screened and named in the inventory                            | In part (as std-CSSTY)                                                                                        |
| std-GATES   | A failing check blocks the merge                                                       | In part (as std-CSSTY)                                                                                        |
| std-INCSEV  | Every incident is handled at a stated severity                                         | Left out                                                                                                      |
| std-NETTST  | A .NET test project runs under one framework, and an integration test owns its dependencies | In part (as std-CSSTY)                                                                                   |
| std-OBS     | A service ships its telemetry to one place, keyed by one id                            | Left out                                                                                                      |
| std-PR      | A pull request carries its reasoning and one accountable approval                      | In part (as std-CSSTY)                                                                                        |
| std-SECRET  | A secret is read from the store at run time                                            | Left out                                                                                                      |
| std-TEST    | A test runs at the lowest level that can catch the fault                               | In part (as std-CSSTY)                                                                                        |
| std-VCS     | Everything a service needs is committed to its repository                              | In part (as std-CSSTY)                                                                                        |
| std-A11Y    | The documentation site and kac are usable without sight, a mouse or colour             | In part: Status, Summary and three clauses (Markdown author owns the structure, pinned component carries an assessment, kac carries no meaning in colour alone). The checklist and three clauses (a change that makes either surface worse, the theme owns contrast, what falls short is written down) went |
| std-CI      | Every check and every publish runs from a workflow in this repository                  | In part: Status, Summary and all six clauses kept; the Conformance checklist went                             |
| std-CONFIG  | Configuration is committed, pinned and explained                                       | In part: Status, Summary and all five clauses kept; the Conformance checklist went                            |
| std-PLUGIN  | The plugin carries the corpus, and a skill answers from what travelled                 | In part: Status and four clauses (a hook runs in a POSIX shell, a skill assumes only that the session can read a file, plugin.json declares every component, the bundle carries a frozen export). The Summary, the checklist and nine clauses (deviation request, finding, framework finding, skill names the type of every field, keyword as cost, says what its corpus did not carry, states what it is for, writes back to the tracker, triage) went |
| std-PROSE   | Prose reads the same way whoever wrote it                                              | In part: Status, Summary and all five clauses kept; the Conformance checklist went                            |
| std-VERS    | A version stamp says what it versions, and a move says what changed                    | In part: Status, Summary and all six clauses kept; the Conformance checklist went                             |

## tools

| Id                     | Title                  | Outcome  |
|------------------------|------------------------|----------|
| tol-actionlint         | actionlint             | Left out |
| tol-checkout           | actions/checkout       | Left out |
| tol-claude-code-cli    | Claude Code CLI        | Left out |
| tol-dotnet-sdk         | .NET SDK               | Left out |
| tol-gh                 | GitHub CLI             | Left out |
| tol-github-pages       | GitHub Pages actions   | Left out |
| tol-markdig            | Markdig                | Left out |
| tol-mkdocs-material    | Material for MkDocs    | Left out |
| tol-mkdocs-redirects   | mkdocs-redirects       | Left out |
| tol-mkdocs             | MkDocs                 | Left out |
| tol-nuget-login        | NuGet/login            | Left out |
| tol-reqnroll           | Reqnroll               | Left out |
| tol-setup-dotnet       | actions/setup-dotnet   | Left out |
| tol-setup-go           | actions/setup-go       | Left out |
| tol-setup-python       | actions/setup-python   | Left out |
| tol-spectre-console    | Spectre.Console        | Left out |
| tol-system-commandline | System.CommandLine     | Left out |
| tol-xunit              | xUnit.net              | Left out |
| tol-yamldotnet         | YamlDotNet             | Left out |
| tol-yamllint           | yamllint               | Left out |

## adrs

| Id       | Title                                                                                     | Outcome  |
|----------|-------------------------------------------------------------------------------------------|----------|
| adr-0001 | Knowledge as code: a structured, validated wiki that AI sessions can read and contribute to | Left out |

## glossary

| Id                    | Title             | Outcome  |
|-----------------------|-------------------|----------|
| gls-engineering       | Engineering       | Left out |
| gls-knowledge-as-code | Knowledge as code | Left out |

## policies

| Id       | Title                                                              | Outcome  |
|----------|--------------------------------------------------------------------|----------|
| pol-A11Y | Software we build is usable by everyone                            | Left out |
| pol-ACCS | Access is by individual identity, on least privilege               | Left out |
| pol-AGNT | Agents propose, people decide                                      | Left out |
| pol-AUTV | Every change is verified automatically, and failures block         | Left out |
| pol-COST | Cost is a non-functional requirement                               | Left out |
| pol-DATA | Data is protected according to its sensitivity                     | Left out |
| pol-DERV | Derived data is verified before it is trusted                      | Left out |
| pol-DEVI | Deviations are recorded, owned and time-bound                      | Left out |
| pol-ENVS | Environments are separated, and production stays in production     | Left out |
| pol-EVER | Everything is in version control                                   | Left out |
| pol-INCR | Incidents are managed and learned from                             | Left out |
| pol-INTC | Interfaces are contracts we honour                                 | Left out |
| pol-KNOW | Knowledge is written down and kept with what it describes          | Left out |
| pol-MEXP | Exposure is minimised and traffic is controlled                    | Left out |
| pol-MNTN | Software stays easy to change                                      | Left out |
| pol-OBSV | Systems are observable and actively monitored                      | Left out |
| pol-PERF | Performance targets are stated and verified                        | Left out |
| pol-PIPE | Changes reach production through the pipeline                      | Left out |
| pol-RECV | Services and data are recoverable                                  | Left out |
| pol-SCRT | Secrets are managed, never embedded                                | Left out |
| pol-SECD | Security is designed in, not added on                              | Left out |
| pol-SUPL | Third-party services are chosen deliberately, and watched          | Left out |
| pol-TRUS | We ship only components we know and trust                          | Left out |
| pol-VURM | Vulnerabilities are found, prioritised and closed to a timeframe   | Left out |

## How I chose

A session here writes C#, workflows, YAML, skills and prose, and then versions and ships them, so the budget went first
to the clauses it has to obey: the obligations of the six standards this corpus states for itself. Five of them went in
with every clause. std-PLUGIN kept the four clauses about how a skill, a hook and `plugin.json` are built, and lost the
ones governing how a skill files and triages issues. std-A11Y kept the clauses a docs page or a `kac` message meets. A
conformance checklist restates its clauses, so I dropped the checklists wherever the clauses were kept. The engineering
standards export only a summary and a checklist, so I kept those whole for the seven that bear on a .NET repository
with CI: code style, testing, .NET testing, gates, pull requests, dependencies and version control. I left out
containers, deployment, observability, secrets for a running service, API contracts and incident severity, because
nothing here runs a service. A few control sections earned a place where they add facts the standards do not give: the
CI commands and tests that enforce a rule, such as the three test layers and `KAC_BASE_REF`, and four engineering
controls whose What it checks section quotes a .NET or pull request obligation the engineering summaries leave out.
The yamllint fix and the SDK deviation went in because a session meets both when it runs the linters or edits a pin.
Policies, tools, services, processes, runbooks, reports, the ADR and the glossary carry context, not rules a session
builds to, or the standards already restate them, so they went. Source links and traceability fields such as `covers:`
and `implements:` went everywhere, because they tell a session where a rule came from and not what to do.
