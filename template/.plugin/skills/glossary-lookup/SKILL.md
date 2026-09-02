---
name: glossary-lookup
description: Look a term up in the knowledge corpus glossary that travels with this plugin. Use when someone asks what a
  word means here — "what do we mean by X", "is there a definition of X", "what counts as an X", "what is the difference
  between X and Y". Use it as well, unprompted, when you meet a term in this repository that names something in the
  business rather than something in the code — a noun in a class, table, field, endpoint, branch or comment that you
  cannot define from the code around it. Check the glossary before you infer a meaning from usage.
---

# Looking a term up in the glossary

The corpus travels with this plugin as data. You read it with the tools you already have. There is nothing to install
and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                      # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/terms.jsonl               # one line of JSON per term. Search this
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/<record>.json             # one glossary this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/<shortcode>/<record>.json # one glossary a corpus this one consumes wrote
```

Use those paths exactly as they appear above; they are already absolute. An installed plugin sits in a cache of its own
rather than in the repository you are working in. A path you build relative to the working directory resolves nowhere.

One file holds every term, whoever wrote it. A corpus that consumes another exports both, so `terms.jsonl` carries this
corpus's terms and the terms of every corpus above it, and one search reaches all of them.

## Find the term

**Use your Grep tool, not a shell command.** It runs on every platform and needs no shell, which is what makes the
promise above true for a reader on Windows. Point it at `${CLAUDE_PLUGIN_ROOT}/corpus/glossary/terms.jsonl`, ask for
matching content rather than a list of files, and search case-insensitively.

Two patterns, in this order:

1. **`"title":\s*"<term>"`** finds the lines that define the term. Write the `\s*`. Nothing promises the export puts no
   space after a colon, and a pattern assuming one returns nothing the day that changes.
2. **`<term>`** on its own finds every line mentioning it. This is how a term defined under one spelling turns up under
   another, in someone else's definition, `not` line or `seeAlso`.

Where the first pattern comes back empty, widen the second: try the singular, and try the other spelling.

**Read the `title` of every hit before you use it.** The field names in this file are ordinary English words: `title`,
`record`, `definition`, `status`, `type`. A search for one of those matches every line in the file. A line defines a
term when its `title` says so, never because it matched.

Each line carries the entry whole:

| Field                | Type                     | What it holds                                                                |
|----------------------|--------------------------|------------------------------------------------------------------------------|
| `id`                 | string                   | `<glossary-id>.<term>`, the address to quote and to search on                |
| `title`              | string                   | the term                                                                     |
| `definition`         | string or null           | what the term means                                                          |
| `not`                | string or null           | what the term excludes, where the corpus drew that boundary                  |
| `seeAlso`            | list of strings, or null | related terms as full ids, so you can search straight to them                |
| `type`               | string                   | `glossary`, on every line of this file                                       |
| `record`             | string                   | the glossary this entry belongs to                                           |
| `part`               | string                   | the term's key inside that glossary                                          |
| `shortcode`          | string, or absent        | the corpus that published the entry. A term written here carries no such key |
| `status`, `reviewBy` | string                   | how far the entry has settled, and the date it was meant to be read again    |
| `path`, `anchor`     | string                   | the two values a link template takes, and see below for which template       |

**A key with no value is `null`, and the key is still there.** Test the value rather than the key. `shortcode` is the
one exception, and the row above says so.

## Read the prefix on an id

**A prefix on an id names the corpus that wrote the entry.** `eng:gls-estate.borrower` is the term `borrower`, in the
glossary `gls-estate`, as the corpus whose shortcode is `eng` published it. The prefix sits on `id`, on `record` and on
every `seeAlso` value, so an id you take from one line and search for carries it.

**A bare id belongs to the corpus you installed.** `gls-estate.borrower` was written here.

**`shortcode` is the key into `sources`.** Each entry in `sources` in `manifest.json` holds one producing corpus: its
name, the version of it that travelled, and where it publishes. Look the shortcode up there before you say anything
about the entry's origin, and name the corpus in words. `eng` means nothing to a reader who has not read the manifest.

**A record file sits under its producer's shortcode**, because two corpora can name one glossary and a filename cannot
say whose it is. The shortcode moves out of the id and becomes the directory, so take it off the `record` value before
you build the filename. The owning record for `eng:gls-estate.borrower` is
`${CLAUDE_PLUGIN_ROOT}/corpus/glossary/eng/gls-estate.json`, and for a bare id it is
`${CLAUDE_PLUGIN_ROOT}/corpus/glossary/gls-estate.json`.

## Build a link from a template

**No line holds a URL.** `manifest.json` holds a publishing block per corpus, and each line holds the two values a
template takes: `path` and `anchor`.

**Take the block belonging to the corpus that wrote the line.** A line carrying `shortcode` is published by that entry
in `sources`, and its `publishing` block is the one to read. A line with no `shortcode` is published by the top-level
`publishing` block. Read the wrong one and you address the right path in the wrong repository at the wrong commit, which
fetches a 404 or somebody else's file, and both read as plausible.

**Copy a template exactly as it stands, replace `{path}` and `{anchor}` with the line's own values, and change nothing
else.** The commit is already inside the string. Do not retype it, shorten it, swap the host or judge whether it looks
right. A template with one character altered gives a 404 that reads as plausible, or a page from a version of the corpus
nobody asked about.

**One target spells `{path}` differently.** Where the block's `target` is `azure-devops-wiki`, the template addresses a
wiki page rather than a file, so substitute the line's `path` with `.md` removed and every `/` written as `%2F`. Every
other target takes the `path` whole. Two corpora can publish to two targets, so read `target` from the block you chose
above, every time.

**To send a reader to a record, use the block's `humanTemplate`.** Substitute `path` and `anchor`. That is the rendered
page, and the anchor lands the reader on the term rather than at the top of the glossary.

**To read a record's source yourself, fetch the file rather than the page.** The same block names the `target`, the
`base`, the `pathPrefix` and the `ref`. Join `pathPrefix` ahead of the line's `path` to reach the file inside the
repository, then ask the client that authenticates to that target for it at that `ref`. Fetching the human URL instead
hands you the markdown wrapped in someone else's HTML, and you will read the page furniture as though it were the
record.

**No unauthenticated host serves that source**, except GitHub's and only for a public repository. Where you have no
client for the target, say so and quote the human link, rather than assembling a URL that will return a sign-in page you
read as the record.

**Where the block's `humanTemplate` is `null`**, that corpus publishes nowhere the export could address. Say so, and
quote the `path` as the record's place in its own repository. Do not assemble a URL of your own.

## Read every hit, not the first

**Where two entries share a title, read both before you answer.** The file's order is stable and carries no ranking you
can use. One estate defines *record* as a thing on a shelf with a barcode; another defines it as a markdown file under
version control. An answer taken from the wrong one is fluent, confident and about the wrong subject.

Open the owning record for each hit, at the path *Read the prefix on an id* builds from `record` and `shortcode`, and
read two things from it:

* **`fields.narrows`.** Where one glossary narrows the other, the two entries are the general meaning and a refinement
  of it. The narrower entry wins wherever its context applies.
* **`sections.Scope`.** Where neither narrows the other, they are separate words that share a spelling. The right entry
  is the one whose Scope admits the thing being asked about.

Two entries may also come from two corpora, and `shortcode` says which. A consuming corpus narrowing a term it inherited
is the ordinary case, so read `fields.narrows` before you treat the pair as a clash.

Where the question does not settle which context it sits in, give both meanings and say which glossary each came from,
naming the corpus as well wherever the two glossaries were written by different ones.

## Answer

* **Give the definition, then the `not` line.** A reader who gets only the definition will go on to apply the term to
  things it excludes.
* **Quote the `id`.** `gls-search.title` is one string a reader can search the corpus for, and it settles in seconds
  whether you read the entry correctly.
* **Name the glossary in words as well**, every time, and name the corpus that published it wherever that is not the one
  installed. A reader working in the other context needs to see the mismatch without decoding an id to find it.
* **Link the reader to the record**, built from `humanTemplate` as above.
* **Follow `seeAlso`** where the question needs a neighbouring term. The values are full ids: search for one directly.

## Say when an entry is unsettled

Two fields on the line say how far the entry has settled. Use it either way, and tell the user what you saw:

* **`status: draft`** — the terms were still settling when the export was taken.
* **`reviewBy` earlier than today** — the entry passed the date it was meant to be read again.

An export is a copy taken on a day, and it reads the same however long ago that was. `generatedAt` and `commit` in
`manifest.json` say when it was taken, and are worth quoting alongside either warning above.

## Say when there is nothing

Where no entry matches, say the term is defined nowhere in this export, and name the corpus and every entry in
`sources` from `manifest.json`, so a reader knows which vocabularies were searched. Then answer from the code if you
can, marked plainly as your reading of the code rather than as the estate's meaning. Offer the term as one worth adding
to the glossary, and leave that to whoever owns it.
