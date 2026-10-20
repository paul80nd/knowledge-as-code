# The rules a change is reviewed against, in the standards of example-dogfooding

Each rule below gives the id a finding cites, then its obligations, one to a bullet.

- **MUST** and **MUST NOT** state a requirement. A change that breaks one is a violation.
- **SHOULD** and **SHOULD NOT** state the usual way. A change that departs from one and gives its reason conforms, and the departure is a judgement call.
- **MAY** permits, and a change cannot break it.
- A standard is in force at status `active`. A finding under any other status is not in force.
- A standard's controls are listed with each one's mechanism and status. A control checks a rule only where its status is `active` and its mechanism is `ci` or `runtime-alert`, and then only the obligations named in its record's What it checks section. `corpus/controls/` inside this plugin has each control's record. A person checks every other obligation in the review.

## std-A11Y: The documentation site and kac are usable without sight, a mouse or colour

Status: active.

Controls: `ctl-0012` Nothing checks a page or the output for accessibility (`not-enforced`, active); `ctl-0014` A pinned component's assessment names the version it is pinned at (`ci`, active).

### A change that makes either surface worse needs a deviation

Cite as `std-A11Y.a-change-that-makes-either-surface-worse-needs-a-deviation`.

- A change knowingly reducing accessibility **MUST NOT** merge without a recorded deviation.
- The deviation **MUST** name the individual accepting the risk.
- The deviation **MUST** state what compensates for the loss.
- The deviation **MUST** state the date somebody looks at it again.
- The deviation **MUST** be a record in this corpus's deviations register.

### A Markdown author owns the structure of a page

Cite as `std-A11Y.a-markdown-author-owns-the-structure-of-a-page`.

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

### A pinned component carries an accessibility assessment

Cite as `std-A11Y.a-pinned-component-carries-an-accessibility-assessment`.

- A third-party component rendering either surface **MUST** have an assessment in its tool record.
- The assessment **MUST** name what was checked, and how.
- The assessment **MUST** name what falls short, or say that nothing found does.
- The assessment **MUST** name the version it was made against.
- A pull request moving a pin to a new major or minor version **MUST** include a new assessment.
- We **MUST NOT** adopt a component the assessment finds short without a recorded deviation.

### kac carries no meaning in colour alone

Cite as `std-A11Y.kac-carries-no-meaning-in-colour-alone`.

- Every message **MUST** state its meaning in words.
- Colour **MUST** repeat what the words already say.
- Every verb **MUST** honour `NO_COLOR` in the environment and `--no-color` on the command line.
- Output **MUST** stay complete when colour is off.
- Output **MUST NOT** separate one column from the next with a box-drawing character or an icon.
- Every answer a prompt asks for **MUST** also be available as a flag.
- A run with no terminal **MUST** exit rather than wait for a keystroke.
- A message a pipeline parses **MUST** go to `--json`, and never through a renderer.

### The theme owns contrast, focus and keyboard reach

Cite as `std-A11Y.the-theme-owns-contrast-focus-and-keyboard-reach`.

- `theme:` in `mkdocs.yml` **SHOULD** use the colour schemes Material for MkDocs ships, unless the change shows that its colours meet [WCAG 2.2 AA].
- `palette:` **SHOULD** offer a light scheme, a dark scheme and the reader's own setting.
- `docs/assets/extra.css` **SHOULD NOT** set a colour, a focus outline or a font size, unless the change shows that what it sets meets [WCAG 2.2 AA].
- The site **MUST NOT** include a template override or a script that changes the theme's keyboard behaviour.
- `features:` **MUST NOT** turn on an entry that hides content behind a pointer gesture.

### What falls short is written down where a reader finds it

Cite as `std-A11Y.what-falls-short-is-written-down-where-a-reader-finds-it`.

- A shortfall an assessment recorded **MUST** sit in the tool record for the component it was found in.
- A tool record **MUST NOT** claim a conformance level nobody verified.
- A correction **MUST** take the shortfall out of the record in the commit that corrects it.
- This project **MUST** publish an accessibility statement where a law or a contract requires one.
- A published statement **MUST** name every shortfall the tool records list.

## std-CI: Every check and every publish runs from a workflow in this repository

Status: active.

Controls: `ctl-0001` A merge waits for the validate job (`ci`, active); `ctl-0002` Every job in kac.yml sits behind the gate (`ci`, active); `ctl-0003` Every workflow passes actionlint (`ci`, active); `ctl-0006` A version carries the changelog section its release publishes (`ci`, active); `ctl-0011` Every corpus whose records changed has moved its content-version (`ci`, active).

### A job declares the permission it needs

Cite as `std-CI.a-job-declares-the-permission-it-needs`.

- A workflow **MUST** declare `permissions: contents: read` at its top level, which is what [OpenSSF Scorecard]'s `Token-Permissions` check asks for.
- A job needing wider access **MUST** declare that permission on itself.
- A job **MUST NOT** declare a permission no step in it uses.
- A workflow **MUST NOT** commit to a branch a person edits.

### A version moves by hand and publishes once

Cite as `std-CI.a-version-moves-by-hand-and-publishes-once`.

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

### A workflow holds no credential of its own

Cite as `std-CI.a-workflow-holds-no-credential-of-its-own`.

- A workflow **MUST** take every credential from GitHub's secret store or from an identity it exchanged.
- A publish to nuget.org **MUST** authenticate through a trusted publishing policy naming this repository, this workflow file and the `nuget.org` environment.
- A job **MUST** exchange its own identity for a short-lived key in the step before the one that uses it.
- A workflow calling GitHub's own API **MUST** take `github.token`.
- A step **MUST** pass a secret to a script through `env:`.
- A step **MUST NOT** interpolate a secret into a script body.
- A step **MUST NOT** print a secret.

### Every action is pinned to a commit

Cite as `std-CI.every-action-is-pinned-to-a-commit`.

