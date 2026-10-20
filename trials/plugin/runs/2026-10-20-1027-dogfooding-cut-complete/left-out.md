# What the cut left out

Every record kept in part lost its field lines: `Status`, `Source` and `tags` from all of them, and the type's own
fields where it has them (`implements`, `category`, `depends-on` and `applies-to` on standards; `last-rehearsed`,
`rehearsal-frequency` and `applies-to` on processes; `severity`, `applies-to`, `last-rehearsed`, `rehearsal-frequency`,
`requires-tools` and `requires-access` on runbooks; `symptom-keywords` and `verified` on fixes; `component-type`,
`platform`, `criticality`, `monitoring-output` and `repos` on services). Every rule kept lost its `covers:` line, and
`seeAlso:` where it had one. Every standard kept lost its Conformance checklist.

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
| fix-0001 | pip cannot find the yamllint version the lint job pins | In part: Symptom, Cause and Resolution kept; Environment and Why it happens left out |
| fix-0002 | An .editorconfig key edited from a shell changes nothing in the IDE | In part: Symptom, Cause, Resolution and Why it happens kept; Environment left out |

## processes

| Id | Title | Outcome |
|----|-------|---------|
| prc-add-a-record | Add a record to a corpus | Left out |
| prc-add-a-type | Add a knowledge type | Left out |
| prc-change-a-skill | Change a skill or a hook | In part: When to use this kept; Prerequisites left out |
| prc-change-the-schema | Change the schema | In part: When to use this kept; Prerequisites left out |
| prc-change-the-tool | Change the tool | In part: When to use this and Prerequisites kept; fields left out |
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
| rbk-goldens-disagree | The golden suite reports a diff nobody meant | In part: Symptoms kept; Impact left out |
| rbk-nuget-404-on-publish | nuget.org answers 404 for a version it has already accepted | Left out |
| rbk-restore-cannot-find-version | kac restore cannot find the version the lock names | In part: Symptoms kept, bar its closing line ("Nothing failed in the producer"); Impact left out |

## services

| Id | Title | Outcome |
|----|-------|---------|
| svc-corpus-feed | Corpus package feed | Left out |
| svc-docs-site | Documentation site | Left out |
| svc-kac | kac | In part: What it does kept; Where it lives, Dependencies and Data left out |
| svc-marketplace | Plugin marketplace | Left out |

## standards

