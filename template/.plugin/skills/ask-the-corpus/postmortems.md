---
trigger: Use when asked whether something failed before, and what went wrong.
---

# Reading a postmortem

A postmortem is the estate's account of one incident: what happened, who it affected, why it happened, and what it
taught. Read one before a change to a service that has failed before, because the account says what made the failure
possible, and a change can make it possible again.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/postmortems/<record>.json             # one postmortem this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/postmortems/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each postmortem is its own file, and there is no flat file of parts. Where `postmortems` is not in `types` in
`manifest.json`, this corpus exports none, so say that and stop.

## Find the record

Search `${CLAUDE_PLUGIN_ROOT}/corpus/postmortems/` and read every file that hits.

* **Search the service or offering id in `affected`**, as `svc-payment-ledger`, where the question is about one part of
  the estate.
* **Search the mechanism**, as `failover` or `replica lag`. `Root cause` and `Contributing factors` state it in the
  author's own words.
* **Search a record id in `prompted`**, as `nfr-ledger-recovery-point`, to find the incident that caused a record.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                             | Type                     | What it holds                                                            |
|---------------------------------|--------------------------|--------------------------------------------------------------------------|
| `fields.id`                     | string                   | the address to cite the postmortem by, as `pmt-0001`                     |
| `fields.title`                  | string                   | what happened, in one line                                               |
| `fields.status`                 | string                   | `draft` or `published`                                                   |
| `fields.occurred-at`            | string                   | the moment the incident began, in UTC, as `2026-06-03T21:14:00Z`         |
| `fields.detected-at`            | string                   | the moment somebody noticed it, in UTC                                   |
| `fields.restored-at`            | string, or null          | the moment service was back for users, in UTC. Set where it is published |
| `fields.duration`               | string, or null          | how long it lasted, in ISO 8601, as `PT12M`. Set where it is published   |
| `fields.severity`               | string                   | the highest severity it reached, as `sev1`                               |
| `fields.affected`               | list of strings          | the ids of the services and offerings it affected                        |
| `fields.prompted`               | list of strings, or null | the ids of the ADRs, runbooks, NFRs, fixes and standards it caused       |
| `fields.tags`                   | list of strings, or null | the word a reader arrives with                                           |
| `sections.Summary`              | string of markdown       | the incident in a paragraph                                              |
| `sections.Impact`               | string of markdown       | who it affected and how badly: the section's opening paragraph only      |
| `sections.Resolution`           | string of markdown       | how service came back                                                    |
| `sections.Root cause`           | string of markdown       | what made the failure possible                                           |
| `sections.Contributing factors` | string of markdown       | what made it worse or slower to find                                     |
| `sections.What went well`       | string of markdown       | what helped                                                              |
| `sections.What went wrong`      | string of markdown       | what hindered                                                            |
| `sections.Where we got lucky`   | string of markdown       | what limited the damage by chance                                        |
| `trust`                         | null                     | always null for this type, which records no verification                 |
| `path`                          | string                   | where the postmortem sits in its own repository                          |
| `links`                         | object, or null          | the built link to that postmortem, under `human`                         |

Every section is always present, because the type requires them.

**Follow `prompted` to the records the incident caused.** A runbook, an NFR or a standard written because of it states
what the estate now does, so answer the "what do we do" part of a question from those records. Read each through the
reference file for its type. Where that type did not travel, name the id and say that the record is not in this export.

**Read `Where we got lucky` before you call a risk handled.** Chance limited the damage there, and nothing else did.

## Say whether it is current

A postmortem does not go out of date. Once it is published, it changes only to fix a typo or a link, so an old account
is as true as a new one.

Read the `status` that puts a postmortem in force from `inForce`, a string, `null`, or absent, on the `postmortems`
entry of `types` in `manifest.json`. The schema can change it, so take it from the export every time.

* **The `inForce` value**: the account the estate stands behind. Answer from it.
* **`status: draft`**: still being written, and its cause may change. Say so before you give any of it.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. `draft` still means what the bullet above says.

**Quote `generatedAt` from `manifest.json` beside a draft.** A draft published since that day still reads here as it
was. Say that the record may have moved since, and link `links.human`.

## Fetch the source

`Timeline` and `Actions` stay in the record, because both name the estate's own clocks, systems and work items. So do
`Related`, and every paragraph of `Impact` after its first.

Fetch the record where the question is what happened when, or what the estate committed to doing afterwards. Read
`${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the postmortem's `path`. A postmortem
under a shortcode folder is published by that corpus, so it reads that corpus's publishing block. Link `links.human`
beside what you quote.

**Where the record cannot be fetched**, link `links.human` and say that the timeline and the actions did not come with
the plugin. State neither from your own reasoning, because an action you infer reads as one somebody agreed to.
