---
id: std-PLUGIN
type: standard
tier: normative
status: active
implements: [ eng:pol-AGNT.ACCEPT, eng:pol-AGNT.ACCESS, eng:pol-AGNT.CONFID, eng:pol-AGNT.EQUAL,
  eng:pol-AGNT.PROV, eng:pol-AGNT.SELFVER, eng:pol-AGNT.UNPROV, eng:pol-DEVI.CONTENT, eng:pol-DEVI.EXPIRY,
  eng:pol-DEVI.OWNER, eng:pol-DEVI.PERM, eng:pol-DEVI.RECORD, eng:pol-DEVI.SURFACE, eng:pol-KNOW.COPY ]
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
`template/.plugin/` contains those skills, `plugin.json` says which of them travel, and `kac bundle` assembles both.
`kac bundle` also generates each view `plugin.json` declares: a file shaped for one moment of the work, written from the
export, such as the index of rules a session reads before it writes code. A hook pushes that index once to each agent
in a session, and again after a compaction. A skill answers from the export beside it, using no more than a session's
ability to read a file, and states plainly what did not travel. The reviewer skill reads the review view whole, in a
subagent of its own, and reads the change it
reviews with whatever the session has. The copy a skill reads cannot be changed, so a skill with something to send
back raises an issue on the repository the export came from: a finding where the corpus is wrong, and a request where
the work is about to depart from a clause that is right. It files with `gh` or `az`, and hands the reader the body
where neither can run. Back at that repository, a skill in the corpus's own working tree triages those issues, marks
each one with the route it belongs on, and drafts the record a routed finding asks for as a pull request. The record
cites the issue it came from, and the pull request cites the record. Merging it closes the issue. That skill ships in
no plugin, because it writes to the tracker and to the records.

## Rules

### REASON: A skill states what it is for, and the reason for each rule it gives

- A skill **SHOULD** state what it is for, the intent of the type it reads, and the reason behind each rule it gives.
- A skill **SHOULD** set out steps only where the order is the content: a record of a type an agent follows, or a
  write-back or review whose order a rule fixes.
- A skill's description **SHOULD** name each case that triggers it, and each case once.

### READS: A skill reading an export only reads files, and one that calls `gh` or `az` says what it does without them

- A skill **MUST** leave the choice of search tool to the session.
- A skill **MUST** name the file or the directory containing the answer.
- A skill reviewing a change **MUST** name the change as everything that differs from a fixed point.
- A skill reviewing a change **MUST** leave the choice of how to read that change to the session.
- A skill reading an export **MUST NOT** require a shell, an interpreter or a runtime on the reader's machine, beyond
  what the session uses to read a change under review.
- A skill reading an export **MUST NOT** require a network call to answer from it.
- A skill reading an export **MUST NOT** ask for access beyond reading the files under `${CLAUDE_PLUGIN_ROOT}` and the
  change under review.
- Every path such a skill names **MUST** open on `${CLAUDE_PLUGIN_ROOT}`.
- A skill that files an issue or fetches a record's published source **MAY** run `gh` or `az` to make that call, even
  where it also reads an export.
- A skill that files an issue or fetches a record's published source **MUST** say what it hands the reader where
  neither `gh` nor `az` can run.

_**Covers:** `eng:pol-AGNT.ACCESS`_

### TYPES: A skill names the type of every field it describes

- A skill describing a field **MUST** name that field's JSON type.
- A skill **MUST** say that a field with no value is present and `null`, where the export writes it that way.
- A skill **MUST** name each key that is absent rather than `null`.
- A skill **MUST NOT** describe a string of markdown as a list.
- Every field description **MUST** agree with a line of a real export.
- A skill **MUST** name the parts file of the type its component declares, and no other type's.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-AGNT.UNPROV`_

### GAPS: A skill says what its corpus did not carry

