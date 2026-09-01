---
name: policy-lookup
description: Find what this estate is committed to, in the policy clauses that travel with this plugin. Use when
  someone asks whether something is allowed here — "do we have a policy on X", "what does our policy say about X",
  "are we allowed to X", "is X against policy", "what are we on the hook for". Use it as well, unprompted, before you
  propose a design or a change touching secrets, access, retention, backups, logging, accessibility, third-party
  components, or what reaches production. Read the clause before you assume nothing governs the thing you are about
  to do.
---

# Looking a clause up in the policies

The corpus travels with this plugin as data. You read it with the tools you already have. There is nothing to install
and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                      # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/policies/clauses.jsonl             # one line of JSON per clause. Search this
${CLAUDE_PLUGIN_ROOT}/corpus/policies/<record>.json             # one policy this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/policies/<shortcode>/<record>.json # one policy a corpus this one consumes wrote
```

Use those paths exactly as they appear above; they are already absolute. An installed plugin sits in a cache of its own
rather than in the repository you are working in. A path you build relative to the working directory resolves nowhere.

One file holds every clause, whoever committed to it. A corpus that consumes another exports both, so `clauses.jsonl`
carries this corpus's clauses and the clauses of every corpus above it, and one search reaches all of them. An estate
is bound by what it inherited as surely as by what it wrote.

## Find the commitment, and leave the verdict

**This skill finds a commitment. It does not rule on one.** A clause line carries the wording, the level it binds at
and the id to cite it by. That is enough to quote what we committed to and say where it is written. It is not enough to
say whether a design meets it. Whoever asked wants a ruling, and the ruling belongs to the estate.

So: quote the clause, name its level, link the policy, and say plainly which part of the question you are leaving to
the reader. Send them to the published record for the rest.

## Search the clause table

**Use your Grep tool, not a shell command.** It runs on every platform and needs no shell, which is what makes the
promise above true for a reader on Windows. Point it at `${CLAUDE_PLUGIN_ROOT}/corpus/policies/clauses.jsonl`, ask for
matching content rather than a list of files, and search case-insensitively.

Two patterns, in this order:

1. **`<subject>`** on its own finds every clause mentioning the thing you are asking about. A clause is written in
   ordinary words, so search the words the estate would use: `secret`, `restore`, `retention`.
2. **`"record":\s*"<policy-id>"`** collects every clause of one policy, once a first hit has told you which policy
   covers the subject. Write the `\s*`. Nothing promises the export puts no space after a colon, and a pattern
   assuming one returns nothing the day that changes.

**Search the stem rather than the word.** "backups" misses "back up", and "retention" misses "retain". Try both
spellings, and try the plainer word the clause is more likely to use.

**Read the `clause` of every hit before you use it.** The field names in this file are ordinary English words:
`clause`, `level`, `record`, `status`, `type`. A search for one of those matches every line in the file. A line
governs your subject when its wording says so, never because it matched.

Each line carries the clause whole:

| Field                | What it holds                                                              |
|----------------------|----------------------------------------------------------------------------|
| `id`                 | `<policy-id>.<clause-key>`, the address to quote and to cite               |
| `clause`             | the obligation, in the words the policy wrote                              |
| `level`              | `MUST`, `MUST NOT`, `SHOULD` or `COULD`. Read this before anything else    |
| `record`             | the policy the clause belongs to                                           |
| `part`               | the clause's key inside that policy                                        |
| `shortcode`          | the corpus that published the clause, absent where this corpus wrote it    |
| `status`, `reviewBy` | how far the policy has settled, and the date it was meant to be read again |
| `path`, `anchor`     | the two values a link template takes, and see below for which template     |

## Read the prefix on an id

**A prefix on an id names the corpus that committed to the clause.** `eng:pol-SCRT.LOGS` is the clause `LOGS`, in the
policy `pol-SCRT`, as the corpus whose shortcode is `eng` published it. The prefix sits on `id` and on `record`, so an
id you take from one line and search for carries it.

**A bare id belongs to the corpus you installed.** `pol-SCRT.LOGS` was written here.

**An inherited clause binds.** A corpus consumes another because that other one governs it, so a clause arriving under
a prefix is a commitment this estate is held to. Quote it as readily as one written here, and say who wrote it.

**`shortcode` is the key into `sources`.** Each entry in `sources` in `manifest.json` holds one producing corpus: its
name, the version of it that travelled, and where it publishes. Look the shortcode up there before you say anything
about the clause's origin, and name the corpus in words. `eng` means nothing to a reader who has not read the manifest.

**A record file sits under its producer's shortcode**, because two corpora can name one policy and a filename cannot
say whose it is. So the owning record for `eng:pol-SCRT.LOGS` is
`${CLAUDE_PLUGIN_ROOT}/corpus/policies/eng/pol-SCRT.json`, and for a bare id it is
`${CLAUDE_PLUGIN_ROOT}/corpus/policies/pol-SCRT.json`.

## Read `level` before you answer

**`level` is the field the answer turns on.** Two clauses of one policy open the same shape of sentence and bind
differently. Read a `COULD` as a `MUST` and you have invented a rule. Read a `MUST` as advice and you have dropped one.

* **`MUST` and `MUST NOT` bind.** The first requires the thing, the second prohibits it.
* **`SHOULD` and `COULD` advise.** Neither is a rule, and neither blocks anything on its own.

**Never take the level out of the wording.** `MUST NOT` opens with `MUST`, so a match on the shorter modal files a
prohibition as an obligation, which is the reverse of what the policy says. The wording also arrives with its markup
stripped, so nothing in it is emphasised the way the table emphasised it. Take `level`, compare it whole, and say which
of the four you found.

## Read the policy beside the clause

**A clause read on its own is stricter than the one we wrote.** Open the owning record, at the path *Read the prefix on
an id* builds from `record` and `shortcode`, and read three things from `sections`:

* **`Scope`.** It says what the clauses bind. A clause about every store binds every store the Scope admits, and
  nothing outside it.
* **`Exceptions`.** It says where a clause gives way and what has to be recorded when it does. Someone asking "we
  cannot do that here" is often asking about this section.
* **`Purpose`.** It travels as its opening paragraph, which is where a policy states its position. Use it to tell
  whether the clause you found is about the subject at all.

The clause table is not among those sections. Its rows travelled as the lines in `clauses.jsonl`, one line each.

## Build a link from a template

**No line holds a URL.** `manifest.json` holds a publishing block per corpus, and each line holds the two values a
template takes: `path` and `anchor`.

**Take the block belonging to the corpus that wrote the line.** A line carrying `shortcode` is published by that entry
in `sources`, and its `publishing` block is the one to read. A line with no `shortcode` is published by the top-level
`publishing` block. Read the wrong one and you address the right path in the wrong repository at the wrong commit,
which fetches a 404 or somebody else's file, and both read as plausible.

**Copy a template exactly as it stands, replace `{path}` and `{anchor}` with the line's own values, and change nothing
else.** The commit is already inside the string. Do not retype it, shorten it, swap the host or judge whether it looks
right. A template with one character altered gives a 404 that reads as plausible, or a page from a version of the
corpus nobody asked about.

**One target spells `{path}` differently.** Where the block's `target` is `azure-devops-wiki`, the template addresses a
wiki page rather than a file, so substitute the line's `path` with `.md` removed and every `/` written as `%2F`. Every
other target takes the `path` whole. Two corpora can publish to two targets, so read `target` from the block you chose
above, every time.

**To send a reader to a policy, use the block's `humanTemplate`.** Substitute `path` and `anchor`. Every clause of one
policy carries the same anchor, because a table row is not a heading and no renderer gives it a fragment of its own.
The link lands on the clause table, and the reader finds the row by the id you quoted.

**To read a policy's source yourself, fetch the file rather than the page.** The same block names the `target`, the
`base`, the `pathPrefix` and the `ref`. Join `pathPrefix` ahead of the line's `path` to reach the file inside the
repository, then ask the client that authenticates to that target for it at that `ref`. Fetching the human URL instead
hands you the markdown wrapped in someone else's HTML, and you will read the page furniture as though it were the
record.

**No unauthenticated host serves that source**, except GitHub's and only for a public repository. Where you have no
client for the target, say so and quote the human link, rather than assembling a URL that will return a sign-in page you
read as the record.

**Where the block's `humanTemplate` is `null`**, that corpus publishes nowhere the export could address. Say so, and
quote the `path` as the policy's place in its own repository. Do not assemble a URL of your own.

## Say what stayed behind

Three things a reader may expect are not here. Name whichever one the question reaches, rather than filling the gap:

* **What an external framework obliges us to.** A clause maps to a framework reference through the corpus's own
  register of frameworks, and no consumer receives that page. The register is what says whether we are obliged,
  self-obligated or merely borrowing an idea, so a mapping quoted without it claims a commitment nobody can check.
* **How the commitment is met.** A policy states what we are on the hook for and a standard beneath it states how to
  satisfy that. `types` in `manifest.json` lists what this export actually carried, and a type absent from it stayed
  behind whole.
* **Anything a clause cites.** A table row has no body, so a clause carries no `seeAlso`. A clause pointing at another
  clause arrives with that id inside its own words: search for the id to read it.

## Say when a policy is unsettled

Two fields on the line say how far the policy has settled, in three states. Read both, and tell the user what
you saw:

* **`status: draft`** — the policy was not agreed when the export was taken, so its clauses are a proposal.
* **`status: retired`** — the policy was withdrawn and kept for the record. It binds nothing.
* **`reviewBy` earlier than today** — the policy passed the date it was meant to be read again.

An export is a copy taken on a day, and it reads the same however long ago that was. `generatedAt` and `commit` in
`manifest.json` say when it was taken, and are worth quoting alongside any of the three.

## Say when there is nothing

Where no clause matches, say nothing in this export commits to anything on the subject, and name the corpus and every
entry in `sources` from `manifest.json`, so a reader knows which policies were searched. Silence is not permission, and
it is not a rule you may supply: it says only that the estate has not written this down. Do not read a commitment out
of a policy about something else. Offer the subject as one worth a policy, and leave that to whoever owns it.
