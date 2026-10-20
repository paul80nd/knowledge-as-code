# Answering from a report

A report answers a question about the corpus as a whole, such as which clauses nothing covers, with the verdicts a
person added and the limits they wrote. It is the quick route to a whole-corpus answer. Answer from the report. Do not
rebuild its answer from the standards and the controls, because a rebuild has neither the verdicts nor the limits.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/reports/<record>.json             # one report this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/reports/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Only an index entry travels. The body stays in the corpus that published it, so a report file says which report
exists, how old it is and how far it was checked, and you fetch the body to answer from it. Where `reports` is not in
`types` in `manifest.json`, this corpus exports none, so say that and stop.

## Find the report that answers the question

Read every file under `${CLAUDE_PLUGIN_ROOT}/corpus/reports/`. There are few, and each is short.

* **Match the question to `title` and `tags`.** A title names the question, as `Clause coverage`.
* **Read `generated.report`** where it is there. It names the `kac report` the content started from, as `coverage` or
  `frameworks`.

Where none matches, say that no report here answers the question, and name the corpus and every entry in `sources` you
searched.

## Read the index entry

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                       | Type                     | What it holds                                                   |
|---------------------------|--------------------------|-----------------------------------------------------------------|
| `fields.id`               | string                   | the address to cite the report by                               |
| `fields.title`            | string                   | the question it answers                                         |
| `fields.status`           | string                   | `draft`, `active` or `stale`                                    |
| `fields.generated`        | object                   | who wrote the content as it stands, and when                    |
| `fields.generated.at`     | string                   | the moment it last changed, in UTC, as `2026-10-20T11:14:00Z`   |
| `fields.generated.by`     | string                   | the tool, agent or person who wrote it                          |
| `fields.generated.report` | string                   | the `kac report` it started from. Absent where none was run     |
| `fields.generated.tool`   | string                   | the tool version that ran it. Absent where none was run         |
| `fields.sources`          | list of objects          | each corpus it answers for, and the version it is true of       |
| `fields.verified`         | list of objects, or null | who checked it and when, each as `at` and `by`. Null on a draft |
| `fields.tags`             | list of strings, or null | the word a reader arrives with                                  |
| `sections`                | object                   | always empty, because the body never travels                    |
| `trust`                   | string                   | `unverified`, `machine-confirmed` or `human-reviewed`           |
| `path`                    | string                   | where the report sits in its own repository                     |
| `links`                   | object, or null          | the built link to that report, under `human`                    |

Each entry in `sources` has `resource`, a string naming the corpus, and `version`, a string giving its
`content-version`.

`trust` is worked out from `verified` at export. A check made before `generated.at` does not count, so a report edited
after its last check reads `unverified` even where `verified` lists a person.

## Say whether it is true of this corpus

Compare each entry in `sources` with `manifest.json`, which needs no network call:

* **This corpus** is the top-level `corpus`, at `contentVersion`.
* **A consumed corpus** is the entry in `sources` in `manifest.json` whose `corpus` equals `resource`, at its
  `contentVersion`.

Where every version matches, the report is true of the corpus installed here. Where one differs, the corpus has moved
past the report. Name each corpus, the version the report is true of and the version installed, and say the report may
miss what changed between them. A `status` of `stale` says the same, and `draft` says nobody has checked it.

## Fetch the body and answer from it

Load `corpus-retrieval` and bring it the report's `path`. A report under a shortcode folder is published by that
corpus, so it reads that corpus's publishing block.

Answer from the body. Quote its `Limits` section, and any section saying what it leaves open, because those say how far
the reader can rely on the answer. Give `generated.at`, `trust` and the version comparison beside it, and link
`links.human`.

**Where the body cannot be fetched**, give the report's `title`, the date in `generated.at` and `links.human`. Say that
you could not read its limits, and state none of them. Any answer you then build from the lookups is yours, without the
report's verdicts, so say so where you give one.