- A skill **MUST** read `types` in `manifest.json` before it says what is missing.
- A skill **MUST** name what stayed behind, and send the reader to the published record for it.
- A skill **MUST NOT** name a fixed set of absences.
- A skill **MUST** name the corpus and every entry in `sources` it searched, where nothing matched.
- A skill **MUST** say that an empty search states only that the estate has not written this down.
- A skill **MUST NOT** read an obligation out of a record about something else.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-AGNT.UNPROV`_

### ROUTER: A router sends a known need to its lookup, and reads any other type from a file it loads when routed there

- A router **MUST** send a question whose type it can name to the lookup skill reading that type, where that skill
  shipped.
- Where that lookup did not ship, a router **MUST** say so and name the type it needed.
- A router **MUST** read a type that no lookup skill reads from a reference file beside its `SKILL.md`.
- A router **MUST** read a reference file only when a question is routed to it, because a file costs a session nothing
  until it is read.
- A router **MUST** declare each type a reference file reads in `optional`, at the shape that file reads.
- A router answering from a report **MUST** compare each version in the report's `sources` with `manifest.json`.
- A router answering from a report **MUST** fetch the report's body through `corpus-retrieval`, and answer from it.
- A router answering from a report **MUST NOT** rebuild the report's answer from the records of other types.
- Where the body cannot be fetched, a router **MUST** give the report's title, its `generated.at` and its link.
- Where the body cannot be fetched, a router **MUST NOT** state a limit of the report.
- A router **MUST** send a session whose change leaves a record describing something no longer true to
  `raise-finding`, naming that record.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-AGNT.UNPROV`_

### DECLARE: `plugin.json` declares every component and the types it reads

- Every skill directory and hook directory in the plugin tree **MUST** appear under `metadata.components`.
- A component **MUST** name every record type it opens a file of, at the shape version it reads, as `glossary@1`: in
  `requires` where it cannot work without the type, and in `optional` where it can.
- A view **MUST** be declared as a component whose `view` names it and whose `path` is where `kac bundle` writes it.
- A component that needs a type present and opens none of its files **MUST** name that type bare.
- A component that reads no export at all **MUST** declare `requires` empty.
- A component that reads no export and supports no other component **MUST** declare `standalone` true.
- A component entry **MUST** have a `note` saying what it reads.
- `metadata.corpusRoot` **MUST** name the directory every skill in the tree addresses the export through.
- A component naming a type the export does not include **MUST** leave the plugin, with every file under its declared
  path.
- `bundle.json` **MUST** name every component that left, and the type that left it out.

_**Covers:** `eng:pol-AGNT.UNPROV`_

### FROZEN: The bundle carries a frozen export, and nothing writes to it

- The export inside a plugin **MUST** be the bytes `kac export` wrote.
- A skill **MUST NOT** write to any file under `${CLAUDE_PLUGIN_ROOT}`.
- A skill **MUST** report a `status` its type does not treat as in force, under every such value that type declares.
- A skill **MUST** report a review date that has passed, where its type exports one.
- A skill **MUST** name that date under the key its type's export writes: `reviewBy` on a part line, and
  `review-by` from a record's frontmatter.
