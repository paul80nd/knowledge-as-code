# Triaging findings on Azure DevOps

Read this where the tracker has a `target` of `azure-devops`. Each section serves one step of `SKILL.md`, and ends by
sending you back to it.

This file states what each step achieves and what Azure DevOps does that you could not work out, and gives no command.
Write each command yourself, for the shell and the client you have, because a command written for another shell fails
in yours.

## The organisation, the project and the area path

`tracker.base` is the project, as `https://dev.azure.com/<org>/<project>`. A client wants its parts apart.

| Part             | What it is                                    | Where it goes                                      |
|------------------|-----------------------------------------------|----------------------------------------------------|
| the organisation | `base` up to and including `<org>`            | the organisation a client is given                 |
| the project      | the segment after `<org>`                     | the project a client is given, an area path, a URL |
| the area path    | the project and `area`, joined by a backslash | a WIQL condition, `System.AreaPath`                |

**Decode the project name everywhere but a URL.** `base` arrives percent-encoded, and a base ending
`Engineering%20Standards` names the project `Engineering Standards`. Give the decoded name to a client and to an area
path. Copy it into a REST URL as `base` gives it.

**Write an area path with backslashes.** `area` separates its segments with `/`, and Azure Boards separates an area
path with `\`. An `area` of `kac-it-swdev/subteam` is `<project>\kac-it-swdev\subteam`. A tracker with no `area` has
the project alone as its area path.

## What Azure DevOps does

| Fact                                                                                                                                                                              | Why it matters                                                                                             |
|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------|
| `az rest` takes the application id of Azure DevOps, `499b84ac-1321-427f-aa17-267ca6975798`, as its `--resource`                                                                   | it tells `az` which token signs the call                                                                   |
| `az rest` signs in through `az login`, and a personal access token set up for `az devops login` does not reach it                                                                 | a sign-in failure is fixed by `az login`, and by nothing else                                              |
| `az boards` and `az repos` come from the `azure-devops` extension, and `az rest` needs none                                                                                       | a missing extension fails before the call is made                                                          |
| On Windows `az` runs as `az.cmd`, so `cmd.exe` reads every argument again                                                                                                         | `cmd.exe` cuts an argument at `&`, even inside quotes, so pass a second query parameter apart from the URL |
| `$expand` and `$Issue` are literal                                                                                                                                                | a shell that expands a `$` name sends a request without it                                                 |
| `az` reads an argument that opens with `@` from the file it names, and sends the text itself where it cannot read the file. PowerShell reads a bare `@` as its splatting operator | a body that never left its file shows only in the response                                                 |
| A JMESPath query naming `System.Tags` needs escaping that differs between shells                                                                                                  | a field read out of the JSON a call returns needs no escaping in any shell                                 |
| A tag is what this platform calls a label, and Azure creates a tag it does not already have                                                                                       | there is no label to make, and no finding is filed unmarked                                                |
| `fields."System.Tags"` is one string, its tags separated by `; `                                                                                                                  | a tag is found by splitting it                                                                             |
| A write to `System.Tags` may replace the set it finds                                                                                                                             | a write that sends only the new tags can take `kac:finding` off                                            |
| `Closed`, `Done` and `Removed` close an item in every process Azure ships                                                                                                         | Agile closes an Issue as `Closed`, and Basic as `Done`                                                     |
| A work item is read at `_apis/wit/workitems/<id>` with `api-version=7.1`, and `$expand=relations` brings its links                                                                | the links are where a linked pull request shows                                                            |
| `multilineFieldsFormat."System.Description"` says how the body is stored                                                                                                          | a Markdown body and an HTML body are read differently                                                      |
| A comment is posted at `_apis/wit/workItems/<id>/comments` with `format=markdown` and `api-version=7.1-preview.4`, as `application/json` whose one key is `text`                  | `format=markdown` stores it as Markdown, so a backtick, a quotation mark and a fence all survive           |
| A JSON string takes a backslash before each backslash and each double quote, `\n` for each line break and `\t` for each tab                                                       | a raw quote or line break ends the string early, and the comment is refused or cut                         |
| `System.AreaPath` is a plain field, so any client that updates a work item sets it                                                                                                | a bounce needs no REST call                                                                                |
| A linked pull request is a relation whose `attributes.name` is `Pull Request` and whose `rel` is `ArtifactLink`. Its `url` ends with the pull request's id, after the last `%2F`  | that relation is what shows a finding a pull request already answers                                       |
| A pull request created with a work item linked and set to transition moves that work item on when somebody completes it                                                           | that link settles the finding, so the body needs no `Closes` line                                          |
| Azure DevOps refuses a pull request description over 4,000 characters                                                                                                             | a longer body opens no pull request                                                                        |

## Signing in

**Know that `az` is signed in through `az login`** before the first read, because `az rest` reaches nothing without
it. Where it is not, say that `az login` is the sign-in this skill needs. Go back to
[Find the corpus first](SKILL.md#find-the-corpus-first).

## Making the labels

Azure creates a tag it does not already have. There is nothing to make, so there is nothing to list or count. Go back
to [Making the labels](SKILL.md#making-the-labels).

## `triage` step 1: read the queue

Azure creates any tag it is given, so a backlog here has no unmarked finding and the second read does not apply. Run
the other two as WIQL queries on the project.

| Read                 | Returns                                      | Conditions                                                                                                                                                                            |
|----------------------|----------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| the findings waiting | `[System.Id]`                                | `[System.AreaPath] UNDER '<area path>' AND [System.Tags] CONTAINS 'kac:finding' AND [System.Tags] NOT CONTAINS 'kac:triaged' AND [System.State] NOT IN ('Closed', 'Done', 'Removed')` |
| the findings routed  | `[System.Id], [System.Title], [System.Tags]` | `[System.AreaPath] UNDER '<area path>' AND [System.Tags] CONTAINS 'kac:triaged'`                                                                                                      |

* **`UNDER` takes in the areas below it too.**
* **Drop the `[System.AreaPath]` condition and its `AND` where the tracker states no area.**

**Read each work item the first query returned, and the one issue you were given, through the REST API**, with its
relations. The body is `fields."System.Description"`, and `fields."System.Tags"` has the tags.

**Read the format before you read the block.**

* **`Markdown`, in upper or lower case:** the body is the Markdown `raise-finding` wrote, with its HTML escaped. Azure
  stores a `>` as `&gt;`, so `provenance: >` arrives as `provenance: &gt;`. Decode the entities the HTML case lists,
  in its order, and then read the block.
* **Absent, or anything else:** the body is HTML. An item filed by hand, or before `raise-finding` marked its body
  Markdown, arrives this way. Read the text out of it: start a line at each `<br>`, `</p>`, `</div>` and `</li>`, drop
  every other tag, and decode `&lt;`, `&gt;`, `&quot;`, `&#39;`, `&nbsp;` and then `&amp;`. The fences may be gone, so
  find the block by its keys. Say in the triage comment that the body arrived as HTML.

