---
id: tol-system-commandline
type: tool
tier: descriptive
status: deprecated
versions:
licence: MIT
decided-in:
replaces:
successor: tol-spectre-console
owner: paul.law
tags: [ cli, parser ]
---

# System.CommandLine

`Tool: tol-system-commandline` `DEPRECATED`

The command line parser `kac` was built on until 0.2.1, when [tol-spectre-console] took over both jobs.

## What we use it for

Nothing. `kac` parsed its verbs and options with it from 2026-08-03 until 2026-08-21, and no project references it
today.

## Status

**deprecated** since 2026-08-21. [tol-spectre-console] replaced it in `kac` 0.2.1, and the reference is gone rather
than pending.

Every verb, option and exit code answered as before. `--help` reflowed into Spectre's layout, `-v` joined `--version`,
and `-?` stopped standing for `--help`. Taken from the entry for 0.2.1 in `tooling/kac/CHANGELOG.md`.

## Where it is used

Nowhere. No service in this catalogue carries it.

## Alternatives considered

* **[tol-spectre-console]**: it reads a command line and renders output, and the tool wanted both. Carrying this parser
  meant carrying a second library for everything a verb prints.

## Licence and obligations

MIT. Nothing followed for a package this repository published.

## Related

* [tol-spectre-console] is what replaced it.

[tol-spectre-console]: spectre-console.md
