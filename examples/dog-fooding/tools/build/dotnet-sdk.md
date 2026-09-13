---
id: tol-dotnet-sdk
type: tool
tier: descriptive
status: approved
versions: 10.0.x
licence: MIT
owner: human:paul.law
tags: [ dotnet, sdk ]
---

# .NET SDK

`Tool: tol-dotnet-sdk` `APPROVED`

The SDK that builds, tests and packs `kac`, and the runtime every other tool in this register runs on.

## What we use it for

Every project under `tooling/` targets `net10.0`. The SDK compiles them, runs the three test layers, and packs the CLI
as a dotnet tool. It also runs `tooling/kac-tests.cs` as a file-based program that has no project around it.

`actions/setup-dotnet` installs `10.0.x` in every job of every workflow here.

## Status

**approved** since 2026-08-03.

## Where it is used

* [svc-kac] is built, tested and packed with it.

The docs site and the marketplace branch are built by other toolchains, so neither uses it.

## Alternatives considered

None. The tool was written in C# from its first commit, so the SDK came with the language and nobody compared
candidates.

## Licence and obligations

MIT. Nothing follows for a package this repository publishes.

## Related

* [std-CONFIG] covers the pins that state the SDK version.

[std-CONFIG]: ../../standards/configuration.md
[svc-kac]: ../../services/kac.md
