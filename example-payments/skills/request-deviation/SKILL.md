---
name: request-deviation
description: Ask the owner of a clause to accept a knowing departure from it, as an issue on the repository that
  publishes the corpus holding that clause. Use when the work in front of you is about to break a rule this corpus
  states, and when someone says "request a deviation", "we need an exception" or "ask for a waiver". Use it as well,
  unprompted, whenever a design you are about to propose departs from a clause a lookup just gave you. A deviation is
  accepted by a named individual with the authority to accept the risk, and nothing in this session can name them.
---

# Asking the owner of a clause to accept a departure

A clause beside you says what has to happen, and the work in front of you is about to do something else. A departure
somebody decided on, wrote down and gave a review date is managed risk. The same departure taken quietly is erosion, and
a year later nobody can tell it from never having known the rule.

Accepting the risk takes an individual with the authority to accept it. You cannot name that person from here. A
record's `owner` does not travel in the export, and the copy beside you is frozen anyway. So the request leaves as an
issue on the repository that published the corpus owning the clause.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json   # which corpus this is, what it consumes, and where each one publishes
```

Use that path exactly as it appears; it is already absolute. An installed plugin sits in a cache of its own rather than
in the repository you are working in.

**Producing the body needs nothing installed.** Filing it needs whichever client already signs in to the platform.
Where that client is missing, the body is still the valuable part, so this skill prints it rather than giving up.

## Never file one unasked

**Show the finished body and wait for a yes.** Every time, including when somebody asked you to request a deviation.
This one says out loud that your organisation is about to break a rule, in which service, and how long for. It goes out
under their name.

**A silent no is a no.** Where nobody answers, print the body and stop. Do not file it and mention it afterwards.

**One deviation, one issue.** A departure from two clauses is one request where the same work and the same reason cover
both, and `departs-from` takes them both. Two pieces of work are two requests.

## Decide whether this is a deviation

**A deviation is a departure you have decided on and not yet taken.** The work is designed, the clause says otherwise,
and somebody has to say yes before it lands. An incident that left no time for the question is the one case where the
request follows the departure.

Four things are not deviation requests, and each goes somewhere else.

* **A clause you think is wrong** is a finding, and `raise-finding` is the skill for it. A finding says you noticed
  something. This asks somebody to carry a risk, and they are different questions to put to the same people.
* **A clause you have misread** is neither. Ask `standards-lookup` or `policy-lookup` for the wording again, and read
  what it actually obliges.
* **A shortcut that breaks no clause** is technical debt. It is still recorded, dated and owned, and the record takes
  `none` in `departs-from`. There is no clause owner to ask, so nothing is filed from here.
* **Work already inside the clause** is nothing at all. A request to depart from a rule you are keeping wastes the
  reader and teaches them to skim the next one.

## Read the clause before you ask

**Quote the clause in the request, in the words the export carried.** Ask `standards-lookup` or `policy-lookup` for it.
A request that paraphrases the rule is one the owner has to go and check before they can answer.

**Read the record's own exceptions.** A policy export carries its `Exceptions` section whole, and a standard's rule
says in its own words where it admits a departure. Some commitments admit none, and the record names them. Where the
clause you are about to break is one of those, the answer is no before anybody reads the request. Say that, and stop.

**Where the export carries neither policies nor standards, say so in the request.** `types` in `manifest.json` lists
what travelled. A clause id that reached you from outside the export is one whose wording and exceptions you cannot
check from here, and an owner reading that knows to check them instead.

## Pick the corpus that owns the clause

`manifest.json` describes this corpus at the top level and every corpus it consumes under `sources`. Each carries its
own `publishing` block.

| Field                  | Type            | What it holds                                                          |
|------------------------|-----------------|------------------------------------------------------------------------|
| `corpus`               | string          | the name of this corpus, as `example-payments`                         |
| `shortcode`            | string or null  | this corpus's own prefix                                               |
| `commit`               | string          | the commit this export was taken at                                    |
| `publishing`           | object          | where this corpus publishes                                            |
| `publishing.base`      | string or null  | the address to file against                                            |
| `publishing.target`    | string          | one of `github`, `azure-devops`, `azure-devops-wiki`, `mkdocs`, `none` |
| `sources`              | list of objects | one entry per corpus this one consumes                                 |
| `sources[].corpus`     | string          | that corpus's name                                                     |
| `sources[].shortcode`  | string          | the prefix its records carry, as the `eng` in `eng:pol-DEVI.OWNER`     |
| `sources[].publishing` | object or null  | where that corpus publishes, with the same keys as the block above     |

**The shortcode on the clause id names the corpus that owns it.** `eng:pol-TRUS.SCREEN` belongs to the `sources` entry
whose `shortcode` is `eng`, and the request goes to the repository that entry's `publishing.base` names. A clause id
carrying no shortcode belongs to the top-level corpus, and the request goes there.

**A bare record id is not a clause id.** `pol-TRUS` names a whole policy, and a departure from every clause it carries
is not what you are asking for. Name the clause, as `pol-TRUS.SCREEN`, and name each one where the work departs from
two.

**Where `manifest.json` is missing or will not parse, stop and say so.** The plugin is not assembled as it should be.
Print the body and ask whoever is with you where it belongs.

**A `publishing` block is always there, and its `base` may be present and `null`.** Test the value rather than the key.
A `base` of `null`, or a `target` of `none`, means that corpus publishes nowhere this export can address. Say so, print
the body, and ask whoever is with you who owns the clause. Do not invent a repository.

## Stay inside one organisation

A deviation register is what one organisation keeps about itself. A request filed outside it publishes your engineering:
which rule you are breaking, in which service, what you put in its place, and how long the gap stands. The maintainer
of a corpus anybody can install never agreed to hold your risk, and cannot accept it on your behalf.

**Compare the owning corpus's `base` with the top-level corpus's `base`.** The same repository is the same people, and
the request goes there. A different repository under the same account or organisation is still inside, so compare the
segment naming the account. On a GitHub address that is the segment after the host, and on an Azure DevOps address it is
the `<org>`. Two different platforms share no such segment, so treat them as two organisations unless somebody tells you
otherwise.

**Where the owner differs, or where you cannot tell, print the body and stop.** Ask whoever is with you who accepts
this risk inside your own organisation. Never file it on a stranger's repository to find out.

**Where either `base` is `null`, there is nothing to compare.** A corpus published nowhere says nothing about who owns
it, so the comparison cannot answer. Print the body and ask.

**Consuming a public corpus is what this is for.** A corpus published by somebody you have no relationship with states
rules you chose to take on, and a departure from one of them is yours to own. Record it at home.

## Write the body

The body opens with a fenced block a person can read and a later run can copy. Then the four headings a deviation takes,
in the order the type gives them, and `## Who is asking` last.

