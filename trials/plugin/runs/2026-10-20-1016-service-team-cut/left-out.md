# What the cut left out

Every record kept in part lost its `Source:` line. Each standard kept its `Status:` line and dropped `category:`, `implements:`, `applies-to:`, `tags:` and, on std-NETTST, `depends-on:`. Each kept rule also dropped its `covers:` line, and its `seeAlso:` line where it had one. Every standard dropped its `#### Conformance checklist` section. Each policy dropped `Status:` and `category:`, and its `#### Purpose`, `#### Scope` and `#### Exceptions` sections; pol-DEVI kept its Exceptions section. The glossary record dropped `Status:`, `tags:` and its `#### Scope` section.

## controls

| Id | Title | Outcome |
|----|-------|---------|
| ctl-0001 | A merge waits for the validate job | Left out |
| ctl-0002 | Every job in kac.yml sits behind the gate | Left out |
| ctl-0003 | Every workflow passes actionlint | Left out |
| ctl-0004 | Every YAML file passes yamllint | Left out |
| ctl-0005 | Every corpus holds the file the template sends it | Left out |
| ctl-0006 | A version carries the changelog section its release publishes | Left out |
| ctl-0007 | Every corpus validates and its generated output is fresh | Left out |
| ctl-0008 | Every corpus publishes, installs and is read back | Left out |
| ctl-0009 | Three test layers run before a merge | Left out |
| ctl-0010 | The documentation site builds with no dead link | Left out |
| ctl-0011 | Every corpus whose records changed has moved its content-version | Left out |
| ctl-0012 | Nothing checks a page or the output for accessibility | Left out |
| ctl-0013 | Nothing checks prose against the house style | Left out |
| ctl-0014 | A pinned component's assessment names the version it is pinned at | Left out |
| ctl-0001 (eng) | Nothing checks that an image is named by digest | Left out |
| ctl-0002 (eng) | Nothing checks that a running service reports its commit | Left out |
| ctl-0003 (eng) | Nothing checks that an alert names an owner and a first action | Left out |
| ctl-0004 (eng) | Nothing checks that a skipped test cites a deviation | Left out |
| ctl-0005 (eng) | Nothing checks that an interface change carries a version and a notice | Left out |
| ctl-0006 (eng) | Nothing checks a repository for a committed secret | Left out |
| ctl-0007 (eng) | Nothing checks that a pull request names the agent that produced it | Left out |
| ctl-0008 (eng) | Nothing checks that a .NET build collects coverage | Left out |
| ctl-0009 (eng) | Nothing checks that a suppression is local and justified | Left out |

## deviations

| Id | Title | Outcome |
|----|-------|---------|
| dev-decisions-live-in-commits | The reasoning behind a decision lives in the commit that made it | Left out |
| dev-export-has-no-notice | A consumer of the export gets no notice period | Left out |
| dev-generated-output-ungoverned | A generated file keeps no record of the run that wrote it | Left out |
| dev-github-concentration | Everything here runs on GitHub, and nothing says how it would leave | Left out |
| dev-no-incident-process | There is no incident process, so everything is handled informally | Left out |
| dev-no-reproducible-build | A build of kac takes whichever .NET 10 SDK the runner installs | Left out |
| dev-one-maintainer | One person knows how this repository works | Left out |
| dev-package-unscanned | Nothing scans the package kac ships | Left out |
| dev-security-design-unstated | A security requirement arrives as a review comment | Left out |
| dev-standing-publish-rights | The maintainer's publish rights never go away | Left out |
| dev-vulnerability-route-unstated | Nobody is accountable for a vulnerability left past its window | Left out |

## fixes

| Id | Title | Outcome |
|----|-------|---------|
| fix-0001 | pip cannot find the yamllint version the lint job pins | Left out |
| fix-0002 | An .editorconfig key edited from a shell changes nothing in the IDE | Left out |

## processes

| Id | Title | Outcome |
|----|-------|---------|
| prc-add-a-record | Add a record to a corpus | Left out |
| prc-add-a-type | Add a knowledge type | Left out |
| prc-change-a-skill | Change a skill or a hook | Left out |
| prc-change-the-schema | Change the schema | Left out |
| prc-change-the-tool | Change the tool | Left out |
| prc-pull-request | Open a pull request | Left out |
| prc-review-a-board-lane | Review and tidy a lane of the project board | Left out |
| prc-sweep-prose | Sweep the writing rules across a folder | Left out |
| prc-write-public-docs | Write the public documentation | Left out |

