---
id: ctl-0012
type: control
tier: normative
status: active
verifies: [ std-A11Y ]
mechanism: not-enforced
applies-to:
  - svc-docs-site
  - svc-kac
review-by: "2027-10-19"
owner: human:paul.law
tags: [ accessibility, assessment, documentation ]
---

# Nothing checks a page, the output or a new component version for accessibility

`Control: ctl-0012` `ACTIVE`

No check fails a page that skips a heading level, output that loses meaning without colour, or a pin that moves with
no new assessment.

## What it checks

* [std-A11Y.a-markdown-author-owns-the-structure-of-a-page] says heading levels "**MUST** descend one at a time".
* It says an image "**MUST** have alternative text saying what a reader who cannot see it needs".
* [std-A11Y.kac-carries-no-meaning-in-colour-alone] says output "**MUST** stay complete when colour is off".
* [std-A11Y.a-pinned-component-carries-an-accessibility-assessment] says a pull request moving a pin to a new major or
  minor version "**MUST** include a new assessment".

## How it works

Nothing runs. The checks these rules need are three jobs on each pull request:

* markdownlint over `docs/`, with `MD001` (`heading-increment`) and `MD045` (`no-alt-text`) turned on, failing the
  `docs` job on a finding.
* A test that runs each verb twice, with `NO_COLOR=1` and with colour on. It strips the escape sequences from the
  second run and fails where the runs print different words.
* A job that reads the pins in `tooling/` and `docs/requirements.txt`. It fails where the major or minor of a pin moves
  for a component whose tool record has an `Accessibility` section, and that tool record does not change in the same
  pull request.

## Coverage and gaps

A reviewer reading the diff is the only thing that notices any of these today. Dependabot moved `Spectre.Console.Cli`
from 0.55.0 to 0.57.2 in [#802](https://github.com/paul80nd/knowledge-as-code/pull/802) with no new assessment, and
every check passed. The assessment was repeated by hand in
[#966](https://github.com/paul80nd/knowledge-as-code/pull/966).

The checks would prove a heading order, an alternative text, words that survive without colour, and a tool record that
moved with its pin. They would not prove that link text makes sense out of context, or that an assessment checked the
right things. Nobody has read either surface with a screen reader. The theme's contrast, focus and keyboard rules go
unchecked. So does every rule under [std-A11Y.what-falls-short-is-written-down-where-a-reader-finds-it] and
[std-A11Y.a-change-that-makes-either-surface-worse-needs-a-deviation].

[std-A11Y.a-change-that-makes-either-surface-worse-needs-a-deviation]: ../standards/accessibility.md#a-change-that-makes-either-surface-worse-needs-a-deviation
[std-A11Y.a-markdown-author-owns-the-structure-of-a-page]: ../standards/accessibility.md#a-markdown-author-owns-the-structure-of-a-page
[std-A11Y.a-pinned-component-carries-an-accessibility-assessment]: ../standards/accessibility.md#a-pinned-component-carries-an-accessibility-assessment
[std-A11Y.kac-carries-no-meaning-in-colour-alone]: ../standards/accessibility.md#kac-carries-no-meaning-in-colour-alone
[std-A11Y.what-falls-short-is-written-down-where-a-reader-finds-it]: ../standards/accessibility.md#what-falls-short-is-written-down-where-a-reader-finds-it
