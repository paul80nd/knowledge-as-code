---
name: harvest-findings
description: Triage the findings filed against this corpus, and draft the record one of them asks for. Use when
  someone says "triage the findings", "harvest the findings", "what findings are open", "turn that finding into a
  record", or gives you an issue number and asks you to triage it or to write the record behind it. No invocation
  runs unasked. `raise-finding` files an observation and stops there. This is what moves it.
---

# Triaging findings, and drafting what they ask for

A finding is an observation an agent filed as an issue on the tracker. `raise-finding` writes it and stops, and nothing
after that decides where it belongs. This skill has three invocations:

* **`triage`** reads every finding nobody has triaged, sorts each one into a route, and shows you a table. Once you say
  yes, it labels each issue and comments the reason.
* **`triage <issue>`** does the same for one finding. It still reads the queue at step 1, because a duplicate shows
  only against the rest, and labels only the issue it was given.
* **`draft <issue>`** takes one triaged finding and writes the record it asks for, as a pull request.

## Never write unasked

**Start only when somebody asks, and show what you will write before you write it.** It goes out under their name, onto
a tracker other people read.

* **One yes covers the whole triage table.** A person who changes three rows has answered for all of them.
* **A silent no is a no.** Where nobody answers, print the table and stop.
* **Creating a label is a write**, so it waits for the same yes.
* **`draft` shows the record before it commits.** It writes a file, a branch and a pull request.

## Find the corpus first

Only the corpus's own maintainer runs this. A session holding the corpus's source is its maintainer, and a session
holding the export alone is not.

**Walk up from where you are to `.corpus.yaml`.** Start from a file inside the corpus you mean, because one repository
may hold several. Its `corpus:` names the corpus to triage, and its tracker address and area name the backlog. Say
both in your reply before you write anything.

* **Where you find no `.corpus.yaml`, refuse.** You are outside a corpus. Somebody holding an export alone uses
  `raise-finding`, which files one issue and asks first.
* **Where it will not parse, stop and say so.** A broken corpus is not something to triage.
* **Test the corpus, never the publishing address.** A corpus published as a wiki or a documentation site has no
  repository in `publishing.base`, and its maintainer still has a tracker and the source.

**Every invocation needs the tracker's client, signed in.** The platform file says how to check. Where the client is
missing, print what you produced and say which invocation you could not finish.

## The platform files

**Every command this skill runs is in the file for the tracker's `target`.** Read that file, and no other, before you
run the first command. Each of its sections is headed by the step it serves, and sends you back to that step.

