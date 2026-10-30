---
id: ctl-licence-review
type: control
tier: normative
status: active
verifies: [ eng:std-DEPS.ADOPT ]
mechanism: review-checklist
frequency: per-pr
evidence: The review on the pull request that adds a tool record or changes its `licence`.
applies-to:
  - all
review-by: "2027-10-30"
owner: human:paul.law
tags: [ dependencies, licences, supply-chain ]
---

# A reviewer checks each tool record's licence against the allowed list

`Control: ctl-licence-review` `ACTIVE`

A package `kac` ships has a licence `eng:std-DEPS` allows, checked by the reviewer of the pull request that adds it.

## What it checks

* `eng:std-DEPS.ADOPT` says somebody "**MUST** check a new package's licence against the licences the conformance
  checklist allows before that pull request merges".
* It says "A build **MUST NOT** ship a package under a refused licence".
* It says "A build **MUST NOT** ship a package with no licence".

## How it works

The reviewer of a pull request that adds a tool record under `tools/`, or changes its `licence`, reads the `licence`
field against the conformance checklist of `eng:std-DEPS`. That checklist lists the licences by SPDX id in three tiers:
allowed, review and refused.

1. Decide whether the tool ships. A package ships where `tooling/kac/kac.csproj` or `tooling/kac.core/kac.core.csproj`
   references it.
2. For a package that ships, check that `licence` is an allowed id, or a review-tier id with an accepted deviation.
   Send back a pull request whose `licence` is a refused id, `NONE`, or an id the checklist does not list.
3. For a tool that never ships, such as yamllint under `GPL-3.0-or-later`, read the record's terms of use instead. The
   checklist of `eng:std-DEPS` says such a tool is judged under `eng:pol-TRUS`.

Four packages ship today, under three tool records. Markdig is under `BSD-2-Clause`. YamlDotNet, Spectre.Console and
Spectre.Console.Cli are under `MIT`. Both licences are allowed ids.

## Coverage and gaps

Nothing compares `licence` with the list automatically, so a reviewer who skips the step lets a refused licence merge.
No field on a tool record says whether the tool ships, so the reviewer reads the project files to decide. A review
leaves no record of the check unless the reviewer comments on the pull request.

The check reads the direct packages alone. A package that arrives as a dependency of one of them has no tool record,
and nothing reads its licence.
