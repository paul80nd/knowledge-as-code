---
id: tol-dotnet-sdk
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:generic/dotnet-sdk, versions: 10.0.x }
homepage: https://dotnet.microsoft.com/
licence: MIT AND Apache-2.0 AND BSD-2-Clause AND BSD-3-Clause AND NCSA AND Unicode-DFS-2020 AND CC0-1.0 AND Zlib
licence-declared: MIT
decided-on: "2026-08-03"
review-by: "2027-08-03"
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

## Where it is used

* [svc-kac] is built, tested and packed with it.

The docs site and the marketplace branch are built by other toolchains, so neither uses it.

## Alternatives considered

None. The tool was written in C# from its first commit, so the SDK came with the language and nobody compared
candidates.

## Licence and obligations

Microsoft licenses the product distribution under MIT on Linux and macOS, and under the .NET Library License on
Windows. This repository builds on Linux and macOS only, so the concluded value leaves the Windows terms out.

The installed SDK puts an MIT `LICENSE.txt` beside a `ThirdPartyNotices.txt` of 56 notices. Every notice is permissive
or a public-domain dedication, and `licence` lists each one SPDX has an identifier for. The rest are bare permission
grants SPDX does not list: ISO 8879, RFC 3492, RFC 4122, and code its author released with no named licence. Nothing in
the file is copyleft.

Nothing follows for a package this repository publishes. CI installs the SDK, runs it, and redistributes no part of
it.

## Related

* [std-CONFIG] covers the pins that state the SDK version.

[std-CONFIG]: ../../standards/configuration.md
[svc-kac]: ../../services/kac.md