- A skill **MUST** quote `generatedAt` from `manifest.json` alongside either.
- A skill **MUST** quote the wording the export contains, and link the record for the rest.
- The words a skill quotes **MUST** come from the export, and never from a second copy kept for agents.
- A view **MUST** be generated by `kac bundle` from the export the plugin contains, and from nothing else.
- A fresh bundle of the same export **MUST** write every view again, byte for byte.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-KNOW.COPY`_

### HOOK: A hook runs in a POSIX shell, and pushes a view once to each agent

- A hook **MUST** run under a POSIX shell.
- A hook **MUST NOT** require a parser, an interpreter or a runtime beyond that shell.
- What a hook prints **MUST** be rendered by `kac bundle`, so the hook computes nothing about the export.
- A hook **MUST** write only under `${CLAUDE_PLUGIN_DATA}`, or under the temporary directory where that is unset.
- A hook pushing a view **MUST** push it at most once to each agent between compactions, the main session and each
  subagent alike.
- A hook pushing a view **MUST** push it again to each agent after a compaction, because the summary can drop it.
- What a hook pushes **MUST** be at most 10,000 characters, because Claude Code shows a session only the first 2,000 of
  a longer one.
- Where a view is longer than that, the hook **MUST** push the view's path and counts in its place.
- What a hook pushes **SHOULD** state facts, because Claude Code passes it to the session as context.

_**Covers:** `eng:pol-AGNT.ACCESS`_

### TRACKER: A skill writes back to the tracker the corpus states

- A skill **MUST** send a correction to the backlog the record's corpus addresses under `tracker`.
- A skill **MUST NOT** file a correction on the address a `publishing` block states, because a published form and a
  backlog are one address on GitHub and two on Azure DevOps, where the project holds the backlog and the repository
  inside it holds none.
- A skill **MUST** file the item under the area path that `tracker` block states.
- Where that area path is refused, a skill **MUST** file the item at the project default and say so.
- Every corpus sharing one Azure DevOps project with another corpus **MUST** state an area under `tracker`, because
  triage selects its queue by area.
- A skill **MUST** raise it as an issue or a work item.
- A skill **SHOULD** use the client that already authenticates to that platform.
- A skill **MUST NOT** offer to edit the installed copy.
- A skill **MUST NOT** ask the reader for a credential.

_**Covers:** `eng:pol-AGNT.ACCESS`, `eng:pol-KNOW.COPY`_

### FINDING: A finding an agent files is an observation, and proposes no record

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
- The title **SHOULD** be the observation in one line.
- A skill **MUST** mark the issue `kac:finding`, using whatever the platform calls a label.
- Where the platform refuses a mark it does not already have, a skill **MUST** file the finding unmarked.
- A skill **MUST** say that it filed one unmarked.

_**Covers:** `eng:pol-AGNT.CONFID`, `eng:pol-AGNT.PROV`, `eng:pol-AGNT.SELFVER`_

### DEVREQ: A deviation request an agent files asks for the owner it cannot name

- A deviation request **MUST** open its body with a fenced block whose info string is `yaml kac-deviation`.
- That block **MUST** open on `corpus`, naming the corpus that will contain the record.
- The block **MUST** then contain `id`, `status`, `departs-from`, `risk` and `review-by`, under the names and in the
  order the `deviations` template writes them.
- The block **MAY** contain `applies-to` and `tags` after those.
- The block **MUST NOT** contain any other key.
- `status` **MUST** be `draft`.
- Every entry in `departs-from` **MUST** name a clause, scoped exactly as the export writes that record's id.
- An entry **MUST NOT** name a whole policy or standard.
- A skill **MUST NOT** write `owner` or `accepted-on`, because the individual who accepts the risk is what the request
  asks for.
- A request **MUST NOT** describe the departure as permanent, indefinite, or standing until further notice.
- The body **MUST** contain `## What we are doing instead`, `## Why we need it`, `## What the risk is`,
  `## What compensates` and `## How it closes`, in that order.
- `## Who is asking` **MUST** follow those five.
- `## Who is asking` **MUST** name the agent, the session it ran in, the repository, the commit it read, and what it was
  doing.
- Where the agent cannot reach one of those, `## Who is asking` **MUST** name it as unreached.
- The title **SHOULD** be the departure in one line, naming the work and the rule it breaks.
- A skill **MUST** file the request on the backlog addressed under `tracker` by the corpus that owns the clause.
- A skill **MUST NOT** file a request on a repository outside the organisation holding the plugin.
- A skill **MUST** mark the issue `kac:deviation`, using whatever the platform calls a label.
- Where the platform refuses a mark it does not already have, a skill **MUST** file the request unmarked.
- A skill **MUST** say that the request accepts nothing, and that the record is still owed.

_**Covers:** `eng:pol-AGNT.PROV`, `eng:pol-DEVI.CONTENT`, `eng:pol-DEVI.EXPIRY`, `eng:pol-DEVI.OWNER`,
`eng:pol-DEVI.PERM`, `eng:pol-DEVI.SURFACE`_

### KEYWORD: A skill reads a rule's keyword as what departing from it costs

