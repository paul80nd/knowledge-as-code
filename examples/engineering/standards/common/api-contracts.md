---
id: std-API
type: standard
tier: normative
status: draft
implements: [ pol-INTC.BREAK, pol-INTC.EXPOSE, pol-INTC.HOLDS, pol-INTC.NOTICE, pol-INTC.SECURE, pol-INTC.SPEC,
  pol-INTC.VERSION ]
verified-by: [ ctl-0005 ]
applies-to:
  - all
review-by: "2027-08-31"
owner: human:paul.law
tags: [ api-versioning, contracts, interfaces ]
---

# An interface is described by a contract in the repository

`Standard: std-API` `DRAFT`

## Summary

Every interface we publish has a machine-readable contract, committed beside the code. The build checks the
implementation against that contract. A change a consumer cannot absorb ships as a new version.

## Rules

### SPEC: The contract is the source of truth

- A service **MUST** publish a machine-readable contract for every interface it exposes, committed to the repository
  that builds the service.
- Where the implementation and the contract disagree, a team **MUST** treat the implementation as the defect.
- A build **MUST** fail where the implementation no longer matches the contract.
- A consumer **SHOULD** test its use of an interface against the published contract, so the test fails on what the
  provider promised.
- A consumer **MAY** also test against a running instance of the provider.

_**Covers:** [pol-INTC].HOLDS, [pol-INTC].SPEC_

### AUTH: Every endpoint authenticates and validates

- A contract **MUST** state the authentication each endpoint takes.
- A contract **MUST** declare an endpoint that takes no authentication as public.
- An endpoint **MUST** authenticate the caller before it does any work, unless the contract declares it public.
- An endpoint **MUST** authorise the operation before it does any work, unless the contract declares it public.
- An endpoint **MUST** validate each field it receives against the contract, and reject a request that does not match.
- An interface **MUST NOT** serve personal data, a secret, or an operation that changes state, to an unauthenticated
  caller.

_**Covers:** [pol-INTC].EXPOSE, [pol-INTC].SECURE_

### VERSION: A change carries a version and a notice

- A published interface **MUST** have a version a consumer can pin to.
- An HTTP interface **SHOULD** put its major version in the path, as `/v2/`, unless the contract states another
  scheme.
- A breaking change **MUST** ship as a new version, with the previous version still serving requests.
- A team **MUST** give notice before it removes a version, on the timescale the deprecation approach states.
- A deprecated version **SHOULD** respond with a `Deprecation` header and a `Sunset` header, so a consumer sees the
  notice in the response.

_**Covers:** [pol-INTC].BREAK, [pol-INTC].NOTICE, [pol-INTC].VERSION_

## Examples

```
Good
  GET /v2/covers/{isbn}
  Deprecation: Sat, 01 Aug 2026 00:00:00 GMT      on /v1 only
  Sunset: Tue, 01 Dec 2026 00:00:00 GMT

Avoid
  GET /covers/{isbn}
  # the response gained a required field on Tuesday
```

The avoided form has no version to pin. A consumer written on Monday breaks on Tuesday, and finds out from a support
ticket.

```
Good
  covers-api/contract/openapi.yaml        committed, and the build asserts the routes against it

Avoid
  a Swagger page generated from the running service
```

A document generated from the implementation always agrees with the implementation, so it never reports a change
nobody meant to make.

## Conformance checklist

- [ ] The repository contains the contract, and a reviewer can read the diff to it in the pull request.
- [ ] The build fails when a route, a field or a status code drifts from the contract.
- [ ] A consumer can name the version it calls, and the previous version still responds.
- [ ] The contract states the authentication of every endpoint.
- [ ] Each endpoint the contract does not declare public rejects an unauthenticated call, confirmed by a test.
- [ ] No endpoint the contract declares public returns personal data or a secret, or changes state.
- [ ] A request with an unknown or malformed field is refused, not accepted and ignored.
- [ ] Notice of each version's removal went to its consumers before the removal, on the stated timescale.

## Rationale and provenance

A consumer plans against what we published. No consumer re-reads the implementation each morning, so the contract
states the promise and the version marks the change.

The build checks the implementation against the contract, which goes further than [Zalando's API-first rule], because
[pol-INTC].HOLDS asks for the contract to be verified. A version in the path departs from [Zalando rule 115], which
forbids it. [AIP-185] puts the major version in the path too. A consumer reads the version from the URL. A gateway
routes on it without opening the request.

## Sources and further reading

- **Normative.** [OpenAPI 3.1] sets the form a contract takes in this estate. This standard says what the document
  covers, and changes nothing about its grammar.
- **Normative.** [RFC 9745] defines the `Deprecation` header field. [RFC 8594] defines the `Sunset` header field, which
  gives the removal date.

## Changelog

- 2026-10-18: graded the rules. A version in the path, the `Deprecation` and `Sunset` headers, and a consumer testing
  against the contract become `SHOULD`. An endpoint the contract declares public needs no authentication.

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[AIP-185]: https://google.aip.dev/185
[OpenAPI 3.1]: https://spec.openapis.org/oas/v3.1.1.html
[RFC 8594]: https://www.rfc-editor.org/rfc/rfc8594.html
[RFC 9745]: https://www.rfc-editor.org/rfc/rfc9745.html
[Zalando rule 115]: https://opensource.zalando.com/restful-api-guidelines/#115
[Zalando's API-first rule]: https://opensource.zalando.com/restful-api-guidelines/#100
[pol-INTC]: ../../policies/delivery/intc-interface-contracts.md#clauses
