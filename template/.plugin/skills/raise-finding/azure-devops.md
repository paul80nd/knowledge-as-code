# Filing a finding on Azure DevOps

Read this where the tracker you picked has a `target` of `azure-devops`. You have the body in a file, and a yes to it.

This file states what filing achieves, the client known to reach it, and what Azure DevOps does that you could not work
out. It gives no command. Write each command yourself, for the shell and the client you have, because a command written
for another shell fails in yours.

## The client

**The `azure-devops` extension to the Azure CLI is the preferred client.** Its `az devops invoke` reaches the REST API
with the sign-in `az login` gave it, or with a personal access token set up through `az devops login`. It was tested,
signed in through `az login` alone, against an organisation with no Entra ID tenant.

**Any other client may file the item**, where every check under [Check the work item](#check-the-work-item) passes.

| Client                 | What is known                                                                                                          |
|------------------------|------------------------------------------------------------------------------------------------------------------------|
| `az devops invoke`     | reaches a route by its area and resource: `wit` and `workitems`, with `project` and `type` as route parameters         |
| `az devops invoke`     | supplies the `$` of `$Issue` itself, so the type goes in as `Issue`. `$Issue` builds `$$Issue` and is refused with 404 |
| `az rest`              | reaches an organisation backed by Entra ID only. Elsewhere it is sent a sign-in page, and exits 0                      |
| `az devops invoke`     | cannot read one work item by its id, because it picks the create route and asks for `type`. `workItemsBatch` reads it  |
| `az boards`            | `work-item show` reads one work item by its id                                                                         |
| Microsoft's MCP server | needs an organisation backed by Entra ID, except in its personal access token mode. It sets the Markdown format itself |

**Install nothing.** Where the extension is missing, say so, then file through another client you have or print the
body. Installing changes the reader's machine, and a managed machine may forbid it.

## What Azure DevOps does

| Fact                                                                                                                                                   | Why it matters                                                                                                     |
|--------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------|
| `base` is the project holding the backlog, as `https://dev.azure.com/<org>/<project>`                                                                  | the organisation follows the host, and the project follows the organisation                                        |
| Only the REST API sets `/multilineFieldsFormat/System.Description`                                                                                     | without it Azure stores the description as HTML, which loses the fences around the block triage copies             |
| A work item is created at `_apis/wit/workitems/$Issue` under the project                                                                               | `$Issue` is literal in a URL: a `$` and the work item type. A shell that expands it sends a request naming no type |
| Every call names an `api-version`. These calls were tested at `7.1`                                                                                    | a later version the server serves may stand in, where every check passes                                           |
| The request is a JSON patch: an array of `add` operations, sent as `application/json-patch+json`                                                       | the endpoint refuses any other shape                                                                               |
| Title and body go into the patch as JSON strings: a backslash before each backslash and each double quote, `\n` for each line break, `\t` for each tab | a raw quote or line break ends the string early, and the patch is refused or the body is cut                       |
| A tag is what Azure calls a label, and Azure creates a tag it does not already have                                                                    | the mark always lands, so a finding here is never filed unmarked                                                   |
| The area path field is the project and `area` joined by `\`, every `/` in `area` written as `\`, with the project's name as it reads                   | Azure Boards separates an area path with backslashes, and `Engineering%20Standards` reads `Engineering Standards`  |
| Inside a JSON string each `\` of the area path is written twice                                                                                        | an `area` of `kac-it-swdev/subteam` goes in as `"<project>\\kac-it-swdev\\subteam"`                                |
| In a URL the project stays percent-encoded, as `base` gives it                                                                                         | a decoded space breaks the URL                                                                                     |
| A credential the organisation refuses is answered with a sign-in page as HTML, status 203, and not with a 401                                          | a response that is not JSON is a refused sign-in, whatever the client's exit code says                             |
| On Windows `az` runs as `az.cmd`, so `cmd.exe` reads every argument again                                                                              | `cmd.exe` cuts an argument at `&`, even inside quotes, so pass a second query parameter apart from the URL         |
| Text inside angle brackets, such as `<org>`, is removed when the description is stored, inside a code span as well                                     | a placeholder is written in braces, as `{org}`, and the check on the body shows one that did not survive           |
| `#` and a number in the description links to that work item in the same organisation                                                                   | write the full URL of an issue on any other tracker                                                                |
| A Markdown description comes back with its HTML escaped: `>` as `&gt;`, so `provenance: >` reads `provenance: &gt;`                                    | compare the body after decoding `&lt;`, `&gt;`, `&quot;`, `&#39;`, `&nbsp;` and then `&amp;`                       |

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

**Create one work item of type `Issue` through the REST API**, from the patch file.

**Where Azure refuses the area, create it once more without the `System.AreaPath` entry.** The area may not exist, or
you may not be able to file into it. Nothing was created, so the second attempt is the only work item. Say in your
reply that it landed in the project's default area, and why.

## Check the work item

The response to the create is the new work item as Azure stored it, so it is the read-back. Check three things in it:

* **`multilineFieldsFormat."System.Description"` is `markdown`**, in upper or lower case.
* **`fields."System.Description"` is the body you sent**, once decoded, apart from trailing whitespace.
* **`fields."System.Tags"` includes `kac:finding`.**

**Where a check fails, report the field, what it holds, and the link at `_links.html.href`, and file nothing again.**
The work item exists, and a second copy is a duplicate somebody has to close. Whoever is with you can correct the one
that is there.

The work item's `id` and the link at `_links.html.href` are what you give in your reply.

## Where filing fails

**A refused sign-in or permission ends with the body printed**, and with the client and the sign-in you tried. Name
`az login` and `az devops login` as the two sign-ins the preferred client can use. A failure for any other reason ends
with the body printed and the reason given.

Then say what you did, as `SKILL.md` describes.