- A skill reporting a rule **MUST** say that departing from a `MUST` or `MUST NOT` takes a deviation the rule's owner
  accepts.
- Where the bundle carries `request-deviation`, a skill **MUST** send that departure to it.
- A skill reporting a rule **MUST** say that a reader may depart from a `SHOULD` or `SHOULD NOT` by giving the reason in
  the change.
- A skill **MUST NOT** send a departure from a `SHOULD`, `SHOULD NOT`, `MAY` or `COULD` to `request-deviation`, because
  a reason given in the change conforms to the rule.
- A skill reviewing a change **MUST** report a breach of a `MUST` or `MUST NOT` as a violation, citing the rule's id.
- A skill reviewing a change **MUST** report a departure from a `SHOULD` or `SHOULD NOT` as a judgement call, citing
  the rule's id and quoting the reason the change gives.
- A skill reviewing a change **MUST** check every obligation itself, except one named in the What it checks section of
  a control at the status its type exports as `inForce`, whose mechanism is `ci` or `runtime-alert`.

_**Covers:** `eng:pol-DEVI.RECORD`_

### TRIAGE: Triage routes a finding with a label, and says why in a comment

- A skill triaging findings **MUST** travel into a corpus's working tree, and **MUST NOT** be a component of a
  plugin.
- A skill triaging findings **MUST** read the corpus it is about from the `.corpus.yaml` it walks up to.
- A skill triaging findings **MUST NOT** decide which corpus it is in from the `publishing` block, because a corpus
  published as a wiki or a site has no repository there and its maintainer still has both a tracker and the source.
- A skill **MUST** select its queue as `kac:finding` without `kac:triaged`.
- Where the corpus states an area, a skill **MUST** select its queue from that area alone.
- A skill **MUST** also find an open issue whose body has the `yaml kac-finding` block and no `kac:finding` mark.
- A skill **MUST** say where a query returned as many issues as its limit allows.
- A skill **MUST NOT** take `kac:finding` off an issue it triaged.
- A skill **MUST** apply `kac:triaged` and exactly one route mark to every issue it triaged, except a finding it
  bounced.
- Where a finding belongs in another area of the same project, a skill **MUST** bounce it: move it to that area by its
  area path, and comment why it moved and where.
- A skill **MUST NOT** apply `kac:triaged` or a route mark to a finding it bounced, because whoever triages that area
  reads it from their own queue.
- The route **MUST** be one of `kac:route-framework`, `kac:route-record`, `kac:route-misfiled`, `kac:route-none` and
  `kac:route-unclear`.
- A skill **MUST** apply `kac:route-unclear` wherever choosing between the others would be a guess.
- A skill **MUST** create every mark it applies before it applies one, on a platform that refuses an unknown mark.
- A skill **MUST** comment the reason for the verdict on the issue it marked.
- A comment routing a duplicate **MUST** name the issue it duplicates.
- A comment routing a finding a record already answers **MUST** name that record.
- The comment **MUST NOT** state the route as machine-readable data, because the mark is that data.
- A skill **MUST NOT** state whether the finding's claim is true.
- A skill **MUST** show every verdict to a person and wait, before it writes any mark or comment or bounces any
  finding.
- A skill drafting a record from a finding **MUST** take it only from an issue marked `kac:route-record`.
- That record **MUST** name an `owner` the person running the skill supplied, and the skill **MUST NOT** invent one.
- That record **MUST** carry a `status` its type treats as unsettled, wherever the type declares one.
- That record **MUST** arrive as a pull request, and the skill **MUST NOT** approve or merge it.
- That record **MUST** name the issue it was drafted from in `sources`, or in its changelog entry where its type
  refines `sources` into something else.
- Where the type takes neither, the pull request body **MUST** state that trace.
- That pull request **MUST** name the record's id.
- That pull request **MUST** link the issue, so that merging it settles the finding.
- A skill **MUST NOT** close the issue itself.
- A skill **MUST NOT** draft a record from an issue already closed, or from one a pull request already answers.

