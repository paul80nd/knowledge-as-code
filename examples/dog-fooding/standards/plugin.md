---
id: std-PLUGIN
type: standard
tier: normative
status: active
implements: [ eng:pol-AGNT.ACCEPT, eng:pol-AGNT.ACCESS, eng:pol-AGNT.CONFID, eng:pol-AGNT.EQUAL,
  eng:pol-AGNT.PROV, eng:pol-AGNT.SELFVER, eng:pol-AGNT.UNPROV, eng:pol-DEVI.CONTENT, eng:pol-DEVI.EXPIRY,
  eng:pol-DEVI.OWNER, eng:pol-DEVI.PERM, eng:pol-DEVI.SURFACE, eng:pol-KNOW.COPY ]
verified-by: [ ctl-0008, ctl-0009 ]
applies-to:
  - all
review-by: "2027-09-07"
owner: human:paul.law
tags: [ agents, export, plugin, skills ]
---

# The plugin carries the corpus, and a skill answers from what travelled

`Standard: std-PLUGIN` `ACTIVE`

## Summary

Every corpus here publishes a Claude Code plugin: a frozen copy of its export, and the skills that read it.
`template/.plugin/` contains those skills, `plugin.json` says which of them travel, and `kac bundle` assembles both. A
skill answers from the export beside it, using no more than a session's ability to read a file, and states plainly what
did not travel. The copy it reads cannot be changed, so a skill with something to send back raises an issue on the
repository the export came from: a finding where the corpus is wrong, and a request where the work is about to depart
from a clause that is right. Back at that repository, a skill triages those issues, marks each one with the route it
belongs on, and drafts the record a routed finding asks for as a pull request.

## Rules

### A skill assumes only that the session can read a file

- A skill **MUST** leave the choice of search tool to the session.
- A skill **MUST** name the file or the directory containing the answer.
- A skill **MUST NOT** require a shell, an interpreter or a runtime on the reader's machine.
- A skill **MUST NOT** require a network call to answer from the export.
- A skill **MUST NOT** ask for access beyond reading the files under `${CLAUDE_PLUGIN_ROOT}`.
- Every path a skill names **MUST** open on `${CLAUDE_PLUGIN_ROOT}`.

_**Covers:** `eng:pol-AGNT.ACCESS`_

### A skill names the type of every field it describes

- A skill describing a field **MUST** name that field's JSON type.
- A skill **MUST** say that a field with no value is present and `null`, where the export writes it that way.
- A skill **MUST** name each key that is absent rather than `null`.
- A skill **MUST NOT** describe a string of markdown as a list.
- Every field description **MUST** agree with a line of a real export.
- A skill **MUST** name the parts file of the type its component declares, and no other type's.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-AGNT.UNPROV`_

### A skill says what its corpus did not carry

- A skill **MUST** read `types` in `manifest.json` before it says what is missing.
- A skill **MUST** name what stayed behind, and send the reader to the published record for it.
- A skill **MUST NOT** name a fixed set of absences.
- A skill **MUST** name the corpus and every entry in `sources` it searched, where nothing matched.
- A skill **MUST** say that an empty search states only that the estate has not written this down.
- A skill **MUST NOT** read an obligation out of a record about something else.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-AGNT.UNPROV`_

### `plugin.json` declares every component and the types it reads

- Every skill directory and hook directory in the plugin tree **MUST** appear under `metadata.components`.
- A component **MUST** name in `requires` every record type it opens a file of, at the shape version it reads, as
  `glossary@1`.
- A component that needs a type present and opens none of its files **MUST** name that type bare.
- A component that reads no export at all **MUST** declare `requires` empty.
- A component that reads no export and supports no other component **MUST** declare `standalone` true.
- A component entry **MUST** have a `note` saying what it reads.
- `metadata.corpusRoot` **MUST** name the directory every skill in the tree addresses the export through.
- A component naming a type the export does not include **MUST** leave the plugin, with every file under its declared
  path.
- `bundle.json` **MUST** name every component that left, and the type that left it out.

_**Covers:** `eng:pol-AGNT.UNPROV`_

### The bundle carries a frozen export, and nothing writes to it

- The export inside a plugin **MUST** be the bytes `kac export` wrote.
- A skill **MUST NOT** write to any file under `${CLAUDE_PLUGIN_ROOT}`.
- A skill **MUST** report a `status` its type does not treat as in force, under every such value that type declares.
- A skill **MUST** report a review date that has passed, where its type exports one.
- A skill **MUST** name that date under the key its type's export writes: `reviewBy` on a part line, and
  `review-by` from a record's frontmatter.
- A skill **MUST** quote `generatedAt` from `manifest.json` alongside either.
- A skill **MUST** quote the wording the export contains, and link the record for the rest.
- The words a skill quotes **MUST** come from the export, and never from a second copy kept for agents.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-KNOW.COPY`_

