# Shaping a type

A type file decides what one kind of record looks like: its fields, its sections, its rules, and how much of it travels
to another corpus. This page is the reasoning behind those decisions for the types the framework ships. Read it when you
are adding a type of your own, or changing what one of these sends.

A corpus is one repository of knowledge records kept in git. A record is one Markdown document in it, with YAML
frontmatter above its prose. An export is the machine-readable copy `kac export` writes for another corpus to read.

[What the schema is held to](held-to.md) says what the loader refuses in a type file. [The export format](export.md) is
the contract every export meets. This page says why each type chose what it chose inside those two.

## What every type decides the same way

Three decisions repeat across the set, so they are stated here once.

**`owner` stays behind.** Every type that exports at all keeps it out, except one. An owner is a fact about this
corpus's stewardship, and a reader holding a vendored copy cannot ask that person anything. Deviations are the
exception, and [deviations](#deviations) says why.

**A procedure's steps do not travel.** Processes and runbooks are followed whole and in order. Neither declares its
steps as parts. A step lifted out of its sequence is one a reader can act on alone, and acting on it alone is how an
incident gets worse.

**`last-rehearsed` and `rehearsal-frequency` travel together.** A date says nothing about staleness without the cadence
or the event you read it against.

## What each type sends

One section per type that declares an `export:` block, in tier order. A tier is the level of authority a type sits at,
and [Taxonomy](../framework/taxonomy.md#the-four-tiers) lists the four.

[A type held back](export.md#a-type-held-back) says what a type declaring none would owe its reader.

### adrs

A standard cites the decision its authority comes from, and `derived-from` resolves only where the ADR travelled too.

`Decision` travels whole, because the decision sentence is the record. `Consequences` travels whole beside it: a reader
who sees what a decision bought but not what it cost sees a better decision than the one you took. `Alternatives
Considered` stays behind. It is the longest section an ADR has, it argues against options this estate weighed, and the
record is one fetch away.

`supersedes` and `superseded-by` travel as a pair. A superseded decision still binds whatever was built under it, so a
reader meeting one needs the id that replaced it. `deciders` stays behind with `owner`.

### postmortems

`Root cause` and `Contributing factors` are what a reader elsewhere can act on, and most of the improvement lives in
the factors. `Summary` travels above them, so a reader can decide in fifteen seconds whether the rest is theirs.
`Resolution` travels beside them, because what ended an impairment is the half a reader facing the same one needs
first.

**The three lessons each take a heading of their own.** Google groups `What went well`, `What went wrong` and `Where we
got lucky` under one `Lessons Learned`, and a group is a heading an author satisfies by writing under any part of it.
Declared separately, each is asked for and a bare one is refused, which is what makes `Where we got lucky` get answered
rather than skipped. All three travel: a copy carrying only what failed teaches half the lesson, and a copy carrying
only what worked teaches the other half.

`Impact` travels as its opening paragraph, where the record states what customers lost. The paragraphs under it measure
that loss against this estate's own NFRs.

`Timeline` and `Actions` stay behind. A timeline names one estate's clocks, alerts and systems. Each action is a link to
a work item in a tracker the reader cannot open, which is the argument [fixes](#fixes) makes about `How we found it`.

Three moments travel, and `duration` with them. `occurred-at` is when the impairment began, `detected-at` when
somebody first knew, and `restored-at` when service was back for users. Each is a timestamp rather than a date,
because the gap between the first two is usually measured in minutes.

**`restored-at` is recovery, not resolution.** MTTR names four measures at once: time to respond, to repair, to
recover and to resolve. This field is recovery, which is the span an availability budget counts and the one
[DORA](https://dora.dev/guides/dora-metrics-four-keys/) asks about. An incident whose actions run for weeks still has
a `restored-at` on the day. Nothing requires `restored-at` to follow `detected-at`: an incident can recover before
anybody notices, and one found later in the logs is written that way.

`duration` restates the span those moments already fix, so that an index can show it without computing one.
`duration-matches-the-moments` fails a value the moments refuse, and its message carries the value to write.

`prompted` travels beside them, and its ids resolve wherever those records travelled too.

### policies

A clause travels with its level as a key of its own. `MUST` and `COULD` open the same shape of sentence, so reading the
words alone means knowing this type's modals to tell an obligation from a suggestion. The key states the level instead,
and an agent asking what it must do filters on one value.

`Scope` and `Exceptions` travel whole, because each bounds the clauses beneath it. A reader holding a clause without
them reads a stricter policy than the one you wrote.

No alignment travels: not on the clause line, not from the cell, not from the roll-up. A mapping that did travel would
say a policy touches A.8.24 and leave the reader to guess what touching it commits you to.
[`frameworks.jsonl`](export.md#frameworksjsonl) states the same edge from the end where the standing can travel beside
it.

### standards

A rule travels as its heading and the obligations beneath it, in the Markdown the standard wrote them in. The bold
**MUST** is what binds a line under BCP 14, so flattening the Markdown would leave an agent reading an obligation as
advice.

`Summary` travels because a rule read without it binds more than the standard does. `Conformance checklist` travels
because it is the same rules written as a test. `Examples` stays behind as the longest section, written against one
stack, and `Rationale and provenance` stays behind because a reader acts on what the rule says.

`implements` and `covers` travel so a reader can count its own coverage. A corpus inheriting the policies it must meet
also inherits the standards discharging them, and without this edge it sees only what its own standards cover. The
exporter drops a rule's `Covers` line before it takes the obligations, so a rule with nothing else travels with no
obligations rather than with the footnote standing where they belong.

`seeAlso` lists the rules of other standards this one points at, because a standard composes by union and one rule
commonly leans on another. It never lists a policy clause: a clause is cited as `[pol-INTC].HOLDS`, and the link ends at
the policy.

`depends-on` states that composition at the record level, for the case a folder cannot: `platform/dotnet/testing.md`
adds the .NET detail to `common/testing.md`, and the two sit in different folders. A consumer reads it to know which
further standards it has to apply. `superseded-by` travels for the same reason, so a reader arriving from an old
citation is sent to the standard that replaced this one.

### controls

A control travels whole. It runs to a few hundred words, and a reader who sees what a check promises without seeing what
it misses sees a stronger check than the one you built.

`mechanism` and `frequency` are the two keys an agent filters on. `not-enforced` travels like any other value, so a
reader learns that a rule is written and that nobody looks. Withholding those records would publish a coverage figure
this corpus cannot show. Two more keys travel beside them. `evidence` says where the proof lives. `last-verified` says
when a manual check last ran, so a reader can tell a check that happened from one that was only planned.

`verifies` points at a whole standard or one rule inside it, and you read the value to tell which. Where a control names
the record, it vouches for that whole document whatever it checks inside it.

### deviations

A deviation travels whole. The people who carry the risk of a departure usually sit outside the team that raised it, and
a register they cannot read surfaces nothing. This is the one type whose whole purpose is to be read by somebody else.

The six sections are the whole of the argument: what you do instead, why, what can still go wrong, what limits that,
what ends it, and how far it applies. A reader who sees five of them sees a smaller departure than the one you took.

**`owner` travels, and this is the only type where it does.** A deviation names the individual who accepted the risk,
and a register that says what was excused without saying who excused it is not a register. This type takes the `human:`
form alone. Accepting a risk is a person putting their name to a consequence, and a post cannot do that: whoever holds
it on the day would be accepting something they never read. `assigned-to` names whoever does the work, admits a post,
and stays behind, because a consumer reads who carries the risk and not who is clearing it.

`risk` rates what is left once what compensates is working. It travels, because it is what a reader sorts a register on,
and it decides how soon `review-by` may fall.

`accepted-on`, `review-by` and `closed-on` say whether a departure is live, overdue or over. They travel beside
`status`, which stays `active` past its review date, so you can compare the two accounts of one record. Nothing moves
the status on that date, because a departure left standing is the state worth seeing. The `expiry` check warns on it.

### nfrs

An NFR is normative, and a reader may act on it without opening anything else. `Target`, `Why this number`, `How it
is measured`, `What a breach costs` and `What we do about a breach` are required and travel whole. `Current actual` is
required too, and stays behind.

`characteristic`, `target`, `window` and `measured-by` state as fields what those sections state at length. The fields
are what a reader filters and sorts on, and the sections are what somebody building against the number reads.

**The cost of a breach and the response to one are two sections.** The SRE workbook separates an error budget from the
error budget policy that spends it. Both headings are required, so `required-section` reports one a record left out.
This type needs no rule of its own.

**`characteristic` takes 25010's subcharacteristic level, not its characteristic level.** An estate commits to
availability, latency, throughput, capacity, recovery, scalability or accuracy. It does not commit to reliability,
which sits above availability and recovery and says nothing about the other five. One record states one quality, so a
service needing both a latency target and a recovery target has two.

**`window` is required of an availability, capacity, latency or throughput target.** A rate or a percentile only means
something over a stated period. `window` declares a `required-when` reading `characteristic`. A recovery, scalability
or accuracy target binds each event or each value, so it needs none.

`Constraints` travels because a target read without its limits reads as a stronger promise than the estate made. It
states those limits in words, because `constrained-by` resolves only where the corpus adopted `integrations`. A corpus
that did not still has the cap, and this section is where it writes it down.

**`Current actual` is required, and stays behind.** It is required because a target read without the reading beside
it says nothing about where the estate stands. It stays behind because it is a measurement taken on the day somebody
wrote it, and an export has no way to say how old it has since become.

**`status: aspirational` says nobody is held to the target.** The SRE workbook keeps an aspirational objective
measured and tracked, and exempts it from the error budget policy that a missed objective otherwise triggers. Here
that exemption is `What we do about a breach` saying nobody is paged, and `status` is what a reader sees first.

**The distinction is a status, not a second field.** A `basis` enum beside `target` would restate the comparison
`Current actual` already supports, and nothing could hold the two in step: `target` is free text such as `p95 under
800ms`, so no rule compares it against a reading written in prose. Whether anybody is held to a number is a decision
instead, which is what `status` records for every type here.

### fixes

`symptom-keywords` is what a lookup searches on. It is the field this type exports for.

`Symptom`, `Environment`, `Cause` and `Resolution` travel whole. A reader matches on the symptom, checks the
environment and the cause are theirs, then acts, so a resolution arriving without either is half an answer. `Why it
happens` travels with them: `Cause` says what went wrong this time, and `Why it happens` says what class it belongs to.

`Environment` is required because `applies-to` takes service ids alone. A fix about a laptop, an editor or a pinned
tool version has no service to name, and a reader still has to match the versions.

`verified` travels, and `trust` is derived from it. This type exists to say how far a resolution has been taken on
trust. `status` and `review-by` say whether the answer is still true, and `fixed-upstream` is the value that earns them
both.

`status: draft` is the value an author writes before anybody checks the resolution. `verified` is required of every
other status and not of a draft, because an agent writing a fix from a filed observation cannot name who will verify
it. A corpus listing `draft` under `export.exclude:` withholds every draft it has, this type's included, so no
unchecked resolution travels. The record still sits in git for a reviewer to read.

`How we found it` stays behind. The route is often the more reusable half of a fix, and it is reusable to somebody who
can run the commands it lists. A reader holding a copy can run none of them.

### services

A service id is cited from every other type, so what travels is what makes that id resolve to something a reader
recognises. `What it does` and `Where it lives` are both short, and together they say which deployable this is and where
its code sits. `repos` travels beside them as a list, because a service whose content is published from one repository
and served from another has two answers to that question.

**A `repos` entry is a bare name, and the corpus resolves it.** Every id in this framework is a bare key, and a
repository name follows that design: `repos-base` in [`.corpus.yaml`](../corpus-descriptor.md#repos-base) states the
host and the organisation once, and an entry appends to it. Backstage, OpsLevel and Cortex each do the same, stating
the provider in an annotation key, a sibling `provider:` or the nesting level, and leaving the value a bare
`org/repo`. [purl](https://ecma-international.org/wp-content/uploads/ECMA-427_1st_edition_december_2025.pdf) addresses
a repository as `pkg:git/<host>/<owner>/<repo>` and answers the estate spread over several hosts, which none of the
corpora here has. It was declined because `feature-file-repo` reads the first segment of a `feature-files` path as a
repository name, and for `pkg:git/x/y` that segment is `pkg:git`.

`Where it lives` keeps its link, so the repository is stated twice: once as a join key and once as a URL a person
follows with the path inside the repository beside it. `mirrors-repo-links` is what keeps the two in step. It builds
`<repos-base>/<entry>`, reconciles the resolved set against the links under the heading in both directions, and warns
where they disagree. A warning rather than an error, because a service can sit outside the estate's usual host, and an
error would make one such service drop `repos-base` for every other record. A corpus stating no base runs no check.

**`depends-on` is the estate's own graph, and the reason this type exports at all.** A reader holding the catalogue
walks the edges without opening a record. `Dependencies` travels beside it, because an edge says which service and the
prose says what the call is for. The edges run one way, downward. A service records what it calls, and the reverse view
is a question a reader asks of the whole graph, so nothing here has to keep a second field in step with the first.

`data-stores` travels as ids. Where the corpus adopted `data`, those records travel too, so a reader can follow one.
`Data` says in prose what this service does with each store. `component-type`, `platform`, `criticality` and `facets`
are the keys an agent filters on, and the first two draw their range from the corpus. An estate lists its own
deployables, groups them once by what they are and once by the runtime and framework a contributor has to learn, and
closes each list on what it found. So one schema above several estates can state no range at all.

**`interfaces` points at a contract, and never copies one.** Backstage's `spec.definition` embeds the contract text in
the catalogue, so the address it was fetched from does not survive. A corpus is a document, so this field states that
address instead, and it travels beside `Where it lives`, which already publishes a repository URL. The value is a whole
URL because nothing derives one: `repos` states a bare repository name, and
[OpenAPI 3.1.1](https://spec.openapis.org/oas/v3.1.1.html) recommends a filename and prescribes no path. The field is
optional on every service, because a machine-readable format exists for an HTTP API and an event stream and not for a
command line tool or a web page. `component-type` is the wrong axis for a `required-when`: an estate spells that range
itself, so a condition naming `api` never fires for an estate that writes `http-api`. `type` draws its range from the
estate for the same reason. No rule checks that a contract is there, because `kac` reads one corpus and cannot open a
source repository.

**`monitoring-output` states a commitment, and never a rota.** Backstage, OpsLevel and Cortex each keep a paging tool's
id in the same file and dereference it through that tool's API. A corpus is a document, so the same id gives a reader a
dead string. This field says what a live problem raises instead: `alert` if a person acts now, `ticket` if the system
files one and a person acts later, `log` if it is recorded and nobody reads it, and `none` if nothing is emitted. The
range is shared in `.schema/_enums.yaml`, so no estate can drop `none` from a list of its own.
`critical-service-is-alerted` warns where a `live` or `deprecated` service graded `critical` states `none`. It carries
the same status guard the field does, so stating `none` on a draft is never worse than leaving the key out.

`Environments` stays behind as a table of addresses a reader holding a copy cannot use. `Operational notes` stays behind
with it, because both describe running the thing rather than depending on it.

### data

Every section but `Related` travels. A service's `data-stores` points at these records, so a reader following one of
those ids reads the document whole.

`classification`, `personal-data`, `data-subjects`, `retention` and `region` are the keys a consumer filters on.
`Classification` says why the data takes that grade, and what the domain excludes. `Retention` says how
deletion is carried out, and whether anybody has checked that it runs. A reader taking the frontmatter value alone
gets neither.

`flows-to` resolves against services and integrations, so a reader can walk from a data domain to the systems that
receive it. `Flows` travels beside it, because it states what each recipient gets and where they process it. The ids
say neither.

`Purpose` states the lawful basis beside the use it supports. `Entities` names the domain's objects in the estate's
own words.

`Related` stays behind as navigation. A consumer follows the record's own `links`.

**A document here contains no actual data.** `no-actual-data` fails an email address outside `example.com`, so what
travels is an inventory about data and never the data itself.

**`data` requires `review-by`, and `services`, `integrations` and `offerings` do not.** A data document's
`classification`, `personal-data`, `retention` and `region` are claims about a store, and nothing in the corpus checks
one. A domain that gained a column of special-category data still reads `personal` until a person opens the record. A
`criticality` and a `their-sla` go stale as quietly, and what separates them is the source. The [ICO][ico-storage]
asks you to review retained personal data regularly and to justify how often. [NIST SP 800-53r5][nist-80053] RA-2 asks
that a categorisation be revisited so it stays accurate. ISO/IEC 27002:2022 5.12 asks the same, read here through a
secondary summary because the standard is paywalled. None of the three sets an interval, so each document sets its own
date.

### integrations

`What it does`, `Failure modes` and `Exit` travel. A consumer asks which external systems this estate calls, what
breaks when one is down, and how hard one would be to replace. Those three sections answer all of it.

`Contract`, `Trial criteria`, `Commercials` and `Contacts` stay behind. An endpoint, a renewal date and a support line
serve whoever owns the account. `Contract` also states where a credential is kept, which nobody outside the estate
needs. `Trial criteria` is an evaluation in progress, and a consumer reads `status` instead.

`their-sla` travels in the vendor's own words, because `constraint-consistency` on `nfrs` compares an availability
target against it. `replaces` and `successor` travel as a pair, so a consumer holding an old citation reaches the
system that took the traffic.

**`Exit` is required of a `critical` or `important` integration still in use.** `exit-required` reports one that has
none. Article 30(3)(f) of [DORA](https://eur-lex.europa.eu/eli/reg/2022/2554/oj) asks an ICT contract for an exit
strategy, and the fact a record is missing is rarely the notice period: it is the wording, the history or the
identities the vendor keeps, which no contract lists. A `retired` integration is excluded, because leaving is already
done, and a `supporting` one because nothing a customer feels depends on it. A `trial` integration is asked like any
other: it is the one most likely to be left.

**Data protection is `data`'s, not this type's.** Article 30(2) of DORA puts data location and personal data in the
vendor contract. Here `data` states them, through `region`, `personal-data` and `flows-to`, which resolves against an
integration id. An integration record repeats neither.

### offerings

An offering answers what the estate offers a customer, which is the question a stranger asks first. Every section
travels, because there is nothing in one to trim: `hub-not-specification` weighs the whole record against its outbound
links and keeps it short.

**The consumer group decides where one offering ends and the next begins.** `Who it is for` is required, and it names
that group. ITIL 4 defines a service offering the same way, and the type takes the definition from there. It does not
take ITIL's other tests: two groups served in two different ways are two offerings here, however much they share
underneath. `spans-more-than-one-service` warns where one service delivers the whole of an offering, because a record
that names one service restates the service record beside it.

**`nfrs` is required once an offering is `live`.** A customer already has it, and no other field says how well it has
to work. A corpus that declined `nfrs` is not asked, and adopting the type starts the obligation with no edit to
`.schema/offerings.yaml`.

**`Where the detail lives` is the only place the work items are written.** The type declares no field for them: a
tracker reference is a link a reader follows, not a value an agent filters on, and one estate's `ADO#1150` is another's
`gh#2101`. The section travels at `full`, so a consumer reads the labels. It does not read the addresses: a section's
link definitions sit outside it and `Exporter.Body` drops them, which is the same thing that turns a record id into a
bare id a consumer looks up. A work item has nothing to look it up in, and an address into another estate's tracker
would not open for them anyway.

`implemented-by` and `nfrs` travel as ids, and both declare `mirrors-section:` against that same list. The record
states each id twice, in the frontmatter an agent reads and in the list a person follows, and
`related-matches-section` reports either end naming one the other does not.

`feature-files` travels as written. A path names its repository first, so a consumer can tell which repository a test
sits in without resolving the rest, and `feature-file-repo` holds that first segment to a repository one of the
implementing services names.

### tools

A tool register says what an estate builds on, which is the question an inventory exists to answer. An inventory nobody
outside the team can read proves nothing to the people who ask for one.

`packages` is what makes the register an inventory. CycloneDX identifies a component by package URL and SPDX by an
external reference of the same form, so one entry per package is what a manifest can be matched against. The version
range sits on the package rather than on the record, because a family chosen together is often pinned apart:
`Spectre.Console` and `Spectre.Console.Cli` are one decision at two version ranges.

`licence` and `Licence and obligations` travel together. The identifier says which licence, and the section says what
that licence obliges you to. A reader acts on the second. `homepage` travels beside them. SPDX leaves a home page
optional and makes a download location mandatory instead, and this register asks for the home page: the first question
the OpenSSF evaluation guide puts is whether you have the project you think you have, rather than a fork of it.

`decided-on` and `review-by` are the two dates an approval needs. `decided-on` says when the current stance was
settled. `review-by` expires it, and `review-in-date` warns once the day has passed. Without the second, an approval
taken once governs new work for ever.

`Status` is optional and says why the tool sits where it does, which `status` and `decided-on` cannot.
`exit-states-a-reason` asks for it once a tool is `deprecated` or `rejected`, because those are the stances that owe
the next person an explanation. It travels, where `Alternatives considered` stays behind, so a consumer of the export
learns why a tool was dropped without fetching the record. `replaces` and `successor` are the same move written as
ids.

`Accessibility` travels because a corpus governing a rendered surface keeps the component's assessment here, and the
assessment is what a published accessibility statement is built from. `Where it is used` travels because it names the
records that use the tool, and those ids resolve for a reader holding them.

`Trial criteria` and `Alternatives considered` stay behind. Both record how this estate reached the decision, and
`decided-in` gives the ADR to a reader who wants that argument.

### glossary

A term travels whole: the definition, and each labelled line beneath it as a piece of its own. A definition on its own
is mostly guessable from the word.

The three labels answer three different questions, so each travels under its own key. `**Also:**` gives another name
for the same thing, `**Avoid:**` gives a name the glossary has dropped, and `**Not:**` gives a neighbouring thing the
term is confused with. A reader searching an acronym reaches the term through `also`, which the heading alone could not
give them. SKOS calls that an `altLabel` and DITA calls it a `glossAlt`.

`line:` lists the keys of one term's line and where each takes its value. The export repeats `status` and `review-by`
onto every term, because the flat file is what gets grepped and whoever grepped it has not opened the record. A copy
taken a year ago reads exactly as it did on the day it was taken, so the date is the signal a vendored export otherwise
lacks.

### explanations

**`explains` admits a standard, a process and an ADR as well as a service or an offering.** Diátaxis starts an
explanation from a why-question about a subject, and a subject is not always a deployable thing. An account of why the
testing approach is shaped as it is explains a standard. Requiring the field at all is this framework's own, and it is
what stops the residual type filling up with prose attached to nothing.

**`What this covers` says what the record leaves to another record.** The Good Docs Project's concept template opens
with a required section that fixes the scope, and Diátaxis asks an explanation to stay closely bounded. The body
between that section and `Where the detail lives` is free-form, because an explanation's shape follows its subject.

Both declared sections travel, and a consumer receives every id in either of them as a bare id, because
`Exporter.Body` drops a section's link definitions. That is the same reduction every other type gets, and the id is
what a consumer looks the record up by.

### reports

A report declares no sections, so the frontmatter is the whole of what travels and the body is one fetch away.

The frontmatter answers the one question a reader asks of a report it cannot open: whether the answer is still true.
`sources` lists each corpus the report answers for and the `content-version` it was true of, so a reader at a later
version knows the answer has moved on. `verified` lists every verification the report has had, and `trust` is the tier
derived from it.

### processes

`When to use this` travels as the trigger. `Prerequisites` travels beside it, and states the access, the tooling, the
prior process and the skills a reader needs before step 1. A reader decides from those two sections whether they may
start, before opening the record.

No field records the skills. EPA QA/G-6 is the only source this type cites that asks a procedure to state them, and one
source is not enough for a field. `Prerequisites` states them in prose, and nothing checks that a process names them.

`requires-access` stays behind, although a process declares the field. `Prerequisites` already states the access in
prose. The runbook export sends the field because it exports `Symptoms` and `Impact` alone, and neither of those states
what a reader needs to start.

### runbooks

`Symptoms` and `Impact` travel. A reader arrives holding a symptom and no id, greps the export for what they are
seeing, finds the record, then fetches it. `symptoms-first` already checks that the symptom leads. `Impact` says who is
affected, so the reader can tell without fetching anything whether this is worth waking somebody for.

`requires-tools` and `requires-access` decide whether a reader may start at all. `severity` is what an agent sorts on
where several runbooks match.

The steps stay behind. AWS splits this document in two, a playbook that finds the cause and a runbook that resolves it,
and this type is both on one page. A reader who has found the right page fetches it whole.

The desired outcome stays behind with the steps. AWS asks a runbook to state its desired outcome clearly. Here it is
the last line of `Resolution`, opening `Confirmed when`, so the reader reads it where they stop. The export says what
the failure looks like and who it affects, not what a runbook restores.

## The framework register

A policy states its alignment to an external framework clause by clause, as alignment rather than certification. Three
checks keep that arrangement true, and all three read `frameworks.md`, the corpus's own register of the frameworks it
has taken a standing against.

**`aligns-with` summarises the references a policy's clauses cite, grouped by framework.** Grouping is what keeps the
frontmatter short: a data protection policy maps to two frameworks and twenty of their references, and a flat list would
spend twenty lines saying which two. The framework is named by the label its `Alignment` cell uses, version included, so
`[ISO 27001:2022].A.8.24` in the table is `ISO 27001:2022` in the roll-up. Naming it by the `frameworks.md` anchor
instead would drop the version, which is the half a coverage claim turns on.

**`alignment-rollup` compares the roll-up against the cells in both directions.** A reference in a cell and not in the
roll-up leaves the generated index under-reporting what the policy covers. A reference in the roll-up and not in any
cell is a claim of coverage no clause can show, and that is the worse of the two, because it reads as evidence. It is an
error, where `clause-order` and `clause-compound` only warn: those two question an author's judgement, and this one
reports two copies of one fact disagreeing.

**`framework-posture` and `framework-uncited` check the register and the clauses against each other.** The register says
whether you are obliged to a framework, self-obligated to it, or borrowing from it. `alignment-rollup` reads that
standing to decide which references it carries, so a framework the register never placed would be dropped with nothing
saying so. From the other side, a register entry no clause reaches is a standing nothing acts on, and it reads to an
auditor as coverage.

Only a binding standing obliges a summary. A clause may cite a framework you took ideas from, and that citation is
provenance rather than obligation. [Lineage](../framework/lineage.md#alignment-not-compliance) says what the three
standings mean.

## Where to go next

[What the schema is held to](held-to.md) says what `kac` refuses when it reads a type file you have changed.

[ico-storage]: https://ico.org.uk/for-organisations/uk-gdpr-guidance-and-resources/data-protection-principles/a-guide-to-the-data-protection-principles/storage-limitation/
[nist-80053]: https://csrc.nist.gov/pubs/sp/800/53/r5/upd1/final
