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

| Type                     | Tier        | What it holds                                                                                                   |
|--------------------------|-------------|-----------------------------------------------------------------------------------------------------------------|
| [Control](controls.md)   | normative   | How a standard's rules are verified: the mechanism, the frequency, and the evidence it leaves.                  |
| [Runbook](runbooks.md)   | procedural  | An incident-time procedure read under pressure: terse, imperative, structured as a decision tree.               |
| [Service](services.md)   | descriptive | One deployable component: purpose, repo, platform, environments, dependencies, data stores, owner.              |
| [Standard](standards.md) | normative   | The rulebook, imperative, RFC 2119, with concrete examples and a conformance checklist.                         |
| [Tool](tools.md)         | descriptive | The approved-software register. What is chosen, rejected or deprecated, and the version ranges we stand behind. |

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

## How this corpus stands against `eng:`

[`../engineering/`](../engineering/) states 22 policies carrying 207 clauses. Every one of them carries a verdict
below, written by hand and read against the repository rather than against the graph.

**Covered.** A standard that binds this corpus states the rule, and the verdict is that standard's id. A standard
binds where a record here names it: a standard naming its clauses in `implements:`, or a control naming the standard
in `verifies:`. Three standards here qualify, and two inherited from `eng:` do. Where two policies state one
obligation from both sides, covering either clause covers the pair, and the row says which clause it is paired with.

**Gap.** The clause reaches this repository and no standard here states it. A gap is a fact rather than a task. Some
are worth closing, and some are what a repository with one maintainer costs.

**Out of scope.** The thing the clause governs does not exist here. The row says what is missing.

**No clause is deviated.** A deviation under `eng:pol-DEVI` is recorded, owned and given a review date, and this
corpus holds no record that does any of that. Where the repository knowingly departs from a clause, the row says so
and the verdict stays a gap.

Three limits sit on the whole map. It reads this corpus alone, so a clause uncovered here may well be covered in
`../library/` or in `../payments/`, and every consumer of `eng:` answers for its own coverage. It never says a clause
is verified: a control names a standard rather than a rule, so it vouches for a whole document whatever it checks
inside it. The controls belong to the standards, listed once below, and a clause row leaves them out.

**Six clauses land differently if a reader follows `implements:` alone.** `eng:std-TEST` names `DEBUG`, `MASK` and
`UNMASK` of `eng:pol-ENVS`, so the graph calls those covered, and there is no data here to mask. The map covers
`eng:pol-OBSV.SECRETS`, `eng:pol-TRUS.TRACE` and `eng:pol-VURM.REGRESS`, which no `implements:` here names: the last
two are the other side of a clause that is covered, and `std-CI` forbids a step to print a secret while naming
`eng:pol-SCRT.LOGS` for it.

Nothing regenerates this map. Move it by hand when a policy, a standard or a control moves.

| Standard        | Controls                                         |
|-----------------|--------------------------------------------------|
| `std-CI`        | ctl-0001, ctl-0002, ctl-0003, ctl-0006           |
| `std-CONFIG`    | ctl-0004, ctl-0005                               |
| `std-PROSE`     | none                                             |
| `eng:std-GATES` | ctl-0001, ctl-0007, ctl-0008, ctl-0009, ctl-0010 |
| `eng:std-TEST`  | ctl-0009                                         |

### Where the 207 clauses land

| Policy     | Clauses | Covered | Gap    | Out of scope |
|------------|---------|---------|--------|--------------|
| [pol-A11Y] | 7       | 0       | 5      | 2            |
| [pol-ACCS] | 11      | 2       | 5      | 4            |
| [pol-AGNT] | 8       | 0       | 8      | 0            |
| [pol-AUTV] | 13      | 8       | 5      | 0            |
| [pol-COST] | 8       | 0       | 0      | 8            |
| [pol-DATA] | 15      | 0       | 0      | 15           |
| [pol-DERV] | 5       | 0       | 5      | 0            |
| [pol-DEVI] | 9       | 2       | 7      | 0            |
| [pol-ENVS] | 10      | 0       | 0      | 10           |
| [pol-EVER] | 8       | 4       | 4      | 0            |
| [pol-INCR] | 13      | 0       | 10     | 3            |
| [pol-INTC] | 8       | 0       | 6      | 2            |
| [pol-KNOW] | 6       | 1       | 5      | 0            |
| [pol-MEXP] | 11      | 0       | 0      | 11           |
| [pol-OBSV] | 10      | 1       | 0      | 9            |
| [pol-PERF] | 5       | 0       | 0      | 5            |
| [pol-PIPE] | 11      | 8       | 0      | 3            |
| [pol-RECV] | 12      | 0       | 0      | 12           |
| [pol-SCRT] | 8       | 4       | 2      | 2            |
| [pol-SECD] | 8       | 0       | 7      | 1            |
| [pol-TRUS] | 13      | 7       | 6      | 0            |
| [pol-VURM] | 8       | 1       | 6      | 1            |
| **Total**  | **207** | **38**  | **81** | **88**       |

