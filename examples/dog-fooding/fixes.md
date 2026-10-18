# Fixes

Problems we have hit before, with the resolution that worked.

**[→ Index](fixes/_index.md)**

## What is a fix?

One document per problem: the symptom as you would encounter it, what you were running, what causes it, how to resolve
it, and why it happens. *"pip cannot find the version the lint job pins."* *"An `.editorconfig` change does nothing in
the IDE."* Somebody has verified it, so a fix carries authority.

Add one when an investigation cost real time. You will hit the same problem again, and so will the next session.

## Why we use them

The same problems come back, and the next person to hit one pays the cost again. A fix turns two hours of debugging
into a thirty-second search, provided the words that person searches for are in `symptom-keywords`.

An agent session does most of the searching here, and it arrives with the symptom and nothing else. Every session that
finds its answer here is one that does not spend an hour reaching the answer the last one already had.

An observation nobody has verified belongs in the tracker. Write a fix from it once somebody has checked that the
problem and the resolution are both real.

## Scope

A fix is **verified**. Somebody has checked that the problem is real, that the resolution works, and that both are
still current. A `draft` is the exception, and says nobody has checked it yet. An observation with no resolution
stays in the tracker.

**Who did the checking is recorded, and a reader weighs it.** An agent that reproduced the symptom and ran the
resolution has done real work, and `verified` names it with its version the way a tool names itself. Read the list to
see how far the fix has been taken: agents alone leave it machine-confirmed, and one `human:` line makes it
human-reviewed. An export carries that reading as `trust`, derived from the list so that one place names who checked.

**A session never verifies its own fix.** An agent cannot check its own observation. Write the record as a `draft`,
or file the observation in the tracker, and let somebody else check it. This corpus withholds a draft from its
export until the status moves.

Other boundaries:

* **[Runbook](runbooks.md).** If it needs a diagnosis tree and an escalation path, it is a runbook. A fix has one known
  resolution.
* **[Standard](standards.md).** If the real answer is "people should stop doing the thing that causes this", the
  resolution is a rule, and it belongs in a standard.
* **One problem per document.** Someone arriving with a symptom matches the first one on the page and never reads the
  second.

## Level of detail and cadence

One fix matches one symptom to one verified resolution, usually one or two steps a reader can run. A problem that needs
a diagnosis tree and an escalation path is a runbook.

A fix changes when somebody verifies it again, or when the resolution stops working. New fixes arrive with each problem
somebody solves.

## Metadata

<!-- BEGIN GENERATED: schema-fixes -->

| Field                | Value                                          | Notes                                                                                         |
|----------------------|------------------------------------------------|-----------------------------------------------------------------------------------------------|
| `tier` *†            | `normative`                                    | The record's trust level, fixed for the type and checked against the folder.                  |
| `status` *†          | `active` `draft` `superseded` `fixed-upstream` | Whether the fix is current, unverified, replaced, or no longer needed. In force at `active`.  |
| `symptom-keywords` * | list                                           | Over-fill it: error text, service names, and what someone types before they know the cause.   |
| `applies-to`         | list                                           | Service ids this fix concerns.                                                                |
| `verified`           | list                                           | Who has checked this fix, one entry per actor, oldest first. Required when `status != draft`. |
| `review-by` *        | date                                           | Quoted. The date by which someone verifies this is still true.                                |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-fixes -->

## Adding a fix

1. Copy [`_template.md`](fixes/_template.md) to `<slug>.md`, named for the symptom rather than the cause. That is what
   people search for.
2. Make the H1 the symptom as encountered, in the words the error message or the user would use.
3. Over-fill `symptom-keywords` with the search terms that failed you the day you hit the problem.
4. Fill `Environment` with the tool, the operating system and the versions you were running.
5. Add a `verified` line naming who checked the resolution and the moment they did it. Leave the key out on a
   `draft`.
6. Set `review-by`. A resolution goes stale when the thing it repairs is rewritten.

**Conventions**

* **Symptom, environment, cause, resolution, in that order.** The reader arrives with a symptom and nothing else, and
  checks the environment is theirs before they act.
* **`applies-to` names service ids, and `Environment` is prose.** A reader follows an id to the service. They read the
  section to tell whether their machine, their tooling and their versions match.
* **Record how you found it**, not just what it was. The diagnostic route is often more reusable than the resolution.
* **If the root cause is still open**, say so, and raise it somewhere it can be tracked. A fix is not a place to park
  unowned work.

## What CI checks

<!-- BEGIN GENERATED: checks-fixes -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                        | Level   | What it verifies                                                                                      |
|------------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `timestamp-format`           | error   | Timestamp fields name a moment the calendar has, in UTC: `YYYY-MM-DDThh:mm:ssZ`.                      |
| `min-items`                  | error   | A list field carries at least as many entries as its schema asks for.                                 |
| `entry-shape / entry-key`    | error   | An object field, and each entry of an object list, carries the keys the field declares and no others. |
| `ref-resolves`               | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `verified-by-a-known-actor`  | error   | A verification names an actor the Open Knowledge Format defines, and never a post.                    |
| `one-verification-per-actor` | error   | An actor is written once in `verified`, and a second check moves the `at` on the entry they have.     |
| `one-problem-per-document`   | warning | A fix has one Symptom section, and a record with two is two fixes.                                    |
| `fix-in-date`                | warning | A fix names a review date that has not passed.                                                        |

<!-- END GENERATED: checks-fixes -->