### A skill writes back to the repository the export came from

- A skill **MUST** send a correction to the repository the record's publishing block addresses.
- A skill **MUST** raise it as an issue or a work item.
- A skill **MUST** use the client that already authenticates to that platform.
- A skill **MUST NOT** offer to edit the installed copy.
- A skill **MUST NOT** ask the reader for a credential.

_**Covers:** `eng:pol-AGNT.ACCESS`, `eng:pol-KNOW.COPY`_

### A finding an agent files is an observation, and proposes no record

- A finding **MUST** open its body with a fenced block whose info string is `yaml kac-finding`.
- That block **MUST** open on `corpus`, naming the corpus the finding is about.
- The block **MUST** then contain `source`, `confidence`, `looks-like` and `provenance`, in that order.
- The block **MAY** contain `applies-to` and `tags` after those.
- The block **MUST NOT** contain any other key, and **MUST NOT** propose an id for a record.
- `source` **MUST** be `session`.
- `confidence` **MUST** be `unverified` where the filing session's own account is the only evidence.
- `looks-like` **MUST** name a type the export declares, `framework`, or `none`.
- `looks-like` **MUST** be `framework` where the finding reports the framework rather than a record.
- `provenance` **MUST** name the agent, the session it ran in, the repository, the commit it read, and what it was
  doing.
- Where the agent cannot reach one of those, `provenance` **MUST** name it as unreached.
- The body **MUST** contain `## What I saw`, `## Context` and `## Why it might matter`, in that order.
- A finding **MUST NOT** put anything between the block and the first of those headings.
- The title **MUST** be the observation in one line.
- A skill **MUST** mark the issue `kac:finding`, using whatever the platform calls a label.
- Where the platform refuses a mark it does not already have, a skill **MUST** file the finding unmarked.
- A skill **MUST** say that it filed one unmarked.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-AGNT.PROV`, `eng:pol-AGNT.SELFVER`_

### A deviation request an agent files asks for the owner it cannot name

- A deviation request **MUST** open its body with a fenced block whose info string is `yaml kac-deviation`.
- That block **MUST** open on `corpus`, naming the corpus that will contain the record.
- The block **MUST** then contain `id`, `status`, `departs-from` and `review-by`, under the names and in the order the
  `deviations` template writes them.
- The block **MAY** contain `applies-to` and `tags` after those.
- The block **MUST NOT** contain any other key.
- `status` **MUST** be `draft`.
- Every entry in `departs-from` **MUST** name a clause, scoped exactly as the export writes that record's id.
- An entry **MUST NOT** name a whole policy or standard.
- A skill **MUST NOT** write `owner` or `accepted-on`, because the individual who accepts the risk is what the request
  asks for.
- A request **MUST NOT** describe the departure as permanent, indefinite, or standing until further notice.
- The body **MUST** contain `## What we are doing instead`, `## Why we need it`, `## What compensates` and
  `## How it closes`, in that order.
