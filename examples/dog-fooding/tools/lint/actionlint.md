---
id: tol-actionlint
type: tool
tier: descriptive
status: approved
versions: 1.7.x
licence: MIT
decided-in:
replaces:
successor:
owner: human:paul.law
tags: [ github-actions, linting, workflows ]
---

# actionlint

`Tool: tol-actionlint` `APPROVED`

The linter every workflow in `.github/workflows/` answers to, run by the `lint` job after yamllint has read the same
files as plain YAML.

## What we use it for

It reads a workflow as a workflow: the expression syntax, the job graph, the action inputs, and the shell in every
`run:` block. The runner carries shellcheck, so actionlint puts each of those blocks through it as well.

It is a Go binary with no pip package, so `.github/requirements.txt` cannot pin it. The `go install` line in the job
carries the version instead, and the header comment in that file says where to look.

## Status

**approved** since 2026-09-02.

## Where it is used

Nowhere in this catalogue. It runs in the `lint` job of `kac.yml` against the workflows, and no service ships it.

## Alternatives considered

None recorded. [tol-yamllint] runs first on the same files, and neither replaces the other.

## Licence and obligations

MIT. Nothing follows for anything this repository publishes.

## Related

* [tol-yamllint] reads the same files as plain YAML.
* [ctl-0003] is the check that runs it.

[ctl-0003]: ../../controls/0003-actionlint.md
[tol-yamllint]: yamllint.md
