# Filing a finding on Azure DevOps

Read this where the tracker you picked has a `target` of `azure-devops`. You have the body in a file, and a yes to it.

This file states what each step achieves and what Azure DevOps does that you could not work out, and gives no command.
Write each command yourself, for the shell and the client you have, because a command written for another shell fails
in yours.

## What Azure DevOps does

| Fact                                                                                                                                                                              | Why it matters                                                                                                    |
|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------|
| `base` is the project holding the backlog, as `https://dev.azure.com/<org>/<project>`                                                                                             | the organisation follows the host, and the project follows the organisation                                       |
| Only the REST API sets `/multilineFieldsFormat/System.Description`                                                                                                                | without it Azure stores the description as HTML, which loses the fences around the block triage copies            |
| A work item is created at `_apis/wit/workitems/$Issue` under the project, with `api-version=7.1`                                                                                  | `$Issue` is literal: a `$` and the work item type. A shell that expands it sends a request naming no type         |
| The request is a JSON patch: an array of `add` operations, sent as `application/json-patch+json`                                                                                  | the endpoint refuses any other shape                                                                              |
| Title and body go into the patch as JSON strings: a backslash before each backslash and each double quote, `\n` for each line break, `\t` for each tab                            | a raw quote or line break ends the string early, and the patch is refused or the body is cut                      |
| A tag is what Azure calls a label, and Azure creates a tag it does not already have                                                                                               | the mark always lands, so a finding here is never filed unmarked                                                  |
| An area path is the project and `area` joined by `\`, every `/` in `area` written as `\`, and the project name decoded                                                            | Azure Boards separates an area path with backslashes, and `Engineering%20Standards` names `Engineering Standards` |
| Inside a JSON string each `\` of the area path is written twice                                                                                                                   | an `area` of `kac-it-swdev/subteam` goes in as `"<project>\\kac-it-swdev\\subteam"`                               |
| In a URL the project stays percent-encoded, as `base` gives it                                                                                                                    | a decoded space breaks the URL                                                                                    |
| `az rest` takes the application id of Azure DevOps, `499b84ac-1321-427f-aa17-267ca6975798`, as its `--resource`                                                                   | it tells `az` which token signs the call                                                                          |
| `az rest` signs in through `az login`, and a personal access token set up for `az devops login` does not reach it                                                                 | a sign-in failure is fixed by `az login`, and by nothing else                                                     |
| On Windows `az` runs as `az.cmd`, so `cmd.exe` reads every argument again                                                                                                         | `cmd.exe` cuts an argument at `&`, even inside quotes, so pass a second query parameter apart from the URL        |
| `az` reads an argument that opens with `@` from the file it names, and sends the text itself where it cannot read the file. PowerShell reads a bare `@` as its splatting operator | a body that never left its file shows only in the response                                                        |
| A Markdown description comes back with its HTML escaped: `>` as `&gt;`, so `provenance: >` reads `provenance: &gt;`                                                               | compare the body after decoding `&lt;`, `&gt;`, `&quot;`, `&#39;`, `&nbsp;` and then `&amp;`                      |

## Write the patch

**Write the patch to a file with your own file tool**, as you wrote the body. It adds these values to the new work
item, one `add` operation each:

| Path                                        | Value                                   |
|---------------------------------------------|-----------------------------------------|
| `/fields/System.Title`                      | the title                               |
| `/fields/System.Description`                | the body you showed and had a yes to    |
| `/multilineFieldsFormat/System.Description` | `Markdown`                              |
| `/fields/System.Tags`                       | `kac:finding`                           |
| `/fields/System.AreaPath`                   | the area path, only where `area` is set |

**`area` says which area path inside the project the item belongs to**, because one project can give one backlog to
any number of corpora. Leave the entry out where `area` is null, and the item lands in the project's default area.

## Create the work item

**Create one work item of type `Issue` through the REST API**, from the patch file. `az rest` is the client that reaches
it with the sign-in `az` already has.

**Where Azure refuses the area, create it once more without the `System.AreaPath` entry.** The area may not exist, or
you may not be able to file into it. Nothing was created, so the second attempt is the only work item. Say in your
reply that it landed in the project's default area, and why.

## Check the work item

The response is the new work item. Read it back, and check three things:

* **`multilineFieldsFormat."System.Description"` is `markdown`**, in upper or lower case.
* **`fields."System.Description"` is the body you sent**, once decoded. A description reading `@` and a path is a body
  that never left its file.
* **`fields."System.Tags"` includes `kac:finding`.**

**Where a check fails, report the field, what it holds, and the link at `_links.html.href`, and file nothing again.**
The work item exists, and a second copy is a duplicate somebody has to close. Whoever is with you can correct the one
that is there.

The work item's `id` and the link at `_links.html.href` are what you give in your reply.

## Where filing fails

**A failure on sign-in or on permission ends with the body printed**, and with `az login` named as the sign-in this
route needs. A failure for any other reason ends with the body printed and the reason given.

Then say what you did, as `SKILL.md` describes.
