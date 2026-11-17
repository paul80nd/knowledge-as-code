---
trigger: Use before code stores, logs or sends data, or asked how it is classified.
---

# Reading a data record

A data record describes one body of data the estate keeps: who owns it, how widely it may be shared, whether it
identifies a person, how long it is kept and where it goes. Read one before you write code that stores, logs, copies or
sends that data. A sensitivity you infer from a column name can be wrong, and the record states the one the estate
decided.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/data/<record>.json             # one data record this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/data/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each data record is its own file, and there is no flat file of parts. Where `data` is not in `types` in
`manifest.json`, this corpus exports none, so say that and stop.

## Find the record

Search `${CLAUDE_PLUGIN_ROOT}/corpus/data/` and read every file that hits.

* **Search the service id in `owned-by` and `flows-to`**, as `svc-lending`. The first finds the data a service owns,
  and the second the data sent to it.
* **Search a data id**, as `dat-borrowers`, where a service's `data-stores` lists one.
* **Search the thing the data describes**, as `loan` or `email address`. `Entities` lists each one in the author's own
  words.

Where no record matches, the estate has not classified this data. Say so, and classify nothing yourself.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                       | Type                     | What it holds                                                              |
|---------------------------|--------------------------|----------------------------------------------------------------------------|
| `fields.id`               | string                   | the address to cite the record by, as `dat-borrowers`                      |
| `fields.title`            | string                   | the data, in a few words                                                   |
| `fields.status`           | string                   | `active` or `deprecated`                                                   |
| `fields.owned-by`         | string                   | the id of the service that owns the data                                   |
| `fields.classification`   | string                   | `public`, `internal` or `confidential`, from published to need-to-know     |
| `fields.personal-data`    | string                   | `none`, `personal`, or `special-category` where GDPR Article 9 applies     |
| `fields.data-subjects`    | list of strings, or null | the people the data is about, as `borrowers`. Set where it is personal     |
| `fields.retention`        | string, or null          | how long the data is kept in practice. Set where it is personal            |
| `fields.region`           | string                   | where the data is stored and processed, as `UK South`                      |
| `fields.flows-to`         | list of strings, or null | the ids of the services and integrations the data is sent to               |
| `fields.review-by`        | string                   | the day someone confirms the classification and retention, as `2027-03-31` |
| `fields.tags`             | list of strings, or null | the word a reader arrives with                                             |
| `sections.Purpose`        | string of markdown       | why the estate keeps the data                                              |
| `sections.Entities`       | string of markdown       | what the data is made of, entity by entity                                 |
| `sections.Where it lives` | string of markdown       | the stores that keep it                                                    |
| `sections.Classification` | string of markdown       | why the classification is what it is                                       |
| `sections.Retention`      | string of markdown       | how long it is kept, and why                                               |
| `sections.Flows`          | string of markdown       | where it goes, why, and what is shared. Absent where the record wrote none |
| `trust`                   | null                     | always null for this type, which records no verification                   |
| `path`                    | string                   | where the record sits in its own repository                                |
| `links`                   | object, or null          | the built link to that record, under `human`                               |

The five sections before `Flows` are always present, because the type requires them.

**Give `classification`, `personal-data` and `retention` with every answer about handling the data.** A change that
logs it, copies it to a new store, or sends it somewhere `flows-to` does not list is a change to what the record states.
Say so, and send it to `raise-finding` once the change exists.

## Say whether it is current

Read the `status` that puts a data record in force from `inForce`, a string, `null`, or absent, on the `data` entry of
`types` in `manifest.json`. The schema can change it, so take it from the export every time.

* **The `inForce` value**: current. Answer from it.
* **`status: deprecated`**: the data is on its way out. Say so before you give any of it, and say that new work should
  not depend on it.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. `deprecated` still means what the bullet above says.

**Say when `review-by` is before today.** Nobody has confirmed the classification and the retention since that day, so
either may have moved. Name the date under the key `review-by`. Quote `generatedAt` from `manifest.json` beside it, say
that the record may have moved since, and link `links.human`.

## Fetch the source

Every section the type exports travels whole. Only `Related` stays in the record, with the record's owner.

Fetch the record where the question needs either. Read `${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and
follow it, bringing the record's `path`. A record under a shortcode folder is published by that corpus, so it reads
that corpus's publishing block. Link `links.human` beside what you quote.

**Where the record cannot be fetched**, link `links.human` and say that `Related` and the owner did not come with the
plugin. State neither from your own reasoning.
