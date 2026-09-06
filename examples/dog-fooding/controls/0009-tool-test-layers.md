---
id: ctl-0009
tier: normative
status: active
verifies: [ eng:std-GATES, eng:std-TEST ]
mechanism: ci
frequency: per-pr
evidence: The `tool` job's log on the pull request, under its three test steps.
applies-to:
  - all
owner: paul.law
tags: [ golden-tests, testing ]
---

# Three test layers run before a merge

`Control: ctl-0009` `ACTIVE`

Each layer asserts something different about the same corpus.

## What it checks

* `eng:std-TEST.pick-the-level-from-the-fault` says a team "**MUST** cover a rule about one unit's behaviour with a
  test on that unit alone".
* `eng:std-GATES.every-change-is-built-and-tested-automatically` says a push "**MUST** trigger a build and the test
  suite, without anyone asking for it".

## How it works

The `tool` job runs `dotnet test tooling/kac.tests` for the unit layer, `dotnet test tooling/kac.features` for the
Reqnroll behaviour specs, and `dotnet run tooling/kac-tests.cs` for the golden fixtures. `tooling/README.md` says what
each layer covers.

The golden suite carries two gates of its own. It fails a reachable check id that no fixture exercises, and it fails a
type page whose generated checks table has drifted from the catalogue. Each fixture is assembled over the real
`.schema/`, copied in per run, so a schema change that alters behaviour surfaces in the run that made it.

## Coverage and gaps

No layer reports line coverage, which is what `eng:std-TEST.know-what-the-suite-reaches` asks for. What the golden
suite measures is the share of check ids a fixture reaches, and the two figures answer different questions.

The suite runs on a pull request into `main`. A push to a branch with no pull request open triggers nothing, so the
answer arrives when the work is offered rather than when it is written.
