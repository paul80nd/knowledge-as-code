---
id: tol-spectre-console
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

## Where it is used

* [svc-kac] carries both packages.

## Alternatives considered

* **[tol-system-commandline]**: it parsed the command line and rendered nothing, so the tool carried a second library
  for output. Replacing it left one.

## Licence and obligations

MIT. Nothing follows for a package this repository publishes.

## Related

* [tol-system-commandline] is what this took over from.

[svc-kac]: ../../services/kac.md
[tol-system-commandline]: system-commandline.md
