---
name: standards-lookup
description: Find the rules you have to build to, in the standards that travel with this plugin. Use when someone
  asks what the rule is for something here, or which standards there are. Use it as well, unprompted, before you
  write or change code touching secrets, APIs, tests, builds, deployment, logging, dependencies, or data a service
  keeps.
---

# Looking a rule up in the standards

A standard states how the estate builds. Each `MUST` is the baseline for the work in front of you. Each `SHOULD` is the
usual way, which the work may leave by saying why. Find every rule that reaches the work, then quote and cite each one,
so the person you work with can see where it is written. Whether the work conforms is a ruling for the estate, so say
which part of the question you leave to them.

The corpus travels with this plugin as data. There is nothing to install and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                       # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/standards/rules.jsonl               # one line of JSON per rule. Search this
${CLAUDE_PLUGIN_ROOT}/corpus/standards/<record>.json             # one standard this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/standards/<shortcode>/<record>.json # one standard a corpus this one consumes wrote
${CLAUDE_PLUGIN_ROOT}/views/build-index.md                       # every standard, and the title of each of its rules
```

Use those paths exactly as written. An installed plugin sits in a cache outside the repository you are working in.

## Collect every rule that reaches the work

**Standards compose by union.** Every rule that reaches the work binds it, so one hit is a partial answer. A general
rule and a stack-specific one often cover the same subject, and both bind. `rules.jsonl` contains this corpus's rules
and those of every corpus it consumes, so one file reaches them all.

**A rule is filed under its author's topic, which is often not your task's.** Search for what the code does and what it
touches, as well as for the words of the task. On the owning record, `category` is the folder the standard sits in,
where `common` reaches everything, and `applies-to` lists the service ids it binds, or `all`.

**Read the `obligations` of every hit before you use it.** The field names in this file are ordinary English words:
`title`, `obligations`, `record`, `status`, `type`. A search for one of those matches every line in the file.

Each line is one heading of a standard's Rules section, with everything written under it:

| Field                | Type                     | What it holds                                                                |
|----------------------|--------------------------|------------------------------------------------------------------------------|
| `id`                 | string                   | `<standard-id>.<rule-id>`, as `std-CI.PERMS`, the address to quote and cite  |
| `title`              | string                   | the heading without its id, saying what the obligations beneath it are about |
| `obligations`        | list of objects, or null | one entry per bullet under that heading, in the order the standard wrote it  |
| `covers`             | list of strings, or null | the policy clauses this rule discharges                                      |
| `seeAlso`            | list of strings, or null | the rules of other standards this one points at                              |
| `type`               | string                   | `standards`, on every line of this file                                      |
| `record`             | string                   | the standard the rule belongs to                                             |
| `part`               | string                   | the rule's id inside that standard, as `PERMS`                               |
| `shortcode`          | string, or absent        | the corpus that published the rule. A rule written here carries no such key  |
| `status`, `reviewBy` | string                   | how far the standard has settled, and the date it was meant to be read again |
| `links`              | object or null           | `human`, the link to the rule. `null` where nothing can address it           |
| `path`, `anchor`     | string                   | the record's file in its corpus, and the fragment the rule sits at           |

Each entry in `obligations` has two keys:

| Key     | Type   | What it holds                                                             |
|---------|--------|---------------------------------------------------------------------------|
| `level` | string | the bullet's keyword: `MUST`, `MUST NOT`, `SHOULD`, `SHOULD NOT` or `MAY` |
| `text`  | string | the bullet as markdown, with its keyword in bold and its lines joined     |

**A key with no value is `null`, and the key is still there.** Test the value rather than the key. Two keys can be
absent: `shortcode` on a rule written here, and `links` on a line exported before the key existed.

## Read the level of each obligation

One line can hold obligations at different levels. Compare each `level` whole, because `MUST NOT` opens with `MUST`.

* **`MUST` and `MUST NOT` bind.** Departing from one takes a deviation its owner accepts, and `request-deviation` asks.
* **`SHOULD` and `SHOULD NOT` state the usual way.** The work departs by giving its reason in the change. No deviation.
* **`MAY` permits.** It blocks nothing.

Quote the entry's `text`, and cite the line's `id`. A rule is addressed by the id its heading opens with, so an
obligation has no id of its own.

## Read the prefix on an id

**A prefix names the corpus that wrote the rule.** `eng:std-TEST.LEVEL` was published by the corpus with shortcode
`eng`, and a bare `std-TEST.LEVEL` was written here. The prefix is on `id`, `record` and every `seeAlso` value. An
inherited rule binds as fully as one written here. Look its `shortcode` up in `sources` in `manifest.json`, and name
that corpus in words.

**A record file sits under its producer's shortcode.** `eng:std-TEST` is
`${CLAUDE_PLUGIN_ROOT}/corpus/standards/eng/std-TEST.json`, and a bare `std-TEST` is
`${CLAUDE_PLUGIN_ROOT}/corpus/standards/std-TEST.json`.

## Read the standard beside the rule

A rule read alone is stricter than its standard. Open the owning record, and read two entries of `sections`, an object
keyed by heading:

* **`Summary`** says what the standard is for, so you can tell whether the rule is about your subject.
* **`Conformance checklist`** states the rules as a test, for whoever has to show the work conforms.

In `fields`, quote the rules of each standard `depends-on` lists. Where `superseded-by` is set, quote the replacement
and say the rule has moved. The lines of `rules.jsonl` are sorted by heading, so open the record where the author's
order matters.

## Link the rule, and read its source

Quote `links.human` to link the rule. It lands on the rule's own heading. To read the source, load `corpus-retrieval`
and bring it the line's `path` and `anchor`.

## Say what stayed behind

`types` in `manifest.json` lists what travelled. Read it before you say anything is missing, and send the reader to the
published standard for the rest.

* **Every standard's Examples and Rationale stay in the record.** A bullet's short reason, opening "because", travels.
* **The clause a rule puts into practice** is here only where `policies` travelled.
* **Whether anyone checks the rule** is here only where `controls` travelled.

`seeAlso` travels. Follow each value, prefix and all, because a rule commonly leans on another.

## Say when a standard is unsettled

Read the `status` that puts a standard in force from `inForce`, a string, `null`, or absent, on the `standards` entry
of `types` in `manifest.json`. The schema can change it, so take it from the export every time. Read `status` and
`reviewBy` on every line you quote, and tell the reader what you saw:

* **The `inForce` value**: in force. Answer from it.
* **`status: draft`**: the standard was a proposal when the export was taken.
* **`status: deprecated`**: the standard is on its way out, and new work is not built to it.
* **`status: superseded`**: another standard replaced it. Find that one, and quote it.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.
* **`reviewBy` earlier than today**: the standard passed the date it was meant to be read again.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. Call no standard in force on the strength of its
status. `draft`, `deprecated` and `superseded` still mean what the bullets above say.

The export reads the same however old it is. Quote `generatedAt` and `commit` from `manifest.json` beside any of these.

## Say when the nearest rule is about something else

Where the estate has not written your subject down, say so in the words it would use. Quote the nearest rule, and say
what it governs. Whether your work falls inside it is a ruling for its owner. A nearest rule passed off as the rule is
a requirement nobody set.

## List the standards that travelled

**Where someone asks which standards the estate has, read `${CLAUDE_PLUGIN_ROOT}/views/build-index.md`.** Each
heading gives one standard's id, title and status, and each line under it gives one rule's title. A standard whose
record sits under `corpus/standards/<shortcode>/` came from a corpus this one consumes. The index already lists it
under its prefix, and it counts as fully as one written here. Where the index is absent, group `rules.jsonl` by
`record`.

Answer with each standard's id and title, so a reader can open the record. To cite a rule, find its line in
`rules.jsonl` by its title, and give its `id` and `links.human`.

**Read the obligations to test a sentence in hand.** Where the question is whether a sentence in hand is already a
rule, read the `obligations` of each rule whose title fits, and compare the wording. A title on the same subject
often leaves that obligation unstated.

**Where someone asks what nothing covers, load `ask-the-corpus`, and stop here.** Coverage is a question about the
whole corpus, and a report is what answers it.

## Say when there is nothing

Where no rule matches, say that this export states no rule on the subject. Name the corpus and every entry in `sources`
from `manifest.json`, so the reader knows what was searched. An empty search states only that the estate has not
written this down: it permits nothing. Quote obligations only from a standard about your subject, and offer the subject
as one worth a standard.
