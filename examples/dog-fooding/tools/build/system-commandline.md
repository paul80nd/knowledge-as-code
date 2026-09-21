---
id: tol-system-commandline
type: tool
tier: descriptive
status: deprecated
packages:
  - { purl: pkg:nuget/System.CommandLine }
homepage: https://github.com/dotnet/command-line-api
licence: MIT
licence-declared: MIT
decided-on: "2026-08-21"
review-by: "2027-08-21"
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

[tol-spectre-console] reads a command line and renders output, and `kac` needed both. Keeping this parser meant a
second library for everything a verb prints, so 0.2.1 dropped it. The package reference is already gone.

Every verb, option and exit code behaved as before. `--help` reflowed into Spectre's layout, `-v` joined `--version`,
and `-?` stopped meaning `--help`. Taken from the entry for 0.2.1 in `tooling/kac/CHANGELOG.md`.

## Where it is used

Nowhere. No service in this catalogue uses it.

## Licence and obligations

MIT. Nothing followed for a package this repository published.

## Related

* [tol-spectre-console] is the parser that replaced it.

[tol-spectre-console]: spectre-console.md
