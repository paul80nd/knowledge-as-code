# Reports

What the corpus says about itself, once somebody has read it.

**[→ Index](reports/_index.md)**

## What is a report?

A report answers a question no single record holds. Which policy clauses nothing implements. Which framework references
hang on one citation. The answer is a walk over every record at once, so nowhere else in the corpus can carry it.

`kac report` does the walking. It fills every cell the corpus states and leaves the judgement cells open, because it
cannot tell a gap worth closing from an obligation about something this organisation does not have. A report is that
output with those cells answered.

## Why we use them

Nothing tells you when an edge is lost. A clause loses its last covering standard, a framework reference loses its last
citing clause, and the corpus goes on validating. A report is where somebody looks.

Keeping it as a record rather than as a file generated on demand puts an owner on the answer and a date on it. The
argument written on top of the numbers is written once, and the next reader starts from it.

## Scope

A report is **not**:

* **An explanation.** If it would still be true with every record deleted, it is an explanation.
* **A one-off observation.** If nothing would reproduce it, it belongs in the tracker.
* **Raw output.** If nobody has answered the judgement cells, it is a command's output and belongs in a pipe.

**A report is stale the moment the corpus moves.** That is the price of keeping it, and the frontmatter is what keeps
the price visible: `sources` names the version of each corpus the report answers for, and `verified` names who last
checked that it still holds.

## Level of detail and cadence

One report answers one question over the whole corpus, and each row is one item with its verdict and a short note.
The evidence behind a verdict belongs in the record the row names.

A report changes whenever a record it reads changes, because `kac report` regenerates its rows. New reports arrive
rarely.

## Metadata

<!-- BEGIN GENERATED: schema-reports -->

| Field         | Value                    | Notes                                                                                            |
|---------------|--------------------------|--------------------------------------------------------------------------------------------------|
| `tier` *†     | `descriptive`            | The record's trust level, fixed for the type and checked against the folder.                     |
| `status` *†   | `draft` `active` `stale` | Whether the report is drafted, current, or behind the corpus. In force at `active`.              |
| `sources` *†  | list                     | Every corpus this report answers for, and the version of each it is true of.                     |
| `generated` * | object                   | Who wrote the content as it stands, and when.                                                    |
| `verified`    | list                     | Who has checked this report, one entry per actor, oldest first. Required when `status != draft`. |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-reports -->

## Adding a report

1. Run `kac report <name>` and read what it printed. The limits it states are part of the document.
2. Copy [`_template.md`](reports/_template.md) to a kebab-case filename naming the question, and paste the output under
   the frontmatter.
3. Answer every judgement cell. A cell you cannot answer is a question for whoever owns the area, not a blank.
4. Set `sources` to the `content-version` of each corpus the run read, and write yourself into `generated.by`. Leave
   `verified` empty and the status at `draft`: `no-self-verification` rejects a verification by whoever wrote the
   content.
5. Ask somebody else to read it. They add a `verified` entry naming themselves and set the status.

**Regenerating replaces the content.** Carry forward every verdict whose row is unchanged, and answer the rows that
moved. Where nothing in the corpus touched the report, raise the `sources` version by hand and leave `generated` alone.

**Which `sources` entry you raise decides what else you edit.** The corpus the report answers for comes first, then one
entry per corpus it imports. Raising the first entry edits frontmatter alone. Raising an imported entry also means
editing the `Imported:` bullet under `## Limits` to match, and adding a `verified` entry for the body you changed.

## What CI checks

<!-- BEGIN GENERATED: checks-reports -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                        | Level   | What it verifies                                                                                      |
|------------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `timestamp-format`           | error   | Timestamp fields name a moment the calendar has, in UTC: `YYYY-MM-DDThh:mm:ssZ`.                      |
| `min-items`                  | error   | A list field carries at least as many entries as its schema asks for.                                 |
| `entry-shape / entry-key`    | error   | An object field, and each entry of an object list, carries the keys the field declares and no others. |
| `report-stale`               | warning | Each corpus a report answers for is at the version the report names.                                  |
| `self-verification`          | error   | A report's `verified` list does not name the producer that `generated.by` names.                      |
| `verified-by-a-known-actor`  | error   | A verification states an actor the Open Knowledge Format defines, and never a `role:`.                |
| `one-verification-per-actor` | error   | An actor is written once in `verified`, and a second check moves the `at` on the entry they have.     |
| `generated-by-a-known-actor` | error   | `generated.by` states an actor the Open Knowledge Format defines.                                     |

<!-- END GENERATED: checks-reports -->
