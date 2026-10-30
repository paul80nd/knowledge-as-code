---
id: ctl-corpus-validation
type: control
tier: normative
status: active
verifies: [ eng:std-GATES.BUILD, std-VERS.CONSUME ]
mechanism: ci
frequency: per-pr
evidence: The `corpora` job's log on the pull request, one run per corpus, plus the `tool` job's template steps.
applies-to:
  - all
review-by: "2027-09-22"
owner: human:paul.law
tags: [ corpus, schema, validation ]
---

# Every corpus validates and its generated output is fresh

`Control: ctl-corpus-validation` `ACTIVE`

Every pull request checks each corpus in this repository against `.schema/`.

## What it checks

* `eng:std-GATES.BUILD` says the build "**MUST** run from a clean checkout,
  on an agent that is not a person's own machine".
* `std-VERS.CONSUME` says a consuming corpus "**MUST** move
  the `resolved:` lock of its `consumes:` entry in the pull request moving the producer's `content-version`".

## How it works

The `corpora` job runs `kac validate` and then `kac generate --check` in each of `examples/library`,
`examples/engineering`, `examples/payments` and `examples/dog-fooding`. Its `validate` passes `--since` with the branch
the pull request targets, so it also compares each record with its state there. The `tool` job runs the same pair in
`template/`, without `--since`, and again in a corpus `kac new` created from the packed tool.

Each matrix cell is a fresh runner with its own checkout, and each corpus declares a different `types:`. Running all
of them proves the framework works for a corpus that adopted a subset. `payments` and `dog-fooding` pack `engineering`
and restore it first, because `.imports/` is not committed.

## Coverage and gaps

`validate` checks a corpus against the schema. It reports a broken reference, a missing section, an unreciprocated
edge or a field out of range. It reads no standard, so a record that obeys the schema and none of [std-PROSE] passes.

`generate --check` reports stale output and prints the command to run locally. It writes nothing back, because the job
has `contents: read`.

`validate` asks each consumed source what it publishes now, once per run. It fails an import that was never restored,
and reports where a newer version sits outside the declared range. Where the newer version sits inside the range, as a
producer's patch move does, it warns and passes. A patch move with no lock move merges into `main` with a warning. It
cannot see another pull request, so a lock moved in a later one passes here.

Most of the types the schema declares have no record in any corpus here, and a rule declared on an empty folder has
never run against content. `kac validate` passes a corpus whose types are empty.

[std-PROSE]: ../standards/prose.md
