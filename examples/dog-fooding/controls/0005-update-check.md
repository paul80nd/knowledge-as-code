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
owner: paul.law
tags: [ overlay, template ]
---

# Every corpus holds the file the template sends it

`Control: ctl-0005` `ACTIVE`

The same file lives in the template and in every corpus, and a copy that drifted fails the build.

## What it checks

* [std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved] says you "**MUST** run
  `kac update --check --from ../../` in every corpus after that copy".

## How it works

The `corpora` job runs `dotnet run --project ../../tooling/kac -- update --check --from ../../` in each of the four
corpora under `examples/`. `manifest.yaml` at the root names the files the check reaches, and each of its rules says
which layer those files sit in.

The check answers in both directions. It reports a copy whose content differs from the template's, and a file a corpus
holds that the template sends nothing to. Either exits non-zero.

## Coverage and gaps

The matrix covers `examples/`. `template/` is where these files are authored, so nothing holds it to a copy of
itself.

`.schema/` and `template/.plugin/` sit outside the manifest, because a corpus reads both where they are authored
rather than taking a copy. A corpus created elsewhere receives its own `.editorconfig`, and each corpus here declares
that file under `skip:`.

[std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved]: ../standards/configuration.md#a-value-living-in-more-than-one-tree-is-copied-and-proved
