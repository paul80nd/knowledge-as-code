---
name: harvest-findings
description: Triage the findings filed against this corpus, and draft the record one of them asks for. Use when
  someone says "triage the findings", "harvest the findings", "what findings are open", "turn that finding into a
  record", or gives you an issue number and asks for the record behind it. Both invocations write to the corpus's own
  tracker, so both need a checkout of the repository that publishes it, and neither runs unasked. `raise-finding` files
  an observation and stops there. This is what moves it.
---

# Triaging findings, and drafting what they ask for

A finding is an observation an agent filed as an issue on the tracker. `raise-finding` writes it and stops. Nothing
after that decides where the observation belongs, so findings pile up and none of them becomes a record.

This skill has two invocations. Each is run by a person who agrees what it is about to do.

* **`triage`** reads every finding nobody has triaged, sorts each one into a route, and shows you a table. It writes
  nothing until you say yes. Then it labels each issue and comments the reason.
* **`draft <issue>`** takes one triaged finding and writes the record it asks for, as a pull request.

```text
${CLAUDE_PLUGIN_ROOT}/corpus/manifest.json   # which corpus this is, and where work about each one is filed
```

Use that path exactly as it appears. It is already absolute. An installed plugin sits in a cache of its own rather than
in the repository you are working in.

**Both invocations need this corpus checked out, and the client that already signs in to the tracker.** `draft` needs
that checkout to be the repository the records live in as well, which the section on it covers. Where a client is
missing, print what you produced and say which invocation you could not finish.

## Never write unasked

**Show every verdict and wait for a yes.** One reply covers the whole table, and a person who changes three rows has
answered for all of them. What you are about to write goes out under their name, onto a tracker other people read.

**A silent no is a no.** Where nobody answers, print the table and stop. Do not label anything and mention it
afterwards.

**Show the record before you commit it.** `draft` writes a file, a branch and a pull request. The person sees the record
first.

## Only the corpus's own maintainer runs this

Every other skill here answers from the export and writes nothing. This one writes to a tracker, so it has a boundary
the others do not need.

**Check that you are in a checkout of this corpus, before either invocation.** Walk up for a `.corpus.yaml` and read
its `corpus:`. It is a checkout of this corpus where that name equals `corpus` in `manifest.json`. An installed plugin
sits in a consumer's cache, and `tracker.base` there addresses the backlog of whoever published the corpus, so
labelling and commenting on it from a consumer's session is writing on somebody else's repository.

**Test the corpus, never the publishing address.** A corpus publishes where it is read and files where work is
tracked, and the two are different places. One published as a wiki or a documentation site has no repository in
`publishing.base` a checkout could ever match, and its maintainer still has a tracker and still has the source.

**Refuse where you find no `.corpus.yaml`, or where the one you find names another corpus.** Say which corpus this
plugin is for, and that triage belongs to whoever maintains it. A consumer with something to report uses
`raise-finding`, which files one issue and asks first.

**Refuse where you were not asked.** Neither invocation starts on its own.

## Triage does not decide whether a finding is true

A session's own account of its own work is not evidence. Checking every claim on a backlog turns one run into as many
investigations, and none of them ends in anything a person can rely on.

**Check two things, and neither is the claim.** Whether this finding says what another open one says, and whether a
record here already covers it.

**Whether the claim is true is settled at the pull request.** That is where a person takes accountability for what an
agent wrote. So a verdict says where the observation belongs, never whether it is right.

## Read the addresses

`manifest.json` describes this corpus at the top level and every corpus it consumes under `sources`. Three blocks say
where work is filed, and they are different addresses.

