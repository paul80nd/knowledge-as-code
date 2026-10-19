---
name: i-want-to
description: Route a piece of work in this repository to the process record or playbook with its steps, and to the writing skill for its surface. Load it before planning any change here, however the request is phrased. Where no row fits the work, you work without a process.
---

# I want to

Match the work to a row below. Where one fits, open what it names, and copy those steps into your task list
**before** you plan anything task-specific. The failure this prevents is reading the steps and then writing a bespoke
plan that quietly drops them.

**A step you decide not to do stays in the list**, with `skip:` and one line saying why. Dropping it silently is not
allowed.

**Where no row fits, say so and work without one.** Forcing a task into the nearest one is worse than carrying none.

**Count first, from the files, whatever the steps say.** A request carries a claim about the corpus, and the claim
is often wrong. A folder it names may hold no records at all. A mark said to be everywhere may be a handful, each one
the form a rule deliberately keeps. Establish that the thing exists and how much of it there is before planning what to
do about it, and report the count where it answers the request on its own.

## Where the steps are

Most of these are process records in [`examples/dog-fooding`](../../../examples/dog-fooding/processes/), the corpus
this repository keeps about itself, and each cites the standard its steps stand on.

| I want to                                                | Read                                                                                      |
|----------------------------------------------------------|-------------------------------------------------------------------------------------------|
| write a record, or rewrite one                           | [prc-add-a-record](../../../examples/dog-fooding/processes/add-a-record.md)               |
| review records against the rules                         | [reviewing-records](playbooks/reviewing-records.md)                                       |
| rework a type against public exemplars                   | [reworking-a-type](playbooks/reworking-a-type.md)                                         |
| change the schema, or write a rule                       | [prc-change-the-schema](../../../examples/dog-fooding/processes/change-the-schema.md)     |
| change `kac`, or add a check                             | [prc-change-the-tool](../../../examples/dog-fooding/processes/change-the-tool.md)         |
| write a feature spec for behaviour `kac` already has     | [feature specs](../../../tooling/CLAUDE.md#the-feature-specs-pin-more-than-findings)      |
| change the docs build, or a test, and leave `kac` alone  | [prc-pull-request](../../../examples/dog-fooding/processes/pull-request.md)               |
| add a knowledge type                                     | [prc-add-a-type](../../../examples/dog-fooding/processes/add-a-type.md)                   |
| bring a folder's prose into line with the writing rules  | [prc-sweep-prose](../../../examples/dog-fooding/processes/sweep-prose.md)                 |
| publish a report, or bring one up to date                | [writing-a-report](../writing-a-report/SKILL.md)                                          |
| triage the findings filed here, or draft one             | [harvest-findings](../harvest-findings/SKILL.md)                                          |
| review and tidy a lane of the project board              | [prc-review-a-board-lane](../../../examples/dog-fooding/processes/review-a-board-lane.md) |
| write the root README, the package page or the docs site | [prc-write-public-docs](../../../examples/dog-fooding/processes/write-public-docs.md)     |
| move, split or merge pages of the docs site              | [prc-write-public-docs](../../../examples/dog-fooding/processes/write-public-docs.md)     |
| measure whether a docs section lets its reader act       | [the comprehension trial](../../../trials/comprehension/README.md)                        |
| change a skill, a hook or `plugin.json`                  | [prc-change-a-skill](../../../examples/dog-fooding/processes/change-a-skill.md)           |
| open a pull request                                      | [prc-pull-request](../../../examples/dog-fooding/processes/pull-request.md)               |

**A page that moves takes its URL with it.** A file under `template/` or `.schema/` citing the old URL is an overlay
file, and every corpus holds a copy. Rewrite each citation, then run what
[four trees](../../../CLAUDE.md#four-trees-hold-the-same-file) says that file owes.

**Every row that changes a file ends by running `prc-pull-request`.** It is where the version, the changelog and
the pages your change made wrong are dealt with. The board row is the one exception: it writes to the tracker and
changes nothing here.

## Which writing skill

`technical-writing` is the floor and every surface loads it first. One voice goes on top, chosen by who reads the words
rather than by which folder holds them.

| You are writing                                                                                   | Load next             |
|---------------------------------------------------------------------------------------------------|-----------------------|
| a record, a type page, a corpus's own `README.md`, or a `description:` and `notes:` in `.schema/` | `writing-a-record`    |
| a report record, which takes `writing-a-record` and then the verdicts on top of it                | `writing-a-report`    |
| a comment, a feature document, a test name, or the changelog                                      | `writing-in-the-tool` |
| the root `README.md`, `PACKAGE.md`, or the documentation site                                     | `writing-the-docs`    |
| a skill, a hook or a `plugin.json` under `.plugin/`, whose rules are `std-PLUGIN`                 | nothing more          |
| a commit message or a pull request body                                                           | nothing more          |
