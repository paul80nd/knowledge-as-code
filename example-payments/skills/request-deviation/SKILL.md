---
name: request-deviation
description: Ask the owner of a clause to accept a knowing departure from it, as an issue on the tracker stated by the
  corpus holding that clause. Use when the work in front of you, or a design you are about to propose, departs from a
  clause this corpus states, and when someone asks for a deviation, an exception or a waiver.
---

# Asking the owner of a clause to accept a departure

A deviation is a departure from a clause that somebody with the authority accepted, wrote down and gave a review date.
That makes it managed risk. The same departure taken quietly is erosion, and a year later nobody can tell it from never
having known the rule. This skill writes the request that asks for the acceptance.

Accepting the risk takes an individual with the authority to accept it, and you cannot name that person from here. A
policy and a standard both leave `owner` behind when they travel, and the copy beside you is frozen. So the request
leaves as an issue on the repository that published the corpus owning the clause.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json   # which corpus this is, what it consumes, and where each one publishes
```

Use that path exactly as written. An installed plugin sits in a cache outside the repository you are working in.

Writing the body needs nothing installed. Filing it needs whichever client already signs in to the platform. Where
that client is missing, print the body, because the body is the valuable part.

The sections below run in order. Each one decides whether the next is needed.

## Ask before you file

**Show the finished body and wait for a yes**, every time, including when somebody asked you to request a deviation.
It says that your organisation is about to break a rule, in which service and for how long, and it goes out under their
name. Where nobody answers, print the body and stop.

**File one request per piece of work.** A departure from two clauses is one request where the same work and the same
reason cover both, and `departs-from` takes both. Two pieces of work are two requests.

## Decide that it is a deviation

A deviation is a departure you have decided on and not yet taken. The work is designed, the clause says otherwise, and
somebody has to say yes before it lands. An incident that left no time for the question is the one case where the
request follows the departure.

Five other things each have their own route:

* **A clause you think is wrong** is a finding, and `raise-finding` files it. A finding reports what you noticed. A
  request asks somebody to carry a risk.
* **A clause you may have misread** needs its wording again. Ask `standards-lookup` or `policy-lookup`, and read what it
  obliges.
* **A departure from a `SHOULD` or `SHOULD NOT` in a standard** needs nobody's acceptance. Say why in the change, and
  file nothing here.
* **A shortcut that breaks no clause** is technical debt. It is still recorded, dated and owned, with `none` in
  `departs-from`. No clause owner is there to ask, so file nothing here.
* **Work already inside the clause** needs nothing. A request to depart from a rule you are keeping teaches its reader
  to skim the next one.

## Read the clause before you ask

**Quote the clause in the words the export has.** Ask `standards-lookup` or `policy-lookup` for it, because an owner
reading a paraphrase has to go and check the rule before they can answer.

**Read the record's own exceptions.** A policy's export has its `Exceptions` section whole, and a standard's rule says
in its own words where it admits a departure. Some commitments admit none, and the record names them. Where yours is
one of those, the answer is no before anybody reads the request: say that, and stop.

**Say in the request where the export has neither policies nor standards.** `types` in `manifest.json` lists what
travelled. An owner reading a clause id you could not check from here knows to check its wording and exceptions.

## Pick the corpus that owns the clause

`manifest.json` states a tracker for this corpus's own records under `tracker`, and one for each corpus it consumes
under `sources`. A clause always belongs to a corpus, so the request goes to one of those trackers.

| Field                 | Type            | What it holds                                                      |
|-----------------------|-----------------|--------------------------------------------------------------------|
| `corpus`              | string          | the name of this corpus, as `example-payments`                     |
| `shortcode`           | string or null  | this corpus's own prefix                                           |
| `commit`              | string          | the commit this export was taken at                                |
| `tracker`             | object          | where work about this corpus's records is filed                    |
| `tracker.target`      | string          | one of `github`, `azure-devops`, `none`                            |
| `tracker.base`        | string or null  | the repository or project to file against                          |
| `tracker.area`        | string or null  | the area path inside an Azure DevOps project, separated by `/`     |
| `tracker.id`          | string or null  | the target and the base normalised, so two backlogs compare equal  |
| `sources`             | list of objects | one entry per corpus this one consumes                             |
| `sources[].corpus`    | string          | that corpus's name                                                 |
| `sources[].shortcode` | string          | the prefix its records carry, as the `eng` in `eng:pol-DEVI.OWNER` |
| `sources[].tracker`   | object          | where work about that corpus's records is filed, same keys         |

A tracker block is always there, and its `base` may be present and `null`. Test the value rather than the key.

* **The shortcode on the clause id names the owning corpus.** `eng:pol-TRUS.SCREEN` belongs to the `sources` entry
  whose `shortcode` is `eng`, and the request goes to that entry's tracker. A clause id with no shortcode goes to the
  top-level `tracker`.
* **Name each clause, as `pol-TRUS.SCREEN`.** A bare `pol-TRUS` names a whole policy, and asks to depart from every
  clause in it.
* **A `base` of `null`, or a `target` of `none`,** means nothing here addresses that backlog. Say so, print the body,
  and ask whoever is with you who owns the clause.
* **A missing or unreadable `manifest.json`** means the plugin is not assembled as it should be. Say so, print the
  body, and ask where it belongs.

Read the address from `tracker`, and leave `publishing` alone. `publishing.base` is where a record is read. On Azure
DevOps that is a repository inside a project, and the backlog is the project, so a request filed there files nowhere.

## Stay inside one organisation

A deviation register is what one organisation keeps about itself. A request filed outside it publishes your
engineering: which rule you are breaking, in which service, what replaces it, and how long the gap stands. The
maintainer of a corpus anybody can install never agreed to hold your risk, and cannot accept it for you.

**Compare the owning tracker's `id` with the top-level `tracker.id` first.** The same `id` is one backlog, so the
request goes there.

**Compare the account where the `id` values differ.** Two backlogs inside one organisation are ordinary. On a GitHub
address the account is the segment after the host, and on an Azure DevOps address it is the `<org>`. Two platforms
share no such segment, so treat them as two organisations unless somebody tells you otherwise.

**Print the body and stop where the account differs, or where you cannot tell.** Ask whoever is with you who accepts
this risk inside your own organisation. File only on a backlog your organisation holds.

**Print the body and ask where either `base` is `null`.** A corpus with no backlog says nothing about who owns it.

Consuming a public corpus is what this protects. A rule you chose to take on from a stranger's corpus is yours to own,
so record the departure at home.

## Write the body

The body opens with a fenced block a person can read and a later run can copy. Then the five headings a deviation
takes, in the order the type gives them, and `## Who is asking` last.

