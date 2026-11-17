---
trigger: Use before relying on a vendor's system, or asked what fails with it.
---

# Reading an integration

An integration is a system a vendor runs and the estate calls: what it does, which services use it, how badly a
customer feels its loss, and what the estate does when it fails. Read one before a change that calls a vendor's system
or depends on it staying up. The estate has already decided the fallback, so a fallback you design yourself can
contradict it.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/integrations/<record>.json             # one integration this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/integrations/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each integration is its own file, and there is no flat file of parts. Where `integrations` is not in `types` in
`manifest.json`, this corpus exports none, so say that and stop.

## Find the record

Search `${CLAUDE_PLUGIN_ROOT}/corpus/integrations/` and read every file that hits.

* **Search the vendor or the product**, as `Pennine` or `SMTP`. `vendor` and `title` name them.
* **Search the service id in `used-by`**, as `svc-catalogue-api`, where the question is what one service depends on
  outside the estate.
* **Search the failure**, as `timeout` or `rate limit`, where something is going wrong. `Failure modes` lists how each
  failure presents.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                      | Type                     | What it holds                                                             |
|--------------------------|--------------------------|---------------------------------------------------------------------------|
| `fields.id`              | string                   | the address to cite the integration by, as `itg-bibliographic-data`       |
| `fields.title`           | string                   | the system, in a few words                                                |
| `fields.status`          | string                   | `active`, `trial` or `retired`                                            |
| `fields.vendor`          | string                   | who supplies the system                                                   |
| `fields.used-by`         | list of strings          | the ids of the services that call it, or called it before it retired      |
| `fields.criticality`     | string                   | `critical`, `important` or `supporting`: what a customer feels without it |
| `fields.their-sla`       | string, or null          | what the vendor's contract promises, in the contract's words              |
| `fields.replaces`        | string, or null          | the id of the integration this one took over from                         |
| `fields.successor`       | string, or null          | the id of the integration that took over from this one                    |
| `fields.tags`            | list of strings, or null | the word a reader arrives with                                            |
| `sections.What it does`  | string of markdown       | what the system does for the estate                                       |
| `sections.Failure modes` | string of markdown       | a table: each failure, how it presents, and the estate's fallback         |
| `sections.Exit`          | string of markdown       | how the estate would leave the vendor. Absent where the record wrote none |
| `trust`                  | null                     | always null for this type, which records no verification                  |
| `path`                   | string                   | where the integration sits in its own repository                          |
| `links`                  | object, or null          | the built link to that integration, under `human`                         |

`What it does` and `Failure modes` are always present, because the type requires them.

**Give the fallback from `Failure modes` for the failure that matched.** A fallback cell reading `None.` means the
estate has none, so say that the dependent service fails with the vendor.

**Read `their-sla` as the vendor's promise, not the estate's.** A target the estate commits to on top of it is an NFR.
Where `nfrs` travelled, search it for this integration's id.

## Say whether it is current

Read the `status` that puts an integration in force from `inForce`, a string, `null`, or absent, on the `integrations`
entry of `types` in `manifest.json`. The schema can change it, so take it from the export every time.

* **The `inForce` value**: in use. Answer from it.
* **`status: trial`**: being evaluated, and not yet settled. Say so before you give any of it, and say that new work
  depending on it rests on a trial.
* **`status: retired`**: no longer used. Say so, and read the integration `successor` names. Where `successor` is
  `null`, say that the record names nothing that took over.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. `trial` and `retired` still mean what the bullets above
say.

**Quote `generatedAt` from `manifest.json` beside the status.** A vendor changed or retired since that day still reads
here as it was. Say that the record may have moved since, and link `links.human`.

## Fetch the source

`Contract`, `Trial criteria`, `Their SLA`, `Commercials`, `Contacts` and `Related` stay in the record. They describe
owning the account, which is the estate's own business.

Fetch the record where the question needs one of them: how the estate calls the system, what would end a trial, or who
to contact at the vendor. Read `${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the
integration's `path`. An integration under a shortcode folder is published by that corpus, so it reads that corpus's
publishing block. Link `links.human` beside what you quote.

**Where the record cannot be fetched**, link `links.human` and name the sections that did not come with the plugin.
State none of them from your own reasoning, because a contact or a contract term you supply reads as the estate's.
