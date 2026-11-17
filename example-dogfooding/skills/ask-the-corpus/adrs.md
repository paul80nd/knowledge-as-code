---
trigger: Use before a design choice, or when asked why something was decided.
---

# Reading an ADR

An ADR (architecture decision record) is a decision that affects more than one repository, with the reasoning behind
it. Read one before you propose a design, because a design that contradicts an accepted ADR reopens a settled decision.
Reopening one takes a new ADR that supersedes it, and the person asking needs to know that before the work starts.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/adrs/<record>.json             # one ADR this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/adrs/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each ADR is its own file, and there is no flat file of parts. Where `adrs` is not in `types` in `manifest.json`, this
corpus exports none, so say that and stop.

## Find the record

Search `${CLAUDE_PLUGIN_ROOT}/corpus/adrs/` and read every file that hits.

* **Search the subject of the design**, as `queue`, `authentication` or `monorepo`. `title` states the decision, and
  `Decision` states it in the author's own words.
* **Search an ADR id**, as `adr-0004`, where a record or a commit cites one. `supersedes`, `superseded-by` and `related`
  hold ADR ids, so the same search finds every ADR linked to it.
* **Read `Consequences`** to see whether a decision about something else still bears on your design. It says what the
  decision made easier and what it made harder.

Where no ADR matches, the estate has not recorded a decision on this. Say so. An ADR on a neighbouring subject decides
nothing about yours.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                     | Type                     | What it holds                                                           |
|-------------------------|--------------------------|-------------------------------------------------------------------------|
| `fields.id`             | string                   | the address to cite the ADR by, as `adr-0001`                           |
| `fields.title`          | string                   | the decision, in one line                                               |
| `fields.status`         | string                   | `proposed`, `rejected`, `accepted`, `deprecated` or `superseded`        |
| `fields.decided-on`     | string, or null          | the day it was settled, as `2026-08-05`. Null while it is `proposed`    |
| `fields.supersedes`     | string, or null          | the id of the ADR this one replaces                                     |
| `fields.superseded-by`  | string, or null          | the id of the ADR that replaces this one. Set where it is `superseded`  |
| `fields.related`        | list of strings, or null | the ids of other ADRs worth reading beside this one                     |
| `fields.tags`           | list of strings, or null | the word a reader arrives with                                          |
| `sections.Context`      | string of markdown       | the problem the decision answered: the section's opening paragraph only |
| `sections.Decision`     | string of markdown       | what was decided                                                        |
| `sections.Consequences` | string of markdown       | what the decision made easier and harder                                |
| `trust`                 | null                     | always null for this type, which records no verification                |
| `path`                  | string                   | where the ADR sits in its own repository                                |
| `links`                 | object, or null          | the built link to that ADR, under `human`                               |

All three sections are always present, because the type requires them. A link inside a section keeps the id as its
text, as `[adr-0002](...)`, so a search for an id finds the sections that cite it.

## Say whether it is current

An ADR does not go out of date. Once it is `accepted` or `rejected`, it changes only when a later ADR supersedes it. A
decision made long ago is in force for as long as its status says so.

Read the `status` that puts an ADR in force from `inForce`, a string, `null`, or absent, on the `adrs` entry of `types`
in `manifest.json`. The schema can change it, so take it from the export every time.

* **The `inForce` value**: in force. Give `decided-on`, and answer from `Decision`.
* **`status: proposed`**: nobody has decided it. Say that it binds nothing yet.
* **`status: rejected`**: considered and turned down. It says why the estate does not do this. Give `decided-on`.
* **`status: deprecated`**: no longer applies, and nothing replaced it. Say that the estate has no current decision on
  this.
* **`status: superseded`**: read the ADR `superseded-by` names, and answer from that one. Follow the chain until an ADR
  is not superseded, and name each one you passed.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written, and say it decides nothing.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, give `status` as written, and call no ADR in force on the strength of its status.
The other statuses still mean what the bullets above say.

**Quote `generatedAt` from `manifest.json` beside the status.** An ADR accepted since that day, or one that superseded
this one, is not in the export. Say that the record may have moved since, and link `links.human`.

## Fetch the source

`Alternatives Considered` stays in the record. So do `deciders`, `Related` and `References`, and every paragraph of
`Context` after its first.

Fetch the record where the question is why the estate chose this over something else, because only `Alternatives
Considered` says what was weighed and why each option lost. Fetch it as well where the question is who agreed it. Read
`${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the ADR's `path`. An ADR under a
shortcode folder is published by that corpus, so it reads that corpus's publishing block. Link `links.human` beside
what you quote.

**Where the record cannot be fetched**, link `links.human` and say that the alternatives and the people who decided did
not come with the plugin. State neither from your own reasoning, because an alternative you supply reads as one the
estate considered.
