# Filing a deviation request on Azure DevOps

Read this where the tracker of the corpus owning the clause has a `target` of `azure-devops`. You have the body in a
file, and a yes to it.

`base` is the project holding the backlog, as `https://dev.azure.com/<org>/<project>`. The organisation follows the
host, and the project follows the organisation.

## The patch file

**File it through the REST API with `az rest`, so the body stays Markdown.** Azure DevOps reads a description as HTML
unless the work item marks it Markdown, and HTML loses the fences around the `kac-deviation` block that the record is
written from.

Write a JSON patch file with your own file tool, as you wrote the body. It is an array, and each entry adds one value
to the new work item:

```json
[
  { "op": "add", "path": "/fields/System.Title", "value": "<the title>" },
  { "op": "add", "path": "/fields/System.Description", "value": "<the body>" },
  { "op": "add", "path": "/multilineFieldsFormat/System.Description", "value": "Markdown" },
  { "op": "add", "path": "/fields/System.Tags", "value": "kac:deviation" }
]
```

**Write the title and the body as JSON strings.** Put a backslash before each backslash and each double quote, and
write each line break as `\n` and each tab as `\t`. The body is the one you showed and had a yes to, character for
character.

**A tag is what this platform calls a label**, so `System.Tags` takes the mark. Azure creates a tag it does not
already have.

## The area path

**`area` says which area path inside the project the item belongs to**, because one project can give one backlog to
any number of corpora.

Where the tracker states one, add one more entry to the patch:

```json
  { "op": "add", "path": "/fields/System.AreaPath", "value": "<project>\\<area>" }
```

The value is the project and the area, joined by a backslash.

* **Decode the project name.** `base` arrives percent-encoded, and a base ending `Engineering%20Standards` names the
  project `Engineering Standards`.
* **Convert every forward slash in `area` to a backslash.** Azure Boards separates an area path with backslashes.
* **Write each backslash twice**, because the value is a JSON string. An `area` of `kac-it-swdev/subteam` goes in as
  `"<project>\\kac-it-swdev\\subteam"`.

**Leave the entry out where `area` is null.** The item then lands in the project's default area.

## The call

```bash
az rest --method post --url 'https://dev.azure.com/<org>/<project>/_apis/wit/workitems/$Issue?api-version=7.1' --resource 499b84ac-1321-427f-aa17-267ca6975798 --headers Content-Type=application/json-patch+json --body @<patch path>
```

* **Single-quote the URL.** `$Issue` is the work item type, and a shell expands it inside double quotes.
* **Copy `<project>` from `base` as it is.** It sits in a URL, so it stays percent-encoded.
* **`--resource` is the application id of Azure DevOps.** It tells `az` which token to sign the call with.

The response is the new work item. Its `id` and the link at `_links.html.href` are what you give in your reply.

**Send it again without the `System.AreaPath` entry where the area is refused**, because it does not exist or you
cannot file into it. Say in your reply that the item landed in the project's default area, and why.

**`az rest` signs in through `az login`.** A personal access token set up for `az devops` does not reach it. Where the
call fails on sign-in or on permission, print the body, and say that `az login` is the sign-in this route needs. Where
it fails for any other reason, print the body and say so.

Then say what you did, as `SKILL.md` describes.