_**Covers:** `eng:pol-AGNT.ACCEPT`, `eng:pol-AGNT.EQUAL`, `eng:pol-AGNT.PROV`, `eng:pol-AGNT.SELFVER`,
`eng:pol-AGNT.UNPROV`_

### STRIP: A framework finding travels stripped, and only by hand

- A skill **MUST** compare the framework block's address with the tracker block's, both targets, both bases and both
  areas normalised.
- Where the two are equal, a skill **MUST** mark the finding and copy nothing.
- Where only the areas differ, a skill **MUST** bounce the finding to the framework's area, as `TRIAGE` states, and
  copy nothing.
- Where the targets or the bases differ, a skill **MUST** post the upstream body as a comment on the issue and stop.
- A skill **MUST NOT** file a finding on a tracker outside the organisation holding the corpus.
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

| Field         | Type                     | What it holds                                   |
|---------------|--------------------------|-------------------------------------------------|
| `obligations` | list of objects, or null | one entry per bullet under that heading         |
| `seeAlso`     | list of strings, or null | the rules of other standards this one points at |

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
names a tool the session may not have, so a reader without it must choose between ignoring the instruction and stopping.
The avoided table states no type at all, so a reader cannot tell whether to loop over the field or read it whole. It
says a field with no value is absent, so a reader testing for the key finds it every time and reads nothing.

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

- [ ] Every skill states what it is for and the intent of the type it reads, and gives the reason for each rule.
- [ ] Every numbered list in a skill is an order that the type it reads, or a rule here, fixes.
- [ ] Every skill's description names each case that triggers it, and none twice.
- [ ] Every instruction a skill reading an export gives can be followed by a session that can read a file and nothing
      else, apart from reading the change a review is asked about, and the `gh` or `az` call that files an issue or
      fetches a record's published source.
- [ ] Every skill that files an issue or fetches a record's published source says what it hands the reader where
      neither `gh` nor `az` can run.
- [ ] Every skill reviewing a change names it as what differs from a fixed point, and names no tool for reading it.
- [ ] Every path a skill names opens on `${CLAUDE_PLUGIN_ROOT}`.
- [ ] Every field a skill describes names its JSON type, and agrees with a line of a real export.
- [ ] Every skill names the parts file of the type it requires, and no other.
- [ ] Every skill says what its corpus did not include, from `types` in `manifest.json`.
- [ ] Every router sends a question it can place to the lookup for that type, and says which type is missing where
      the lookup did not ship.
- [ ] Every type a router reads with no lookup is read from a reference file it loads only when routed there, and is
      named in `optional` at that file's shape.
- [ ] Every answer a router gives from a report compares the report's `sources` with `manifest.json`, comes from the
      fetched body, and states no limit it could not read.
- [ ] Every router sends a change that makes a record wrong to `raise-finding`, naming the record.
- [ ] Every skill directory and hook directory in the plugin tree is declared under `metadata.components`.
- [ ] Every `requires` entry names a type at the shape version it reads, bare where it opens none of that type's
      files, and empty where the component reads no export.
- [ ] Every type a component reads only where the export carries it is named in `optional`, at the shape it reads.
- [ ] Every view is declared as a component naming it in `view`, and is written by `kac bundle` from the export alone.
- [ ] Every hook runs under a POSIX shell alone, prints a file `kac bundle` rendered, and writes only under
      `${CLAUDE_PLUGIN_DATA}`, or the temporary directory where that is unset.
- [ ] Every hook pushing a view pushes it at most once to each agent between compactions, and again after one.
- [ ] Every hook pushing a view pushes at most 10,000 characters, and the view's path and counts where the view is
      longer.
- [ ] Every component reading no export declares `standalone` where it supports nothing, and leaves it off where it
      supports the components that do.
