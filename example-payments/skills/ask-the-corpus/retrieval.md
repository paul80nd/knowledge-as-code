# Reading a record's published source

An export is a copy taken on a day. A type carries as much of a record as a reader needs to judge it, and the rest
stays in the corpus that published it. Fetch the rest, so an answer can quote what the record says rather than what
the summary suggests. Every type reads the same way.

**The reference file that sent you here names the record.** Bring its `path`, and the line's `anchor` where it came
from a parts file.

Fetching runs a client for the platform that publishes the record. Each platform below names the client it prefers,
and any other client that returns the file may stand in. Where no client can run, ask whoever is with you for the
section you need.

## Take the publishing block of the corpus that wrote it

`manifest.json` has a `publishing` block per corpus. A record or a line with a `shortcode` is published by the entry in
`sources` with that shortcode, so read that entry's block. One with no `shortcode` is published by the top-level
`publishing` block. The wrong block addresses the right path in the wrong repository at the wrong commit, and both the
404 and somebody else's file that come back look plausible.

| Key             | Type           | What it holds                                                                               |
|-----------------|----------------|---------------------------------------------------------------------------------------------|
| `target`        | string         | `github`, `azure-devops`, `azure-devops-wiki` or `none`                                     |
| `base`          | string or null | the repository the corpus publishes from, or the wiki where `target` is `azure-devops-wiki` |
| `repository`    | string or null | the repository an `azure-devops-wiki` publishes from, and `null` on every other target      |
| `pathPrefix`    | string or null | the corpus's folder inside the repository it publishes from                                 |
| `ref`           | string or null | the commit the export was taken at                                                          |
| `humanTemplate` | string or null | the link pattern, with `{path}` and `{anchor}` to replace                                   |

A key with no value is `null`. An export written before a key existed leaves it out, so read a missing key as `null`.

## Fetch the file at its ref

Fetch the file, because a human URL returns the markdown inside somebody else's HTML, and the page furniture reads as
though it were the record. Join `pathPrefix` ahead of the record's `path`, and ask for it at `ref`.

Only GitHub serves a public repository's source without credentials. Everywhere else, the client that authenticates to
the platform fetches it. That client is already there: it is the one that opens an issue or a work item when something
needs writing back.

This file gives no command. Write the fetch yourself, for the shell and the client you have, because a command written
for another shell fails in yours. Each platform below states what you could not work out.

### GitHub

**`gh` is the preferred client.** It was tested against a public repository on github.com.

| Fact                                                                                                     | Why it matters                                                  |
|----------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------|
| `base` is the repository, as `https://github.com/<owner>/<repo>`                                         | the owner and the repository address the contents API           |
| The REST contents API serves a file at a ref, through `repos/<owner>/<repo>/contents/`                   | `gh` reaches it with the sign-in it already has                 |
| The media type `application/vnd.github.raw` makes the response the file                                  | without it the response is JSON describing the file             |
| The REST API answers for a public repository without credentials, and `gh` asks for a sign-in either way | a private repository answers only where you are signed in to it |

### Azure DevOps

**The `azure-devops` extension to the Azure CLI is the preferred client.** Its `az devops invoke` reaches the REST API
with the sign-in `az login` gave it, or with a personal access token set up through `az devops login`. It was tested,
signed in through `az login` alone, against an `azure-devops-wiki` corpus in an organisation with no Entra ID tenant.
Install nothing: where the extension is missing, say so, then use another client you have or ask for the section.

| Fact                                                                                                                            | Why it matters                                                                   |
|---------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------|
| `az repos` has no command for a file's content                                                                                  | the REST API is the only route                                                   |
| The [Git Items API] serves a file, and names the parameters that address a path at a commit                                     | it is the resource to fetch from                                                 |
| `az devops invoke` reaches it as the `git` area and the `items` resource, with `project` and `repositoryId` as route parameters | the route names the repository, and the path and the commit are query parameters |
| `az devops invoke` refuses a response that is not JSON unless it writes the response to a file                                  | ask for JSON with `includeContent=true`, and the file's text is in `content`     |
| The media type `text/plain` makes the response the file                                                                         | without it the response describes the file                                       |
| Every call names an `api-version`. `7.1` is the version the tracker skills were tested at                                       | a later version the server serves may stand in                                   |
| A credential the organisation refuses is answered with a sign-in page as HTML, status 203, and not with a 401                   | a response that is not JSON, or not the record, is a refused sign-in             |
| `az rest` reaches an organisation backed by Entra ID only. Elsewhere it is sent a sign-in page, and exits 0                     | it is a client for an organisation known to be backed by Entra ID                |
| An `azure-devops-wiki` record is read from `repository` through the Git Items API, at `ref`, under `pathPrefix`                 | `base` is the wiki, and the file sits in the repository behind it                |
| Where `repository` is `null`, the [Wikis API] lists the project's wikis and the repository each is published from               | the export was taken where the corpus had no `origin` it could read              |
| On Windows `az` runs as `az.cmd`, and `cmd.exe` cuts an argument at `&`, even inside quotes                                     | pass each query parameter as its own argument                                    |

### Where no client fetches it

This covers a client that is not installed or not signed in, one the repository refuses, and a session that cannot run
a command at all. Say that you found the record and cannot read it, name the platform, and quote the human link. Ask
whoever is with you to paste the section you need. Answer only from what the record says, because a reader cannot tell
a guess about its steps from the corpus.

## Check what you got

**Read the first line of the response before you use it.** A sign-in page, an HTML wrapper and a JSON error all
arrive with a success exit code from something. A record opens with its frontmatter's `---`, or with its own heading.

**Say which commit you read.** `generatedAt` and `commit` in `manifest.json` say when the export was taken, and the
file you fetched is at the `ref` that export named. Say it wherever a reader is about to act on a procedure or an
obligation.

## Link the reader to the record

**Quote `links.human` exactly as it stands.** A record's JSON and each line of a parts file have it, built with the
commit inside it. An inherited line keeps the link its producer built, so it opens the corpus that wrote the part.

**Build a link from `humanTemplate` for a line with no `links` key.** A producer that exported before lines had the
key sends a line without it. Replace `{path}` and `{anchor}` in the template of the block chosen above, and keep every
other character. Where that block's `target` is `azure-devops-wiki`, remove `.md` from `path` and write every `/` as
`%2F`. Every other target takes `path` whole.

**Quote the `path` where `links` or `humanTemplate` is `null`.** That corpus publishes nowhere the export could
address, so say so, and give the `path` as the record's place in its own repository.

[Git Items API]: https://learn.microsoft.com/en-us/rest/api/azure/devops/git/items
[Wikis API]: https://learn.microsoft.com/en-us/rest/api/azure/devops/wiki/wikis/list