`````
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

## What we are doing instead

`eng:pol-TRUS.SCREEN` obliges us to screen a component for known vulnerabilities before we adopt it. The
reconciliation client publishes no SBOM, and our scanner reports nothing for it either way. We read its
dependency tree by hand and found nothing known against any of them.

## Why we need it

The settlement file format changes on 1 October, and this client is the only implementation of it. Writing
our own costs three weeks, and the alternative is a month of reconciliation done by hand.

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

**The title is the departure in one line.** Write the sentence somebody scanning a list of issues can act on. "A
deviation request for the payments API" names nobody's rule and no piece of work.

Each key of the block, and what to put in it:

* **`corpus`** is the top-level `corpus` from `manifest.json`. It is the corpus that will hold the record, which is the
  one departing rather than the one that wrote the rule.
* **`id`** is `dev-` and a short slug of the title. You cannot see what ids that corpus already holds, so treat this as
  a suggestion. Whoever writes the record settles it.
* **`status`** is `draft`. Nobody has accepted this yet, and that is the whole reason the issue exists.
* **`departs-from`** is the clause ids, each one scoped exactly as you found it. A request names at least one, because
  a departure from no clause has nobody to ask.
* **`review-by`** is the day somebody looks at this again, quoted. Propose the earliest date the work makes possible,
  and say in `## How it closes` what you based it on. The owner may move it, and is the only one who can.
* **`applies-to`** and `tags` are optional, and go after `review-by` in that order. Nothing else belongs in the block.