- A step in `.github/workflows/` **MUST** pin its action to a commit SHA, which is what [OpenSSF Scorecard]'s `Pinned-Dependencies` check asks for.
- A pinned step **SHOULD** state the released version in a trailing comment, as `# v7.0.1`, because a reviewer reads the version there.
- `.github/dependabot.yml` **MUST** track `github-actions`.
- A job with a write permission **MUST NOT** install a tool at a moving version.
- A workflow needing a tool at `latest` **MUST** install it in a job with `contents: read`.

### One job holds the write permission

Cite as `std-CI.one-job-holds-the-write-permission`.

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

### The gate runs on every pull request into main

Cite as `std-CI.the-gate-runs-on-every-pull-request-into-main`.

- A check that can block a merge **MUST** run in `.github/workflows/kac.yml` on a pull request into `main`.
- `kac.yml` **MUST** name every one of its jobs in the `needs:` of its `validate` job.
- The branch rule on `main` **MUST** name `validate` as the check a merge waits for.
- A job **MUST NOT** declare `continue-on-error`.
- A workflow **MUST** pass `actionlint`, which also puts every `run:` block through shellcheck.
- A workflow file **MUST** arrive on `main` by the same reviewed merge as any other change.

## std-CONFIG: Configuration is committed, pinned and explained

Status: active.

Controls: `ctl-0004` Every YAML file passes yamllint (`ci`, active); `ctl-0005` Every corpus holds the file the template sends it (`ci`, active).

### A file says what it is for, and why each value is what it is

Cite as `std-CONFIG.a-file-says-what-it-is-for-and-why-each-value-is-what-it-is`.

- A configuration file **SHOULD** open with a comment saying what it is for, or with the `# yaml-language-server: $schema=` line naming what describes it.
- A comment **MUST** give the reason a value is what it is.
- A comment **SHOULD NOT** restate the value beside it.
- Where one line states the reason, a comment **SHOULD** run to one line.

### A pin names an exact version

Cite as `std-CONFIG.a-pin-names-an-exact-version`.

- A dependency **MUST** name one version, as a `PackageReference` does and as a line in `docs/requirements.txt` does.
- `.github/dependabot.yml` **MUST** name every directory containing a manifest that builds this repository, which is what [OpenSSF Scorecard]'s `Dependency-Update-Tool` check asks for.
- Every ecosystem it tracks **MUST** have a `schedule:`.
- Packages that only move as a set **SHOULD** be raised as one grouped update.
- A pin Dependabot cannot read **MUST** be named in that file's header comment, with what it pins today.

### A value living in more than one tree is copied and proved

Cite as `std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved`.

- A setting every project under `tooling/` takes **SHOULD** sit in `tooling/Directory.Build.props`.
- You **MUST** copy a file `manifest.yaml` names into `template/` and into each corpus under `examples/`.
- Every overlay copy **MUST** match its source, as `kac update --check --from ../../` reports in that corpus.
- You **MUST** read `.schema/` and `template/.plugin/` where they are authored.
- You **MUST NOT** copy `.schema/` or `template/.plugin/` into a corpus here.
- A corpus's own configuration **MUST** stay in that corpus.
- The package `kac pack` writes **MUST NOT** contain another corpus's configuration.

### Every value the build reads is committed

Cite as `std-CONFIG.every-value-the-build-reads-is-committed`.

- A value deciding how this repository builds, validates or publishes **MUST** sit in a tracked file.
- A setting only a provider's console can store **MUST** be named in the header comment of the file it affects.
- A configuration file **MUST NOT** contain a key, a token or a password.
- A file a corpus keeps **MUST NOT** name a path from the machine that wrote it.
- A corpus's `.gitignore` **MUST** list `.dist/`, `.imports/` and `_reports/`, which the tool rebuilds whole.

### The shape of a YAML file is yamllint's to decide

Cite as `std-CONFIG.the-shape-of-a-yaml-file-is-yamllints-to-decide`.

- A YAML file **MUST** pass `yamllint --strict` under the `.yamllint` at this repository's root.
- `.yamllint` **MUST** extend yamllint's `default` ruleset.
- An override in `.yamllint` **MUST** give the reason it departs, in the comment above it.
- An `ignore:` **MUST** name the construct it exempts, in that same comment.
- `.editorconfig` **MUST** agree with `.yamllint` wherever both set the same thing.
- A change to a configuration file **MUST** take the review a change to code takes.

## std-PLUGIN: The plugin carries the corpus, and a skill answers from what travelled

Status: active.

Controls: `ctl-0008` Every corpus publishes, installs and is read back (`ci`, active); `ctl-0009` Three test layers run before a merge (`ci`, active).

### A deviation request an agent files asks for the owner it cannot name

Cite as `std-PLUGIN.a-deviation-request-an-agent-files-asks-for-the-owner-it-cannot-name`.

- A deviation request **MUST** open its body with a fenced block whose info string is `yaml kac-deviation`.
- That block **MUST** open on `corpus`, naming the corpus that will contain the record.
- The block **MUST** then contain `id`, `status`, `departs-from`, `risk` and `review-by`, under the names and in the order the `deviations` template writes them.
- The block **MAY** contain `applies-to` and `tags` after those.
- The block **MUST NOT** contain any other key.
- `status` **MUST** be `draft`.
- Every entry in `departs-from` **MUST** name a clause, scoped exactly as the export writes that record's id.
- An entry **MUST NOT** name a whole policy or standard.
- A skill **MUST NOT** write `owner` or `accepted-on`, because the individual who accepts the risk is what the request asks for.
- A request **MUST NOT** describe the departure as permanent, indefinite, or standing until further notice.
- The body **MUST** contain `## What we are doing instead`, `## Why we need it`, `## What the risk is`, `## What compensates` and `## How it closes`, in that order.
- `## Who is asking` **MUST** follow those five.
- `## Who is asking` **MUST** name the agent, the session it ran in, the repository, the commit it read, and what it was doing.
- Where the agent cannot reach one of those, `## Who is asking` **MUST** name it as unreached.
- The title **SHOULD** be the departure in one line, naming the work and the rule it breaks.
- A skill **MUST** file the request on the backlog addressed under `tracker` by the corpus that owns the clause.
- A skill **MUST NOT** file a request on a repository outside the organisation holding the plugin.
- A skill **MUST** mark the issue `kac:deviation`, using whatever the platform calls a label.
- Where the platform refuses a mark it does not already have, a skill **MUST** file the request unmarked.
- A skill **MUST** say that the request accepts nothing, and that the record is still owed.

