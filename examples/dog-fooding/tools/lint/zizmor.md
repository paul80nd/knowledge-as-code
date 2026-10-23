---
id: tol-zizmor
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:pypi/zizmor, versions: 1.31.x }
homepage: https://docs.zizmor.sh
licence: MIT
licence-declared: MIT
decided-on: "2026-10-23"
review-by: "2027-10-23"
owner: human:paul.law
tags: [ github-actions, linting, security, workflows ]
---

# zizmor

`Tool: tol-zizmor` `APPROVED`

The static analyser that checks every workflow and each `dependabot.yml` for insecure patterns. The `lint` job runs it
after actionlint, over `.github/` and `template/.github/`.

## What we use it for

It reports an action pinned to a tag, a checkout that keeps its credential, a workflow granting more than read at its
top level, and an expression an attacker can write into a `run:` block. In a `dependabot.yml`, it reports an ecosystem
with a cooldown shorter than seven days. It runs with its default persona, which reports the findings its authors rate
as actionable and leaves out the pedantic ones.

The job gives it `github.token`, so it also asks GitHub whether each pinned commit belongs to the action it names.

The `lint` job installs it from `.github/requirements.txt`, and Dependabot moves that pin. A finding the workflows keep
on purpose is suppressed by an inline `# zizmor: ignore[...]` comment, with the reason in the comment above the step.

## Where it is used

Nowhere in this catalogue. It runs in the `lint` job of `kac.yml` against the workflows and each `dependabot.yml`, and no service ships it.

## Alternatives considered

None recorded. [tol-actionlint] reads the same files, and neither replaces the other: actionlint checks that a workflow
runs, and zizmor checks that it is safe to run.

## Licence and obligations

MIT. The wheel's software bill of materials lists the Rust crates compiled into the binary, and each is under a
permissive licence or offers one. The `lint` job runs it as a separate program, and nothing this repository publishes
contains it.

## Related

* [tol-actionlint] runs before it in the same job.
* [std-CI] states the workflow rules it checks.
* [ctl-0015] is the check that runs it.

[ctl-0015]: ../../controls/0015-zizmor.md
[std-CI]: ../../standards/workflows.md
[tol-actionlint]: actionlint.md
