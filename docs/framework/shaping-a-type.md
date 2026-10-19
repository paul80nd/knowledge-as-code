# Shaping a type

Each type the framework ships has a shape. A type is one kind of record, and a record is one Markdown document with YAML
frontmatter above its prose. A type's shape is the fields in that frontmatter, the sections in the body, and the rules
`kac` checks it against. This page takes the types one at a time. Each section says what a record of that type is for,
then why its fields and sections serve that purpose. Read it when you write a record and want to know what a section
asks of you, or why a field is required.

A corpus is one repository of knowledge records kept in git. An export is the machine-readable copy `kac export` writes
for another corpus to read.

[The default types](types.md) introduces every type in one line, and [Lineage](lineage.md) says where each one came
from. Each section below closes on the one sentence about the export you need as an author: what a reader elsewhere
sees of your record without opening it. [The export format](../reference/export.md) states the rest, key by key.

## What every type decides the same way

These decisions repeat across the set, so they are stated here once.

**`owner` names who keeps a record true, and stays in its corpus.** It is a fact about this corpus's stewardship, and a
reader holding a copy elsewhere cannot ask that person anything. Every type that exports keeps it out, except
[deviations](#deviations).

**A procedure's steps are one sequence.** Processes and runbooks are followed whole and in order. Neither declares its
steps as parts, the pieces of a record a reader can cite on their own. A step lifted out of its sequence is one a reader
can act on alone, and acting on it alone is how an incident gets worse.

**`last-rehearsed` and `rehearsal-frequency` go together.** A date says nothing about staleness without the cadence you
read it against, so the two travel as a pair.

**A type page states its level of detail and cadence in prose.** The section says what one step, rule or part
contains, how often a record changes, and how often new ones arrive. It guides a writer and whoever triages a finding,
so no field declares it and no rule reads it.

## The types

One section per type, in tier order. A tier is the level of authority a type sits at, and
[Taxonomy](taxonomy.md#the-four-tiers) lists the four.

### adrs

An ADR (architecture decision record) records a decision that affects more than one repository, and the reasoning behind
it. Its decision never changes after it is accepted. A later ADR supersedes it instead, and a standard cites the ADR
its authority comes from in `derived-from`.

`Decision` states the choice, and that sentence is the record. `Consequences` is required beside it: a reader who sees
what a decision bought but not what it cost sees a better decision than the one you took. `Context` opens on the problem
the decision answered. `Alternatives Considered` argues against the options the estate weighed, and is usually the
longest section.

`supersedes` and `superseded-by` are a pair. A superseded decision still binds whatever was built under it, so a reader
meeting one needs the id that replaced it.

**A decision states its constraint, and cites a descriptive record only for background.** A descriptive record, such as
a service or a glossary, changes as the estate does. An ADR is not rewritten once accepted. `decision-cites-descriptive`
warns where a link in `Decision` or `Consequences` lands on a record of a descriptive type. A link in any other section
passes. The section stands in for why the link is there, so the check is a warning.

**In the export:** `Decision` and `Consequences` travel whole and `Context` as its opening paragraph, while
`Alternatives Considered` and `deciders` stay behind.

### postmortems

A postmortem records what happened during one incident, so that the estate, and anybody facing the same failure, can
learn from it. Once published, it is corrected in place wherever a fact in it is wrong, because a fair account
is worth more than an unchanged one.

The sections follow the order a reader asks their questions in:

* `Summary` lets a reader decide in fifteen seconds whether the rest is theirs.
* `Impact` states what customers lost in its opening paragraph, and measures that loss against the estate's own
  non-functional requirements below it.
* `Resolution` says what ended the impairment, which a reader facing the same one needs first.
* `Root cause` and `Contributing factors` are what a reader can act on, and most of the improvement lives in the
  factors.
* `Timeline` and `Actions` record this estate's own clocks, alerts and work items.

**The three lessons each take a heading of their own.** `What went well`, `What went wrong` and `Where we got lucky` are
each asked for, and a bare one is refused, so a record states what worked as well as what failed.

**Three moments fix the incident, each as a timestamp.** `occurred-at` is when the impairment began, `detected-at` when
somebody first knew, and `restored-at` when service was back for users. Each is a timestamp, because the gap between the
first two is usually measured in minutes.

**`restored-at` is recovery, not resolution.** It is the span an availability budget counts, and the one the [DORA
metrics](https://dora.dev/guides/dora-metrics-four-keys/) ask about, so an incident whose actions run for weeks still
has a `restored-at` on the day. Nothing requires it to follow `detected-at`, because an incident can recover before
anybody notices.

`duration` restates the span those moments already fix, so that an index can show it without computing one.
`duration-matches-the-moments` fails a value the moments refuse, and its message gives the value to write. `prompted`
lists the ADRs, runbooks, non-functional requirements (NFRs), fixes and standards the incident caused.

**In the export:** the three moments, `duration` and `prompted` travel with every section except `Timeline`, `Actions`
and `Related`, because `Timeline` and `Actions` name clocks and a tracker a reader elsewhere cannot open.

### policies

A policy states a commitment the estate makes: what it commits to and why, largely independent of any stack, and
changing rarely. Each commitment is a clause, a row with an id of its own, so a standard can cite the one clause it puts
into practice.

`Purpose` says why the commitment exists. `Scope` and `Exceptions` bound the clauses beneath them, because a reader
holding a clause without them reads a stricter policy than the one you wrote.

**A clause states its level as a modal.** `MUST` and `COULD` open the same shape of sentence, so a reader has to know
this type's modals to tell an obligation from a suggestion. `kac` reads the modal into a `level` key of its own, and an
agent asking what it must do filters on one value.

**A clause's `Alignment` cell maps it to an external framework.** [The framework register](#the-framework-register) says
how that mapping is checked, and what it claims.

**In the export:** each clause travels as its own entry with its `level`, beside `Scope`, `Exceptions` and the opening
paragraph of `Purpose`, and no alignment travels: [`frameworks.jsonl`](../reference/export.md#frameworksjsonl) sends
that edge from the framework's end, with the standing beside it.

### standards

A standard is the rulebook you build to. It puts a policy's clauses into practice as imperative rules, with examples
and a conformance checklist.

`Rules` has one heading per rule, and `kac validate` requires a bold modal on every bullet beneath it. `Summary` says
what the rules govern, because a rule read without it binds more than the standard does. `Conformance checklist` tests
what the rules require, and nothing else. `Examples` shows them against one stack, and `Rationale and provenance` says
why each rule exists.

**A rule states MUST, SHOULD or MAY, as BCP 14 does.** A MUST is the outcome the standard's owner answers for, and
departing from it takes a deviation. A SHOULD is the usual way to reach it, and whoever departs from it says why in the
change. A bullet may end with one short reason opening "because", so the reason travels in the export with the rule.
Every standard ends with a line saying how to depart from it, which the template writes after the `Changelog`. The
type's `lineage.divergence` says why it follows public API rulebooks here, where HCGF keeps a standard to mandatory
requirements.

**`implements` cites the policy clauses this standard puts into practice.** The citation points one way, upward. A
policy lists no standards back, and a clause no standard cites is a normal state. Each rule closes on a `Covers` line
naming the clauses it discharges, and `kac validate` checks the union of those lines equals the field.

A rule leaning on a rule in another standard cites it, because a standard composes by union. A rule citing a policy
clause writes it as `[pol-INTC].HOLDS`, and that link ends at the policy.

**`depends-on` composes standards across folders.** `platform/dotnet/testing.md` adds the .NET detail to
`common/testing.md`, and the two sit in different folders. A reader follows `depends-on` to know which further standards
they have to apply. `superseded-by` sends a reader arriving from an old citation to the standard that replaced this one.

**In the export:** each rule travels as its heading and one entry per bullet, giving the bullet's `level` and its text
in your Markdown, beside `Summary`, `Conformance checklist` and `implements`. `Examples` and `Rationale and provenance`
stay behind.

### controls

A control says how a standard's rules are verified: the mechanism, how often it runs, and the evidence it leaves. It
exists so a reader can tell a rule somebody checks from a rule that is only written down.

`What it checks`, `How it works` and `Coverage and gaps` are all required. The third is the one a reader needs most: a
check described without what it misses reads as stronger than the one you built.

**`mechanism: not-enforced` records a rule nothing checks.** It is a value like any other, so the gap is written down
and counted. `mechanism` and `frequency` are the keys an agent filters on.

Three more fields date and place the check:

* `evidence` says where the proof lives.
* `last-verified` says when a manual check last ran, so a check that happened reads apart from one that was only
  planned.
* `review-by` says when somebody reads the record against the check it describes, which is a different question from how
  often that check runs.

`verifies` points at a whole standard or one rule inside it, and you read the value to tell which. Where a control names
the record, it vouches for that whole document whatever it checks inside it.

**In the export:** the fields and the three required sections travel, `not-enforced` controls included, because
withholding them would publish a coverage figure this corpus cannot show.

### deviations

A deviation records a knowing departure from a rule, the person who accepted the risk, and the date somebody looks at it
again. It is the one type written to be read by somebody else: the people who carry the risk usually sit outside the
team that raised it.

Six sections make the argument, and a reader who sees five of them sees a smaller departure than the one you took:

* `What we are doing instead`
* `Why we need it`
* `What the risk is`
* `What compensates`, which limits that risk
* `How it closes`, which ends the departure
* `Scope`, which says how far it applies

**`owner` names the individual who accepted the risk.** A register that says what was excused without saying who
excused it is not a register. This type takes the `human:` form alone. Accepting a risk is a person putting their name
to a consequence, and a post cannot do that: whoever holds it on the day would be accepting something they never read.
`assigned-to` names whoever does the work, and admits a post.

`risk` rates what is left once what compensates is working. It is what a reader sorts a register on, and it decides how
soon `review-by` may fall.

**Three dates say whether a departure is live, overdue or over.** `accepted-on`, `review-by` and `closed-on` sit beside
`status`, so you can check the dates against the status. Nothing moves the status when `review-by` passes, because a
departure left standing is the state worth seeing. Two checks warn on it instead:

* `expiry` reports an `active` record past its date, whose acceptance is due for review.
* `draft-expiry` reports a `draft` record past its date. Nobody finished accepting that departure, and the code may
  already be taking it. A draft whose date is still ahead is silent.

A `closed` record is silent, because the departure is over.

**In the export:** every section but `Related` travels, and `owner` with them, the only type where it does.
`assigned-to` stays behind, because a consumer reads who carries the risk.

### nfrs

An NFR (a non-functional requirement) states one quality a service or an offering must meet, with how it is measured. It
exists so that whoever builds or runs the thing has a number to build against, and a stated response when the number is
missed. An NFR is normative, so a reader may act on it without opening anything else.

`Target`, `Why this number`, `How it is measured`, `What a breach costs` and `What we do about a breach` are all
required. `characteristic`, `target`, `window`, `alert-after` and `measured-by` state as fields what those sections
state at length. The fields are what a reader filters and sorts on. The sections are what somebody building against the
number reads.

**The cost of a breach and the response to one are two sections.** The SRE workbook separates an error budget from the
error budget policy that spends it. Both headings are required, so `required-section` reports one a record left out.

**`characteristic` takes 25010's subcharacteristic level.** An estate commits to availability, latency, throughput,
capacity, recovery, scalability or accuracy. It does not commit to reliability, which sits above availability and
recovery and says nothing about the other five. One record states one quality, so a service needing both a latency
target and a recovery target has two.

**`window` is required of an availability, capacity, latency or throughput target.** A rate or a percentile only means
something over a stated period. A recovery, scalability or accuracy target binds each event or each value, so it needs
none.

**`alert-after` states the alert's configured delay, and never a detection time somebody measured.** That delay is what
an estate can state before anything breaks, as [OpenSLO](https://github.com/OpenSLO/OpenSLO)'s `alertAfter` does. A
recovery target already covers how fast a fault is noticed.

**A delay claims an alert exists, and `alert-after-needs-an-alert` asks the services whether one does.** Where
`applies-to` names an offering, the check follows its `implemented-by` to the services behind it. A service stating
`monitoring-output: alert` or `ticket` satisfies the check. One service not yet obliged to state the field leaves the
whole target unjudged.

`Constraints` states the limits a target sits under, because a target read without them reads as a stronger promise
than the estate made. It states them in words, because `constrained-by` resolves only where the corpus adopted
`integrations`. A corpus that did not still has the cap, and writes it here.

**`Current actual` is required.** A target read without the reading beside it says nothing about where the estate
stands.

**`status: aspirational` says nobody is held to the target.** The SRE workbook keeps an aspirational objective measured
and tracked, and exempts it from the error budget policy. Here that exemption is `What we do about a breach` saying
nobody is paged, and `status` is what a reader sees first.

**In the export:** the fields and every section travel except `Current actual` and `Related`. `Current actual` is a
reading taken on the day somebody wrote it, and an export cannot say how old it has since become.

### fixes

A fix records a problem somebody has solved, with a resolution somebody has verified. It exists so the next person to
meet the symptom finds the answer before they spend an hour on the cause.

`symptom-keywords` is what a lookup searches on. The sections follow the order a reader works in. They match on
`Symptom`, check that the `Environment` and the `Cause` are theirs, then act on the `Resolution`. `Why it happens` is
optional, and says what class of problem this one belongs to.

**`Environment` is required because `applies-to` takes service ids alone.** A fix about a laptop, an editor or a pinned
tool version has no service to name, and a reader still has to match the versions.

**`verified` says how far a resolution has been taken on trust.** It lists who checked the resolution and when, and
`kac` derives `trust` from it. `status` and `review-by` say whether the answer is still true. `fixed-upstream` says the
cause is gone, and keeps the record for whoever searches for the symptom.

**`status: draft` is the value an author writes before anybody checks the resolution.** `verified` is required of every
other status and not of a draft, because an agent writing a fix from a filed observation cannot name who will verify
it. A corpus can withhold its drafts from the export, so no unchecked resolution leaves it.

`How we found it` is optional. The route is often the more reusable half of a fix, to somebody who can run the commands
it lists.

**In the export:** `symptom-keywords`, `verified` and the five sections from `Symptom` to `Why it happens` travel, and
`How we found it` stays behind, because a reader holding a copy can run none of its commands.

### services

A service record describes one deployable component: what it does, where its code lives, what it calls and what data it
keeps. Every other type cites service ids, so the record exists to make each id resolve to something a reader
recognises.

`What it does` and `Where it lives` are both short, and together they say which deployable this is and where its code
sits. `Environments` is required, and lists the addresses the service runs at. `Operational notes` describes running it.

**`repos` lists bare repository names, and the corpus resolves them.** A service whose content is published from one
repository and served from another has two answers to where its code lives, so the field is a list. Every id in this
framework is a bare key, and a repository name follows that design: `repos-base` in
[`.corpus.yaml`](../use/corpus-descriptor.md#repos-base) states the host and the organisation once, and an entry appends
to it. Backstage, OpsLevel and Cortex each do the same, leaving the value a bare `org/repo`.

`Where it lives` keeps its link, so the repository is stated twice: once as a join key and once as a URL a person
follows. `mirrors-repo-links` builds `<repos-base>/<entry>`, compares the result against the links under the heading in
both directions, and warns where they disagree, since a service can sit outside the estate's usual host.

**`depends-on` is the estate's own graph.** A reader walks the edges without opening a record. The edges run one way,
downward: a service records what it calls, and the reverse view is a question asked of the whole graph.

**`Dependencies` states the coupling, and `depends-on` states the calls.** `Dependencies` may name an id the field
omits, on purpose. It names a coupling a reader expects to find there, and says why that coupling is not an edge: a
message read off a bus, or a build that never calls the service it builds. So `depends-on` declares no
`mirrors-section:`, which would report every one of those.

**`criticality` is ranked, and the graph is where the ranking is read.** `critical` sits above `important`, which sits
above `supporting`. `ordered: true` on the enum in `.schema/_enums.yaml` declares that the first value listed is the
most significant. `dependency-criticality` then warns where a service outranks a service its `depends-on` cites. The
edge is often a degradation the estate accepts, and the warning asks the record to say so.

`data-stores` lists the data records this service keeps, and `Data` says in prose what it does with each.
`component-type` and `platform` draw their range from the corpus. An estate lists its own deployables and the runtimes a
contributor has to learn, so one schema above several estates can state no range at all.

**`interfaces` points at a contract, and never copies one.** The value is a whole URL, because nothing derives one:
[OpenAPI 3.1.1](https://spec.openapis.org/oas/v3.1.1.html) prescribes no path. The field is optional on every service,
because a command line tool or a web page has no machine-readable contract. Each interface's `type` draws its range from
the estate. No rule checks that a contract is there, because `kac` cannot open a source repository.

**`monitoring-output` states a commitment, and never a rota.** A paging tool's id tells a reader of a corpus nothing,
so this field says what a live problem raises instead:

| Value    | What a live problem raises                         |
|----------|----------------------------------------------------|
| `alert`  | A person acts now                                  |
| `ticket` | The system files one, and a person acts later      |
| `log`    | A record nobody reads                              |
| `none`   | Nothing                                            |

The range is shared in `.schema/_enums.yaml`, so no estate can drop `none` from a list of its own.
`critical-service-is-alerted` warns where a `live` or `deprecated` service graded `critical` states `none`. It has the
same status guard as the field, so stating `none` on a draft is never worse than leaving the key out.

**In the export:** the frontmatter travels, `depends-on` included, which is the reason this type exports at all, with
`What it does`, `Where it lives`, `Dependencies` and `Data`. `Environments` and `Operational notes` stay behind, because
both describe running the service.

### data

A data record describes one data domain: which service owns it, how sensitive it is, how long it is kept, and where
personal data flows. It exists so a reader following a service's `data-stores` finds a statement they can act on.

`classification`, `personal-data`, `data-subjects`, `retention` and `region` are the keys a reader filters on. Each has
a section that qualifies it. `Classification` says why the data takes that grade, and what the domain excludes.
`Retention` says how deletion is carried out, and whether anybody has checked that it runs. A reader taking the
frontmatter value alone gets neither.

`flows-to` resolves against services and integrations, so a reader can walk from a data domain to the systems that
receive it. `Flows` states what each recipient gets and where they process it. `Purpose` states the lawful basis beside
the use it supports. `Entities` names the domain's objects in the estate's own words.

**A record here contains no actual data.** `no-actual-data` fails an email address outside `example.com`.

**`data` requires `review-by`, and `services`, `integrations` and `offerings` do not.** A data record's
`classification`, `personal-data`, `retention` and `region` are claims about a store, and nothing in the corpus checks
one. The [ICO][ico-storage] asks you to review retained personal data regularly, and [NIST SP 800-53r5][nist-80053]
RA-2 asks that a categorisation be revisited. Neither sets an interval, so each record sets its own date.

**In the export:** every section travels except `Related`, which is navigation a reader replaces with the record's own
links.

### integrations

An integration records an external system the estate depends on: what it does for you, how it fails, what the vendor
promises, and how you would leave. It exists so a reader knows what breaks when a vendor is down, and how hard that
vendor would be to replace.

`What it does`, `Contract` and `Failure modes` are required. `Contract` states the endpoint, the authentication and
where the credential is kept. `Trial criteria`, `Commercials` and `Contacts` are optional, and serve whoever owns the
account.

`their-sla` states the vendor's promise in the vendor's own words. A reader setting a target of their own reads what the
vendor promised, and a paraphrase drops the window and the measurement point. `replaces` and `successor` are a pair, so
a reader holding an old citation finds the system that took the traffic.

**`Exit` is required of a `critical` or `important` integration still in use.** `exit-required` warns on one that has
none. Article 30(3)(f) of the EU's [Digital Operational Resilience Act](https://eur-lex.europa.eu/eli/reg/2022/2554/oj)
asks a contract for such a service to state an exit strategy. A `retired` integration is excluded, because leaving is
already done, and a `supporting` one, because nothing a customer feels depends on it. A `trial` integration is asked
like any other: it is the one most likely to be left.

**Data protection belongs to `data`.** Article 30(2) of the same Act puts data location and personal data in the vendor
contract. Here a data record states them, through `region`, `personal-data` and `flows-to`, which resolves against an
integration id. An integration record repeats neither.

**In the export:** `What it does`, `Failure modes` and `Exit` travel. `Contract`, `Trial criteria`, `Commercials` and
`Contacts` stay behind, because they describe owning the account.

### offerings

An offering records what the estate offers a customer and why, as a hub linking to what implements, tests and
constrains it. It is what a stranger asks about first. `hub-not-specification` weighs the record against its
outbound links, which keeps it short.

**The consumer group decides where one offering ends and the next begins.** `Who it is for` is required, and it names
that group, as ITIL 4 defines a service offering. Two groups served in two different ways are two offerings, however
much they share underneath. `spans-more-than-one-service` warns where one service delivers the whole of an offering,
because a record that names one service restates the service record beside it.

**`nfrs` is required once an offering is `live`.** A customer already has it, and no other field says how well it has
to work. A corpus that declined `nfrs` is not asked, and adopting the type starts the obligation with no edit to
`.schema/offerings.yaml`.

**`Where the detail lives` is the only place the work items are written.** The type declares no field for them: a
tracker reference is a link a reader follows, not a value an agent filters on, and one estate's `ADO#1150` is another's
`gh#2101`.

`implemented-by` and `nfrs` each declare `mirrors-section:` against that list. The record states each id twice, in the
frontmatter an agent reads and in the list a person follows, and `related-matches-section` reports either end naming one
the other does not.

`feature-files` lists the test files that specify the offering. A path names its repository first, so a reader can tell
which repository a test sits in without resolving the rest. `feature-file-repo` checks that first segment against the
repositories the implementing services name.

**In the export:** every section travels. A reader sees each work item's label in `Where the detail lives` and not its
address, which would not open outside the estate anyway.

### tools

A tool record is one entry in the approved-software register: what the estate chose, rejected or deprecated, and the
version ranges it stands behind. A register says what an estate builds on, which is the question an inventory exists to
answer.

**`packages` is what makes the register an inventory.** CycloneDX identifies a component by package URL and SPDX by an
external reference of the same form, so one entry per package is what a manifest can be matched against. The version
range sits on the package, because a family chosen together is often pinned apart: `Spectre.Console` and
`Spectre.Console.Cli` are one decision at two version ranges.

**`licence` and `licence-declared` split what a person concluded from what the package states about itself.** SPDX
splits it the same way, at `PackageLicenseConcluded` and `PackageLicenseDeclared`.

`licence` is required and accepts `NOASSERTION`. A register that can leave the field out hides an entry nobody screened,
and `NOASSERTION` shows one. `licence-declared` keeps a string SPDX has no expression for, so npm's
`SEE LICENSE IN README.md` is recorded as written. That string is the evidence for why the entry needed a conclusion at
all. `Licence and obligations` says what the licence obliges you to, and `conclusion-states-a-reason` asks for it where
the two values differ.

`homepage` is required, so a reader can tell the project from a fork of it, the first question the OpenSSF evaluation
guide puts.

**`decided-on` and `review-by` are the two dates an approval needs.** `decided-on` says when the current stance was
settled. `review-by` expires it, and `review-in-date` warns once the day has passed. Without the second, an approval
taken once governs new work for ever.

`Status` is optional, and says why the tool sits where it does, which `status` and `decided-on` cannot.
`exit-states-a-reason` asks for it once a tool is `deprecated` or `rejected`, because those are the stances that owe the
next person an explanation. `replaces` and `successor` are the same move written as ids. `Trial criteria` and
`Alternatives considered` record how the estate reached the decision, and `decided-in` cites the ADR that argued it.

`Accessibility` keeps the assessment of a component that renders a surface, which a published accessibility statement
is built from. `Where it is used` lists the records that use the tool.

**In the export:** the frontmatter travels with `What we use it for`, `Status`, `Accessibility`,
`Licence and obligations` and `Where it is used`. `Trial criteria` and `Alternatives considered` stay behind.

### glossary

A glossary defines the terms whose meaning is specific to the estate, or easily confused. It exists so a reader, or an
agent, takes a word in the sense the estate means. `Scope` says which context the glossary admits, so a contributor can
decide whether a new word belongs. Each term is an H3 under `Terms`. `narrows` names the more general glossary this one
narrows.

A term opens on its definition. A definition on its own is mostly guessable from the word, so up to three labelled
lines follow it, each answering a different question:

| Label        | What it gives                                    |
|--------------|--------------------------------------------------|
| `**Also:**`  | Another name for the same thing                  |
| `**Avoid:**` | A name the glossary has dropped                  |
| `**Not:**`   | A neighbouring thing the term is confused with   |

A reader searching an acronym finds the term through `Also`, which the heading alone could not give them. SKOS calls
that an `altLabel` and DITA calls it a `glossAlt`.

**A redefinition links the entry it departs from.** Where two glossaries in one corpus define the same term,
`redefinition-unlinked` warns unless one of the entries links to the other at the term's anchor. A glossary every corpus
shares cannot link a record only one corpus has, so the rule asks for one link. It reports against the glossary further
down its `narrows` chain.

**In the export:** each term travels whole as its own entry, each labelled line under its own key, with the record's
`status` and `review-by` repeated on every term. A copy taken a year ago reads as it did on the day, so the date on each
grepped line is the signal a reader otherwise lacks.

### explanations

An explanation is narrative that helps you understand how something works, or why it is shaped the way it is. It is the
residual type, for prose that fits no other.

**`explains` admits a standard, a process and an ADR as well as a service or an offering.** Diátaxis starts an
explanation from a why-question about a subject, and a subject is not always a deployable thing. An account of why the
testing approach is shaped as it is explains a standard. Requiring the field at all is this framework's own, and it is
what stops the residual type filling up with prose attached to nothing.

**`What this covers` says what the record leaves to another record.** The Good Docs Project's concept template opens
with a required section that fixes the scope, and Diátaxis asks an explanation to stay closely bounded. The body between
that section and `Where the detail lives` is free-form, because an explanation's shape follows its subject.

**In the export:** both declared sections travel, and every record a reader can cite in them arrives as a bare id.

### reports

A report answers one question about the corpus over the whole of it, with the judgement a person added. `kac report`
writes the headings the question needs, so the type declares no sections and the body is free-form.

The frontmatter answers the one question a reader asks of a report: whether the answer is still true. `sources` lists
each corpus the report answers for and the `content-version` it was true of, so a reader at a later version knows the
answer has moved on. `verified` lists every verification the report has had, and `trust` is the tier `kac` derives from
it.

**In the export:** the frontmatter alone travels, and a reader opens the record for the body.

### processes

A process is a planned procedure followed at a desk: releasing, onboarding, provisioning, rotating a secret. It exists
so the task is carried out the same way each time, by whoever picks it up.

Five sections are required, in the order a reader uses them: `When to use this`, `Prerequisites`, `Steps`,
`Verification` and `If it goes wrong`. A reader decides from the first two whether they may start, before step 1.
`Prerequisites` states the access, the tooling, the prior process and the skills a reader needs.

No field records the skills. `Prerequisites` states them in prose, and nothing checks that a process names them.

**In the export:** `When to use this`, `Prerequisites` and the rehearsal dates travel. The steps stay behind, and so
does `requires-access`, because `Prerequisites` already states the access in prose.

### runbooks

A runbook is an incident-time procedure, read under pressure: terse, imperative, and structured as a decision tree. It
exists so whoever is paged can restore service without working out the steps.

**`Symptoms` comes first.** A reader arrives holding a symptom and no id, and `symptoms-first` fails a runbook that
opens on anything else. `Impact` says who is affected, so the reader can tell whether this is worth waking somebody for.

The required sections after them are `Immediate actions`, `Diagnosis`, `Resolution`, `Escalation` and `Communication`,
so one page both finds the cause and resolves it. The desired outcome is the last line of `Resolution`, opening
`Confirmed when`, so the reader reads it where they stop.

`requires-tools` and `requires-access` decide whether a reader may start at all. `severity` is what an agent sorts on
where several runbooks match. `rehearsal-method` says which kind of walk the last rehearsal was.

**In the export:** `Symptoms`, `Impact`, the tools, the access, the severity and the rehearsal dates travel. The steps
and `rehearsal-method` stay behind, because a reader who has found the right runbook fetches it whole.

## The framework register

A policy states its alignment to an external framework clause by clause. The alignment is a mapping, not a
certification, and four checks keep it true. `alignment-unlinked` asks whether a cell's citation is a link. The other
three read `frameworks.md`, the corpus's own register of the frameworks it has taken a standing against.

**`aligns-with` summarises the references a policy's clauses cite, grouped by framework.** Grouping is what keeps the
frontmatter short. The framework is named by the label its `Alignment` cell uses, version included, so
`[ISO 27001:2022].A.8.24` in the table is `ISO 27001:2022` in the roll-up.

**`alignment-rollup` compares the roll-up against the cells in both directions.** A reference in a cell and not in the
roll-up leaves the generated index under-reporting what the policy covers. A reference in the roll-up and not in any
cell is a claim of coverage no clause can show, which reads as evidence, so it is an error.

**`alignment-unlinked` reports a citation Markdown never made into a link.** `[UK GDPR].Art.5(1)(e)` with no definition
behind it stays on the page as text. The cell then states no mapping, and `alignment-rollup`, `framework-posture` and
`framework-uncited` each see a clause that cites nothing. While the definition is missing, `alignment-rollup` does not
report `aligns-with` as claiming that framework.

**`framework-posture` and `framework-uncited` check the register and the clauses against each other.** The register says
whether you are obliged to a framework, self-obligated to it, or borrowing from it. `alignment-rollup` reads that
standing to decide which references it includes. A register entry no clause cites is a standing nothing acts on, and
reads to an auditor as coverage.

Only a binding standing obliges a summary. A clause may cite a framework you took ideas from, and that citation is
provenance rather than obligation. [Lineage](lineage.md#alignment-not-compliance) says what the three standings mean.

## Where to go next

[Contributing](contributing.md) says how a record you have written is reviewed and merged.

[ico-storage]: https://ico.org.uk/for-organisations/uk-gdpr-guidance-and-resources/data-protection-principles/a-guide-to-the-data-protection-principles/storage-limitation/
[nist-80053]: https://csrc.nist.gov/pubs/sp/800/53/r5/upd1/final
