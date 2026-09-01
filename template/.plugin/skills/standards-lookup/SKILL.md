---
name: standards-lookup
description: Find the rules you have to build to, in the standards that travel with this plugin. Use when someone
  asks how something is done here — "what is our standard for X", "how do we do X", "is there a rule about X", "what
  do I have to do to ship this". Use it as well, unprompted, before you write or change code touching secrets, APIs,
  tests, builds, deployment, logging, dependencies, or data a service keeps. Read the rule before you assume nothing
  governs the thing you are about to build.
---

# Looking a rule up in the standards

The corpus travels with this plugin as data. You read it with the tools you already have. There is nothing to install
and nothing to run.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json                       # what this export is, and where each corpus publishes
${CLAUDE_PLUGIN_ROOT}/corpus/standards/rules.jsonl               # one line of JSON per rule. Search this
${CLAUDE_PLUGIN_ROOT}/corpus/standards/<record>.json             # one standard this corpus wrote
${CLAUDE_PLUGIN_ROOT}/corpus/standards/<shortcode>/<record>.json # one standard a corpus this one consumes wrote
```

Use those paths exactly as they appear above; they are already absolute. An installed plugin sits in a cache of its own
rather than in the repository you are working in. A path you build relative to the working directory resolves nowhere.

## Find the rule, and leave the verdict

**This skill finds a rule. It does not certify code against one.** A rule line carries the obligations and the id to
cite them by. That is enough to quote what the estate requires and say where it is written. It is not enough to say
the work in front of you conforms. Whoever asked wants a ruling, and the ruling belongs to the estate.

So: quote the obligation, name its keyword, link the standard, and say plainly which part of the question you are
leaving to the reader.

## Collect every rule that applies, rather than the first

**Standards compose by union.** The rules binding a piece of work are every rule in every category that reaches it, so
one hit is a partial answer. A general rule and a stack-specific one are two records writing the same subject at two
altitudes, and both bind.

**`rules.jsonl` already holds the whole union.** One file carries this corpus's rules and the rules of every corpus it
consumes, so a single search over it reaches everything that binds. What you are collecting is every hit within that
file, not a set of files to visit in turn.

Two fields on the owning record narrow a standard to your work, and you read both:

* **`category`** says which folder the standard was filed under. `common` reaches everything and a deeper folder
  reaches less.
* **`applies-to`** names the service ids the standard binds, or the single literal `all`.

So search the subject, read the categories the hits name, then search the file again for the categories you have not
covered yet. A rule you inherited narrows the same way a rule written here does.

## Search the rules

**Use your Grep tool, not a shell command.** It runs on every platform and needs no shell, which is what makes the
promise above true for a reader on Windows. Point it at `${CLAUDE_PLUGIN_ROOT}/corpus/standards/rules.jsonl`, ask for
matching content rather than a list of files, and search case-insensitively.

Two patterns, in this order:

1. **`<subject>`** on its own finds every rule mentioning the thing you are asking about. A rule is written in ordinary
   words, so search the words the estate would use: `secret`, `retry`, `contract`.
2. **`"record":\s*"<standard-id>"`** collects every rule of one standard, once a first hit has told you which standard
   covers the subject. Write the `\s*`. Nothing promises the export puts no space after a colon, and a pattern assuming
   one returns nothing the day that changes.

**Search the stem rather than the word.** "retries" misses "retry", and "deployment" misses "deploy". Try both
spellings, and try the plainer word the rule is more likely to use.

**Read the `obligations` of every hit before you use it.** The field names in this file are ordinary English words:
`title`, `obligations`, `record`, `status`, `type`. A search for one of those matches every line in the file. A line
governs your work when its wording says so, never because it matched.

Each line carries one heading of a standard's Rules section, with everything written under it:

| Field                | What it holds                                                                      |
|----------------------|------------------------------------------------------------------------------------|
| `id`                 | `<standard-id>.<rule-key>`, the address to quote and to cite                       |
| `title`              | the heading, which says what the obligations beneath it are about                  |
| `obligations`        | the bullets, in the markdown the standard wrote them in. Read this whole           |
| `seeAlso`            | the rules of other standards this one points at, or absent where it points at none |
| `record`             | the standard the rule belongs to                                                   |
| `part`               | the rule's key inside that standard                                                |
| `shortcode`          | the corpus that published the rule, absent where this corpus wrote it              |
| `status`, `reviewBy` | how far the standard has settled, and the date it was meant to be read again       |
| `path`, `anchor`     | the two values a link template takes, and see below for which template             |

## Read the prefix on an id

**A prefix on an id names the corpus that wrote the rule.** `eng:std-TEST.every-test-is-hermetic` is the rule
`every-test-is-hermetic`, in the standard `std-TEST`, as the corpus whose shortcode is `eng` published it. The prefix
sits on `id`, on `record` and on every `seeAlso` value, so an id you take from one line and search for carries it.

**A bare id belongs to the corpus you installed.** `std-TEST.every-test-is-hermetic` was written here.

**An inherited rule binds.** A corpus consumes another because that other one governs it, so a rule arriving under a
prefix is one you have to build to. Quote it as readily as one written here, and say who wrote it.

**`shortcode` is the key into `sources`.** Each entry in `sources` in `manifest.json` holds one producing corpus: its
name, the version of it that travelled, and where it publishes. Look the shortcode up there before you say anything
about the rule's origin, and name the corpus in words. `eng` means nothing to a reader who has not read the manifest.

**A record file sits under its producer's shortcode**, because two corpora can name one standard and a filename cannot
say whose it is. So the owning record for `eng:std-TEST.every-test-is-hermetic` is
`${CLAUDE_PLUGIN_ROOT}/corpus/standards/eng/std-TEST.json`, and for a bare id it is
`${CLAUDE_PLUGIN_ROOT}/corpus/standards/std-TEST.json`.

## Read the keyword on every bullet

**One line holds several obligations, and they do not all bind.** `obligations` is a list of bullets, and each carries
its own RFC 2119 keyword in capitals and in bold. No field on the line says how strongly a bullet binds, so the keyword
inside the wording is the only thing that says it. Take the bullet, not the line.

* **`MUST` and `MUST NOT` bind.** The first requires the thing, the second prohibits it.
* **`SHOULD` and `MAY` advise.** Neither is a rule, and neither blocks anything on its own.

**Only capitals count.** [RFC 8174](https://www.rfc-editor.org/rfc/rfc8174) gives the keywords their normative meaning
when they are written in capitals and not otherwise, so a lower-case "must" is prose. **`MUST NOT` opens with `MUST`**,
so a match on the shorter keyword files a prohibition as an obligation, which is the reverse of what the standard says.
Compare the keyword whole.

## Read the standard beside the rule

**A rule read on its own is stricter than the one we wrote.** Open the owning record, at the path *Read the prefix on
an id* builds from `record` and `shortcode`, and read two things from `sections`:

* **`Summary`.** It says what the standard is for, in a paragraph. Use it to tell whether the rule you found is about
  your subject at all.
* **`Conformance checklist`.** It is the same rules written as a test, so it is the fastest thing to hand somebody who
  has to show the work conforms.

Read `fields` as well. `applies-to` and `category` are the two the section above turns on.

The Rules section is not among those sections. Its headings travelled as the lines in `rules.jsonl`, one line each,
sorted by heading rather than in the order the standard writes them. Open the record where the author's grouping
matters.

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

**To send a reader to a rule, use the block's `humanTemplate`.** Substitute `path` and `anchor`. A rule is written as a
heading, so it has an anchor of its own and the link lands on the rule rather than at the top of the standard.

**To read a standard's source yourself, fetch the file rather than the page.** The same block names the `target`, the
`base`, the `pathPrefix` and the `ref`. Join `pathPrefix` ahead of the line's `path` to reach the file inside the
repository, then ask the client that authenticates to that target for it at that `ref`. Fetching the human URL instead
hands you the markdown wrapped in someone else's HTML, and you will read the page furniture as though it were the
record.

**No unauthenticated host serves that source**, except GitHub's and only for a public repository. Where you have no
client for the target, say so and quote the human link, rather than assembling a URL that will return a sign-in page you
read as the record.

**Where the block's `humanTemplate` is `null`**, that corpus publishes nowhere the export could address. Say so, and
quote the `path` as the standard's place in its own repository. Do not assemble a URL of your own.

## Say what stayed behind

**`types` in `manifest.json` is the list of what travelled.** What a reader receives depends on which types the corpus
adopted and which its producers exported, so nothing here can name a fixed set of absences. Read `types`, and a type
that is not in it stayed behind whole.

Two things stay behind whatever `types` says, because they are sections of a standard rather than types of their own:

* **A worked example.** Every standard holds an Examples section showing the rule met and the rule missed, and it stays
  in the record. Send a reader who wants one to the published standard.
* **Why the rule exists.** The argument lives in the standard's Rationale and provenance section, which stays behind
  because it reasons over records this export need not carry.

Two more depend on `types`, so check it before you say either is missing:

* **What the rule descends from.** A standard names the policy clause it puts into practice. Where `policies` is in
  `types` the clause is here, under the prefix of the corpus that wrote it, and you can quote it. The obligation
  travels whole either way.
* **How anyone checks it happened.** A standard says what to do, and a control says how we know it was done. Where
  `controls` is not in `types`, no control travelled and nothing here says whether anyone looked.

`seeAlso` is the one cross-reference that does travel: it names the rules of other standards this one leans on, and
composition means a rule commonly leans on one. Follow them before you answer, prefix and all. A policy clause the rule
cites stays inside the wording instead, because a clause is a table row and the link names the policy alone.

## Say when a standard is unsettled

Two fields on the line say how far the standard has settled, in four states. Read both, and tell the user what you saw:

* **`status: draft`** — the standard was not agreed when the export was taken, so its rules are a proposal.
* **`status: deprecated`** — the standard is on its way out, and nothing new is built to it.
* **`status: superseded`** — another standard replaced it. Find that one before you quote this.
* **`reviewBy` earlier than today** — the standard passed the date it was meant to be read again.

An export is a copy taken on a day, and it reads the same however long ago that was. `generatedAt` and `commit` in
`manifest.json` say when it was taken, and are worth quoting alongside any of the four.

## Say when there is nothing

Where no rule matches, say nothing in this export states a rule on the subject, and name the corpus and every entry in
`sources` from `manifest.json`, so a reader knows which standards were searched. Silence is not permission, and it is
not a rule you may supply: it says only that the estate has not written this down. Do not read an obligation out of a
standard about something else. Offer the subject as one worth a standard, and leave that to
whoever owns it.
