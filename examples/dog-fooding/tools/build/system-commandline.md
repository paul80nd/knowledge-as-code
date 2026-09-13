---
id: tol-system-commandline
type: tool
tier: descriptive
status: deprecated
licence: MIT
successor: tol-spectre-console
owner: human:paul.law
tags: [ cli, parser ]
---

# System.CommandLine

`Tool: tol-system-commandline` `DEPRECATED`

The command line parser `kac` was built on until 0.2.1, when [tol-spectre-console] took over parsing and rendering.

## What we use it for

Nothing. `kac` parsed its verbs and options with it from 2026-08-03 until 2026-08-21. No project references it today.

## Status

**deprecated** since 2026-08-21. [tol-spectre-console] replaced it in `kac` 0.2.1, and the package reference is
already gone.

Every verb, option and exit code behaved as before. `--help` reflowed into Spectre's layout, `-v` joined `--version`,
and `-?` stopped meaning `--help`. Taken from the entry for 0.2.1 in `tooling/kac/CHANGELOG.md`.

## Where it is used

Nowhere. No service in this catalogue uses it.

## Alternatives considered

* **[tol-spectre-console]**: it reads a command line and renders output, and `kac` needed both. Keeping this parser
  meant a second library for everything a verb prints.

## Licence and obligations

MIT. Nothing followed for a package this repository published.

## Related

* [tol-spectre-console] is the parser that replaced it.

[tol-spectre-console]: spectre-console.md