| Field                   | Type            | What it contains                                                  |
|-------------------------|-----------------|-------------------------------------------------------------------|
| `corpus`                | string          | the name of this corpus, as `example-payments`                    |
| `tracker`               | object          | where work about this corpus's own records is filed               |
| `tracker.target`        | string          | one of `github`, `azure-devops`, `none`                           |
| `tracker.base`          | string or null  | the backlog to file against                                       |
| `tracker.id`            | string or null  | that pair normalised, which is how you tell two backlogs apart    |
| `framework`             | object          | where a problem with `kac`, the schema or a skill is reported     |
| `framework.target`      | string          | the same three values                                             |
| `framework.base`        | string or null  | the framework's backlog                                           |
| `framework.id`          | string or null  | that pair normalised                                              |
| `publishing`            | object          | where this corpus is read, which is where a record is written     |
| `publishing.target`     | string          | one of `github`, `azure-devops`, `azure-devops-wiki`, `mkdocs`, `none` |
| `publishing.base`       | string or null  | the repository holding the records                                |
| `publishing.pathPrefix` | string or null  | the folder inside it the corpus sits in                           |
| `sources`               | list of objects | one entry per corpus this one consumes                            |
| `sources[].corpus`      | string          | that corpus's name                                                |
| `sources[].shortcode`   | string          | the prefix its records carry, as the `eng` in `eng:pol-AGNT.PROV` |
| `sources[].tracker`     | object          | that corpus's own backlog, with the same keys as the block above  |
| `types`                 | list of objects | the types this corpus adopted                                     |
| `types[].type`          | string          | the name of one adopted type, as `fixes`                          |

**`base` may be present and `null`.** Test the value rather than the key. `base` and `id` are null together, and a
`target` of `none` means that corpus files nowhere this export can address.

**Compare `id`, never a URL.** `github:github.com/Example/Repo` and `https://github.com/example/repo.git` are one
backlog written two ways, and `id` is the pair already normalised.

**A `sources` entry has no `framework` block.** The framework this corpus took is stated once, at the top level.

**Where `manifest.json` is missing or will not parse, stop and say so.** The plugin is not assembled as it should be.
That is itself worth a finding.

## Two label axes

`kac:finding` says what the ticket is. It never comes off. A finding filed two years ago and settled last week still
has it, so every finding ever filed stays one query away.

`kac:triaged` plus one route label says what triage decided. The queue is `kac:finding` without `kac:triaged`, so
nothing is triaged twice.

| Label                 | What it means                                                               |
|-----------------------|-----------------------------------------------------------------------------|
| `kac:route-framework` | the finding reports `kac`, the schema, a skill or the plugin. No record.    |
| `kac:route-record`    | a record in this corpus is wrong, or one is missing                         |
| `kac:route-misfiled`  | this belongs to a corpus under `sources`, and the ticket is on this backlog |
| `kac:route-none`      | a duplicate, or a record here already covers it                             |
| `kac:route-unclear`   | triaged, and a person has to decide                                         |

**Apply exactly one route label.** A finding matching two routes is `kac:route-unclear`, and the comment says which two.

**`kac:route-unclear` is a verdict, not a failure.** Reach for it wherever choosing between the others would be a guess.
An issue left untriaged is an issue nobody sees again.

## Make the labels first

A corpus that ran `kac new` last week has none of these. `gh issue edit` rejects the whole command when one label is
missing, so create them before you triage anything, whether or not you think they are there.

**Create `kac:finding` as well.** `raise-finding` files a finding unlabelled where the repository refuses the label,
so making it is what stops the next one going missing.

### GitHub

`tracker.target` is `github`, and `tracker.base` is the repository, as `https://github.com/<owner>/<repo>`.

```bash
gh label create kac:finding --repo <owner>/<repo> --color 006B75 --force \
  --description "An observation an agent filed against a corpus"
gh label create kac:triaged --repo <owner>/<repo> --color BFDADC --force \
  --description "A finding triage has routed"
gh label create kac:route-framework --repo <owner>/<repo> --color 1D76DB --force \
  --description "The finding reports the framework rather than a record"
gh label create kac:route-record --repo <owner>/<repo> --color 0E8A16 --force \
  --description "A record here is wrong, or one is missing"
gh label create kac:route-misfiled --repo <owner>/<repo> --color FBCA04 --force \
  --description "The finding belongs to a corpus this one consumes"
gh label create kac:route-none --repo <owner>/<repo> --color CFD3D7 --force \
  --description "A duplicate, or already covered by a record"
gh label create kac:route-unclear --repo <owner>/<repo> --color D876E3 --force \
  --description "Triaged, and a person has to decide"
```