### pol-A11Y: software we build is usable by everyone

The documentation site is the one thing here a stranger reads, and `kac` prints to a terminal. Nobody has checked
either against [WCAG 2.2 AA].

| Clause    | Verdict      | Note                                                                           |
|-----------|--------------|--------------------------------------------------------------------------------|
| `UPFRONT` | Gap          | The site is designed as it is written, and no record asks for a requirement.   |
| `CONFORM` | Gap          | Nothing checks the published site against WCAG 2.2 AA.                         |
| `VENDOR`  | Gap          | The MkDocs theme was taken on its features, and nothing records what it fails. |
| `PUBLISH` | Out of scope | [PSBAR 2018] binds public sector bodies, and this repository is not one.       |
| `WORSE`   | Gap          | Nothing would report a change that made the site harder to use.                |
| `ASSIST`  | Gap          | Nobody has read the site with a screen reader.                                 |
| `INCLUDE` | Out of scope | There is no user research here for anybody to take part in.                    |

### pol-ACCS: access is by individual identity, on least privilege

Access here is a GitHub account and the tokens a workflow holds. `std-CI` reaches the tokens, and nothing reaches the
account.

| Clause    | Verdict      | Note                                                                              |
|-----------|--------------|-----------------------------------------------------------------------------------|
| `NAMED`   | Gap          | Every change arrives under a named account, and no record states the rule.        |
| `LEAST`   | `std-CI`     | A job declares the permission it needs, and holds no other.                       |
| `DUTIES`  | `std-CI`     | A person approves the `nuget.org` environment before a publish spends it.         |
| `AUTHN`   | Gap          | GitHub holds the authentication, and this corpus says nothing about it.           |
| `RECERT`  | Out of scope | One maintainer holds every grant, so there is no access review to run.            |
| `REVOKE`  | Out of scope | Nobody joins and nobody leaves.                                                   |
| `ADMIN`   | Out of scope | The only administrative tooling is GitHub's settings, which record their own use. |
| `SHARED`  | Gap          | No shared account exists, and no record forbids one.                              |
| `PERSIST` | Gap          | The maintainer's publish rights stand permanently, and nothing revisits them.     |
| `DIRECT`  | Out of scope | Identity is GitHub's, and it is already in one place.                             |
| `ZERO`    | Gap          | Publishing waits for an approval, and the rights behind it never go away.         |

### pol-AGNT: agents propose, people decide

Agents write a large part of this repository, and every clause governing them is a gap. `std-PROSE` covers where the
rules for an agent are written, which is `eng:pol-KNOW.AGENTS`, and not what the agent may then do.

| Clause    | Verdict | Note                                                                             |
|-----------|---------|----------------------------------------------------------------------------------|
| `PROV`    | Gap     | A commit trailer names the agent and the session, and no record requires it.     |
| `ACCEPT`  | Gap     | The maintainer merges the work, and no record makes that acceptance.             |
| `EQUAL`   | Gap     | The gate cannot tell who wrote a branch, and no record states that as the rule.  |
| `CONFID`  | Gap     | Nothing here expires an unverified observation.                                  |
| `SELFVER` | Gap     | An agent's own account of a change is what a reviewer usually reads first.       |
| `DUTIES`  | Gap     | One maintainer opens and merges, and nothing records that as accepted.           |
| `UNPROV`  | Gap     | Nothing traces a proposal back to the run that produced it.                      |
| `ACCESS`  | Gap     | An agent works with the maintainer's own credentials, and no record bounds that. |

### pol-AUTV: every change is verified automatically, and failures block

This is the best-covered policy here, between the gate `std-CI` describes and the two inherited standards its
controls verify.

