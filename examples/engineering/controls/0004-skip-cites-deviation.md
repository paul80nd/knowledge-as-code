---
id: ctl-0004
type: control
tier: normative
status: active
verifies: [ std-GATES.BLOCK ]
mechanism: not-enforced
applies-to:
  - all
review-by: "2027-10-17"
owner: human:paul.law
tags: [ deviations, skipped-tests, testing ]
---

# Nothing checks that a skipped test cites a deviation

`Control: ctl-0004` `ACTIVE`

No check fails a change that skips a test and cites no deviation.

## What it checks

* [std-GATES.BLOCK] says a team "**MUST NOT** skip, silence or suppress a check without a
  recorded deviation".

## How it works

Nothing runs. The check this rule needs is a pipeline job over the lines a pull request adds. It fails on a line that
adds a skip marker and cites no deviation id:

```sh
git diff -U0 origin/main...HEAD \
  | grep -E '^\+.*(Skip *=|\[Ignore|mark\.skip|pytest\.skip\(|t\.Skip\(|(it|test)\.skip\()' \
  | grep -vE 'dev-[a-z0-9-]+'
```

The job fails where that prints a line. The same pattern can run over the staged diff as a pre-commit check, or over
the written content as a Claude Code hook on `Write` and `Edit`.

## Coverage and gaps

A reviewer reading the test diff is the only thing that notices an uncited skip today.

The check proves an id is cited. It cannot tell whether the deviation exists, names an owner, or is inside its review
date, which the other rules under [std-GATES.BLOCK] require. It knows only the markers in its
pattern, so a skip in another framework, or a pipeline step set to continue on error, passes it. The rule lets a test be
guarded off a platform it cannot run on without a deviation, and the check fails such a guard where it uses one of the
markers. A reviewer decides whether a required check runs the test on a platform where it can.

[std-GATES.BLOCK]: ../standards/common/build-gates.md#block-a-failing-check-stops-the-change
