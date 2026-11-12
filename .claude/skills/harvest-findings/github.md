# Triaging findings on GitHub

Read this where the tracker has a `target` of `github`. Each section serves one step of `SKILL.md`, and ends by sending
you back to it.

This file states what each step achieves and what GitHub does that you could not work out, and gives no command. Write
each command yourself, for the shell and the client you have, because a command written for another shell fails in
yours. `gh` is the client that signs in to GitHub.

## What GitHub does

| Fact                                                                                                         | Why it matters                                                                                      |
|--------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------|
| `tracker.base` is the repository, as `https://github.com/<owner>/<repo>`                                     | every read and write addresses `<owner>/<repo>`                                                     |
| `gh` refuses a label the repository does not have, and refuses a whole edit where one label in it is missing | every label is made before step 8 applies the first                                                 |
| The REST API drops every label silently for an account without push access                                   | the check after each write is what shows a label that did not land                                  |
| Creating a label that already exists fails, unless the request asks to update a label already there          | a request that updates one can be sent for all seven labels and changes nothing                     |
| An issue list returns 30 issues unless it asks for more                                                      | a read that returned as many as it asked for has more behind it, so step 1 asks for 200 and says so |
| Search treats `kac-finding` as two words                                                                     | a body search for the block returns any issue that mentions a finding                               |
| Adding a label adds it to the set the issue has                                                              | `kac:finding` stays on                                                                              |
| GitHub has no areas                                                                                          | the queue is the whole repository, and nothing is bounced                                           |
| An issue body and a comment are stored as the Markdown they were sent                                        | the block reads as it was written, and a comment needs no marking                                   |
| `closedByPullRequestsReferences` lists every pull request that closes an issue, from the moment one opens    | it shows a finding a pull request already answers while the issue is still open                     |
| A pull request whose body says `Closes #<issue>` closes that issue on merge                                  | merging settles the finding, and the issue keeps a link to the pull request                         |

## Signing in

**Know that you are signed in to the repository's host** before the first read. Go back to
[Find the corpus first](SKILL.md#find-the-corpus-first).

## Making the labels

**Read the names of every label the repository has**, up to 100. Count the missing ones from the table below into what
you tell the person you are about to write.

**Once they have agreed, create each missing label** with the colour and the description the table gives. A request
that updates a label already there may send all seven instead.

| Label                 | Colour   | Description                                            |
|-----------------------|----------|--------------------------------------------------------|
| `kac:finding`         | `006B75` | An observation an agent filed against a corpus         |
| `kac:triaged`         | `BFDADC` | A finding triage has routed                            |
| `kac:route-framework` | `1D76DB` | The finding reports the framework rather than a record |
| `kac:route-record`    | `0E8A16` | A record here is wrong, or one is missing              |
| `kac:route-misfiled`  | `FBCA04` | The finding belongs to a corpus this one consumes      |
| `kac:route-none`      | `CFD3D7` | A duplicate, or already covered by a record            |
| `kac:route-unclear`   | `D876E3` | Triaged, and a person has to decide                    |

**Check by reading the labels again.** Every name in the table is there. Where one is missing, say which, and apply no
label that depends on it.

Go back to [Making the labels](SKILL.md#making-the-labels).

## `triage` step 1: read the queue

The first two reads return each issue's number, title, body and the day it was filed. The third returns its number,
title and labels. Ask for 200 issues a read.

| Read                      | State       | Search string                            | Then                                                      |
|---------------------------|-------------|------------------------------------------|-----------------------------------------------------------|
| the findings waiting      | open        | `label:kac:finding -label:kac:triaged`   | classify each one                                         |
| the bodies a label missed | open        | `kac-finding in:body -label:kac:finding` | keep only a body containing ```` ```yaml kac-finding ```` |
| the findings routed       | every state | `label:kac:triaged`                      | compare against them for duplicates                       |

**Mark each issue the second read keeps with `kac:finding`.** Check that the issue's labels now include it.

**Given one issue, read its number, title, body, labels and state.**

A GitHub body is the Markdown its author wrote, so read the block as it is. Go back to
[step 1](SKILL.md#1-read-the-queue).

## `triage` step 8: label, and comment the reason

**Add `kac:triaged` and the route label to the issue in one edit.** Then post the comment from its file. Step 9 posts
the upstream body as a comment the same way.

**Check what each write returned.**

* **The issue's labels include `kac:finding`, `kac:triaged` and the route.**
* **The comment reads as the file you wrote**, fences and backticks intact.

**Where a check fails, report the field and the issue's URL, and write nothing more to that issue.** The verdict is
already partly there, and a second attempt leaves two comments or two routes.

Go back to [step 8](SKILL.md#8-label-and-comment-the-reason).

## `draft` step 1: check the issue

**Read the issue's state, its labels and `closedByPullRequestsReferences`.** A `state` of `CLOSED` is a closed
finding, and an entry in `closedByPullRequestsReferences` is a pull request that already answers it. Go back to
[step 1](SKILL.md#1-check-the-issue).

## `draft` step 3: the address in `sources`

```yaml
sources:
  - { resource: https://github.com/<owner>/<repo>/issues/<issue> }
```

Go back to [step 3](SKILL.md#3-write-the-record-or-the-edit).

## `draft` step 4: open the pull request

`publishing.base` is the repository, as `https://github.com/<owner>/<repo>`.

**Open one pull request on that repository from your branch**, with the record as its title in one line and the body
from its file. **Write `Closes #<issue>` in the body**, so merging it closes the finding.

**Check the pull request.** The response gives its URL. Read the issue's `closedByPullRequestsReferences` again, and
it lists the pull request. Where it does not, report the pull request's URL and that the issue is unlinked, and open no
second pull request.

Go back to [step 4](SKILL.md#4-open-the-pull-request).
