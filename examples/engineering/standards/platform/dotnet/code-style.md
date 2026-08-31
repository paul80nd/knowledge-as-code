---
id: std-CSSTY
tier: normative
status: draft
implements: [ pol-AUTV.WARN, pol-SECD.CODING ]
applies-to:
  - all
review-by: "2027-08-31"
owner: paul.law
tags: [ analyzers, code-style, csharp ]
---

# C# follows the runtime team's conventions, and the compiler enforces them

`Standard: std-CSSTY` `DRAFT`

## Summary

C# in this estate is written to the .NET runtime team's own coding style, expressed as an `.editorconfig` in the
repository. Analysers run as part of the build, and a warning is a defect.

## Rules

### The conventions come from one file

- A repository holding C# **MUST** carry an `.editorconfig` at its root, taken from the .NET runtime team's own.
- A project **MUST** set `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild`, so the build checks the style.
- A project **MUST** enable nullable reference types.
- Code **MUST NOT** silence a nullability warning with `!`.
- A project **MUST** enable the .NET analysers at the `latest` analysis level.

_**Covers:** [pol-AUTV].WARN, [pol-SECD].CODING_

### A suppression is local and says why

- A suppression **MUST** sit on the member it applies to, with a `Justification` naming the reason.
- A repository **MUST NOT** disable a rule in `.editorconfig` to clear a warning in one file.

_**Covers:** [pol-AUTV].WARN_

### The security rules the analysers cover

- Code **MUST** build a SQL command through a parameter rather than by joining strings.
- Code **MUST** use the framework's cryptography.
- Code **MUST NOT** call an algorithm the analysers report as broken.
- Code **MUST** compare a secret with `CryptographicOperations.FixedTimeEquals` rather than with `==`.

_**Covers:** [pol-SECD].CODING_

## Examples

```
Good
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <AnalysisLevel>latest</AnalysisLevel>
  <Nullable>enable</Nullable>

Avoid
  <NoWarn>CA1848;CS8600;CS8602</NoWarn>
```

The avoided line turns three rules off for the whole project. Two of them are nullability, so every reference in the
project stops being checked to clear the file that first complained.

```
Good
  [SuppressMessage("Performance", "CA1848",
      Justification = "Called once at start-up. Source-generated logging is not worth the partial class here.")]

Avoid
  #pragma warning disable CA1848
```

The first names the member, the rule and the reason. The second reaches everything below it in the file, including code
written next year.

## Conformance checklist

- [ ] The repository root holds an `.editorconfig`, and it matches the upstream one apart from documented departures.
- [ ] A build with a style violation fails.
- [ ] `<NoWarn>` is empty in every project file.
- [ ] Every `SuppressMessage` in the repository carries a `Justification` a reader can act on.
- [ ] The solution builds with zero warnings from a clean checkout.
- [ ] No `!` appears in the repository outside a test asserting a null argument is refused.

## Rationale and provenance

C# has a house style already, written by the people who write the language's own libraries. Taking theirs means a
reviewer argues about the change instead of the braces, and a newcomer has read the rules before they arrive.

## Sources and further reading

- **Normative.** [dotnet/runtime coding style] is the style this standard adopts. The departures are the analyser
  settings above, and nothing else.
- **Normative.** [Common C# code conventions] carries the naming and layout rules the `.editorconfig` encodes.
- **Informative.** [Code quality analysis rules] lists the rules the build turns into errors.

[Code quality analysis rules]: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/
[Common C# code conventions]: https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions
[dotnet/runtime coding style]: https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md
[pol-AUTV]: ../../../policies/delivery/autv-automated-verification.md#clauses
[pol-SECD]: ../../../policies/security/secd-security-by-design.md#clauses