### A finding an agent files is an observation, and proposes no record

Cite as `std-PLUGIN.a-finding-an-agent-files-is-an-observation-and-proposes-no-record`.

- A finding **MUST** open its body with a fenced block whose info string is `yaml kac-finding`.
- That block **MUST** open on `corpus`, naming the corpus the finding is about.
- The block **MUST** then contain `source`, `confidence`, `looks-like` and `provenance`, in that order.
- The block **MAY** contain `applies-to` and `tags` after those.
- The block **MUST NOT** contain any other key, and **MUST NOT** propose an id for a record.
- `source` **MUST** be `session`.
- `confidence` **MUST** be `unverified` where the filing session's own account is the only evidence.
- `looks-like` **MUST** name a type the export declares, `framework`, or `none`.
- `looks-like` **MUST** be `framework` where the finding reports the framework rather than a record.
- `provenance` **MUST** name the agent, the session it ran in, the repository, the commit it read, and what it was doing.
- Where the agent cannot reach one of those, `provenance` **MUST** name it as unreached.
- The body **MUST** contain `## What I saw`, `## Context` and `## Why it might matter`, in that order.
- A finding **MUST NOT** put anything between the block and the first of those headings.
- The title **SHOULD** be the observation in one line.
- A skill **MUST** mark the issue `kac:finding`, using whatever the platform calls a label.
- Where the platform refuses a mark it does not already have, a skill **MUST** file the finding unmarked.
- A skill **MUST** say that it filed one unmarked.

### A framework finding travels stripped, and only by hand

Cite as `std-PLUGIN.a-framework-finding-travels-stripped-and-only-by-hand`.

- A skill **MUST** compare the framework block's address with the tracker block's, both targets and both bases normalised.
- Where the two are equal, a skill **MUST** mark the finding and copy nothing.
- Where the two differ, a skill **MUST** post the upstream body as a comment on the issue and stop.
- A skill **MUST NOT** file a finding on a tracker outside the organisation holding the corpus.
- The upstream body **MUST NOT** contain the session id, the repository the session worked in, that repository's commit, or the name of the corpus the finding was filed from.
- The upstream body **MUST** state the version of the framework the session ran.

### A hook runs in a POSIX shell, and pushes a view once a session

Cite as `std-PLUGIN.a-hook-runs-in-a-posix-shell-and-pushes-a-view-once-a-session`.

- A hook **MUST** run under a POSIX shell.
- A hook **MUST NOT** require a parser, an interpreter or a runtime beyond that shell.
- What a hook prints **MUST** be rendered by `kac bundle`, so the hook computes nothing about the export.
- A hook **MUST** write only under `${CLAUDE_PLUGIN_DATA}`, or under the temporary directory where that is unset.
- A hook pushing a view **MUST** push it at most once a session.
- What a hook pushes **MUST** be at most 10,000 characters, because Claude Code shows a session only the first 2,000 of a longer one.
- Where a view is longer than that, the hook **MUST** push the view's path and counts in its place.
- What a hook pushes **SHOULD** state facts, because Claude Code passes it to the session as context.

### A skill assumes only that the session can read a file

Cite as `std-PLUGIN.a-skill-assumes-only-that-the-session-can-read-a-file`.

- A skill **MUST** leave the choice of search tool to the session.
- A skill **MUST** name the file or the directory containing the answer.
- A skill reviewing a change **MUST** name the change as everything that differs from a fixed point.
- A skill reviewing a change **MUST** leave the choice of how to read that change to the session.
- A skill **MUST NOT** require a shell, an interpreter or a runtime on the reader's machine, beyond what the session uses to read a change under review.
- A skill reading an export **MUST NOT** require a network call to answer from it.
- A skill reading an export **MUST NOT** ask for access beyond reading the files under `${CLAUDE_PLUGIN_ROOT}` and the change under review.
- Every path such a skill names **MUST** open on `${CLAUDE_PLUGIN_ROOT}`.

### A skill names the type of every field it describes

Cite as `std-PLUGIN.a-skill-names-the-type-of-every-field-it-describes`.

- A skill describing a field **MUST** name that field's JSON type.
- A skill **MUST** say that a field with no value is present and `null`, where the export writes it that way.
- A skill **MUST** name each key that is absent rather than `null`.
- A skill **MUST NOT** describe a string of markdown as a list.
- Every field description **MUST** agree with a line of a real export.
- A skill **MUST** name the parts file of the type its component declares, and no other type's.

### A skill reads a rule's keyword as what departing from it costs

Cite as `std-PLUGIN.a-skill-reads-a-rules-keyword-as-what-departing-from-it-costs`.

- A skill reporting a rule **MUST** say that departing from a `MUST` or `MUST NOT` takes a deviation the rule's owner accepts.
- Where the bundle carries `request-deviation`, a skill **MUST** send that departure to it.
- A skill reporting a rule **MUST** say that a reader may depart from a `SHOULD` or `SHOULD NOT` by giving the reason in the change.
- A skill **MUST NOT** send a departure from a `SHOULD`, `SHOULD NOT`, `MAY` or `COULD` to `request-deviation`, because a reason given in the change conforms to the rule.
- A skill reviewing a change **MUST** report a breach of a `MUST` or `MUST NOT` as a violation, citing the rule's id.
- A skill reviewing a change **MUST** report a departure from a `SHOULD` or `SHOULD NOT` as a judgement call, citing the rule's id and quoting the reason the change gives.
- A skill reviewing a change **MUST** check every obligation itself, except one named in the What it checks section of an `active` control whose mechanism is `ci` or `runtime-alert`.

### A skill says what its corpus did not carry

Cite as `std-PLUGIN.a-skill-says-what-its-corpus-did-not-carry`.