| Clause    | Verdict                                 | Note                                                                |
|-----------|-----------------------------------------|---------------------------------------------------------------------|
| `INTEG`   | `std-CI`, `std-CONFIG`, `eng:std-GATES` | Every job runs on a pull request into `main`.                       |
| `BLOCK`   | `std-CI`, `std-CONFIG`, `eng:std-GATES` | The branch rule names `validate` as the check a merge waits for.    |
| `REPRO`   | Gap                                     | Any clone builds the tool, and no record states the rule.           |
| `LEVELS`  | `eng:std-TEST`                          | Unit, behaviour and golden layers each catch a different fault.     |
| `REGRESS` | `eng:std-GATES`                         | Paired with `eng:pol-VURM.REGRESS`, which states the same duty.     |
| `BROKEN`  | Gap                                     | Nothing says a red `main` comes before other work.                  |
| `BYPASS`  | `eng:std-GATES`                         | A merge over a failing check needs a deviation nobody can record.   |
| `DISABLE` | `eng:std-GATES`                         | `std-CI` adds that no job may declare `continue-on-error`.          |
| `MACHINE` | `eng:std-GATES`                         | Each matrix cell is a fresh runner holding its own checkout.        |
| `OFTEN`   | Gap                                     | Branch size is a habit here rather than a rule.                     |
| `WARN`    | Gap                                     | `eng:std-CSSTY` states it, and no record here adopts that standard. |
| `COVER`   | `eng:std-TEST`                          | `kac-tests.cs` carries the coverage gate.                           |
| `BITWISE` | Gap                                     | Nothing asks two builds of the tool to produce the same bytes.      |

### pol-COST: cost is a non-functional requirement

**Out of scope, all eight clauses.** Nothing here runs on metered infrastructure. GitHub hosts the workflows, the
packages, the marketplace branch and the site for a public repository, and bills none of it. That reaches `ATTRIB`,
`VISIBLE`, `WEIGHED`, `SIZING`, `ANOMALY`, `UNUSED`, `UNOWNED` and `ERODE`.

### pol-DATA: data is protected according to its sensitivity

**Out of scope, all fifteen clauses.** Nothing here holds data on anybody's behalf. Every file is public by design,
and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that
account holder asked it to. That reaches `CLASS`, `CRYPTO`, `RETIRE`, `LAWFUL`, `MINIMAL`, `RIGHTS`, `LOCATE`,
`XBORDER`, `DELETE`, `UNMASK`, `SHARE`, `LINGER`, `LOGS`, `AGILE` and `CLEAR`.

### pol-DERV: derived data is verified before it is trusted

`kac generate` computes a generated block from the schema and the frontmatter, and `kac export` computes the data a
consumer reads. Both are derived data, and every clause here is a gap.

| Clause    | Verdict | Note                                                                                  |
|-----------|---------|---------------------------------------------------------------------------------------|
| `EXPECT`  | Gap     | `generate --check` regenerates and compares, and no record states that as the rule.   |
| `CHECK`   | Gap     | ctl-0007 runs that check on every pull request, for `eng:std-GATES` rather than this. |
| `RUNLOG`  | Gap     | The run leaves a workflow log, and nothing keeps its inputs beside its output.        |
| `FAILED`  | Gap     | A stale generated file fails the gate, and no record says the output is unusable.     |
| `LINEAGE` | Gap     | A generated block names itself, and not the records it was computed from.             |

### pol-DEVI: deviations are recorded, owned and time-bound

No type this corpus adopted holds a deviation, so it has nowhere to record one. The two clauses `eng:std-GATES`
covers reach a suppressed check, and nothing here reaches a departure from a policy.

| Clause    | Verdict         | Note                                                                     |
|-----------|-----------------|--------------------------------------------------------------------------|
| `RECORD`  | Gap             | No record here holds a deviation, so none is recorded before or after.   |
| `OWNER`   | `eng:std-GATES` | A skipped check names the person who accepted it.                        |
| `CONTENT` | Gap             | Nothing states what a deviation has to say.                              |
| `EXPIRY`  | `eng:std-GATES` | A skipped check carries the date it is revisited.                        |
| `SURFACE` | Gap             | A departure lives in a commit message, where only its author looks.      |
| `CLOSE`   | Gap             | Nothing here is closed, because nothing here is open.                    |
| `PERM`    | Gap             | The departures this map names have stood since the corpus was written.   |
| `CUSTOM`  | Gap             | Nothing tests a long-standing habit against the policy it breaks.        |
| `DEBT`    | Gap             | A shortcut becomes an issue on the tracker, and no record requires that. |

