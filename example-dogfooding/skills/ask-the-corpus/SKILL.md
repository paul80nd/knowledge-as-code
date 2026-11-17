---
name: ask-the-corpus
description: Answer from the organisation's recorded knowledge, which travels with this plugin as data. Use when
  someone asks "anything I should know before I change X". Use it as well, unprompted, after a change that leaves a
  record here describing something that is no longer true.
when_to_use: "Use before you write or change code or a build, or when asked what rule applies or which standards exist. Use before designing for secrets, access, data or production, or asked what is allowed or what policies exist. Use before changing a check, CI job or checked rule, or when asked what checks a rule, or what nothing checks. Use before a planned task that may be written down, or when asked how a task is done or which processes exist. Use before you debug or file a finding on a problem, or when given an error or asked what problems are known. Use when asked what a term means, how two differ or which exist, or on a business noun the code cannot define. Use when someone asks whether a rule has an accepted exception. Use when someone asks what the corpus covers as a whole, or what nothing covers. Use when something is failing now and someone needs the agreed first step."
---

# Answering from the corpus

The corpus is what the organisation has written down about its own work, and it travels with this plugin as data.
Each type of record answers one kind of question, and a reference file beside this one says how to answer from a
record of that type. Send the question to the file for its type, and read only that file.

There is nothing to install and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                # which types travelled, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/bundle.json                         # which files shipped, and which were left out and why
${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/<type>.md     # how to answer from a record of one type
${CLAUDE_PLUGIN_ROOT}/skills/ask-the-corpus/retrieval.md  # how to fetch a record's published source
```

Use those paths exactly as written. An installed plugin sits in a cache outside the repository you are working in.

## Read what travelled first

**Read `types` in `manifest.json` before you route anything.** A reference file ships only where the export has its
type, so a type missing from `types` means its file is missing too. `bundle.json` lists each file and skill that
shipped under `included`, and each one left out under `trimmed`, with the reason.

## Route the question

Match the question to a row, then read the file the row names. Read a reference file only when a row sends you to it.

| The question                                                     | Type       | Read               |
|------------------------------------------------------------------|------------|--------------------|
| What is the rule for X. What must I build to                     | standards  | `standards.md`     |
| Are we allowed to X. What are we committed to                    | policies   | `policies.md`      |
| What checks this rule. Is it enforced                            | controls   | `controls.md`      |
| How do we carry out this planned task                            | processes  | `processes.md`     |
| Has anyone hit this problem. What does this error mean           | fixes      | `fixes.md`         |
| What does this word mean here                                    | glossary   | `glossary.md`      |
| Is there an accepted exception to this rule. Is it still current | deviations | `deviations.md`    |
| What does the corpus cover. Which clauses have nothing behind it | reports    | `reports.md`       |
| Something is broken. What do I do first                          | runbooks   | `runbooks.md`      |
| Does my change meet the standards                                | standards  | `standards-review` |
| Anything I should know before I change X                         | any        | the next section   |

`standards-review` is a skill of its own. Load it.

**Where the row's reference file or skill did not ship, say so, and name the type it needed.** That type is not in
`types`, so the corpus does not export it. Say that as the answer.

## Ask about the thing you are about to change

A question such as "anything I should know before I change X" names a subject and no type. Find what the corpus
records about X, then read each record through the route for its type.

* **Name X as the records would**: a service, a file, a pipeline, a tool or a rule. Take its id where you know one.
* **Search `${CLAUDE_PLUGIN_ROOT}/corpus/` for that name or id.** Each hit's directory under `corpus/` is its type.
* **Read each hit through the row for its type.** Where no row covers the type, read the record's file whole, quote
  what it says, and link its `links.human`.

**Read every deviation the search returns.** An accepted deviation changes what is right for X. A change that closes
one, or relies on one, is something the person asking needs to know before the work starts.

## Read the prefix on an id

**A prefix names the corpus that wrote the record.** `eng:std-TEST.LEVEL` was published by the corpus with shortcode
`eng`, and a bare `std-TEST.LEVEL` was written here. An inherited record binds as fully as one written here. Look its
`shortcode` up in `sources` in `manifest.json`, and name that corpus in words.

**A record file sits under its producer's shortcode.** `eng:std-TEST` is
`${CLAUDE_PLUGIN_ROOT}/corpus/standards/eng/std-TEST.json`, and a bare `std-TEST` is
`${CLAUDE_PLUGIN_ROOT}/corpus/standards/std-TEST.json`. Every type files its records the same way.

## Say when the nearest record is about something else

Where the estate has not written your subject down, say so in the words it would use. Quote the nearest record, and say
what it governs. Whether your work falls inside it is a ruling for its owner. A nearest record passed off as the answer
is a requirement nobody set.

## Say when there is nothing

Where nothing matches, name the corpus and every entry in `sources` in `manifest.json`, so the reader knows what was
searched. An empty search states only that the estate has not written this down: it permits nothing.

## Report a change that makes a record wrong

**Where your change alters something a record here describes, load `raise-finding` once the change exists.** Name
the record that is now wrong, or the one now missing, by its id and `links.human`. A service you reconfigured, a
pipeline step you added or removed, and a deviation your change now relies on or closes are all cases. The copy here
is frozen, so an issue is the only way the people who keep the records hear of it.

State what you saw and what you changed. Propose no record and no new wording, because whoever triages the finding
decides what it becomes.
