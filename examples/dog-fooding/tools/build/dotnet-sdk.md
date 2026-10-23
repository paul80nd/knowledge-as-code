---
id: tol-dotnet-sdk
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:generic/dotnet-sdk, versions: 10.0.x }
homepage: https://dotnet.microsoft.com/
licence: >-
  MIT AND Apache-2.0 AND BSD-2-Clause AND BSD-3-Clause AND NCSA AND Unicode-DFS-2020 AND CC0-1.0 AND Zlib AND
  LicenseRef-Microsoft-DotNet-Library-License AND LicenseRef-Microsoft-Windows-SDK-License
licence-declared: MIT
decided-on: "2026-08-03"
review-by: "2027-08-03"
owner: human:paul.law
tags: [ dotnet, sdk ]
---

# .NET SDK

`Tool: tol-dotnet-sdk` `APPROVED`

The SDK that builds, tests and packs `kac`, and the runtime the register's NuGet packages run on.

## What we use it for

Every project under `tooling/` targets `net10.0`. The SDK compiles them, runs the three test layers, and packs the CLI
as a dotnet tool. It also runs `tooling/kac-tests.cs` as a file-based program that has no project around it.

`global.json` at the repository root pins the SDK at 10.0.402. Its `rollForward` is `disable`, so `dotnet` refuses
to build with any other SDK, even a later patch installed beside it. [tol-setup-dotnet] reads the file and installs
that version in every job that builds, tests, packs or publishes with `dotnet`. A contributor installs the same
version to build locally.

Dependabot raises each new 10.0 SDK as a pull request that moves the pin. The `versions` field approves the 10.0 line, so a pin
inside it needs no new decision, and Dependabot ignores a new major.

## Where it is used

* [svc-corpus-feed] is packed by `kac`, which runs on it.
* [svc-kac] is built, tested and packed with it.
* [svc-marketplace] is bundled by `kac`, which runs on it.

The docs site is built by another toolchain, so it does not use the SDK.

## Alternatives considered

None. The tool was written in C# from its first commit, so the SDK came with the language and nobody compared
candidates.

## Licence and obligations

Microsoft licenses the product distribution under MIT on Linux and macOS. On Windows most files are MIT too, and
Microsoft's [Windows licence information] names the files that are not:

* The [.NET Library License] covers `coreclr.dll` and `Microsoft.DiaSymReader.Native.*.dll`, among others.
* The [Windows SDK License] covers `D3DCompiler_47_cor3.dll`, which the WPF runtime uses.

The `round-trip` and `import-round-trip` jobs in `kac.yml` install the SDK on `windows-latest`, so `licence` includes
both. Neither has an SPDX identifier, so each is a `LicenseRef-`. The Windows zip of SDK 10.0.402 contains every file
the page names, and its root `LICENSE.txt` is the .NET Library License.

The SDK also puts a `ThirdPartyNotices.txt` of 56 notices beside its `LICENSE.txt`, and the file is the same on Windows
and macOS. Every notice is permissive or a public-domain dedication, and `licence` lists each one SPDX has an identifier
for. The rest are bare permission grants SPDX does not list: ISO 8879, RFC 3492, RFC 4122, and code its author released
with no named licence. Nothing in the file is copyleft.

Nothing follows for a package this repository publishes. The .NET Library License allows installing and using the
software to design, develop and test programs, which is what CI does with it. CI redistributes no part of the SDK.

## Related

* [std-CONFIG] covers the pins that state the SDK version.
* [tol-setup-dotnet] is the action that installs it.

[.NET Library License]: https://dotnet.microsoft.com/dotnet_library_license.htm
[Windows SDK License]: https://learn.microsoft.com/legal/windows-sdk/license
[Windows licence information]: https://github.com/dotnet/core/blob/main/license-information-windows.md
[std-CONFIG]: ../../standards/configuration.md
[svc-corpus-feed]: ../../services/corpus-feed.md
[svc-kac]: ../../services/kac.md
[svc-marketplace]: ../../services/marketplace.md
[tol-setup-dotnet]: ../workflows/setup-dotnet.md
