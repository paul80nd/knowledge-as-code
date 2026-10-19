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
review-by: "2027-09-22"
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
* `std-PLUGIN.pluginjson-declares-every-component-and-the-types-it-reads` says every skill directory and hook directory
  in the plugin tree "**MUST** appear under `metadata.components`".

## How it works

Three jobs share the work. `corpora` runs `export`, `bundle` and `pack` in each of the four corpora. `bundle` fails on a
directory under `skills/` or `hooks/` that no component in `metadata.components` claims. Then
`claude plugin validate --strict` runs over the assembled plugin and over its marketplace. Two more checks follow it:
every component `bundle.json` lists is in the tree, as a skill or hook directory or a view's file, and
`.claude-plugin/` contains `plugin.json` and nothing else.

`round-trip` installs the plugin on Ubuntu and on Windows for `library`, `engineering` and `payments`, then asks each
skill in it the questions that skill describes. `import-round-trip` publishes from `engineering`, consumes it from
`payments`, renames a clause upstream, and asserts the downstream build fails and reports that clause.

The `tool` job adds the empty case. It bundles a corpus that adopted no type, then runs
`claude plugin validate --strict` over the plugin and its marketplace. It asserts that nothing reading a type shipped,
and that `raise-finding`, the one standalone skill, survived. No job installs that plugin.

## Coverage and gaps

`dog-fooding` is left out of `round-trip`, because it is the same shape as `payments`.

`round-trip` reads one sentence of each skill. It greps `SKILL.md` for the parts file the component requires, and
fails a skill that names another type's. Nothing else `std-PLUGIN` asks of the wording is checked here. Two of its
rules are checked by [ctl-0009] instead, which is where the unit tests that read a skill's prose run.

`harvest-findings` travels into a corpus's working tree rather than into a plugin, so no job here reaches it. Every
check above reads an assembled bundle, and that skill is in none. The clause saying no plugin may declare a component
that triages findings has no control either.

Nothing here runs the publishing workflows. CI proves that a package and a plugin assemble. Whether nuget.org, GitHub
Packages or the `marketplace` branch accepts either is answered after the merge.

The Claude Code CLI installs at `latest` in every one of these jobs, so a release of it can fail the gate with no
change here.

[ctl-0009]: 0009-tool-test-layers.md