- A skill **MUST** read `types` in `manifest.json` before it says what is missing.
- A skill **MUST** name what stayed behind, and send the reader to the published record for it.
- A skill **MUST NOT** name a fixed set of absences.
- A skill **MUST** name the corpus and every entry in `sources` it searched, where nothing matched.
- A skill **MUST** say that an empty search states only that the estate has not written this down.
- A skill **MUST NOT** read an obligation out of a record about something else.

### A skill states what it is for, and the reason for each rule it gives

Cite as `std-PLUGIN.a-skill-states-what-it-is-for-and-the-reason-for-each-rule-it-gives`.

- A skill **SHOULD** state what it is for, the intent of the type it reads, and the reason behind each rule it gives.
- A skill **SHOULD** set out steps only where the order is the content: a record of a type an agent follows, or a write-back or review whose order a rule fixes.
- A skill's description **SHOULD** name each case that triggers it, and each case once.

### A skill writes back to the tracker the corpus states

Cite as `std-PLUGIN.a-skill-writes-back-to-the-tracker-the-corpus-states`.

- A skill **MUST** send a correction to the backlog the record's corpus addresses under `tracker`.
- A skill **MUST NOT** file a correction on the address a `publishing` block states, because a published form and a backlog are one address on GitHub and two on Azure DevOps, where the project holds the backlog and the repository inside it holds none.
- A skill **MUST** file the item under the area path that `tracker` block states.
- Where that area path is refused, a skill **MUST** file the item at the project default and say so.
- A skill **MUST** raise it as an issue or a work item.
- A skill **SHOULD** use the client that already authenticates to that platform.
- A skill **MUST NOT** offer to edit the installed copy.
- A skill **MUST NOT** ask the reader for a credential.

### plugin.json declares every component and the types it reads

Cite as `std-PLUGIN.pluginjson-declares-every-component-and-the-types-it-reads`.

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

### The bundle carries a frozen export, and nothing writes to it

Cite as `std-PLUGIN.the-bundle-carries-a-frozen-export-and-nothing-writes-to-it`.

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

### Triage routes a finding with a label, and says why in a comment

Cite as `std-PLUGIN.triage-routes-a-finding-with-a-label-and-says-why-in-a-comment`.

- A skill triaging findings **MUST** travel into a corpus's working tree, and **MUST NOT** be a component of a plugin.
- A skill triaging findings **MUST** read the corpus it is about from the `.corpus.yaml` it walks up to.
- A skill triaging findings **MUST NOT** decide which corpus it is in from the `publishing` block, because a corpus published as a wiki or a site has no repository there and its maintainer still has both a tracker and the source.
- A skill **MUST** select its queue as `kac:finding` without `kac:triaged`.
- A skill **MUST** also find an open issue whose body has the `yaml kac-finding` block and no `kac:finding` mark.
- A skill **MUST** say where a query returned as many issues as its limit allows.
- A skill **MUST NOT** take `kac:finding` off an issue it triaged.
- A skill **MUST** apply `kac:triaged` and exactly one route mark to every issue it triaged.
- The route **MUST** be one of `kac:route-framework`, `kac:route-record`, `kac:route-misfiled`, `kac:route-none` and `kac:route-unclear`.
- A skill **MUST** apply `kac:route-unclear` wherever choosing between the others would be a guess.
- A skill **MUST** create every mark it applies before it applies one, on a platform that refuses an unknown mark.
- A skill **MUST** comment the reason for the verdict on the issue it marked.
- A comment routing a duplicate **MUST** name the issue it duplicates.
- A comment routing a finding a record already answers **MUST** name that record.
- The comment **MUST NOT** state the route as machine-readable data, because the mark is that data.
- A skill **MUST NOT** state whether the finding's claim is true.
- A skill **MUST** show every verdict to a person and wait, before it writes any mark or comment.
- A skill drafting a record from a finding **MUST** take it only from an issue marked `kac:route-record`.
- That record **MUST** name an `owner` the person running the skill supplied, and the skill **MUST NOT** invent one.
- That record **MUST** carry a `status` its type treats as unsettled, wherever the type declares one.
- That record **MUST** arrive as a pull request, and the skill **MUST NOT** approve or merge it.
- That record **MUST** name the issue it was drafted from in `sources`, or in its changelog entry where its type refines `sources` into something else.
- Where the type takes neither, the pull request body **MUST** state that trace.
- That pull request **MUST** name the record's id.
- That pull request **MUST** link the issue, so that merging it settles the finding.
- A skill **MUST NOT** close the issue itself.
- A skill **MUST NOT** draft a record from an issue already closed, or from one a pull request already answers.

## std-PROSE: Prose reads the same way whoever wrote it

Status: active.

Controls: `ctl-0013` Nothing checks prose against the house style (`not-enforced`, active).

### Commit messages and pull requests

Cite as `std-PROSE.commit-messages-and-pull-requests`.

- A subject line **MUST** say what changed.
- A subject line **SHOULD** be in the imperative, with no full stop.
- The body **MUST** say why. It is the one place that may describe what used to be true.
- A pull request body **MUST** give the reason and the evidence.
- A pull request body **SHOULD NOT** retell the diff.

### The base style

Cite as `std-PROSE.the-base-style`.

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

### What prose must not change

Cite as `std-PROSE.what-prose-must-not-change`.

- You **MUST** leave alone an identifier, a path, a flag, a command, and the output a command printed.
- You **MUST** leave alone a heading you may not rename, the H1 of a record, and every clause id.
- You **MAY** reword a clause where the obligation is identical: the same subject, the same modal, the same threshold, the same list, the same exception and the same cross-reference.
- Rewording a clause **MUST** reword every quotation of it in the same change. `clause-quoted-faithfully` reports a quotation that no longer matches.
- You **MUST** leave alone the span between a `BEGIN GENERATED` marker and its `END GENERATED`. To move it, change the schema or the frontmatter and run `kac generate`.
- Where this standard and `.schema/` disagree, the schema **MUST** win, and you **MUST** report the contradiction.
- You **MUST** name in your reply every rule you left behind on one of these grounds.

### What stays true

Cite as `std-PROSE.what-stays-true`.

