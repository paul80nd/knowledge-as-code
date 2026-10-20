# The knowledge of Example Dogfooding

This is what the corpus records, type by type. Each heading gives the id to cite it by.

## controls

### Every job in kac.yml sits behind the gate (`ctl-0002`)

#### How it works

`WorkflowGateTests` in `tooling/kac.tests` reads `.github/workflows/kac.yml` and collects the keys under `jobs:`. It
compares that set against `validate`'s `needs:` plus `validate` itself. The `tool` job runs it under
`dotnet test tooling/kac.tests`.

Comparing sets catches two faults. A job added with no line in `needs:` fails. A `needs:` entry listing a job the file no longer declares fails as well.

### A version carries the changelog section its release publishes (`ctl-0006`)

#### How it works

`ChangelogTests` in `tooling/kac.tests` runs three tests:

* The first reads the `<Version>` element from `tooling/kac/kac.csproj` and looks for a `## <version> - <date>`
  heading in `tooling/kac/CHANGELOG.md`.
* The second fails a heading with nothing under it, which is a section added to satisfy the first test and left empty.
* The third reads `docs/index.md`, which tells a reader the tool is early because it has not reached 1.0.0. It fails
  that sentence once the version reaches 1.0.0.

The `tool` job runs all three under `dotnet test tooling/kac.tests`.

### Every corpus validates and its generated output is fresh (`ctl-0007`)

#### How it works

The `corpora` job runs `kac validate` and then `kac generate --check` in each of `examples/library`,
`examples/engineering`, `examples/payments` and `examples/dog-fooding`. The `tool` job runs the same pair in
`template/`, and again in a corpus `kac new` created from the packed tool.

Each matrix cell is a fresh runner with its own checkout, and each corpus declares a different `types:`. Running all of them proves the framework works for a corpus that adopted a subset. `payments` and `dog-fooding` pack `engineering` and restore it first, because `.imports/` is not committed.

### Three test layers run before a merge (`ctl-0009`)

#### How it works

The `tool` job runs three commands:

* `dotnet test tooling/kac.tests` for the unit layer.
* `dotnet test tooling/kac.features` for the Reqnroll behaviour specs.
* `dotnet run tooling/kac-tests.cs` for the golden fixtures.

