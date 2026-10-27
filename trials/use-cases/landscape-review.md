# The use cases against the landscape

This review reads the use cases in this folder against the [landscape review](../landscape/runs/2026-10-27.md) of
2026-10-27. It answers three questions for the trial plan in
[#1164](https://github.com/paul80nd/knowledge-as-code/issues/1164): whether each use case is a problem adopters
commonly have, which common problem is missing, and which use cases the trial should test.

A corpus is one repository of knowledge records. A record is one Markdown document in it, with a declared type. The
plugin is the corpus packaged for Claude Code.

## Each moment is a problem other offerings solve

Every moment has a family in the landscape built to solve the same problem. That is the evidence each one is common.

| Moment     | The problem adopters have                                      | Who else solves it                                         | Scenarios |
|------------|----------------------------------------------------------------|------------------------------------------------------------|----------:|
| design     | a design repeats a decision already made, or breaks one        | ADR tooling: MADR, adr-tools, Log4brains                   |         5 |
| build      | an agent writing code ignores the house rules                  | Cursor rules, Claude Code skills, Windsurf and Cline rules |         7 |
| review     | a reviewer cannot say which rule a change breaks               | Cortex and OpsLevel checks, which pass or fail an entity   |         4 |
| operate    | whoever is on call cannot find the owner or the known fix      | Backstage, Cortex and OpsLevel catalogues                  |         5 |
| contribute | what a session learns is lost, or written down unreviewed      | the MCP memory server, which writes with no review         |         3 |
| assure     | an auditor cannot tell which rule is enforced, or by what      | OSCAL and Compliance Trestle                               |         3 |
| learn      | a newcomer cannot find what the estate offers or what it means | TechDocs, OpsLevel Tech Docs, OKF's generated indexes      |         3 |

Two scenarios are narrower than their moment:

* **A process written as outcomes draws no finding asking for a step** tests this repository's own processes. The
  problem behind it is common, since a process nobody follows is the reason runbook and process tools exist. The case
  measures one corpus's pull requests, so the adopting corpus can run it only once it has a process of its own.
* **A design that would use a retired integration learns its successor**, and **a new reader learns what the estate
  offers, and what is not live yet**, test a lifecycle status. Backstage's `lifecycle` and OpsLevel's `lifecycle` are
  the same idea, so the problem is common. Each tests a type the adopting corpus may not take.

## What no scenario tests

Every scenario tests delivery: whether a reader or an agent receives the right record and acts on it. The landscape
review found `kac`'s lead elsewhere. It is the only offering read that checks one record against another before
merge. [Design intent](../../docs/framework/intent.md) opens on the same claim: a corpus is "validated, identified and
versioned Markdown". No scenario tests that it is validated, or that it is versioned.

Two problems are missing.

1. **A change that breaks the corpus fails before it merges.** An author cites a clause that does not exist, drops a
   rule another record cites, or lets a review date pass. This is the problem every validator in the landscape solves
   in part, and none solves across records. The capture trial's fault log already counts these faults, and #1164 now
   names it as the trial's second claim. No scenario states the claim or its threshold.
2. **A team takes a new version of the corpus it consumes, and learns what the new version broke.** An OSCAL profile
   imports a catalogue with no version, and a Claude Code plugin dependency takes a range and checks nothing. A
   `consumes:` range with a lock, and a citation that fails when the producer drops its target, is where `kac` leads.
   No trial measures it.

The scenario tags cannot describe either one yet. `@intent` says what an agent does with a type: follow, flow, nudge,
respect or inform. A scenario about `kac validate` or `kac restore` tests the tool, and no intent fits.

## The baselines for the build scenarios measure the plugin nobody installs

**A rule filed under another topic reaches the session** and **a tracing rule reaches a session writing an outbound
call** give the plugin arm's baseline, 0 of 3 for each. The plugin arm runs with the build-index hook silent. The hook
arm is the plugin that ships, and it complied in 3 of 3 on `pricing-call`, as #1020 records. Both scenarios read as
failing where the shipping plugin passes one of them. #1020 corrects the plugin trial's own lead, and these two
baselines restate it.

## What the trial should test

The trial runs in the adopting corpus, on its own team's tasks (#898). The types it adopts decide most of the list,
and #834 decides those after the capture trial's second reading.

| Priority           | Scenarios                                                                                                                                            | Why                                                                                     |
|--------------------|------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------|
| Test in any case   | every build scenario on `standards` and `policies`, and both review scenarios that cite a broken MUST                                                | the problem every agent context tool targets, and the one the team's own tasks will hit |
| Test in any case   | the missing scenario 1, a change that breaks the corpus fails before it merges                                                                       | the lead; the capture trial's fault log measures it                                     |
| Test where adopted | the design scenarios on `adrs` and `tools`, the operate scenarios on `fixes` and `runbooks`, and the assure scenarios on `controls` and `deviations` | each is common, and each needs its type in the adopting corpus                          |
| Test where adopted | the missing scenario 2, a new version of the consumed corpus                                                                                         | needs the adopting corpus to consume one, as a team corpus consumes an organisation's   |
| Test where adopted | the operate scenarios excluded for the sandbox: the runbook's steps, and the owner from the service record                                           | the adopting team's machines may allow what the sandbox blocks                          |
| Already planned    | the learn scenarios                                                                                                                                  | the newcomer trial (#897) runs them with somebody in the adopting team                  |
| Leave to this repo | the process-level scenario                                                                                                                           | it measures this repository's processes, and the adopting corpus has none yet           |

## What was decided

The developer decided these on 2026-10-27, reviewing this page.

* **The two missing scenarios join the feature files as `@proposed`.** A change that breaks the corpus sits in
  `contribute.feature`, measured by the capture trial's fault log. A new version of a consumed corpus sits in
  `assure.feature`, because its reader is the corpus owner.
* **A scenario that tests the tool takes `@check:<command>` in place of `@intent`.** No intent fits a scenario with no
  agent in it, and a new intent value would change [design intent](../../docs/framework/intent.md).
* **The two build baselines move with #1020.** That issue moves the plugin trial's lead to the hook arm, and these
  baselines restate it.
