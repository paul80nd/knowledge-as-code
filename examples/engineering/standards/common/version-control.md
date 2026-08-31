---
id: std-VCS
tier: normative
status: draft
implements:
  - pol-EVER
applies-to:
  - all
review-by: "2027-08-26"
owner: paul.law
tags: [ change-management, repositories, source-control ]
---

# Everything a service needs is committed to its repository

`Standard: std-VCS` `DRAFT`

## Summary

One repository holds everything that builds, deploys, runs and recovers a service, and every change to it arrives by
reviewed merge from an identified author.

## Rules

### The repository holds everything the service needs

- A repository **MUST** hold the source, the build definition, the infrastructure definition, the configuration
  templates and the recovery scripts for the services it owns ([pol-EVER].ASSETS).
- A repository **MUST NOT** depend on an asset held only on a workstation, in a cloud portal or in an unversioned
  share ([pol-EVER].ORPHAN).
- Infrastructure, schema and configuration changes **MUST** go through the same review as application code
  ([pol-EVER].PARITY).

### Every change is attributable and reviewed

- The default branch **MUST** refuse a direct push, so that every change arrives as a merge ([pol-EVER].BRANCH).
- A commit subject **MUST** name the work item that asked for the change, written as `#<id>` ([pol-EVER].INTENT).
- A commit **MUST** carry the author's own verified identity ([pol-EVER].HISTORY).
- A commit **MUST NOT** be authored by a shared or generic account ([pol-EVER].SHARED).

## Examples

```
Good
  #4812 Move the retry budget out of the handler

Avoid
  fixes
```

A subject naming no work item leaves the next reader with the diff and nothing else. Git records what changed and the
work item records what asked for it.

```
Good
  services/covers/infra/storage.bicep    committed beside the code it provisions

Avoid
  a storage account created in the portal, with the code that reads it in git
```

The second cannot be rebuilt from the repository, so the recovery path runs through whoever remembers the portal.

## Conformance checklist

- [ ] The repository builds from a clean clone on a machine that has never seen this service.
- [ ] The infrastructure definition sits in the repository, and the running estate matches it.
- [ ] Branch protection is on, and it blocks direct pushes to the default branch.
- [ ] Recent commit subjects each name a work item.
- [ ] No commit in the last release carries a shared account as its author.

## Rationale and provenance

A team that can rebuild a service from source follows a recovery procedure. A team missing one asset investigates
instead. We also cannot ask an author about a change a year later if we cannot tell who made it.

- [pol-EVER] commits us to holding everything in version control, with a history that attributes each change.

[pol-EVER]: ../../policies/delivery/ever-everything-in-version-control.md#clauses
