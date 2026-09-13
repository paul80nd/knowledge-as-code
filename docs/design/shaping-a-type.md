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
you read it against.

## What each type sends

One section per type that declares an `export:` block, in tier order. A tier is the level of authority a type sits at,
and [Taxonomy](../framework/taxonomy.md#the-five-tiers) lists the five.

Six types declare no block. Only `discoveries` decided that, and `.schema/discoveries.yaml` gives its reason. For
`postmortems`, `capabilities`, `integrations`, `data` and `explanations` the block is unwritten rather than refused, and
[A type held back](export.md#a-type-held-back) says why nothing here could prove one right.

### adrs

A standard cites the decision its authority comes from, and `derived-from` resolves only where the ADR travelled too.

`Decision` travels whole, because the decision sentence is the record. `Consequences` travels whole beside it: a reader
who sees what a decision bought but not what it cost sees a better decision than the one you took. `Alternatives
Considered` stays behind. It is the longest section an ADR has, it argues against options this estate weighed, and the
record is one fetch away.

`supersedes` and `superseded-by` travel as a pair. A superseded decision still binds whatever was built under it, so a
reader meeting one needs the id that replaced it. `deciders` stays behind with `owner`.

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

### controls

A control travels whole. It runs to a few hundred words, and a reader who sees what a check promises without seeing what
it misses sees a stronger check than the one you built.

`mechanism` and `frequency` are the two keys an agent filters on. `not-enforced` travels like any other value, so a
reader learns that a rule is written and that nobody looks. Withholding those records would publish a coverage figure
this corpus cannot show. `evidence` travels beside them as the only key saying where the proof lives.

`verifies` points at a whole standard or one rule inside it, and you read the value to tell which. Where a control names
the record, it vouches for that whole document whatever it checks inside it.

### deviations

A deviation travels whole. The people who carry the risk of a departure usually sit outside the team that raised it, and
a register they cannot read surfaces nothing. This is the one type whose whole purpose is to be read by somebody else.

The five sections are the whole of the argument: what you do instead, why, what limits the risk meanwhile, what ends it,
and how far it applies. A reader who sees four of them sees a smaller departure than the one you took.

**`owner` travels, and this is the only type where it does.** A deviation names the individual who accepted the risk,
and a register that says what was excused without saying who excused it is not a register.

`accepted-on`, `review-by` and `closed-on` say whether a departure is live, overdue or over. They travel beside
`status`, which stays `active` past its review date, so you can compare the two accounts of one record. Nothing moves
the status on that date, because a departure left standing is the state worth seeing. The `expiry` check warns on it.

### nfrs

An NFR is normative, and a reader may act on it without opening anything else. The three required sections travel whole:
what the target is, how anybody tells whether it was met, and what happens where it is missed.

`target` and `measured-by` state as fields what those sections state at length. The fields are what a reader filters and
sorts on, and the sections are what somebody building against the number reads.

`Constraints` travels because a target read without its limits reads as a stronger promise than the estate made. It
states those limits in words, because `constrained-by` points at integrations and integrations travel nowhere.

`Current actual` stays behind. It is a measurement taken on the day somebody wrote it, and an export has no way to say
how old it has since become.

### fixes

`symptom-keywords` is what a lookup searches on. It is the field this type exports for.

`Symptom`, `Cause` and `Resolution` travel whole. A reader matches on the symptom, checks the cause is theirs, then
acts, so a resolution arriving without its cause is half an answer. `Why it happens` travels with them: `Cause` says
what went wrong this time, and `Why it happens` says what class it belongs to.

`verified` travels, and `trust` is derived from it. This type exists to say how far a resolution has been taken on
trust. `status` and `review-by` say whether the answer is still true, and `fixed-upstream` is the value that earns them
both.

`How we found it` stays behind. The route is often the more reusable half of a fix, and it is reusable to somebody who
can run the commands it lists. A reader holding a copy can run none of them. `promoted-from` stays behind too, because
it points at a discovery, and discoveries travel nowhere.

### services

A service id is cited from every other type, so what travels is what makes that id resolve to something a reader
recognises. `What it does` and `Where it lives` are both short, and together they say which deployable this is and where
its code sits.

**`depends-on` is the estate's own graph, and the reason this type exports at all.** A reader holding the catalogue
walks the edges without opening a record. `Dependencies` travels beside it, because an edge says which service and the
prose says what the call is for. The edges run one way, downward. A service records what it calls, and the reverse view
is a question a reader asks of the whole graph, so nothing here has to keep a second field in step with the first.

`data-stores` travels as ids a reader cannot follow, because data records travel nowhere. `Data` is what that reader
reads instead. `platform`, `criticality` and `facets` are the three keys an agent filters on. Of the three, only
`platform` draws its range from the corpus. An estate lists its own deployables, groups them by the runtime and the
framework a contributor has to learn, and closes the list on what it found, so one schema above several estates can
state no range at all.

`Environments` stays behind as a table of addresses a reader holding a copy cannot use. `Operational notes` stays behind
with it, because both describe running the thing rather than depending on it.

### tools

A tool register says what an estate builds on, which is the question an inventory exists to answer. An inventory nobody
outside the team can read proves nothing to the people who ask for one.

`licence` and `Licence and obligations` travel together. The identifier says which licence, and the section says what
that licence obliges you to. A reader acts on the second.

`Status` says whether the tool is in use, on trial or on its way out, and `replaces` and `successor` are the same move
written as ids. `Where it is used` travels because it names the records that use the tool, and those ids resolve for a
reader holding them.

`Trial criteria` and `Alternatives considered` stay behind. Both record how this estate reached the decision, and
`decided-in` gives the ADR to a reader who wants that argument.

### glossary

A term travels whole: the definition, and the `**Not:**` line beneath it as a second piece. A definition on its own is
mostly guessable from the word.

`line:` lists the keys of one term's line and where each takes its value. The export repeats `status` and `review-by`
onto every term, because the flat file is what gets grepped and whoever grepped it has not opened the record. A copy
taken a year ago reads exactly as it did on the day it was taken, so the date is the signal a vendored export otherwise
lacks.

### reports

A report declares no sections, so the frontmatter is the whole of what travels and the body is one fetch away.

The frontmatter answers the one question a reader asks of a report it cannot open: whether the answer is still true.
`sources` lists each corpus the report answers for and the `content-version` it was true of, so a reader at a later
version knows the answer has moved on. `verified` lists every verification the report has had, and `trust` is the tier
derived from it.

### processes

`When to use this` travels as the trigger. `Prerequisites` travels beside it, because the access a step needs decides
whether a reader may start at all, and that is a question asked before the record is opened.

### runbooks

`Symptoms` is the one section that travels. A reader arrives holding a symptom and no id, greps the export for what they
are seeing, finds the record, then fetches it. `symptoms-first` already checks that.

`requires-access` decides whether a reader may start at all. `severity` is what an agent sorts on where several runbooks
match.

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