## reports

| Id | Title | Outcome |
|----|-------|---------|
| rpt-framework-coverage | Framework coverage | Left out |
| rpt-clause-coverage | Clause coverage | Left out |

## runbooks

| Id | Title | Outcome |
|----|-------|---------|
| rbk-goldens-disagree | The golden suite reports a diff nobody meant | Left out |
| rbk-nuget-404-on-publish | nuget.org answers 404 for a version it has already accepted | Left out |
| rbk-restore-cannot-find-version | kac restore cannot find the version the lock names | Left out |

## services

| Id | Title | Outcome |
|----|-------|---------|
| svc-corpus-feed | Corpus package feed | Left out |
| svc-docs-site | Documentation site | Left out |
| svc-kac | kac | Left out |
| svc-marketplace | Plugin marketplace | Left out |

## standards

| Id | Title | Outcome |
|----|-------|---------|
| std-API | An interface is described by a contract in the repository | In part: Status, Summary and all three rules (a-change-carries-a-version-and-a-notice, every-endpoint-authenticates-and-validates, the-contract-is-the-source-of-truth) |
| std-CONT | A container image is pinned by digest and runs as a non-root user | In part: Status, Summary and all three rules (a-published-tag-never-moves, the-base-image-is-chosen-and-pinned, the-container-holds-only-what-the-service-needs) |
| std-CSSTY | C# follows the runtime team's conventions, and the compiler enforces them | In part: Status, Summary and all three rules (a-suppression-is-local-and-says-why, the-conventions-come-from-one-file, the-security-rules-the-analysers-cover) |
| std-DEPLOY | One artefact is built once and promoted to production | In part: Status, Summary and all four rules (a-way-back-exists-before-the-change-goes, configuration-sits-outside-the-artefact, production-changes-through-the-pipeline-alone, the-artefact-is-built-once) |
| std-DEPS | A dependency is pinned, screened and named in the inventory | In part: Status, Summary and all three rules (what-happens-afterwards, what-happens-before-adoption, where-a-package-comes-from) |
| std-GATES | A failing check blocks the merge | In part: Status, Summary and both rules (a-failing-check-stops-the-change, every-change-is-built-and-tested-automatically) |
| std-INCSEV | Every incident is handled at a stated severity | In part: Status, Summary and every-incident-has-a-severity; a-significant-incident-is-reviewed, the-severity-moves-with-what-is-known and the-severity-sets-the-response went |
| std-NETTST | A .NET test project runs under one framework, and an integration test owns its dependencies | In part: Status, Summary and all four rules (a-unit-test-crosses-no-boundary, an-integration-test-starts-what-it-needs, coverage-is-collected-by-the-build, the-framework-and-the-project-layout) |
| std-OBS | A service ships its telemetry to one place, keyed by one id | In part: Status, Summary and all four rules (everything-lands-in-the-central-store, one-request-reads-as-one-timeline, somebody-acts-on-every-alert, telemetry-carries-no-personal-data) |
| std-PR | A pull request carries its reasoning and one accountable approval | In part: Status, Summary and all three rules (a-change-arrives-as-a-pull-request, agent-produced-work-says-what-produced-it, somebody-other-than-the-author-approves-it) |
| std-SECRET | A secret is read from the store at run time | In part: Status, Summary and all three rules (a-secret-comes-from-the-store, every-secret-rotates, nowhere-else-holds-a-secret) |
| std-TEST | A test runs at the lowest level that can catch the fault | In part: Status, Summary and all three rules (know-what-the-suite-reaches, pick-the-level-from-the-fault, the-data-is-never-a-real-customers) |
| std-VCS | Everything a service needs is committed to its repository | In part: Status, Summary and both rules (every-change-is-attributable-and-reviewed, the-repository-holds-everything-the-service-needs) |
| std-A11Y | The documentation site and kac are usable without sight, a mouse or colour | Left out |
| std-CI | Every check and every publish runs from a workflow in this repository | Left out |
| std-CONFIG | Configuration is committed, pinned and explained | Left out |
| std-PLUGIN | The plugin carries the corpus, and a skill answers from what travelled | Left out |
| std-PROSE | Prose reads the same way whoever wrote it | Left out |
| std-VERS | A version stamp says what it versions, and a move says what changed | Left out |

## tools

