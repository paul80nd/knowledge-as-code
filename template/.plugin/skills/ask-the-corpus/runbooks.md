# Answering from a runbook

A runbook is the response to a failure, written down before the failure happened. Somebody reads it under pressure, so
it opens on the symptoms and works through to a fix. Answer from the runbook whenever one matches. The organisation
already decided what to do first, so a first step you work out yourself can contradict it.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/runbooks/<record>.json             # one runbook this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/runbooks/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each runbook is its own file, and there is no flat file of parts. Of the sections, only `Symptoms` and `Impact` travel.
The steps, the `Afterwards` and `Related` sections and `rehearsal-method` stay in the corpus that published the
runbook, because a reader who has found the right runbook fetches it whole. Where `runbooks` is not in `types` in
`manifest.json`, this corpus exports none, so say that and stop.

## Find the runbook that matches the symptom

Search `${CLAUDE_PLUGIN_ROOT}/corpus/runbooks/` and read every file that hits.

* **Search the words of the failure**, as the error line, the command that failed, or where it fails, as `CI`.
  `Symptoms` lists what a reader sees, in the author's words.
* **Search the service id in `applies-to`**, as `svc-kac`, where the question is about one service.
* **Read `Impact`** to choose between two runbooks whose symptoms both match. It says who is affected.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                          | Type                     | What it holds                                                    |
|------------------------------|--------------------------|------------------------------------------------------------------|
| `fields.id`                  | string                   | the address to cite the runbook by                               |
| `fields.title`               | string                   | the failure it covers, in one line                               |
| `fields.status`              | string                   | `active`, `draft` or `retired`                                   |
| `fields.severity`            | string                   | the severity it is written for, as `sev3`                        |
| `fields.applies-to`          | list of strings, or null | the service ids it covers                                        |
| `fields.last-rehearsed`      | string                   | the day somebody last walked it, as `2026-10-18`, or `never`     |
| `fields.rehearsal-frequency` | string                   | how often it is walked, as `on-change` or `quarterly`            |
| `fields.requires-tools`      | list of strings, or null | what the reader installs before starting                         |
| `fields.requires-access`     | list of strings, or null | the permissions the reader needs before starting                 |
| `fields.tags`                | list of strings, or null | the word a reader arrives with                                   |
| `sections.Symptoms`          | string of markdown       | what a reader sees when this failure is happening                |
| `sections.Impact`            | string of markdown       | who is affected, and how badly                                   |
| `trust`                      | null                     | always null for this type, which records no verification         |
| `path`                       | string                   | where the runbook sits in its own repository                     |
| `links`                      | object, or null          | the built link to that runbook, under `human`                    |

`Symptoms` and `Impact` are always present, because the type requires them. A runbook under a shortcode folder was
written by the corpus with that `shortcode` in `sources` in `manifest.json`. Name that corpus in words.

## Say whether it is current

* **`active`**: in force. Answer from it.
* **`draft`**: nobody has settled it. Say so before you give any of it.
* **`retired`**: stood down. Say so, and search again for the runbook that replaced it.

**Say when `last-rehearsed` is `never`, or more than 92 days before today.** The steps may no longer work, because
nobody has walked them against the system in that time. Quote `generatedAt` from `manifest.json` beside any of these,
and say the record may have moved since.

## Fetch the steps and answer from them

Quote the runbook's `id`, `title`, the symptom that matched and its `Impact`. Then load `corpus-retrieval` and bring
it the runbook's `path`. A runbook under a shortcode folder is published by that corpus, so it reads that corpus's
publishing block.

Answer from the fetched record. `Immediate actions`, the section after `Impact`, is what to do first. Give the steps
in the order the runbook gives them, and link `links.human`.

**Where the record cannot be fetched**, give the runbook's `title` and `links.human`, and say that its steps did not
come with the plugin. State no steps of your own. A step you infer from `Symptoms` reads to the
person at the keyboard as the runbook's, and the runbook may say something else. Ask them to open the link, or to
paste the steps so you can work through them together.
