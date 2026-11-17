---
trigger: Use before a change to a service's speed or uptime, or asked its target.
---

# Reading an NFR

An NFR (non-functional requirement) is a target the estate commits to: a number for one quality of service, such as
latency, availability or recovery, with the instrument that measures it. An agreed NFR binds the services and offerings
in `applies-to`. Read one before a change that could make a service slower, less available or harder to recover,
because the estate has already said how much of each it accepts.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/nfrs/<record>.json             # one NFR this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/nfrs/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each NFR is its own file, and there is no flat file of parts. Where `nfrs` is not in `types` in `manifest.json`, this
corpus exports none, so say that and stop.

## Find the record

Search `${CLAUDE_PLUGIN_ROOT}/corpus/nfrs/` and read every file that hits.

* **Search the service id in `applies-to`**, as `svc-payment-api`. Then search the id of every offering that service
  implements, because an NFR on an offering binds each service behind it. An offering's `implemented-by` lists those
  services, where `offerings` travelled.
* **Search an NFR id**, as `nfr-authorisation-latency`, where a service's or an offering's `nfrs` field names one.
* **Search the quality**, as `latency` or `recovery`. `characteristic` states it as one word.

Where no NFR matches, the estate has set no target for this. Say so, and set none yourself.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                                  | Type                     | What it holds                                                         |
|--------------------------------------|--------------------------|-----------------------------------------------------------------------|
| `fields.id`                          | string                   | the address to cite the NFR by, as `nfr-authorisation-latency`        |
| `fields.title`                       | string                   | the commitment, in one line                                           |
| `fields.status`                      | string                   | `draft`, `aspirational`, `agreed` or `retired`                        |
| `fields.characteristic`              | string                   | the quality it commits to, as `latency` or `availability`             |
| `fields.applies-to`                  | list of strings          | the ids of the services and offerings it binds                        |
| `fields.target`                      | string                   | the number, as `p95 under 800ms` or `99.5%`                           |
| `fields.window`                      | string, or null          | the period the number is read over, as `rolling 1 hour`               |
| `fields.alert-after`                 | string, or null          | how long a breach lasts before an alert or a ticket, as `15 minutes`  |
| `fields.measured-by`                 | string                   | the instrument that reports the number, and where to read it          |
| `fields.constrained-by`              | list of strings, or null | the ids of the integrations whose own SLA caps the target             |
| `fields.agreed-on`                   | string, or null          | the day the owner accepted the commitment. Set where it is `agreed`   |
| `fields.review-by`                   | string                   | the day someone confirms it is still true, as `2027-08-28`            |
| `fields.tags`                        | list of strings, or null | the word a reader arrives with                                        |
| `sections.Target`                    | string of markdown       | the number, the window, and exactly what is measured                  |
| `sections.Why this number`           | string of markdown       | what the number was derived from                                      |
| `sections.How it is measured`        | string of markdown       | how the instrument arrives at the number                              |
| `sections.What a breach costs`       | string of markdown       | what a miss costs a customer and the estate                           |
| `sections.What we do about a breach` | string of markdown       | the alert, who it pages, and what they do                             |
| `sections.Constraints`               | string of markdown       | what caps the number from outside. Absent where the record wrote none |
| `trust`                              | null                     | always null for this type, which records no verification              |
| `path`                               | string                   | where the NFR sits in its own repository                              |
| `links`                              | object, or null          | the built link to that NFR, under `human`                             |

The five sections before `Constraints` are always present, because the type requires them.

**Give `target`, `window` and `measured-by` together.** A target without its window cannot be checked, and the
instrument says which number counts.

**Say that a change expected to miss an agreed target needs the owner's agreement.** The owner accepted the number on
`agreed-on`, and the export does not name them, so link `links.human`. A change that moves the number is a change to
the record. Send it to `raise-finding` once the change exists.

## Say whether it is current

Read the `status` that puts an NFR in force from `inForce`, a string, `null`, or absent, on the `nfrs` entry of `types`
in `manifest.json`. The schema can change it, so take it from the export every time.

* **The `inForce` value**: agreed, and it binds. Give `agreed-on`.
* **`status: aspirational`**: a target set and not yet binding. Say so, and say a change may miss it without breaking
  a commitment.
* **`status: draft`**: nobody has agreed it. Say that it binds nothing yet.
* **`status: retired`**: no longer a commitment. Say so, and search again for the NFR that replaced it.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written, and say it binds nothing.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, give `status` as written, and say you cannot tell from the export whether the target
binds. The other statuses still mean what the bullets above say.

**Say when `review-by` is before today.** Nobody has confirmed the number since that day, so it may no longer be the
one agreed. Name the date under the key `review-by`. Quote `generatedAt` from `manifest.json` beside it, say that the
record may have moved since, and link `links.human`.

## Fetch the source

`Current actual`, the number the service achieves today, stays in the record. So does `Related`.

Fetch the record where the question is how close the service runs to its target. Read
`${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the NFR's `path`. An NFR under a
shortcode folder is published by that corpus, so it reads that corpus's publishing block. Link `links.human` beside
what you quote.

**Where the record cannot be fetched**, link `links.human` and say that `Current actual` did not come with the plugin.
Give no figure for it, because a number you estimate reads as one somebody measured. Name `measured-by` as where the
reader finds the real one.
