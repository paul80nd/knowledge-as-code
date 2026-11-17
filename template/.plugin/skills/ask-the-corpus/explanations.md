---
trigger: Use when asked how parts of the estate fit together.
---

# Reading an explanation

An explanation is a map of part of the estate: how several services, offerings, standards and decisions fit together,
with a link to the record behind each one. It sets no rule and records no decision, so it binds nothing. Read one to
find which records answer the question, then answer from those records.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/explanations/<record>.json             # one explanation this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/explanations/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each explanation is its own file, and there is no flat file of parts. Where `explanations` is not in `types` in
`manifest.json`, this corpus exports none, so say that and stop.

## Find the record

Search `${CLAUDE_PLUGIN_ROOT}/corpus/explanations/` and read every file that hits.

* **Search a record id in `explains`**, as `svc-catalogue-api`, where the question is about one service, offering,
  standard, process or ADR.
* **Search the subject**, as `jacket image` or `request path`. `What this covers` states it in the author's own words.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                               | Type                     | What it holds                                                              |
|-----------------------------------|--------------------------|----------------------------------------------------------------------------|
| `fields.id`                       | string                   | the address to cite the explanation by, as `exp-covers-path`               |
| `fields.title`                    | string                   | what it explains, in one line                                              |
| `fields.status`                   | string                   | `draft`, `active` or `stale`                                               |
| `fields.explains`                 | list of strings          | the ids of the services, offerings, standards, processes and ADRs it spans |
| `fields.review-by`                | string                   | the day someone confirms it is still true, as `2027-03-31`                 |
| `fields.tags`                     | list of strings, or null | the word a reader arrives with                                             |
| `sections.What this covers`       | string of markdown       | what the explanation is about, and where it stops                          |
| `sections.Where the detail lives` | string of markdown       | a link to each record that owns a fact the explanation uses                |
| `trust`                           | null                     | always null for this type, which records no verification                   |
| `path`                            | string                   | where the explanation sits in its own repository                           |
| `links`                           | object, or null          | the built link to that explanation, under `human`                          |

Both sections are always present, because the type requires them.

**Follow the ids to the records that own the facts.** Each id in `explains` and each link in `Where the detail lives`
names a record. Read that record through the reference file for its type, and answer from it. Where its type did not
travel, name the id and say that the record is not in this export.

## Say whether it is current

Read the `status` that puts an explanation in force from `inForce`, a string, `null`, or absent, on the `explanations`
entry of `types` in `manifest.json`. The schema can change it, so take it from the export every time.

* **The `inForce` value**: current. Use it as the map.
* **`status: draft`**: still being written. Say so before you give any of it.
* **`status: stale`**: its author marked it behind what runs. Say so, and answer only from the records it links to.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. `draft` and `stale` still mean what the bullets above
say.

**Say when `review-by` is before today.** Nobody has checked the map against the estate since that day. Name the date
under the key `review-by`. Quote `generatedAt` from `manifest.json` beside it, say that the record may have moved since,
and link `links.human`.

## Fetch the source

The body between `What this covers` and `Where the detail lives` stays in the record. It has the author's own headings
and any diagram, and it is the explanation itself.

Fetch the record where the question is how the parts connect, and the linked records do not say. Read
`${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the explanation's `path`. An
explanation under a shortcode folder is published by that corpus, so it reads that corpus's publishing block. Link
`links.human` beside what you quote.

**Where the record cannot be fetched**, link `links.human` and say that the body of the explanation did not come with
the plugin. Answer from the linked records alone, and draw no connection between them that they do not state.
