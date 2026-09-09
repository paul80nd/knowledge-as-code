---
id: tol-yamllint
type: tool
tier: descriptive
status: approved
versions: 1.38.x
licence: GPL-3.0-or-later
owner: human:paul.law
tags: [ linting, yaml ]
---

# yamllint

`Tool: tol-yamllint` `APPROVED`

The linter every YAML file in this repository answers to, run as `yamllint --strict .` by the `lint` job.

## What we use it for

`.yamllint` extends yamllint's own `default` ruleset and carries the four places this repository departs from it, each
with the reason in the comment above it. The shape of a YAML file is this tool's to decide rather than a reviewer's.

The `lint` job installs it from `.github/requirements.txt`, pinned so a linter release changes what the gate rejects
when we take it rather than on its own.

## Status

**approved** since 2026-09-02.

## Where it is used

Nowhere in this catalogue. It runs in the `lint` job of `kac.yml` against every YAML file in the repository, and no
service ships it.

## Alternatives considered

None recorded. [tol-actionlint] reads the same files, and the two are not alternatives: this one reads a YAML file as
YAML, and that one reads a workflow as a workflow.

## Licence and obligations

GPL-3.0-or-later. It is run as a separate program and never linked into anything published here, so the copyleft terms
reach no artefact this repository ships.

## Related

* [tol-actionlint] runs beside it in the same job.
* [std-CONFIG] says what `.yamllint` must carry, and [ctl-0004] is the check that runs it.

[ctl-0004]: ../../controls/0004-yamllint.md
[std-CONFIG]: ../../standards/configuration.md
[tol-actionlint]: actionlint.md
