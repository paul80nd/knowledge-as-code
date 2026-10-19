---
id: tol-nuget-login
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:github/nuget/login, versions: 1.x }
homepage: https://github.com/NuGet/login
licence: Apache-2.0 AND MIT
licence-declared: Apache-2.0
decided-on: "2026-08-20"
review-by: "2027-08-20"
owner: human:paul.law
tags: [ github-actions, nuget, oidc, trusted-publishing ]
---

# NuGet/login

`Tool: tol-nuget-login` `APPROVED`

The action that exchanges the publishing job's identity for a short-lived nuget.org API key. That key is the credential
`kac` is published with.

## What we use it for

The `publish` job of `publish-tool.yml` runs it in the step before `dotnet nuget push`. The job declares
`contents: write` and `id-token: write`, and runs in the `nuget.org` environment, which waits for a person to approve.

At the pinned commit, the action does these things in order:

1. It requests an OIDC token from GitHub, with the audience `https://www.nuget.org`.
2. It posts that token and the `user` input to `https://www.nuget.org/api/v2/token`.
3. It outputs the API key nuget.org returns as `NUGET_API_KEY`.

It masks the request token, the OIDC token and the API key in the log. It fails where the job lacks `id-token: write`.
The step passes `user` from the `NUGET_USER` secret, and the push step reads the key through `env:`.

The NuGet organisation on GitHub maintains it. It has tagged `v1.1.0` and `v1.2.0`, and its `v1` tag points at the
same commit as `v1.2.0`. The step pins that commit, and Dependabot moves the pin. `versions` approves the major the
pin sits in.

## Where it is used

* [svc-kac] is published to nuget.org with the key it returns.

## Alternatives considered

None recorded.

## Licence and obligations

The repository's `LICENSE` is Apache-2.0, and its `package.json` declares no licence. The action runs `dist/index.js`,
a bundle of `@actions/core` and the packages it depends on. The repository ships no list of what the bundle contains.
`package-lock.json` pins `@actions/core`, `@actions/exec`, `@actions/http-client`, `@actions/io`, `@fastify/busboy`,
`tunnel` and `undici`. The npm registry lists each at its pinned version as MIT.

The runner fetches the action for the job, and nothing here redistributes it, so nothing follows for anything this
repository publishes.

## Related

* [std-CI] requires a publish to nuget.org to authenticate through a trusted publishing policy.
* [tol-gh] tags the release in the same job, once the push has succeeded.

[std-CI]: ../../standards/workflows.md
[svc-kac]: ../../services/kac.md
[tol-gh]: ../release/gh.md
