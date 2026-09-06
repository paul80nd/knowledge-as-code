---
id: ctl-0004
tier: normative
status: active
verifies: [ std-CONFIG ]
mechanism: ci
frequency: per-pr
evidence: The `lint` job's log on the pull request, under the step "Check every YAML file".
applies-to:
  - all
owner: paul.law
tags: [ linting, yaml ]
---

# Every YAML file passes yamllint

`Control: ctl-0004` `ACTIVE`

The shape of a YAML file is settled by a linter and not by a reviewer.

## What it checks

* [std-CONFIG.the-shape-of-a-yaml-file-is-yamllints-to-decide] says a YAML file "**MUST** pass `yamllint --strict`
  under the `.yamllint` at this repository's root".

## How it works

The `lint` job installs yamllint at the version `.github/requirements.txt` pins, then runs `yamllint --strict .` from
the root. `.yamllint` extends yamllint's `default` ruleset and carries the four places this repository departs from
it. `--strict` fails the build on `comments`, `comments-indentation` and `truthy`, which the default ruleset reports
as warnings.

`.yamllint` reads `.gitignore`, so the walk covers the files a fresh checkout holds and nothing the tool rebuilds.

## Coverage and gaps

yamllint reads shape. Whether a value is correct, whether a comment gives a reason, and whether that reason is true
are all a reviewer's to judge.

The other four rules [std-CONFIG] states have no check of their own. Dependabot moves the pin in
`.github/requirements.txt`, so a yamllint release arrives as a pull request.

[std-CONFIG]: ../standards/configuration.md
[std-CONFIG.the-shape-of-a-yaml-file-is-yamllints-to-decide]: ../standards/configuration.md#the-shape-of-a-yaml-file-is-yamllints-to-decide
