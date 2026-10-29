# Triaging findings on Azure DevOps

Read this where the tracker has a `target` of `azure-devops`. Each section serves one step of `SKILL.md`, and ends by
sending you back to it.

## The organisation, the project and the area path

`tracker.base` is the project, as `https://dev.azure.com/<org>/<project>`. The commands want its parts apart.

| Part              | What it is                                    | Where a command takes it                  |
|-------------------|-----------------------------------------------|-------------------------------------------|
| the organisation  | `base` up to and including `<org>`            | `--org`                                   |
| the project       | the segment after `<org>`                     | `--project`, an area path, a REST URL     |
| the area path     | the project and `area`, joined by a backslash | a WIQL query, `System.AreaPath`           |

**Decode the project name everywhere but a URL.** `base` arrives percent-encoded, and a base ending
`Engineering%20Standards` names the project `Engineering Standards`. Pass the decoded name to `--project` and into an
area path. Copy it into a REST URL as `base` gives it.

**Write an area path with backslashes.** `area` separates its segments with `/`, and Azure Boards separates an area
path with `\`. An `area` of `kac-it-swdev/subteam` is `<project>\kac-it-swdev\subteam`. A tracker with no `area` has
the project alone as its area path.

**`--resource` in every `az rest` call is the application id of Azure DevOps.** It tells `az` which token to sign the
call with.

**Pass the query string in `--uri-parameters`, and end the URL at its path.** On Windows `az` runs as `az.cmd`, and
`cmd.exe` cuts a URL at its `&` even inside quotes. Single-quote `'$expand=relations'`, because a shell expands
`$expand` otherwise.

## Signing in

```bash
az account show
```

**`az rest` signs in through `az login`, and so does every other command here.** A personal access token set up for
`az devops login` does not reach `az rest`. Where this fails, say that `az login` is the sign-in this skill needs. Go
back to [Find the corpus first](SKILL.md#find-the-corpus-first).

## Making the labels

A tag is what this platform calls a label, and Azure creates a tag it does not already have. There is nothing to make,
so there is nothing to list or count. Go back to [Making the labels](SKILL.md#making-the-labels).

## `triage` step 1: read the queue

Azure creates any tag it is given, so a backlog here has no unmarked finding and the second read does not apply. The
first read returns ids. The third returns the titles, which is enough to spot a duplicate.

```bash
az boards query --org https://dev.azure.com/<org> --project "<project>" \
  --wiql "SELECT [System.Id] FROM WorkItems WHERE [System.AreaPath] UNDER '<area path>' AND [System.Tags] CONTAINS 'kac:finding' AND [System.Tags] NOT CONTAINS 'kac:triaged' AND [System.State] NOT IN ('Closed', 'Done', 'Removed')"
az boards query --org https://dev.azure.com/<org> --project "<project>" \
  --wiql "SELECT [System.Id], [System.Title], [System.Tags] FROM WorkItems WHERE [System.AreaPath] UNDER '<area path>' AND [System.Tags] CONTAINS 'kac:triaged'"
```

* **`UNDER` takes in the areas below it too.**
* **Drop the `[System.AreaPath]` clause and its `AND` where the tracker states no area.**
* **The three states close an item in every process Azure ships.** Agile closes an Issue as `Closed`, and Basic as
  `Done`.

Read each work item the first query returned, and the one issue you were given, through the REST API:

```bash
az rest --method get --url 'https://dev.azure.com/<org>/<project>/_apis/wit/workitems/<id>' --uri-parameters '$expand=relations' api-version=7.1 --resource 499b84ac-1321-427f-aa17-267ca6975798
```

The body is `fields."System.Description"`. `fields."System.Tags"` has the tags, as one string split by `; `.

**Read the format before you read the block.** `multilineFieldsFormat."System.Description"` says how the body is
stored.

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

Write a JSON file with your own file tool, as you wrote the comment:

```json
{ "text": "<the comment>" }
```

Write the comment as a JSON string. Put a backslash before each backslash and each double quote, and write each line
break as `\n` and each tab as `\t`.

```bash
az rest --method post --url 'https://dev.azure.com/<org>/<project>/_apis/wit/workItems/<id>/comments' --uri-parameters format=markdown api-version=7.1-preview.4 --resource 499b84ac-1321-427f-aa17-267ca6975798 --headers Content-Type=application/json --body @<path>
```

`format=markdown` stores the comment as Markdown, so a backtick, a quotation mark and a fence all survive. Go back to
the section that sent you here.

## Bounce a work item

Move it to the other address's area path. `System.AreaPath` is a plain field, so `az boards` sets it:

```bash
az boards work-item update --org https://dev.azure.com/<org> --id <id> \
  --fields "System.AreaPath=<the other address's area path>"
```

Then comment why it moved, and the corpus whose area it moved to, as [Write a comment](#write-a-comment) does. Where
Azure refuses the area, write no comment. Go back to [Read the addresses](SKILL.md#read-the-addresses).

## `triage` step 8: label, and comment the reason

**Azure replaces the whole tag field.** Read the tags the work item has, immediately before you write, and write them
back with the new ones on the end. Writing only the new ones drops `kac:finding`, and the issue leaves every query that
would have found it again.

```bash
az boards work-item show --org https://dev.azure.com/<org> --id <id> --query "fields.\"System.Tags\""
az boards work-item update --org https://dev.azure.com/<org> --id <id> \
  --fields "System.Tags=<the tags that command printed>; kac:triaged; kac:route-record"
```

Then post the comment, as [Write a comment](#write-a-comment) does. Go back to
[step 8](SKILL.md#8-label-and-comment-the-reason).

## `draft` step 1: check the issue

Read the work item with the `az rest` call in [step 1](#triage-step-1-read-the-queue). Its `$expand=relations` brings
the links.

* **A linked pull request is a relation whose `attributes.name` is `Pull Request`.** Its `rel` is `ArtifactLink`, and
  its `url` ends with the pull request's id, after the last `%2F`. Add
  `--query "relations[?attributes.name=='Pull Request'].url"` to list only those.
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

```bash
az repos pr create --org https://dev.azure.com/<org> --project "<project>" --repository "<repo>" \
  --source-branch <branch> --title "<the record, in one line>" --description @<path> \
  --work-items <id> --transition-work-items true
```

* **`--work-items <id>` links the finding**, and `--transition-work-items true` moves it on when somebody completes the
  pull request. That link settles the finding, so the body needs no `Closes` line.
* **`@<path>` reads the description from the file.** `az` loads any argument starting with `@` from that file. Where
  it cannot read the file it sends the text `@<path>` instead, so check the `description` in the response.
* **Keep the body under 4,000 characters.** Azure DevOps refuses a longer pull request description.
* **The target is the repository's default branch.** Add `--target-branch <branch>` only where the corpus merges
  elsewhere.

The response has `pullRequestId`. The pull request's URL is
`https://dev.azure.com/<org>/<project>/_git/<repo>/pullrequest/<pullRequestId>`. Go back to
[step 4](SKILL.md#4-open-the-pull-request).