`````
Title: Adopt the reconciliation client before it has been screened for vulnerabilities

```yaml kac-deviation
corpus: example-payments
id: dev-unscreened-recon-client
status: draft
departs-from: [ eng:pol-TRUS.SCREEN ]
risk: medium
review-by: "2026-11-07"
applies-to: [ svc-payment-api ]
tags: [ dependencies, vulnerabilities ]
```

## What we are doing instead

`eng:pol-TRUS.SCREEN` obliges us to screen a component for known vulnerabilities before we adopt it. The
reconciliation client publishes no SBOM, and our scanner reports nothing for it either way. We read its
dependency tree by hand and found nothing known against any of them.

## Why we need it

The settlement file format changes on 1 October, and this client is the only implementation of it. Writing
our own costs three weeks, and the alternative is a month of reconciliation done by hand.

## What the risk is

A vulnerability in the client or its tree is live in the settlement path until the weekly re-read finds
it. The container reaches no card data, so what an attacker gets is the settlement file and the run.

## What compensates

The client runs in its own container with no outbound network and no access to the card token store. A
weekly job re-reads its dependency tree against the advisory feed, and the on-call alert names this
deviation.

## How it closes

The vendor has committed to publishing an SBOM with their November release. The review date is the week
after it. Where it does not arrive, the choice is to re-accept the risk or to write our own client.

## Who is asking

Claude Code, in session 01J8ZC4M6QK2XR7VN0PYWTB3AE, working in acme/payments at 4f1c9ad, designing the
settlement import. The export beside it was taken at 89d6f54.
`````

**Write the title as the departure in one line**, naming the work and the rule, so somebody scanning a list of issues
can act on it. "A deviation request for the payments API" names nobody's rule and no piece of work.

Each key of the block, in this order:

* **`corpus`** is the top-level `corpus` from `manifest.json`: the corpus that will hold the record, which is the one
  departing.
* **`id`** is `dev-` and a short slug of the title. You cannot see the ids that corpus already has, so whoever writes
  the record settles it.
* **`status`** is `draft`, because nobody has accepted this yet.
* **`departs-from`** is the clause ids, each scoped exactly as you found it. A request names at least one, because a
  departure from no clause has nobody to ask.
* **`risk`** is `high`, `medium` or `low`: how bad the risk is once what compensates is working. Rate it from
  `## What the risk is`, and expect the owner to move it.
* **`review-by`** is the day somebody looks at this again, quoted. Propose the earliest date the work makes possible,
  and say in `## How it closes` what you based it on. A `high` rating brings that date inside six months, which the
  corpus warns on.
