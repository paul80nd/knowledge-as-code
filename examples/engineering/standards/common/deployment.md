---
id: std-DEPLOY
type: standard
tier: normative
status: draft
implements: [ pol-ENVS.PROMOTE, pol-ENVS.SAMEDEF, pol-PIPE.ASCODE, pol-PIPE.CONFIG, pol-PIPE.DEPLOY, pol-PIPE.GATES,
  pol-PIPE.LOCAL, pol-PIPE.MANUAL, pol-PIPE.REVERT, pol-PIPE.SAMEART, pol-PIPE.TRACE ]
verified-by: [ ctl-0002 ]
applies-to:
  - all
review-by: "2027-08-31"
owner: human:paul.law
tags: [ artefacts, deployment, release ]
---

# One artefact is built once and promoted to production

`Standard: std-DEPLOY` `DRAFT`

## Summary

The pipeline builds an artefact once, then promotes that same artefact through each environment. Configuration comes
from outside the artefact. The pipeline is the only route into production. Somebody other than a change's author
approves it before it goes.

## Rules

### The artefact is built once

- A pipeline **MUST** build the artefact once, and promote that build to every environment.
- An artefact **MUST** record the commit it was built from, so a running version identifies the change it came from.
- A deployment record **MUST** name the artefact, the change and the approval behind it.
- A pipeline **MUST NOT** deploy an artefact built anywhere but the pipeline.

_**Covers:** [pol-PIPE].LOCAL, [pol-PIPE].SAMEART, [pol-PIPE].TRACE_

### Configuration sits outside the artefact

- A service **MUST** read environment-specific configuration at start-up, from outside the artefact.
- An artefact **MUST NOT** contain a hostname, connection string or feature setting that differs between environments.

_**Covers:** [pol-PIPE].CONFIG_

### Production changes through the pipeline alone

- A change **MUST** reach production through the automated pipeline.
- A change **MUST** reach each environment below production by promotion from the one before it.
- Every environment **MUST** be provisioned from the same definition, parameterised per environment.
- The pipeline definition **MUST** be committed to the repository, and reviewed like the code.
- A team **MUST NOT** hand-edit production code, configuration, infrastructure or schema, other than a flag change
  made under [pol-PIPE].FLAGS.
- A team changing production by hand to restore service, where the pipeline is unavailable or its delay would extend
  an outage, **MUST** record a deviation at the time.
- A team **MUST** commit a hand change made to restore service to the repository before the incident closes.

_**Covers:** [pol-ENVS].PROMOTE, [pol-ENVS].SAMEDEF, [pol-PIPE].ASCODE, [pol-PIPE].DEPLOY, [pol-PIPE].MANUAL_

### Change approval

- A change **MUST** be approved by someone other than its author before it reaches production.
- The approval **MUST** be recorded where the change is, such as in branch protection.
- A change of a higher-risk kind **MUST** carry an approver with authority for that risk. The higher-risk kinds are a
  schema or data migration, a change to access or security configuration, and infrastructure that holds data.
- A change **MAY** bypass approval during an incident.
- A change that bypassed approval **MUST** be reviewed by a second person within two working days of the change
  reaching production.
- That review **MUST** be recorded against the change.

_**Covers:** [pol-PIPE].GATES_

### A way back exists before the change goes

- A release **MUST** have a stated rollback or recovery path before it starts.
- A database migration **MUST** leave the previous version of the service able to run against the new schema, unless
  the migration removes data the previous version reads.

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

The avoided form tests one image and ships another. They differ by whatever moved in that hour: a base image tag, a
transitive package, a build agent.

## Conformance checklist

- [ ] The image or package running in production has the same digest as the one tested below it.
- [ ] The artefact reports the commit it was built from, on a health or version endpoint.
- [ ] No configuration file inside the artefact names an environment.
- [ ] The pipeline definition is in the repository, and its last change went through review.
- [ ] Production access allows deployment through the pipeline identity alone, apart from a hand change made to
  restore service.
- [ ] Every hand change to production has a deviation, and the change is in the repository.
- [ ] Every change in the last release has an approval recorded against it by somebody other than its author, given
  before the change reached production.
- [ ] Each higher-risk change in the last release has an approver with authority for that risk.
- [ ] Every change that bypassed approval has a second person's review recorded against it within two working days
  of the change reaching production.
- [ ] The last release's rollback path is written down, and somebody has run it in an environment below production.

## Rationale and provenance

An artefact rebuilt for each environment is a different artefact, so the testing done below production applied to a
different build. Building once means we ship what we tested.

A migration removing data the previous version reads cannot keep that version running. The rollback or recovery path
for such a release is a restore.

The emergency route is the exception [pol-PIPE] already grants. It matches the break-glass practice in
[AWS Well-Architected SEC03-BP03] and [Microsoft Entra's emergency access accounts]: used only when the normal route is
unavailable, and recorded at the time.

Change approval keeps what a change board gave and automation does not: a second person, and authority matched to the
risk. Traceability, rollback and tests already sit under [pol-PIPE].TRACE, [pol-PIPE].REVERT and [std-GATES].

## Sources and further reading

- **Informative.** [ISO 27001:2022].A.8.32 is the change management control the change approval rules are drawn from.

## Changelog

- 2026-10-21: added change approval: an approver other than the author, an approver with authority for a higher-risk
  change, and an emergency path reviewed within two working days.
- 2026-10-18: added the emergency route for a hand change and the flag exception, both from [pol-PIPE]. A migration
  removing data the previous version reads is exempt from keeping that version running.

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[AWS Well-Architected SEC03-BP03]: https://docs.aws.amazon.com/wellarchitected/latest/security-pillar/sec_permissions_emergency_process.html
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[Microsoft Entra's emergency access accounts]: https://learn.microsoft.com/entra/identity/role-based-access-control/security-emergency-access
[pol-ENVS]: ../../policies/security/envs-environment-separation.md#clauses
[pol-PIPE]: ../../policies/delivery/pipe-pipeline-to-production.md#clauses
[std-GATES]: build-gates.md