| Id | Title | Outcome |
|----|-------|---------|
| std-API | An interface is described by a contract in the repository | Left out |
| std-CONT | A container image is pinned by digest and runs as a non-root user | Left out |
| std-CSSTY | C# follows the runtime team's conventions, and the compiler enforces them | In part: Summary, `a-suppression-is-local-and-says-why` and `the-conventions-come-from-one-file` kept; `the-security-rules-the-analysers-cover` left out |
| std-DEPLOY | One artefact is built once and promoted to production | Left out |
| std-DEPS | A dependency is pinned, screened and named in the inventory | Left out |
| std-GATES | A failing check blocks the merge | In part: Summary, `a-failing-check-stops-the-change` and `every-change-is-built-and-tested-automatically` kept |
| std-INCSEV | Every incident is handled at a stated severity | Left out |
| std-NETTST | A .NET test project runs under one framework, and an integration test owns its dependencies | In part: Summary, `a-unit-test-crosses-no-boundary`, `coverage-is-collected-by-the-build` and `the-framework-and-the-project-layout` kept; `an-integration-test-starts-what-it-needs` left out |
| std-OBS | A service ships its telemetry to one place, keyed by one id | Left out |
| std-PR | A pull request carries its reasoning and one accountable approval | In part: Summary and `agent-produced-work-says-what-produced-it` kept; `a-change-arrives-as-a-pull-request` and `somebody-other-than-the-author-approves-it` left out |
| std-SECRET | A secret is read from the store at run time | Left out |
| std-TEST | A test runs at the lowest level that can catch the fault | In part: `pick-the-level-from-the-fault` kept; Summary, `know-what-the-suite-reaches` and `the-data-is-never-a-real-customers` left out |
| std-VCS | Everything a service needs is committed to its repository | Left out |
| std-A11Y | The documentation site and kac are usable without sight, a mouse or colour | In part: `a-change-that-makes-either-surface-worse-needs-a-deviation`, `a-markdown-author-owns-the-structure-of-a-page`, `a-pinned-component-carries-an-accessibility-assessment` and `kac-carries-no-meaning-in-colour-alone` kept; Summary, `the-theme-owns-contrast-focus-and-keyboard-reach` and `what-falls-short-is-written-down-where-a-reader-finds-it` left out |
| std-CI | Every check and every publish runs from a workflow in this repository | In part: Summary and all six rules kept (`a-job-declares-the-permission-it-needs`, `a-version-moves-by-hand-and-publishes-once`, `a-workflow-holds-no-credential-of-its-own`, `every-action-is-pinned-to-a-commit`, `one-job-holds-the-write-permission`, `the-gate-runs-on-every-pull-request-into-main`) |
| std-CONFIG | Configuration is committed, pinned and explained | In part: Summary and all five rules kept (`a-file-says-what-it-is-for-and-why-each-value-is-what-it-is`, `a-pin-names-an-exact-version`, `a-value-living-in-more-than-one-tree-is-copied-and-proved`, `every-value-the-build-reads-is-committed`, `the-shape-of-a-yaml-file-is-yamllints-to-decide`) |
| std-PLUGIN | The plugin carries the corpus, and a skill answers from what travelled | In part: `a-hook-runs-in-a-posix-shell-and-pushes-a-view-once-a-session`, `a-skill-assumes-only-that-the-session-can-read-a-file`, `a-skill-names-the-type-of-every-field-it-describes`, `a-skill-states-what-it-is-for-and-the-reason-for-each-rule-it-gives`, `pluginjson-declares-every-component-and-the-types-it-reads` and `the-bundle-carries-a-frozen-export-and-nothing-writes-to-it` kept; Summary, `a-deviation-request-an-agent-files-asks-for-the-owner-it-cannot-name`, `a-finding-an-agent-files-is-an-observation-and-proposes-no-record`, `a-framework-finding-travels-stripped-and-only-by-hand`, `a-skill-reads-a-rules-keyword-as-what-departing-from-it-costs`, `a-skill-says-what-its-corpus-did-not-carry`, `a-skill-writes-back-to-the-tracker-the-corpus-states` and `triage-routes-a-finding-with-a-label-and-says-why-in-a-comment` left out |
| std-PROSE | Prose reads the same way whoever wrote it | In part: Summary and all five rules kept (`commit-messages-and-pull-requests`, `the-base-style`, `what-prose-must-not-change`, `what-stays-true`, `where-this-repository-differs-from-microsoft`) |
| std-VERS | A version stamp says what it versions, and a move says what changed | In part: Summary, `a-producers-move-obliges-every-consumer-in-the-same-pull-request`, `a-published-version-is-never-replaced`, `what-a-move-of-each-stamp-means` and `which-stamps-move-together-and-which-move-alone` kept; `a-reader-meeting-a-stamp-it-does-not-know-stops` and `a-stamp-is-a-semantic-version-a-count-or-a-range` left out |

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
| gls-engineering | Engineering | Left out (no term kept) |
| gls-knowledge-as-code | Knowledge as code | Left out (no term kept) |

## policies

| Id | Title | Outcome |
|----|-------|---------|
| pol-A11Y | Software we build is usable by everyone | Left out |
| pol-ACCS | Access is by individual identity, on least privilege | Left out |
| pol-AGNT | Agents propose, people decide | Left out |
| pol-AUTV | Every change is verified automatically, and failures block | Left out |
| pol-COST | Cost is a non-functional requirement | Left out |
| pol-DATA | Data is protected according to its sensitivity | Left out |
| pol-DERV | Derived data is verified before it is trusted | Left out |
| pol-DEVI | Deviations are recorded, owned and time-bound | Left out |
| pol-ENVS | Environments are separated, and production stays in production | Left out |
| pol-EVER | Everything is in version control | Left out |
| pol-INCR | Incidents are managed and learned from | Left out |
| pol-INTC | Interfaces are contracts we honour | Left out |
| pol-KNOW | Knowledge is written down and kept with what it describes | Left out |
| pol-MEXP | Exposure is minimised and traffic is controlled | Left out |
| pol-MNTN | Software stays easy to change | Left out |
| pol-OBSV | Systems are observable and actively monitored | Left out |
| pol-PERF | Performance targets are stated and verified | Left out |
| pol-PIPE | Changes reach production through the pipeline | Left out |
| pol-RECV | Services and data are recoverable | Left out |
| pol-SCRT | Secrets are managed, never embedded | Left out |
| pol-SECD | Security is designed in, not added on | Left out |
| pol-SUPL | Third-party services are chosen deliberately, and watched | Left out |
| pol-TRUS | We ship only components we know and trust | Left out |
| pol-VURM | Vulnerabilities are found, prioritised and closed to a timeframe | Left out |
