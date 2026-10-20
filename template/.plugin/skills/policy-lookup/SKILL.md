---
name: policy-lookup
description: Find what this estate is committed to, in the policy clauses that travel with this plugin. Use when
  someone asks whether something is allowed here. Use it as well, unprompted, before you propose a design or a change
  touching secrets, access, retention, backups, logging, accessibility, third-party components, or what reaches
  production.
---

# Looking a clause up in the policies

A policy states what the estate is committed to. Each `MUST` is a commitment the work is held to. Each `SHOULD` is the
usual way, which the work may leave by saying why. Find every clause that reaches the work, then quote and cite each
one, so the person you work with can see where it is written. Whether a design meets a clause is a ruling for the
estate, so say which part of the question you leave to them.

The corpus travels with this plugin as data. There is nothing to install and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                      # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/policies/clauses.jsonl             # one line of JSON per clause. Search this
${CLAUDE_PLUGIN_ROOT}/corpus/policies/<record>.json             # one policy this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/policies/<shortcode>/<record>.json # one policy a corpus this one consumes wrote
```

Use those paths exactly as written. An installed plugin sits in a cache outside the repository you are working in.

## Search the clauses

`clauses.jsonl` contains this corpus's clauses and those of every corpus it consumes, so one file reaches them all. An
estate is bound by what it inherited as fully as by what it wrote.

**Read the `clause` of every hit before you use it.** The field names in this file are ordinary English words:
`clause`, `level`, `record`, `status`, `type`. A search for one of those matches every line in the file.

Each line is one row of a policy's clause table:

| Field                | Type              | What it holds                                                                   |
|----------------------|-------------------|---------------------------------------------------------------------------------|
| `id`                 | string            | `<policy-id>.<clause-key>`, the address to quote and to cite                    |
| `clause`             | string            | the obligation, in the words the policy wrote                                   |
| `level`              | string or null    | `MUST`, `MUST NOT`, `SHOULD` or `COULD`. Read this before anything else         |
| `type`               | string            | `policies`, on every line of this file                                          |
| `record`             | string            | the policy the clause belongs to                                                |
| `part`               | string            | the clause's key inside that policy                                             |
| `shortcode`          | string, or absent | the corpus that published the clause. A clause written here carries no such key |
| `status`, `reviewBy` | string            | how far the policy has settled, and the date it was meant to be read again      |
| `links`              | object or null    | `human`, the link to the clause. `null` where nothing can address it            |
| `path`, `anchor`     | string            | the record's file in its corpus, and the fragment the clause sits at            |

**A key with no value is `null`, and the key is still there.** Test the value rather than the key. Two keys can be
absent: `shortcode` on a clause written here, and `links` on a line exported before the key existed.

## Read the level of each clause

The answer turns on `level`, because two clauses of one policy can open the same sentence and bind differently. Compare
it whole, because `MUST NOT` opens with `MUST`. The wording arrives with its markup stripped, so take the level from
`level` alone. A clause opening with no modal has a `level` of `null`.

* **`MUST` and `MUST NOT` bind.** Departing from one takes a deviation its owner accepts, and `request-deviation` asks.
* **`SHOULD` states the usual way.** The work departs by giving its reason in the change. No deviation.
* **`COULD` permits.** It blocks nothing.

Quote the line's `clause`, name its level, and cite its `id`.

## Read the prefix on an id

**A prefix names the corpus that committed to the clause.** `eng:pol-SCRT.LOGS` was published by the corpus with
shortcode `eng`, and a bare `pol-SCRT.LOGS` was written here. The prefix is on `id` and `record`. An inherited clause
binds as fully as one written here. Look its `shortcode` up in `sources` in `manifest.json`, and name that corpus in
words.

**A record file sits under its producer's shortcode.** `eng:pol-SCRT` is
`${CLAUDE_PLUGIN_ROOT}/corpus/policies/eng/pol-SCRT.json`, and a bare `pol-SCRT` is
`${CLAUDE_PLUGIN_ROOT}/corpus/policies/pol-SCRT.json`.

## Read the policy beside the clause

A clause read alone is stricter than its policy. Open the owning record, and read three entries of `sections`, an
object keyed by heading:

* **`Purpose`** travels as its opening paragraph, which states the policy's position. It tells you whether the clause
  is about your subject.
* **`Scope`** says what the clauses bind. A clause about every store binds every store the Scope admits.
* **`Exceptions`** says where a clause gives way, and what has to be recorded when it does. "We cannot do that here" is
  often a question about this section.

## Link the clause, and read its source

Quote `links.human` to link the clause. Every clause of one policy has the same anchor, because a table row has no
fragment of its own, so the link lands on the clause table and the reader finds the row by its id. To read the source,
load `corpus-retrieval` and bring it the line's `path` and `anchor`.

## Say what stayed behind

`types` in `manifest.json` lists what travelled. Read it before you say anything is missing, and send the reader to the
published policy for the rest.

* **How a commitment is met** is here only where `standards` travelled.
* **What an external framework obliges the estate to** stays in the corpus that wrote the policy. Its register of
  frameworks says whether the estate is obliged or only borrowing, so a mapping quoted without it claims a commitment
  nobody can check.
* **The day a policy came into force** stays in the record's `active-from`.

A clause cites another clause by writing its id in its own words. Search for that id to read it.

## Say when a policy is unsettled

Read `status` and `reviewBy` on every line you quote, and tell the reader what you saw:

* **`status: draft`**: the policy was a proposal when the export was taken.
* **`status: retired`**: the policy was withdrawn, and binds nothing.
* **`reviewBy` earlier than today**: the policy passed the date it was meant to be read again.

The export reads the same however old it is. Quote `generatedAt` and `commit` from `manifest.json` beside any of these.

## Say when the nearest clause is about something else

Where the estate has not written your subject down, say so in the words it would use. "Password rotation" meets a
policy about rotating secrets. Quote the nearest clause, and say what it governs. Whether your subject falls inside its
Scope is a ruling for its owner. A nearest clause passed off as the clause is a commitment nobody made.

## Say when there is nothing

Where no clause matches, say that this export commits to nothing on the subject. Name the corpus and every entry in
`sources` from `manifest.json`, so the reader knows what was searched. An empty search states only that the estate has
not written this down: it permits nothing. Quote clauses only from a policy about your subject, and offer the subject
as one worth a policy.
