---
name: glossary-lookup
description: Look a term up in the knowledge corpus glossary that travels with this plugin. Use when someone asks what a
  word means here, or how two words differ. Use it as well, unprompted, when a noun in this repository names something
  in the business that you cannot define from the code around it.
---

# Looking a term up in the glossary

A glossary states what the estate means by its words. Use the entry as the meaning wherever it is relevant, because a
meaning inferred from usage is fluent and often belongs to some other estate. Quote and cite the entry, so the person
you work with can check you read it correctly.

The corpus travels with this plugin as data. There is nothing to install and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                      # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/terms.jsonl               # one line of JSON per term. Search this
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/<record>.json             # one glossary this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/<shortcode>/<record>.json # one glossary a corpus this one consumes wrote
```

Use those paths exactly as written. An installed plugin sits in a cache outside the repository you are working in.

## Find the term

`terms.jsonl` contains this corpus's terms and those of every corpus it consumes, so one file reaches them all.

**A term answers to more than its heading.** `also` gives other names for the same thing, and `avoid` gives a name the
corpus has dropped. Search both before you say a word is undefined, because the word you were asked about often sits in
`also` on a line with another `title`.

**Read the `title` of every hit before you use it.** The field names in this file are ordinary English words: `title`,
`record`, `definition`, `status`, `type`. A search for one of those matches every line in the file.

Each line is one entry of a glossary:

| Field                | Type                     | What it holds                                                                |
|----------------------|--------------------------|------------------------------------------------------------------------------|
| `id`                 | string                   | `<glossary-id>.<term>`, the address to quote and to search on                |
| `title`              | string                   | the term                                                                     |
| `definition`         | string or null           | what the term means                                                          |
| `also`               | string or null           | other names for the same thing: an acronym, an abbreviation, a second word   |
| `avoid`              | string or null           | a name for it the corpus has dropped, and what to write instead              |
| `not`                | string or null           | what the term excludes, where the corpus drew that boundary                  |
| `seeAlso`            | list of strings, or null | related terms as full ids, so you can search straight to them                |
| `type`               | string                   | `glossary`, on every line of this file                                       |
| `record`             | string                   | the glossary this entry belongs to                                           |
| `part`               | string                   | the term's key inside that glossary                                          |
| `shortcode`          | string, or absent        | the corpus that published the entry. A term written here carries no such key |
| `status`, `reviewBy` | string                   | how far the entry has settled, and the date it was meant to be read again    |
| `links`              | object or null           | `human`, the link to the term. `null` where nothing can address it           |
| `path`, `anchor`     | string                   | the record's file in its corpus, and the fragment the term sits at           |

**A key with no value is `null`, and the key is still there.** Test the value rather than the key. A line another
corpus exported before a key existed can lack `also`, `avoid` and `links`, and `shortcode` is absent on a term written
here. Read a missing `also` or `avoid` as a `null` one: the corpus said nothing there.

## Read the prefix on an id

**A prefix names the corpus that wrote the entry.** `eng:gls-estate.borrower` was published by the corpus with
shortcode `eng`, and a bare `gls-estate.borrower` was written here. The prefix is on `id`, `record` and every `seeAlso`
value. Look its `shortcode` up in `sources` in `manifest.json`, and name that corpus in words.

**A record file sits under its producer's shortcode.** `eng:gls-estate` is
`${CLAUDE_PLUGIN_ROOT}/corpus/glossary/eng/gls-estate.json`, and a bare `gls-estate` is
`${CLAUDE_PLUGIN_ROOT}/corpus/glossary/gls-estate.json`.

## Read every entry that shares a title

The file's order carries no ranking, so read every hit before you answer. One estate defines *record* as a thing on a
shelf, and another as a markdown file under version control. Open the owning record for each hit, and read two things:

* **`fields.narrows`.** Where one glossary narrows the other, the narrower entry wins wherever its context applies. A
  consuming corpus narrowing a term it inherited is the ordinary case.
* **`sections.Scope`.** Where neither narrows the other, the two are separate words with one spelling. The right entry
  is the one whose Scope admits the thing being asked about.

Where the question does not settle the context, give both meanings, and say which glossary and which corpus each came
from.

## Answer

* **Give the definition, then the `not` line**, because a reader who gets only the definition applies the term to
  things it excludes.
* **Give the `title` where you matched on `also` or `avoid`**, because it is the word the estate writes. Where you
  matched on `avoid`, say the corpus has dropped that name.
* **Quote the `id`, and name the glossary in words**, so a reader can check the entry and see a mismatched context
  without decoding an id.
* **Follow `seeAlso`** where the question needs a neighbouring term. The values are full ids.

Quote `links.human` to link the term. It lands on the term's own heading. To read the source, load `corpus-retrieval`
and bring it the line's `path` and `anchor`. A line with no `links` goes there too.

## Say what stayed behind

`types` in `manifest.json` lists what travelled. Read it before you say anything is missing, and send the reader to the
published glossary for the rest. Every section of a glossary except its Scope stays in the record.

## Say when an entry is unsettled

Read the `status` that puts an entry in force from `inForce`, a string, `null`, or absent, on the `glossary` entry of
`types` in `manifest.json`. The schema can change it, so take it from the export every time. Read `status` and
`reviewBy` on every line you quote, and tell the reader what you saw:

* **The `inForce` value**: in force. Answer from it.
* **`status: draft`**: the terms were still settling when the export was taken.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.
* **`reviewBy` earlier than today**: the entry passed the date it was meant to be read again.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. Call no entry in force on the strength of its status.
`draft` still means what the bullet above says.

The export reads the same however old it is. Quote `generatedAt` and `commit` from `manifest.json` beside either.

## Say when the nearest entry is about something else

Where the corpus has not defined your word, say so in the spelling you were asked about. Quote the nearest entry, and
say what it defines. Whether your word means what that entry means is a ruling for the glossary's owner. A nearest
entry passed off as the entry is a definition nobody wrote.

## Say when there is nothing

Where no entry matches, say the term is defined nowhere in this export. Name the corpus and every entry in `sources`
from `manifest.json`, so the reader knows what was searched. An empty search states only that the estate has not
written this down. Answer from the code if you can, marked as your reading of the code, and offer the term as one worth
adding to the glossary.