- `## Who is asking` **MUST** follow those four.
- `## Who is asking` **MUST** name the agent, the session it ran in, the repository, the commit it read, and what it was
  doing.
- Where the agent cannot reach one of those, `## Who is asking` **MUST** name it as unreached.
- The title **MUST** be the departure in one line, naming the work and the rule it breaks.
- A skill **MUST** file the request on the repository addressed by the `publishing` block of the corpus that owns the
  clause.
- A skill **MUST NOT** file a request on a repository outside the organisation holding the plugin.
- A skill **MUST** mark the issue `kac:deviation`, using whatever the platform calls a label.
- Where the platform refuses a mark it does not already have, a skill **MUST** file the request unmarked.
- A skill **MUST** say that the request accepts nothing, and that the record is still owed.

_**Covers:** `eng:pol-AGNT.PROV`, `eng:pol-DEVI.CONTENT`, `eng:pol-DEVI.EXPIRY`, `eng:pol-DEVI.OWNER`,
`eng:pol-DEVI.PERM`, `eng:pol-DEVI.SURFACE`_

### Triage routes a finding with a label, and says why in a comment

- A skill triaging findings **MUST** run only where the session is in a checkout of the repository the corpus's
  `publishing` block addresses.
- A skill **MUST NOT** triage, mark or comment on a tracker reached from an installed plugin alone.
- A skill **MUST** select its queue as `kac:finding` without `kac:triaged`.
- A skill **MUST** also find an open issue whose body has the `yaml kac-finding` block and no `kac:finding` mark.
- A skill **MUST** say where a query returned as many issues as its limit allows.
- A skill **MUST NOT** take `kac:finding` off an issue it triaged.
- A skill **MUST** apply `kac:triaged` and exactly one route mark to every issue it triaged.
- The route **MUST** be one of `kac:route-framework`, `kac:route-record`, `kac:route-misfiled`, `kac:route-none` and
  `kac:route-unclear`.
- A skill **MUST** apply `kac:route-unclear` wherever choosing between the others would be a guess.
- A skill **MUST** create every mark it applies before it applies one, on a platform that refuses an unknown mark.
- A skill **MUST** comment the reason for the verdict on the issue it marked.
- A comment routing a duplicate **MUST** name the issue it duplicates.
- A comment routing a finding a record already answers **MUST** name that record.
- The comment **MUST NOT** state the route as machine-readable data, because the mark is that data.
- A skill **MUST NOT** state whether the finding's claim is true.
- A skill **MUST** show every verdict to a person and wait, before it writes any mark or comment.
- A skill drafting a record from a finding **MUST** take it only from an issue marked `kac:route-record`.
- That record **MUST** name an `owner` the person running the skill supplied, and the skill **MUST NOT** invent one.
- That record **MUST** carry a `status` its type treats as unsettled, wherever the type declares one.
- That record **MUST** arrive as a pull request, and the skill **MUST NOT** approve or merge it.

_**Covers:** `eng:pol-AGNT.ACCEPT`, `eng:pol-AGNT.EQUAL`, `eng:pol-AGNT.SELFVER`_

### A framework finding travels stripped, and only by hand

- A skill **MUST** compare the `id` of the framework block with the `id` of the tracker block.
- Where the two are equal, a skill **MUST** mark the finding and copy nothing.
- Where the two differ, a skill **MUST** post the upstream body as a comment on the issue and stop.
- A skill **MUST NOT** file a finding on a tracker outside the organisation holding the plugin.
- The upstream body **MUST NOT** contain the session id, the repository the session worked in, that repository's
  commit, or the name of the corpus the finding was filed from.
- The upstream body **MUST** state the version of the framework the session ran.

_**Covers:** `eng:pol-AGNT.PROV`_

## Examples

