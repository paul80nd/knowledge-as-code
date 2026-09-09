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

Use those paths exactly as they appear above. They are already absolute. An installed plugin sits in a cache of its own
rather than in the repository you are working in. A path you build relative to the working directory resolves nowhere.

One file holds every term, whoever wrote it. A corpus that consumes another exports both, so `terms.jsonl` carries this
corpus's terms and the terms of every corpus above it, and one search reaches all of them.

## Find the term

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
| `path`, `anchor`     | string                   | the two values `corpus-retrieval` builds a link from                         |

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

## Link to the term, and read its source

**Load the `corpus-retrieval` skill.** It carries the whole of this: which publishing block addresses the corpus that
wrote the line, how to substitute a template without breaking it, how to fetch the file through the client that
authenticates to that platform, and what to say where nothing reaches it.

**Bring it two values.** A line holds `path` and `anchor`, and those are what a template takes.

**A term is written as a heading**, so it has an anchor of its own and the link lands on the entry rather than at
the top of the glossary.

## Read every hit, not the first

**Where two entries share a title, read both before you answer.** The file's order is stable and carries no ranking you
can use. One estate defines *record* as a thing on a shelf with a barcode. Another defines it as a markdown file under
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
* **Link the reader to the record**, with the `corpus-retrieval` skill.
* **Follow `seeAlso`** where the question needs a neighbouring term. The values are full ids: search for one directly.

## Say when an entry is unsettled

Two fields on the line say how far the entry has settled. Use it either way, and tell the user what you saw:

* **`status: draft`** — the terms were still settling when the export was taken.
* **`reviewBy` earlier than today** — the entry passed the date it was meant to be read again.

An export is a copy taken on a day, and it reads the same however long ago that was. `generatedAt` and `commit` in
`manifest.json` say when it was taken, and are worth quoting alongside either warning above.

## Say when the entry is about something else

A word the glossary has not defined often sits beside one it has. Answer that case in three steps:

1. **Say the corpus has not defined your word**, in the spelling you were asked about.
2. **Name the nearest entry, and say what it defines.** Quote the definition as the glossary wrote it.
3. **Leave the reading to whoever owns the glossary.** Whether your word means what that entry means is a ruling, and
   this skill does not make one.

Answering from the nearest entry without saying it is the nearest one hands the reader a definition nobody wrote.

## Say when there is nothing

Where no entry matches, say the term is defined nowhere in this export, and name the corpus and every entry in
`sources` from `manifest.json`, so a reader knows which vocabularies were searched. Then answer from the code if you
can, marked plainly as your reading of the code rather than as the estate's meaning. Offer the term as one worth adding
to the glossary, and leave that to whoever owns it.