- [ ] No skill offers to write to the export, and each names the issue tracker instead.
- [ ] Every skill that files reads the address from a `tracker` block, and none of them reads `publishing`.
- [ ] Every item filed on Azure DevOps carries the area path its `tracker` block states, or says it went to the
      project default.
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
- [ ] Every skill that reports a rule says a `MUST` departure takes a deviation, and a `SHOULD` departure a reason.
- [ ] No skill sends a departure from a `SHOULD`, `SHOULD NOT`, `MAY` or `COULD` to `request-deviation`.
- [ ] Every review files a broken `MUST` as a violation and a `SHOULD` departure as a judgement call, each under its
      rule's id.
- [ ] Every review checks each obligation that no control in force, with mechanism `ci` or `runtime-alert`, names.
- [ ] No finding is dropped, or held back, because the target has no `kac:finding` label.
- [ ] Triage named the corpus from the `corpus:` of the `.corpus.yaml` it walked up to, and never from the
      `publishing` block.
- [ ] No plugin declares a component that triages findings.
- [ ] The queue read both the marked findings and the open issues whose body carries the block without the mark.
- [ ] The queue read only the corpus's own area, where the corpus states one.
- [ ] Every triaged issue keeps `kac:finding`. One kept here gains `kac:triaged` and exactly one `kac:route-*` mark,
      and one bounced to another area gains neither and has a comment saying why and where.
- [ ] Every corpus sharing an Azure DevOps project with another corpus states an area.
- [ ] Every verdict was shown to a person and agreed before any mark, comment or bounce was written.
- [ ] Every verdict has a comment saying why, and none of them says whether the claim is true.
- [ ] No comment repeats its route as machine-readable data.
- [ ] Every record drafted from a finding came from a `kac:route-record` issue, and arrived as a pull request nobody
      approved.
- [ ] Every drafted record names an `owner` a person supplied, and carries the status its type treats as unsettled.
- [ ] Every drafted record names the issue it came from, or its pull request body does where the type takes neither a
      source nor a changelog.
- [ ] Every pull request for a drafted record names it, and closes its issue on merge.
- [ ] No skill closed a finding itself, and none drafted a record from a finding already closed or already answered by
      a pull request.
- [ ] No framework finding was filed outside the organisation holding the plugin.
- [ ] Every upstream body states the framework version and identifies neither the filer nor the corpus.

## Rationale and provenance

A skill is the one document here written for a reader nobody here can see. It runs in somebody else's session, on
somebody else's machine, against an export taken on an earlier day. Every rule above follows from one of those three
facts.

The session is why a skill names a property of the answer and not a tool. A session with no Grep tool met a
**MUST**-shaped instruction naming one, and its only options were to ignore the instruction or to stop. The machine is
why a skill reading an export asks for no shell. Each lookup skill promises at the top that there is nothing to install
and nothing to run, and a Windows reader with no `grep` is who that promise is for.

A skill that files an issue or fetches a record's published source cannot make that promise. Its work is a call to a
platform, and no file read makes one. So it runs `gh` or `az`, which a reader on that platform is usually signed in to
already. Where neither can run, it hands the reader the body to file by hand, or asks them to paste the record.

The session is also why a skill states its intent and the reason for each rule. A session meets cases the author never
saw, and a reason is what it can apply to the next one. A step covers only the case it was written for, so a skill
that missed a case gains a reason, or loses a step. Steps stay where the order is the content: a runbook, a fix, a
review, and a write-back whose order a rule above fixes. The description is the one part of a skill in every session's
context, so it names each case that triggers it once. [writing-for-agents] is the reference for that text. This
standard cites it and does not restate it, because it changes upstream.

A router serves a session that knows the corpus exists and has a question, but not which type answers it. Every
bundled skill puts its description in every session, so one router costs one description where a lookup for each rarely
asked type would cost one each. A known need still goes to its lookup, because the lookup states that type's fields and
reasons. A type with no lookup is read from a file beside the router, which costs nothing until a question is routed to
it, and a new type joins as one more file. A report is the quick route to a whole-corpus answer, because its value is
the verdicts and limits a person wrote, and a rebuild from the standards and controls has neither. Its body does not
travel, so the router fetches it, and where it cannot, it gives the report's place and leaves its limits unstated
rather than inferring them. Without the fetch it still answers from the export, because the index entry says which
report exists, how old it is and which versions it is true of.

