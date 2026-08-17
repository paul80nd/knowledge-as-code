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
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json           # what this export is: corpus, versions, commit, date
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/terms.jsonl    # one line of JSON per term — grep this
${CLAUDE_PLUGIN_ROOT}/corpus/glossary/<record>.json  # one file per glossary, holding its Scope and what it narrows
```

Address every path through `${CLAUDE_PLUGIN_ROOT}`. An installed plugin is copied into a cache, so a relative path that
climbs out of the plugin root resolves nowhere.

## Find the term

Grep the flat file, case-insensitively, on the title:

```bash
grep -i '"title":"<term>"' "${CLAUDE_PLUGIN_ROOT}/corpus/glossary/terms.jsonl"
```

Nothing back? Try the singular, try the other spelling, then grep the whole line for the word — a term is often defined
under one name and mentioned in another entry's definition.

Each line carries the entry whole:

| Field                 | What it holds                                                             |
|-----------------------|---------------------------------------------------------------------------|
| `id`                  | `<glossary-id>.<term>` — the address to quote and to search on             |
| `title`, `definition` | the term and its meaning                                                   |
| `not`                 | what the term excludes, where the corpus drew that boundary                |
| `seeAlso`             | related terms as full ids, so you can grep straight to them                |
| `record`              | the glossary this entry belongs to                                         |
| `status`, `reviewBy`  | how far the entry has settled, and the date it was meant to be read again  |
| `links`               | `human` to read the record rendered, `raw` to fetch its source             |

## Read every hit, not the first

**Where two entries share a title, read both before you answer.** The file's order is stable and carries no ranking you
can use. One estate defines *record* as a thing on a shelf with a barcode; another defines it as a markdown file under
version control. An answer taken from the wrong one is fluent, confident and about the wrong subject.

Open the owning record for each hit — `${CLAUDE_PLUGIN_ROOT}/corpus/glossary/<record>.json` — and read two things from
it:

* **`fields.narrows`.** Where one glossary narrows the other, the two entries are the general meaning and a refinement
  of it. The narrower entry wins wherever its context applies.
* **`sections.Scope`.** Where neither narrows the other, they are separate words that share a spelling. The right entry
  is the one whose Scope admits the thing being asked about.

Where the question does not settle which context it sits in, give both meanings and say which glossary each came from.

## Answer

* **Give the definition, then the `not` line.** A reader who gets only the definition will go on to apply the term to
  things it excludes.
* **Name the glossary you took the answer from**, every time. A reader working in the other context needs to see the
  mismatch.
* **Link `links.human`** so the reader can go to the record.
* **Follow `seeAlso`** where the question needs a neighbouring term. The values are full ids: grep for one directly.

## Say when an entry is unsettled

Two fields on the line say how far the entry has settled. Use it either way, and tell the user what you saw:

* **`status: draft`** — the terms were still settling when the export was taken.
* **`reviewBy` earlier than today** — the entry passed the date it was meant to be read again.

An export is a copy taken on a day, and it reads the same however long ago that was. `generatedAt` and `commit` in
`manifest.json` say when it was taken, and are worth quoting alongside either warning above.

## Say when there is nothing

Where no entry matches, say the corpus does not define the term, and name the corpus from `manifest.json`. Then answer
from the code if you can, marked plainly as your reading of the code rather than as the estate's meaning. Offer the term
as one worth adding to the glossary, and leave that to whoever owns it.