`--force` updates a label that is already there, so running this every time costs one call each and changes nothing.

**A command that fails on sign-in or on permission is not a failure to report.** `gh auth status` says whether you are
signed in to that host. An account that cannot create a label can still read the issues, so carry on and say in your
reply that you will apply no labels.

### Azure DevOps

`tracker.target` is `azure-devops`, and `tracker.base` is the project, as `https://dev.azure.com/<org>/<project>`. `az`
wants the two apart: the organisation is `base` up to and including `<org>`, and the project is the segment after it.

**Azure creates a tag it does not already have.** There is nothing to make first, so skip this section and triage.

### Where the platform runs no tracker

`tracker.target` of `none`, or a `base` of `null`, means this corpus files nowhere. There is no queue to read. Say so,
name the corpus, and ask whoever is with you where its findings go.

## `triage`

### 1. Read the queue

The queue takes three reads: the findings waiting, the findings a label never reached, and the findings already
routed. The third is not work. You need it to spot a duplicate of something settled last month.

#### GitHub

```bash
gh issue list --repo <owner>/<repo> --state open --limit 200 \
  --search "label:kac:finding -label:kac:triaged" --json number,title,body,createdAt
gh issue list --repo <owner>/<repo> --state open --limit 200 \
  --search "kac-finding in:body -label:kac:finding" --json number,title,body,createdAt
gh issue list --repo <owner>/<repo> --state all --limit 200 \
  --search "label:kac:triaged" --json number,title,labels
```

#### Azure DevOps

The query returns ids. Read each work item after it, because the body is where the block and the observation are.

```bash
az boards query --org https://dev.azure.com/<org> --project <project> \
  --wiql "SELECT [System.Id] FROM WorkItems WHERE [System.Tags] CONTAINS 'kac:finding' AND [System.Tags] NOT CONTAINS 'kac:triaged' AND [System.State] <> 'Closed'"
az boards work-item show --org https://dev.azure.com/<org> --id <id>
```

**A finding may have arrived with no mark at all.** `raise-finding` files one unmarked where the platform refuses a
mark it does not have, so the second GitHub search looks for the block in the body instead. Azure creates a tag it does
not hold, so an Azure backlog has no unmarked case. Mark what the search finds `kac:finding` before you triage it.

**Read every one before you classify any of them.** Duplicates are only visible against the whole queue, and two
findings that are halves of one problem read as two unrelated tickets on their own.

**Say so where a query filled its limit.** A backlog longer than the limit is a backlog you read part of, and a run
that does not say which part read the rest as nothing.

**An empty queue is an answer.** Say the queue is empty and stop.

### 2. Read the block in each body

A finding opens with a fenced `yaml kac-finding` block. `corpus` says which corpus it is about, and `looks-like` says
which type the filing session thought it resembled.

**`looks-like` is a hint and decides nothing.** A body written before the field existed has none, and a person
filing by hand leaves it out. Classify from the body either way.

**A body with no block is still a finding.** Somebody filed it by hand, or an older skill wrote it. Read the prose and
classify it.

### 3. Find the duplicates

Compare each finding against the others in the queue, and against the routed findings the third query returned. Two
findings are one where the same thing was observed, whoever wrote them and however differently.

**Two halves of one problem are one finding.** Route the fuller one and mark the other `kac:route-none`, naming the
issue it duplicates.

**The one that stays open is the one with more in it**, not the one filed first.

### 4. Ask whether a record already covers it

**Load the `fix-lookup` skill and give it the symptom**, for a finding that reports something going wrong. It searches
the words a reader arrives with. Where it returns a fix whose resolution answers the finding, the route is
`kac:route-none`.

**Where this plugin has no `fix-lookup`, this corpus declined the `fixes` type.** `types` in `manifest.json` says which
types it took. There is nothing to search, so say so and classify from the body.

**Use whichever other lookup skill this plugin has**, chosen by what the finding is about. A finding about a rule is a
question for `standards-lookup`, one about what is allowed for `policy-lookup`, one about an order of work for
`process-lookup`. A bundle trimmed the ones whose types this corpus declined, so use what is here.