### pol-ENVS: environments are separated, and production stays in production

**Out of scope, all ten clauses.** Nothing here runs, so there is no environment below production to separate one
from. `kac` is a command somebody runs on their own machine, and what CI publishes is the only thing that reaches
anybody else. That reaches `SPLIT`, `CREDS`, `SAMEDEF`, `BASELIN`, `PROMOTE`, `MASK`, `DEBUG`, `REUSE`, `UNMASK` and
`EPHEM`.

### pol-EVER: everything is in version control

This repository was built to satisfy this policy, and half of it is still a gap.

| Clause    | Verdict      | Note                                                                              |
|-----------|--------------|-----------------------------------------------------------------------------------|
| `ASSETS`  | `std-CONFIG` | Every value the build reads is committed.                                         |
| `HISTORY` | Gap          | Git attributes every change, and no record states the rule.                       |
| `INTENT`  | Gap          | A branch names its issue by habit. `eng:std-VCS` states it and nothing adopts it. |
| `BRANCH`  | `std-CI`     | A push to `main` is rejected, and the branch rule lives in GitHub's settings.     |
| `PARITY`  | `std-CONFIG` | A YAML file and a workflow answer to the same gate as the code.                   |
| `ORPHAN`  | `std-CONFIG` | A value living in more than one tree is copied and proved.                        |
| `SHARED`  | Gap          | Paired with `eng:pol-ACCS.SHARED`, which states the same duty.                    |
| `SIGNED`  | Gap          | Commits are not signed.                                                           |

### pol-INCR: incidents are managed and learned from

A bad version on nuget.org reaches whoever installs it, so most of this policy binds. Three runbooks cover known
failures, and a runbook is not a standard.

| Clause    | Verdict      | Note                                                                                 |
|-----------|--------------|--------------------------------------------------------------------------------------|
| `PROCESS` | Gap          | The runbooks cover three failures, and no record says who decides in the rest.       |
| `TRIAGE`  | Out of scope | One maintainer, and nobody to escalate to.                                           |
| `COMMS`   | Gap          | Whoever installed a bad version hears nothing until the next one lands.              |
| `RECOVER` | Gap          | `std-CI` states that a correction ships as a new version, for `eng:pol-PIPE.REVERT`. |
| `EVIDENC` | Gap          | The corpus adopted no type that holds an incident record.                            |
| `NOTIFY`  | Out of scope | No personal data, so no breach to report to a supervisory authority.                 |
| `INFORM`  | Out of scope | Same: there is nobody whom a breach here could put at risk.                          |
| `REPORT`  | Gap          | `.github/SECURITY.md` gives the route, and no record here names it.                  |
| `LEARN`   | Gap          | A fix lands and nothing asks what allowed the fault.                                 |
| `ACTIONS` | Gap          | Paired with `eng:pol-SECD.ACTIONS`, which states the same duty.                      |
| `DRILL`   | Gap          | The publish path is first exercised for real, every time.                            |
| `ADHOC`   | Gap          | Nothing here has to be handled formally, so everything is handled informally.        |
| `TOOSOON` | Gap          | Nothing holds an incident open until the learning is written down.                   |

### pol-INTC: interfaces are contracts we honour

`kac` publishes four interfaces: the command surface, `.schema/`, the export a consumer reads, and the package. The
documentation site describes them and no record here makes any of it a contract. `eng:std-API` states this policy, and
nothing here adopts it.

| Clause    | Verdict      | Note                                                                           |
|-----------|--------------|--------------------------------------------------------------------------------|
| `SPEC`    | Gap          | `.schema/` is the contract the tool enforces, and no record says so.           |
| `VERSION` | Gap          | `std-CI` moves versions by hand and states nothing about what a break costs.   |
| `DEPREC`  | Gap          | Nothing says how much notice a consumer of the export gets.                    |
| `NOTICE`  | Gap          | Same, for a removed command or a renamed field.                                |
| `SECURE`  | Out of scope | Nothing here listens on a network. `kac` reads files its caller already holds. |
| `HOLDS`   | Gap          | ctl-0008 reads a published corpus back, for `eng:std-GATES` rather than this.  |
| `BREAK`   | Gap          | A schema change and a tool release are versioned by judgement.                 |
| `EXPOSE`  | Out of scope | Everything here is public, so no interface hides anything.                     |

### pol-KNOW: knowledge is written down and kept with what it describes

