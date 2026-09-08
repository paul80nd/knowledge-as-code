---
id: rpt-clause-coverage
type: report
tier: descriptive
status: active
owner: human:paul.law
generated: { at: 2026-09-08T13:55:35Z, by: kac/0.24.0 }
sources:
  - { resource: example-dogfooding, version: "0.15.0" }
  - { resource: example-engineering, version: "0.11.0" }
confirmed:
  - { at: 2026-09-08T13:41:31Z, by: human:paul.law }
tags: [ coverage, governance ]
---

# Clause coverage

`Report: rpt-clause-coverage` `ACTIVE`

## Limits

This reads `example-dogfooding` and what it imports. A clause uncovered here may well be covered in a corpus consuming
this one, and every consumer answers for its own coverage.

Imported:

* `eng`, example-engineering, at 0.11.0.

No column here says a clause is verified. A control names a standard and not a rule, so it vouches for a whole document
whatever it checks inside it.

## The verdicts

Every row carries one of four words. `kac report` prints `covered` and `uncovered` and stops, because it cannot tell a
clause nobody has got to from a clause about something this repository does not have.

| Verdict               | What it says                                                                        |
|-----------------------|-------------------------------------------------------------------------------------|
| `Covered`             | A standard names the clause in `implements:`, and the `Covered by` column names it. |
| `Covered by its pair` | The paired clause is covered, and the two state one obligation from both sides.     |
| `Gap`                 | Nothing covers it, and the thing it governs exists here.                            |
| `Out of scope`        | Nothing covers it, and the thing it governs does not exist here.                    |

A gap is a fact rather than a task. Some are worth closing, and some are what a repository with one maintainer costs.
Every gap names the deviation holding it in the `Deviations` column, because a departure nobody wrote down is one nobody
can tell from never having known the rule.

A covered clause has a rule. Whether this repository follows that rule is a separate question, and the `Note` answers it
where the two differ.

## Totals

| Policy         | Clauses | Covered | Uncovered |
|----------------|---------|---------|-----------|
| `eng:pol-A11Y` | 7       | 5       | 2         |
| `eng:pol-ACCS` | 11      | 2       | 9         |
| `eng:pol-AGNT` | 8       | 8       | 0         |
| `eng:pol-AUTV` | 13      | 9       | 4         |
| `eng:pol-COST` | 8       | 0       | 8         |
| `eng:pol-DATA` | 15      | 0       | 15        |
| `eng:pol-DERV` | 5       | 0       | 5         |
| `eng:pol-DEVI` | 9       | 2       | 7         |
| `eng:pol-ENVS` | 10      | 7       | 3         |
| `eng:pol-EVER` | 8       | 7       | 1         |
| `eng:pol-INCR` | 13      | 0       | 13        |
| `eng:pol-INTC` | 8       | 7       | 1         |
| `eng:pol-KNOW` | 6       | 4       | 2         |
| `eng:pol-MEXP` | 11      | 1       | 10        |
| `eng:pol-OBSV` | 10      | 9       | 1         |
| `eng:pol-PERF` | 5       | 0       | 5         |
| `eng:pol-PIPE` | 11      | 9       | 2         |
| `eng:pol-RECV` | 12      | 0       | 12        |
| `eng:pol-SCRT` | 8       | 5       | 3         |
| `eng:pol-SECD` | 8       | 2       | 6         |
| `eng:pol-TRUS` | 13      | 8       | 5         |
| `eng:pol-VURM` | 8       | 4       | 4         |
| **Total**      | **207** | **89**  | **118**   |

## Clauses

### eng:pol-A11Y

| Clause    | Level    | Covered by | Deviations                  | Controls | Pair candidate | Verdict      | Note                                                                          |
|-----------|----------|------------|-----------------------------|----------|----------------|--------------|-------------------------------------------------------------------------------|
| `UPFRONT` | MUST     | `std-A11Y` |                             |          |                | Covered      | `std-A11Y` says what each surface owes before a page is written.              |
| `CONFORM` | MUST     | `std-A11Y` |                             |          |                | Covered      | WCAG 2.2 AA is the measure, and a reviewer applies it. No check does.         |
| `VENDOR`  | MUST     | `std-A11Y` |                             |          |                | Covered      | `tol-mkdocs-material` and `tol-spectre-console` each carry an assessment.     |
| `PUBLISH` | MUST     | `std-A11Y` |                             |          |                | Covered      | A statement is owed where a law or a contract asks. PSBAR 2018 binds neither. |
| `WORSE`   | MUST NOT | `std-A11Y` |                             |          |                | Covered      | A change knowingly reducing either surface needs a recorded deviation.        |
| `ASSIST`  | SHOULD   |            | `dev-no-screen-reader-pass` |          |                | Gap          | Neither surface has been read with a screen reader.                           |
| `INCLUDE` | COULD    |            |                             |          |                | Out of scope | There is no user research here for anybody to take part in.                   |

### eng:pol-ACCS

