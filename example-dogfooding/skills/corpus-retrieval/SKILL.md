---
name: corpus-retrieval
description: Fetch the published source of a record this corpus export summarises. Load it from a lookup skill that
  has found what it was asked for and needs the words the export left behind.
---

# Reading a record's published source

An export is a copy taken on a day. A type carries as much of a record as a reader needs to judge it, and the rest
stays in the corpus that published it. This skill fetches the rest, so an answer can quote what the record says rather
than what the summary suggests. Every type reads the same way.

**The skill that sent you here names the record.** Bring its `path`, and the line's `anchor` where it came from a
parts file.

## Take the publishing block of the corpus that wrote it

`manifest.json` has a `publishing` block per corpus. A record or a line with a `shortcode` is published by the entry in
`sources` with that shortcode, so read that entry's block. One with no `shortcode` is published by the top-level
`publishing` block. The wrong block addresses the right path in the wrong repository at the wrong commit, and both the
404 and somebody else's file that come back look plausible.

| Key             | Type           | What it holds                                                         |
|-----------------|----------------|-----------------------------------------------------------------------|
| `target`        | string         | `github`, `azure-devops`, `azure-devops-wiki`, `mkdocs` or `none`     |
| `base`          | string or null | the repository the corpus publishes from                              |
| `pathPrefix`    | string or null | the corpus's folder inside that repository                            |
| `ref`           | string or null | the commit the export was taken at                                    |
| `humanTemplate` | string or null | the link pattern, with `{path}` and `{anchor}` to replace             |

A key with no value is `null`, and the key is still there.

## Fetch the file at its ref

Fetch the file, because a human URL returns the markdown inside somebody else's HTML, and the page furniture reads as
though it were the record. Join `pathPrefix` ahead of the record's `path`, and ask for it at `ref`.

Only GitHub serves a public repository's source without credentials. Everywhere else, the client that authenticates to
the platform fetches it. That client is already there: it is the one that opens an issue or a work item when something
needs writing back.

### GitHub

`gh` fetches a file at a ref. The accept header makes the response the file, and not JSON describing it:

```bash
gh api "repos/<owner>/<repo>/contents/<pathPrefix><path>?ref=<ref>" -H "Accept: application/vnd.github.raw"
```

`<owner>/<repo>` comes from the block's `base`. A public repository answers without credentials, and a private one
answers where `gh auth status` reports you signed in to that host.

### Azure DevOps

`az repos` has no command for a file's content. `az devops invoke` reaches the REST API, and the Git Items resource
serves a file:

```bash
az devops invoke --area git --resource items --accept-media-type text/plain --org <base> ...
```

The [Git Items API](https://learn.microsoft.com/en-us/rest/api/azure/devops/git/items) names the parameters that
address a path at a ref. `--accept-media-type text/plain` makes the response the file, and not a description of it.

### Where nothing here reaches it

Say that you found the record and cannot read it, name the platform, and quote the human link. Ask whoever is with you
to paste the section you need. Answer only from what the record says, because a reader cannot tell a guess about its
steps from the corpus.

## Check what you got

**Read the first line of the response before you use it.** A sign-in page, an HTML wrapper and a JSON error all
arrive with a success exit code from something. A record opens with its own heading.

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