- Prose **MUST** describe what exists today. Agreed but unbuilt work **MUST** go to the issue tracker.
- A schema rule the tool does not implement **MUST** be written as declared and not running.
- A count **MUST** name the command or the test that reports it, unless a decision fixes the set.
- Reasoning that lives elsewhere **SHOULD** be cited, not restated, unless the reader may not have the source.
- You **SHOULD** rewrite the whole block when you add a fact to existing prose.

### Where this repository differs from Microsoft

Cite as `std-PROSE.where-this-repository-differs-from-microsoft`.

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

## std-VERS: A version stamp says what it versions, and a move says what changed

Status: active.

Controls: `ctl-0007` Every corpus validates and its generated output is fresh (`ci`, active).

### A producer's move obliges every consumer in the same pull request

Cite as `std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request`.

- A consuming corpus **MUST** move the `resolved:` lock of its `consumes:` entry in the pull request moving the producer's `content-version`.
- That consumer **MUST** move its `version:` range as well wherever the producer is below 1.0.0 and its minor moved.
- A corpus here **MUST NOT** publish a `content-version` that a committed range in this repository refuses.
- You **SHOULD** delete `.imports/` in every consumer before you open the pull request, unless you leave the proof to the pull request's CI run.
- You **SHOULD** repack the producer after that delete, unless you leave the proof to the pull request's CI run.
- You **SHOULD** run `kac restore` again in every consumer, unless you leave the proof to the pull request's CI run.
- You **SHOULD** read what each of those restores reports.
- A `kac restore` over an `.imports/` you did not delete **MUST NOT** be offered as proof.

### A published version is never replaced

Cite as `std-VERS.a-published-version-is-never-replaced`.

- A published `<Version>` or `content-version` **MUST NOT** be pushed again, replaced or deleted.
- A correction **MUST** ship as a new version.
- The bytes a published version contains **MUST** stay the bytes it published.

### A reader meeting a stamp it does not know stops

Cite as `std-VERS.a-reader-meeting-a-stamp-it-does-not-know-stops`.

- `kac bundle` **MUST** refuse an export whose `formatVersion` is not the one that build reads.
- `kac bundle` **MUST** leave out a component whose type the export includes at a `shapeVersion` the component did not name.
- A reader that does not know one type's `shapeVersion` **MUST** leave that type alone and read the rest of the export.
- A `kac` older than `minimum-tool:` **MUST** stop on the manifest and name it.
- A `content-version` a reader does not know **MUST NOT** stop that reader.

### A stamp is a semantic version, a count, or a range

Cite as `std-VERS.a-stamp-is-a-semantic-version-a-count-or-a-range`.

- `content-version` in a corpus's `.corpus.yaml` and `<Version>` in `tooling/kac/kac.csproj` **MUST** be written and read under [Semantic Versioning 2.0.0].
- `descriptor-version`, `upstream.template-version`, `version:` in `manifest.yaml`, `version:` under `export:` in a `.schema/<type>.yaml`, `formatVersion`, `mechanismVersion` and a type's `shapeVersion` **MUST** each be a whole number that only increases.
- A count **MUST NOT** be read as a major, a minor or a patch.
- A key spelled `version:` **MUST** version the file or the block holding it.
- `minimum-tool:` in `manifest.yaml` **MUST** name a `<Version>` nuget.org has published.
- A `version:` under `consumes:` **MUST** be an exact version or a caret over one, and no other form.
- A caret **MUST NOT** be read as admitting a prerelease.

### What a move of each stamp means

Cite as `std-VERS.what-a-move-of-each-stamp-means`.

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

### Which stamps move together, and which move alone

Cite as `std-VERS.which-stamps-move-together-and-which-move-alone`.

- `content-version` and the `resolved:` lock of every consumer of that corpus **MUST** move in one pull request.
- `version:` in `manifest.yaml` and the `upstream.template-version` of every corpus here **MUST** move in one pull request.
- `version:` under `export:` in a `.schema/<type>.yaml` and the `shapeVersion` an export publishes for that type **MUST** be one number.
- `descriptor-version`, `formatVersion` and a `shapeVersion` **MUST NOT** oblige any other stamp to move.
- `<Version>` **MUST NOT** move because a corpus's records changed.
- A `content-version` **MUST NOT** move because `kac` changed.

## eng:std-API: An interface is described by a contract in the repository

Status: draft.

Controls: `eng:ctl-0005` Nothing checks that an interface change carries a version and a notice (`not-enforced`, active).

### A change carries a version and a notice

Cite as `eng:std-API.a-change-carries-a-version-and-a-notice`.

- A published interface **MUST** have a version a consumer can pin to.
- An HTTP interface **SHOULD** put its major version in the path, as `/v2/`, unless the contract states another scheme.
- A breaking change **MUST** ship as a new version, with the previous version still serving requests.
- A team **MUST** give notice before it removes a version, on the timescale the deprecation approach states.
- A deprecated version **SHOULD** respond with a `Deprecation` header and a `Sunset` header, so a consumer sees the notice in the response.

### Every endpoint authenticates and validates

Cite as `eng:std-API.every-endpoint-authenticates-and-validates`.

- A contract **MUST** state the authentication each endpoint takes.
- A contract **MUST** declare an endpoint that takes no authentication as public.
- An endpoint **MUST** authenticate the caller before it does any work, unless the contract declares it public.
- An endpoint **MUST** authorise the operation before it does any work, unless the contract declares it public.
- An endpoint **MUST** validate each field it receives against the contract, and reject a request that does not match.
- An interface **MUST NOT** serve personal data, a secret, or an operation that changes state, to an unauthenticated caller.

### The contract is the source of truth

Cite as `eng:std-API.the-contract-is-the-source-of-truth`.

- A service **MUST** publish a machine-readable contract for every interface it exposes, committed to the repository that builds the service.
- Where the implementation and the contract disagree, a team **MUST** treat the implementation as the defect.
- A build **MUST** fail where the implementation no longer matches the contract.
- A consumer **SHOULD** test its use of an interface against the published contract, so the test fails on what the provider promised.
- A consumer **MAY** also test against a running instance of the provider.

## eng:std-CONT: A container image is pinned by digest and runs as a non-root user

Status: draft.