| Clause    | Level    | Covered by               | Deviations                    | Controls                                       | Pair candidate        | Verdict             | Note                                                                          |
|-----------|----------|--------------------------|-------------------------------|------------------------------------------------|-----------------------|---------------------|-------------------------------------------------------------------------------|
| `NAMED`   | MUST     |                          | `dev-github-holds-identity`   |                                                |                       | Gap                 | Every change arrives under a named account, and no standard states it.        |
| `LEAST`   | MUST     | `std-CI`, `eng:std-CONT` |                               | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006` |                       | Covered             | A job declares the permission it needs. No container runs here.               |
| `DUTIES`  | MUST     | `std-CI`                 |                               | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006` | `eng:pol-AGNT.DUTIES` | Covered             | A person approves the `nuget.org` environment before a publish spends it.     |
| `AUTHN`   | MUST     |                          | `dev-github-holds-identity`   |                                                |                       | Gap                 | GitHub holds the authentication, and no standard says what it must be.        |
| `RECERT`  | MUST     |                          |                               |                                                |                       | Out of scope        | One maintainer holds every grant, so there is no access review to run.        |
| `REVOKE`  | MUST     |                          |                               |                                                |                       | Out of scope        | Nobody joins and nobody leaves.                                               |
| `ADMIN`   | MUST     |                          |                               |                                                |                       | Out of scope        | The administrative tooling is GitHub's settings, which record their use.      |
| `SHARED`  | MUST NOT |                          |                               |                                                | `eng:pol-EVER.SHARED` | Covered by its pair | Covered through `eng:pol-EVER.SHARED`, the same duty from the other side.     |
| `PERSIST` | MUST NOT |                          | `dev-standing-publish-rights` |                                                |                       | Gap                 | The maintainer's publish rights stand permanently, and nothing revisits them. |
| `DIRECT`  | SHOULD   |                          |                               |                                                |                       | Out of scope        | Identity is GitHub's, and it is already in one place.                         |
| `ZERO`    | COULD    |                          | `dev-standing-publish-rights` |                                                |                       | Gap                 | Publishing waits for an approval, and the rights behind it never go away.     |

### eng:pol-AGNT

| Clause    | Level    | Covered by                 | Deviations | Controls   | Pair candidate        | Verdict | Note                                                                                                                                                                                                                      |
|-----------|----------|----------------------------|------------|------------|-----------------------|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `PROV`    | MUST     | `std-PLUGIN`, `eng:std-PR` |            | `ctl-0008` |                       | Covered | A commit trailer names what produced a change. A finding an agent files names the agent, the repository and the commit it read.                                                                                           |
| `ACCEPT`  | MUST     | `eng:std-PR`               |            |            |                       | Covered | The approval is what makes the work somebody's.                                                                                                                                                                           |
| `EQUAL`   | MUST     | `eng:std-PR`               |            |            |                       | Covered | A change arrives as a pull request, whoever wrote the branch.                                                                                                                                                             |
| `CONFID`  | MUST     | `std-PLUGIN`               |            | `ctl-0008` |                       | Covered | A skill dates the export and reports an unsettled record. A discovery here states its confidence and carries an expiry date, and a finding arrives carrying both. Nothing runs `expiry-sweep`, so no observation expires. |
| `SELFVER` | MUST NOT | `std-PLUGIN`, `eng:std-PR` |            | `ctl-0008` |                       | Covered | The approver reads the change rather than the agent's account of it. A session filing a finding cannot call its own observation corroborated.                                                                             |
| `DUTIES`  | MUST NOT | `eng:std-PR`               |            |            | `eng:pol-ACCS.DUTIES` | Covered | Somebody other than the author approves it. One maintainer does both here.                                                                                                                                                |
| `UNPROV`  | MUST NOT | `std-PLUGIN`               |            | `ctl-0008` |                       | Covered | A skill names the record an answer came from. No proposal names the run behind it.                                                                                                                                        |
| `ACCESS`  | MUST NOT | `std-PLUGIN`               |            | `ctl-0008` |                       | Covered | A skill asks only to read a file. An agent still holds the maintainer's own keys.                                                                                                                                         |

### eng:pol-AUTV

| Clause    | Level    | Covered by                              | Deviations                   | Controls                                                                                                               | Pair candidate         | Verdict | Note                                                              |
|-----------|----------|-----------------------------------------|------------------------------|------------------------------------------------------------------------------------------------------------------------|------------------------|---------|-------------------------------------------------------------------|
| `INTEG`   | MUST     | `std-CONFIG`, `std-CI`, `eng:std-GATES` |                              | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0004`, `ctl-0005`, `ctl-0006`, `ctl-0007`, `ctl-0008`, `ctl-0009`, `ctl-0010` |                        | Covered | Every job runs on a pull request into `main`.                     |
| `BLOCK`   | MUST     | `std-CONFIG`, `std-CI`, `eng:std-GATES` |                              | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0004`, `ctl-0005`, `ctl-0006`, `ctl-0007`, `ctl-0008`, `ctl-0009`, `ctl-0010` |                        | Covered | The branch rule names `validate` as the check a merge waits for.  |
| `REPRO`   | MUST     |                                         | `dev-no-reproducible-build`  |                                                                                                                        |                        | Gap     | Any clone builds the tool, and no standard states the rule.       |
| `LEVELS`  | MUST     | `eng:std-NETTST`, `eng:std-TEST`        |                              | `ctl-0009`                                                                                                             |                        | Covered | Unit, behaviour and golden layers each catch a different fault.   |
| `REGRESS` | MUST     | `eng:std-GATES`                         |                              | `ctl-0001`, `ctl-0007`, `ctl-0008`, `ctl-0009`, `ctl-0010`                                                             | `eng:pol-VURM.REGRESS` | Covered | A fixed defect keeps the test that catches it.                    |
| `BROKEN`  | MUST     |                                         | `dev-branch-habits-unstated` |                                                                                                                        |                        | Gap     | Nothing says a red `main` comes before other work.                |
| `BYPASS`  | MUST NOT | `eng:std-GATES`                         |                              | `ctl-0001`, `ctl-0007`, `ctl-0008`, `ctl-0009`, `ctl-0010`                                                             |                        | Covered | A merge over a failing check needs a deviation nobody can record. |
| `DISABLE` | MUST NOT | `eng:std-GATES`                         |                              | `ctl-0001`, `ctl-0007`, `ctl-0008`, `ctl-0009`, `ctl-0010`                                                             |                        | Covered | `std-CI` adds that no job may declare `continue-on-error`.        |
| `MACHINE` | MUST NOT | `eng:std-GATES`                         |                              | `ctl-0001`, `ctl-0007`, `ctl-0008`, `ctl-0009`, `ctl-0010`                                                             |                        | Covered | Each matrix cell is a fresh runner holding its own checkout.      |
| `OFTEN`   | SHOULD   |                                         | `dev-branch-habits-unstated` |                                                                                                                        |                        | Gap     | Branch size is a habit here rather than a rule.                   |
| `WARN`    | SHOULD   | `eng:std-CSSTY`                         |                              |                                                                                                                        |                        | Covered | The analysers decide, and a suppression is local and says why.    |
| `COVER`   | SHOULD   | `eng:std-NETTST`, `eng:std-TEST`        |                              | `ctl-0009`                                                                                                             |                        | Covered | `kac-tests.cs` carries the coverage gate.                         |
| `BITWISE` | COULD    |                                         | `dev-no-reproducible-build`  |                                                                                                                        |                        | Gap     | Nothing asks two builds of the tool to produce the same bytes.    |

