---
trigger: Use before changing a service, or asked what it does, calls or keeps.
---

# Reading a service

A service is one deployable part of the estate: what it does, which repositories hold it, what it calls, what data it
keeps, and how a failure reaches a person. Read one before you change a service, because the record says what depends
on it and how much a customer feels its loss. The code you are reading shows what the service calls. It does not show
what calls it.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/services/<record>.json             # one service this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/services/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each service is its own file, and there is no flat file of parts. Where `services` is not in `types` in
`manifest.json`, this corpus exports none, so say that and stop.

## Find the record

Search `${CLAUDE_PLUGIN_ROOT}/corpus/services/` and read every file that hits.

* **Search the name of the repository you are working in, in `repos`**, as `platform`. Several services can share a
  repository, so read `Where it lives` to tell which one owns the folder you are changing.
* **Search a service id**, as `svc-lending`. A hit in `depends-on` is a service that calls this one, so the same search
  finds everything a change could break.
* **Search the service's name or what it does**, as `thumbnail` or `notices`, where you know neither.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                        | Type                     | What it holds                                                                         |
|----------------------------|--------------------------|---------------------------------------------------------------------------------------|
| `fields.id`                | string                   | the address to cite the service by, as `svc-catalogue-api`                            |
| `fields.title`             | string                   | the service's name                                                                    |
| `fields.status`            | string                   | `live`, `building`, `deprecated` or `retired`                                         |
| `fields.component-type`    | string                   | the sort of component, from a list the corpus defines, as `api` or `worker`           |
| `fields.platform`          | string                   | what it is built on, from a list the corpus defines, as `dotnet`                      |
| `fields.criticality`       | string                   | `critical`, `important` or `supporting`: what a customer feels without it             |
| `fields.monitoring-output` | string, or null          | `alert`, `ticket`, `log` or `none`: how a live problem reaches a person               |
| `fields.repos`             | list of strings          | the repositories a change to the service is made in                                   |
| `fields.interfaces`        | list of objects, or null | each interface it offers: `type`, a string as `openapi`, and `contract`, a string URL |
| `fields.depends-on`        | list of strings, or null | the ids of the services it calls                                                      |
| `fields.data-stores`       | list of strings, or null | the ids of the data records it owns or reads                                          |
| `fields.nfrs`              | list of strings, or null | the ids of the NFRs it must meet                                                      |
| `fields.facets`            | list of strings, or null | slices of the catalogue: an exposure, as `internal`, then any traits                  |
| `fields.tags`              | list of strings, or null | the word a reader arrives with                                                        |
| `sections.What it does`    | string of markdown       | what the service is for                                                               |
| `sections.Where it lives`  | string of markdown       | the repository, the folder and the hosting                                            |
| `sections.Dependencies`    | string of markdown       | what it calls, and how. Absent where the record wrote none                            |
| `sections.Data`            | string of markdown       | the stores it owns, and how it connects to them. Absent where the record wrote none   |
| `trust`                    | null                     | always null for this type, which records no verification                              |
| `path`                     | string                   | where the service sits in its own repository                                          |
| `links`                    | object, or null          | the built link to that service, under `human`                                         |

`What it does` and `Where it lives` are always present, because the type requires them.

**Give `criticality` and `monitoring-output` before a change to the service.** A `critical` service with
`monitoring-output: none` fails without telling anybody, so say so.

**Follow `data-stores` and `nfrs` to the records they name.** A data record says how the service may handle the data,
and an NFR says what it must keep meeting. Read each through the reference file for its type. Where that type did not
travel, name the id and say that the record is not in this export.

## Say whether it is current

Read the `status` that puts a service in force from `inForce`, a string, `null`, or absent, on the `services` entry of
`types` in `manifest.json`. The schema can change it, so take it from the export every time.

* **The `inForce` value**: running and in use. Answer from it.
* **`status: building`**: not yet running. Say so before you give any of it.
* **`status: deprecated`**: still running, and on its way out. Say so, and say that new work should not depend on it.
* **`status: retired`**: no longer running. Say so, and say that a dependency on it is a dependency on nothing.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. The other statuses still mean what the bullets above
say.

**Quote `generatedAt` from `manifest.json` beside what you report.** A service added, moved or retired since that day
still reads here as it was. Say that the record may have moved since, and link `links.human`.

## Fetch the source

`Environments` and `Operational notes` stay in the record. They say where the service runs, and what its operators
know that its code does not.

Fetch the record where the question is how to reach an environment, or how the service behaves in operation. Read
`${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the service's `path`. A service
under a shortcode folder is published by that corpus, so it reads that corpus's publishing block. Link `links.human`
beside what you quote.

**Where the record cannot be fetched**, link `links.human` and say that `Environments` and `Operational notes` did not
come with the plugin. State neither from your own reasoning, because an address or a quirk you infer reads as one the
operators wrote down.
