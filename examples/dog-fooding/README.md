# `dog-fooding` ... a tiered example corpus defined against the KaC framework itself

> **Everything here is real.** Unlike the other example corpora this one describes the repository it sits in: the tool,
> the docs site, the marketplace branch, and the rules a change to any of them answers to. It's here to test the
> framework against itself as per 'eating your own dogfood'.

```text
                                              YOU ARE HERE
                                                   ▼
┌───────────────┐      ┌─────────────┐      ┌─────────────┐
│ kac framework │ ━━━► │ engineering │ ━┳━► │ dog-fooding │
└───────────────┘      └─────────────┘  ┃   └─────────────┘
        ╎                       ╎       ┃   ┌─────────────┐
   The framework defines how    ╎       ┗━► │ payments    │
   knowledge is structured      ╎           └─────────────┘
                                ╎               ╎
        Parent corpus defines cross-cutting     ╎
        engineering expectations and policies   ╎
                                                ╎
             Downstream corpora adopt those expectations
             and extend them within a specific domain
```

> [/examples/README.md](../README.md) provides the overview of the KaC example corpora - read it first.

## The knowledge types

<!-- BEGIN GENERATED: types-index -->

| Type                       | Tier        | What it holds                                                                                                   |
|----------------------------|-------------|-----------------------------------------------------------------------------------------------------------------|
| [Control](controls.md)     | normative   | How a standard's rules are verified: the mechanism, the frequency, and the evidence it leaves.                  |
| [Deviation](deviations.md) | normative   | A knowing departure from a rule, the person who accepted the risk, and the date it is reviewed.                 |
| [Process](processes.md)    | procedural  | A planned procedure followed deliberately (releasing, onboarding, provisioning, rotating a secret).             |
| [Runbook](runbooks.md)     | procedural  | An incident-time procedure read under pressure: terse, imperative, structured as a decision tree.               |
| [Service](services.md)     | descriptive | One deployable component: purpose, repo, platform, environments, dependencies, data stores, owner.              |
| [Standard](standards.md)   | normative   | The rulebook, imperative, RFC 2119, with concrete examples and a conformance checklist.                         |
| [Tool](tools.md)           | descriptive | The approved-software register. What is chosen, rejected or deprecated, and the version ranges we stand behind. |

**Where does a document go?** The [taxonomy](knowledge-as-code/taxonomy.md) has the decision table, what each type is
and is not, and the calls that are genuinely close.

<!-- END GENERATED: types-index -->

## Working in this corpus