```
✅ Good
**Read the `obligations` of every hit before you use it.** The field names in this file are ordinary English
words: `title`, `obligations`, `record`, `status`, `type`. A search for one of those matches every line in
the file.

| Field         | Type                     | What it holds                                             |
|---------------|--------------------------|-----------------------------------------------------------|
| `obligations` | string or null           | every bullet under that heading, as one block of markdown |
| `seeAlso`     | list of strings, or null | the rules of other standards this one points at           |

**A key with no value is `null`, and the key is still there.** Test the value rather than the key.

❌ Avoid
**Use your Grep tool, not a shell command.** It runs on every platform and needs no shell.

| Field         | What it holds                                                   |
|---------------|-----------------------------------------------------------------|
| `obligations` | the bullets, in the markdown the standard wrote them in         |
| `seeAlso`     | the rules this one points at, or absent where it points at none |
```

The good instruction names no tool. It says which file the answer is in, and what will mislead a reader who takes a
match for an answer. It leaves how to search to the session, which knows what tools it has. The avoided instruction
names a tool the session may not have, so a reader without it must choose between ignoring the instruction and
stopping. The avoided table states no type at all, so a reader iterates a string of markdown one character at a time.
It says a field with no value is absent, so a reader testing for the key finds it every time and reads nothing.

A finding is the whole of an issue body. `fix-0002` settled what a session first noticed about Rider, and this is the
finding that observation would have arrived as.

`````
✅ Good
Title: Rider does not re-read an .editorconfig changed from a shell

```yaml kac-finding
corpus: example-dogfooding
source: session
confidence: unverified
looks-like: fixes
provenance: >
  Claude Code, in session 01J8ZC4M6QK2XR7VN0PYWTB3AE, in paul80nd/knowledge-as-code at 24dcea21,
  editing .editorconfig from a shell while the developer had the repository open in Rider.
tags: [ editorconfig, formatting, rider ]
```

## What I saw

A key edited in `.editorconfig` from the terminal changed nothing about how Rider formatted a file. Opening
`.editorconfig` in the IDE made the same change take effect. Flipping a key with an obvious effect was the
control.

## Context

Seen once, on one machine. The Rider version was not recorded.

## Why it might matter

A session changes `.editorconfig`, sees no difference in the IDE, and concludes the edit did nothing. The
next move is usually to change something else that was already right.

❌ Avoid
Title: An observation about Rider

While working on the wrap width I noticed that Rider seems to cache `.editorconfig`, which could be worth
looking into. Fairly confident about this one. Raised by an agent session.
`````

The avoided body states the same observation and gives whoever triages it nothing to act on. There is no commit
anybody can read, and no `looks-like` saying which type the observation would fit. Its title gives the subject alone,
so a reader scanning the issue list learns nothing. "Fairly confident" is not a value `confidence` takes, and a
session vouching for itself is what `eng:pol-AGNT.SELFVER` refuses.

A deviation request opens on a block of the same kind, and what it leaves out is the point.

`````
✅ Good
Title: Adopt the reconciliation client before it has been screened for vulnerabilities

```yaml kac-deviation
corpus: example-payments
id: dev-unscreened-recon-client
status: draft
departs-from: [ eng:pol-TRUS.SCREEN ]
review-by: "2026-11-07"
applies-to: [ svc-payment-api ]
tags: [ dependencies, vulnerabilities ]
```

❌ Avoid
Title: Dependency screening deviation

```yaml kac-deviation
corpus: example-payments
id: dev-unscreened-recon-client
status: active
departs-from: [ eng:pol-TRUS ]
owner: human:paul.law
review-by: "until the vendor publishes an SBOM"
```
`````

The avoided block accepts the risk on somebody else's behalf. It names an `owner` who has not answered, a `status`
saying the departure is already in force, and a `review-by` giving a condition where the field takes a date. Bare
`eng:pol-TRUS` claims a departure from every clause that policy states, where the work departs from one of them. Its
title names the subject, so a reader scanning the issue list learns neither which rule is broken nor which service
broke it.

## Conformance checklist

