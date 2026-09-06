---
id: rbk-nuget-404-on-publish
tier: procedural
status: active
applies-to: [ svc-kac ]
severity: sev3
last-rehearsed: "never"
rehearsal-frequency:
requires-access:
  - Actions, to read a run log and re-run a job on paul80nd/knowledge-as-code
  - The required reviewer on the nuget.org environment
owner: paul.law
tags: [ nuget, publishing ]
---

# nuget.org answers 404 for a version it has already accepted

`Runbook: rbk-nuget-404-on-publish` `ACTIVE`

## Symptoms

* `dotnet tool install --global KnowledgeAsCode.Tool --version <version>` fails with a 404, minutes after the release
  for that version appeared on GitHub.
* The package page on nuget.org does not list the version, and the tag and the release both exist.
* A re-run of `publish-tool.yml` prints `version <version> is not on nuget.org — it will be published`, for a version
  the earlier run already pushed.

What you are not seeing: any failure in the run that published. The push step reported success and the release step
wrote the tag.

## Immediate actions

1. Leave `<Version>` in `tooling/kac/kac.csproj` where it is. A published version is never replaced, only followed.
2. Tell anyone waiting on the version to hold off installing it.
3. Note the time the publish step reported success. The window is measured from there.

## Diagnosis

**Did the `Publish to nuget.org` step report success?**

* **Yes** → nuget.org accepted the version and has not finished listing it. Go to [Resolution](#resolution).
* **No** → this runbook does not cover it. Read the step's own error and [escalate](#escalation).

**Has it been less than fifteen minutes since that step succeeded?**

* **Yes** → wait. Go to [Resolution](#resolution).
* **No** → [escalate](#escalation).

## Resolution

**Do not re-run `publish-tool.yml` to force it.** The re-run pushes the same package again, and `--skip-duplicate` is
what keeps that from failing rather than what makes it work.

1. Wait fifteen minutes from the time the publish step reported success.
2. Ask the flat container what it now holds:

   ```sh
   curl -s https://api.nuget.org/v3-flatcontainer/knowledgeascode.tool/index.json | jq -r '.versions[]'
   ```

3. Confirm the version is in that list.
4. Install it, from a directory that is not a corpus:

   ```sh
   dotnet tool install --global KnowledgeAsCode.Tool --version <version>
   kac --version
   ```

Service is restored when `kac --version` prints the version you published.

## Escalation

| When                                                          | Who                            | How                                    |
|---------------------------------------------------------------|--------------------------------|----------------------------------------|
| The version is still unlisted an hour after a successful push | Paul Law, as the package owner | An issue on this repository            |
| The publish step itself failed                                | Paul Law, as the package owner | An issue on this repository            |
| The version is listed and installs still fail                 | nuget.org                      | https://www.nuget.org/policies/Contact |

## Afterwards

* No postmortem. This is a known property of the registry rather than a fault in the pipeline.
* Where the wait ran past an hour, record how long it took in the escalation issue. The fifteen minutes above can then
  be corrected.

## Related

* [svc-kac] is what this covers.

[svc-kac]: ../services/kac.md