A reviewer is the one skill whose input is not in the plugin. The change it reviews lives in the repository's history,
and every session reading that history already has a way to do it. So the skill names the change as what differs from
a fixed point, and leaves the session to read it, the same way a lookup leaves it the search.

A reviewer leaves a rule out only where a check runs without a person. `ci` and `runtime-alert` run by themselves.
`review-checklist` and `manual` are a person checking, and in a review that person is the reviewer. A control's
`verifies` names the rules it checks, and a rule can hold more obligations than the control watches. So the reviewer
reads its What it checks section to learn which obligations it covers, and checks the rest.

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
`docs/maintaining/plugin.md` describes how `kac bundle` acts on all of them.

`optional` is a fourth state, for a type a component reads where it is present and does without where it is not. The
review view lists the controls checking each rule, and a corpus exporting no controls still needs its rules reviewed.
So a missing optional type trims nothing, and one at another shape still stops the build.

A view is not a second copy kept for agents. A second copy is one somebody maintains beside the export, and it drifts
from the export as soon as either is edited. A view is generated from the export inside the same plugin, at the same
moment, by the same command, and a fresh bundle writes it again unchanged. So the words it quotes are the export's
words, and the rule against a second copy is met.

A hook runs on every machine that installs the plugin, with no session deciding to call it, so it asks that machine for
the least it can. Claude Code runs a hook under `sh`, Git Bash or PowerShell, so a POSIX script reaches the first two
with nothing installed. `kac bundle` renders the JSON a hook prints, and the script only cuts the session id out of its
input and prints a file. The plugin's own directory is replaced on every update, so the marker saying a session has
already seen the view goes in the data directory Claude Code keeps for the plugin. A view pushed on every call would
cost its length on every turn that followed, and a view over the cap reaches a session as a path and a fragment it is
not asked to read.

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

A drafted record names the issue it came from because a reviewer is being asked to accept something an agent wrote.
`eng:pol-AGNT.PROV` wants that review to be a check rather than an act of faith, and `eng:pol-AGNT.UNPROV` refuses a
proposal nobody can trace back to what produced it. The issue closing on merge is the same link from the other end.
It points at the pull request rather than the file, so that body names the record. The skill closes nothing itself,
because closing a finding would settle an observation nobody has accepted.

A pull request answers a finding from the moment it opens, and the issue stays open until somebody merges it. Reading
the state alone would leave that window, and a second run would write the record a reviewer is already holding. So the
check is the pull requests linked to the issue rather than whether the issue is closed. A `reports` record keeps its
trace in the pull request body alone: its `sources` states corpora and versions, and it has no changelog for an issue
to go in.

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
on a public corpus's tracker, that is an organisation's engineering published to anyone watching, and the maintainer
there could not accept the risk anyway. So the skill compares the two `id` values, and where they differ reads the owner
segment of both bases and stops.

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

Triage writes marks and comments across a whole backlog, so it is bounded by where it travels. A lookup answers from
the copy beside it and writes nothing. `raise-finding` writes one issue and asks first. In a consumer's installed
plugin `tracker.base` addresses the backlog of whoever published the corpus, so a consumer running triage would mark
up a stranger's repository. Shipping triage in a plugin at all is therefore the thing to refuse. It travels into the
corpus's working tree instead, where the only session that can load it is one holding the corpus's source, and that
session is the maintainer. `request-deviation` draws its boundary by comparing two addresses, because it does ship in
a plugin and has two to compare.

The corpus comes from `corpus:` and never from the publishing address, because a corpus files where its publishing
block does not point. That is also why a skill that files reads `tracker` and not `publishing`. `Tracker.cs` derives a
tracker from `publishing` only where that target has a backlog, and cuts an Azure DevOps base back to the project,
which is why the descriptor states `tracker:` separately at all. A corpus published as a wiki or a documentation site
has no repository in `publishing.base`, so a skill reading the corpus out of that block would refuse that corpus's own
maintainer for ever. `.corpus.yaml` is beside the records in every case.

