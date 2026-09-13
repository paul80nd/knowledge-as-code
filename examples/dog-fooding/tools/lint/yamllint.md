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

The linter that checks every YAML file in this repository. The `lint` job runs it as `yamllint --strict .`.

## What we use it for

`.yamllint` extends yamllint's own `default` ruleset. It lists the four places this repository departs from that
ruleset, each with the reason in the comment above it. This tool decides the shape of a YAML file, so a reviewer does
not have to.

The `lint` job installs it from `.github/requirements.txt`. The pin means a linter release changes what the gate
rejects only when the pin moves.

## Status

**approved** since 2026-09-02.

## Where it is used

Nowhere in this catalogue. It runs in the `lint` job of `kac.yml` against every YAML file in the repository, and no
service ships it.

## Alternatives considered

None recorded. [tol-actionlint] reads the same files, and neither replaces the other: yamllint checks a YAML file as
YAML, and actionlint checks a workflow as a workflow.

## Licence and obligations

GPL-3.0-or-later. The `lint` job runs it as a separate program, and nothing published here links to it, so the
copyleft terms apply to no artefact this repository ships.

## Related

* [tol-actionlint] runs beside it in the same job.
* [std-CONFIG] states what `.yamllint` has to contain.
* [ctl-0004] is the check that runs it.

[ctl-0004]: ../../controls/0004-yamllint.md
[std-CONFIG]: ../../standards/configuration.md
[tol-actionlint]: actionlint.md