**A record that is wrong is not a record that covers it.** `kac:route-none` is for a finding the corpus already answers.
A finding saying the answer is wrong is `kac:route-record`.

### 5. Choose the route

Work down this list and take the first that fits.

1. **It duplicates another finding, or a record here already answers it.** `kac:route-none`.
2. **Its `corpus` names an entry under `sources`, or its subject is a record with that entry's shortcode.**
   `kac:route-misfiled`.
3. **It reports `kac`, the schema, the template, the plugin or a skill.** `kac:route-framework`. `looks-like:
   framework` says so where the block has it.
4. **It says a record here is wrong, or that one is missing.** `kac:route-record`.
5. **Anything else.** `kac:route-unclear`.

**A finding asking for a type this corpus declined is `kac:route-unclear`.** Adopting a type is a decision a person
takes, so say in the comment which type it would need.

### 6. Show the table and wait

Print one row per finding: the issue number, its title cut short, the route, and the reason in under a dozen words. Put
the `kac:route-unclear` rows at the top, because those are the ones needing an answer.

**Say what you are about to write.** One `kac:triaged` label, one route label and one comment on each issue.

Then wait. A person who changes a row has answered for the table.

### 7. Label, and comment the reason

Do these together, one issue at a time. An issue labelled with no comment is a verdict nobody can argue with.

#### GitHub

```bash
gh issue edit <number> --repo <owner>/<repo> --add-label kac:triaged --add-label kac:route-record
gh issue comment <number> --repo <owner>/<repo> --body-file <path>
```

Write the comment to a file. Passing it inline turns every backtick into a quoting problem.

#### Azure DevOps

Azure replaces the whole tag field, so read the tags the work item has and write them back with the new ones on the
end. Writing only the new ones drops `kac:finding`, and the issue leaves every query that would have found it again.

```bash
az boards work-item show --org https://dev.azure.com/<org> --id <id> --query "fields.\"System.Tags\""
az boards work-item update --org https://dev.azure.com/<org> --id <id> \
  --fields "System.Tags=<the tags that command printed>; kac:triaged; kac:route-record" \
  --discussion "<the reason>"
```

**`--discussion` takes the text and not a path.** Keep the Azure comment free of backticks and quotation marks.

#### What the comment says

Three sentences is plenty. The route, why, and what happens next.

* **Name the route in the first sentence**, in the words the label uses.
* **Name the issue a duplicate duplicates**, and the record that already covers a finding you routed to
  `kac:route-none`.
* **Name both routes** where you chose `kac:route-unclear` because two fitted, and say what a person has to decide.
* **Say nothing about whether the claim is true.** You did not check, and writing as though you did is the one thing
  this verdict must not do.

**Do not repeat the route as machine-readable data.** The label is the data. A second copy in the comment is a second
thing that can disagree with the first.

### 8. The framework route

A framework finding describes the tool rather than the person who filed it, so it may travel. It travels under two
conditions: a person reads every word first, and the body contains none of the filer's own provenance.

**Compare `framework.id` with `tracker.id`.**

**Every case here still gets `kac:triaged` and `kac:route-framework`, as step 7 applies them.** A finding left without
`kac:triaged` stays in the queue and is re-read, re-commented and re-drafted on every run.

**Where `framework.base` is `null`, this corpus states no address for the framework.** Mark the finding, draft the
upstream body, and say in your reply that nobody named a tracker for it. The fix is a `framework:` block in
`.corpus.yaml`.

**Where they are equal, the finding is already where framework work is filed.** Mark it and stop. There is nothing to
copy.

**Where they differ, draft the upstream body and post it as a comment.** Then mark it and stop. Somebody copies the
body into whatever tracker they can reach. Never file it yourself: an Azure DevOps user has no GitHub account, and a
corpus published from somebody else's repository is somebody else's backlog.

**Strip the filer's provenance out of what you draft.** Take out the session id, the repository the session was working
in, its commit, and the name of the corpus. What is left is what the framework maintainer needs.

