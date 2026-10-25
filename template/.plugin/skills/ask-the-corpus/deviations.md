# Reading a deviation

A deviation records a rule broken on purpose: what is done instead, the risk a named person accepted, and the day it
is looked at again. Read one before you call a departure a violation, and before you rely on one, because an accepted
departure changes what is right for the thing it covers.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/deviations/<record>.json             # one deviation this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/deviations/<shortcode>/<record>.json # one a corpus this one consumes wrote
```

Each deviation is its own file, because a deviation is read whole. There is no flat file of parts. Where `deviations`
is not in `types` in `manifest.json`, this corpus exports none, so say that and stop.

## Find the deviations that bear on the question

Search `${CLAUDE_PLUGIN_ROOT}/corpus/deviations/` and read every file that hits.

* **Search the policy or standard id in `departs-from`**, as `std-CONFIG`. An entry names one clause inside it, as
  `std-CONFIG.PIN`, so the record's id alone finds every clause of it.
* **Search the service id in `applies-to`**, as `svc-kac`, where the question is about one service.
* **Search the subject** where you know neither, as `SDK` or `package`. The sections are in the author's own words.

## Read the record

Every key in `fields` is present, and is `null` where the record left it empty. Test the value rather than the key.

| Key                                   | Type                     | What it holds                                                |
|---------------------------------------|--------------------------|--------------------------------------------------------------|
| `fields.id`                           | string                   | the address to cite the deviation by                         |
| `fields.title`                        | string                   | the departure, in one line                                   |
| `fields.status`                       | string                   | `active`, `draft` or `closed`                                |
| `fields.risk`                         | string                   | `high`, `medium` or `low`, after what compensates            |
| `fields.departs-from`                 | list of strings          | each clause it departs from, or the literal `none`           |
| `fields.owner`                        | string                   | the person who accepted the risk, as `human:alex.doe`        |
| `fields.accepted-on`                  | string, or null          | the day they accepted it, as `2026-09-07`. Null on a draft   |
| `fields.review-by`                    | string                   | the day it is looked at again, as `2027-03-07`               |
| `fields.closed-on`                    | string, or null          | the day it ended. Null until it is `closed`                  |
| `fields.applies-to`                   | list of strings, or null | the service ids it covers, or the literal `all`              |
| `fields.tags`                         | list of strings, or null | the word a reader arrives with                               |
| `sections.What we are doing instead`  | string of markdown       | what happens in place of the rule                            |
| `sections.Why we need it`             | string of markdown       | why the departure was worth accepting                        |
| `sections.What the risk is`           | string of markdown       | what can go wrong while it stands                            |
| `sections.What compensates`           | string of markdown       | what limits the risk meanwhile                               |
| `sections.How it closes`              | string of markdown       | what has to be true for it to end                            |
| `sections.Scope`                      | string of markdown       | where it applies. Absent where the record wrote none         |
| `trust`                               | null                     | always null for this type, which records no verification     |
| `path`                                | string                   | where the record sits in its own repository                  |
| `links`                               | object, or null          | the built link to that record, under `human`                 |

The five sections before `Scope` are always present, because the type requires them. `owner` travels with this type
alone, so it is the one place an export says who accepted a risk.

**Search a bracketed id inside a section**, as `[ctl-0007]`. The export drops the link definitions, so the id alone is
the cross-reference.

## Say whether it is current

A deviation is current where its `status` is the `inForce` value and its `review-by` has not passed. `inForce` is a
string, `null`, or absent, on the `deviations` entry of `types` in `manifest.json`. The schema can change it, so take
it from the export every time. Read `inForce`, `status` and `review-by`, and say what you saw:

* **The `inForce` value**: accepted and in force. Give `owner`, `accepted-on` and `review-by`.
* **`draft`**: asked for and accepted by nobody. It permits nothing, so a departure resting on it is not yet allowed.
* **`closed`**: it ended on `closed-on`. The rule applies again, unless another deviation covers the same clause.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written, and say it permits
  nothing.
* **`review-by` before today**: the acceptance is due again and nobody has renewed it. The status stays as it was,
  because nothing moves it. Say the review date has passed, and name the date under the key `review-by`.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, give `status`, `owner`, `accepted-on` and `review-by` as written, and say you cannot
tell from the export whether the deviation permits the departure. `draft` and `closed` still mean what the bullets above
say.

**Quote `generatedAt` from `manifest.json` beside any of these.** The export reads the same however old it is, so a
deviation closed or renewed since that day still reads here as it was. Say that the record may have moved since, and
link `links.human`. Where `review-by` falls between `generatedAt` and today, say so as well, because that is the case
most likely to have moved.

## Read the prefix on an id

A deviation under a shortcode folder was written by the corpus with that `shortcode` in `sources` in `manifest.json`.
An entry in `departs-from` with a prefix, as `eng:pol-AUTV.REPRO`, names a clause that corpus wrote. Name the corpus in
words.

## Say what stayed behind

`assigned-to`, who does the work that closes the deviation, stays in the record. So does its `Related` section. Link
`links.human` for either.

Where no deviation matches, say so, name the corpus and every entry in `sources` you searched, and say that an empty
search states only that nobody has written a deviation down. A departure the work in front of you is about to make
from a `MUST` or `MUST NOT` needs one accepted. Where `request-deviation` shipped, send that departure to it.