[Build, test and release](https://paul80nd.github.io/knowledge-as-code/maintaining/build-test-release/) says what each layer proves.

The golden suite adds two checks of its own. It fails a reachable check id that no fixture exercises. It fails a type page whose generated checks table has drifted from the catalogue. Each fixture is built over the real `.schema/`, copied in on each run, so a schema change that alters behaviour shows up in the run that made it.

### Every corpus whose records changed has moved its content-version (`ctl-0011`)

#### How it works

`ContentVersionTests` in `tooling/kac.tests` compares the branch against the commit it grew from, in four steps:

* It asks `git diff` for every file the branch added, changed, renamed or deleted since the merge base.
* It asks `git ls-files` for a file the working tree keeps that nobody has committed, which `git diff` never reports.
* It keeps the files that are records of a type declaring an `export:` block, in a corpus under `examples/`.
* For each corpus left, it reads `content-version` from `.corpus.yaml` at both revisions, and fails where the stamp
  did not rise.

The `tool` job runs it under `dotnet test tooling/kac.tests`. That job checks out with `fetch-depth: 0` and names the target branch in `KAC_BASE_REF`, because a shallow checkout keeps no base to compare against. A job that names a branch git cannot reach fails rather than passing on a comparison it never made.

### Nothing checks that a skipped test cites a deviation (`ctl-0004`)

#### What it checks

* [std-GATES.a-failing-check-stops-the-change] says a team "**MUST NOT** skip, silence or suppress a check without a
  recorded deviation".

### Nothing checks that a pull request names the agent that produced it (`ctl-0007`)

#### What it checks

* [std-PR.agent-produced-work-says-what-produced-it] says a pull request containing agent-produced work "**MUST** name
  the agent, the model and the prompt or task that produced it".

### Nothing checks that a .NET build collects coverage (`ctl-0008`)

#### What it checks

* [std-NETTST.coverage-is-collected-by-the-build] says a build "**MUST** collect coverage from its test run and publish
  the report".
* It says a build "**SHOULD** collect it with `dotnet test --collect:"XPlat Code Coverage"`, which writes a Cobertura
  report".

### Nothing checks that a suppression is local and justified (`ctl-0009`)

#### What it checks

* [std-CSSTY.a-suppression-is-local-and-says-why] says a suppression "**MUST** sit on the member it applies to, with a
  `Justification` giving the reason".
* It says a repository "**MUST NOT** disable a rule in `.editorconfig` to clear a warning in one file".

## deviations

### A build of kac takes whichever .NET 10 SDK the runner installs (`dev-no-reproducible-build`)

Status: active.
departs-from: eng:pol-AUTV.REPRO, std-CONFIG.a-pin-names-an-exact-version

#### What we are doing instead

Each job that builds it installs the SDK with [tol-setup-dotnet] and `dotnet-version: 10.0.x`, which takes the newest 10.0 SDK on the day. [tol-dotnet-sdk] approves the same range. No `global.json` fixes the version, so version control alone does not say which SDK built a commit.

The package published to nuget.org is built once, by the publish workflow, from the commit on `main`. Its run log shows the SDK that job installed.

## fixes

### pip cannot find the yamllint version the lint job pins (`fix-0001`)

Status: active.

#### Symptom

`pip install -r .github/requirements.txt` fails on a Mac, and the release it says is missing is the one the file pins.

```text
ERROR: Could not find a version that satisfies the requirement yamllint==1.38.0 (from versions: 0.1.0, ... 1.37.1)
ERROR: No matching distribution found for yamllint==1.38.0
```

The versions it offers stop one release short of the pin.

#### Environment

macOS, running Apple's `/usr/bin/python3`, which is 3.9. `.github/requirements.txt` pins yamllint at 1.38.0.

#### Cause

yamllint 1.38.0 needs Python 3.10 or newer. pip filters the index down to the releases the running interpreter supports, so the pinned release is on PyPI and invisible from 3.9. The error quotes the pin it was given, and says nothing about the interpreter.

#### Resolution

1. Install both linters from Homebrew, which installs a Python of its own.

```sh
   brew install yamllint actionlint
   ```

2. Check each version against what the `lint` job pins.

```sh
   yamllint --version    # 1.38.0
   actionlint --version  # 1.7.12
   ```

Homebrew installs the pinned yamllint and the pinned actionlint together. The `lint` job installs actionlint with a
`go install` line, so `.github/requirements.txt` does not list it.

## standards

### C# follows the runtime team's conventions, and the compiler enforces them (`std-CSSTY`)

Status: draft.

#### Summary

C# in this estate is written to the .NET runtime team's own coding style, set out as an `.editorconfig` in the repository. Analysers run as part of the build. A warning is a defect.

#### Conformance checklist

- [ ] The repository root has an `.editorconfig`, and it matches the upstream one apart from documented departures.
- [ ] A build with a style violation fails.
- [ ] `<NoWarn>` is empty in every project file.
- [ ] Every `SuppressMessage` in the repository has a `Justification` a reader can act on.
- [ ] The solution builds with zero warnings from a clean checkout.
- [ ] No `!` appears in the repository outside a test asserting a null argument is refused.

### A dependency is pinned, screened and named in the inventory (`std-DEPS`)

Status: draft.

#### Summary

Every third-party package a build pulls in is pinned to a version and comes from a feed we control. Somebody screens it for vulnerabilities and licence terms before anyone adopts it. A critical finding stops the release.

#### Conformance checklist

- [ ] Every package source configured in the repository points at the organisation's feed.
- [ ] The lockfile is committed, and CI installs from it without resolving afresh.
- [ ] The dependency scan runs on every build, and its results are visible without opening the tool.
- [ ] No critical finding is open past 7 days without a recorded deviation.
- [ ] Every licence in the tree appears on the allowed list.
- [ ] The build artefacts for the last release are still in the managed repository.

### A failing check blocks the merge (`std-GATES`)

Status: draft.

#### Summary

Every push builds the code and runs the test suite automatically. A check that fails stops the change. Turning a check off takes a recorded deviation with a named owner.

#### Conformance checklist

- [ ] A push triggers the build with no manual step.
- [ ] Branch policy lists the build and the test suite as required checks.
- [ ] The build passes from a clean clone on a fresh agent.
- [ ] Every skipped, excluded or suppressed check names a deviation.
- [ ] Every such deviation is open, owned by an individual, and inside its review date.
- [ ] The last defect fix in this repository has a test that fails without the fix.

### A .NET test project runs under one framework, and an integration test owns its dependencies (`std-NETTST`)

Status: draft.

#### Summary

A repository's C# tests use one test framework and one assertion style. A unit test crosses no process boundary. An integration test starts the dependencies it needs, and shares none of them with another run.

#### Conformance checklist

- [ ] Every test project in the repository references the same test framework, and `dotnet test` runs them all.
- [ ] Every test project uses one assertion style.
- [ ] No unit test project references Testcontainers or a database driver.
- [ ] A search for `Thread.Sleep` and `async void` across the test projects returns nothing.
- [ ] The integration suite passes on a machine running no shared database, queue or service.
- [ ] Running the integration suite twice in a row passes both times.
- [ ] The build publishes a coverage report, and the test projects are excluded from it.

### A pull request carries its reasoning and one accountable approval (`std-PR`)

Status: draft.

#### Summary

Every change to the default branch arrives as a pull request that says what it does and why. A named person other than the author approves it before it merges.

#### Conformance checklist

- [ ] Branch policy requires a pull request, and requires at least one approval.
- [ ] Branch policy excludes the author from satisfying that approval, whether that author is a person or an agent.
- [ ] Auto-merge, where it is enabled, cannot complete a pull request that no other identity has approved.
- [ ] The description links a work item.
- [ ] Where an agent produced any part of the change, the description names the agent, the model and the task.
- [ ] The reviewer states what they checked, and does not restate what the change claims.

### A test runs at the lowest level that can catch the fault (`std-TEST`)

Status: draft.

#### Summary

A team writes each test at the cheapest level that can fail on the fault it looks for. The whole suite runs on data that was never a real customer's.

#### Conformance checklist

- [ ] The suite that runs on every push finishes in under ten minutes.
- [ ] Running the suite twice in a row, and in a different order, gives the same result.
- [ ] No test in the repository calls a hostname outside the build agent.
- [ ] The build publishes a coverage figure, and the threshold it fails at is in the repository.
- [ ] Every fixture in the repository is generated or masked, confirmed by reading it.
- [ ] No test configuration names a production connection string or endpoint.

### Everything a service needs is committed to its repository (`std-VCS`)

Status: draft.

#### Summary

One repository contains everything that builds, deploys, runs and recovers a service. Every change to it arrives by reviewed merge from an identified author.

#### Conformance checklist

- [ ] The repository builds from a clean clone on a machine that has never seen this service.
- [ ] The infrastructure definition is in the repository, and the running estate matches it.
- [ ] Branch protection is on, and it blocks direct pushes to the default branch.
- [ ] Recent commits each name a work item.
- [ ] No commit in the last release has a shared account as its author.

### The documentation site and kac are usable without sight, a mouse or colour (`std-A11Y`)

Status: active.

#### Summary

This standard covers two surfaces: the documentation site MkDocs builds from `docs/`, and the `kac` command line. [WCAG 2.2 AA] governs the site. On it a Markdown author owns heading order, link text, table structure and image alternatives, and Material for MkDocs owns contrast, focus and keyboard reach. WCAG is written for content a browser renders, so it does not apply to a terminal. `kac` follows rules stated in the terms a terminal has: colour repeats what the words already say, and every answer a prompt asks for is also a flag. Both components are pinned, and each has an assessment in its tool record stating what was checked and what falls short.

#### A Markdown author owns the structure of a page (`std-A11Y.a-markdown-author-owns-the-structure-of-a-page`)

obligations:
- A page **MUST** open on one H1.
- Heading levels **MUST** descend one at a time.
- A heading **MUST** name what sits beneath it.
- Link text **MUST** say where the link goes when the words around it are gone.
- A table **MUST** have a header row.
- A table **MUST** contain data, and never a layout.
- An image **MUST** have alternative text saying what a reader who cannot see it needs.
- A diagram **MUST** have a text equivalent stating the same information.
- That text equivalent **SHOULD** be prose following the diagram.
- A page **MUST NOT** ask a reader to act on a colour, a shape or a position.
- A change to a page **MUST** pass the conformance checklist below before it merges.

#### A pinned component carries an accessibility assessment (`std-A11Y.a-pinned-component-carries-an-accessibility-assessment`)

obligations:
- A third-party component rendering either surface **MUST** have an assessment in its tool record.
- The assessment **MUST** name what was checked, and how.
- The assessment **MUST** name what falls short, or say that nothing found does.
- The assessment **MUST** name the version it was made against.
- A pull request moving a pin to a new major or minor version **MUST** include a new assessment.
- We **MUST NOT** adopt a component the assessment finds short without a recorded deviation.

#### kac carries no meaning in colour alone (`std-A11Y.kac-carries-no-meaning-in-colour-alone`)

obligations:
- Every message **MUST** state its meaning in words.
- Colour **MUST** repeat what the words already say.
- Every verb **MUST** honour `NO_COLOR` in the environment and `--no-color` on the command line.
- Output **MUST** stay complete when colour is off.
- Output **MUST NOT** separate one column from the next with a box-drawing character or an icon.
- Every answer a prompt asks for **MUST** also be available as a flag.
- A run with no terminal **MUST** exit rather than wait for a keystroke.
- A message a pipeline parses **MUST** go to `--json`, and never through a renderer.
### Every check and every publish runs from a workflow in this repository (`std-CI`)

Status: active.

#### Summary

One workflow gates a pull request. The others publish what has merged: the tool to nuget.org, each corpus to GitHub
Packages, the plugins to the `marketplace` branch, and the site to GitHub Pages. A job declares what it may do before
it does it, pins the actions it runs, and stores no credential of its own. Those three rules are [OpenSSF Scorecard]'s
`Token-Permissions`, `Pinned-Dependencies` and `Dangerous-Workflow` checks. [actionlint] decides the rest of how a
workflow is written.

#### A job declares the permission it needs (`std-CI.a-job-declares-the-permission-it-needs`)

obligations:
- A workflow **MUST** declare `permissions: contents: read` at its top level, which is what [OpenSSF Scorecard]'s `Token-Permissions` check asks for.
- A job needing wider access **MUST** declare that permission on itself.
- A job **MUST NOT** declare a permission no step in it uses.
- A workflow **MUST NOT** commit to a branch a person edits.

#### A version moves by hand and publishes once (`std-CI.a-version-moves-by-hand-and-publishes-once`)

obligations:
- A change to `kac` a user can observe **MUST** take a line under `## Unreleased` in `tooling/kac/CHANGELOG.md`, in the pull request making that change.
- A pull request **MUST NOT** move `<Version>` in `tooling/kac/kac.csproj` unless the branch owner has decided to release.
- A pull request moving `<Version>` **MUST** rename `## Unreleased` to that version and its date.
- That rename **SHOULD** sit in the commit moving `<Version>`.
- `content-version` in a corpus's `.corpus.yaml` **MUST** move in the pull request changing what that corpus knows.
- A publishing job **MUST** ask the registry whether it already holds the version in front of it.
- Where the registry holds that version already, a publishing job **MUST** finish green.
- A publish to nuget.org **MUST** wait for a person to approve the `nuget.org` environment.
- A publish to nuget.org **MUST** tag the commit it published from.
- The release notes **MUST** be that version's section of `tooling/kac/CHANGELOG.md`.
- A person **MUST NOT** push a package from their own machine.

#### A workflow holds no credential of its own (`std-CI.a-workflow-holds-no-credential-of-its-own`)

obligations:
- A workflow **MUST** take every credential from GitHub's secret store or from an identity it exchanged.
- A publish to nuget.org **MUST** authenticate through a trusted publishing policy naming this repository, this workflow file and the `nuget.org` environment.
- A job **MUST** exchange its own identity for a short-lived key in the step before the one that uses it.
- A workflow calling GitHub's own API **MUST** take `github.token`.
- A step **MUST** pass a secret to a script through `env:`.
- A step **MUST NOT** interpolate a secret into a script body.
- A step **MUST NOT** print a secret.

#### Every action is pinned to a commit (`std-CI.every-action-is-pinned-to-a-commit`)

obligations:
- A step in `.github/workflows/` **MUST** pin its action to a commit SHA, which is what [OpenSSF Scorecard]'s `Pinned-Dependencies` check asks for.
- A pinned step **SHOULD** state the released version in a trailing comment, as `# v7.0.1`, because a reviewer reads the version there.
- `.github/dependabot.yml` **MUST** track `github-actions`.
- A job with a write permission **MUST NOT** install a tool at a moving version.
- A workflow needing a tool at `latest` **MUST** install it in a job with `contents: read`.

#### One job holds the write permission (`std-CI.one-job-holds-the-write-permission`)

obligations:
- A publishing workflow **MUST** declare its write permission on one job.
- A workflow publishing a package or a branch **MUST** declare `needs:` on a job with `contents: read`.
- A workflow publishing a package or a branch **MUST** run its checks again against the merge commit.
- The publishing job **MUST** build what it publishes from that commit.
- The publishing job **MUST** declare `timeout-minutes`.
- A workflow publishing a version **MUST** declare a `concurrency` group that queues rather than cancels.
- A branch a workflow publishes **MUST** contain only the files that workflow built.
- A workflow writing to a branch **SHOULD** empty its worktree before it stages a file.
- That workflow **SHOULD** prove `git ls-files` returns nothing in the emptied worktree.
- A person **MUST NOT** edit a published branch, package or release by hand.

#### The gate runs on every pull request into main (`std-CI.the-gate-runs-on-every-pull-request-into-main`)

obligations:
- A check that can block a merge **MUST** run in `.github/workflows/kac.yml` on a pull request into `main`.
- `kac.yml` **MUST** name every one of its jobs in the `needs:` of its `validate` job.
- The branch rule on `main` **MUST** name `validate` as the check a merge waits for.
- A job **MUST NOT** declare `continue-on-error`.
- A workflow **MUST** pass `actionlint`, which also puts every `run:` block through shellcheck.
- A workflow file **MUST** arrive on `main` by the same reviewed merge as any other change.

### Configuration is committed, pinned and explained (`std-CONFIG`)

Status: active.

#### Summary

The schema under `.schema/`, each corpus's descriptor, the overlay manifest, the site's navigation and the dependency pins all decide what this repository builds and publishes. Each of them is a tracked file. Each states an exact version where it states a version, and each has the comment a reviewer needs to judge the change. [yamllint] decides how the YAML itself is shaped, and `.yamllint` lists the few places this repository departs from it.

#### A file says what it is for, and why each value is what it is (`std-CONFIG.a-file-says-what-it-is-for-and-why-each-value-is-what-it-is`)

obligations:
- A configuration file **SHOULD** open with a comment saying what it is for, or with the `# yaml-language-server: $schema=` line naming what describes it.
- A comment **MUST** give the reason a value is what it is.
- A comment **SHOULD NOT** restate the value beside it.
- Where one line states the reason, a comment **SHOULD** run to one line.

#### A pin names an exact version (`std-CONFIG.a-pin-names-an-exact-version`)

obligations:
- A dependency **MUST** name one version, as a `PackageReference` does and as a line in `docs/requirements.txt` does.
- `.github/dependabot.yml` **MUST** name every directory containing a manifest that builds this repository, which is what [OpenSSF Scorecard]'s `Dependency-Update-Tool` check asks for.
- Every ecosystem it tracks **MUST** have a `schedule:`.
- Packages that only move as a set **SHOULD** be raised as one grouped update.
- A pin Dependabot cannot read **MUST** be named in that file's header comment, with what it pins today.

#### A value living in more than one tree is copied and proved (`std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved`)

obligations:
- A setting every project under `tooling/` takes **SHOULD** sit in `tooling/Directory.Build.props`.
- You **MUST** copy a file `manifest.yaml` names into `template/` and into each corpus under `examples/`.
- Every overlay copy **MUST** match its source, as `kac update --check --from ../../` reports in that corpus.
- You **MUST** read `.schema/` and `template/.plugin/` where they are authored.
- You **MUST NOT** copy `.schema/` or `template/.plugin/` into a corpus here.
- A corpus's own configuration **MUST** stay in that corpus.
- The package `kac pack` writes **MUST NOT** contain another corpus's configuration.

#### Every value the build reads is committed (`std-CONFIG.every-value-the-build-reads-is-committed`)

obligations:
- A value deciding how this repository builds, validates or publishes **MUST** sit in a tracked file.
- A setting only a provider's console can store **MUST** be named in the header comment of the file it affects.
- A configuration file **MUST NOT** contain a key, a token or a password.
- A file a corpus keeps **MUST NOT** name a path from the machine that wrote it.
- A corpus's `.gitignore` **MUST** list `.dist/`, `.imports/` and `_reports/`, which the tool rebuilds whole.

#### The shape of a YAML file is yamllint's to decide (`std-CONFIG.the-shape-of-a-yaml-file-is-yamllints-to-decide`)

obligations:
- A YAML file **MUST** pass `yamllint --strict` under the `.yamllint` at this repository's root.
- `.yamllint` **MUST** extend yamllint's `default` ruleset.
- An override in `.yamllint` **MUST** give the reason it departs, in the comment above it.
- An `ignore:` **MUST** name the construct it exempts, in that same comment.
- `.editorconfig` **MUST** agree with `.yamllint` wherever both set the same thing.
- A change to a configuration file **MUST** take the review a change to code takes.

### The plugin carries the corpus, and a skill answers from what travelled (`std-PLUGIN`)

Status: active.
#### A hook runs in a POSIX shell, and pushes a view once a session (`std-PLUGIN.a-hook-runs-in-a-posix-shell-and-pushes-a-view-once-a-session`)

obligations:
- A hook **MUST** run under a POSIX shell.
- A hook **MUST NOT** require a parser, an interpreter or a runtime beyond that shell.
- What a hook prints **MUST** be rendered by `kac bundle`, so the hook computes nothing about the export.
- A hook **MUST** write only under `${CLAUDE_PLUGIN_DATA}`, or under the temporary directory where that is unset.
- A hook pushing a view **MUST** push it at most once a session.
- What a hook pushes **MUST** be at most 10,000 characters, because Claude Code shows a session only the first 2,000 of a longer one.
- Where a view is longer than that, the hook **MUST** push the view's path and counts in its place.
- What a hook pushes **SHOULD** state facts, because Claude Code passes it to the session as context.

#### A skill assumes only that the session can read a file (`std-PLUGIN.a-skill-assumes-only-that-the-session-can-read-a-file`)

obligations:
- A skill **MUST** leave the choice of search tool to the session.
- A skill **MUST** name the file or the directory containing the answer.
- A skill reviewing a change **MUST** name the change as everything that differs from a fixed point.
- A skill reviewing a change **MUST** leave the choice of how to read that change to the session.
- A skill **MUST NOT** require a shell, an interpreter or a runtime on the reader's machine, beyond what the session uses to read a change under review.
- A skill reading an export **MUST NOT** require a network call to answer from it.
- A skill reading an export **MUST NOT** ask for access beyond reading the files under `${CLAUDE_PLUGIN_ROOT}` and the change under review.
- Every path such a skill names **MUST** open on `${CLAUDE_PLUGIN_ROOT}`.
#### plugin.json declares every component and the types it reads (`std-PLUGIN.pluginjson-declares-every-component-and-the-types-it-reads`)

obligations:
- Every skill directory and hook directory in the plugin tree **MUST** appear under `metadata.components`.
- A component **MUST** name every record type it opens a file of, at the shape version it reads, as `glossary@1`: in `requires` where it cannot work without the type, and in `optional` where it can.
- A view **MUST** be declared as a component whose `view` names it and whose `path` is where `kac bundle` writes it.
- A component that needs a type present and opens none of its files **MUST** name that type bare.
- A component that reads no export at all **MUST** declare `requires` empty.
- A component that reads no export and supports no other component **MUST** declare `standalone` true.
- A component entry **MUST** have a `note` saying what it reads.
- `metadata.corpusRoot` **MUST** name the directory every skill in the tree addresses the export through.
- A component naming a type the export does not include **MUST** leave the plugin, with every file under its declared path.
- `bundle.json` **MUST** name every component that left, and the type that left it out.

#### The bundle carries a frozen export, and nothing writes to it (`std-PLUGIN.the-bundle-carries-a-frozen-export-and-nothing-writes-to-it`)

obligations:
- The export inside a plugin **MUST** be the bytes `kac export` wrote.
- A skill **MUST NOT** write to any file under `${CLAUDE_PLUGIN_ROOT}`.
- A skill **MUST** report a `status` its type does not treat as in force, under every such value that type declares.
- A skill **MUST** report a review date that has passed, where its type exports one.
- A skill **MUST** name that date under the key its type's export writes: `reviewBy` on a part line, and `review-by` from a record's frontmatter.
- A skill **MUST** quote `generatedAt` from `manifest.json` alongside either.
- A skill **MUST** quote the wording the export contains, and link the record for the rest.
- The words a skill quotes **MUST** come from the export, and never from a second copy kept for agents.
- A view **MUST** be generated by `kac bundle` from the export the plugin contains, and from nothing else.
- A fresh bundle of the same export **MUST** write every view again, byte for byte.
### Prose reads the same way whoever wrote it (`std-PROSE`)

Status: active.

#### Summary

Every word this repository publishes follows the Microsoft Writing Style Guide, with the changes below. That covers records, README files, code comments, schema descriptions, documentation pages, commit messages and pull request bodies. The writing skills under `.claude/skills/` state the same rules for an agent. Each surface then adds its own shape on top of them.

#### Commit messages and pull requests (`std-PROSE.commit-messages-and-pull-requests`)

obligations:
- A subject line **MUST** say what changed.
- A subject line **SHOULD** be in the imperative, with no full stop.
- The body **MUST** say why. It is the one place that may describe what used to be true.
- A pull request body **MUST** give the reason and the evidence.
- A pull request body **SHOULD NOT** retell the diff.

#### The base style (`std-PROSE.the-base-style`)

obligations:
- Prose **MUST** follow the Microsoft Writing Style Guide, except where a clause below changes it.
- A sentence **SHOULD** state the rule or the fact before the reason.
- An instruction **SHOULD** come before the reason for taking it.
- A purpose or a condition deciding whether to act **MUST** come before the instruction it governs.
- A verb **SHOULD** be literal. A command *lists*, *checks*, *writes*, *reports* or *rejects*.
- *Hold to*, *carry*, *name*, *answer*, *reach* and *seal* **SHOULD NOT** stand in for a plain verb. *Holds* has four plain replacements: *has*, *contains*, *keeps* and *checks*. *Carries* has *has*, *contains*, *includes* and *states*.
- A word on that list **MAY** appear where it is itself the plain verb. A heading *names* a topic, a standard *cites* a clause, and a service *returns* what a caller asked for.
- A sentence **SHOULD** state one idea.
- An instruction **SHOULD** give one step.
- A heading **MUST** name its topic, because a rule heading in a standard becomes its clause id.
- A heading **SHOULD** be in sentence case.
- Three or more parallel items **SHOULD** be a list or a table.

#### What prose must not change (`std-PROSE.what-prose-must-not-change`)

obligations:
- You **MUST** leave alone an identifier, a path, a flag, a command, and the output a command printed.
- You **MUST** leave alone a heading you may not rename, the H1 of a record, and every clause id.
- You **MAY** reword a clause where the obligation is identical: the same subject, the same modal, the same threshold, the same list, the same exception and the same cross-reference.
- Rewording a clause **MUST** reword every quotation of it in the same change. `clause-quoted-faithfully` reports a quotation that no longer matches.
- You **MUST** leave alone the span between a `BEGIN GENERATED` marker and its `END GENERATED`. To move it, change the schema or the frontmatter and run `kac generate`.
- Where this standard and `.schema/` disagree, the schema **MUST** win, and you **MUST** report the contradiction.
- You **MUST** name in your reply every rule you left behind on one of these grounds.

#### What stays true (`std-PROSE.what-stays-true`)

obligations:
- Prose **MUST** describe what exists today. Agreed but unbuilt work **MUST** go to the issue tracker.
- A schema rule the tool does not implement **MUST** be written as declared and not running.
- A count **MUST** name the command or the test that reports it, unless a decision fixes the set.
- Reasoning that lives elsewhere **SHOULD** be cited, not restated, unless the reader may not have the source.
- You **SHOULD** rewrite the whole block when you add a fact to existing prose.

#### Where this repository differs from Microsoft (`std-PROSE.where-this-repository-differs-from-microsoft`)

obligations:
- Spelling **MUST** be British.
- An em dash **MUST NOT** appear. A full stop, a comma or a colon replaces it.
- Prose **MUST** wrap at 120 columns, counted in characters. A table row and a link definition **MUST NOT** wrap.
- *We* **MUST** appear only in a commitment in a policy or a standard, because a reader takes it as one.
- Outside a commitment the text **SHOULD** say *you*, or name the tool, the framework or the corpus.
- A framework term (corpus, record, type, tier, layer, export, plugin) **SHOULD** have a gloss on its first use on each page, unless the page links the term's glossary entry.
- A figure of speech **SHOULD NOT** appear.
- A claim **SHOULD** give at most one reason.
- A contrast (*rather than*, *instead of*) **MUST** appear only where the reader already holds the wrong idea. A contrast inside a clause **MUST** stay, because it names what the clause forbids.
- One thing **MUST** have one name everywhere.

### A version stamp says what it versions, and a move says what changed (`std-VERS`)

Status: active.

#### Summary

The stamps on this repository's files decide what it publishes and what a corpus consuming it may take. Two are
semantic versions a person moves: `<Version>` in `tooling/kac/kac.csproj`, and `content-version` in a corpus's
`.corpus.yaml`. The rest are counts a tool writes and a reader compares for equality. A count moves where something
reading the file has to respond, so a reader that finds a count above its own stops there. Every consumer in this
repository takes a producer's move in the same pull request.

#### A producer's move obliges every consumer in the same pull request (`std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request`)

obligations:
- A consuming corpus **MUST** move the `resolved:` lock of its `consumes:` entry in the pull request moving the producer's `content-version`.
- That consumer **MUST** move its `version:` range as well wherever the producer is below 1.0.0 and its minor moved.
- A corpus here **MUST NOT** publish a `content-version` that a committed range in this repository refuses.
- You **SHOULD** delete `.imports/` in every consumer before you open the pull request, unless you leave the proof to the pull request's CI run.
- You **SHOULD** repack the producer after that delete, unless you leave the proof to the pull request's CI run.
- You **SHOULD** run `kac restore` again in every consumer, unless you leave the proof to the pull request's CI run.
- You **SHOULD** read what each of those restores reports.
- A `kac restore` over an `.imports/` you did not delete **MUST NOT** be offered as proof.

#### A published version is never replaced (`std-VERS.a-published-version-is-never-replaced`)

obligations:
- A published `<Version>` or `content-version` **MUST NOT** be pushed again, replaced or deleted.
- A correction **MUST** ship as a new version.
- The bytes a published version contains **MUST** stay the bytes it published.

#### A reader meeting a stamp it does not know stops (`std-VERS.a-reader-meeting-a-stamp-it-does-not-know-stops`)

obligations:
- `kac bundle` **MUST** refuse an export whose `formatVersion` is not the one that build reads.
- `kac bundle` **MUST** leave out a component whose type the export includes at a `shapeVersion` the component did not name.
- A reader that does not know one type's `shapeVersion` **MUST** leave that type alone and read the rest of the export.
- A `kac` older than `minimum-tool:` **MUST** stop on the manifest and name it.
- A `content-version` a reader does not know **MUST NOT** stop that reader.

#### A stamp is a semantic version, a count, or a range (`std-VERS.a-stamp-is-a-semantic-version-a-count-or-a-range`)

obligations:
- `content-version` in a corpus's `.corpus.yaml` and `<Version>` in `tooling/kac/kac.csproj` **MUST** be written and read under [Semantic Versioning 2.0.0].
- `descriptor-version`, `upstream.template-version`, `version:` in `manifest.yaml`, `version:` under `export:` in a `.schema/<type>.yaml`, `formatVersion`, `mechanismVersion` and a type's `shapeVersion` **MUST** each be a whole number that only increases.
- A count **MUST NOT** be read as a major, a minor or a patch.
- A key spelled `version:` **MUST** version the file or the block holding it.
- `minimum-tool:` in `manifest.yaml` **MUST** name a `<Version>` nuget.org has published.
- A `version:` under `consumes:` **MUST** be an exact version or a caret over one, and no other form.
- A caret **MUST NOT** be read as admitting a prerelease.

#### What a move of each stamp means (`std-VERS.what-a-move-of-each-stamp-means`)

obligations:
- `content-version` **MUST** be read as a statement about what the corpus publishes rather than about the file.
- What a corpus publishes **MUST** be read as its export, and never as the repository its records are readable in.
- A change confined to a section a record's type leaves out of `export:` **MUST NOT** move `content-version`.
- Its major **MUST** mark a meaning that changed or a published URL that broke, its minor a record or a rule added, and its patch a change of wording.
- `content-version` **MUST** move where a skill in the bundle changes what it tells a reader to do. `kac bundle` stamps the plugin with it, so a reader takes a changed skill only where it moves.
- A change to a field's accepted value format, its name, or its removal **MUST** move `content-version`'s major where the change reaches a record whose type names that field under `export:`, and its patch where it reaches none.
- A corpus below 1.0.0 **MUST** move its minor where the rule above asks for its major, because reaching 1.0.0 is a statement about that corpus's stability rather than about the change in front of you.
- A field added **MUST** move `content-version`'s minor where a record's type names it under `export:`, and its patch where none does.
- A move of `<Version>` **MUST** name a change to `kac` that a user can observe.
- `descriptor-version` **MUST** move where `.corpus.yaml` gains a key, loses one, or has one read differently.
- `version:` in `manifest.yaml` **MUST** move where a corpus has to respond: a file added, removed, renamed, or moved between layers.
- `upstream.template-version` **MUST** be the `version:` of the manifest that corpus last took.
- `formatVersion` and a type's `shapeVersion` **MUST** move where a reader written against the shape before it would now be wrong.
- Adding a key to a line, or a file to a type's directory, **MUST NOT** move `formatVersion` or a `shapeVersion`.
- `mechanismVersion` in an export's `manifest.json` **MUST** be that corpus's `upstream.template-version`, and **MUST** be null where the corpus states none.
- A rename of `mechanismVersion` **MUST** move `formatVersion`.

#### Which stamps move together, and which move alone (`std-VERS.which-stamps-move-together-and-which-move-alone`)

obligations:
- `content-version` and the `resolved:` lock of every consumer of that corpus **MUST** move in one pull request.
- `version:` in `manifest.yaml` and the `upstream.template-version` of every corpus here **MUST** move in one pull request.
- `version:` under `export:` in a `.schema/<type>.yaml` and the `shapeVersion` an export publishes for that type **MUST** be one number.
- `descriptor-version`, `formatVersion` and a `shapeVersion` **MUST NOT** oblige any other stamp to move.
- `<Version>` **MUST NOT** move because a corpus's records changed.
- A `content-version` **MUST NOT** move because `kac` changed.