| Id | Title | Outcome |
|----|-------|---------|
| tol-actionlint | actionlint | Left out |
| tol-checkout | actions/checkout | Left out |
| tol-claude-code-cli | Claude Code CLI | Left out |
| tol-dotnet-sdk | .NET SDK | Left out |
| tol-gh | GitHub CLI | Left out |
| tol-github-pages | GitHub Pages actions | Left out |
| tol-markdig | Markdig | Left out |
| tol-mkdocs-material | Material for MkDocs | Left out |
| tol-mkdocs-redirects | mkdocs-redirects | Left out |
| tol-mkdocs | MkDocs | Left out |
| tol-nuget-login | NuGet/login | Left out |
| tol-reqnroll | Reqnroll | Left out |
| tol-setup-dotnet | actions/setup-dotnet | Left out |
| tol-setup-go | actions/setup-go | Left out |
| tol-setup-python | actions/setup-python | Left out |
| tol-spectre-console | Spectre.Console | Left out |
| tol-system-commandline | System.CommandLine | Left out |
| tol-xunit | xUnit.net | Left out |
| tol-yamldotnet | YamlDotNet | Left out |
| tol-yamllint | yamllint | Left out |

## adrs

| Id | Title | Outcome |
|----|-------|---------|
| adr-0001 | Knowledge as code: a structured, validated wiki that AI sessions can read and contribute to | Left out |

## glossary

| Id | Title | Outcome |
|----|-------|---------|
| gls-engineering | Engineering | In part: terms could, may, must, must-not, personal-data, secret, sensitive-personal-data, should; component, sensitive-data and we went |
| gls-knowledge-as-code | Knowledge as code | Left out |

## policies

| Id | Title | Outcome |
|----|-------|---------|
| pol-A11Y | Software we build is usable by everyone | Left out |
| pol-ACCS | Access is by individual identity, on least privilege | In part: clauses NAMED, AUTHN, PERSIST |
| pol-AGNT | Agents propose, people decide | In part: clauses UNPROV, ACCESS |
| pol-AUTV | Every change is verified automatically, and failures block | In part: clauses REPRO, MAPPED, BROKEN, OFTEN |
| pol-COST | Cost is a non-functional requirement | Left out |
| pol-DATA | Data is protected according to its sensitivity | In part: clauses CLASS, MASK, CRYPTO, LAWFUL, MINIMAL, RIGHTS, DELETE, BULK, SHARE, LINGER, REVIVE, LOGS |
| pol-DERV | Derived data is verified before it is trusted | In part: clauses EXPECT, CHECK, RUNLOG, FAILED |
| pol-DEVI | Deviations are recorded, owned and time-bound | In part: Exceptions section, clauses RECORD, CONTENT |
| pol-ENVS | Environments are separated, and production stays in production | In part: clauses SPLIT, CREDS |
| pol-EVER | Everything is in version control | Left out |
| pol-INCR | Incidents are managed and learned from | In part: clauses RECOVER, HOLD |
| pol-INTC | Interfaces are contracts we honour | In part: clauses DEPREC, COMPAT |
| pol-KNOW | Knowledge is written down and kept with what it describes | In part: clauses SYNC, DECIDE, HEADS, BEHAVE, RUNBOOK |
| pol-MEXP | Exposure is minimised and traffic is controlled | In part: clauses DENY, TRANSIT, PEERID, EGRESS, LATERAL, WEAKEN, PRIVATE |
| pol-MNTN | Software stays easy to change | In part: clauses DEAD, HARDER |
| pol-OBSV | Systems are observable and actively monitored | In part: clauses USAGE, SECMON, TAMPER |
| pol-PERF | Performance targets are stated and verified | In part: clauses MEASURE, PEAK, NOTEST |
| pol-PIPE | Changes reach production through the pipeline | In part: clause FLAGS |
| pol-RECV | Services and data are recoverable | In part: clauses RTORPO, BACKUP, TIMEOUT, DEGRADE, IDEMPOT, UNTEST, RETRY, SHED |
| pol-SCRT | Secrets are managed, never embedded | In part: clauses ENCRYPT, KEYS, REUSE, APART |
| pol-SECD | Security is designed in, not added on | In part: clauses REQS, DESIGN, THREAT, HIRISK |
| pol-SUPL | Third-party services are chosen deliberately, and watched | In part: clause EXIT |
| pol-TRUS | We ship only components we know and trust | In part: clauses SUPPORT, PATCH, MALWARE |
| pol-VURM | Vulnerabilities are found, prioritised and closed to a timeframe | In part: clauses DISCLOS, REGRESS |