### eng:pol-COST

| Clause    | Level    | Covered by | Deviations | Controls | Pair candidate | Verdict      | Note                                                                                                                                                                      |
|-----------|----------|------------|------------|----------|----------------|--------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `ATTRIB`  | MUST     |            |            |          |                | Out of scope | Nothing here runs on metered infrastructure. GitHub hosts the workflows, the packages, the marketplace branch and the site for a public repository, and bills none of it. |
| `VISIBLE` | MUST     |            |            |          |                | Out of scope | Nothing here runs on metered infrastructure. GitHub hosts the workflows, the packages, the marketplace branch and the site for a public repository, and bills none of it. |
| `WEIGHED` | MUST     |            |            |          |                | Out of scope | Nothing here runs on metered infrastructure. GitHub hosts the workflows, the packages, the marketplace branch and the site for a public repository, and bills none of it. |
| `SIZING`  | MUST     |            |            |          |                | Out of scope | Nothing here runs on metered infrastructure. GitHub hosts the workflows, the packages, the marketplace branch and the site for a public repository, and bills none of it. |
| `ANOMALY` | MUST     |            |            |          |                | Out of scope | Nothing here runs on metered infrastructure. GitHub hosts the workflows, the packages, the marketplace branch and the site for a public repository, and bills none of it. |
| `UNUSED`  | MUST     |            |            |          |                | Out of scope | Nothing here runs on metered infrastructure. GitHub hosts the workflows, the packages, the marketplace branch and the site for a public repository, and bills none of it. |
| `UNOWNED` | MUST NOT |            |            |          |                | Out of scope | Nothing here runs on metered infrastructure. GitHub hosts the workflows, the packages, the marketplace branch and the site for a public repository, and bills none of it. |
| `ERODE`   | MUST NOT |            |            |          |                | Out of scope | Nothing here runs on metered infrastructure. GitHub hosts the workflows, the packages, the marketplace branch and the site for a public repository, and bills none of it. |

### eng:pol-DATA

| Clause    | Level    | Covered by | Deviations | Controls | Pair candidate        | Verdict             | Note                                                                                                                                                                                                                       |
|-----------|----------|------------|------------|----------|-----------------------|---------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `CLASS`   | MUST     |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `CRYPTO`  | MUST     |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `RETIRE`  | MUST     |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `LAWFUL`  | MUST     |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `MINIMAL` | MUST     |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `RIGHTS`  | MUST     |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `LOCATE`  | MUST     |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `XBORDER` | MUST     |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `DELETE`  | MUST     |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `UNMASK`  | MUST NOT |            |            |          | `eng:pol-ENVS.UNMASK` | Covered by its pair | Covered through `eng:pol-SCRT.REUSE`: the test data here is never a real customer's.                                                                                                                                       |
| `SHARE`   | MUST NOT |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `LINGER`  | MUST NOT |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `LOGS`    | MUST NOT |            |            |          | `eng:pol-SCRT.LOGS`   | Covered by its pair | Covered through `eng:pol-SCRT.LOGS`: both forbid a step to print a secret.                                                                                                                                                 |
| `AGILE`   | COULD    |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |
| `CLEAR`   | COULD    |            |            |          |                       | Out of scope        | Nothing here holds data on anybody's behalf. Every file is public by design, and the only personal data is the name and address a contributor puts in a commit, which GitHub publishes as that account holder asked it to. |

### eng:pol-DERV

| Clause    | Level    | Covered by | Deviations                        | Controls | Pair candidate | Verdict | Note                                                                                  |
|-----------|----------|------------|-----------------------------------|----------|----------------|---------|---------------------------------------------------------------------------------------|
| `EXPECT`  | MUST     |            | `dev-generated-output-ungoverned` |          |                | Gap     | `generate --check` regenerates and compares, and no standard states that as the rule. |
| `CHECK`   | MUST     |            | `dev-generated-output-ungoverned` |          |                | Gap     | ctl-0007 runs that check on every pull request, for `eng:std-GATES` rather than this. |
| `RUNLOG`  | MUST     |            | `dev-generated-output-ungoverned` |          |                | Gap     | The run leaves a workflow log, and nothing keeps its inputs beside its output.        |
| `FAILED`  | MUST NOT |            | `dev-generated-output-ungoverned` |          |                | Gap     | A stale generated file fails the gate, and no standard says the output is unusable.   |
| `LINEAGE` | SHOULD   |            | `dev-generated-output-ungoverned` |          |                | Gap     | A generated block names itself, and not the records it was computed from.             |

