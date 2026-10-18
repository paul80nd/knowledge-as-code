---
id: ctl-0001
type: control
tier: normative
status: active
verifies: [ std-CONT ]
mechanism: not-enforced
applies-to:
  - all
review-by: "2027-10-17"
owner: human:paul.law
tags: [ base-images, containers, digests ]
---

# Nothing checks that an image is named by digest

`Control: ctl-0001` `ACTIVE`

No check fails a Dockerfile or a deployment that names an image by tag alone.

## What it checks

* [std-CONT.the-base-image-is-chosen-and-pinned] says a Dockerfile "**MUST** pin the base image by digest rather than by
  a moving tag".
* [std-CONT.a-published-tag-never-moves] says a deployment "**MUST** name an image by digest, so the running container
  is the one the pipeline built".

## How it works

Nothing runs. The check these rules need reads two kinds of file, and fails on any image reference without
`@sha256:`:

* every `FROM` line in every Dockerfile, as `grep -nE '^FROM ' Dockerfile | grep -v '@sha256:'` prints them
* every `image:` value in the deployment manifests, as `grep -nE '^\s*image:' deploy/*.yaml | grep -v '@sha256:'`
  prints them

A `FROM` that names a build argument passes only where that `ARG` has a default pinned by digest. A build argument with
no default lets the build pass a tag.

The same check can run in three places. Each one fails the fault at a different moment:

| Where              | When it fails                                                                  |
|--------------------|--------------------------------------------------------------------------------|
| A pipeline job     | On the pull request, and a merge waits for it.                                 |
| A pre-commit check | Before the commit, on the author's machine.                                    |
| A Claude Code hook | Before an agent writes the file, as a `PreToolUse` hook on `Write` and `Edit`. |

## Coverage and gaps

A reviewer reading the `FROM` line is the only thing that stops a tag-only base today.

The check would prove a reference cannot move. It would not prove these:

* the base comes from the organisation's registry, which a second pattern on the registry host would catch
* the base was refreshed in the last month
* a manifest the pipeline renders gets the digest the pipeline built, so the check has to read the rendered output
  where a placeholder such as `@${IMAGE_DIGEST}` stands in the source

[std-CONT.a-published-tag-never-moves]: ../standards/platform/containers.md#a-published-tag-never-moves
[std-CONT.the-base-image-is-chosen-and-pinned]: ../standards/platform/containers.md#the-base-image-is-chosen-and-pinned
