---
id: ctl-0012
type: control
tier: normative
status: active
verifies: [ std-A11Y.COLOUR, std-A11Y.STRUCT ]
mechanism: not-enforced
applies-to:
  - svc-docs-site
  - svc-kac
review-by: "2027-10-19"
owner: human:paul.law
tags: [ accessibility, documentation ]
---

# Nothing checks a page or the output for accessibility

`Control: ctl-0012` `ACTIVE`

No check fails a page that skips a heading level, or output that loses meaning without colour.

## What it checks

* [std-A11Y.STRUCT] says heading levels "**MUST** descend one at a time".
* It says an image "**MUST** have alternative text saying what a reader who cannot see it needs".
* [std-A11Y.COLOUR] says output "**MUST** stay complete when colour is off".

## How it works

Nothing runs. The checks these rules need are two jobs on each pull request:

* markdownlint over `docs/`, with `MD001` (`heading-increment`) and `MD045` (`no-alt-text`) turned on, failing the
  `docs` job on a finding.
* A test that runs each verb twice, with `NO_COLOR=1` and with colour on. It strips the escape sequences from the
  second run and fails where the runs print different words.

## Coverage and gaps

A reviewer reading the diff is the only thing that notices any of these today.

The checks would prove a heading order, an alternative text, and words that survive without colour. They would not
prove that link text makes sense out of context. Nobody has read either surface with a screen reader. The theme's
contrast, focus and keyboard rules go unchecked. So does every rule under
[std-A11Y.GAPS] and
[std-A11Y.WORSE].

[ctl-0014] checks two rules under [std-A11Y.ASSESS]: that an assessment
names the pinned version, and that a pin moving its major or minor brings a new one. Below 0.1.0 any move of the pin
brings a new one.
[#967](https://github.com/paul80nd/knowledge-as-code/issues/967) found the second unchecked after Dependabot moved
`Spectre.Console.Cli` in [#802](https://github.com/paul80nd/knowledge-as-code/pull/802). The other rules under it go
unchecked.

[ctl-0014]: 0014-assessed-pin-tests.md
[std-A11Y.WORSE]: ../standards/accessibility.md#worse-a-change-that-makes-either-surface-worse-needs-a-deviation
[std-A11Y.STRUCT]: ../standards/accessibility.md#struct-a-markdown-author-owns-the-structure-of-a-page
[std-A11Y.ASSESS]: ../standards/accessibility.md#assess-a-pinned-component-carries-an-accessibility-assessment
[std-A11Y.COLOUR]: ../standards/accessibility.md#colour-kac-carries-no-meaning-in-colour-alone
[std-A11Y.GAPS]: ../standards/accessibility.md#gaps-what-falls-short-is-written-down-where-a-reader-finds-it
