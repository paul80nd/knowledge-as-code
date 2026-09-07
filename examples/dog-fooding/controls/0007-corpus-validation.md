---
id: ctl-0007
type: control
tier: normative
status: active
verifies: [ eng:std-GATES, std-VERS ]
mechanism: ci
frequency: per-pr
evidence: The `corpora` job's log on the pull request, one run per corpus, plus the `tool` job's template steps.
applies-to:
  - all
owner: paul.law
tags: [ corpus, schema, validation ]
---

# Every corpus validates and its generated output is fresh

`Control: ctl-0007` `ACTIVE`

Every corpus in this repository is judged against `.schema/` on every pull request.

## What it checks

* `eng:std-GATES.every-change-is-built-and-tested-automatically` says the build "**MUST** run from a clean checkout,
  on an agent provisioned from a definition in the repository".
* `std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request` says a consuming corpus "**MUST** move
  the `resolved:` lock of its `consumes:` entry in the pull request moving the producer's `content-version`".

## How it works

The `corpora` job runs `kac validate` and then `kac generate --check` in each of `examples/library`,
`examples/engineering`, `examples/payments` and `examples/dog-fooding`. The `tool` job runs the same pair in
`template/`, and again in a corpus `kac new` stood up from the packed tool.

Each matrix cell is a fresh runner holding its own checkout, and each corpus declares a different `types:`. Running
all of them is what proves the framework holds for a corpus that adopted a subset. `payments` and `dog-fooding` pack
`engineering` and restore it first, because `.imports/` is not committed.

## Coverage and gaps

`validate` judges a corpus against the schema, so it reports a broken reference, a missing section, an unreciprocated
edge or a field out of range. It reads no standard, so a record obeying the schema and none of [std-PROSE] passes.

`generate --check` reports staleness and names the command to run locally. It writes nothing back, because the job
holds `contents: read`.

`validate` asks each consumed source what it publishes now, once per run. It fails an import that was never restored,
warns where a newer version sits inside the declared range, and reports where one sits outside it. What it cannot see
is a pull request, so a lock moved in a later one passes here.

Most of the types the schema declares hold no record in any corpus here, and a rule declared on an empty folder has
never run against content. `kac validate` passes a corpus whose types are empty.

[std-PROSE]: ../standards/prose.md
