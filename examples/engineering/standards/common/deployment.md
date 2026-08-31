---
id: std-DEPLOY
tier: normative
status: draft
implements: [ pol-ENVS.PROMOTE, pol-ENVS.SAMEDEF, pol-PIPE.ASCODE, pol-PIPE.CONFIG, pol-PIPE.DEPLOY, pol-PIPE.LOCAL,
  pol-PIPE.MANUAL, pol-PIPE.REVERT, pol-PIPE.SAMEART, pol-PIPE.TRACE ]
applies-to:
  - all
review-by: "2027-08-31"
owner: paul.law
tags: [ artefacts, deployment, release ]
---

# One artefact is built once and promoted to production

`Standard: std-DEPLOY` `DRAFT`

## Summary

The pipeline builds an artefact once, then promotes that same artefact through each environment. Configuration arrives
from outside it, and the only route into production is the pipeline.

## Rules

### The artefact is built once

- A pipeline **MUST** build the artefact once, and promote that build to every environment.
- An artefact **MUST** carry the commit it was built from, so a running version resolves to a change.
- A deployment record **MUST** name the artefact, the change and the approval behind it.
- A pipeline **MUST NOT** deploy an artefact built anywhere but the pipeline.

_**Covers:** [pol-PIPE].LOCAL, [pol-PIPE].SAMEART, [pol-PIPE].TRACE_

### Configuration sits outside the artefact

- A service **MUST** read environment-specific configuration at start-up, from outside the artefact.
- An artefact **MUST NOT** carry a hostname, connection string or feature setting that differs between environments.

_**Covers:** [pol-PIPE].CONFIG_

### Production changes through the pipeline alone

- A change **MUST** reach production through the automated pipeline.
- A change **MUST** reach each environment below production by promotion from the one before it.
- Every environment **MUST** be provisioned from the same definition, parameterised per environment.
- The pipeline definition **MUST** live in the repository and be reviewed like the code.
- A team **MUST NOT** hand-edit production code, configuration, infrastructure or schema.

_**Covers:** [pol-ENVS].PROMOTE, [pol-ENVS].SAMEDEF, [pol-PIPE].ASCODE, [pol-PIPE].DEPLOY, [pol-PIPE].MANUAL_

### A way back exists before the change goes

- A release **MUST** have a stated rollback or recovery path before it starts.
- A database migration **MUST** leave the previous version of the service able to run against the new schema.

_**Covers:** [pol-PIPE].REVERT_

## Examples

```
Good
  build   -> covers-api:1.14.0+9f3c1ab        one image
  deploy  -> dev, then test, then prod        the same image, three settings files

Avoid
  build   -> covers-api:dev
  build   -> covers-api:prod                  built again, from the same branch, an hour later
```

The avoided form tests one image and ships another. The two differ by whatever moved in that hour: a base image tag, a
transitive package, a build agent.

## Conformance checklist

- [ ] The image or package running in production carries the same digest as the one tested below it.
- [ ] The artefact reports the commit it was built from, on a health or version endpoint.
- [ ] No configuration file inside the artefact names an environment.
- [ ] The pipeline definition is in the repository, and its last change went through review.
- [ ] Production access allows deployment through the pipeline identity alone.
- [ ] The last release's rollback path is written down, and somebody has run it in an environment below production.

## Rationale and provenance

An artefact rebuilt per environment is a different artefact, and the testing done below production applied to the other
one. Building once makes the thing we tested the thing we ship.

[pol-ENVS]: ../../policies/security/envs-environment-separation.md#clauses
[pol-PIPE]: ../../policies/delivery/pipe-pipeline-to-production.md#clauses
