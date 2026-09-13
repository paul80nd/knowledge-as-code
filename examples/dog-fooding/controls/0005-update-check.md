---
id: ctl-0005
type: control
tier: normative
status: active
verifies: [ std-CONFIG ]
mechanism: ci
frequency: per-pr
evidence: The `corpora` job's log on the pull request, under the step "Check the corpus is in step with the template".
applies-to:
  - all
owner: human:paul.law
tags: [ overlay, template ]
---

# Every corpus holds the file the template sends it

`Control: ctl-0005` `ACTIVE`

The same file lives in the template and in every corpus. A copy that drifted fails the build.

## What it checks

* [std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved] says you "**MUST** run
  `kac update --check --from ../../` in every corpus after that copy".

## How it works

The `corpora` job runs `dotnet run --project ../../tooling/kac -- update --check --from ../../` in each of the four
corpora under `examples/`. `manifest.yaml` at the root lists the files the check covers, and each of its rules states
which layer those files belong to.

The check reports two faults: a copy whose content differs from the template's, and a file in a corpus that the
template sends nothing to. Either one exits non-zero.

## Coverage and gaps

The matrix covers `examples/`. These files are authored in `template/`, and nothing compares `template/` against a
copy of itself.

`manifest.yaml` covers `.schema/` and `template/.plugin/`, and neither reaches a corpus here as a copy. `.schema/`
lands on the path it was read from. Every corpus names `plugin.from`, so it reads the plugin tree from `template/`. A
corpus created elsewhere gets its own `.editorconfig`, and each corpus here declares that file under `skip:`.

[std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved]: ../standards/configuration.md#a-value-living-in-more-than-one-tree-is-copied-and-proved