`````
The body below reports the framework. Copy it to https://github.com/example/framework.

---
Title: `kac validate` reports no error for a citation into a corpus that is not consumed

## What I saw

A record cited `xyz:std-THING`, and `xyz` is in no `consumes:` entry. `validate` reported no error and
`export` wrote the citation through into the parts file.

## Context

Seen with `kac` 0.28.0, against template version 14. Reproduced twice.

## Why it might matter

A consumer restores the export and follows a citation into a corpus it does not have.
`````

**Name the framework version, and nothing that identifies the filer.** `mechanismVersion` in `manifest.json` is the
template version, and the `kac` version is what the session ran.

## `draft <issue>`

One issue, one pull request.

### 1. Check the issue

**`draft` takes a `kac:route-record` finding and nothing else.** Refuse the others and say where each goes.

* `kac:route-framework` has no record to write. Its upstream body is already a comment on the issue.
* `kac:route-misfiled` belongs on another corpus's backlog. Refile it there.
* `kac:route-none` is settled.
* `kac:route-unclear` needs a person first.
* An issue with no `kac:triaged` label has not been triaged. Run `triage`.

### 2. Check you are in the right checkout

`publishing.base` is the repository the records live in, and `publishing.pathPrefix` is the folder inside it. The
tracker and the repository are two addresses, and on Azure DevOps they are two different things.

**Read `publishing.target` first.** Only `github` and `azure-devops` address a repository a pull request can open
against. `azure-devops-wiki` addresses a wiki, `mkdocs` a documentation site, and `none` nothing at all. Refuse the
last three, print the record, and say where the corpus is published from instead.

**Refuse where the checkout is a different repository.** Say which repository the record belongs in.

**Refuse where you have no checkout.** Print what the record should say and stop.

### 3. Write the record, or the edit

A finding routed here asks for one of two things: an edit to a record that already exists, or a record nobody has
written. Read it and decide which.

**An edit: read the record, and change the words the finding is about.** Leave everything else alone. Add an entry to
the record's `## Changelog` where it has one, newest first.

**A new record: start from the type's `_template.md`.** It sits in the folder the record goes in, and states every field
that type needs and what each one takes. Follow it rather than copying a neighbouring record.

**Load the `writing-a-record` skill where this checkout has it**, and `technical-writing` before it. A record written in
some other voice reads as an import.

**Write `status: draft` where the type takes it.** A record an agent wrote has been accepted by nobody, and a type that
verifies its records refuses one an agent claims to have verified. The type's `_template.md` says which fields a draft
leaves out.

**Ask who owns the record, and write the answer.** `owner` is required on every type, so a record without one fails
`kac validate` and the pull request carries something the corpus refuses. It takes a person as `human:alex.doe` or a
post as `role:head-of-engineering`. Never guess it, and never copy the owner of a neighbouring record.

**Never write a verification line naming somebody.** A `verified` entry says a person checked the answer, and nobody
has.

**Run `kac validate` where the checkout has the tool.** A record failing its own schema wastes the reviewer's time.

### 4. Open the pull request

Branch, commit and open it. Put the issue number in the body, so the pull request and the finding link to each other.

```bash
gh pr create --repo <owner>/<repo> --title "<the record, in one line>" --body-file <path>
```

On Azure DevOps the command is `az repos pr create`, against the repository inside the project.

**Say in the body that an agent wrote it, from which finding.** The reviewer is deciding whether the observation is
true, which triage did not.

**Do not merge it, and do not approve it.** A person who did not write it accepts it.

## Say what you did

After `triage`, say how many findings you read, how many you labelled, and the count against each route. Name every
`kac:route-unclear` row and what a person has to decide about it. Where you drafted an upstream body, say which issue it
is a comment on and which tracker it is for.

After `draft`, name the pull request and its URL, and the issue it answers. Say which fields you left empty, and that
the record is a draft until somebody accepts it.

**Say what you could not do.** A missing client, a label you could not create, an issue the tracker refused to update. A
finding you read out to somebody is worth more than one lost to a missing tool.
