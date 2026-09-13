---
id: tol-spectre-console
type: tool
tier: descriptive
status: approved
versions: Spectre.Console 0.57.x, Spectre.Console.Cli 0.55.x
licence: MIT
replaces: tol-system-commandline
owner: human:paul.law
tags: [ cli, console, terminal ]
---

# Spectre.Console

`Tool: tol-spectre-console` `APPROVED`

Two packages that give `kac` its command line and everything it prints. `Spectre.Console.Cli` reads the command line.
`Spectre.Console` renders the tables, colours and progress the verbs report with.

## What we use it for

`Program.cs` wires `Spectre.Console.Cli` to each verb, and every option and exit code is declared there. `kac.core`
references `Spectre.Console`, and its reporters build what a run prints.

One library now reads the command line and asks a question. `System.CommandLine` read the command line and
`Spectre.Console` asked the question, so the parser moved here in 0.2.1. Taken from the entry for that version in
`tooling/kac/CHANGELOG.md`.

## Status

**approved** since 2026-08-21.

## Accessibility

Assessed at Spectre.Console 0.57.x and Spectre.Console.Cli 0.55.x, by reading every call that colours or prompts.
[WCAG 2.2 AA] covers content a browser renders and applies to no terminal. [std-A11Y] states the rules for the
terminal, and this assessment checks `kac` against them.

Nothing the tool prints conveys meaning through colour alone. `Commands.Severity` pairs every severity with the word
`error`, `warning` or `info`. `Tag` and `Tally` print the word beside the colour. `--no-color` calls `Out.NoColor`,
which drops both consoles to `ColorSystem.NoColors`, and Spectre does the same for `NO_COLOR` in the environment.
Findings are laid out in a `Grid`, so no box-drawing character separates one column from the next. `--json` writes
straight to the stream and never through a renderer. Every prompt `ConsoleAsker` draws has a flag on the verb that
supplies the answer. `Asking.Asks` returns false where there is no terminal, so an unattended run finishes without
waiting.

Two things fall short. `SelectionPrompt` and `MultiSelectionPrompt` redraw the whole list on each keystroke, which a
screen reader may announce in full every time. `--yes` and the per-answer flags avoid both prompts. `Md.Snippet`
truncates a long value with a Unicode ellipsis, and nothing substitutes an ASCII form where the terminal reports no
Unicode support.

## Where it is used

* [svc-kac] uses both packages.

## Alternatives considered

* **[tol-system-commandline]**: it parsed the command line and rendered nothing, so `kac` needed a second library for
  output. Replacing it left one library.

## Licence and obligations

MIT. Nothing follows for a package this repository publishes.

## Related

* [std-A11Y] states the rules for the command line it renders.
* [tol-system-commandline] is the parser it replaced.

[WCAG 2.2 AA]: https://www.w3.org/TR/WCAG22/
[std-A11Y]: ../../standards/accessibility.md
[svc-kac]: ../../services/kac.md
[tol-system-commandline]: system-commandline.md
