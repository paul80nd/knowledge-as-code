---
id: std-API
type: standard
tier: normative
status: draft
implements: [ pol-INTC.BREAK, pol-INTC.EXPOSE, pol-INTC.HOLDS, pol-INTC.NOTICE, pol-INTC.SECURE, pol-INTC.SPEC,
  pol-INTC.VERSION ]
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

### The contract is the source of truth

- A service **MUST** publish a machine-readable contract for every interface it exposes, committed to the repository
  that builds the service.
- Where the implementation and the contract disagree, a team **MUST** treat the implementation as the defect.
- A build **MUST** fail where the implementation no longer matches the contract.
- A consumer **MUST** test against the published contract, not against a running instance of the provider.

_**Covers:** [pol-INTC].HOLDS, [pol-INTC].SPEC_

### Every endpoint authenticates and validates

- An endpoint **MUST** authenticate the caller before it does any work.
- An endpoint **MUST** authorise the operation before it does any work.
- An endpoint **MUST** validate each field it receives against the contract, and reject a request that does not match.
- An interface **MUST NOT** serve personal data, a secret, or an operation that changes state, to an unauthenticated
  caller.

_**Covers:** [pol-INTC].EXPOSE, [pol-INTC].SECURE_

### A change carries a version and a notice

- A published interface **MUST** have a version a consumer can pin to.
- A breaking change **MUST** ship as a new version, with the previous version still serving requests.
- A team **MUST** give notice before it removes a version, on the timescale the deprecation approach states.
- A deprecated version **MUST** respond with a `Deprecation` header and a `Sunset` header, so a consumer sees the
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
- [ ] Every path has a version segment, and the previous version still responds.
- [ ] Each endpoint rejects an unauthenticated call, confirmed by a test.
- [ ] A request with an unknown or malformed field is refused, not accepted and ignored.
- [ ] Each deprecated version returns a removal date in its response headers.

## Rationale and provenance

A consumer plans against what we published. No consumer re-reads the implementation each morning, so the contract
states the promise and the version marks the change.

## Sources and further reading

- **Normative.** [OpenAPI 3.1] sets the form a contract takes in this estate. This standard says what the document
  covers, and changes nothing about its grammar.
- **Normative.** [RFC 9745] defines the `Deprecation` header field. [RFC 8594] defines the `Sunset` header field, which
  gives the removal date.

[OpenAPI 3.1]: https://spec.openapis.org/oas/v3.1.1.html
[RFC 8594]: https://www.rfc-editor.org/rfc/rfc8594.html
[RFC 9745]: https://www.rfc-editor.org/rfc/rfc9745.html
[pol-INTC]: ../../policies/delivery/intc-interface-contracts.md#clauses
