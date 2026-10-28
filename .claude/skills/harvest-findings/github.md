# Triaging findings on GitHub

Read this where the tracker has a `target` of `github`. Each section serves one step of `SKILL.md`, and ends by sending
you back to it.

`tracker.base` is the repository, as `https://github.com/<owner>/<repo>`. Every command takes it as
`--repo <owner>/<repo>`.

## Signing in

```bash
gh auth status
```

It says whether you are signed in to the host. Go back to [Find the corpus first](SKILL.md#find-the-corpus-first).

## Making the labels

GitHub refuses an unknown label, and `gh issue edit` rejects the whole command when one label is missing.

Read first:

```bash
gh label list --repo <owner>/<repo> --limit 100 --json name --jq '.[].name'
```

Then, once they have agreed:

```bash
gh label create kac:finding --repo <owner>/<repo> --color 006B75 --force \
  --description "An observation an agent filed against a corpus"
gh label create kac:triaged --repo <owner>/<repo> --color BFDADC --force \
  --description "A finding triage has routed"
gh label create kac:route-framework --repo <owner>/<repo> --color 1D76DB --force \
  --description "The finding reports the framework rather than a record"
gh label create kac:route-record --repo <owner>/<repo> --color 0E8A16 --force \
  --description "A record here is wrong, or one is missing"
gh label create kac:route-misfiled --repo <owner>/<repo> --color FBCA04 --force \
  --description "The finding belongs to a corpus this one consumes"
gh label create kac:route-none --repo <owner>/<repo> --color CFD3D7 --force \
  --description "A duplicate, or already covered by a record"
gh label create kac:route-unclear --repo <owner>/<repo> --color D876E3 --force \
  --description "Triaged, and a person has to decide"
```

`--force` updates a label that is already there, so running this every time costs one call each and changes nothing.

Go back to [Making the labels](SKILL.md#making-the-labels).

## `triage` step 1: read the queue

The three reads, in order: the findings waiting, the bodies a label never reached, and the findings already routed.

```bash
gh issue list --repo <owner>/<repo> --state open --limit 200 \
  --search "label:kac:finding -label:kac:triaged" --json number,title,body,createdAt
gh issue list --repo <owner>/<repo> --state open --limit 200 \
  --search "kac-finding in:body -label:kac:finding" --json number,title,body,createdAt \
  --jq '[.[] | select(.body | contains("```yaml kac-finding"))]'
gh issue list --repo <owner>/<repo> --state all --limit 200 \
  --search "label:kac:triaged" --json number,title,labels
```

GitHub reads `kac-finding` as two words, so the second search returns any issue that discusses a finding. The `--jq`
filter keeps only a body with the opening fence in it. Mark each one it keeps:

```bash
gh issue edit <number> --repo <owner>/<repo> --add-label kac:finding
```

GitHub has no areas, so the queue is the whole repository.

Given one issue, read it with:

```bash
gh issue view <issue> --repo <owner>/<repo> --json number,title,body,labels,state
```

A GitHub body is the Markdown its author wrote, so read the block as it is. Go back to
[step 1](SKILL.md#1-read-the-queue).

## `triage` step 8: label, and comment the reason

```bash
gh issue edit <number> --repo <owner>/<repo> --add-label kac:triaged --add-label kac:route-record
gh issue comment <number> --repo <owner>/<repo> --body-file <path>
```

`--add-label` adds to the labels the issue has, so `kac:finding` stays. Step 9 posts the upstream body with the same
`gh issue comment`. GitHub has no areas, so nothing is bounced here.

Go back to [step 8](SKILL.md#8-label-and-comment-the-reason).

## `draft` step 1: check the issue

```bash
gh issue view <issue> --repo <owner>/<repo> --json state,labels,closedByPullRequestsReferences
```

`closedByPullRequestsReferences` lists every pull request that closes this issue, from the moment one opens. A `state`
of `CLOSED` is a closed finding. Go back to [step 1](SKILL.md#1-check-the-issue).

## `draft` step 3: the address in `sources`

```yaml
sources:
  - { resource: https://github.com/<owner>/<repo>/issues/<issue> }
```

Go back to [step 3](SKILL.md#3-write-the-record-or-the-edit).

## `draft` step 4: open the pull request

`publishing.base` is the repository, as `https://github.com/<owner>/<repo>`.

```bash
gh pr create --repo <owner>/<repo> --head <branch> --title "<the record, in one line>" --body-file <path>
```

**Write `Closes #<issue>` in the body.** GitHub closes the finding when somebody merges the pull request, and leaves
the link to it on the issue.

The command prints the pull request's URL. Go back to [step 4](SKILL.md#4-open-the-pull-request).