The four `CLAUDE.md` files, the site and this corpus are the answer to most of this policy, and `std-PROSE` reaches
only how the words are written for an agent.

| Clause   | Verdict     | Note                                                                                 |
|----------|-------------|--------------------------------------------------------------------------------------|
| `DOCS`   | Gap         | The site and the `CLAUDE.md` files carry it, and no record requires them.            |
| `SYNC`   | Gap         | `generate --check` catches a stale generated block, and nothing catches stale prose. |
| `DECIDE` | Gap         | The reasoning behind a decision lives in the commit that made it.                    |
| `AGENTS` | `std-PROSE` | The rules sit where the agents doing the work read them.                             |
| `HEADS`  | Gap         | One maintainer, and nothing tests what only they know.                               |
| `COPY`   | Gap         | One tree holds the words for both readers, by convention rather than by rule.        |

### pol-MEXP: exposure is minimised and traffic is controlled

**Out of scope, all eleven clauses.** There is no network here to control. `kac` reads a folder and reaches a registry
only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it
terminates. That reaches `SEGMENT`, `DENY`, `TRANSIT`, `PEERID`, `PRIVATE`, `EGRESS`, `ASCODE`, `PUBLIC`, `LATERAL`,
`WEAKEN` and `ZEROTR`.

### pol-OBSV: systems are observable and actively monitored

**Out of scope, nine clauses.** Nothing here runs, so there is nothing to watch. The workflow log is the only
telemetry, GitHub keeps it, and no alert has anywhere to arrive. That reaches `CENTRAL`, `CLOCKS`, `RETAIN`, `HEALTH`,
`SECMON`, `ALERTS`, `BLIND`, `SLO` and `CORREL`.

`SECRETS` is the tenth, and `std-CI` covers it: a step must not print a secret.

### pol-PERF: performance targets are stated and verified

**Out of scope, all five clauses.** Nothing here is performance-sensitive. No corpus is large enough for its size to
matter, and no load arrives that somebody did not start. That reaches `TARGETS`, `MEASURE`,
`DEFECT`, `PEAK` and `NOTEST`.

### pol-PIPE: changes reach production through the pipeline

Publishing is what this repository does to production, and `std-CI` states nearly all of it.

| Clause    | Verdict      | Note                                                                         |
|-----------|--------------|------------------------------------------------------------------------------|
| `DEPLOY`  | `std-CI`     | Every publish runs from a workflow, and nobody publishes from a machine.     |
| `SAMEART` | Out of scope | There is one stage, so nothing is promoted between environments.             |
| `CONFIG`  | `std-CONFIG` | The values a build reads are committed beside it.                            |
| `TRACE`   | `std-CI`     | A publish tags the commit it published from.                                 |
| `REVERT`  | `std-CI`     | A published version is permanent, and a correction ships as a new one.       |
| `ASCODE`  | `std-CI`     | The workflows are reviewed like any other file.                              |
| `GATES`   | `std-CI`     | `validate` is the check a merge waits for.                                   |
| `FLAGS`   | Out of scope | Nothing here carries a flag that changes behaviour in production.            |
| `MANUAL`  | `std-CI`     | Nobody edits a published branch, package or release by hand.                 |
| `LOCAL`   | `std-CI`     | The publishing job builds what it publishes from the merge commit.           |
| `PROGDEL` | Out of scope | A version is published whole, and there is nothing to release progressively. |

### pol-RECV: services and data are recoverable

**Out of scope, all twelve clauses.** Nothing here serves a request, so nothing degrades, retries or sheds load. What
there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be
changed. That reaches `RTORPO`, `BACKUP`, `RESTORE`, `OFFSITE`, `TIMEOUT`, `DEGRADE`, `IDEMPOT`, `UNTEST`, `RETRY`,
`REDUND`, `SHED` and `CHAOS`.

### pol-SCRT: secrets are managed, never embedded

Trusted publishing left this repository with almost no secret to manage. The workflows spend `github.token` and an
identity they exchange, and `std-CI` states where both come from.

