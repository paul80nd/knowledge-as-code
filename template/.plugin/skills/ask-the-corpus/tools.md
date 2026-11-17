---
trigger: Use before adding a tool or package, or asked whether one is approved.
---

# Reading a tool

A tool record is one entry in the estate's register of what it builds on: a package, a CLI or a service, the version
approved, the licence that applies, and whether new work may use it. Read one before you add a dependency or a tool,
because the register says what the estate has already approved, refused or retired.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/tools/<record>.json             # one tool this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/tools/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each tool is its own file, and there is no flat file of parts. Where `tools` is not in `types` in `manifest.json`,
this corpus exports none, so say that and stop.

## Find the record

Search `${CLAUDE_PLUGIN_ROOT}/corpus/tools/` and read every file that hits.

* **Search the package's name**, as `Markdig` or `actionlint`. A `purl` in `packages` contains it, as
  `pkg:nuget/Markdig`, and `title` names the tool.
* **Search the kind of tool**, as `lint` or `test`, where the question is what the estate uses for a job. `category`
  states it.
* **Search a tool id in `replaces` and `successor`**, as `tol-gh`, to follow a tool to what replaced it.

Where no tool matches, the register does not list it. Say so, and say that an unlisted tool is approved by nothing.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                                | Type                     | What it holds                                                                         |
|------------------------------------|--------------------------|---------------------------------------------------------------------------------------|
| `fields.id`                        | string                   | the address to cite the tool by, as `tol-actionlint`                                  |
| `fields.title`                     | string                   | the tool's name                                                                       |
| `fields.status`                    | string                   | `approved`, `trial`, `deprecated` or `rejected`                                       |
| `fields.category`                  | string                   | the kind of tool, as `lint`                                                           |
| `fields.packages`                  | list of objects, or null | each package approved: `purl`, a string, and `versions`, a string range or null       |
| `fields.homepage`                  | string                   | the project's own page                                                                |
| `fields.licence`                   | string                   | the licence concluded to apply, as an SPDX expression, `NONE` or `NOASSERTION`        |
| `fields.licence-declared`          | string, or null          | the package's own statement of its licence, word for word                             |
| `fields.decided-on`                | string                   | the day the current status was decided, as `2026-09-02`                               |
| `fields.review-by`                 | string                   | the day somebody checks the entry is still right, as `2027-09-02`                     |
| `fields.decided-in`                | string, or null          | the id of the ADR that recorded the decision                                          |
| `fields.replaces`                  | list of strings, or null | the ids of the tools this one supersedes                                              |
| `fields.successor`                 | string, or null          | the id of the tool that replaces this one                                             |
| `fields.tags`                      | list of strings, or null | the word a reader arrives with                                                        |
| `sections.What we use it for`      | string of markdown       | the job the tool does here                                                            |
| `sections.Status`                  | string of markdown       | why it has the status it has. Absent where the record wrote none                      |
| `sections.Accessibility`           | string of markdown       | how usable it is without sight, a mouse or colour. Absent where the record wrote none |
| `sections.Licence and obligations` | string of markdown       | what the licence asks of the estate. Absent where the record wrote none               |
| `sections.Where it is used`        | string of markdown       | the repositories and pipelines that use it. Absent where the record wrote none        |
| `trust`                            | null                     | always null for this type, which records no verification                              |
| `path`                             | string                   | where the tool sits in its own repository                                             |
| `links`                            | object, or null          | the built link to that tool, under `human`                                            |

`What we use it for` is always present, because the type requires it.

**Compare the version you mean to use with `versions`.** A version outside the range is not the one approved. Where
`versions` is `null`, the entry states no range, so say that and give none of your own.

**Give `licence` with every approval.** Where it differs from `licence-declared`, the estate concluded a licence of its
own, and `Licence and obligations` says why.

## Say whether it is current

Read the `status` that puts a tool in force from `inForce`, a string, `null`, or absent, on the `tools` entry of
`types` in `manifest.json`. The schema can change it, so take it from the export every time.

* **The `inForce` value**: approved for new work. Give `decided-on`.
* **`status: trial`**: being evaluated where `Trial criteria` says. Say that new work outside the trial needs it
  settled first.
* **`status: deprecated`**: on its way out. Say so, and read the tool `successor` names. Existing use is drift.
* **`status: rejected`**: evaluated and refused. Say so, and give the reason from `Status` where it travelled.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written, and say it approves
  nothing.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, give `status` as written, and call no tool approved on the strength of its status.
The other statuses still mean what the bullets above say.

**Say when `review-by` is before today.** Nobody has checked the version, the licence or the status since that day.
Name the date under the key `review-by`. Quote `generatedAt` from `manifest.json` beside it, say that the record may
have moved since, and link `links.human`.

## Fetch the source

`Trial criteria`, `Alternatives considered` and `Related` stay in the record.

Fetch the record where the question is what would end a trial, or which other tools were weighed and why each lost.
Read `${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the tool's `path`. A tool under
a shortcode folder is published by that corpus, so it reads that corpus's publishing block. Link `links.human` beside
what you quote.

**Where the record cannot be fetched**, link `links.human` and say that the trial criteria and the alternatives did not
come with the plugin. State neither from your own reasoning, because an alternative you supply reads as one the estate
weighed.
