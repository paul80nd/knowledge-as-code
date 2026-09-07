---
name: corpus-retrieval
description: Reach the published source of a record this corpus export summarises, and build a link somebody can
  follow. Load it from a lookup skill that has found what it was asked for and needs the words the export left behind,
  or a URL to quote. It says which publishing block addresses a record, how to fetch the file through the client that
  authenticates to that platform, and what to do where nothing here can reach it.
---

# Reading a record's published source

An export is a copy taken on a day. A type carries as much of a record as a reader needs to judge it, and the rest
stays in the corpus that published it. This is how you reach that.

**The skill that sent you here says what it needs.** It names the record, and it says whether the address wants an
anchor. Everything below is the same for every type.

## Take the publishing block belonging to the corpus that wrote it

`manifest.json` holds a publishing block per corpus. A record or a line carrying `shortcode` is published by that
entry in `sources`, and its `publishing` block is the one to read. One carrying no `shortcode` is published by the
top-level `publishing` block.

Read the wrong one and you address the right path in the wrong repository at the wrong commit. That fetches a 404 or
somebody else's file, and both read as plausible.

## Send a reader to the record

**A record file already holds the link.** `links.human` on the record's own JSON is a built URL with the commit inside
it. Quote it as it stands.

**A parts line holds no URL.** It carries `path` and `anchor`, which are the two values the block's `humanTemplate`
takes. Copy the template exactly as it stands, replace `{path}` and `{anchor}`, and change nothing else. Do not retype
the commit, shorten it, swap the host or judge whether it looks right. A template with one character altered gives a
404 that reads as plausible, or a page from a version of the corpus nobody asked about.

**One target spells `{path}` differently.** Where the block's `target` is `azure-devops-wiki`, the template addresses a
wiki page rather than a file, so substitute `path` with `.md` removed and every `/` written as `%2F`. Every other
target takes the `path` whole. Two corpora can publish to two targets, so read `target` from the block you chose above,
every time.

**Where `links.human` or `humanTemplate` is `null`**, that corpus publishes nowhere the export could address. Say so,
and quote the `path` as the record's place in its own repository. Do not assemble a URL of your own.

## Read the source yourself

**Fetch the file rather than the page.** The block names the `target`, the `base`, the `pathPrefix` and the `ref`. Join
`pathPrefix` ahead of the record's `path` to reach the file inside the repository, and ask for it at that `ref`.
Fetching the human URL instead hands you the markdown wrapped in someone else's HTML, and you will read the page
furniture as though it were the record.

**No unauthenticated host serves that source**, except GitHub's and only for a public repository. So the client that
authenticates to the platform is what fetches it, and that client is already there: it is the same one that opens an
issue or a work item when something needs writing back.

### GitHub

`gh` fetches a file at a ref, and the accept header is what makes the response the file rather than JSON describing it:

```bash
gh api "repos/<owner>/<repo>/contents/<pathPrefix><path>?ref=<ref>" -H "Accept: application/vnd.github.raw"
```

`<owner>/<repo>` comes from the block's `base`. A public repository answers this without credentials, and a private one
answers it where `gh auth status` reports you signed in to that host.

### Azure DevOps

`az repos` carries no command for a file's content. `az devops invoke` is the general route to the REST API, and the
Git Items resource is what serves a file:

```bash
az devops invoke --area git --resource items --accept-media-type text/plain --org <base> ...
```

The [Git Items API](https://learn.microsoft.com/en-us/rest/api/azure/devops/git/items) names the parameters that
address a path at a ref. Take `--accept-media-type text/plain`, or the response describes the file instead of being
it.

### Where nothing here reaches it

Say plainly that you found the record and cannot read it, name the platform and quote the human link. Then ask whoever
is with you to paste the section you need.

**Do not answer from the summary as though it were the record.** A process read from its trigger alone is a guess about
what the steps say, and a reader cannot tell your guess from the corpus.

## Check what you got

**Read the first line of the response before you use it.** A sign-in page, an HTML wrapper and a JSON error all arrive
with a success exit code from something. A record opens with its own heading.

**Say when the copy is old.** `generatedAt` and `commit` in `manifest.json` say when the export was taken, and the
source you just fetched is at the `ref` that export named. Where a reader is about to act on a procedure or an
obligation, say which commit you read it at.