### eng:pol-DEVI

| Clause    | Level    | Covered by      | Deviations                | Controls                                                   | Pair candidate | Verdict | Note                                                                       |
|-----------|----------|-----------------|---------------------------|------------------------------------------------------------|----------------|---------|----------------------------------------------------------------------------|
| `RECORD`  | MUST     |                 | `dev-deviations-unstated` |                                                            |                | Gap     | The register exists. No standard says a record comes before the departure. |
| `OWNER`   | MUST     | `eng:std-GATES` |                           | `ctl-0001`, `ctl-0007`, `ctl-0008`, `ctl-0009`, `ctl-0010` |                | Covered | A skipped check names the person who accepted it.                          |
| `CONTENT` | MUST     |                 | `dev-deviations-unstated` |                                                            |                | Gap     | The schema requires four sections. No standard states them.                |
| `EXPIRY`  | MUST     | `eng:std-GATES` |                           | `ctl-0001`, `ctl-0007`, `ctl-0008`, `ctl-0009`, `ctl-0010` |                | Covered | A skipped check carries the date it is revisited.                          |
| `SURFACE` | MUST     |                 | `dev-deviations-unstated` |                                                            |                | Gap     | The register is published, and no standard says it has to be.              |
| `CLOSE`   | MUST     |                 | `dev-deviations-unstated` |                                                            |                | Gap     | Every record here is open, and no standard says what closing takes.        |
| `PERM`    | MUST NOT |                 | `dev-deviations-unstated` |                                                            |                | Gap     | Every record carries a review date, and `expiry` warns once one goes by.   |
| `CUSTOM`  | MUST NOT |                 | `dev-deviations-unstated` |                                                            |                | Gap     | This map did exactly that, once. No standard asks for it again.            |
| `DEBT`    | SHOULD   |                 | `dev-deviations-unstated` |                                                            |                | Gap     | A shortcut becomes an issue on the tracker, and no standard requires that. |

### eng:pol-ENVS

| Clause    | Level    | Covered by       | Deviations | Controls   | Pair candidate        | Verdict      | Note                                                                      |
|-----------|----------|------------------|------------|------------|-----------------------|--------------|---------------------------------------------------------------------------|
| `SPLIT`   | MUST     |                  |            |            |                       | Out of scope | There is no tier below production to hold apart.                          |
| `CREDS`   | MUST     |                  |            |            |                       | Out of scope | There is no lower environment for a production secret to reach.           |
| `SAMEDEF` | MUST     | `eng:std-DEPLOY` |            |            |                       | Covered      | One artefact is promoted rather than rebuilt. Nothing here is promoted.   |
| `BASELIN` | MUST     | `eng:std-CONT`   |            |            |                       | Covered      | The base image is chosen and pinned. No image runs here.                  |
| `PROMOTE` | MUST     | `eng:std-DEPLOY` |            |            |                       | Covered      | Production changes through the pipeline alone.                            |
| `MASK`    | MUST     | `eng:std-TEST`   |            | `ctl-0009` |                       | Covered      | The test data is never a real customer's. Every fixture here is invented. |
| `DEBUG`   | MUST NOT | `eng:std-TEST`   |            | `ctl-0009` |                       | Covered      | The same rule. There is no production system to debug against.            |
| `REUSE`   | MUST NOT | `eng:std-SECRET` |            |            | `eng:pol-SCRT.REUSE`  | Covered      | An environment below production holds its own secrets.                    |
| `UNMASK`  | MUST NOT | `eng:std-TEST`   |            | `ctl-0009` | `eng:pol-DATA.UNMASK` | Covered      | The same rule as `MASK`.                                                  |
| `EPHEM`   | SHOULD   |                  |            |            |                       | Out of scope | Every CI job is a fresh runner already.                                   |

### eng:pol-EVER

