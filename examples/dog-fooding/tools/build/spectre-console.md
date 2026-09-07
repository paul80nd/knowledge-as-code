---
id: tol-spectre-console
type: tool
tier: descriptive
status: approved
versions: Spectre.Console 0.57.x, Spectre.Console.Cli 0.55.x
licence: MIT
decided-in:
replaces: tol-system-commandline
successor:
owner: paul.law
tags: [ cli, console, terminal ]
---

# Spectre.Console

`Tool: tol-spectre-console` `APPROVED`

Two packages that give `kac` its command line and everything it prints: `Spectre.Console.Cli` reads the command line,
and `Spectre.Console` renders the tables, colours and progress the verbs report with.

## What we use it for

`Program.cs` wires `Spectre.Console.Cli` to each verb, and every option and exit code is declared there.
`Spectre.Console` is referenced by `kac.core`, where the reporters build what a run prints.

One library reads a command line and asks a question, rather than two. That is why the parser moved here in 0.2.1.
Taken from the entry for that version in `tooling/kac/CHANGELOG.md`.

## Status

**approved** since 2026-08-21.

## Accessibility

Assessed at Spectre.Console 0.57.x and Spectre.Console.Cli 0.55.x, by reading every call that colours or prompts.
[WCAG 2.2 AA] is written for content a browser renders and reaches no terminal, so [std-A11Y] states what `kac` owes a
reader instead, and this assessment answers those rules.

Nothing the tool prints carries meaning in colour alone. `Commands.Severity` pairs every severity with the word
`error`, `warning` or `info`, and `Tag` and `Tally` print the word beside the colour. `Out.NoColor` drops both consoles
to `ColorSystem.NoColors`, which is what `--no-color` reaches, and Spectre does the same for `NO_COLOR` in the
environment. Findings are laid out in a `Grid`, so no box-drawing character separates one column from the next.
`--json` writes straight to the stream and never through a renderer. Every prompt `ConsoleAsker` draws has a flag on
the verb that answers it, and `Asking.Asks` returns false where there is no terminal, so an unattended run finishes
rather than waiting.

Two things fall short. `SelectionPrompt` and `MultiSelectionPrompt` redraw the whole list on each keystroke, which a
screen reader may announce in full every time. `--yes` and the per-answer flags are the route round both.
`Md.Snippet` truncates a long value with a Unicode ellipsis, and nothing substitutes an ASCII form where the terminal
reports no Unicode support.

## Where it is used

* [svc-kac] carries both packages.

## Alternatives considered

* **[tol-system-commandline]**: it parsed the command line and rendered nothing, so the tool carried a second library
  for output. Replacing it left one.

## Licence and obligations

MIT. Nothing follows for a package this repository publishes.

## Related

* [std-A11Y] states what the command line it renders owes a reader.
* [tol-system-commandline] is what this took over from.

[WCAG 2.2 AA]: https://www.w3.org/TR/WCAG22/
[std-A11Y]: ../../standards/accessibility.md
[svc-kac]: ../../services/kac.md
[tol-system-commandline]: system-commandline.md
