---
id: std-CONFIG
tier: normative
status: active
implements: [ eng:pol-EVER.ASSETS, eng:pol-EVER.ORPHAN, eng:pol-EVER.PARITY, eng:pol-PIPE.CONFIG, eng:pol-SCRT.EMBED,
  eng:pol-TRUS.INVENT, eng:pol-TRUS.REVIEW, eng:pol-TRUS.SOURCE ]
applies-to:
  - all
review-by: "2027-09-02"
owner: paul.law
tags: [ configuration, dependencies, yaml ]
---

# Configuration is committed, pinned and explained

`Standard: std-CONFIG` `ACTIVE`

## Summary

The schema under `.schema/`, the descriptor each corpus carries, the overlay manifest, the site's navigation and the
dependency pins all decide what this repository builds and publishes. Each of them is a tracked file, names an exact
version where it names a version, and carries the comment a reviewer needs to judge the change.

## Rules

### Every value the build reads is committed

- A value deciding how this repository builds, validates or publishes **MUST** sit in a tracked file.
- A setting only a provider's console can hold **MUST** be named in the header comment of the file it affects.
- A configuration file **MUST NOT** hold a key, a token or a password.
- A file a corpus keeps **MUST NOT** name a path from the machine that wrote it.
- A corpus's `.gitignore` **MUST** hold `.dist/`, `.imports/` and `_reports/`, which the tool rebuilds whole.

_**Covers:** `eng:pol-EVER.ASSETS`, `eng:pol-EVER.ORPHAN`, `eng:pol-SCRT.EMBED`_

### A pin names an exact version

- A dependency **MUST** name one version: a `PackageReference`, a line in `docs/requirements.txt`, or an action's
  commit SHA.
- `.github/dependabot.yml` **MUST** name every directory holding a manifest that builds this repository.
- Every ecosystem it tracks **MUST** carry a `schedule:`, so a component is looked at again after we adopt it.
- Packages that only move as a set **MUST** be raised as one grouped update.
- A manifest Dependabot cannot read **MUST** be named in that file's header comment, with what it pins today.

_**Covers:** `eng:pol-TRUS.INVENT`, `eng:pol-TRUS.REVIEW`, `eng:pol-TRUS.SOURCE`_

### A file says what it is for, and why each value is what it is

- A YAML file **MUST** open with a comment saying what it is for, or with the `# yaml-language-server: $schema=` line
  naming what describes it.
- A comment **MUST** give the reason a value is what it is.
- A comment **MUST NOT** restate the value beside it.
- A comment **SHOULD** run to one line where one line carries the reason.
- A date **MUST** be quoted.
- A YAML file **MUST** indent by two spaces.
- A flow sequence **MUST** be written `[a, b]`, which `.editorconfig` sets.
- A change to a configuration file **MUST** take the review a change to code takes.

_**Covers:** `eng:pol-EVER.PARITY`_

### A value living in more than one tree is copied and proved

- A setting every project under `tooling/` takes **MUST** sit in `tooling/Directory.Build.props`.
- A file `manifest.yaml` names **MUST** be copied by hand into `template/` and into each corpus under `examples/`.
- `kac update --check --from ../../` **MUST** pass in every corpus after that copy.
- `.schema/` and `template/.plugin/` **MUST** be read where they are authored, and copied into no corpus here.
- A corpus's own configuration **MUST** stay in that corpus, and the package `kac pack` seals **MUST NOT** carry
  another corpus's.

_**Covers:** `eng:pol-PIPE.CONFIG`_

## Examples

```
✅ Good
# MkDocs and its theme, pinned in docs/requirements.txt so a theme release changes the site when we take it.
- package-ecosystem: pip
  directory: /docs
  schedule:
    interval: weekly

mkdocs==1.6.1
mkdocs-material==9.7.7

review-by: "2027-09-02"

❌ Avoid
# The pip ecosystem.
- package-ecosystem: pip
  directory: /docs

mkdocs
mkdocs-material

review-by: 2027-09-02
```

The avoided comment names the key underneath it and says nothing a reader could not see. An unpinned requirement
changes the site on a day nobody chose. The unquoted date arrives as a datetime, and renders with a timezone shift.

## Conformance checklist

- [ ] Every file the change touches is tracked, and holds no credential.
- [ ] Every dependency it adds names one exact version.
- [ ] `.github/dependabot.yml` names the directory that manifest sits in.
- [ ] Every new value carries a comment giving its reason, and no comment restates its value.
- [ ] Every date is quoted.
- [ ] `kac update --check --from ../../` passes in each corpus the change reaches.

## Rationale and provenance

The tool walks up for a `.corpus.yaml` to find the corpus, and up again for `.schema/` to find what to judge it
against. Both walks read files, so a corpus is portable and a checkout is enough to reproduce a run.

Nothing here checks a comment, an indent or a pin. `kac update --check` compares the overlay copies in both
directions, and it is the one rule on this page a command answers. The rest is what a reviewer is reading for.

`.github/workflows/` and the release are [std-CI]'s. This standard reaches the files those workflows read.

## Sources and further reading

- **Normative.** [EditorConfig] defines the format of `.editorconfig`, which sets the indent and the flow-sequence
  form used here.
- **Informative.** [YAML 1.2.2] is the specification that decides how an unquoted scalar is read.

## Changelog

- 2026-09-02: initial version.

[EditorConfig]: https://editorconfig.org
[YAML 1.2.2]: https://yaml.org/spec/1.2.2/
[std-CI]: workflows.md