| Clause    | Verdict                | Note                                                                   |
|-----------|------------------------|------------------------------------------------------------------------|
| `STORE`   | `std-CI`               | Every credential comes from GitHub's secret store or from an exchange. |
| `ROTATE`  | `std-CI`               | A job exchanges its identity for a short-lived key as it spends it.    |
| `KEYS`    | Out of scope           | There is no key or certificate here to hold through a lifecycle.       |
| `LEAKED`  | Gap                    | No record says what looks for a secret that has leaked.                |
| `EMBED`   | `std-CI`, `std-CONFIG` | No secret sits in a workflow, a configuration file or an artefact.     |
| `REUSE`   | Out of scope           | There is no environment below production to reuse a secret in.         |
| `LOGS`    | `std-CI`               | A step passes a secret through `env:` and never prints one.            |
| `ZEROSEC` | Gap                    | Trusted publishing removed the last static key, and no record says so. |

### pol-SECD: security is designed in, not added on

`.github/SECURITY.md` states what is in scope, and says how CI contains the untrusted code it runs on purpose. No
record here names that page, and `eng:std-CSSTY` carries the coding clause for a standard nothing here adopts.

| Clause    | Verdict      | Note                                                                             |
|-----------|--------------|----------------------------------------------------------------------------------|
| `REQS`    | Gap          | A security requirement arrives as a review comment rather than as a requirement. |
| `DESIGN`  | Gap          | The workflows fail closed and deny by default, stated by no record.              |
| `THREAT`  | Gap          | `.github/SECURITY.md` says how CI contains what it runs, named by no record.     |
| `IMPACT`  | Out of scope | No processing of personal data, so nothing to assess the impact of.              |
| `ACTIONS` | Gap          | A finding becomes an issue by habit rather than by rule.                         |
| `CODING`  | Gap          | The C# follows the runtime team's conventions. `eng:std-CSSTY` states that.      |
| `CODEREV` | Gap          | Review happens on every pull request, and `eng:std-PR` is not adopted here.      |
| `HIRISK`  | Gap          | Nothing sorts a change by risk before it is built.                               |

### pol-TRUS: we ship only components we know and trust

Dependencies are pinned and Dependabot moves them, which `std-CONFIG` states. What happens when a component is
screened, licensed or left behind is not written anywhere.

| Clause    | Verdict                | Note                                                                    |
|-----------|------------------------|-------------------------------------------------------------------------|
| `INVENT`  | `std-CONFIG`           | A pin names an exact version, and `tools/` names what is chosen.        |
| `SCREEN`  | Gap                    | Dependabot raises an alert, and no record says what to do with it.      |
| `LICENCE` | Gap                    | Nothing screens a licence before a package arrives.                     |
| `MALWARE` | Gap                    | Nothing scans the package the tool ships.                               |
| `SOURCE`  | `std-CI`, `std-CONFIG` | An action is pinned to a commit, and a package comes from nuget.org.    |
| `CLOUD`   | Gap                    | GitHub holds most of the responsibility here, and no record divides it. |
| `EXIT`    | Gap                    | Nothing says how this would leave GitHub.                               |
| `REPO`    | `std-CI`               | Every artefact is a version on a registry that keeps it.                |
| `TRACE`   | `std-CI`               | Paired with `eng:pol-PIPE.TRACE`, which states the same duty.           |
| `REVIEW`  | `std-CONFIG`           | Dependabot brings each pinned version back weekly.                      |
| `UNTRUST` | `std-CI`               | A moving version may not enter a job holding a write permission.        |
| `MUTATE`  | `std-CI`               | A published version is never pushed again, replaced or deleted.         |
| `ATTEST`  | Gap                    | Nothing proves the origin of an artefact before it is installed.        |

### pol-VURM: vulnerabilities are found, prioritised and closed to a timeframe

`.github/SECURITY.md` opens the route in and sets expectations, and no record turns any of that into a rule.

| Clause    | Verdict         | Note                                                                       |
|-----------|-----------------|----------------------------------------------------------------------------|
| `SCAN`    | Gap             | Dependabot reports a vulnerable package, and no record states the rule.    |
| `RANK`    | Gap             | Nothing here sorts a finding by severity.                                  |
| `TIMEBOX` | Gap             | `.github/SECURITY.md` promises acknowledgement, and no remediation window. |
| `DISCLOS` | Gap             | A private advisory is the route in, and no record names it.                |
| `REGRESS` | `eng:std-GATES` | Paired with `eng:pol-AUTV.REGRESS`, which states the same duty.            |
| `SHIP`    | Gap             | Nothing stops a release over an open finding.                              |
| `OVERDUE` | Gap             | There is no timeframe for a finding to run past.                           |
| `INDEP`   | Out of scope    | One maintainer, and nobody else to test what they built.                   |

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