Controls: `eng:ctl-0001` Nothing checks that an image is named by digest (`not-enforced`, active).

### A published tag never moves

Cite as `eng:std-CONT.a-published-tag-never-moves`.

- A pipeline **MUST** tag an image with the build's own version.
- A registry **MUST** refuse a second push to a tag that already exists.
- A deployment **MUST** name an image by digest, so the running container is the one the pipeline built.

### The base image is chosen and pinned

Cite as `eng:std-CONT.the-base-image-is-chosen-and-pinned`.

- A Dockerfile **MUST** take its base image from the organisation's registry.
- A Dockerfile **MUST** pin the base image by digest rather than by a moving tag.
- A repository **MUST** rebuild against a refreshed base at least monthly, so a patched base is deployed to the running service.

### The container holds only what the service needs

Cite as `eng:std-CONT.the-container-holds-only-what-the-service-needs`.

- An image **MUST** declare a non-root `USER`.
- A container **MUST** run as that user.
- A container **MUST** run with a read-only root filesystem, writing only to a declared volume.
- An image **MUST** be built from a runtime base rather than an SDK base, so no compiler ships to production.
- An image **MUST NOT** expose a management, debug or metrics port to anything outside the cluster.

## eng:std-CSSTY: C# follows the runtime team's conventions, and the compiler enforces them

Status: draft.

Controls: `eng:ctl-0009` Nothing checks that a suppression is local and justified (`not-enforced`, active).

### A suppression is local and says why

Cite as `eng:std-CSSTY.a-suppression-is-local-and-says-why`.

- A suppression **MUST** sit on the member it applies to, with a `Justification` giving the reason.
- A repository **MUST NOT** disable a rule in `.editorconfig` to clear a warning in one file.

### The conventions come from one file

Cite as `eng:std-CSSTY.the-conventions-come-from-one-file`.

- A repository holding C# **MUST** have an `.editorconfig` at its root.
- That `.editorconfig` **SHOULD** be the .NET runtime team's own.
- A repository **SHOULD** state each departure from the runtime team's file in a comment beside it.
- A project **MUST** set `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild`, so the build checks the style.
- A project **MUST** enable nullable reference types.
- Code **MUST NOT** silence a nullability warning with `!`, except in a test asserting that a null argument is refused.
- A project **MUST** enable the .NET analysers at the `latest` analysis level.

### The security rules the analysers cover

Cite as `eng:std-CSSTY.the-security-rules-the-analysers-cover`.

- Code **MUST** build a SQL command through a parameter rather than by joining strings.
- Code **MUST** use the framework's cryptography.
- Code **MUST NOT** call an algorithm the analysers report as broken.
- Code **MUST** compare a secret with `CryptographicOperations.FixedTimeEquals` rather than with `==`.

## eng:std-DEPLOY: One artefact is built once and promoted to production

Status: draft.

Controls: `eng:ctl-0002` Nothing checks that a running service reports its commit (`not-enforced`, active).

### A way back exists before the change goes

Cite as `eng:std-DEPLOY.a-way-back-exists-before-the-change-goes`.

- A release **MUST** have a stated rollback or recovery path before it starts.
- A database migration **MUST** leave the previous version of the service able to run against the new schema, unless the migration removes data the previous version reads.

### Configuration sits outside the artefact

Cite as `eng:std-DEPLOY.configuration-sits-outside-the-artefact`.

- A service **MUST** read environment-specific configuration at start-up, from outside the artefact.
- An artefact **MUST NOT** contain a hostname, connection string or feature setting that differs between environments.

### Production changes through the pipeline alone

Cite as `eng:std-DEPLOY.production-changes-through-the-pipeline-alone`.

- A change **MUST** reach production through the automated pipeline.
- A change **MUST** reach each environment below production by promotion from the one before it.
- Every environment **MUST** be provisioned from the same definition, parameterised per environment.
- The pipeline definition **MUST** be committed to the repository, and reviewed like the code.
- A team **MUST NOT** hand-edit production code, configuration, infrastructure or schema, other than a flag change made under [pol-PIPE].FLAGS.
- A team changing production by hand to restore service, where the pipeline is unavailable or its delay would extend an outage, **MUST** record a deviation at the time.
- A team **MUST** commit a hand change made to restore service to the repository before the incident closes.

### The artefact is built once

Cite as `eng:std-DEPLOY.the-artefact-is-built-once`.

- A pipeline **MUST** build the artefact once, and promote that build to every environment.
- An artefact **MUST** record the commit it was built from, so a running version identifies the change it came from.
- A deployment record **MUST** name the artefact, the change and the approval behind it.
- A pipeline **MUST NOT** deploy an artefact built anywhere but the pipeline.

## eng:std-DEPS: A dependency is pinned, screened and named in the inventory

Status: draft.

Controls: none.

### What happens afterwards

Cite as `eng:std-DEPS.what-happens-afterwards`.

- A pipeline **MUST** scan the dependency tree on every build and on a weekly schedule.
- A team **MUST** rank a finding by its severity and by whether the affected path runs.
- A team **MUST** close a critical finding within 7 days and a high finding within 14.
- A team **MUST NOT** release with an open critical finding, absent a recorded deviation naming who accepts the risk.

### What happens before adoption

Cite as `eng:std-DEPS.what-happens-before-adoption`.

- Somebody **MUST** screen a new package for known vulnerabilities before the pull request that adds it merges.
- Somebody **MUST** check a new package's licence against the allowed list before that pull request merges.
- A pull request adding a package **MUST** say what the package is for, so a reviewer can weigh it against writing the code.

### Where a package comes from

Cite as `eng:std-DEPS.where-a-package-comes-from`.

- A build **MUST** resolve packages from the organisation's feed, which proxies the public registries.
- A repository **MUST** commit a lockfile pinning every direct and transitive dependency to a version.
- A build **MUST** install from the lockfile, and fail where the lockfile and the manifest disagree.
- A build **MUST** publish its artefacts to the managed repository, versioned and retained.
- A team **MUST NOT** add a package from a personal feed, a git URL or an archive downloaded by hand.

## eng:std-GATES: A failing check blocks the merge

Status: draft.

