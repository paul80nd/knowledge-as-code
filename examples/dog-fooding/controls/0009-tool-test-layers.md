---
id: ctl-0009
type: control
tier: normative
status: active
verifies: [ eng:std-GATES.BUILD, eng:std-TEST.LEVEL, std-PLUGIN.FROZEN ]
mechanism: ci
frequency: per-pr
evidence: The logs of `tool (ubuntu-latest)` and `tool (windows-latest)` on the pull request, under their test steps.
applies-to:
  - all
review-by: "2027-09-22"
owner: human:paul.law
tags: [ golden-tests, testing ]
---

# Three test layers run before a merge

`Control: ctl-0009` `ACTIVE`

Each layer asserts something different about the same corpus.

## What it checks

* `eng:std-TEST.LEVEL` says a team "**MUST** cover a rule about one unit's behaviour with a
  test on that unit alone".
* `eng:std-GATES.BUILD` says a push "**MUST** trigger a build and the test
  suite, without anyone asking for it".
* `std-PLUGIN.FROZEN` says a skill "**MUST** report a `status`
  its type does not treat as in force" and "**MUST** report a review date that has passed, where its type exports
  one". `PluginSkillStalenessTests` holds each lookup skill to both, deriving what it owes from the schema, down to
  the key each type's export writes the date under.

## How it works

The `tool` job runs three commands, once on `ubuntu-latest` and once on `windows-latest`:

* `dotnet test --project tooling/kac.tests` for the unit layer.
* `dotnet test --project tooling/kac.features` for the Reqnroll behaviour specs.
* `dotnet test --project tooling/kac.goldens` for the golden fixtures.

[Build, test and release](https://paul80nd.github.io/knowledge-as-code/maintaining/build-test-release/) says what
each layer proves.

The golden suite adds two checks of its own. It fails a reachable check id that no fixture exercises. It fails a type
page whose generated checks table has drifted from the catalogue. Each fixture is built over the real `.schema/`,
copied in on each run, so a schema change that alters behaviour shows up in the run that made it.

## Coverage and gaps

No layer reports line coverage, which `eng:std-TEST.REACH` asks for. The golden suite measures
the share of check ids a fixture exercises, and the two figures answer different questions.

The suite runs on a pull request into `main`. A push to a branch with no pull request open triggers nothing, so the
answer arrives when the work is offered for merge.