Go back to [step 1](SKILL.md#1-read-the-queue).

## Write a comment

Step 8 comments the reason, step 9 posts the upstream body, and a bounce says why it moved. Each one is a comment
posted the same way.

**Write the comment to a JSON file with your own file tool**, as its one key `text`, written as a JSON string. Then
post it through the REST API as Markdown.

**Check the comment the call returns.** Its `format` is `markdown`, and its `text` reads as the comment you wrote.
Where either check fails, report the field and the work item's link, and post nothing again. The comment is there, and
a second leaves two.

Go back to the section that sent you here.

## Bounce a work item

**Set `System.AreaPath` to the other address's area path.** Then comment why it moved, and the corpus whose area it
moved to, as [Write a comment](#write-a-comment) does.

**Check the work item the update returns.** `fields."System.AreaPath"` is the new area path, and `fields."System.Tags"`
still includes `kac:finding`. Where either check fails, report the field and the work item's link, and write nothing
more to it.

Where Azure refuses the area, write no comment. Go back to [Read the addresses](SKILL.md#read-the-addresses).

## `triage` step 8: label, and comment the reason

**Write back every tag the work item has, with `kac:triaged` and the route on the end, joined by `; `.** Then the
write keeps `kac:finding` whether Azure merges the tags you send or replaces them. A finding without `kac:finding`
leaves every query that would find it again.

Read the tags through the REST API immediately before you write, so the set you write back is the current one.

**Check the work item the update returns.** `fields."System.Tags"` includes `kac:finding`, `kac:triaged` and the
route. Where it does not, report the field, what it holds, and the work item's link, and write nothing more to it.

Then post the comment, as [Write a comment](#write-a-comment) does. Go back to
[step 8](SKILL.md#8-label-and-comment-the-reason).

## `draft` step 1: check the issue

**Read the work item through the REST API with its relations**, as [step 1](#triage-step-1-read-the-queue) does.

* **A relation whose `attributes.name` is `Pull Request` is a pull request that already answers it.**
* **`fields."System.State"` of `Closed`, `Done` or `Removed` is a closed finding.**
* **The tags are the labels.** Read `kac:triaged` and the route from `fields."System.Tags"`.

Go back to [step 1](SKILL.md#1-check-the-issue).

## `draft` step 3: the address in `sources`

```yaml
sources:
  - { resource: https://dev.azure.com/<org>/<project>/_workitems/edit/<id> }
```

Copy `<project>` as `base` gives it. Go back to [step 3](SKILL.md#3-write-the-record-or-the-edit).

## `draft` step 4: open the pull request

`publishing.base` is the repository, as `https://dev.azure.com/<org>/<project>/_git/<repo>`. Decode `<repo>` as you
decode the project.

**Open one pull request on that repository from your branch**, with the record as its title in one line and the body
from its file. Link the finding as a work item, set to transition when the pull request completes. Keep the body under
4,000 characters.

**The target is the repository's default branch.** Name another only where the corpus merges elsewhere.

**Check the pull request the call returns.** Its `description` is the body you wrote, and not `@` and a path. Read the
work item again with its relations, and a relation whose `attributes.name` is `Pull Request` names it. Where either
check fails, report the field and the pull request's URL, and open no second pull request.

The response has `pullRequestId`. The pull request's URL is
`https://dev.azure.com/<org>/<project>/_git/<repo>/pullrequest/<pullRequestId>`. Go back to
[step 4](SKILL.md#4-open-the-pull-request).
