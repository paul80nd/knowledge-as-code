---
id: ctl-0009
type: control
tier: normative
status: active
verifies: [ std-CSSTY.SUPPR ]
mechanism: not-enforced
applies-to:
  - all
review-by: "2027-10-17"
owner: human:paul.law
tags: [ analysers, csharp, suppressions ]
---

# Nothing checks that a suppression is local and justified

`Control: ctl-0009` `ACTIVE`

No check fails a change that silences an analyser warning without a reason.

## What it checks

* [std-CSSTY.SUPPR] says a suppression "**MUST** sit on the member it applies to, with a
  `Justification` giving the reason".
* It says a repository "**MUST NOT** disable a rule in `.editorconfig` to clear a warning in one file".

## How it works

Nothing runs. The check these rules need is a pipeline job over the lines a pull request adds. It fails on any of
these:

* `#pragma warning disable`
* a `<NoWarn>` element in a project file
* a `dotnet_diagnostic.<id>.severity = none` line in `.editorconfig`
* a `[SuppressMessage(` with no `Justification =`, or one with an `assembly:` target

The same patterns can run over the staged diff as a pre-commit check, or over the written content as a Claude Code
hook on `Write` and `Edit`.

## Coverage and gaps

A reviewer reading the diff is the only thing that notices a blanket suppression today.

The check proves a `Justification` is present, and a reviewer still judges whether it gives a reason. A `.editorconfig`
severity change may be a deliberate rule for the whole repository. The check fails it anyway, and a reviewer decides. An
attribute split across lines passes a pattern that reads one line.

[std-CSSTY.SUPPR]: ../standards/platform/dotnet/code-style.md#suppr-a-suppression-is-local-and-says-why