Controls: `ctl-0001` A merge waits for the validate job (`ci`, active); `ctl-0007` Every corpus validates and its generated output is fresh (`ci`, active); `ctl-0008` Every corpus publishes, installs and is read back (`ci`, active); `ctl-0009` Three test layers run before a merge (`ci`, active); `ctl-0010` The documentation site builds with no dead link (`ci`, active); `eng:ctl-0004` Nothing checks that a skipped test cites a deviation (`not-enforced`, active).

### A failing check stops the change

Cite as `eng:std-GATES.a-failing-check-stops-the-change`.

- Branch policy **MUST** block a merge while any required check is failing.
- A team **MUST NOT** merge over a failing check without a recorded deviation.
- A team **MUST NOT** skip, silence or suppress a check without a recorded deviation.
- A deviation covering a suppressed check **MUST** name the individual who accepts the risk.
- That deviation **MUST** have a review date.

### Every change is built and tested automatically

Cite as `eng:std-GATES.every-change-is-built-and-tested-automatically`.

- A push to any branch **MUST** trigger a build and the test suite, without anyone asking for it.
- The build **MUST** run from a clean checkout, on an agent that is not a person's own machine.
- The agent **SHOULD** be provisioned from a definition in the repository, because a change to it is then reviewed like the code.
- A fix for a defect **MUST** arrive with a test that fails without it.

## eng:std-INCSEV: Every incident is handled at a stated severity

Status: draft.

Controls: none.

### A significant incident is reviewed

Cite as `eng:std-INCSEV.a-significant-incident-is-reviewed`.

- A sev1 or sev2 incident **MUST** be treated as significant wherever a policy here names one.
- A sev1 or sev2 incident **MUST** have a postmortem.
- A personal data breach **MUST** have a postmortem, whatever its severity.

### Every incident has a severity

Cite as `eng:std-INCSEV.every-incident-has-a-severity`.

- Whoever declares an incident **MUST** set its severity at declaration.
- An incident **MUST** be sev1 where a `critical` service is unavailable, or where customers' money has been taken, refunded or paid out wrongly, or their personal data is wrong or exposed, at any scale.
- An incident **MUST** be sev2 where a service is degraded or a fault reaches some customers, and a workaround exists or the harm is contained.
- An incident **MUST** be sev3 where no customer is affected, or one customer is affected with no lasting harm.
- An incident meeting none of these **MUST** be sev2 until the incident lead sets another level.

### The severity moves with what is known

Cite as `eng:std-INCSEV.the-severity-moves-with-what-is-known`.

- The incident lead **MUST** raise the severity as soon as the incident meets a higher level.
- The incident lead **SHOULD** lower the severity once the incident no longer meets its level, because a sev1 response held open costs the people paged for it.
- A lowered severity **MUST** have its reason recorded in the incident record.
- A postmortem **MUST** state the highest severity the incident reached.

### The severity sets the response

Cite as `eng:std-INCSEV.the-severity-sets-the-response`.

- A sev1 or sev2 **MUST** have a named incident lead.
- A sev1 **MUST** page the on-call engineer as soon as it is declared.
- A sev1 **MUST** tell the people it affects its status at least every 30 minutes.
- A sev2 **MUST** page the on-call engineer.
- A sev2 **MUST** tell the people it affects its status at least every 2 hours.
- A sev3 **MAY** be handled as routine work, in working hours.

## eng:std-NETTST: A .NET test project runs under one framework, and an integration test owns its dependencies

Status: draft.

Controls: `eng:ctl-0008` Nothing checks that a .NET build collects coverage (`not-enforced`, active).

### A unit test crosses no boundary

Cite as `eng:std-NETTST.a-unit-test-crosses-no-boundary`.

- A unit test **MUST NOT** open a socket, read the filesystem, or start a container.
- A test **MUST NOT** call `Thread.Sleep` or `Task.Delay` to wait for something.
- A test **SHOULD** take its time from an injected clock, such as `TimeProvider`.
- A test **MUST** await every asynchronous call, so the test that caused a failure is the test that reports it.

### An integration test starts what it needs

Cite as `eng:std-NETTST.an-integration-test-starts-what-it-needs`.

- An integration test **MUST** start the dependencies it needs.
- An integration test **MUST NOT** share an instance of a dependency with another run.
- An integration test **SHOULD** start those dependencies with Testcontainers.
- An integration test for an ASP.NET service **SHOULD** host it in the test process with `WebApplicationFactory`.
- An integration test **MUST** leave no state behind, so the suite passes when it runs twice.

### Coverage is collected by the build

Cite as `eng:std-NETTST.coverage-is-collected-by-the-build`.

- A build **MUST** collect coverage from its test run and publish the report.
- A build **SHOULD** collect it with `dotnet test --collect:"XPlat Code Coverage"`, which writes a Cobertura report.
- A test project **MUST** be excluded from its own coverage figure.

### The framework and the project layout

Cite as `eng:std-NETTST.the-framework-and-the-project-layout`.

- A repository **MUST** use one test framework across its test projects.
- Every test project **MUST** run under `dotnet test`.
- A test project **SHOULD** use xUnit, because the estate's examples and controls are written against it.
- A test project **MUST** be named for the project it tests, with a `.Tests` or `.IntegrationTests` suffix.
- A project **MUST** use one assertion style throughout its tests.
- A project **SHOULD** assert with the test framework's own `Assert`.
- A project **MAY** assert with AwesomeAssertions, or with another library whose licence is on the allowed list [std-DEPS.what-happens-before-adoption] checks a new package against.

## eng:std-OBS: A service ships its telemetry to one place, keyed by one id

Status: draft.

Controls: `eng:ctl-0003` Nothing checks that an alert names an owner and a first action (`not-enforced`, active).

### Everything lands in the central store

Cite as `eng:std-OBS.everything-lands-in-the-central-store`.

- A service **MUST** emit its logs, traces and metrics to the central platform.
- A service **MUST NOT** keep the only copy of a log, so that losing the host still leaves the evidence.
- A service **MUST NOT** go into production without health monitoring and at least one alert on it.

### One request reads as one timeline

