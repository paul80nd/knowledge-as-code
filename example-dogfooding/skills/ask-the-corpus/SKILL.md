---
name: ask-the-corpus
description: Send a question to the part of this corpus that answers it, when you do not know which record holds the
  answer. Use when someone asks "anything I should know before I change X", whether a rule has an accepted exception,
  or what the corpus covers as a whole. Use it as well, unprompted, after a change that leaves a record here
  describing something that is no longer true.
---

# Asking the corpus a question you cannot place

The corpus is what the organisation has written down about its own work, and it travels with this plugin as data.
Each type of record answers one kind of question, and a lookup skill reads one type. This skill sends a question to
the reader for the type that answers it. Use it when you cannot yet name that type. Where you can, go straight to the
lookup.

There is nothing to install and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                # which types travelled, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/bundle.json                         # which skills shipped, and which were left out and why
${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/deviations.md # how to read a deviation, and whether it is current
${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/reports.md    # how to answer from a report
${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/runbooks.md   # how to answer from a runbook when something is broken
```

Use those paths exactly as written. An installed plugin sits in a cache outside the repository you are working in.

## Read what travelled first

**Read `types` in `manifest.json` before you route anything.** A lookup ships only where the export has its type, so
a type missing from `types` means its lookup is missing too. `bundle.json` lists each skill that shipped under
`included`, and each one left out under `trimmed`, with the reason. A skill you can load is one that shipped.

## Route the question

Match the question to a row, then go where the row says. Read a reference file only when a row sends you to it.

| The question                                                     | Type       | Go to              |
|------------------------------------------------------------------|------------|--------------------|
| What is the rule for X. What must I build to                     | standards  | `standards-lookup` |
| Are we allowed to X. What are we committed to                    | policies   | `policy-lookup`    |
| What checks this rule. Is it enforced                            | controls   | `controls-lookup`  |
| How do we carry out this planned task                            | processes  | `process-lookup`   |
| Has anyone hit this problem. What does this error mean           | fixes      | `fix-lookup`       |
| What does this word mean here                                    | glossary   | `glossary-lookup`  |
| Does my change meet the standards                                | standards  | `standards-review` |
| Is there an accepted exception to this rule. Is it still current | deviations | `deviations.md`    |
| What does the corpus cover. Which clauses have nothing behind it | reports    | `reports.md`       |
| Something is broken. What do I do first                          | runbooks   | `runbooks.md`      |
| Anything I should know before I change X                         | any        | the next section   |

**Where the row's skill did not ship, say so, and name the type it needed.** That type is not in `types`, so the
corpus does not export it. Say that as the answer.

## Ask about the thing you are about to change

A question such as "anything I should know before I change X" names a subject and no type. Find what the corpus
records about X, then read each record through the route for its type.

* **Name X as the records would**: a service, a file, a pipeline, a tool or a rule. Take its id where you know one.
* **Search `${CLAUDE_PLUGIN_ROOT}/corpus/` for that name or id.** Each hit's directory under `corpus/` is its type.
* **Read each hit through the row for its type.** Where no row covers the type, read the record's file whole, quote
  what it says, and link its `links.human`.

**Read every deviation the search returns.** An accepted deviation changes what is right for X. A change that closes
one, or relies on one, is something the person asking needs to know before the work starts.

## Say when there is nothing

Where nothing matches, name the corpus and every entry in `sources` in `manifest.json`, so the reader knows what was
searched. An empty search states only that the estate has not written this down.

## Report a change that makes a record wrong

**Where your change alters something a record here describes, load `raise-finding` once the change exists.** Name
the record that is now wrong, or the one now missing, by its id and `links.human`. A service you reconfigured, a
pipeline step you added or removed, and a deviation your change now relies on or closes are all cases. The copy here
is frozen, so an issue is the only way the people who keep the records hear of it.

State what you saw and what you changed. Propose no record and no new wording, because whoever triages the finding
decides what it becomes.
