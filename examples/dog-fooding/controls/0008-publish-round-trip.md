---
id: ctl-0008
type: control
tier: normative
status: active
verifies: [ eng:std-GATES, std-PLUGIN ]
mechanism: ci
frequency: per-pr
evidence: The `corpora`, `round-trip` and `import-round-trip` job logs on the pull request.
applies-to:
  - all
owner: human:paul.law
tags: [ export, packaging, plugin ]
---

# Every corpus publishes, installs and is read back

`Control: ctl-0008` `ACTIVE`

A corpus that validates can still export a package nobody can install.

## What it checks

* `eng:std-GATES.every-change-is-built-and-tested-automatically` says a push "**MUST** trigger a build and the test
  suite, without anyone asking for it".
* `std-PLUGIN.a-skill-names-the-type-of-every-field-it-describes` says a skill "**MUST** name the parts file of the
  type its component declares, and no other type's".

## How it works

Three jobs share the work. `corpora` runs `export`, `bundle` and `pack` in each of the four corpora, then
`claude plugin validate --strict` over the assembled plugin and over its marketplace. Two guards follow it: every
component `bundle.json` includes is a directory in the tree, and `.claude-plugin/` holds `plugin.json` alone.

`round-trip` installs the plugin on Ubuntu and on Windows for `library`, `engineering` and `payments`, then asks each
skill in it the questions that skill describes. `import-round-trip` publishes from `engineering`, consumes it from
`payments`, renames a clause upstream, and asserts the downstream build goes red naming it.

The `tool` job adds the empty case. It bundles a corpus that adopted no type and asserts the plugin still installs.

## Coverage and gaps

`dog-fooding` is left out of `round-trip`, because it is the same shape as `payments`.

`round-trip` reads one sentence of each skill: it greps `SKILL.md` for the parts file the component requires, and
fails a skill naming another type's. Nothing else `std-PLUGIN` asks of the wording is read here.

The component list is read from `bundle.json`, which carries what the manifest declared. So a skill or hook directory
somebody forgot to declare travels undeclared and fails nothing.

Nothing here reaches the publishing workflows. What CI proves is that a package and a plugin assemble. Whether
nuget.org, GitHub Packages or the `marketplace` branch accepts either is answered after the merge.

The Claude Code CLI installs at `latest` in every one of these jobs, so a release of it can turn the gate red with no
change here.
