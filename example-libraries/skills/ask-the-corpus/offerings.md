---
trigger: Use when asked what a customer-facing feature does or what delivers it.
---

# Reading an offering

An offering is something the estate gives a customer, described across the services that deliver it: what it does,
who it is for, why it exists, and where they meet it. It is the place a question about a feature starts, because no
single service record says what the customer gets. Read one before a change to behaviour a customer sees, and follow
it to the services behind it.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/offerings/<record>.json             # one offering this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/offerings/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each offering is its own file, and there is no flat file of parts. Where `offerings` is not in `types` in
`manifest.json`, this corpus exports none, so say that and stop.

## Find the record

Search `${CLAUDE_PLUGIN_ROOT}/corpus/offerings/` and read every file that hits.

* **Search the feature in the customer's words**, as `renew` or `refund`. `title` and `What it does` use them.
* **Search the service id in `implemented-by`**, as `svc-lending`, where the question is which offerings a change to
  one service affects.
* **Search a feature file path in `feature-files`**, as `lending/features/renewal.feature`, where you are working on
  the test that describes it.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                               | Type                     | What it holds                                                              |
|-----------------------------------|--------------------------|----------------------------------------------------------------------------|
| `fields.id`                       | string                   | the address to cite the offering by, as `ofr-borrowing`                    |
| `fields.title`                    | string                   | what the customer gets, in one line                                        |
| `fields.status`                   | string                   | `planned`, `building`, `live` or `deprecated`                              |
| `fields.implemented-by`           | list of strings          | the ids of the services that deliver it                                    |
| `fields.feature-files`            | list of strings, or null | the feature files that test it, each path opening on its repository        |
| `fields.nfrs`                     | list of strings, or null | the ids of the NFRs it must meet                                           |
| `fields.tags`                     | list of strings, or null | the word a reader arrives with                                             |
| `sections.What it does`           | string of markdown       | what the customer can do                                                   |
| `sections.Who it is for`          | string of markdown       | who uses it                                                                |
| `sections.Why it exists`          | string of markdown       | the need it meets                                                          |
| `sections.Surfaces`               | string of markdown       | where the customer meets it. Absent where the record wrote none            |
| `sections.Where the detail lives` | string of markdown       | the services, the work items and the documents behind it                   |
| `sections.Known limitations`      | string of markdown       | what it does not do yet, or does badly. Absent where the record wrote none |
| `trust`                           | null                     | always null for this type, which records no verification                   |
| `path`                            | string                   | where the offering sits in its own repository                              |
| `links`                           | object, or null          | the built link to that offering, under `human`                             |

`What it does`, `Who it is for`, `Why it exists` and `Where the detail lives` are always present, because the type
requires them.

**Follow `implemented-by` and `nfrs` to the records they name.** Read each through the reference file for its type.
Where that type did not travel, name the id and say that the record is not in this export.

**Read `Known limitations` before you call a behaviour a bug.** A limitation written there is one the estate knows of
and has accepted for now.

`Where the detail lives` is the only place the work items are written. Each one travels as its label, so quote the
label and send the reader to `links.human` for the address.

## Say whether it is current

The status is the offering's own, and a service behind it can be at another. Read the `status` that puts an offering
in force from `inForce`, a string, `null`, or absent, on the `offerings` entry of `types` in `manifest.json`. The schema
can change it, so take it from the export every time.

* **The `inForce` value**: customers use it now. Answer from it.
* **`status: planned`** or **`status: building`**: no customer has it yet. Say so before you give any of it.
* **`status: deprecated`**: on its way out. Say so, and say that new work should not extend it.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. The other statuses still mean what the bullets above
say.

**Quote `generatedAt` from `manifest.json` beside the status.** An offering launched or withdrawn since that day still
reads here as it was. Say that the record may have moved since, and link `links.human`.

## Fetch the source

Every section and field the type declares travels whole. The offering's owner stays in the record. Fetch the record
only where the question is who owns the offering, or where an answer will be acted on and the export may be old.

Read `${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the offering's `path`. An
offering under a shortcode folder is published by that corpus, so it reads that corpus's publishing block. Link
`links.human` beside what you quote.

**Where the record cannot be fetched**, link `links.human` and say that the owner did not come with the plugin. Name
nobody from your own reasoning.