An area path is stated rather than derived for the same reason the tracker is. One Azure DevOps project gives one
backlog to any number of corpora, and each publishes from a repository of its own, so no publishing block names the
area the project divides that backlog by. GitHub gives a repository one issue list and labels to sort it, so the key
means nothing there and `kac validate` refuses it. The same division bounds triage. A queue read across the whole
project would mark another corpus's findings, so triage reads its own area, and each corpus sharing a project states
one. Two corpora in one project share an `id`, because `id` leaves the area out, so triage compares the area beside
it. A finding that belongs in the other area is bounced there by its area path and keeps its history. It goes
unmarked, because a route mark is the verdict of the corpus that keeps a finding, and the corpus whose area it reaches
has not yet given one.

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
- **Normative.** [writing-for-agents], the `writing-for-agents` skill at v1.3.1, is the reference for how a skill and
  its description are written.

## Changelog

- 2026-10-28: triage reads its queue from the corpus's own area, and compares the area wherever it compares two
  addresses. A finding that belongs in another area of the same project is bounced there by its area path, with a
  comment and no triage mark. Every corpus sharing an Azure DevOps project states an area.
- 2026-10-27: a hook pushes a view once to each agent, the main session and each subagent alike, and again after a
  compaction.
- 2026-10-27: the bar on requiring a shell or a runtime binds a skill reading an export. A skill that files an issue or
  fetches a record's published source may run `gh` or `az` for that call, and says what it hands the reader where
  neither can run. `READS` takes a heading stating both.
- 2026-10-23: a review leaves to CI a control at the status its type exports as `inForce`, and no longer assumes
  `active`.
- 2026-10-20: a router sends a question it can place to the lookup for that type, and reads a type no lookup reads
  from a reference file it loads only when routed there. It answers a whole-corpus question from the report's fetched
  body, and sends a change that makes a record wrong to `raise-finding`.
- 2026-10-20: a skill states what it is for, the intent of the type it reads, and the reason for each rule it gives.
  It sets out steps only where the order is the content, and its description names each case that triggers it once.
- 2026-10-19: a skill reviewing a change names it as what differs from a fixed point, and leaves the session to read
  it. It reports a broken `MUST` as a violation and a `SHOULD` departure as a judgement call, and checks every
  obligation that no running `ci` or `runtime-alert` control names.
- 2026-10-19: a hook runs in a POSIX shell, prints what `kac bundle` rendered, writes only to the plugin's data
  directory or the temporary directory, and pushes a view once a session within the 10,000 characters Claude Code
  passes on.
- 2026-10-19: a view is declared as a component, and `kac bundle` generates it from the export alone, so it is not a
  second copy kept for agents. A component names a type it reads only where present in `optional`.
- 2026-10-18: using the client that already authenticates, and the one-line title of a finding and of a deviation
  request, become `SHOULD`. A skill still asks the reader for no credential, and nothing parses a title.
- 2026-10-18: a skill reads a rule's keyword as what departing from it costs. A `MUST` departure takes a deviation,
  and a `SHOULD` departure takes a reason given in the change, since a standard may now state either.
- 2026-09-23: a skill that files reads the address from a `tracker` block, and never from `publishing`. On Azure DevOps
  the two differ: the project holds the backlog and the repository inside it holds none. The item carries the area path
  that block states, and falls back to the project default when the area is refused.
- 2026-09-15: a record drafted from a finding cites that finding, and its pull request cites the record and closes
  the issue. A reviewer can check the record against the observation, and the finding points back at the record. No
  record is drafted from a finding a pull request already answers.
- 2026-09-15: triage travels into a corpus's working tree and ships in no plugin. It writes to the tracker and to the
  records, so the reader is the maintainer holding the source.
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

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[Claude Code plugin manifest]: https://json.schemastore.org/claude-code-plugin-manifest.json
[writing-for-agents]: https://github.com/mattpocock/skills/tree/v1.3.1/skills/productivity/writing-for-agents