| `target`       | Read                                                                                               |
|----------------|----------------------------------------------------------------------------------------------------|
| `github`       | [github.md](github.md): `gh` for the labels, the issues and the pull request                       |
| `azure-devops` | [azure-devops.md](azure-devops.md): `az boards`, `az rest` and `az repos`                          |
| `none`         | nothing. [Where the platform runs no tracker](#where-the-platform-runs-no-tracker) says what to do |

Both files sit beside this one, in the base directory this skill was loaded from.

## Triage does not decide whether a finding is true

**Check three things, and none is the claim**: whether this finding says what another open one says, whether a record
here already covers it, and whether it asks for detail below the level the record is written at. A session's account of
its own work is not evidence, and checking every claim turns one run into as many investigations.

**Whether the claim is true is settled at the pull request**, where a person takes accountability for what an agent
wrote. A verdict says where the observation belongs, never whether it is right.

## Read the addresses

`.corpus.yaml` describes this corpus. Three blocks say where work is filed, and they are different addresses.

| Key                      | What it contains                                                       |
|--------------------------|------------------------------------------------------------------------|
| `corpus`                 | the name of this corpus, as `example-payments`                         |
| `tracker.target`         | one of `github`, `azure-devops`, `none`                                |
| `tracker.base`           | the backlog to file against                                            |
| `tracker.area`           | the area path inside an Azure DevOps project, separated by `/`         |
| `framework.target`       | the same three values                                                  |
| `framework.base`         | where a problem with `kac`, the schema or a skill is reported          |
| `framework.area`         | the area path that problem is filed under                              |
| `publishing-target`      | one of `github`, `azure-devops`, `azure-devops-wiki`, `mkdocs`, `none` |
| `publishing.base`        | the repository holding the records                                     |
| `publishing.path-prefix` | the folder inside it the corpus sits in                                |
| `consumes[].corpus`      | the name of one corpus this one consumes                               |
| `consumes[].shortcode`   | the prefix its records carry, as the `eng` in `eng:pol-AGNT.PROV`      |
| `types`                  | the types this corpus adopted, one name per line                       |

**Most corpora state no `tracker:` block, and derive one from `publishing` instead.** A repository on GitHub has an
issue list of its own, so there is nothing to state. Read the block where it is there, and derive it where it is not.

| `publishing-target` | The tracker it derives         |
|---------------------|--------------------------------|
| `github`            | `github`, at `publishing.base` |
| `azure-devops`      | `azure-devops`, at the project |
| `azure-devops-wiki` | `azure-devops`, at the project |
| `mkdocs`            | none                           |
| `none`              | none                           |

**An Azure DevOps base is cut back to the project.** One project holds one backlog and many repositories, so the
segment from `/_git/` or `/_wiki/` onwards names a repository or a wiki and no ticket is filed on either. Take
`https://dev.azure.com/acme/Platform` out of `https://dev.azure.com/acme/Platform/_git/knowledge`.

**An area divides one Azure DevOps project between corpora.** Nothing derives it, so a corpus without `area` files in
the project's default area. `raise-finding` files to the same area. `azure-devops.md` says how to write it as an area
path.

**A key may be present and empty.** Test the value, not the key.

**Normalise an address before you compare it.** `https://github.com/Example/Repo.git` and
`https://www.github.com/example/repo` are one backlog written two ways. Drop the scheme, drop a leading `www.`, drop a
`.git` suffix, drop a trailing slash, and lowercase what is left. Normalise an area the same way: trim a `/` from
either end and lowercase it, and read an empty one as none.

**Two addresses match only where their targets, bases and areas all match.** One URL under two clients is two
backlogs. One Azure DevOps project under two areas is one backlog divided between two owners.

**Where the target and base match and only the area differs, bounce the work item.** A bounce moves the finding to
the other area and leaves the verdict to whoever triages that area. Nothing is refiled: the finding keeps its id, its
history and `kac:finding`. Move it to the other address's area, or to the project alone where that address states no
area, then comment why it moved and where. `azure-devops.md` has both commands under "Bounce a work item".

**A bounced finding takes no `kac:triaged` and no route label.** It leaves this corpus's queue because the queue
selects by area, and it arrives in the other area's queue untriaged. A route label here would hide it there.

A bounce is a write, so it waits for the same yes as the labels. Where Azure refuses the area, because it does not
exist or you cannot write to it, leave the work item where it is, with no comment, and say so in your reply.

**A `consumes:` entry states no framework.** The framework this corpus took is stated once, at the top level.

**Where the corpus consumes another one, `.imports/<shortcode>/manifest.json` states that corpus's own tracker.** Its
`tracker` block has `target`, `base`, `area` and `id`. The `id` leaves `area` out, so compare the area beside it.
`kac restore` writes the folder, so it is missing until somebody runs it. A finding you route to another corpus needs
that address, so say where you could not read one.

## Two label axes

`kac:finding` says what the ticket is. It never comes off, so every finding ever filed stays one query away.

`kac:triaged` plus one route label says what triage decided about a finding it keeps. The queue is `kac:finding`
without `kac:triaged`, so nothing is triaged twice. A finding bounced to another area gets neither label, as
[Read the addresses](#read-the-addresses) says.

| Label                 | What it means                                                                   |
|-----------------------|---------------------------------------------------------------------------------|
| `kac:route-framework` | the finding reports `kac`, the schema, a skill or the plugin. No record.        |
| `kac:route-record`    | a record in this corpus is wrong, or one is missing                             |
| `kac:route-misfiled`  | a corpus under `consumes:` answers this, and the ticket was filed here          |
| `kac:route-none`      | a duplicate, a record here already covers it, or it is below the record's level |
| `kac:route-unclear`   | triaged, and a person has to decide                                             |

**Apply exactly one route label.** A finding matching two routes is `kac:route-unclear`, and the comment says which two.

**`kac:route-unclear` is a verdict, not a failure.** Reach for it wherever choosing between the others would be a guess.
An issue left untriaged is an issue nobody sees again.

## Making the labels

A platform that refuses an unknown label needs every label you are about to apply made before step 8 applies the
first one. A corpus that ran `kac new` last week has none of them. The platform file says whether its platform refuses
one.

**List them before you triage, and create nothing yet.** Read what the tracker already has, and count the missing ones
into what you tell the person you are about to write.

**Create the missing ones after they say yes**, immediately before you label the first issue.

**Count `kac:finding` among them.** `raise-finding` files a finding unlabelled where the tracker refuses the label, so
making it is what stops the next one going missing.

**An account that cannot create a label can still read the issues.** Carry on, and say in your reply that you will
apply no labels.

## Where the platform runs no tracker

A `tracker.target` of `none`, or a derivation that reaches none, means this corpus files nowhere. There is no queue to
read. Say so, name the corpus, and ask whoever is with you where its findings go.

## `triage`

### 1. Read the queue

The queue takes three reads: the findings waiting, the findings a label never reached, and the findings already
routed. The third is not work: it is how you spot a duplicate of something already settled.

**Read only this corpus's own area.** Several corpora can file into one Azure DevOps project, each under its own area,
and the others' findings are theirs to triage. A finding another corpus bounced here arrives with `kac:finding` alone,
so the first read takes it in. Where the tracker states no area, say in your reply that the queue read the whole
project.

**A finding may have arrived with no mark at all.** `raise-finding` files one unmarked where the platform refuses a
mark it does not have. The second read searches the bodies for the block instead, and returns candidates, not
findings. Give each one whose body has the opening fence the `kac:finding` label, then triage it. A platform that
creates any tag it is given has no unmarked case.

**Read every one before you classify any of them.** Duplicates are only visible against the whole queue, and two
findings that are halves of one problem read as two unrelated tickets on their own.

**Say so where a query filled its limit.** You read only part of that backlog.

**An empty queue is an answer.** Say the queue is empty and stop.

**Given one issue, read it on its own as well as the queue.** Classify it alone, and show a one-row table. Refuse one
already labelled `kac:triaged`.

### 2. Read the block in each body

A finding opens with a fenced `yaml kac-finding` block. `corpus` says which corpus it is about, and `looks-like` says
which type the filing session thought it resembled.

**`looks-like` is a hint and decides nothing.** A body written before the field existed has none, and a person
filing by hand leaves it out. Classify from the body either way.

**A body with no block is still a finding.** Somebody filed it by hand, or an older skill wrote it. Read the prose and
classify it.

**A body may arrive as HTML.** Read the block's text out of it, as the platform file says, and say in the triage
comment that the body arrived as HTML.

### 3. Find the duplicates

Compare each finding against the others in the queue, and against the routed findings the third read returned. Two
findings are one where the same thing was observed, whoever wrote them and however differently.

**Two halves of one problem are one finding.** Route the fuller one and mark the other `kac:route-none`, naming the
issue it duplicates.

**The one that stays open is the one with more in it**, not the one filed first.

### 4. Ask whether a record already covers it

**Search this corpus's own records.** They are in the working tree, one folder per type. `types` in `.corpus.yaml`
lists the types it adopted. Search the words a reader would arrive with, not an id.

| The finding is about        | Search       |
|-----------------------------|--------------|
| something going wrong       | `fixes/`     |
| a rule you have to build to | `standards/` |
| what the estate allows      | `policies/`  |
| an order of work            | `processes/` |
| what proves a rule          | `controls/`  |

**The table is the common cases.** Where the subject fits none of them, search the folder of whichever adopted type
it belongs to.

**A type this corpus declined has no folder.** Say so and classify from the body.

**Where the corpus consumes another one, search `.imports/<shortcode>/` as well.** A rule it inherits still answers a
finding. That folder exists only after `kac restore`, so say so where it is missing.

**A record that is wrong is not a record that covers it.** `kac:route-none` is for a finding the corpus already answers.
A finding saying the answer is wrong is `kac:route-record`.

**Nor is a `not-enforced` control.** It records that nothing checks the rule. A finding asking for that check is
`kac:route-record`, and the draft moves the control's `mechanism` once the check exists.

### 5. Read the level of detail

**Read `## Level of detail and cadence` on the type page of the record the finding touches.** The page is
`<folder>.md` at the corpus root. The section says what one step, rule or part of that type contains, and where finer
detail belongs.

**Judge whether the finding asks for detail below that level.** Each step added fits the record to one case and makes
it longer for every other reader. A finding at the record's level goes on to step 6 as it is. Mark a below-level
finding with the first of these that fits, and step 6 routes it:

1. **Narrowed.** A word of the record is wrong or missing at its own level: an outcome too vague to act on, or a reason
   that does not reach the case. The draft refines that step, rule or part.
2. **Redirected.** Another destination is written at the level the finding asks for. In this corpus, a command for
   something broken goes to a runbook or a fix, and a mechanism goes to a standard's **SHOULD** or a control. The draft
   is a record of that type. Outside the corpus, a command or a trap in a planned task goes to the tool's
   documentation, a `CLAUDE.md` or a contributors' guide.
   * A redirect outside the corpus routes `kac:route-record` where a record here changes as well, and
     `kac:route-none` where none does.
3. **Settled.** The record is right at its level, and the finding adds a detail only its own case needed.

**Where the type page has no such section, say so in the comment and go on to step 6.** A corpus may have rewritten
the page it was seeded with.

### 6. Choose the route

Work down this list and take the first that fits.

1. **It duplicates another finding, a record here already answers it, or step 5 settled it.** `kac:route-none`.
2. **Its `corpus` names an entry under `consumes:`, or its subject is a record with that entry's shortcode.**
   `kac:route-misfiled`.
3. **It reports `kac`, the schema, the template, the plugin or a skill.** `kac:route-framework`. `looks-like:
   framework` says so where the block has it.
4. **Step 5 redirected it outside the corpus, and no record here changes.** `kac:route-none`.
5. **It says a record here is wrong or missing, or step 5 narrowed or redirected it.** `kac:route-record`.
6. **Anything else.** `kac:route-unclear`.

**A finding asking for a type this corpus declined is `kac:route-unclear`, whichever item above it matched.** Adopting a
type is a decision a person takes, so say in the comment which type it would need.

**A misfiled finding is not always on the wrong tracker.** Compare that corpus's tracker in
`.imports/<shortcode>/manifest.json` with this corpus's, target, base and area, as
[Read the addresses](#read-the-addresses) normalises them. Say in the comment which of these it is:

* **The targets or the bases differ.** The ticket has to move to another backlog, and the comment names it.
* **Only the areas differ.** Bounce the work item to that corpus's area, as
  [Read the addresses](#read-the-addresses) shows. It takes no label here, and that corpus's triage routes it.
* **All three are equal.** One backlog serves both corpora. Nothing moves, and what changes is which corpus answers.

### 7. Show the table and wait

Print one row per finding: the issue number, its title cut short, the route, and the reason in under a dozen words. Put
the `kac:route-unclear` rows at the top, because those are the ones needing an answer.

Show a bounce as its own row, with the area it goes to in place of a route.

**Say what you are about to write**: every label the tracker is missing, then one `kac:triaged` label, one route
label and one comment on each issue you keep, and one move and its comment on each issue you bounce. Then wait for the
yes.

### 8. Label, and comment the reason

Create the labels the tracker was missing first, as [Making the labels](#making-the-labels) says. Then label and
comment one issue at a time, doing the pair together. An issue labelled with no comment is a verdict nobody can argue
with. Bounce each finding that goes to another area, as [Read the addresses](#read-the-addresses) says.

**Write each comment to a file with your own file tool.** Passing it inline turns every backtick into a quoting
problem. The platform file has the commands that label, comment and bounce.

#### What the comment says

Three sentences is plenty. The route, why, and what happens next.

* **Name the route in the first sentence**, in the words the label uses.
* **Name the issue a duplicate duplicates**, and the record that already covers a finding you routed to
  `kac:route-none`.
* **Say whether a misfiled finding moves**, and name the corpus that answers it either way.
* **Say why a bounced finding left and which corpus's area it went to.** Its triage there starts from this comment.
* **Say that the body arrived as HTML**, where it did.
* **Say what step 5 decided** where it narrowed, redirected or settled the finding: the step, rule or part the draft
  refines, the type the record belongs to, the file outside the corpus the detail goes to, or the section that settles
  it.
* **Name both routes** where you chose `kac:route-unclear` because two fitted, and say what a person has to decide.
* **Say nothing about whether the claim is true.** You did not check it.

**Do not repeat the route as machine-readable data.** The label is the data. A second copy in the comment is a second
thing that can disagree with the first.

### 9. The framework route

A framework finding describes the tool rather than the person who filed it, so it may travel. It travels under two
conditions: a person reads every word first, and the body contains none of the filer's own provenance.

**Compare the framework's address with this corpus's tracker address**: target, base and area, as
[Read the addresses](#read-the-addresses) normalises them. They are equal where a problem with `kac` and a problem with
a record are filed in the same place.

**Every case kept here still gets `kac:triaged` and `kac:route-framework`, as step 8 applies them.** A finding left
without `kac:triaged` in this corpus's area stays in the queue and is re-read, re-commented and re-drafted on every
run. A bounced finding is the one exception, because it leaves the area.

**Where `framework.base` is `null`, this corpus states no address for the framework.** Mark the finding, draft the
upstream body, and say in your reply that nobody named a tracker for it. The fix is a `framework:` block in
`.corpus.yaml`.

**Where they are equal, the finding is already where framework work is filed.** Mark it and stop. There is nothing to
copy.

**Where only the areas differ, bounce the work item to the framework's area, and stop.**
[Read the addresses](#read-the-addresses) shows the bounce. It takes no label here. The work item stays inside the
project, so there is nothing to copy and nothing to strip.

**Where the targets or the bases differ, draft the upstream body and post it as a comment**, with the comment command
step 8 uses. Then mark it and stop. Somebody copies the body into whatever tracker they can reach. Never file it
yourself: an Azure DevOps user has no GitHub account, and a corpus published from somebody else's repository is
somebody else's backlog.

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

**Name the framework version, and nothing that identifies the filer.** `upstream.template-version` in `.corpus.yaml`
is the template version, and the `kac` version is what the session ran.

## `draft <issue>`

One issue, one pull request. The pull request may change more than one record. It may also contain the check a
`not-enforced` control describes, where this repository runs that check. Where the check runs in another repository,
the body says what that repository owes.

### 1. Check the issue

**`draft` takes a `kac:route-record` finding and nothing else.** Refuse the others and say where each goes.

* `kac:route-framework` has no record to write. Its upstream body is already a comment on the issue.
* `kac:route-misfiled` asks for a record in a corpus this one consumes, so its own maintainer drafts it. Where that
  corpus files on another tracker, which `.imports/<shortcode>/manifest.json` says, refile the ticket there first.
* `kac:route-none` is settled.
* `kac:route-unclear` needs a person first.
* An issue with no `kac:triaged` label has not been triaged. Run `triage`.

**Refuse a finding that already has a pull request, open or merged.** The record it asked for is written. The
platform file reads the linked pull requests and the state alongside the labels. Name the pull request and stop.

**Refuse a closed finding as well.** Somebody closed it by hand, so the observation is settled and no record is owed.

### 2. Check where the pull request goes

The tracker and the repository are two addresses, and on Azure DevOps they differ.

**Read `publishing-target` first.** Only `github` and `azure-devops` address a repository a pull request can open
against, and `publishing.base` names it. `azure-devops-wiki` addresses a wiki, `mkdocs` a documentation site, and
`none` nothing at all. Refuse the last three, print the record, and say where the corpus is published from instead.

**`publishing.path-prefix` is the folder the corpus sits in**, where the repository holds more than the corpus.

### 3. Write the record, or the edit

A finding routed here asks for one of two things: an edit to a record that already exists, or a record nobody has
written. Read it and decide which.

**An edit: read the record, and change the words the finding is about.** Leave everything else alone. Add an entry to
the record's `## Changelog` where it has one, newest first, naming the issue.

**Write the edit at the type's level.** `## Level of detail and cadence` on the type page states it, and the triage
comment names the step, rule or part to refine. Rework that part, or its reason, before you add a new step, rule or
part. A sharper outcome fits the next case as well, and an added step fits one.

**A new record: start from the type's `_template.md`.** It sits in the folder the record goes in, and states every field
that type needs and what each one takes. Follow it rather than copying a neighbouring record.

**Load `technical-writing`, then `writing-a-record`.** Both sit beside this one under `.claude/skills/`.

**Name the issue in `sources`.** Every type accepts the field, and it gives a reviewer the observation to check the
record against. Write the full address the platform file gives, because no type renders `sources` and a short form
such as `<owner>/<repo>#<issue>` links nowhere.

```yaml
sources:
  - { resource: <the issue's full address> }
```

**An edit keeps the entries the record already has**, and adds this one under them.

**A `reports` record is the exception.** Its `sources` names the corpora the report answers for and the version of
each, and it keeps no changelog. So the pull request body is the only trace a report drafted from a finding keeps.
`writing-a-report` says how to regenerate one.

**Write `status: draft` where the type takes it.** A record an agent wrote has been accepted by nobody, and a type that
verifies its records refuses one an agent claims to have verified. The type's `_template.md` says which fields a draft
leaves out.

**Ask who owns the record, and write the answer.** `owner` is required on every type, so a record without one fails
`kac validate` and the pull request carries something the corpus refuses. It takes a person as `human:alex.doe` or a
post as `role:head-of-engineering`. Never guess it, and never copy the owner of a neighbouring record.

**Never write a verification line naming somebody.** A `verified` entry says a person checked the answer, and nobody
has.

**Run `kac validate` where the checkout has the tool.**

### 4. Open the pull request

The pull request links the finding, so merging it settles the finding. Closing the finding yourself would settle an
observation nobody has accepted.

**Commit the record to a new branch and push it.** Then write the body to a file and open the pull request against the
repository step 2 found, with the command in the platform file. Its title is the record, in one line.

**Name the record's id and its path in the body.** The finding closes with a link to the pull request, so the body is
where a reader arriving from the observation learns which record answered it.

**Say in the body that an agent wrote it, from which finding.** The reviewer is deciding whether the observation is
true, which triage did not.

**Do not merge it, and do not approve it.** A person who did not write it accepts it.

## Say what you did

After `triage`, say how many findings you read, how many you labelled, and the count against each route. Name every
`kac:route-unclear` row and what a person has to decide about it. Where you drafted an upstream body, say which issue it
is a comment on and which tracker it is for. Name each work item you bounced, and the area it went to. Name each
finding whose body arrived as HTML.

After `draft`, name the pull request and its URL, and the issue it answers. Say that the record names that issue in
`sources`, and that merging the pull request settles it. Say which fields you left empty, and that the record is a
draft until somebody accepts it.

**Name what you could not do**: a missing client, a label you could not create, an issue the tracker refused to
update, an area it refused to bounce one to.