- [ ] Every instruction a skill gives can be followed by a session that can read a file and nothing else.
- [ ] Every path a skill names opens on `${CLAUDE_PLUGIN_ROOT}`.
- [ ] Every field a skill describes names its JSON type, and agrees with a line of a real export.
- [ ] Every skill names the parts file of the type it requires, and no other.
- [ ] Every skill says what its corpus did not include, from `types` in `manifest.json`.
- [ ] Every skill directory and hook directory in the plugin tree is declared under `metadata.components`.
- [ ] Every `requires` entry names a type at the shape version it reads, bare where it opens none of that type's
      files, and empty where the component reads no export.
- [ ] Every component reading no export declares `standalone` where it supports nothing, and leaves it off where it
      supports the components that do.
- [ ] No skill offers to write to the export, and each names the issue tracker instead.
- [ ] `bundle.json` names every component that left, and the type that left it out.
- [ ] Every finding opens on a `yaml kac-finding` block containing `corpus`, then the keys the rule names, and nothing
      else.
- [ ] Every finding's `provenance` names the agent, the session, the repository, the commit and what it was doing, or
      names which of those it could not reach.
- [ ] Every deviation request opens on a `yaml kac-deviation` block containing `corpus`, then the `deviations` keys
      the rule names, and nothing else.
- [ ] No deviation request states an `owner` or an `accepted-on`, and none reads as a standing departure.
- [ ] Every deviation request is filed inside the organisation holding the plugin, and says that nothing is accepted
      yet.
- [ ] No deviation request is dropped, or held back, because the target has no `kac:deviation` label.
- [ ] No finding is dropped, or held back, because the target has no `kac:finding` label.
- [ ] Triage ran only in a checkout of the repository the `publishing` block addresses.
- [ ] The queue read both the marked findings and the open issues whose body carries the block without the mark.
- [ ] Every triaged issue keeps `kac:finding`, and gains `kac:triaged` and exactly one `kac:route-*` mark.
- [ ] Every verdict was shown to a person and agreed before any mark or comment was written.
- [ ] Every verdict has a comment saying why, and none of them says whether the claim is true.
- [ ] No comment repeats its route as machine-readable data.
- [ ] Every record drafted from a finding came from a `kac:route-record` issue, and arrived as a pull request nobody
      approved.
- [ ] Every drafted record names an `owner` a person supplied, and carries the status its type treats as unsettled.
- [ ] No framework finding was filed outside the organisation holding the plugin.
- [ ] Every upstream body states the framework version and identifies neither the filer nor the corpus.

## Rationale and provenance

A skill is the one document here written for a reader nobody here can see. It runs in somebody else's session, on
somebody else's machine, against an export taken on an earlier day. Every rule above follows from one of those three
facts.

The session is why a skill names a property of the answer and not a tool. A session with no Grep tool met a
**MUST**-shaped instruction naming one, and its only options were to ignore the instruction or to stop. The machine is
why nothing here asks for a shell. Each skill promises at the top that there is nothing to install and nothing to run,
and a Windows reader with no `grep` is who that promise is for.

The frozen export is why a skill states a date and a status. An export reads the same however old it is, so the skill
is the only thing that can say how old it is. It is also why a correction goes out through the issue tracker: the
installed copy sits in a cache of the consumer's own, so an agent that edited it would change nothing anybody else
reads.

`requires` has three states, and the manifest rules keep them apart. A component that opens a type's files names the
shape it reads them at, so a moved key stops the build before it gets to a reader. A component that needs the type
present and opens none of its files names it bare, which is what the breadcrumb hook does. A component that reads no
export at all declares nothing.

That third state covers two different questions, so `standalone` separates them. `corpus-retrieval` and
`request-deviation` read no export and exist for the lookup skills. One fetches a record's published source and the
other asks the owner of a clause a lookup found, so a bundle that kept either after the last lookup skill left would
ship a skill nothing can call. `raise-finding` reads no export and serves whoever installed the plugin, and a corpus
with no record at all is the one a session most needs a route to report. Declaring it standalone keeps that route open.
`docs/design/plugin.md` describes how `kac bundle` acts on all of them.