**`owner` and `accepted-on` are the answer, so the block leaves them out.** The individual accepting the risk is what
the request asks for, and the day they accept it is the day they reply. Proposing a name here reads as a decision
somebody already took.

**`owner` is required whatever the status, so the record cannot land without one.** A draft deviation missing it fails
validation. The reply is what unblocks the record as well as the work, which is worth saying when you hand the request
over.

**Write nothing open-ended.** "Permanent", "indefinitely" and "until further notice" each describe a rule that needs
rewriting rather than a deviation, and a corpus validating the record warns on all three.

**Keep the four sections short, and write them as the record will read.** What happens instead, what it buys, what
makes the risk survivable, and what has to be true for it to end. Where you cannot fill one honestly, say that in it. A
departure with nothing compensating it is an unmanaged risk, and the owner has to know that before they accept it.

**`## Who is asking` names the agent, the session, the repository, the commit and the work.** Add `manifest.json`'s own
`commit`, because that says which export you read the clause from. **Name any of those you cannot reach**, rather than
leaving it out. The four sections above are what a deviation record takes, and this one is not, so whoever writes the
record keeps it in the issue.

## File it

**Use the client that already signs in to the platform.** Never ask for a credential, and never offer to edit the copy
under `${CLAUDE_PLUGIN_ROOT}`.

Write the body to a file first. Passing it inline turns every backtick and quote into a quoting problem, and the block
is full of both.

The section to follow is chosen by the block's `target`, every time. Read it from the `publishing` block of the corpus
that owns the clause, which may publish to a different platform from the one you are holding.

### GitHub

`target` is `github`, and `base` is the repository, as `https://github.com/<owner>/<repo>`.

```bash
gh issue create --repo <owner>/<repo> --title "<the title>" --body-file <path> --label kac:deviation
```

**A repository that holds no `kac:deviation` label refuses the whole command.** The error names the label. Run it again
with no `--label`, and say in your reply that you filed it unlabelled and why. Never let a missing label cost the
request, because the work is waiting on the answer.

**A command that fails on sign-in or on permission is not a failure to report.** `gh auth status` says whether you are
signed in to that host. A repository with issues turned off, or an account without the rights to open one, ends the same
way: print the body and say which of the two it was.

### Azure DevOps

`target` is `azure-devops` or `azure-devops-wiki`, and `base` carries the organisation and the project together, as
`https://dev.azure.com/<org>/<project>/_git/<repo>`. A wiki publishes from
`https://dev.azure.com/<org>/<project>/_wiki/wikis/<id>` instead, and the two segments you need sit in the same places.
`az` wants them apart: the organisation is `base` up to and including `<org>`, and the project is the segment after it.

```bash
az boards work-item create --org https://dev.azure.com/<org> --project <project> --type Issue --title "<the title>" --fields "System.Description=@<path>" "System.Tags=kac:deviation"
```

**A tag is what this platform calls a label**, so `System.Tags` is where the mark goes. Azure creates a tag it does not
already hold rather than refusing, so nothing is lost here. Where the command fails for any other reason, print the body
and say so.

### Where the platform runs no issue tracker

`target` of `mkdocs` or `none` names a corpus published as pages, or not published at all. There is nowhere to file.
Print the whole body, name the corpus that owns the clause, and ask whoever is with you who accepts this risk.

### Where no client is here

Print the whole body, name the repository it belongs on, and say plainly that you could not file it. Ask whoever is with
you to paste it. A request read out to somebody is worth more than one lost to a missing tool.

## Say what you did

Close by naming the issue you opened and its URL, or the repository the body still needs pasting on. Where you filed it
unlabelled, say so. Where you left something out of `## Who is asking`, say which.

**Say that nothing is accepted yet.** The issue is a question, and the work departs from the clause until an individual
with the authority answers it. Name what you are waiting for: a person, and a review date they are content with.

**Say that the record is still to write.** The answer gives `owner` and `accepted-on`, and the deviation then lives as a
record in the corpus that is departing. Filing the request does not put it there, and an issue nobody turns into a
record leaves the departure undocumented in the only place a reader looks.