| Clause    | Level    | Covered by                            | Deviations             | Controls                                       | Pair candidate        | Verdict | Note                                                            |
|-----------|----------|---------------------------------------|------------------------|------------------------------------------------|-----------------------|---------|-----------------------------------------------------------------|
| `ASSETS`  | MUST     | `std-CONFIG`, `eng:std-VCS`           |                        | `ctl-0004`, `ctl-0005`                         |                       | Covered | Every value the build reads is committed.                       |
| `HISTORY` | MUST     | `eng:std-VCS`                         |                        |                                                |                       | Covered | Every change is attributable and reviewed.                      |
| `INTENT`  | MUST     | `eng:std-PR`, `eng:std-VCS`           |                        |                                                |                       | Covered | A pull request carries the reasoning behind the change.         |
| `BRANCH`  | MUST     | `std-CI`, `eng:std-PR`, `eng:std-VCS` |                        | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006` |                       | Covered | A push to `main` is rejected.                                   |
| `PARITY`  | MUST     | `std-CONFIG`, `eng:std-VCS`           |                        | `ctl-0004`, `ctl-0005`                         |                       | Covered | A YAML file and a workflow answer to the same gate as the code. |
| `ORPHAN`  | MUST NOT | `std-CONFIG`, `eng:std-VCS`           |                        | `ctl-0004`, `ctl-0005`                         |                       | Covered | A value living in more than one tree is copied and proved.      |
| `SHARED`  | MUST NOT | `eng:std-VCS`                         |                        |                                                | `eng:pol-ACCS.SHARED` | Covered | No shared account exists here to lose attribution to.           |
| `SIGNED`  | COULD    |                                       | `dev-unsigned-commits` |                                                |                       | Gap     | Commits are not signed.                                         |

### eng:pol-INCR

| Clause    | Level    | Covered by | Deviations                        | Controls | Pair candidate         | Verdict      | Note                                                                            |
|-----------|----------|------------|-----------------------------------|----------|------------------------|--------------|---------------------------------------------------------------------------------|
| `PROCESS` | MUST     |            | `dev-no-incident-process`         |          |                        | Gap          | The runbooks cover three failures, and nothing says who decides in the rest.    |
| `TRIAGE`  | MUST     |            |                                   |          |                        | Out of scope | One maintainer, and nobody to escalate to.                                      |
| `COMMS`   | MUST     |            | `dev-no-incident-process`         |          |                        | Gap          | Whoever installed a bad version hears nothing until the next one lands.         |
| `RECOVER` | MUST     |            | `dev-no-incident-process`         |          |                        | Gap          | `std-VERS` says a correction ships as a new version, for `eng:pol-PIPE.REVERT`. |
| `EVIDENC` | MUST     |            | `dev-nothing-learns-from-a-fault` |          |                        | Gap          | The corpus adopted no type that holds an incident record.                       |
| `NOTIFY`  | MUST     |            |                                   |          |                        | Out of scope | No personal data, so no breach to report to a supervisory authority.            |
| `INFORM`  | MUST     |            |                                   |          |                        | Out of scope | Same: there is nobody whom a breach here could put at risk.                     |
| `REPORT`  | MUST     |            | `dev-no-incident-process`         |          |                        | Gap          | `.github/SECURITY.md` gives the route, and no standard names it.                |
| `LEARN`   | MUST     |            | `dev-nothing-learns-from-a-fault` |          |                        | Gap          | A fix lands and nothing asks what allowed the fault.                            |
| `ACTIONS` | MUST     |            | `dev-nothing-learns-from-a-fault` |          | `eng:pol-SECD.ACTIONS` | Gap          | Paired with `eng:pol-SECD.ACTIONS`, which no standard covers either.            |
| `DRILL`   | MUST     |            | `dev-no-incident-process`         |          |                        | Gap          | The publish path is first exercised for real, every time.                       |
| `ADHOC`   | MUST NOT |            | `dev-no-incident-process`         |          |                        | Gap          | Nothing here has to be handled formally, so everything is handled informally.   |
| `TOOSOON` | MUST NOT |            | `dev-nothing-learns-from-a-fault` |          |                        | Gap          | Nothing holds an incident open until the learning is written down.              |

### eng:pol-INTC

| Clause    | Level    | Covered by    | Deviations                 | Controls | Pair candidate | Verdict | Note                                                                   |
|-----------|----------|---------------|----------------------------|----------|----------------|---------|------------------------------------------------------------------------|
| `SPEC`    | MUST     | `eng:std-API` |                            |          |                | Covered | The contract is the source of truth. `.schema/` is that contract here. |
| `VERSION` | MUST     | `eng:std-API` |                            |          |                | Covered | A change carries a version and a notice.                               |
| `DEPREC`  | MUST     |               | `dev-export-has-no-notice` |          |                | Gap     | No standard says how much notice a consumer of the export gets.        |
| `NOTICE`  | MUST     | `eng:std-API` |                            |          |                | Covered | The same rule as `VERSION`.                                            |
| `SECURE`  | MUST     | `eng:std-API` |                            |          |                | Covered | Every endpoint authenticates and validates. Nothing here listens.      |
| `HOLDS`   | MUST     | `eng:std-API` |                            |          |                | Covered | ctl-0008 reads a published corpus back and compares it.                |
| `BREAK`   | MUST NOT | `eng:std-API` |                            |          |                | Covered | A break carries a version increment and a notice.                      |
| `EXPOSE`  | MUST NOT | `eng:std-API` |                            |          |                | Covered | Everything here is public, so no interface hides anything.             |

### eng:pol-KNOW

| Clause   | Level    | Covered by   | Deviations                      | Controls   | Pair candidate | Verdict | Note                                                                       |
|----------|----------|--------------|---------------------------------|------------|----------------|---------|----------------------------------------------------------------------------|
| `DOCS`   | MUST     | `std-VERS`   |                                 | `ctl-0007` |                | Covered | Every stamp is named, and what a move of each one says is written down.    |
| `SYNC`   | MUST     | `std-VERS`   |                                 | `ctl-0007` |                | Covered | A producer's move and its consumers' locks land in one pull request.       |
| `DECIDE` | MUST     |              | `dev-decisions-live-in-commits` |            |                | Gap     | The reasoning behind a decision lives in the commit that made it.          |
| `AGENTS` | MUST     | `std-PROSE`  |                                 |            |                | Covered | The rules sit where the agents doing the work read them.                   |
| `HEADS`  | MUST NOT |              | `dev-one-maintainer`            |            |                | Gap     | One maintainer, and nothing tests what only they know.                     |
| `COPY`   | MUST NOT | `std-PLUGIN` |                                 | `ctl-0008` |                | Covered | A skill quotes what the export carried, and links the record for the rest. |

### eng:pol-MEXP

| Clause    | Level    | Covered by     | Deviations | Controls | Pair candidate        | Verdict      | Note                                                                                                                                                                                                      |
|-----------|----------|----------------|------------|----------|-----------------------|--------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `SEGMENT` | MUST     |                |            |          |                       | Out of scope | There is no network here to control. `kac` reads a folder and reaches a registry only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates. |
| `DENY`    | MUST     |                |            |          |                       | Out of scope | There is no network here to control. `kac` reads a folder and reaches a registry only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates. |
| `TRANSIT` | MUST     |                |            |          |                       | Out of scope | There is no network here to control. `kac` reads a folder and reaches a registry only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates. |
| `PEERID`  | MUST     |                |            |          |                       | Out of scope | There is no network here to control. `kac` reads a folder and reaches a registry only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates. |
| `PRIVATE` | MUST     |                |            |          |                       | Out of scope | There is no network here to control. `kac` reads a folder and reaches a registry only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates. |
| `EGRESS`  | MUST     |                |            |          |                       | Out of scope | There is no network here to control. `kac` reads a folder and reaches a registry only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates. |
| `ASCODE`  | MUST     |                |            |          | `eng:pol-PIPE.ASCODE` | Out of scope | There is no network here to control. `kac` reads a folder and reaches a registry only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates. |
| `PUBLIC`  | MUST NOT | `eng:std-CONT` |            |          |                       | Covered      | A container holds only what the service needs. No container runs here.                                                                                                                                    |
| `LATERAL` | MUST NOT |                |            |          |                       | Out of scope | There is no network here to control. `kac` reads a folder and reaches a registry only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates. |
| `WEAKEN`  | MUST NOT |                |            |          |                       | Out of scope | There is no network here to control. `kac` reads a folder and reaches a registry only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates. |
| `ZEROTR`  | COULD    |                |            |          |                       | Out of scope | There is no network here to control. `kac` reads a folder and reaches a registry only when somebody runs `restore` or `pack`, and GitHub serves the site and the packages over connections it terminates. |

### eng:pol-OBSV

| Clause    | Level    | Covered by    | Deviations | Controls | Pair candidate | Verdict      | Note                                                                                |
|-----------|----------|---------------|------------|----------|----------------|--------------|-------------------------------------------------------------------------------------|
| `CENTRAL` | MUST     | `eng:std-OBS` |            |          |                | Covered      | Everything lands in the central store.                                              |
| `CLOCKS`  | MUST     | `eng:std-OBS` |            |          |                | Covered      | One request reads as one timeline.                                                  |
| `RETAIN`  | MUST     | `eng:std-OBS` |            |          |                | Covered      | The store keeps telemetry for a stated period. GitHub keeps the logs.               |
| `HEALTH`  | MUST     | `eng:std-OBS` |            |          |                | Covered      | A service is monitored and an owner hears when it degrades.                         |
| `SECMON`  | MUST     |               |            |          |                | Out of scope | No standard reaches security monitoring, and nothing here emits an event.           |
| `ALERTS`  | MUST     | `eng:std-OBS` |            |          |                | Covered      | Somebody acts on every alert.                                                       |
| `BLIND`   | MUST NOT | `eng:std-OBS` |            |          |                | Covered      | A service with no monitoring does not ship.                                         |
| `SECRETS` | MUST NOT | `eng:std-OBS` |            |          |                | Covered      | Telemetry carries no personal data, and `std-CI` adds that no step prints a secret. |
| `SLO`     | SHOULD   | `eng:std-OBS` |            |          |                | Covered      | What good looks like is written down and watched.                                   |
| `CORREL`  | SHOULD   | `eng:std-OBS` |            |          |                | Covered      | One id ties a request together across systems.                                      |

### eng:pol-PERF

| Clause    | Level    | Covered by | Deviations | Controls | Pair candidate | Verdict      | Note                                                                                                                                      |
|-----------|----------|------------|------------|----------|----------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| `TARGETS` | MUST     |            |            |          |                | Out of scope | Nothing here is performance-sensitive. No corpus is large enough for its size to matter, and no load arrives that somebody did not start. |
| `MEASURE` | MUST     |            |            |          |                | Out of scope | Nothing here is performance-sensitive. No corpus is large enough for its size to matter, and no load arrives that somebody did not start. |
| `DEFECT`  | MUST     |            |            |          |                | Out of scope | Nothing here is performance-sensitive. No corpus is large enough for its size to matter, and no load arrives that somebody did not start. |
| `PEAK`    | MUST     |            |            |          |                | Out of scope | Nothing here is performance-sensitive. No corpus is large enough for its size to matter, and no load arrives that somebody did not start. |
| `NOTEST`  | MUST NOT |            |            |          |                | Out of scope | Nothing here is performance-sensitive. No corpus is large enough for its size to matter, and no load arrives that somebody did not start. |

### eng:pol-PIPE

| Clause    | Level    | Covered by                     | Deviations | Controls                                       | Pair candidate        | Verdict      | Note                                                                         |
|-----------|----------|--------------------------------|------------|------------------------------------------------|-----------------------|--------------|------------------------------------------------------------------------------|
| `DEPLOY`  | MUST     | `std-CI`, `eng:std-DEPLOY`     |            | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006` |                       | Covered      | Every publish runs from a workflow, and nobody publishes by hand.            |
| `SAMEART` | MUST     | `eng:std-DEPLOY`               |            |                                                |                       | Covered      | The artefact is built once. The publish job rebuilds from the merge commit.  |
| `CONFIG`  | MUST     | `std-CONFIG`, `eng:std-DEPLOY` |            | `ctl-0004`, `ctl-0005`                         |                       | Covered      | Configuration sits outside the artefact.                                     |
| `TRACE`   | MUST     | `std-CI`, `eng:std-DEPLOY`     |            | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006` | `eng:pol-TRUS.TRACE`  | Covered      | A publish tags the commit it published from.                                 |
| `REVERT`  | MUST     | `std-VERS`, `eng:std-DEPLOY`   |            | `ctl-0007`                                     |                       | Covered      | A correction ships as a new version, and no published one moves.             |
| `ASCODE`  | MUST     | `std-CI`, `eng:std-DEPLOY`     |            | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006` | `eng:pol-MEXP.ASCODE` | Covered      | The workflows are reviewed like any other file.                              |
| `GATES`   | MUST     | `std-CI`                       |            | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006` |                       | Covered      | `validate` is the check a merge waits for.                                   |
| `FLAGS`   | MUST     |                                |            |                                                |                       | Out of scope | Nothing here carries a flag that changes behaviour in production.            |
| `MANUAL`  | MUST NOT | `std-CI`, `eng:std-DEPLOY`     |            | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006` |                       | Covered      | Nobody edits a published branch, package or release by hand.                 |
| `LOCAL`   | MUST NOT | `std-CI`, `eng:std-DEPLOY`     |            | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006` |                       | Covered      | The publishing job builds what it publishes.                                 |
| `PROGDEL` | COULD    |                                |            |                                                |                       | Out of scope | A version is published whole, and there is nothing to release progressively. |

### eng:pol-RECV

| Clause    | Level    | Covered by | Deviations | Controls | Pair candidate | Verdict      | Note                                                                                                                                                                                                         |
|-----------|----------|------------|------------|----------|----------------|--------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `RTORPO`  | MUST     |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `BACKUP`  | MUST     |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `RESTORE` | MUST     |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `OFFSITE` | MUST     |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `TIMEOUT` | MUST     |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `DEGRADE` | MUST     |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `IDEMPOT` | MUST     |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `UNTEST`  | MUST NOT |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `RETRY`   | MUST NOT |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `REDUND`  | SHOULD   |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `SHED`    | SHOULD   |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |
| `CHAOS`   | COULD    |            |            |          |                | Out of scope | Nothing here serves a request, so nothing degrades, retries or sheds load. What there is to recover is a git repository GitHub holds and every clone copies, and a published version that cannot be changed. |

### eng:pol-SCRT

| Clause    | Level    | Covered by                               | Deviations                        | Controls                                                               | Pair candidate       | Verdict             | Note                                                                     |
|-----------|----------|------------------------------------------|-----------------------------------|------------------------------------------------------------------------|----------------------|---------------------|--------------------------------------------------------------------------|
| `STORE`   | MUST     | `std-CI`, `eng:std-SECRET`               |                                   | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006`                         |                      | Covered             | Every credential comes from the store or from an exchange.               |
| `ROTATE`  | MUST     | `std-CI`, `eng:std-SECRET`               |                                   | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006`                         |                      | Covered             | A job exchanges its identity for a short-lived key.                      |
| `KEYS`    | MUST     |                                          |                                   |                                                                        |                      | Out of scope        | There is no key or certificate here to hold through a lifecycle.         |
| `LEAKED`  | MUST     | `eng:std-SECRET`                         |                                   |                                                                        |                      | Covered             | A pipeline runs a secret scanner over the history. No job here does.     |
| `EMBED`   | MUST NOT | `std-CONFIG`, `std-CI`, `eng:std-SECRET` |                                   | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0004`, `ctl-0005`, `ctl-0006` |                      | Covered             | No secret sits in a workflow, a configuration file or an artefact.       |
| `REUSE`   | MUST NOT |                                          |                                   |                                                                        | `eng:pol-ENVS.REUSE` | Covered by its pair | Covered through `eng:pol-ENVS.REUSE`, the same duty from the other side. |
| `LOGS`    | MUST NOT | `std-CI`, `eng:std-SECRET`               |                                   | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006`                         | `eng:pol-DATA.LOGS`  | Covered             | A step passes a secret through `env:` and never prints one.              |
| `ZEROSEC` | COULD    |                                          | `dev-trusted-publishing-unstated` |                                                                        |                      | Gap                 | Trusted publishing removed the last static key, and no standard says so. |

### eng:pol-SECD

| Clause    | Level    | Covered by      | Deviations                        | Controls | Pair candidate         | Verdict      | Note                                                                           |
|-----------|----------|-----------------|-----------------------------------|----------|------------------------|--------------|--------------------------------------------------------------------------------|
| `REQS`    | MUST     |                 | `dev-security-design-unstated`    |          |                        | Gap          | A security requirement arrives as a review comment rather than a requirement.  |
| `DESIGN`  | MUST     |                 | `dev-security-design-unstated`    |          |                        | Gap          | The workflows fail closed and deny by default, stated by no standard.          |
| `THREAT`  | MUST     |                 | `dev-security-design-unstated`    |          |                        | Gap          | `.github/SECURITY.md` says how CI contains what it runs, named by no standard. |
| `IMPACT`  | MUST     |                 |                                   |          |                        | Out of scope | No processing of personal data, so nothing to assess the impact of.            |
| `ACTIONS` | MUST     |                 | `dev-nothing-learns-from-a-fault` |          | `eng:pol-INCR.ACTIONS` | Gap          | Paired with `eng:pol-INCR.ACTIONS`, which no standard covers either.           |
| `CODING`  | MUST     | `eng:std-CSSTY` |                                   |          |                        | Covered      | The conventions come from one file, and the analysers enforce them.            |
| `CODEREV` | MUST     | `eng:std-PR`    |                                   |          |                        | Covered      | Somebody other than the author approves it. One maintainer does both here.     |
| `HIRISK`  | MUST NOT |                 | `dev-security-design-unstated`    |          |                        | Gap          | Nothing sorts a change by risk before it is built.                             |

### eng:pol-TRUS

| Clause    | Level    | Covered by                                             | Deviations                 | Controls                                                               | Pair candidate       | Verdict             | Note                                                                      |
|-----------|----------|--------------------------------------------------------|----------------------------|------------------------------------------------------------------------|----------------------|---------------------|---------------------------------------------------------------------------|
| `INVENT`  | MUST     | `std-CONFIG`, `eng:std-DEPS`                           |                            | `ctl-0004`, `ctl-0005`                                                 |                      | Covered             | A pin names an exact version, and `tools/` names what is chosen.          |
| `SCREEN`  | MUST     | `eng:std-DEPS`                                         |                            |                                                                        |                      | Covered             | A new package is screened before the pull request adding it merges.       |
| `LICENCE` | MUST     | `eng:std-DEPS`                                         |                            |                                                                        |                      | Covered             | A licence is checked against the allowed list before adoption.            |
| `MALWARE` | MUST     |                                                        | `dev-package-unscanned`    |                                                                        |                      | Gap                 | Nothing scans the package the tool ships.                                 |
| `SOURCE`  | MUST     | `std-CONFIG`, `std-CI`, `eng:std-CONT`, `eng:std-DEPS` |                            | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0004`, `ctl-0005`, `ctl-0006` |                      | Covered             | An action is pinned to a commit, and a package comes from nuget.org.      |
| `CLOUD`   | MUST     |                                                        | `dev-github-concentration` |                                                                        |                      | Gap                 | GitHub holds most of the responsibility here, and no standard divides it. |
| `EXIT`    | MUST     |                                                        | `dev-github-concentration` |                                                                        |                      | Gap                 | Nothing says how this would leave GitHub.                                 |
| `REPO`    | MUST     | `std-CI`, `eng:std-DEPS`                               |                            | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006`                         |                      | Covered             | Every artefact is a version on a registry that keeps it.                  |
| `TRACE`   | MUST     |                                                        |                            |                                                                        | `eng:pol-PIPE.TRACE` | Covered by its pair | Covered through `eng:pol-PIPE.TRACE`, the same duty from the other side.  |
| `REVIEW`  | MUST     | `std-CONFIG`                                           |                            | `ctl-0004`, `ctl-0005`                                                 |                      | Covered             | Dependabot brings each pinned version back weekly.                        |
| `UNTRUST` | MUST NOT | `std-CI`, `eng:std-DEPS`                               |                            | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006`                         |                      | Covered             | A moving version may not enter a job holding a write permission.          |
| `MUTATE`  | MUST NOT | `std-VERS`, `std-CI`, `eng:std-CONT`                   |                            | `ctl-0001`, `ctl-0002`, `ctl-0003`, `ctl-0006`, `ctl-0007`             |                      | Covered             | A published tag never moves.                                              |
| `ATTEST`  | SHOULD   |                                                        | `dev-package-unscanned`    |                                                                        |                      | Gap                 | Nothing proves the origin of an artefact before it is installed.          |