A finding is an observation, and it proposes no record. A session cannot see what the receiving corpus already
contains, so an id it invents and an expiry it guesses are decisions taken by the wrong reader. The block states what
only the filer knows: which corpus this is about, who observed it, how far they can vouch for it, and what they were
doing. `confidence` is the part `eng:pol-AGNT.CONFID` asks for, because a session vouching for itself is not evidence.

`looks-like` gives triage a starting point. An agent can see which type an observation resembles, and says so in one
word. Whoever receives the issue decides where it belongs, and writes the record from it with every field the type
needs. `framework` is there for what no type holds: the tool, the schema, the plugin and the skills themselves.

The label is how a person filters and how triage reads its queue, and the block in the body is what makes a finding
findable when the label never arrived. `gh issue create` refuses a label the target repository does not have, and a
corpus published from somebody else's repository has whatever labels its maintainer chose. So a finding is filed
unmarked rather than lost, and triage searches the body for the block as well as the backlog for the mark. The block is
the contract either way, which is why a platform calling a label something else costs nothing: an Azure tag and a
GitHub label mean the same thing, and the shape in the body is what both of them point at.

A deviation request uses the same mechanism to ask a different question. A finding says the corpus is wrong. A request
says the rule is right and the work is about to break it, so somebody has to accept that risk. One skill doing both
would trigger at one of those moments and be wrong at the other, because a finding is filed after the fact and a
request has to arrive before the work merges.

`owner` is what a request exists for, and the one key it must not write. No export includes a record's `owner`, so the
individual with the authority to accept a risk cannot be found from an installed plugin. `eng:pol-DEVI.OWNER` requires
that person to be named, and an agent proposing one reads as a decision somebody already took. So the block states
everything the record will need, leaves `owner` and `accepted-on` to the reply, and the skill says the record is still
owed.

Both skills write the platform mechanics out in full, and neither cites the other. A session loads one skill, and the
bundle that kept `raise-finding` may have dropped the other, so a citation across would sometimes point at a file the
reader does not have. The cost is that a mistake in one of them has to be corrected twice.

A skill may still send a reader to one the bundle can trim, and `raise-finding` sends one to `fix-lookup` before it
files anything about a problem. What the rule above refuses is the silent version. The citation says which type the
absent skill needed, so a reader who does not have it reads that as an answer instead of a broken instruction.

The organisation boundary is the one rule here that no clause states. A deviation register is what one organisation
keeps about itself, and a request names the rule being broken, the service it is in and how long the gap lasts. Filed
on a public corpus's repository, that is an organisation's engineering published to anyone watching, and the maintainer
there could not accept the risk anyway. So the skill reads the owner segment of both `publishing` addresses and stops
where they differ.

Triage is the other half of the round trip, and it needs two marks rather than one. `kac:finding` says what the ticket
is, which never changes. `kac:triaged` and a route say what was decided, which does. Keeping them apart makes the queue
a query anybody can run, gives "I could not tell" a mark of its own, and leaves every finding ever filed one query away.
A skill that swapped `kac:finding` for a route would lose the backlog the first time it ran.

Routing is the work, and drafting is the small part of it. Reading the fourteen findings open when this rule was
written found nine reporting the framework with no record behind them, four asking for an edit to a record that already
existed, and one asking for a new `fix`. Two of the fourteen were halves of one problem, and one had already been
closed as a duplicate of another. So a run spends its time telling findings apart, and rarely writes anything.