* **`applies-to`** and `tags` are optional, and go after `review-by` in that order. The block takes no other key.

**Leave `owner` and `accepted-on` to the reply.** The individual accepting the risk is what the request asks for, and
the day they accept is the day they reply. A name proposed here reads as a decision somebody already took. `owner` is
required on every deviation record, so the reply unblocks the record as well as the work. Say so when you hand the
request over.

**Give every departure an end.** "Permanent", "indefinitely" and "until further notice" each describe a rule that
needs rewriting, and a corpus validating the record warns on all three.

**Keep the five sections short, and write them as the record will read**: what happens instead, what it buys, what can
still go wrong, what makes that survivable, and what has to be true for it to end. Where you cannot fill one honestly,
say so in it. A departure with nothing compensating it is an unmanaged risk, and the owner has to know that first.

**`## Who is asking` names the agent, the session, the repository, its commit and the work.** Add `manifest.json`'s
own `commit`, because that says which export you read the clause from. Name any of those you cannot reach, so a
reviewer can tell a session with no id from one that did not say. A deviation record has no such section, so whoever
writes the record keeps it in the issue.

## File it

Use the client that already signs in to the platform, with the credential it already has. Ask nobody for a credential.
Leave the copy under `${CLAUDE_PLUGIN_ROOT}` as it is, and offer no edit to it, because a change there reaches nobody.

Write the body to a file first, because passing it inline turns every backtick and quote in the block into a quoting
problem.

Choose the section below by the `target` of the tracker of the corpus owning the clause, which may be on a different
platform from the one you are holding.

### GitHub

`target` is `github`, and `base` is the repository, as `https://github.com/<owner>/<repo>`.

```bash
gh issue create --repo <owner>/<repo> --title "<the title>" --body-file <path> --label kac:deviation
```

**Run it again with no `--label` where the repository has no `kac:deviation` label.** The error names the label. Say
in your reply that you filed it unlabelled and why, because the work is waiting on the answer.

**A failure on sign-in or on permission ends with the body printed.** `gh auth status` says whether you are signed in
to that host. A repository with issues turned off, or an account without the rights to open one, ends the same way.
Say which it was.

### Azure DevOps

`target` is `azure-devops`, and `base` is the project holding the backlog, as `https://dev.azure.com/<org>/<project>`.
`az` takes the two segments apart: the organisation follows the host, and the project follows the organisation.

**Decode the project name before you pass it**, because `base` arrives percent-encoded. A base ending
`Engineering%20Standards` names the project `Engineering Standards`.

```bash
az boards work-item create --org https://dev.azure.com/<org> --project <project> --type Issue --title "<the title>" --fields "System.Description=@<path>" "System.Tags=kac:deviation"
```

**A tag is what this platform calls a label**, so `System.Tags` takes the mark. Azure creates a tag it does not
already have. Where the command fails for any other reason, print the body and say so.

#### The area path

**`area` says which area path inside the project the item belongs to**, because one project can give one backlog to
any number of corpora.

Where the tracker states one, add `System.AreaPath` to `--fields` as the project and the area joined by a backslash.
Azure Boards separates an area path with backslashes, and `area` arrives with forward slashes, so convert every one:
an `area` of `kac-it-swdev/subteam` goes in as `<project>\kac-it-swdev\subteam`.

```bash
az boards work-item create --org https://dev.azure.com/<org> --project <project> --type Issue --title "<the title>" --fields "System.Description=@<path>" "System.Tags=kac:deviation" "System.AreaPath=<project>\<area>"
```

**Leave the field out where `area` is null.** The item then lands in the project's default area.

**Run it again without `System.AreaPath` where the area is refused**, because it does not exist or you cannot file
into it. Say in your reply that the item landed in the project's default area, and why.

### Where the tracker files nowhere

A `target` of `none` names a corpus with no backlog this export can address. Print the whole body, name the corpus
that owns the clause, and ask whoever is with you who accepts this risk.

### Where no client is here

Print the whole body, name the repository it belongs on, and say that you could not file it. Ask whoever is with you
to paste it, because a request read out to somebody still reaches the owner.

## Say what you did

Name the issue you opened and its URL, or the repository the body still needs pasting on. Say where you filed it
unlabelled or in the project's default area, and which parts of `## Who is asking` you could not reach.

**Say that nothing is accepted yet.** The work departs from the clause until an individual with the authority answers.
Name what you are waiting for: a person, and a review date they are content with.

**Say that the record is still owed.** The answer gives `owner` and `accepted-on`, and the deviation then lives as a
record in the corpus that is departing. An issue nobody turns into a record leaves the departure undocumented in the
only place a reader looks.