### eng:pol-VURM

| Clause    | Level    | Covered by     | Deviations                         | Controls | Pair candidate         | Verdict             | Note                                                                       |
|-----------|----------|----------------|------------------------------------|----------|------------------------|---------------------|----------------------------------------------------------------------------|
| `SCAN`    | MUST     | `eng:std-DEPS` |                                    |          |                        | Covered             | The dependency tree is scanned on every build and weekly.                  |
| `RANK`    | MUST     | `eng:std-DEPS` |                                    |          |                        | Covered             | A finding is ranked by severity and by whether the path runs.              |
| `TIMEBOX` | MUST     | `eng:std-DEPS` |                                    |          |                        | Covered             | Critical closes in 7 days and high in 30. Nothing here tracks that.        |
| `DISCLOS` | MUST     |                | `dev-vulnerability-route-unstated` |          |                        | Gap                 | A private advisory is the route in, and no standard names it.              |
| `REGRESS` | MUST     |                |                                    |          | `eng:pol-AUTV.REGRESS` | Covered by its pair | Covered through `eng:pol-AUTV.REGRESS`, the same duty from the other side. |
| `SHIP`    | MUST NOT | `eng:std-DEPS` |                                    |          |                        | Covered             | A release with an open critical finding needs a recorded deviation.        |
| `OVERDUE` | MUST NOT |                | `dev-vulnerability-route-unstated` |          |                        | Gap                 | No standard names who is accountable past the window.                      |
| `INDEP`   | SHOULD   |                |                                    |          |                        | Out of scope        | One maintainer, and nobody else to test what they built.                   |

## What this leaves open

**The tool prints two verdicts and a person writes the rest.** It prints `covered` and `uncovered`, and splitting
`uncovered` into a gap and something out of scope is a judgement about this estate. That judgement, and the `Note`
beside it, belong to whoever confirms this report.

