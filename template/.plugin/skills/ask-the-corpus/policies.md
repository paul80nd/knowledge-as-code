---
trigger: Use before designing for secrets, access, data or production, or asked what is allowed or what policies exist.
---

# Answering from a policy

A policy states what the estate is committed to. Each `MUST` is a commitment the work is held to. Each `SHOULD` is the
usual way, which the work may leave by saying why. Answer from the clauses whenever one reaches the work, because the
estate already decided, and a design you reason out yourself can break a commitment nobody told you about. Find every
clause that reaches the work, then quote and cite each one, so the person you work with can see where it is written.
Whether a design meets a clause is a ruling for the estate, so say which part of the question you leave to them.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/policies/clauses.jsonl             # one line of JSON per clause. Search this
${CLAUDE_PLUGIN_ROOT}/corpus/policies/<record>.json             # one policy this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/policies/<shortcode>/<record>.json # one policy a corpus this one consumes wrote
```

## Find the record

Search `clauses.jsonl`. It contains this corpus's clauses and those of every corpus it consumes, so one file reaches
them all. On each line the prefix is on `id` and `record`, as `eng:pol-SCRT.LOGS` and `eng:pol-SCRT`.

**Read the `clause` of every hit before you use it.** The field names in this file are ordinary English words:
`clause`, `level`, `record`, `status`, `type`. A search for one of those matches every line in the file.

**Search in the words the estate would use.** "Password rotation" meets a policy about rotating secrets. Where you quote
the nearest clause, say what it governs, and leave to its owner whether your subject falls inside its `Scope`.

**Search for a cited id to read the clause it cites.** A clause cites another clause by writing that id in its own
words.

### List the policies that travelled

**Where someone asks which policies the estate has, group `clauses.jsonl` by `record`.** Each group is one policy,
and each line in it is one clause. A policy whose record sits under `corpus/policies/<shortcode>/` came from a corpus
this one consumes. Its clauses are in the same file under its prefix, and they count as fully as the ones written
here. A line gives no policy title, so read `fields.title`, a string, from the policy's own file.

Answer with each policy's id and title, and each clause's `id`, so a reader can open each one. Quote `links.human` for
any clause you name.

### Test a sentence in hand

Where the question is whether a sentence in hand is already a commitment, read the `clause` of each line on its
subject, and compare the wording. A policy on the same subject often leaves that commitment unstated.

### Ask what nothing covers

**Where someone asks what nothing covers, read `reports.md`, and stop here.** Coverage is a question about the whole
corpus, and a report is what answers it.

### Say when no clause matches

Quote clauses only from a policy about your subject. Where none matches, say that this export commits to nothing on
the subject, and offer the subject as one worth a policy.

## Read the record

Each line of `clauses.jsonl` is one row of a policy's clause table:

| Field                | Type              | What it holds                                                                   |
|----------------------|-------------------|---------------------------------------------------------------------------------|
| `id`                 | string            | `<policy-id>.<clause-key>`, the address to quote and to cite                    |
| `clause`             | string            | the obligation, in the words the policy wrote                                   |
| `level`              | string or null    | `MUST`, `MUST NOT`, `SHOULD` or `COULD`. Read this before anything else         |
| `type`               | string            | `policies`, on every line of this file                                          |
| `record`             | string            | the policy the clause belongs to                                                |
| `part`               | string            | the clause's key inside that policy                                             |
| `shortcode`          | string, or absent | the corpus that published the clause. A clause written here carries no such key |
| `status`, `reviewBy` | string            | how far the policy has settled, and the date it was meant to be read again      |
| `links`              | object or null    | `human`, the link to the clause. `null` where nothing can address it            |
| `path`, `anchor`     | string            | the record's file in its corpus, and the fragment the clause sits at            |

**A key with no value is `null`, and the key is still there.** Test the value rather than the key. Two keys can be
absent: `shortcode` on a clause written here, and `links` on a line exported before the key existed.

### Read the level of each clause

The answer turns on `level`, because two clauses of one policy can open the same sentence and bind differently. Compare
it whole, because `MUST NOT` opens with `MUST`. The wording arrives with its markup stripped, so take the level from
`level` alone. A clause opening with no modal has a `level` of `null`.

* **`MUST` and `MUST NOT` bind.** Departing from one takes a deviation its owner accepts, and `request-deviation` asks.
* **`SHOULD` states the usual way.** The work departs by giving its reason in the change. No deviation.
* **`COULD` permits.** It blocks nothing.

Quote the line's `clause`, name its level, and cite its `id`.

### Read the policy beside the clause

A clause read alone is stricter than its policy. Open the owning record, and read three entries of `sections`, an
object keyed by heading:

* **`Purpose`** travels as its opening paragraph, which states the policy's position. It tells you whether the clause
  is about your subject.
* **`Scope`** says what the clauses bind. A clause about every store binds every store the Scope admits.
* **`Exceptions`** says where a clause gives way, and what has to be recorded when it does. "We cannot do that here" is
  often a question about this section.

## Say whether it is current

Read the `status` that puts a policy in force from `inForce`, a string, `null`, or absent, on the `policies` entry of
`types` in `manifest.json`. The schema can change it, so take it from the export every time. Read `status` and
`reviewBy` on every line you quote, and tell the reader what you saw:

* **The `inForce` value**: in force. Answer from it.
* **`status: draft`**: the policy was a proposal when the export was taken.
* **`status: retired`**: the policy was withdrawn, and binds nothing.
* **Any other status, where `inForce` gives one**: not in force. Give the status as written before you give any of it.
* **`reviewBy` earlier than today**: the policy passed the date it was meant to be read again.

**Where `inForce` is `null` or absent, the export does not state which status is in force.** An export written before
the key existed has none. Say so, and give `status` as written. Call no policy in force on the strength of its status.
`draft` and `retired` still mean what the bullets above say.

The export reads the same however old it is. Quote `generatedAt` and `commit` from `manifest.json` beside any of these.

## Fetch the source

`types` in `manifest.json` lists what travelled. Read it before you say anything is missing, and send the reader to the
published policy for the rest.

* **How a commitment is met** is here only where `standards` travelled.
* **What an external framework obliges the estate to** stays in the corpus that wrote the policy. Its register of
  frameworks says whether the estate is obliged or only borrowing, so a mapping quoted without it claims a commitment
  nobody can check.
* **The day a policy came into force** stays in the record's `active-from`.

**Fetch the published policy when the answer needs one of these**, or wording the line and the record's `sections` do
not give. Read `${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md` and follow it, bringing the line's `path` and
`anchor`.

**Quote `links.human` to link the clause.** Every clause of one policy has the same anchor, because a table row has no
fragment of its own, so the link lands on the clause table and the reader finds the row by its id.

**Where the policy cannot be fetched**, answer from the line and the record, give `links.human`, and say which of the
above did not come with the plugin. State no framework mapping or `active-from` date of your own, because the reader
would take it as the policy's.
