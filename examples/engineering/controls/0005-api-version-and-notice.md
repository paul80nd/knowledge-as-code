---
id: ctl-0005
type: control
tier: normative
status: active
verifies: [ std-API ]
mechanism: not-enforced
applies-to:
  - all
review-by: "2027-10-17"
owner: human:paul.law
tags: [ api, deprecation, openapi, versioning ]
---

# Nothing checks that an interface change carries a version and a notice

`Control: ctl-0005` `ACTIVE`

No check fails a contract whose routes have no version, or a breaking change made inside a version.

## What it checks

* [std-API.a-change-carries-a-version-and-a-notice] says a published interface "**MUST** have a version a consumer can
  pin to".
* It says an HTTP interface "**SHOULD** put its major version in the path, as `/v2/`, unless the contract states another
  scheme".
* It says a breaking change "**MUST** ship as a new version, with the previous version still serving requests".
* It says a deprecated version "**SHOULD** respond with a `Deprecation` header and a `Sunset` header, so a consumer sees
  the notice in the response".

## How it works

Nothing runs. The checks these rules need are a pipeline job on each pull request, reading the committed OpenAPI
contract:

* `spectral lint contract/openapi.yaml --ruleset .spectral.yaml` fails a path without a version. The ruleset applies
  the `pattern` function to each key under `$.paths`, matching `^/v[0-9]+/`. A second rule fails an operation marked
  `deprecated: true` that declares no `Deprecation` or `Sunset` response header.
* `oasdiff breaking <contract on main> contract/openapi.yaml --fail-on ERR` fails a breaking change made inside a
  version that already exists.

The two spectral rules check a `SHOULD`. A repository departing from one turns that rule off in `.spectral.yaml`, with
a comment giving the reason. Where the contract states another versioning scheme, that comment names the scheme.

A pre-commit check could run the first command. A Claude Code hook could run it on `Write` and `Edit` of the contract.
The second needs the contract on `main`, so it belongs in the pipeline.

## Coverage and gaps

A reviewer reading the contract diff is the only thing that notices a missing version today.

Both checks read the contract, so they prove nothing about a route the contract leaves out.
[std-API.the-contract-is-the-source-of-truth] requires the build to fail where the implementation drifts from the
contract, and no control records a check of that. A header the contract declares is not proof the service sends it. Only
a test against the running service proves that. The notice period before a version is removed goes unchecked.

[std-API.a-change-carries-a-version-and-a-notice]: ../standards/common/api-contracts.md#a-change-carries-a-version-and-a-notice
[std-API.the-contract-is-the-source-of-truth]: ../standards/common/api-contracts.md#the-contract-is-the-source-of-truth