Needs `kac` on your path. [`../../README.md`](../../README.md#running-the-tool) covers the ways to get one.

```bash
kac restore      # fetch the corpora this one consumes into .imports/
kac validate     # frontmatter, links, structure, clauses and the graph
kac generate     # regenerate the indexes and generated blocks
kac export       # write the corpus to .dist/export/ as data a consumer reads
kac bundle       # assemble that export and .plugin/ into a plugin under .dist/plugin/
kac checks       # list every check the validator implements
```

`restore` comes first, and it needs a package to take. Run `kac export` and then `kac pack` in
[`../engineering/`](../engineering/) once, and this corpus's `source:` has a folder to read.

While you are changing the tool, run `dotnet run --project ../../tooling/kac -- validate` instead. That reaches the
working tree, and an installed `kac` does not.

## Reading this corpus as a plugin while you change it

The plugin a session here already carries is served from the `marketplace` branch, which holds what is on `main`. A
branch editing this corpus leaves that copy behind, so `standards-lookup` answers with a record you have already
rewritten. Build the plugin here and install it as a marketplace of your own, and a session reads the working tree
instead. `export` reads the `.imports/` that `restore` fetches, so a clean clone runs the block above first.

```bash
kac export       # write the records this branch holds
kac bundle       # assemble them and ../../template/.plugin/ into .dist/plugin/
claude plugin marketplace add ./.dist
claude plugin install example-dogfooding@example-dogfooding
```

`bundle` writes `.dist/` as a marketplace offering one plugin, and names the marketplace and the plugin alike after
the corpus. `claude plugin marketplace add` resolves whatever path you type and stores the absolute one in your own
user settings. That absolute path is a fact about your machine, so the install stays yours and no commit carries it.

Leaving the branch copy enabled as well gives you two of every skill. Turn it off in the settings file git ignores:

```bash
claude plugin disable example-dogfooding@knowledge-as-code --scope local
```

Then restart the session. Claude Code reads a skill when a session starts, so nothing installed part way through one is
reachable until the next.

### Rebuilding after a change

| You changed                                                               | Run                             |
|---------------------------------------------------------------------------|---------------------------------|
| a record here, or the `.corpus.yaml` describing this corpus               | `kac export`, then `kac bundle` |
| a skill or a hook in [`../../template/.plugin/`](../../template/.plugin/) | `kac bundle`                    |

`bundle` reads what `export` last wrote, so a skill change needs no second export. Reinstall either way:

```bash
claude plugin uninstall example-dogfooding@example-dogfooding
claude plugin install example-dogfooding@example-dogfooding
```

**Uninstall before you install.** The install caches the plugin under the `content-version` it carries, and neither
`claude plugin update` nor `claude plugin marketplace update` looks past that number. A rebuild leaving
`content-version` where it was refreshes nothing and reports success. Restart the session again to pick the new copy
up.

## What it demonstrates

**A corpus about the repository holding it.** The corpora beside it prove that the framework holds for an invented
estate, which is a weaker claim than it sounds: nothing pushes back when the fiction is convenient. Here the estate
answers back. A service record naming the wrong workflow is caught by whoever next reads the workflow, and a standard
nobody follows is visible as a standard nobody follows.

**A graph the tool can check.** Every type this corpus adopted holds records, and the edges between them are
frontmatter rather than prose. `implements:` on a standard names a clause in `../engineering/`, `verifies:` on a
control names the standard, and `applies-to:` on a runbook names the service it covers. `kac validate` fails the day
one of those stops resolving.

## What this corpus declares about itself

[`.corpus.yaml`](.corpus.yaml) says what this corpus is, which of the framework's types it has adopted, where its
published form is served from, and any deviation from the shared baseline it has deliberately accepted.

It declines `adrs`, because a decision about `kac` is recorded in `tooling/CLAUDE.md` and in the commit that made it,
and moving those here is a separate call. It declines `glossary`, because the framework's own vocabulary belongs to
`../engineering/` and this corpus cites it as `eng:`.

<details>
<summary>The clause map: 207 clauses, one verdict each</summary>

## How this corpus stands against `eng:`

[`../engineering/`](../engineering/) states 22 policies carrying 207 clauses. Every one of them carries a verdict
below, written by hand.

**Twelve standards arrive with `eng:`, and six are this corpus's own.** A standard is inherited the way a policy is:
a rule holding for any repository is written once in the governance corpus, and the corpus below it writes only what is
its own. `kac bundle` seals both sets into one plugin, so an agent reading this corpus reads every one of them.

**Covered.** One of those standards names the clause in `implements:`, and the verdict is that standard's id. Where two
policies state one obligation from both sides, covering either clause covers the pair. A covered clause has a rule.
Whether this repository follows that rule is a separate question, and the row answers it where the two differ.

**Gap.** No standard names the clause, and it reaches this repository. A gap is a fact rather than a task. Some are
worth closing, and some are what a repository with one maintainer costs. Every gap below names the deviation holding
it, because a departure nobody wrote down is one nobody can tell from never having known the rule.

**Out of scope.** No standard names the clause, and the thing it governs does not exist here.

**Every gap is deviated.** [deviations](deviations.md) holds a record for each departure this map found, with an
owner and a date somebody comes back to it. A gap row names that record and its verdict stays `Gap`, because a
deviation says the departure was decided rather than that a standard now covers the clause. One record holds every
clause the same departure reaches, so the register is shorter than the list of gaps.

Three limits sit on the whole map. It reads this corpus and what it imports, so a clause uncovered here may well be
covered in `../library/` or in `../payments/`, and every consumer of `eng:` answers for its own coverage. It never
says a clause is verified: a control names a standard rather than a rule, so it vouches for a whole document whatever
it checks inside it. The controls belong to the standards, listed once below, and a clause row leaves them out.

**Six clauses are covered through their pair.** No `implements:` anywhere names `eng:pol-ACCS.SHARED`,
`eng:pol-DATA.LOGS`, `eng:pol-DATA.UNMASK`, `eng:pol-SCRT.REUSE`, `eng:pol-TRUS.TRACE` or `eng:pol-VURM.REGRESS`.
Each states an obligation another clause states from the other side, and a standard covers that one.

Nothing regenerates this map. Move it by hand when a policy, a standard or a control moves. [prc-add-a-record]
carries the step that sends you here. The hand-written form stands until `kac` reports the coverage edge itself.
[#313] carries that work.

### The controls behind the standards

`eng:` exports policies, standards and a glossary, and holds no controls, so every control here is this corpus's own.
Six standards have one. The other twelve have none, which says nothing about whether they are followed.

| Standard        | Controls                                         |
|-----------------|--------------------------------------------------|
| `std-CI`        | ctl-0001, ctl-0002, ctl-0003, ctl-0006           |
| `std-CONFIG`    | ctl-0004, ctl-0005                               |
| `std-PLUGIN`    | ctl-0008                                         |
| `std-VERS`      | ctl-0007                                         |
| `eng:std-GATES` | ctl-0001, ctl-0007, ctl-0008, ctl-0009, ctl-0010 |
| `eng:std-TEST`  | ctl-0009                                         |

### Where the 207 clauses land

| Policy     | Clauses | Covered | Gap    | Out of scope |
|------------|---------|---------|--------|--------------|
| [pol-A11Y] | 7       | 5       | 1      | 1            |
| [pol-ACCS] | 11      | 3       | 4      | 4            |
| [pol-AGNT] | 8       | 8       | 0      | 0            |
| [pol-AUTV] | 13      | 9       | 4      | 0            |
| [pol-COST] | 8       | 0       | 0      | 8            |
| [pol-DATA] | 15      | 2       | 0      | 13           |
| [pol-DERV] | 5       | 0       | 5      | 0            |
| [pol-DEVI] | 9       | 2       | 7      | 0            |
| [pol-ENVS] | 10      | 7       | 0      | 3            |
| [pol-EVER] | 8       | 7       | 1      | 0            |
| [pol-INCR] | 13      | 0       | 10     | 3            |
| [pol-INTC] | 8       | 7       | 1      | 0            |
| [pol-KNOW] | 6       | 4       | 2      | 0            |
| [pol-MEXP] | 11      | 1       | 0      | 10           |
| [pol-OBSV] | 10      | 9       | 0      | 1            |
| [pol-PERF] | 5       | 0       | 0      | 5            |
| [pol-PIPE] | 11      | 9       | 0      | 2            |
| [pol-RECV] | 12      | 0       | 0      | 12           |
| [pol-SCRT] | 8       | 6       | 1      | 1            |
| [pol-SECD] | 8       | 2       | 5      | 1            |
| [pol-TRUS] | 13      | 9       | 4      | 0            |
| [pol-VURM] | 8       | 5       | 2      | 1            |
| **Total**  | **207** | **95**  | **47** | **65**       |

### pol-A11Y: software we build is usable by everyone

The documentation site is the one thing here a stranger reads, and `kac` prints to a terminal. `std-A11Y` governs
both and divides them, because [WCAG 2.2 AA] measures a page a browser renders and reaches no terminal. Nothing in CI
checks any of it.

| Clause    | Verdict                          | Note                                                                            |
|-----------|----------------------------------|---------------------------------------------------------------------------------|
| `UPFRONT` | `std-A11Y`                       | `std-A11Y` says what each surface owes before a page is written.                |
| `CONFORM` | `std-A11Y`                       | WCAG 2.2 AA is the measure, and a reviewer applies it. No check does.           |
| `VENDOR`  | `std-A11Y`                       | `tol-mkdocs-material` and `tol-spectre-console` each carry an assessment.       |
| `PUBLISH` | `std-A11Y`                       | A statement is owed where a law or a contract asks. [PSBAR 2018] binds neither. |
| `WORSE`   | `std-A11Y`                       | A change knowingly reducing either surface needs a recorded deviation.          |
| `ASSIST`  | Gap, `dev-no-screen-reader-pass` | Neither surface has been read with a screen reader.                             |
| `INCLUDE` | Out of scope                     | There is no user research here for anybody to take part in.                     |

### pol-ACCS: access is by individual identity, on least privilege

Access here is a GitHub account and the tokens a workflow holds. `std-CI` reaches the tokens, and nothing reaches the
account.

| Clause    | Verdict                            | Note                                                                          |
|-----------|------------------------------------|-------------------------------------------------------------------------------|
| `NAMED`   | Gap, `dev-github-holds-identity`   | Every change arrives under a named account, and no standard states it.        |
| `LEAST`   | `eng:std-CONT`, `std-CI`           | A job declares the permission it needs. No container runs here.               |
| `DUTIES`  | `std-CI`                           | A person approves the `nuget.org` environment before a publish spends it.     |
| `AUTHN`   | Gap, `dev-github-holds-identity`   | GitHub holds the authentication, and no standard says what it must be.        |
| `RECERT`  | Out of scope                       | One maintainer holds every grant, so there is no access review to run.        |
| `REVOKE`  | Out of scope                       | Nobody joins and nobody leaves.                                               |
| `ADMIN`   | Out of scope                       | The administrative tooling is GitHub's settings, which record their use.      |
| `SHARED`  | `eng:std-VCS`                      | Covered through `eng:pol-EVER.SHARED`, the same duty from the other side.     |
| `PERSIST` | Gap, `dev-standing-publish-rights` | The maintainer's publish rights stand permanently, and nothing revisits them. |
| `DIRECT`  | Out of scope                       | Identity is GitHub's, and it is already in one place.                         |
| `ZERO`    | Gap, `dev-standing-publish-rights` | Publishing waits for an approval, and the rights behind it never go away.     |

### pol-AGNT: agents propose, people decide

Agents write a large part of this repository. `eng:std-PR` governs what they hand over, and asks for an approval from
somebody other than the author, which one maintainer does both halves of here. `std-PLUGIN` governs what they read: the
plugin this corpus publishes, and the skills that answer out of it.

| Clause    | Verdict      | Note                                                                               |
|-----------|--------------|------------------------------------------------------------------------------------|
| `PROV`    | `eng:std-PR` | Agent-produced work says what produced it, and a commit trailer names it.          |
| `ACCEPT`  | `eng:std-PR` | The approval is what makes the work somebody's.                                    |
| `EQUAL`   | `eng:std-PR` | A change arrives as a pull request, whoever wrote the branch.                      |
| `CONFID`  | `std-PLUGIN` | A skill dates the export and reports an unsettled record. No observation expires.  |
| `SELFVER` | `eng:std-PR` | The approver reads the change rather than the agent's account of it.               |
| `DUTIES`  | `eng:std-PR` | Somebody other than the author approves it. One maintainer does both here.         |
| `UNPROV`  | `std-PLUGIN` | A skill names the record an answer came from. No proposal names the run behind it. |
| `ACCESS`  | `std-PLUGIN` | A skill asks only to read a file. An agent still holds the maintainer's own keys.  |

### pol-AUTV: every change is verified automatically, and failures block

This is the best-covered policy here, between the gate `std-CI` describes and four inherited standards.

| Clause    | Verdict                                 | Note                                                              |
|-----------|-----------------------------------------|-------------------------------------------------------------------|
| `INTEG`   | `eng:std-GATES`, `std-CI`, `std-CONFIG` | Every job runs on a pull request into `main`.                     |
| `BLOCK`   | `eng:std-GATES`, `std-CI`, `std-CONFIG` | The branch rule names `validate` as the check a merge waits for.  |
| `REPRO`   | Gap, `dev-no-reproducible-build`        | Any clone builds the tool, and no standard states the rule.       |
| `LEVELS`  | `eng:std-NETTST`, `eng:std-TEST`        | Unit, behaviour and golden layers each catch a different fault.   |
| `REGRESS` | `eng:std-GATES`                         | A fixed defect keeps the test that catches it.                    |
| `BROKEN`  | Gap, `dev-branch-habits-unstated`       | Nothing says a red `main` comes before other work.                |
| `BYPASS`  | `eng:std-GATES`                         | A merge over a failing check needs a deviation nobody can record. |
| `DISABLE` | `eng:std-GATES`                         | `std-CI` adds that no job may declare `continue-on-error`.        |
| `MACHINE` | `eng:std-GATES`                         | Each matrix cell is a fresh runner holding its own checkout.      |
| `OFTEN`   | Gap, `dev-branch-habits-unstated`       | Branch size is a habit here rather than a rule.                   |
| `WARN`    | `eng:std-CSSTY`                         | The analysers decide, and a suppression is local and says why.    |
| `COVER`   | `eng:std-NETTST`, `eng:std-TEST`        | `kac-tests.cs` carries the coverage gate.                         |
| `BITWISE` | Gap, `dev-no-reproducible-build`        | Nothing asks two builds of the tool to produce the same bytes.    |

### pol-COST: cost is a non-functional requirement

**Out of scope, all eight clauses.** Nothing here runs on metered infrastructure. GitHub hosts the workflows, the
packages, the marketplace branch and the site for a public repository, and bills none of it. That reaches `ATTRIB`,
`VISIBLE`, `WEIGHED`, `SIZING`, `ANOMALY`, `UNUSED`, `UNOWNED` and `ERODE`.

### pol-DATA: data is protected according to its sensitivity

**Out of scope, thirteen clauses.** Nothing here holds data on anybody's behalf. Every file is public by design, and
the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account
holder asked it to. That reaches `CLASS`, `CRYPTO`, `RETIRE`, `LAWFUL`, `MINIMAL`, `RIGHTS`, `LOCATE`, `XBORDER`,
`DELETE`, `SHARE`, `LINGER`, `AGILE` and `CLEAR`.

Two clauses reach the workflow log through their pair. `eng:std-SECRET` and `std-CI` cover `LOGS`, because both forbid
a step to print a secret. `eng:std-TEST` covers `UNMASK`, because its test data is never a real customer's.

### pol-DERV: derived data is verified before it is trusted

`kac generate` computes a generated block from the schema and the frontmatter, and `kac export` computes the data a
consumer reads. Both are derived data, and every clause here is a gap.

| Clause    | Verdict                                | Note                                                                                  |
|-----------|----------------------------------------|---------------------------------------------------------------------------------------|
| `EXPECT`  | Gap, `dev-generated-output-ungoverned` | `generate --check` regenerates and compares, and no standard states that as the rule. |
| `CHECK`   | Gap, `dev-generated-output-ungoverned` | ctl-0007 runs that check on every pull request, for `eng:std-GATES` rather than this. |
| `RUNLOG`  | Gap, `dev-generated-output-ungoverned` | The run leaves a workflow log, and nothing keeps its inputs beside its output.        |
| `FAILED`  | Gap, `dev-generated-output-ungoverned` | A stale generated file fails the gate, and no standard says the output is unusable.   |
| `LINEAGE` | Gap, `dev-generated-output-ungoverned` | A generated block names itself, and not the records it was computed from.             |

### pol-DEVI: deviations are recorded, owned and time-bound

This corpus holds the register, in [deviations](deviations.md). What it does not hold is a standard saying how a
deviation is written, closed or reviewed. The two clauses `eng:std-GATES` covers reach a suppressed check rather than a
departure from a policy.

| Clause    | Verdict                        | Note                                                                       |
|-----------|--------------------------------|----------------------------------------------------------------------------|
| `RECORD`  | Gap, `dev-deviations-unstated` | The register exists. No standard says a record comes before the departure. |
| `OWNER`   | `eng:std-GATES`                | A skipped check names the person who accepted it.                          |
| `CONTENT` | Gap, `dev-deviations-unstated` | The schema requires four sections. No standard states them.                |
| `EXPIRY`  | `eng:std-GATES`                | A skipped check carries the date it is revisited.                          |
| `SURFACE` | Gap, `dev-deviations-unstated` | The register is published, and no standard says it has to be.              |
| `CLOSE`   | Gap, `dev-deviations-unstated` | Every record here is open, and no standard says what closing takes.        |
| `PERM`    | Gap, `dev-deviations-unstated` | Every record carries a review date. `expiry` is declared and does not run. |
| `CUSTOM`  | Gap, `dev-deviations-unstated` | This map did exactly that, once. No standard asks for it again.            |
| `DEBT`    | Gap, `dev-deviations-unstated` | A shortcut becomes an issue on the tracker, and no standard requires that. |

### pol-ENVS: environments are separated, and production stays in production

Nothing here runs, so there is no environment below production. Seven of these clauses carry a rule written for a
service that does run, and the rule lands on nothing.

| Clause    | Verdict          | Note                                                                      |
|-----------|------------------|---------------------------------------------------------------------------|
| `SPLIT`   | Out of scope     | There is no tier below production to hold apart.                          |
| `CREDS`   | Out of scope     | There is no lower environment for a production secret to reach.           |
| `SAMEDEF` | `eng:std-DEPLOY` | One artefact is promoted rather than rebuilt. Nothing here is promoted.   |
| `BASELIN` | `eng:std-CONT`   | The base image is chosen and pinned. No image runs here.                  |
| `PROMOTE` | `eng:std-DEPLOY` | Production changes through the pipeline alone.                            |
| `MASK`    | `eng:std-TEST`   | The test data is never a real customer's. Every fixture here is invented. |
| `DEBUG`   | `eng:std-TEST`   | The same rule. There is no production system to debug against.            |
| `REUSE`   | `eng:std-SECRET` | An environment below production holds its own secrets.                    |
| `UNMASK`  | `eng:std-TEST`   | The same rule as `MASK`.                                                  |
| `EPHEM`   | Out of scope     | Every CI job is a fresh runner already.                                   |

### pol-EVER: everything is in version control

This repository was built to satisfy this policy, and `eng:std-VCS` states most of it.

| Clause    | Verdict                               | Note                                                            |
|-----------|---------------------------------------|-----------------------------------------------------------------|
| `ASSETS`  | `eng:std-VCS`, `std-CONFIG`           | Every value the build reads is committed.                       |
| `HISTORY` | `eng:std-VCS`                         | Every change is attributable and reviewed.                      |
| `INTENT`  | `eng:std-PR`, `eng:std-VCS`           | A pull request carries the reasoning behind the change.         |
| `BRANCH`  | `eng:std-PR`, `eng:std-VCS`, `std-CI` | A push to `main` is rejected.                                   |
| `PARITY`  | `eng:std-VCS`, `std-CONFIG`           | A YAML file and a workflow answer to the same gate as the code. |
| `ORPHAN`  | `eng:std-VCS`, `std-CONFIG`           | A value living in more than one tree is copied and proved.      |
| `SHARED`  | `eng:std-VCS`                         | No shared account exists here to lose attribution to.           |
| `SIGNED`  | Gap, `dev-unsigned-commits`           | Commits are not signed.                                         |

### pol-INCR: incidents are managed and learned from

A bad version on nuget.org reaches whoever installs it, so most of this policy binds. Three runbooks cover known
failures, and no standard in either corpus reaches incident response.

| Clause    | Verdict                                | Note                                                                            |
|-----------|----------------------------------------|---------------------------------------------------------------------------------|
| `PROCESS` | Gap, `dev-no-incident-process`         | The runbooks cover three failures, and nothing says who decides in the rest.    |
| `TRIAGE`  | Out of scope                           | One maintainer, and nobody to escalate to.                                      |
| `COMMS`   | Gap, `dev-no-incident-process`         | Whoever installed a bad version hears nothing until the next one lands.         |
| `RECOVER` | Gap, `dev-no-incident-process`         | `std-VERS` says a correction ships as a new version, for `eng:pol-PIPE.REVERT`. |
| `EVIDENC` | Gap, `dev-nothing-learns-from-a-fault` | The corpus adopted no type that holds an incident record.                       |
| `NOTIFY`  | Out of scope                           | No personal data, so no breach to report to a supervisory authority.            |
| `INFORM`  | Out of scope                           | Same: there is nobody whom a breach here could put at risk.                     |
| `REPORT`  | Gap, `dev-no-incident-process`         | `.github/SECURITY.md` gives the route, and no standard names it.                |
| `LEARN`   | Gap, `dev-nothing-learns-from-a-fault` | A fix lands and nothing asks what allowed the fault.                            |
| `ACTIONS` | Gap, `dev-nothing-learns-from-a-fault` | Paired with `eng:pol-SECD.ACTIONS`, which no standard covers either.            |
| `DRILL`   | Gap, `dev-no-incident-process`         | The publish path is first exercised for real, every time.                       |
| `ADHOC`   | Gap, `dev-no-incident-process`         | Nothing here has to be handled formally, so everything is handled informally.   |
| `TOOSOON` | Gap, `dev-nothing-learns-from-a-fault` | Nothing holds an incident open until the learning is written down.              |

### pol-INTC: interfaces are contracts we honour

`kac` publishes four interfaces: the command surface, `.schema/`, the export a consumer reads, and the package.
`eng:std-API` states the rules for all four, and it is written for an HTTP service.

| Clause    | Verdict                         | Note                                                                   |
|-----------|---------------------------------|------------------------------------------------------------------------|
| `SPEC`    | `eng:std-API`                   | The contract is the source of truth. `.schema/` is that contract here. |
| `VERSION` | `eng:std-API`                   | A change carries a version and a notice.                               |
| `DEPREC`  | Gap, `dev-export-has-no-notice` | No standard says how much notice a consumer of the export gets.        |
| `NOTICE`  | `eng:std-API`                   | The same rule as `VERSION`.                                            |
| `SECURE`  | `eng:std-API`                   | Every endpoint authenticates and validates. Nothing here listens.      |
| `HOLDS`   | `eng:std-API`                   | ctl-0008 reads a published corpus back and compares it.                |
| `BREAK`   | `eng:std-API`                   | A break carries a version increment and a notice.                      |
| `EXPOSE`  | `eng:std-API`                   | Everything here is public, so no interface hides anything.             |

### pol-KNOW: knowledge is written down and kept with what it describes

The four `CLAUDE.md` files, the site and this corpus are the answer to most of this policy. `std-PROSE` reaches how the
words are written for an agent, `std-PLUGIN` reaches the one copy those words travel in, and `std-VERS` reaches how
each of them is versioned and kept beside what it describes.

| Clause   | Verdict                              | Note                                                                       |
|----------|--------------------------------------|----------------------------------------------------------------------------|
| `DOCS`   | `std-VERS`                           | Every stamp is named, and what a move of each one says is written down.    |
| `SYNC`   | `std-VERS`                           | A producer's move and its consumers' locks land in one pull request.       |
| `DECIDE` | Gap, `dev-decisions-live-in-commits` | The reasoning behind a decision lives in the commit that made it.          |
| `AGENTS` | `std-PROSE`                          | The rules sit where the agents doing the work read them.                   |
| `HEADS`  | Gap, `dev-one-maintainer`            | One maintainer, and nothing tests what only they know.                     |
| `COPY`   | `std-PLUGIN`                         | A skill quotes what the export carried, and links the record for the rest. |

### pol-MEXP: exposure is minimised and traffic is controlled

**Out of scope, ten clauses.** There is no network here to control. `kac` reads a folder and reaches a registry only
when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates.
That reaches `SEGMENT`, `DENY`, `TRANSIT`, `PEERID`, `PRIVATE`, `EGRESS`, `ASCODE`, `LATERAL`, `WEAKEN` and `ZEROTR`.

`PUBLIC` is the eleventh, and `eng:std-CONT` covers it: a container holds only what the service needs, and exposes no
management interface. No container runs here.

### pol-OBSV: systems are observable and actively monitored

Nothing here runs, so there is nothing to watch. `eng:std-OBS` states nine of these clauses for a service that does,
and the workflow log is the only telemetry this repository produces.

| Clause    | Verdict       | Note                                                                                |
|-----------|---------------|-------------------------------------------------------------------------------------|
| `CENTRAL` | `eng:std-OBS` | Everything lands in the central store.                                              |
| `CLOCKS`  | `eng:std-OBS` | One request reads as one timeline.                                                  |
| `RETAIN`  | `eng:std-OBS` | The store keeps telemetry for a stated period. GitHub keeps the logs.               |
| `HEALTH`  | `eng:std-OBS` | A service is monitored and an owner hears when it degrades.                         |
| `SECMON`  | Out of scope  | No standard reaches security monitoring, and nothing here emits an event.           |
| `ALERTS`  | `eng:std-OBS` | Somebody acts on every alert.                                                       |
| `BLIND`   | `eng:std-OBS` | A service with no monitoring does not ship.                                         |
| `SECRETS` | `eng:std-OBS` | Telemetry carries no personal data, and `std-CI` adds that no step prints a secret. |
| `SLO`     | `eng:std-OBS` | What good looks like is written down and watched.                                   |
| `CORREL`  | `eng:std-OBS` | One id ties a request together across systems.                                      |

### pol-PERF: performance targets are stated and verified

**Out of scope, all five clauses.** Nothing here is performance-sensitive. No corpus is large enough for its size to
matter, and no load arrives that somebody did not start. That reaches `TARGETS`, `MEASURE`, `DEFECT`, `PEAK` and
`NOTEST`.

### pol-PIPE: changes reach production through the pipeline

Publishing is what this repository does to production, and `std-CI` and `eng:std-DEPLOY` state nearly all of it.

| Clause    | Verdict                        | Note                                                                         |
|-----------|--------------------------------|------------------------------------------------------------------------------|
| `DEPLOY`  | `eng:std-DEPLOY`, `std-CI`     | Every publish runs from a workflow, and nobody publishes by hand.            |
| `SAMEART` | `eng:std-DEPLOY`               | The artefact is built once. The publish job rebuilds from the merge commit.  |
| `CONFIG`  | `eng:std-DEPLOY`, `std-CONFIG` | Configuration sits outside the artefact.                                     |
| `TRACE`   | `eng:std-DEPLOY`, `std-CI`     | A publish tags the commit it published from.                                 |
| `REVERT`  | `eng:std-DEPLOY`, `std-VERS`   | A correction ships as a new version, and no published one moves.             |
| `ASCODE`  | `eng:std-DEPLOY`, `std-CI`     | The workflows are reviewed like any other file.                              |
| `GATES`   | `std-CI`                       | `validate` is the check a merge waits for.                                   |
| `FLAGS`   | Out of scope                   | Nothing here carries a flag that changes behaviour in production.            |
| `MANUAL`  | `eng:std-DEPLOY`, `std-CI`     | Nobody edits a published branch, package or release by hand.                 |
| `LOCAL`   | `eng:std-DEPLOY`, `std-CI`     | The publishing job builds what it publishes.                                 |
| `PROGDEL` | Out of scope                   | A version is published whole, and there is nothing to release progressively. |

### pol-RECV: services and data are recoverable

**Out of scope, all twelve clauses.** Nothing here serves a request, so nothing degrades, retries or sheds load. What
there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be
changed. That reaches `RTORPO`, `BACKUP`, `RESTORE`, `OFFSITE`, `TIMEOUT`, `DEGRADE`, `IDEMPOT`, `UNTEST`, `RETRY`,
`REDUND`, `SHED` and `CHAOS`.

### pol-SCRT: secrets are managed, never embedded

Trusted publishing left this repository with almost no secret to manage. The workflows spend `github.token` and an
identity they exchange, and `std-CI` states where both come from.

| Clause    | Verdict                                  | Note                                                                     |
|-----------|------------------------------------------|--------------------------------------------------------------------------|
| `STORE`   | `eng:std-SECRET`, `std-CI`               | Every credential comes from the store or from an exchange.               |
| `ROTATE`  | `eng:std-SECRET`, `std-CI`               | A job exchanges its identity for a short-lived key.                      |
| `KEYS`    | Out of scope                             | There is no key or certificate here to hold through a lifecycle.         |
| `LEAKED`  | `eng:std-SECRET`                         | A pipeline runs a secret scanner over the history. No job here does.     |
| `EMBED`   | `eng:std-SECRET`, `std-CI`, `std-CONFIG` | No secret sits in a workflow, a configuration file or an artefact.       |
| `REUSE`   | `eng:std-SECRET`                         | Covered through `eng:pol-ENVS.REUSE`, the same duty from the other side. |
| `LOGS`    | `eng:std-SECRET`, `std-CI`               | A step passes a secret through `env:` and never prints one.              |
| `ZEROSEC` | Gap, `dev-trusted-publishing-unstated`   | Trusted publishing removed the last static key, and no standard says so. |

### pol-SECD: security is designed in, not added on

`.github/SECURITY.md` states what is in scope, and says how CI contains the untrusted code it runs on purpose. No
standard names that page, and the two clauses that are covered are covered by standards written for the code rather
than for the design.

| Clause    | Verdict                                | Note                                                                           |
|-----------|----------------------------------------|--------------------------------------------------------------------------------|
| `REQS`    | Gap, `dev-security-design-unstated`    | A security requirement arrives as a review comment rather than a requirement.  |
| `DESIGN`  | Gap, `dev-security-design-unstated`    | The workflows fail closed and deny by default, stated by no standard.          |
| `THREAT`  | Gap, `dev-security-design-unstated`    | `.github/SECURITY.md` says how CI contains what it runs, named by no standard. |
| `IMPACT`  | Out of scope                           | No processing of personal data, so nothing to assess the impact of.            |
| `ACTIONS` | Gap, `dev-nothing-learns-from-a-fault` | Paired with `eng:pol-INCR.ACTIONS`, which no standard covers either.           |
| `CODING`  | `eng:std-CSSTY`                        | The conventions come from one file, and the analysers enforce them.            |
| `CODEREV` | `eng:std-PR`                           | Somebody other than the author approves it. One maintainer does both here.     |
| `HIRISK`  | Gap, `dev-security-design-unstated`    | Nothing sorts a change by risk before it is built.                             |

### pol-TRUS: we ship only components we know and trust

Dependencies are pinned and Dependabot moves them, and `eng:std-DEPS` states what happens before a package is adopted
and after.

| Clause    | Verdict                                                | Note                                                                      |
|-----------|--------------------------------------------------------|---------------------------------------------------------------------------|
| `INVENT`  | `eng:std-DEPS`, `std-CONFIG`                           | A pin names an exact version, and `tools/` names what is chosen.          |
| `SCREEN`  | `eng:std-DEPS`                                         | A new package is screened before the pull request adding it merges.       |
| `LICENCE` | `eng:std-DEPS`                                         | A licence is checked against the allowed list before adoption.            |
| `MALWARE` | Gap, `dev-package-unscanned`                           | Nothing scans the package the tool ships.                                 |
| `SOURCE`  | `eng:std-CONT`, `eng:std-DEPS`, `std-CI`, `std-CONFIG` | An action is pinned to a commit, and a package comes from nuget.org.      |
| `CLOUD`   | Gap, `dev-github-concentration`                        | GitHub holds most of the responsibility here, and no standard divides it. |
| `EXIT`    | Gap, `dev-github-concentration`                        | Nothing says how this would leave GitHub.                                 |
| `REPO`    | `eng:std-DEPS`, `std-CI`                               | Every artefact is a version on a registry that keeps it.                  |
| `TRACE`   | `eng:std-DEPLOY`, `std-CI`                             | Covered through `eng:pol-PIPE.TRACE`, the same duty from the other side.  |
| `REVIEW`  | `std-CONFIG`                                           | Dependabot brings each pinned version back weekly.                        |
| `UNTRUST` | `eng:std-DEPS`, `std-CI`                               | A moving version may not enter a job holding a write permission.          |
| `MUTATE`  | `eng:std-CONT`, `std-CI`, `std-VERS`                   | A published tag never moves.                                              |
| `ATTEST`  | Gap, `dev-package-unscanned`                           | Nothing proves the origin of an artefact before it is installed.          |

### pol-VURM: vulnerabilities are found, prioritised and closed to a timeframe

`eng:std-DEPS` sets the scanning, the ranking and the windows. `.github/SECURITY.md` opens the route in, and no
standard names it.

| Clause    | Verdict                                 | Note                                                                       |
|-----------|-----------------------------------------|----------------------------------------------------------------------------|
| `SCAN`    | `eng:std-DEPS`                          | The dependency tree is scanned on every build and weekly.                  |
| `RANK`    | `eng:std-DEPS`                          | A finding is ranked by severity and by whether the path runs.              |
| `TIMEBOX` | `eng:std-DEPS`                          | Critical closes in 7 days and high in 30. Nothing here tracks that.        |
| `DISCLOS` | Gap, `dev-vulnerability-route-unstated` | A private advisory is the route in, and no standard names it.              |
| `REGRESS` | `eng:std-GATES`                         | Covered through `eng:pol-AUTV.REGRESS`, the same duty from the other side. |
| `SHIP`    | `eng:std-DEPS`                          | A release with an open critical finding needs a recorded deviation.        |
| `OVERDUE` | Gap, `dev-vulnerability-route-unstated` | No standard names who is accountable past the window.                      |
| `INDEP`   | Out of scope                            | One maintainer, and nobody else to test what they built.                   |

</details>

[#313]: https://github.com/paul80nd/knowledge-as-code/issues/313
[PSBAR 2018]: ../engineering/frameworks.md#psbar-2018
[WCAG 2.2 AA]: ../engineering/frameworks.md#wcag
[pol-A11Y]: ../engineering/policies/governance/a11y-accessibility.md#clauses
[pol-ACCS]: ../engineering/policies/security/accs-access-by-identity.md#clauses
[pol-AGNT]: ../engineering/policies/governance/agnt-agents-propose-people-decide.md#clauses
[pol-AUTV]: ../engineering/policies/delivery/autv-automated-verification.md#clauses
[pol-COST]: ../engineering/policies/delivery/cost-cost-as-an-nfr.md#clauses
[pol-DATA]: ../engineering/policies/security/data-data-protection.md#clauses
[pol-DERV]: ../engineering/policies/delivery/derv-derived-data-is-verified.md#clauses
[pol-DEVI]: ../engineering/policies/governance/devi-deviations-are-recorded.md#clauses
[pol-ENVS]: ../engineering/policies/security/envs-environment-separation.md#clauses
[pol-EVER]: ../engineering/policies/delivery/ever-everything-in-version-control.md#clauses
[pol-INCR]: ../engineering/policies/operations/incr-incident-response.md#clauses
[pol-INTC]: ../engineering/policies/delivery/intc-interface-contracts.md#clauses
[pol-KNOW]: ../engineering/policies/governance/know-knowledge-is-written-down.md#clauses
[pol-MEXP]: ../engineering/policies/security/mexp-minimised-exposure.md#clauses
[pol-OBSV]: ../engineering/policies/operations/obsv-observability.md#clauses
[pol-PERF]: ../engineering/policies/delivery/perf-performance-targets.md#clauses
[pol-PIPE]: ../engineering/policies/delivery/pipe-pipeline-to-production.md#clauses
[pol-RECV]: ../engineering/policies/operations/recv-recoverability.md#clauses
[pol-SCRT]: ../engineering/policies/security/scrt-secrets-are-never-embedded.md#clauses
[pol-SECD]: ../engineering/policies/security/secd-security-by-design.md#clauses
[pol-TRUS]: ../engineering/policies/security/trus-trusted-components.md#clauses
[pol-VURM]: ../engineering/policies/security/vurm-vulnerability-remediation.md#clauses
[prc-add-a-record]: processes/add-a-record.md