Cite as `eng:std-OBS.one-request-reads-as-one-timeline`.

- A service **MUST** stamp every record with a timestamp taken from a synchronised clock, stating its offset from UTC.
- A service **SHOULD** write that timestamp in UTC, because records from several services then sort without conversion.
- A service **MUST** accept an inbound `traceparent`.
- A service **MUST** pass that `traceparent` to everything it calls.
- A log line **MUST** include the trace id, so a search on one request returns every service that handled it.
- A service **MUST** emit structured fields, not a formatted sentence, so a search can filter on a value.

### Somebody acts on every alert

Cite as `eng:std-OBS.somebody-acts-on-every-alert`.

- A service **MUST** publish availability and latency objectives.
- A service **MUST** alert when it is on course to miss one of those objectives.
- An alert **MUST** name the accountable owner and the first action to take.
- A team **MUST** delete or fix an alert nobody acts on, and not leave it to be filtered.
- A team **MUST** retain telemetry for the period the repository records against the service.

### Telemetry carries no personal data

Cite as `eng:std-OBS.telemetry-carries-no-personal-data`.

- A service **MUST** redact unmasked personal data before the record is written.
- A service **MUST** keep every secret, credential and token out of a log line, a trace attribute and a metric label. [std-SECRET.nowhere-else-holds-a-secret] states where a secret may appear.

## eng:std-PR: A pull request carries its reasoning and one accountable approval

Status: draft.

Controls: `eng:ctl-0007` Nothing checks that a pull request names the agent that produced it (`not-enforced`, active).

### A change arrives as a pull request

Cite as `eng:std-PR.a-change-arrives-as-a-pull-request`.

- A change **MUST** arrive on the default branch as a merged pull request, not as a direct push.
- A pull request description **MUST** link the work item that asked for the change.

### Agent-produced work says what produced it

Cite as `eng:std-PR.agent-produced-work-says-what-produced-it`.

- A pull request containing agent-produced work **MUST** name the agent, the model and the prompt or task that produced it.
- Agent-produced work **MUST** have the approval of a named person, who owns the change afterwards.
- A reviewer **MUST NOT** accept an agent's own summary of a change as evidence that the change is correct.
- Branch policy **MUST** apply to agent-produced work unchanged, with no exemption and no second route in.
- The identity that opened an agent-produced pull request **MUST NOT** approve or merge it.

### Somebody other than the author approves it

Cite as `eng:std-PR.somebody-other-than-the-author-approves-it`.

- A pull request **MUST** have at least one approval from somebody other than the author before it merges.
- A reviewer **MUST** check the change against the standards that apply to it, and not read for style alone.
- A reviewer **MUST** state what they checked when they approve.

## eng:std-SECRET: A secret is read from the store at run time

Status: draft.

Controls: `eng:ctl-0006` Nothing checks a repository for a committed secret (`not-enforced`, active).

### A secret comes from the store

Cite as `eng:std-SECRET.a-secret-comes-from-the-store`.

- A service **MUST** read every secret from the managed store, through an identity granted to the workload.
- An environment below production **MUST** have its own secrets, distinct from production's.

### Every secret rotates

Cite as `eng:std-SECRET.every-secret-rotates`.

- A secret **MUST** be replaceable in the store without a code change or a rebuild.
- Every secret **MUST** rotate on a period recorded against it in the store.

### Nowhere else holds a secret

Cite as `eng:std-SECRET.nowhere-else-holds-a-secret`.

- A repository **MUST NOT** contain a secret, in source, in a configuration file, in a pipeline definition or in a test fixture.
- A build **MUST NOT** embed a secret in an artefact or an image.
- A service **MUST NOT** write a secret to a log, to a console, to an error message or to a support ticket.
- A pipeline **MUST** run a secret scanner over the repository and its history, and fail on a finding.

## eng:std-TEST: A test runs at the lowest level that can catch the fault

Status: draft.

Controls: `ctl-0009` Three test layers run before a merge (`ci`, active).

### Know what the suite reaches

Cite as `eng:std-TEST.know-what-the-suite-reaches`.

- A build **MUST** report line coverage.
- A build **MUST** fail where coverage falls below the figure the repository records.
- A team **MUST** read a coverage drop as a gap in the tests rather than as a threshold to lower.

### Pick the level from the fault

Cite as `eng:std-TEST.pick-the-level-from-the-fault`.

- A team **MUST** cover a rule about one unit's behaviour with a test on that unit alone.
- A team **MUST** cover an agreement between two components with a test that exercises both.
- A team **MUST** keep the tests that run on every push under ten minutes, so the answer arrives while the work is still open.
- A test **MUST** fail for one reason.
- A test's name **MUST** say what that reason is.
- A test **MUST NOT** depend on the order the suite runs in, on the wall clock, or on a service somebody else deploys.

### The data is never a real customer's

Cite as `eng:std-TEST.the-data-is-never-a-real-customers`.

- A test **MUST** run against generated data, or against production data that a masking step has already processed.
- A team **MUST NOT** copy unmasked production data into a test fixture, a seed script or a local database.
- A team **MUST NOT** run a test, a script or a debugger against production.

## eng:std-VCS: Everything a service needs is committed to its repository

Status: draft.

Controls: none.

### Every change is attributable and reviewed

Cite as `eng:std-VCS.every-change-is-attributable-and-reviewed`.

- The default branch **MUST** refuse a direct push, so that every change arrives as a merge.
- A commit **MUST** name the work item that asked for the change.
- A commit **SHOULD** name it in the subject as `#<id>`, because the forge then links the commit to the item.
- A commit **MUST** be made under the author's own verified identity.
- A commit **MUST NOT** be authored by a shared or generic account.

### The repository holds everything the service needs

Cite as `eng:std-VCS.the-repository-holds-everything-the-service-needs`.

- A repository **MUST** contain the source, the build definition, the infrastructure definition, the configuration templates and the recovery scripts for the services it owns.
- A repository **MUST NOT** depend on an asset kept only on a workstation, in a cloud portal or in an unversioned share.
- Infrastructure, schema and configuration changes **MUST** go through the same review as application code.
