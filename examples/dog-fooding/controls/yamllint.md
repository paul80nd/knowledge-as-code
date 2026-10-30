---
id: ctl-yamllint
type: control
tier: normative
status: active
verifies: [ std-CONFIG.YAML ]
mechanism: ci
frequency: per-pr
evidence: The `lint` job's log on the pull request, under the step "Check every YAML file".
applies-to:
  - all
review-by: "2027-09-22"
owner: human:paul.law
tags: [ linting, yaml ]
---

# Every YAML file passes yamllint

`Control: ctl-yamllint` `ACTIVE`

yamllint settles the shape of every YAML file in this repository.

## What it checks

* [std-CONFIG.YAML] says a YAML file "**MUST** pass `yamllint --strict`
  under the `.yamllint` at this repository's root".

## How it works

The `lint` job installs yamllint at the version `.github/requirements.txt` pins, then runs `yamllint --strict .` from
the root. `.yamllint` extends yamllint's `default` ruleset and sets the four rules this repository changes. `--strict`
fails the build on `comments`, `comments-indentation` and `truthy`, which the default ruleset reports as warnings.

`.yamllint` reads `.gitignore`, so the run covers the files a fresh checkout has and skips anything the tool rebuilds.

## Coverage and gaps

yamllint checks shape. A reviewer judges three things: whether a value is correct, whether a comment gives a reason,
and whether that reason is true.

yamllint covers one of the five rules [std-CONFIG] states, and [ctl-update-check] covers a second. The other three have
no check of their own. Dependabot moves the pin in `.github/requirements.txt`, so a yamllint release arrives as a pull
request.

[ctl-update-check]: update-check.md
[std-CONFIG]: ../standards/configuration.md
[std-CONFIG.YAML]: ../standards/configuration.md#yaml-the-shape-of-a-yaml-file-is-yamllints-to-decide