Triage does not check whether a finding is true, for the reason `eng:pol-AGNT.SELFVER` gives. The evidence for most
findings is one session's account of its own work, and reading fourteen of those turns one run into fourteen
investigations that end in nothing anybody can rely on. The claim is settled at the pull request instead, where
`eng:pol-AGNT.ACCEPT` puts a person's name on it and `eng:pol-AGNT.EQUAL` sends it through the same review as any other
change. That is also why a drafted record takes whichever status its type treats as unsettled: it has been verified by
nobody, and `fixes` requires `verified` on every status but `draft`. `owner` is the field that cannot be drafted at
all. Every type requires it, so a record without one fails `kac validate`, and it names the individual accountable for
the record, which is the one thing an agent may not decide. So the skill asks the person running it and writes what
they say.

Triage is the one skill here that writes to a tracker, so it is the one that needs a boundary the export cannot give
it. A lookup answers from the copy beside it and writes nothing. `raise-finding` writes one issue and asks first.
Triage writes marks and comments across a whole backlog, and in a consumer's installed plugin `tracker.base` addresses
the backlog of whoever published the corpus. A consumer session running it would triage a stranger's repository. The
checkout is the only signal available: a session holding the repository the `publishing` block addresses is the corpus's
own maintainer, and every other session is not. `request-deviation` draws the same boundary by comparing two addresses,
and has two to compare.

The verdict is a mark, and the reason is a comment. A route stated twice is two things that can disagree, so the
comment argues and states no data. It is also the only part a person can push back on, which a mark on its own gives
them no way to do.

A framework finding is the one thing here that may leave the organisation, and `request-deviation` refuses the same
move. The difference is the subject. A deviation names the filer's own service, their rule and how long they intend to
break it, so publishing it harms them. A framework finding describes the tool, and the maintainer of that tool is the
only person who can act on it. So it travels, and two conditions make that safe: a person reads every word, and the
body identifies neither the filer nor their corpus. Stripping that is not a departure from
`eng:pol-AGNT.PROV`. A session id means nothing on a stranger's tracker, the framework version is the provenance that
does mean something there, and the person who copies the body is the one who files it.

Comparing `id` rather than a URL is why the export normalises the pair. `github:github.com/Example/Repo` and
`https://github.com/example/repo.git` are one backlog written two ways, and a skill parsing either would read them as
two and copy a finding onto the tracker it was already filed on.

CI checks little of this. `round-trip.sh` installs the plugin, asks each skill the question that skill describes, and
greps each `SKILL.md` for the parts file its component requires. Everything else above is a reviewer's, and two gaps
are worth naming. An undeclared component directory travels undeclared, because `bundle.json` lists what was declared
and nothing looks for what was not. Nothing here reads a filed finding, request or verdict. All three arrive on a
repository this build never opens, so their shapes survive only as long as the skills writing them follow this
standard.

## Sources and further reading

- **Normative.** [Claude Code plugin manifest] is the schema `plugin.json` is validated against, and `metadata` is
  where this repository's own keys sit inside it.

## Changelog

- 2026-09-15: added the triage marks a filed finding is routed by, the checkout triage runs from, and the conditions a
  framework finding travels under. The label is what triage selects on, and the block in the body is what finds a
  finding the label never reached.
- 2026-09-13: a finding states the observation and proposes no record. It drops the `id` and `expires` a discovery
  needed, and takes `looks-like` for the type it resembles.
- 2026-09-13: a skill names the review date under the key its own type's export writes, and one skill may cite another
  the bundle can trim, so long as it says what the absence means.
- 2026-09-13: the staleness rules read the type rather than a fixed list. A skill reports every `status` its own type
  declares, and reports `reviewBy` only where that type exports one.
- 2026-09-09: a skill leaves the choice of search tool to the session, and names no tool of its own.
- 2026-09-08: added the shape a deviation request carries, and the boundary that keeps one inside its own
  organisation.
- 2026-09-08: a component reading no export says whether it supports the others, so one serving the reader
  survives a bundle that trimmed every lookup.
- 2026-09-08: added the shape a finding an agent files carries, and the label duty beside it.
- 2026-09-07: initial version.

[Claude Code plugin manifest]: https://json.schemastore.org/claude-code-plugin-manifest.json
