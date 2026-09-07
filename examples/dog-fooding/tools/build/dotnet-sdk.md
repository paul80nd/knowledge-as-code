---
id: tol-dotnet-sdk
type: tool
tier: descriptive
status: approved
versions: 10.0.x
licence: MIT
decided-in:
replaces:
successor:
owner: paul.law
tags: [ dotnet, sdk ]
---

# .NET SDK

`Tool: tol-dotnet-sdk` `APPROVED`

The SDK that builds, tests and packs `kac`, and the runtime every other tool in this register sits on.

## What we use it for

Every project under `tooling/` targets `net10.0`. The SDK compiles them, runs the three test layers, and packs the CLI
as a dotnet tool. It also runs `tooling/kac-tests.cs` as a file-based program, with no project around it.

`actions/setup-dotnet` installs `10.0.x` in every job of every workflow here.

## Status

**approved** since 2026-08-03.

## Where it is used

* [svc-kac] is built, tested and packed with it.

The docs site and the marketplace branch are built by other toolchains, so neither carries it.

## Alternatives considered

None. The tool was written in C# from its first commit, so the SDK arrived with the language rather than as a choice
between candidates.

## Licence and obligations

MIT. Nothing follows for a package this repository publishes.

## Related

* [std-CONFIG] holds the pins that name the SDK version.

[std-CONFIG]: ../../standards/configuration.md
[svc-kac]: ../../services/kac.md
