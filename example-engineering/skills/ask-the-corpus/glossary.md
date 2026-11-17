---
trigger: Use when asked what a term means, how two differ or which exist, or on a business noun the code cannot define.
---

# Looking a term up in the glossary

A glossary states what the estate means by its words. Use the entry as the meaning wherever it is relevant, because a
meaning inferred from usage is fluent and often belongs to some other estate. Quote and cite the entry, so the person
you work with can check you read it correctly.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                      # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/terms.jsonl               # one line of JSON per term. Search this
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/<record>.json             # one glossary this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/<shortcode>/<record>.json # one glossary a corpus this one consumes wrote
```

## Find the record

`terms.jsonl` contains this corpus's terms and those of every corpus it consumes, so one file reaches them all. A term
from a consumed corpus carries its prefix on `id`, `record` and every `seeAlso` value, as `eng:gls-estate.borrower`.

**A term answers to more than its heading.** `also` gives other names for the same thing, and `avoid` gives a name the
corpus has dropped. Search both before you say a word is undefined, because the word you were asked about often sits in
`also` on a line with another `title`.

**Read the `title` of every hit before you use it.** The field names in this file are ordinary English words: `title`,
`record`, `definition`, `status`, `type`. A search for one of those matches every line in the file.

**Where the corpus has not defined your word, say so in the spelling you were asked about.** Quote the nearest entry,
and say what it defines. Whether your word means what that entry means is a ruling for the glossary's owner. Where no
entry comes near, answer from the code if you can, marked as your reading of the code, and offer the term as one worth
adding to the glossary.

### List the glossaries that travelled

**Where someone asks which terms the estate defines, group `terms.jsonl` by `record`.** Each group is one glossary,
and each line in it is one term, under its `title`. A glossary whose record sits under `corpus/glossary/<shortcode>/`
came from a corpus this one consumes. Its terms are in the same file under its prefix. A line gives no glossary title,
so read `fields.title`, a string, from the glossary's own file.

Answer with each glossary's id and title, and each term's `id` and `title`, so a reader can open each one.

**Read the definition to test a meaning in hand.** Where the question is whether a meaning in hand is already
defined, read the `definition` and `not` of each entry on its subject, and compare the wording. A term with the right
title can still mean something else here.

**Where someone asks what nothing covers, read `reports.md`, and stop here.** Coverage is a question about the whole
corpus, and a report is what answers it.

## Read the record

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

Answer from the entry in this order:

* **Give the definition, then the `not` line**, because a reader who gets only the definition applies the term to
  things it excludes.
* **Give the `title` where you matched on `also` or `avoid`**, because it is the word the estate writes. Where you
  matched on `avoid`, say the corpus has dropped that name.
* **Quote the `id`, and name the glossary in words**, so a reader can check the entry and see a mismatched context
  without decoding an id.
* **Follow `seeAlso`** where the question needs a neighbouring term. The values are full ids.

### Read every entry that shares a title

The file's order carries no ranking, so read every hit before you answer. One estate defines *record* as a thing on a
shelf, and another as a markdown file under version control. Open the owning record for each hit, and read two things:

* **`fields.narrows`, a string or null.** It is the id of the more general glossary this one narrows. Where one
  glossary narrows the other, the narrower entry wins wherever its context applies. A consuming corpus narrowing a
  term it inherited is the ordinary case.
* **`sections.Scope`, a string of markdown.** Where neither narrows the other, the two are separate words with one
  spelling. The right entry is the one whose Scope admits the thing being asked about.

Where the question does not settle the context, give both meanings, and say which glossary and which corpus each came
from.

## Say whether it is current

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

The export reads the same however old it is. Quote `generatedAt` and `commit` from `manifest.json` beside any of these.

## Fetch the source

Every section of a glossary except its Scope stays in the record. `types` in `manifest.json` lists what travelled.
Read it before you say anything is missing, and send the reader to the published glossary for the rest.

Quote `links.human` to link the term. It lands on the term's own heading.

To read the source, read `${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the line's
`path` and `anchor`. A line with no `links` goes there too. Where the fetch fails, give the entry as it travelled, say
that the rest of the glossary did not come with the plugin, and send the reader to `links.human` where the line has
one.
