# Changelog

> What changed in each published version of `kac`.

This page covers the tool, published to nuget.org as
[`KnowledgeAsCode.Tool`](https://www.nuget.org/packages/KnowledgeAsCode.Tool). The same repository holds the schema, the
framework's documentation and the pages a corpus starts from. Those travel as a template with a version of its own,
which `manifest.yaml` declares and `kac new` stamps into every corpus it creates. A change there is recorded here where
somebody running `kac` can observe it, and nowhere otherwise.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Versions sit below 1.0.0 while the command
surface may still change shape.

A push to `main` publishes whenever `kac.csproj` names a version nuget.org does not already hold, and that publish tags
the commit and opens a release carrying the section for that version. A change lands its entry under `## Unreleased`
first, and whoever owns the branch decides whether it ships now or waits for the rest of what it belongs to.

## Unreleased

### Added

- **A policy in force says the day it came into force.** `active-from` takes the day, quoted, and `kac validate`
  requires it where `status` is `active`. NIST SP 800-53 Rev. 5's `-1` controls and ISO/IEC 27001 clause 5.2 both ask a
  policy to record its approval, and `owner` says who without saying when. `review-not-before-active` fails a
  `review-by` earlier than it. The date stays out of the export, and `policy-lookup` lists it among what stayed
  behind. `kac new` sends a `_template.md` carrying the field, and a corpus already holding an `active` policy owes
  the date at its next `kac update`.

- **An integration says how the estate would leave it.** `Exit` is a section, and `exit-required` warns where a
  `critical` or `important` integration still in use has none. Article 30(3)(f) of DORA asks an ICT contract for an
  exit strategy, and the section travels in the export. `kac new` sends a `_template.md` naming it.

- **A retired integration names what took over.** `successor` takes an integration id, `replaces` is the same edge
  read the other way, and `kac validate` checks both ends. `retired-has-successor` warns where a `retired` integration
  names none. Both fields travel in the export.

- **An integration on trial says what would end the trial.** `Trial criteria` is a section, and
  `trial-criteria-required` warns where a `trial` integration has none. The section stays out of the export, because
  a consumer reads `status`.

- **A control that never stops says so.** `frequency` takes `continuous`. A control whose `mechanism` is
  `runtime-alert` has to carry a frequency, and none of `per-pr` through `annual` is true of an alert rule that
  evaluates without stopping.

- **A manual control says when it last ran.** `last-verified` takes the day, quoted, or `"never"`, and `kac validate`
  requires it where `mechanism` is `manual`. An automated check leaves a run in its own logs, and a periodic human
  check leaves nothing otherwise. It travels in the export, and `kac new` sends a `_template.md` naming it.

- **A data document says whether it holds personal data.** `personal-data` takes `none`, `personal` or
  `special-category`, and `kac validate` requires it. `retention` is now required from this field rather than from
  `classification`, so data that is confidential and personal is asked for a retention period.

- **A data document says who the data is about.** `data-subjects` takes the categories of people, in the corpus's own
  words, and `kac validate` requires it where `personal-data` is anything but `none`. GDPR Article 30(1)(c) asks for
  the categories of data subjects, and the NIST Privacy Framework asks the same as ID.IM-P3.

- **A data document says where the data is kept.** `region` takes a cloud region or a place, and `kac validate`
  requires it. The `Where it lives` table asked for it and nothing read the answer.

- **A data document says what the data is for.** `Purpose` is a required section. GDPR Article 30(1)(b) asks for the
  purposes of the processing, and the NIST Privacy Framework asks the same as ID.IM-P5.

- **The `Flows` table says where each recipient processes the data.** `kac new` sends a `_template.md` with the
  column. `region` covers the owning service, so a transfer out of the country is recorded beside the recipient that
  makes it, which is what GDPR Article 30(1)(e) asks about.

- **A deviation rates the risk it leaves.** `risk` takes `high`, `medium` or `low`, and `kac validate` requires it.
  The rating is the risk left once what compensates is working, and `high-risk-review-window` warns where a `high`
  rating sets `review-by` more than six months after `accepted-on`.

- **A deviation says what can still go wrong.** `What the risk is` is a required section, and it travels in the
  export. The sections beside it said what limits the risk and never what remains of it.

- **A deviation can name who does the work.** `assigned-to` takes a person or a post, and stays out of the export.
  `owner` accepts the risk and this field closes it, which are one person in a small estate and two in a large one.

- **A rule can measure a gap in days.** `days('a', 'b')` counts whole days between two date fields, where `span()`
  answers in hours between two timestamps. `docs/design/expressions.md` is the reference.

- **An explanation says what it covers.** `What this covers` is a required section, and it travels in the export. It
  states what the record covers and what it leaves to another record, which is what keeps the residual type bounded.
  `kac validate` reports `sections` against an explanation without it, and `kac new` sends a `_template.md` carrying
  it.

- **A fix says what you were running.** `Environment` is a required section, and it travels in the export. It states
  the tool, the operating system and the versions, which is how a reader tells whether the fix is theirs.
  `applies-to` takes service ids alone, and a fix about a laptop or a pinned tool version has no service to name.
  `kac validate` reports `sections` against a fix without it, and `kac new` sends a `_template.md` carrying it.

- **A rule can ask whether a key is written once.** `entries_unique('field', 'key')` reads one key across the objects
  a field holds and answers false where two of them state the same value. True where the field is absent.
  `docs/design/expressions.md` is the reference.

- **An NFR says which quality it commits to.** `characteristic` is required, and takes `accuracy`, `availability`,
  `capacity`, `latency`, `recovery`, `scalability` or `throughput`. The values sit at ISO/IEC 25010's
  subcharacteristic level, so a corpus can list the qualities it has promised nothing about. It travels in the export,
  indexes beside the target, and `kac new` sends a `_template.md` naming it.

- **An NFR states the period its figure is read over.** `window` takes the period, such as `monthly` or
  `rolling 4 weeks`. `kac validate` requires it where `characteristic` is `availability`, `capacity`, `latency` or
  `throughput`, because a rate or a percentile only means something over a stated period. A recovery, scalability or
  accuracy target binds each event or each value, so it needs none. It travels in the export.

- **An NFR says why the figure is that one.** `Why this number` is a required section, and it travels in the export.
  It states what fixes the figure and what the target leaves out, which the three records in `examples/payments` were
  each stating inside `Target`. `kac validate` reports `sections` against an NFR without it.

- **An agreed NFR says when it was agreed.** `agreed-on` takes the day, quoted, and `kac validate` requires it where
  `status` is `agreed`. An agreed target is a commitment somebody accepted, and nothing recorded when. It travels in
  the export.

- **A process says when it is rehearsed again.** `rehearsal-frequency` is required, and it takes a new `on-change`
  beside `per-release`, `quarterly` and `annual`. ComplianceForge HCGF reviews a procedure when its technology, its
  steps or its people change. NIST SP 800-53 Rev. 5's `-1` controls ask for a frequency and the events beside it.
  Nothing running on a schedule can measure `on-change`, so `process-lookup` tells a reader to ask what has moved.
  `kac new` sends a `_template.md` carrying the field. A corpus already holding a process writes a value into every
  one of them, because `kac update` reaches the schema and never a record.

- **A process says how to back out.** `If it goes wrong` is a required section, where it was optional. The type page
  already promised a rollback, and the section stays out of the export, because a reader who has to back out is
  holding the record. `kac new` sends a `_template.md` naming it.

### Changed

- **`policies` states what it takes from current practice.** `lineage` was measured against ComplianceForge HCGF's
  policy layer, NIST SP 800-12 Rev. 1 and NIST SP 800-53 Rev. 5. `alignment` said a policy pushes its mandatory
  language down to the standard beneath it, and 206 of the 243 clauses in `examples/engineering` are **MUST** or
  **MUST NOT**. `alignment`, `divergence` and `collision` now describe a policy that binds in its own right, and
  `kac generate` writes all three into every adopting corpus's `knowledge-as-code/lineage.md`.

- **`target` on an NFR no longer carries the measurement window.** The window is `window`, so a consumer reads the
  figure and the period apart. `nfrs` publishes at `export.version` 2, because a reader that took the period out of
  `target` would now find none there.

- **`nfrs` states what it takes from current practice.** `lineage` was measured against ISO/IEC 25010:2023 and the
  Google SRE workbook's *Implementing SLOs*. `alignment` names the four things the workbook asks an objective for, and
  `divergence` names the error budget it keeps and this type does not take. `kac generate` writes both into every
  adopting corpus's `knowledge-as-code/lineage.md`.

- **A glossary term carries the other names it answers to.** `**Also:**` gives a second name for the same thing, an
  acronym or an abbreviation, and `**Avoid:**` gives a name the glossary has dropped. Both travel in the export as
  `also` and `avoid`, so a search reaches a term by a name its heading does not carry. SKOS calls the first an
  `altLabel` and DITA `glossentry` calls it a `glossAlt`, and neither had anywhere to go here before.

- **`parts.asides:` replaces `parts.aside:`, and takes a list.** A type declares every bold label its parts may open a
  block with, and `export.parts.line:` addresses one as `part.aside.<Label>`. `part.aside` on its own is gone.
  Against a schema still naming either, `kac validate` reports `schema-unknown-key` for the block key and
  `schema-dispatch` for the line source. It reports `schema-shape` against a label the type's `parts.asides:` does not
  declare.

- **`glossary` states what it takes from current practice.** `lineage` was measured against Evans, *Domain-Driven
  Design*, SKOS and DITA `glossentry`. `prior-art` now names all three, and `alignment` and `divergence` say what the
  type takes from each and where it parts from them. `kac generate` writes both into every adopting corpus's
  `knowledge-as-code/lineage.md`.

- **`manual-periodic` is now `manual`.** The value held a cadence inside a method name, where `frequency` states the
  cadence beside it. `kac validate` reports `enum` against a control still carrying the old value, and a corpus
  rewrites it by hand.

- **`classification` grades confidentiality alone.** The field takes `public`, `internal` and `confidential`.
  `personal` and `special-category` move to `personal-data`, because a category of data is not a grade of sensitivity.
  `kac validate` reports `enum` against a data document still carrying either value in `classification`, and a corpus
  rewrites it by hand.

- **`data` states what it takes from current practice.** `lineage` was measured against GDPR Article 30, the NIST
  Privacy Framework v1.0 and ISO/IEC 27002:2022. `alignment` claimed transfers and security measures, which the type
  asks for nowhere. Both now name what the type takes and where it parts from them. `kac generate` writes both into
  every adopting corpus's `knowledge-as-code/lineage.md`.

- **`controls` states what it takes from current practice.** `lineage` was measured against NIST SP 800-53A Rev. 5,
  the OSCAL assessment models and the CIS Controls Assessment Specification. `alignment` and `divergence` now name
  what the type took from each and where it parts from them. `kac generate` writes both into every adopting corpus's
  `knowledge-as-code/lineage.md`.

- **A deviation's `owner` is a person, never a post.** The field takes `human:alex.doe` alone, where every other type
  also admits `role:`. One individual accepts a risk and a post cannot. `kac validate` reports `field-pattern`
  against a deviation still naming a role, and `assigned-to` is where a post belongs.

- **`deviations` states what it takes from current practice.** `lineage` was measured against NIST SP 800-37 Rev. 2,
  the PCI DSS v4.0 compensating controls worksheet and the FedRAMP POA&M template. `alignment` claimed a bounded
  period and tracked remediation, which the type asks for nowhere. Both now name what the type takes and where it
  parts from them. `kac generate` writes both into every adopting corpus's `knowledge-as-code/lineage.md`.

- **An explanation can explain a standard, a process or an ADR.** `explains` took a service or an offering alone, and
  a subject is not always a deployable thing. The export's `shapeVersion` for the type moves from 1 to 2, because a
  reader that resolved every id in the services or offerings folder would now be wrong.

- **`explanations` states what it takes from current practice.** `lineage` was measured against Diátaxis, the Good
  Docs Project's concept template and arc42. `alignment` claimed discursive prose and weighed alternatives, which the
  type asks for nowhere. Both now name what the type takes and where it parts from them. `kac generate` writes both
  into every adopting corpus's `knowledge-as-code/lineage.md`.

- **`fixes` states what it takes from current practice.** `lineage` was measured against the KCS v6 article, the
  ServiceNow known error form and ITIL Problem Management. `alignment` names the four sections KCS asks for, and
  `divergence` names the workaround ITIL expects and this type does not take. `kac generate` writes both into every
  adopting corpus's `knowledge-as-code/lineage.md`.

- **`verified` says who has checked a record, not how often.** An actor is written once, and a check they make again
  moves the `at` on the entry they have. The Open Knowledge Format keeps the list for independent checks, a human
  sign-off beside a nightly process, and git already keeps when each actor checked before. `kac validate` reports
  `one-verification-per-actor` against a `fix` or a `report` naming one actor twice, and a corpus trims each list by
  hand. The trust tier reads the actors and never the count, so no tier moves.

- **`processes` states its lineage against both the sources it declares.** `alignment` names what the type takes from
  the Diátaxis how-to guide as well as from ComplianceForge HCGF. `divergence` records that this type writes for a
  reader who has never done the task, where Diátaxis writes for a competent one. A `collision:` entry names what ITIL
  and BPMN mean by *process*. All three render into `knowledge-as-code/lineage.md`.

### Fixed

- **`meta/type.schema.json` offers `part.citations.<Label>`.** The editor's copy of the line-source vocabulary never
  had a pattern for it, so it marked `standards.yaml`'s `covers: part.citations.Covers` as invalid while the build
  stayed green. `MetaSchemaTests` now holds that enum to the list the exporter fills.

## 0.28.0 - 2026-09-17

### Added

- **`spans-more-than-one-service` warns where one service delivers the whole of an offering.** An offering naming one
  service in `implemented-by` restates the service record beside it. The rule is guarded on the field, so a corpus that
  declined `services` is not warned about one it cannot fill.

- **A rule expression can count a field's entries.** `entries('implemented-by')` answers how many, where
  `present()` answers whether. A scalar counts as one, and an absent field as none, so a rule that must not fire on an
  absent field guards with `present()` first. `min-items:` states the same floor as an error. Use the fact where the
  shortfall is worth a warning and not a failure. `docs/design/expressions.md` lists it.

- **An integration travels to a consumer.** `.schema/integrations.yaml` declares an `export:` block, so `kac export`
  writes a file per integration record. `What it does` and `Failure modes` travel at `full`. `Contract`, `Commercials`
  and `Contacts` stay behind, because each describes owning the account. `docs/design/export.md` now lists `data` as the
  one type declaring no block.

- **An explanation travels to a consumer.** `.schema/explanations.yaml` declares an `export:` block, so `kac export`
  writes a file per explanation record. `Where the detail lives` is the one section it declares, so it is the only one
  that travels, at `full`. The rest of the body is free-form and no schema can name it. `docs/design/export.md` no
  longer lists `explanations` among the types declaring no block.

- **A postmortem travels to a consumer.** `.schema/postmortems.yaml` declares an `export:` block, so `kac export`
  writes a file per postmortem record. `Summary`, `Root cause`, `Contributing factors` and `What went well` travel at
  `full`, and `Impact` as its opening paragraph. `Timeline` and `Actions` stay behind, because a timeline names one
  estate's clocks and each action links a work item the reader cannot open. `docs/design/export.md` no longer lists
  `postmortems` among the types declaring no block.

- **An offering travels to a consumer.** `.schema/offerings.yaml` declares an `export:` block, so `kac export`
  writes a file per offering record. Every section travels at `full`, because `Where the detail lives` is the only place
  an offering states its work items and the rest of the record is short by design. `docs/design/export.md` no longer
  lists `offerings` among the types declaring no block.

- **`feature-file-repo` warns where a feature file names a repository no implementing service does.** A path in
  `feature-files` starts with its repository, spelled as that repository's service spells `repo:`. The check compares
  that first segment against the services in `implemented-by` and names the ones it found. It warns rather than fails,
  because a regression pack can live in a repository no service claims. Nothing reads the rest of the path until
  `feature-file-orphans` runs.

- **A service names the NFRs that bind it.** `.schema/services.yaml` declares `nfrs:`, and it travels in the services
  export. `nfrs.applies-to` and both `nfrs:` fields declare `reciprocal:`, so an NFR and the record it binds each name
  the other, and `validate` reports either end that does not.

### Changed

- **An offering says who it is for.** `Who it is for` is a required section, and it travels in the export. ITIL 4
  defines a service offering by the consumer group it serves, and that group is what decides where one offering ends and
  the next begins. The `Why it exists` guidance in each `_template.md` no longer asks for the audience, because the new
  section holds it. An existing offering gains one heading.

- **An offering that is `live` states an NFR.** `nfrs` is required once the status reaches `live`: a customer already
  has the offering, and nothing else on the record says how well it has to work. A corpus that declined
  `nfrs` is not asked, and adopting the type starts the obligation with no edit to `.schema/offerings.yaml`.

- **`offerings` names its prior art.** `lineage` said "None that fits" and left `alignment` and `divergence` empty. It
  now names ITIL 4 Foundation 2.3.2, the service offering, and states what the type takes from ITIL, the GOV.UK Service
  Manual and Backstage, and where it parts from each. Both values render into every adopting corpus's
  `knowledge-as-code/lineage.md`. `docs/framework/lineage.md` records that ITIL is paywalled, beside the rows that
  already were.

- **The `capability` type is now `offering`.** A record lands in `offerings/` as `ofr-borrowing`, the page beside it is
  `offerings.md`, and the identity line reads `Offering:`. ITIL 4 calls this document a service offering and defines it
  by the consumer group it serves, which is what this type always meant. `Capability` meant something else to two of its
  likely readers: ArchiMate uses it for an ability an organisation possesses, and SAFe for functionality below an epic.
  A corpus that adopted `capabilities` renames the folder and the page. It changes each record's
  `type:`, `id:` and identity line, and rewrites every `cap-` reference in the records of other types. It then writes
  `offerings` over `capabilities` in `types:` and deletes `.schema/capabilities.yaml` by hand.
  `kac update --drop-type capabilities`
  refuses that last step, because the template no longer declares the name. The export is `offerings@1`, so a consumer
  sees `capabilities` stop and `offerings` start. The template version moves to 18, and `kac new` stamps
  `template-version: 18`.

- **`kac validate` no longer asks for a field no record in the corpus can fill.** A field whose `ref:` names only types
  nothing there supplies is dropped from the required pass, and a `required-when:` on such a field never fires. A type
  an import publishes counts as supplied, so a standard citing a producer's policy clause is still asked for in a corpus
  adopting no `policies` of its own. Adopting one of those types starts the obligation with no edit to
  `.schema/`. A field with `allow-literal:` is fillable without them, so it is still asked for. `ref-resolves` is
  unchanged: a value a record does write is held to the same standard as before.

- **A postmortem records what ended the incident, and all three of the lessons.** `Resolution` and two further sections,
  `What went wrong` and `Where we got lucky`, join `What went well`, and all five travel in the export. Google SRE
  groups the three lessons under one `Lessons Learned` heading; each is declared on its own here, so
  `required-section` asks for it and `empty-section` refuses a bare one. An existing postmortem gains three headings.

- **A postmortem may name more than one root cause.** The template said "Resist listing several". Google SRE writes
  `Root causes` in the plural and PagerDuty records contributing factors and no root cause at all, so the guidance now
  says to name more than one where more than one stands out. Nothing about the section changes.

- **A postmortem states when service came back, and `duration` is checked against it.** `restored-at` is a third
  timestamp, required once a postmortem is published, and `duration` is now an ISO 8601 duration such as `PT4H20M`.
  `duration-matches-the-moments` fails a value the two moments refuse, and its message carries the span they give, so
  the fix is a paste. `restored-not-before-occurred` fails service coming back before it went. Nothing requires
  `restored-at` to follow `detected-at`: an incident can recover before anybody notices it.

- **A rule expression can ask for the time between two moments.** `span('occurred-at', 'restored-at')` answers with an
  ISO 8601 duration in hours, minutes and seconds, and with nothing where either field is absent, is not a moment, or
  where the second is the earlier. `docs/design/expressions.md` carries it in the table of what an expression may call.

- **A postmortem states when an incident began and when it was noticed, to the second.** `occurred-on` and
  `detected-on` are now `occurred-at` and `detected-at`, and each takes a UTC timestamp as `2026-09-07T20:18:00Z`. The
  gap between the pair is what the pair is for, and it is usually measured in minutes. `-on` names a date everywhere
  else in the schema, so the names moved with the type. A corpus holding postmortem records renames both keys and writes
  a time into each: `validate` reports the old spelling as `unknown-key`.

- **`Where the detail lives` and the frontmatter say the same thing, and `validate` checks it.** `implemented-by` and
  `nfrs` declare `mirrors-section: Where the detail lives`, so `related-matches-section` reports either end naming an id
  the other does not. That section is a bulleted list rather than a headerless table, which is the form `services`
  already uses for the same shape and the one a screen reader can read. It lost its `Tested by` line, which restated
  `feature-files` and had already drifted from it, and its `Decided in` line, which no field ever backed.

- **`kac new` seeds an offering template and type page that name no tracker.** Both said functional detail lives in
  Azure DevOps epics. They now describe a work item and leave the tracker to the corpus. Inside the corpus the template
  links only to `services`, so one that adopted neither `adrs` nor `nfrs` no longer receives a definition into a folder
  it does not have. The type page also states the floor the type has: an offering whose
  `implemented-by` names one service is a synonym for that service. Both files seed, so an existing corpus keeps the
  wording it was created with.

- **A report's `generated.at` dates the content's last meaningful change.** `.schema/reports.yaml` described it as
  "the moment the content last changed", dropping the qualifier [OKF v0.2] states, while the framework moves the stamp
  for neither a hand-raised `sources` version nor the `Imported:` bullet an imported entry's raise rewrites. The
  description and the field's `notes:` are read by whoever maintains a corpus's schema, and `kac update` takes both
  down. No generated block changes: the `schema-reports` table prints the description of `generated` itself and none of
  its entries.

- **`kac new` seeds report guidance the schema accepts.** `reports.md` told you to verify a report you had just written,
  which `no-self-verification` rejects. It now says to write yourself into `generated.by`, leave `verified`
  empty, and ask somebody else to read it. Both it and `reports/_template.md` also say which `sources` entry a
  hand-raise touches: the corpus the report answers for comes first and its raise edits frontmatter alone, and raising
  an imported entry also means editing the `Imported:` bullet under `## Limits` and adding a `verified` entry. The files
  seed, so an existing corpus keeps the wording it was created with.

- **`kac new` seeds the example policy as `policies/devi-deviations.md`.** The file was
  `devi-deviations-are-recorded.md`, which restated the record's title. A policy filename names what the policy is, and
  the H1 states the intent. `.schema/policies.yaml` and the `policies.md` type page both state the rule. The file seeds,
  so an existing corpus keeps the name it was created with. The template version moves to 17, and `kac new`
  stamps `template-version: 17`.

### Removed

- **`ado-epics` is gone from `offerings`, and with it the `int` value type.** A work item id assumed one tracker, and a
  corpus planning on GitHub issues had nowhere to put the equivalent. Work items are now links in the
  `Where the detail lives` list, labelled the way the corpus's own tracker labels them. `ado-epics` was the only field
  in the taxonomy declared `of: int`, so `int-format` guarded nothing and `type: int` and `of: int` are no longer values
  a schema may declare. `kac checks` prints one check fewer.

### Fixed

- **An index column heading spells an initialism in capitals.** `kac generate` headed the `their-sla` column Their sla,
  because `id` was the only field name it read as an initialism. That column now heads Their SLA. Every other field
  heads a column in sentence case, as it did.

## 0.27.0 - 2026-09-16

### Added

- **The documentation site lists every skill, under [Skills](https://paul80nd.github.io/knowledge-as-code/skills/).**
  Sixteen of them across three trees, and which tree a skill lives in decides who can read it and what moving it costs.
  The three tables are generated from the files that already decide the split: `plugin.json` for what travels inside a
  plugin, `manifest.yaml` for what travels into a corpus, and the `.claude/skills/` directory for what stays here.
  `SkillReferenceTests` fails a stale table, so the page cannot drift from the manifests the way a hand-written one
  would. Nothing about `kac` changed.

- **`kac new` and `kac update` send a corpus the `writing-a-report` skill.** It joins `technical-writing` and
  `writing-a-record` in the overlay, under one rule: a writing skill travels where it governs a surface a corpus holds.
  A corpus can adopt `reports`, so the skill that says how to fill a report's judgement cells travels with it.
  `writing-in-the-tool` and `writing-the-docs` describe C# and a documentation site no corpus has, and `i-want-to`
  routes to this repository's own processes where a corpus reads its own through `process-lookup`, so all three stay
  behind. `reports/_template.md` gains the four verdicts as well, for an author who opens the template and loads no
  skill. Before this the words `Covered`, `Covered by its pair`, `Gap` and `Out of scope` were stated in one file that
  never left this repository, so a corpus adopting `reports` was told to answer the judgement cells and nowhere told
  with what. `manifest.yaml` moves to 15, and `kac update` stamps `upstream.template-version`.

- **`kac new` and `kac update` send a corpus the `harvest-findings` skill, which triages filed findings and drafts the
  record one asks for.** `raise-finding` files an observation as an issue and stops, so nothing moves it afterwards.
  `triage` reads every finding without a `kac:triaged` label, sorts each into one of five routes, shows a person the
  table and writes nothing until they agree. `draft <issue>` takes a `kac:route-record` finding and opens a pull request
  carrying the record. Both invocations write: to the tracker, and to the records themselves. So the reader is the
  corpus's own maintainer, and the skill lands in the corpus's working tree under `.claude/skills/`, beside the writing
  skills. No plugin ships it, because a consumer holds a frozen export and not the source. It reads the
  `tracker`, `framework` and `publishing` blocks of `.corpus.yaml` for the backlog, for where the tool is reported and
  for where the record lives. A framework finding filed on a tracker other than the framework's own is drafted as a
  comment and copied by hand, because an Azure DevOps user has no GitHub account. `manifest.yaml` moves to 16, and
  `kac update` stamps `upstream.template-version`.

- **`kac validate` refuses a target the descriptor names and the tool cannot act on**, under a new
  `descriptor-target` check. `publishing-target` takes the five values the link rules are written for. `tracker.target`
  and `framework.target` take `github`, `azure-devops` or `none`, because a wiki and a documentation site publish
  records and hold no backlog. The message names the key, the value and the list it takes. Only `kac new` held its flags
  to these lists before, and every one of the keys is written by hand after that, so a misspelling read as a corpus that
  publishes nowhere and files nowhere.

- **`.corpus.yaml` states where work about its records is filed, under a new `tracker:` key.** It takes a `target` and a
  `base`, the same pair `framework:` takes. A corpus that states no block gets the tracker its `publishing:` block
  implies, so a corpus on GitHub configures nothing: a repository there has an issue list of its own. State the block
  where the backlog and the published form are two places. One Azure DevOps project holds one backlog and many
  repositories, so a repository URL does not address the backlog, and `kac export` derives the project from it.
  `manifest.json` states a tracker three times: for the corpus, for the framework, and for every entry in `sources`.
  Each one gains an `id`, which is the target and the base normalised, so a caller comparing two of them compares two
  strings and never parses a URL. `base` and `id` are both `null` wherever a block addresses no backlog, so a base is
  never written beside a target that cannot use it. `descriptor-version` moves to 4, and `kac update` stamps it. No
  skill reads the key yet.

- **`.corpus.yaml` states where to report a problem with the framework, under a new `framework:` key.** It takes a
  `target` and a `base`, the same pair `publishing-target` and `publishing.base` take. `kac export` writes the block
  into `manifest.json`, beside the `id` every tracker there gains, so the address travels to whoever installs the corpus
  as a plugin. Its `base` is written the way every tracker's is, so an Azure DevOps base is the project holding the
  backlog. Before this, the only address an export carried was the corpus's own `publishing.base`.
  `upstream.url` was never one: it says where the template was copied from, and that is often a folder. `kac new`
  writes the block, taking the base from `--from` where that names a repository and from the framework's own repository
  where it names a folder. `descriptor-version` moves to 3, and `kac update` stamps it. No skill reads the key yet.

- **A `fix` takes `status: draft`, and a draft states no verification.** `verified` is required of every other status,
  so a session can write a fix from an observation nobody has checked and `kac validate` passes. The type's export shape
  moves to `fixes@2`, because `verified` arrives as null on a draft. A corpus listing `draft` under
  `export.exclude:` in `.corpus.yaml` withholds a draft, which is how an unchecked resolution stays out of
  `fix-lookup`.

- **`fix-lookup`, the skill that says whether a problem has already been solved here.** It searches
  `symptom-keywords`, which a fix over-fills with error text and the words somebody arrives with, then reports the
  Symptom, Cause and Resolution the corpus settled. It states the record's derived `trust` on every answer, so a caller
  can tell a resolution a person checked from one an agent ran. A fix declares no part, so the skill searches
  `corpus/fixes/` and the record is the unit. It ships in every corpus's `plugin.json` and leaves the bundle wherever
  the export carries no fix. Take it with `kac update --from <template>`.

- **`raise-finding` searches the fixes before it files anything about a problem.** Seven sessions meeting the same
  problem should not open seven issues. Where the plugin has no `fix-lookup`, the corpus declined the `fixes` type and
  the finding is the only route. Take it with `kac update --from <template>`.

### Changed

- **A verification taken before `generated.at` reaches no trust tier.** `kac export` derived `trust` from `verified`
  alone, so a report somebody read in March still shipped as `human-reviewed` after an agent rewrote its verdicts in
  September. Authorship now passes to whoever answers those cells, which moves `generated.at` past every reading of the
  words before them, so the exporter leaves an older entry out. A moment that will not parse still counts, because
  `timestamp-format` already reports it against the record.

- **A report's `generated` says who wrote the content, and authorship passes to whoever edits it.** `kac report` wrote
  itself into `generated.by` and left it there, so a report an agent finished credited the tool with the agent's
  verdicts. `by` now names whoever wrote the content a reader meets, which the Open Knowledge Format defines it as and
  illustrates with a person. Two keys survive the handover: `report` states the report the run used, as `coverage`, so a
  record says which `kac report` name regenerates it, and `tool` keeps the version that wrote the mechanical half. Both
  are optional, and a report nobody produced with the tool states neither. `generated-by-a-producer` is renamed
  `generated-by-a-known-actor` and admits all three actor forms, so `human:alex.doe` is now a legal author.

- **A report states no verification until somebody reads it.** `verified` was required with at least one entry, so an
  agent finishing a report had to name a verifier to get past `kac validate`, and the two worked corpora each named an
  agent that does not exist. The field now follows `fixes`: a draft states none, and every other status states one. A
  report with no entry is the Open Knowledge Format's unverified tier, which `kac export` already ships as `trust`.

- **An accepted ADR is edited in place, and only a changed decision needs a superseding ADR.**
  `immutable-after-accepted` allowed a typo fix, a link correction and a status transition, and nothing else. That list
  left out an edit changing no decision. A sentence that no longer matched the decision the ADR already stated read as
  forbidden. The rule now asks whether the decision changed, and `adrs.md` says to name the edit in the commit message.
  `immutable-after-published` takes the same shape for a postmortem, where a new understanding is a new postmortem. Both
  are still declared and do not run, because telling a changed decision from a correction needs git history. The Decided
  tier note in `_tiers.yaml` drops "never rewritten" for the same reason, so a corpus's
  `knowledge-as-code/taxonomy.md` changes when you regenerate it. `kac update --from <template>` takes all three, and
  `kac generate` rewrites the blocks under them. `adrs.md` and `postmortems.md` seed a corpus, so the Immutability
  paragraph on each stays that corpus's own to reword.

- **`kac export` reports every reason it refuses in one run.** A type the corpus has not adopted, an `export-exclude`
  key it cannot act on, a consumed corpus nothing is restored for, a consumed corpus at another export format and two
  corpora disagreeing about a type are each one line, and all of them print before the run stops. A shape or fidelity
  line now ends with what to do about it. Before, the first of them stopped the run on its own.

- **A finding `raise-finding` files is the observation, and proposes no record.** Its `kac-finding` block drops `id`
  and `expires`, which were a proposed discovery's own fields, and gains `looks-like`. That key takes the type the
  observation resembles, `framework` where what was noticed is the tool, the schema or a skill, or `none`. Whoever
  triages the issue gets the hint and keeps the decision. `corpus`, `source`, `confidence`, `provenance`,
  `applies-to` and `tags` are unchanged, and so are the body's three headings. Take it with
  `kac update --from <template>`.

### Removed

- **The `discoveries` type and the `observed` tier.** A corpus no longer keeps an unverified observation. One goes into
  whatever tracker that corpus already uses, and what the corpus keeps is the answer somebody settled and verified.
  `kac new` writes one type fewer, and `kac validate` counts one template fewer. `fixes` and `standards`
  drop `promoted-from`, so a record that still has that key fails `unknown-key`; `sources` states where the content came
  from instead. A `schema-shape` message naming the tiers now offers four. Take it with
  `kac update --from <template>`, which deletes `discoveries.md`, the folder and the schema file.

### Fixed

- **`kac export` publishes a grandparent's record files down a chain three corpora deep.** A consumer read only the
  record files sitting directly in an imported type's directory. The records its producer had inherited were dropped,
  and their part lines reached the next corpus naming files that never arrived. `kac` now reads the folders a producer
  filed those records in, and files each record under the corpus that wrote it. A corpus consuming both a producer and
  that producer's own producer meets one record twice, and writes and counts it once. `sources` already listed the
  grandparent, and still does.

- **`kac validate` resolves a citation into a corpus reached through another.** `gp:pol-OLD` was reported as a shortcode
  this corpus consumes nothing under. The reader behind `validate` skipped the folders an export files an inherited
  record in, and filed what it did read under the corpus it arrived through. A record is now found by the shortcode of
  the corpus that wrote it, whether this corpus declared that corpus in `consumes:` or not.
  `eng:pol-OLD` correspondingly stops resolving, which is what `ref-resolves` already says about a record eng does not
  have. While a declared import is unrestored, a shortcode nothing here knows is left to `import-restored` instead of
  drawing a second finding telling the reader to declare it.

- **`kac report coverage` credits a grandparent's standard against the clause it implements.** A record file keeps the
  ids its own corpus wrote, and the edge was scoped to the corpus the record arrived through. The clause read as a gap
  and the standard covered nothing, because the clause lines are scoped to the corpus that wrote them.

## 0.26.0 - 2026-09-13

### Added

- **`kac validate` checks that a quoted clause still says what it is quoted as saying.** A control quotes the clause it
  verifies, and nothing until now compared the two. `clause-quoted-faithfully` takes every double-quoted span on a line
  that also cites a clause, and reports it as an error where the cited clause no longer contains those words. Whitespace
  is collapsed on both sides, so a quotation wrapped across two lines is read whole. A quoted span on a line citing
  nothing is left alone, and so is a citation into a corpus you consume: an export sends a record's ids and fields, not
  its wording. `.schema/controls.yaml` declares the rule, so take it with
  `kac update --from <template>`.

- **A fix travels in an export.** `.schema/fixes.yaml` declares an `export:` block at shape 1, so `kac export` writes
  one JSON per fix. The record carries `symptom-keywords`, which is what a lookup searches on. It carries Symptom,
  Cause, Resolution and Why it happens whole, because a resolution read without its cause is half an answer. It carries
  `verified`, and the record's `trust` is derived from that list. Three things stay behind: `How we found it`, which
  names commands a consumer cannot run, `promoted-from`, which names a discovery that travels nowhere, and `owner`. Take
  the schema with `kac update --from <template>`, and adopt the type with `kac update --add-type fixes`.

- **`kac report --out <path>` writes the report to a file.** Without it the report still goes to standard output, so a
  caller piping one loses nothing. With it `kac` writes the file after it has read the corpus, which is what keeps a run
  off the console encoding and out of the way of a shell holding the same path open. A path a file already occupies is
  refused and nothing is written, because a finished report holds verdicts somebody wrote.

- **Every record can say where its content came from.** `.schema/_universal.yaml` declares `sources`, an optional list
  whose entries carry a required `resource`. A `resource` names something a reader can follow, such as a ticket URL, or
  the population the content was drawn from. `sources` is what
  the [Open Knowledge Format](https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md) calls the
  same list, and
  `resource` is its one required key. No type carries the universal field into an export, so a record a consumer holds
  stands up without the ticket behind it. `reports` keeps its own `sources`, which requires the field and adds the
  `version` each corpus was at. Take the field with `kac update --from <template>`.

### Changed

- **The prose in `.schema/` is rewritten to the writing rules.** Every `description:`, `notes:`, `message:` and comment
  in the schema was rewritten against them. A record author reads a shorter field description in the `## Metadata`
  table, and a plainer sentence from `kac checks` and from a rule that fails. No check id, severity, `expr:`, pattern,
  threshold or export shape changed. Reasoning that had grown into a `notes:` now sits on the design site, under
  [Checks](https://paul80nd.github.io/knowledge-as-code/design/checks/),
  [Shaping a type](https://paul80nd.github.io/knowledge-as-code/design/shaping-a-type/) and
  [Reports](https://paul80nd.github.io/knowledge-as-code/design/reports/), and the `notes:` cites it. Run
  `kac generate` after taking the schema with `kac update --from <template>`.

- **The `## Metadata` table says what a field is, where it used to describe the schema.** A field declaring only
  `notes:` fell back to them for its table cell, so maintainer commentary was published to whoever writes a record.
  Every field now declares a `description:`. An ADR's `superseded-by` reads "The ADR that replaces this one." where it
  read "CI reconciles both directions, so a one-sided supersession fails the build." Run `kac generate` after taking the
  schema with `kac update --from <template>`.

- **Every check message opens lower case.** Twenty-three rule messages in `.schema/` opened with a capital, where
  `kac` prints a message mid-line after the check id. Several also ran to four or five sentences. Each now opens lower
  case and states what is wrong, then what to write instead.

- **`.schema/` states the reasoning local to a field and cites the site for the rest.** A `notes:` had grown into a
  multi-paragraph design argument in forty-six places, and much of it repeated a documentation page. The prose in
  `.schema/` is a third shorter. The per-type export choices and the framework-register rules now sit at
  <https://paul80nd.github.io/knowledge-as-code/design/shaping-a-type/>, which each type file cites.

- **`kac report frameworks` says more beside each framework's table.** Every reference row carries a `Citations` count,
  so a reference one clause cites reads as `1` without counting the cell next to it. Each framework's section opens on
  the standing the register files it under, linked to the register entry that placed it, which is the line a reader
  would otherwise scroll back to the totals table for.

- **Each report writes its own `## Limits`.** Both printed one wording, written for a reading of clause coverage.
  `frameworks` now says that an `Alignment` cell stays in the corpus that wrote it, so it counts the citations written
  here, and that a citation records the naming rather than a clause meeting what it cites.

- **The five lookup skills drop their search procedure.** `kac new`, `kac update` and `kac bundle` send skills that name
  no search tool and no search flags. A trial ran three variants of `policy-lookup` over five questions: the skill as it
  shipped, one without the tool name, and one without the search section at all. Every variant found and cited every
  governing clause. Each run of the shipped skill spent tool calls hunting a Grep tool the session did not hold. What
  stays is what an agent cannot work out for itself: the file map, the field table, and the warning that a field name
  like `status` matches every line of the file.

- **`glossary-lookup`, `policy-lookup` and `standards-lookup` answer the near miss.** Each carries a section for a
  subject the corpus has not written down that sits beside one it has. "Password rotation" meets a policy about rotating
  secrets, and the skill now says to name the nearest clause as the nearest one and leave the reading to its owner.

- **`confirmed` is now `verified`, it takes any actor, and the export carries the trust tier derived from it.** The
  field is renamed on `fixes` and `reports`, which is what
  the [Open Knowledge Format](https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md) calls the
  same list. It no longer refuses an agent: a session that reproduced a symptom and ran the resolution has checked
  something real, and
  `verified-by-a-known-actor` admits it, named with its version the way the tool names itself. That rule still refuses a
  `role:`, because a post cannot read an answer. Who is in the list decides the record's tier, which each record file
  now ships as `trust`: an empty list is `unverified`, agents alone are `machine-confirmed`, and one `human:` actor is
  `human-reviewed`. A type whose export does not name `verified` carries `trust` as `null`. One actor is still refused,
  and `no-self-verification` is the new rule and reports as `self-verification`: a report may not be verified by the
  producer its `generated.by` names. A fix declares `raiser-does-not-verify` and nothing runs it, because nothing on a
  fix names who raised it. `kac report` writes `verified: []` where it wrote `confirmed: []`. A corpus that adopted
  either type renames the key in every record and in its `_template.md`, and takes the new schema with
  `kac update --from <template>`. The `reports` type's `shapeVersion` moves to 2, so a consumer reading records of that
  type reads the new key.

- **The `faq` type is now `fix`, and its `Fix` section is now `Resolution`.** A record lands in `fixes/` as
  `fix-0001`, the page beside it is `fixes.md`, and `kac validate` holds the record to Symptom, Cause and Resolution.
  The type and its third section no longer share a word. A corpus that adopted `faqs` renames the folder and the page,
  changes each record's `type:`, `id:`, identity line and `Fix` heading, then writes `fixes`
  over `faqs` in `types:` and deletes `.schema/faqs.yaml` by hand. `kac update --drop-type faqs` refuses that step,
  because the template no longer declares the name. `kac update --from <template>` then takes the new schema file. The
  type's page and its `_template.md` are seeds, so a corpus keeps the copies it has, and
  `validate` names every line of them still saying FAQ.

### Fixed

- **`kac` prints UTF-8 on Windows.** Standard output took whatever code page the machine was installed with, so a clause
  citing `§9` reached the reader as a replacement character while the same text in the record was intact. Every command
  writes through the same stream. `kac report` is where it showed, because a report quotes citation text back.

- **`kac report` writes frontmatter a parser accepts.** `owner: human:` is not valid YAML, so a generated report met
  `frontmatter-parses` over the whole document rather than a message naming what to fill in. `id`, `owner` and
  `verified` now arrive empty, and `kac validate` reports `required-field` against each one.

- **`kac report` calls a fresh report a draft.** The schema requires `status` and no run wrote it, so every generated
  report failed `kac validate` until somebody added the field by hand. The frontmatter now carries `status: draft`.

- **`id-matches-filename` names the two things that disagree.** It printed the whole id where it meant the id's own
  slug, so a record filed as `reports/rpt-clause-coverage.md` was told that `rpt-clause-coverage` does not match
  `rpt-clause-coverage`. It now reads `id 'rpt-clause-coverage' carries slug 'clause-coverage', and the filename
  carries 'rpt-clause-coverage'.` The number and the mnemonic branches take the same wording.

- **An unhandled fault goes to stderr.** Spectre's own handler wrote one to stdout, so `kac report coverage > out.md`
  put the message inside the report and left the console silent. Every verb's own refusal already went to stderr, and
  this joins them.

- **The `report` page taught a filename that does not validate.** Its example wrote `reports/rpt-clause-coverage.md`,
  and a record whose filename repeats the id prefix fails `id-matches-filename`. The filename carries the question the
  report answers, and the prefix belongs to the `id`.

- **`sections` is described as the object it is.** `policy-lookup` and `standards-lookup` told a reader to read two or
  three things from `sections` without saying it is keyed by heading, and a reader parsing it as a list gets nothing
  back.

## 0.25.0 - 2026-09-09

### Added

- **Seven more types travel in an export.** `.schema/` declares an `export:` block at shape 1 on `adrs`,
  `deviations`, `nfrs`, `reports`, `runbooks`, `services` and `tools`, so `kac export` writes one JSON per record for
  each of them. `deviations` carries `owner`, because a register that says what was excused without saying who excused
  it is not a register. `runbooks` carries `Symptoms` and no steps, for the reason `processes` carries no steps.
  `discoveries` holds everything back still, and its schema file says why. A standard now carries `derived-from` too, so
  the ADR behind a rule resolves for a consumer holding both. Take them with `kac update --from <template>`.
- **`kac validate` warns where an optional field is written with no value.** `empty-optional-key` reports a bare key on
  a field the schema does not require, because it says exactly what leaving the key out says. A required field is the
  other case, and `required-field` still reports that one. A field declaring
  `required-when:` is exempt. The templates a corpus starts from now carry the required fields alone, and each one names
  the optional fields it leaves out. Take those with `kac update --from <template>`.
- **A plugin component can declare itself standalone.** `metadata.components` in `plugin.json` takes
  `"standalone": true` on a component whose `requires` is empty. `kac bundle` trims an empty-`requires` component when
  every component reading a type has gone, because such a component exists to support those. A standalone one supports
  nothing and serves the reader, so it now survives that sweep. Take it with `kac update --from <template>`.
- **`raise-finding`, the skill that files what a session noticed about a corpus.** The plugin's export is frozen, so an
  issue on the repository that published it is the only route back. The body carries the keys a `discoveries` record
  needs, and the skill asks before it files, every time. Take it with `kac update --from <template>`.
- **`request-deviation`, the skill that asks a clause's owner to accept a departure from it.** The body carries the keys
  a `deviations` record needs, and leaves `owner` and `accepted-on` to the reply, because the individual accepting the
  risk is what the request asks for. It files inside the organisation holding the plugin and nowhere else, and it asks
  before it files. Take it with `kac update --from <template>`.
- **A control travels in an export.** `.schema/controls.yaml` declares an `export:` block at shape 1, so `kac export`
  writes one JSON per control carrying `verifies`, `mechanism`, `frequency`, `evidence` and the three sections a control
  holds. A control declares no part, so no flat file is written and the record is the unit. Take it with
  `kac update --from <template>`.
- **`controls-lookup`, the skill that says what proves a rule.** It answers what checks a standard, where the evidence
  lives, and which standards nothing claims. `mechanism: not-enforced` is what makes the last of those answerable. It
  reads each `verifies` entry to see whether the control named a record or one rule inside it, and refuses a per-rule
  figure where only records were named. Take it with `kac update --from <template>`.
- **A corpus states the ranges the framework cannot know.** A field declaring `values: $corpus.<name>` in `.schema/`
  draws its range from `enums:` in `.corpus.yaml`, so one schema above several catalogues stands behind the list each of
  them wrote. `services.platform` is the first field to use it: what a service is built on is one list in a library and
  another in a payments platform. `kac validate` reports `corpus-enum-undeclared` once against `.corpus.yaml` where a
  corpus holds a record carrying such a field and has stated a range no record can satisfy, meaning none at all or one
  carrying a value that is not lower case. An out-of-range value stays an ordinary `enum` failure quoting the corpus's
  own values. `kac new` opens the block and leaves it empty. Take it with `kac update --from <template>`.

### Changed

- **`.corpus.yaml` is at descriptor format 2, and `services.platform` no longer carries a range of its own.** A corpus
  holding services states `enums.platform` in its descriptor before `kac validate` passes. Derive the values from your
  own deployables and close the list on what you found, which is what the type's page has always asked for. There is no
  migration: `kac update` stamps the format and writes no values, because only the corpus can say what its estate runs
  on.

### Fixed

- **An export field declared as an object carries its keys.** A field such as a report's `generated` reached a consumer
  as `null`, because only a list of objects was read. It now travels as an object carrying the keys its `shape:` or
  `entry:` block names.
- **`kac report` stamps the release without the commit behind it.** `generated.by` takes the Open Knowledge Format's
  `<producer>/<version>` form, and the value carried the build metadata as well, as
  `kac/0.24.0+24dcea21945982d92104c78a854465207d644ad6`. It now reads `kac/0.24.0`. A regenerated report no longer shows
  a moved commit where the tool's version stood still.
- **A discovery can be promoted to a standard.** `promoted-to` names an FAQ or a standard and declares
  `reciprocal: promoted-from`, but only `faqs` carried that field, so promoting to a standard failed `reciprocal` and
  adding the key to the standard failed `unknown-key`. `standards` now declares `promoted-from` as well, optional and
  pointing back at the discovery. Take it with `kac update --from <template>`, which brings the schema down with it.
- **`label-canonical` catches a shortcut label that leads to a record it does not name.** The check compared a label
  against the canonical spelling of its own id, so `[std-BOGUS]` defined as `../standards/workflows.md` passed:
  `link-resolves` was happy with the path, and the reader was shown an id no record carries. `kac validate` now holds a
  label to the id in the frontmatter of the record it resolves to, which reaches a label the id styles do not recognise
  at all. A template is exempt, since its definitions demonstrate the form under labels nobody has chosen yet. The row
  this check gets on a type page is reworded to match, so run `kac generate` after upgrading.

## 0.24.0 - 2026-09-08

### Added

- **`kac report <name>` prints a report over the corpus and everything it imports.** Two reports ship. `kac report
  coverage` names every policy clause and what discharges it, with the deviations departing from it, the controls behind
  each covering standard, and any clause elsewhere sharing its key. `kac report frameworks` names every external
  framework reference the clause tables cite, the standing the register files each framework under, the clauses citing
  each one, and how many rest on a single citation. Each row of both carries an empty `Note`, for whoever confirms the
  report. Output is markdown on standard output, so a caller pipes it where they want it. Every run stamps
  `generated` and `sources` into the frontmatter it writes, naming the tool version, the moment, and the
  `content-version` each corpus answered at. The tool prints `covered` and `uncovered` and never splits a gap from
  something out of scope, because only a person can tell those apart.

- **A standard's `implements:` and its `Covers` lines reach a consumer.** The record carries `implements`, and each rule
  line carries `covers`, holding the clause ids that rule discharges. A corpus inheriting the policies it answers to can
  now count its own coverage: before this, it saw what its own standards covered and nothing that arrived with the
  policies. `part.citations.<Label>` is the export source behind the rule line, and it takes the ids from the labelled
  footnote closing a part. Neither addition moves `standards@1`, because a reader written against the shape before them
  is still correct.

- **`framework-uncited` fails a framework on the register that no clause cites.** The register is the list of frameworks
  an estate has taken a standing against, so an entry nothing reaches is a standing nobody acts on, and it reads as
  coverage to whoever is looking for evidence. It is the third check `alignment-rollup` reports under. The register is
  found by following a clause's own link, so a corpus whose clauses cite nothing has none in view. A finding lands on
  the policy that reached the page and names the page the entry is deleted from.

- **`policies/frameworks.jsonl` travels in the export.** One line per external framework reference, naming the standing
  the register files it under, the clauses citing it, the policies holding those clauses, and the page and anchor the
  register entry sits at. A type names the file with `frameworks:` in its `export:` block and the exporter fills the
  keys, because a reference is read from a clause's cell and from the register the cell links to rather than from any
  field a type declares. Navigation stays one way: `clauses.jsonl` is unchanged, and a clause still carries no
  framework. Both `policies@2` and `formatVersion` stand, because a reader written against the shape before this is
  still correct.

- **`report-stale` warns where a report answers for a version the corpus has left behind.** Every other record is about
  the estate, so a corpus that moved leaves it as true as it was; a report is about the corpus, and the same change can
  make it wrong with nothing in the record showing it. Each `sources` entry is held against the version in front of the
  reader: the descriptor's own `content-version`, or the version a consumed corpus's restore resolved to. A warning,
  because the report may well still hold, and whoever owns it either confirms that and raises the version by hand or
  runs it again.

- **`reports` is a knowledge type the framework ships.** A finished report is a record: it has an owner, a person
  confirms it before it is published, and a reader browsing the corpus finds it beside everything else.
  `generated` names what produced the content and when, `sources` names each corpus it answers for and the
  `content-version` each was at, and `confirmed` names every person who has checked it since. Sections are free-form,
  because a report's headings follow the question it answers. Take it with `kac update --add-type reports`.

- **A field may hold one object, and a shared shape may say what it holds.** `type: object` declares a value that is one
  mapping, and its keys are held to their own declarations exactly as a list's object entries are. `_shapes.yaml`
  joins `_enums.yaml` as a shared block, declaring an object shape a field takes whole with `shape: <name>`. Nothing
  narrows a shape at the point of use, so a type holding one of its keys to a narrower value writes a rule. `event`, an
  actor doing something at a point in time, is the shape that ships. This moves the template to version 8, so a corpus
  takes `_shapes.yaml` with `kac update`.

- **`entries_match('field', 'key', 're')` joins the expression facts.** It reads one key inside every object a field
  holds: each entry of a list of them, and the one an `object` field holds. It is true where the field is absent and
  true where an object omits the key, because presence is `required-field`'s question and `entry-key`'s, so a rule
  written on top of it reports one fault once.

- **A whole number is a field type the tool checks.** `type: int`, and `of: int` on a list, are read by
  `int-format`: plain decimal with an optional leading sign, and within what a 64-bit number holds. A separator or a
  base prefix is refused rather than decoded, because YAML reads `1_000` and `0x1f` as numbers of its own and an author
  should not have to know which spellings the parser admits. `ado-epics` on a capability is the field this reaches, and
  its entries were checked by nothing before.

### Changed

- **An FAQ's `confirmed.by` is held to a person by a rule rather than by a pattern.** The finding moves from
  `field-pattern` on the entry's own line to `confirmed-by-a-person` against the record, and the message says why a
  post, an agent and a team alias are each refused. `confirmed` now takes the shared `event` shape, whose `by` is a
  plain string, and who may confirm is the FAQ type's own question to ask.

### Fixed

- **`kac update` writes the descriptor's upstream block with one space after each colon.** It padded every key it
  stamped to a column, and `yamllint --strict` refuses that under its `colons` rule, so a corpus running the linter its
  template ships went red on `commit`, `template-version` and `taken-on` the moment it updated. The next run repairs a
  descriptor an earlier one aligned.

- **A field's `type:` and `of:` are held to what the tool dispatches.** Either naming a value no check reads now fails
  `schema-dispatch` when the schema loads, so `type: tiemstamp` is reported rather than loading and holding the field to
  nothing. An entry key answers to the same vocabulary, to whatever depth an `entry:` block nests, because its value
  goes back through the same checks. The types are `date`, `enum`, `id`, `int`, `list`, `string` and `timestamp`; a
  list's entries are `id`, `int`, `object` and `string`. `bool` was offered by `meta/type.schema.json` and dispatched by
  nothing, as was `of: date`, and both are gone from it.

- **An `of:` on a field that is not a list is reported.** It is read from a list's entries and nowhere else, so a scalar
  carrying one states a shape its value can never take. `values:`, `min-items:` and `min-records:` were already held to
  the field type they are read against, and `of:` now joins them.

- **An unquoted placeholder in a record is reported.** YAML reads `owner: {{owner}}` as a flow mapping rather than as
  text, so the value reached no check at all and the record validated clean. `bare-key` now reports it and names the
  quoting that fixes it, and `required-field` reports the field missing where the type requires it. A template still
  reports the same spelling under `template-fields`, which answers for the documents copied from it. The two seed
  records `kac new` writes carried the mark, and now name an owner.

## 0.23.0 - 2026-09-07

### Added

- **A schema field can hold a moment.** `type: timestamp` sits beside `type: date` in a type's `fields:` block, and
  takes `2026-09-07T20:18:00Z`: UTC, to the second, unquoted. A date is a day and is written quoted, because YAML
  rereads an unquoted one as a datetime and a reader's own zone then shifts the day it shows. A `Z` instant carries its
  zone in the value, so no reread moves it. `timestamp-format` errors on a value written in another shape, and on one
  naming a moment the calendar does not have.

- **A field naming a person carries an actor prefix.** `owner` takes `human:alex.doe` for a person, or
  `role:head-of-engineering` for a post. Exactly one person holds a post, so a role keeps answerability with one human
  and survives a handover that leaves every record naming the previous holder wrong. `confirmed.by` on an FAQ and
  `deciders` on an ADR take `human:` alone: each records who performed an act, and a post cannot perform one. A bare
  name, an agent, a session id and a team alias all fail `field-pattern`, so the tier boundary between a discovery and
  an FAQ is checked rather than described. The three prefixes are [OKF v0.2]'s, whose trust tiers key off `human:` the
  same way. A corpus created before this rewrites the field in each record it holds, and
  `kac validate` names the ones still bare.

- **Every record carries its own `type`.** The universal schema requires the field, directly after `id`. Its value is
  the singular type name the record's folder declares: `standard` in `standards/`, `adr` in `adrs/`. A record read away
  from its folder therefore says what it is. `type-matches-folder` errors where the field and the folder disagree. Every
  `_template.md` carries the line, so `kac new` writes it. A corpus created before this adds the line to each record it
  holds, and `kac validate` names the ones that are missing it.

### Changed

- **An FAQ records every confirmation, rather than the last one.** `confirmed` replaces `confirmed-by` and
  `confirmed-on` with a list, one entry per confirmation and oldest first:
  `- { at: 2026-09-07T20:18:00Z, by: human:alex.doe }`. The moment and the person are one entry, so they are edited
  together and neither can be left behind. A reader asking when the answer was last checked, by whom, and who checked it
  before that now has all three. The shape is [OKF v0.2]'s. A corpus holding FAQs written before this rewrites the two
  keys as one entry per record, and `kac validate` names the ones still carrying the old pair.

## 0.22.0 - 2026-09-07

### Added

- **The framework declares a `deviations` type.** A deviation records a knowing departure from a policy or a standard:
  the clauses it departs from, the person who accepted the risk, the day they accepted it, and the day somebody looks at
  it again. `departs-from` names those clauses one by one and refuses a bare policy or standard id, because a bare id
  claims a departure from every clause the rule carries. `kac update --add-type deviations` takes it, and `kac new`
  offers it beside the rest. Three rules run over a record. `review-after-acceptance` errors where the review date falls
  on or before the acceptance date, so a deviation cannot expire as it is written. `not-open-ended` warns where the
  record reads as a standing departure rather than a bounded one. `expiry` warns where a record is still `active` on a
  day its `review-by` has gone by, and stays a warning so a late review never makes deleting the record the cheapest way
  to a green build.
- **A rule expression can call `today()`.** It answers with the day the run happens, as an ISO date, so a rule compares
  it against a date field under the string comparison the grammar already uses between two dates. The day is read once
  for the whole run, so a corpus validated across midnight cannot answer one way for its first record and another for
  its last.

### Changed

- **The template's shape is at version 7.** It carries the `deviations` page, its index and its record template, so a
  corpus running `kac update` is offered the type.

### Fixed

- **A field may name a part spelled unlike the record holding it.** `id-format` read the whole entry as one id, so
  `std-ERRORS.a-failure-says-what-happened` failed: a standard's record id carries a mnemonic and its rules are heading
  slugs. The check now reads the record and leaves the part to `ref-resolves`, which is what answers for whether the
  part exists.

## 0.21.0 - 2026-09-07

### Added

- **A corpus adopting `processes` now publishes them.** `kac export` writes one JSON per process carrying its
  frontmatter, its `When to use this` trigger and its `Prerequisites`, and stops there. `Steps` and `Verification`
  stay in the record, because a procedure is followed whole and in order against the version in force rather than
  against a copy taken on an earlier day. The type declares no parts, so nothing writes a flat file for it and
  `manifest.json` reports `partsFile`, `recordKey`, `partKey`, `idKey` and `seeAlsoKey` as null beside `parts` at zero.
- **`kac bundle` ships two more skills.** `process-lookup` finds the procedure written for a planned task and reads its
  trigger before deciding it is yours. It is trimmed where the export carries no processes. `corpus-retrieval`
  reaches a record's published source and builds a link to it, naming `gh` and `az devops invoke` as the clients that
  authenticate to each platform and saying what to do where neither reaches. The three skills that shipped before now
  hand their link building to it.

### Changed

- **A component your plugin manifest declares with an empty `requires` now travels only where a component that reads a
  type did.** It reads no export itself, so it is there to support the ones that do, and a plugin shipping it alone
  would carry a skill supporting nothing a reader can reach. `bundle.json` gives the reason as
  `no component it supports survived`, and a run that trims every component warns as it did before. A file no component
  claims is unchanged: it needs no declaration and travels whatever the corpus adopted.
- **The template's shape is at version 6.** It carries the two skills above, so a corpus running `kac update` receives
  them.

- **`kac checks` asks you for the half of a drifted checks table you hold.** Where the reader-facing table and your
  `.schema/_checks.yaml` disagree, every line of the report names `on-type-page:` in that file and says where it sits.
  The row beside it is in the table `kac` ships, which your corpus holds no copy of, so the report says whose it is
  rather than naming a source file you cannot open.

### Fixed

- **The lookup skills `kac bundle` ships now state the type of every field they describe.** `obligations`,
  `definition` and `not` are one string of markdown holding the record's bullets, and a skill calling any of them a list
  sent a reader looping over a string. A field with no value arrives as `null` beside a key that is still there, so test
  the value rather than the key. `shortcode` is the one key a line can be missing outright.

## 0.20.0 - 2026-09-01

### Added

- **`kac export` carries the corpora your corpus consumes.** Every type a producer exported travels, so a consumer
  receives types it never adopted and a citation into them resolves. Their parts merge into one flat file per type, and
  their records are filed under the shortcode of the corpus that wrote them. An inherited line carries that shortcode on
  `id`, on `record` and on every `seeAlso` value, and again under `shortcode`; a line with none is your own. The
  manifest gains `sources`, one entry per corpus inherited, each holding the publishing block its producer wrote,
  because a record of theirs is read at their commit in their repository. `kac pack` seals all of it, so a third corpus
  inherits the chain.
- **`kac export` refuses rather than writing a hole.** It stops with the reason and no files where a declared import has
  not been restored, where a consumed corpus is at an export format this build does not read, where two corpora export
  one type at different shapes or section fidelities, and where one corpus arrives twice at two versions.

### Changed

- **The breadcrumb names the corpus each count belongs to.** A merged export holds several corpora's records under one
  type, so a type now gets a line for your own records and another for each corpus you consume, reading
  `standards (from eng). 37 entries across 12 records: …`. A type you wrote none of gets no line under your own name.
  The closing line warns against answering from memory rather than naming what a word means, because a corpus may ship
  any of the three lookup skills or none of them.

- **`glossary-lookup`, `policy-lookup` and `standards-lookup` read `sources`.** Each builds a link and a fetch from the
  publishing block of the corpus that wrote the line, reached through the line's `shortcode`, rather than from yours.
  Each says what a prefixed id means and where an inherited record's file sits. `standards-lookup` states that one file
  already holds the whole union, and reads what stayed behind off `types` in the manifest rather than naming a fixed
  four. `kac update` brings the three down.

- **A type's manifest entry names two more of its part line's keys.** `idKey` and `seeAlsoKey` join `recordKey` and
  `partKey`, so a corpus merging that type stamps the keys the producing type actually named. `formatVersion` moves to
  4, so run `kac export` again before `kac bundle`, which refuses an export built to another shape.

- **`kac validate` reports a `Covers` line that names nothing.** `mirrors-citations` now reports a labelled footnote
  gathering no citation the field could carry, against the line. A line naming only ids of types the field does not
  point at gathers nothing either. A section carrying no line at all stays silent, which is how a rule discharging no
  clause is written.
- **`kac validate` reports a `Covers` line a space left out of italic.** Markdown will not read an emphasis mark with a
  space against it, so `_**Covers:** [pol-SCRT].EMBED _` is not the form and the marks reach the page.
  `mirrors-citations` reports the line, and its citations count as before, so a standard covering six clauses gets one
  finding rather than six against its frontmatter. A line that is bold alone is still the labelled prose form and is
  passed over.
- **`kac validate` tells a `Covers` line naming a record whole to name the part.** Where the field declares
  `part-required:`, `mirrors-citations` reports the line in the words `ref-resolves` uses, naming the target type's own
  word for a part: `this 'Covers' line names 'pol-EVER' whole, and 'implements' names a clause`. It used to report that
  the field did not list the id, which sent an author to put a bare policy id there and meet `ref-resolves` refusing it
  on the next run. A field that admits a bare id keeps the message it had.

## 0.19.0 - 2026-08-31

### Added

- **A standard exports its rules, and a corpus adopting standards ships a skill that reads them.** The type declares
  `parts:` over the H3 headings under `## Rules`, so `kac export` writes one line per rule to
  `standards/rules.jsonl`, carrying the obligations in the markdown the standard wrote them in. Each record travels
  beside them with its Summary and its conformance checklist. `kac bundle` includes the new `standards-lookup` skill
  wherever the export carries standards, and trims it where it does not. `kac new` ships the skill in the plugin tree,
  and `kac update` sends it to a corpus already created.
- **A field can be held to the citations its prose gathers.** A type declares `mirrors-citations: <Label>` beside the
  field's `ref:`, and `kac validate` reports drift in both directions between the field and the labelled lines. A line
  is written in italic with the label bold, and closes the section whose citations it gathers, so a line standing in the
  middle of one is reported where it sits. A standard's `implements:` declares it: each rule closes on
  `_**Covers:** …_` naming the clauses it discharges, so the frontmatter says which obligations the standard answers and
  each rule says which of them it answers. The obligations under a rule then carry no clause citation of their own, and
  `kac export` drops the footnote before it takes them, so a part carrying nothing else travels with no obligations
  rather than with a coverage line standing where its words belong. `kac validate` also reports the key declared with no
  `ref:` to resolve against. `kac new` ships the form in the standards template.
- **A standard may carry a `Sources and further reading` section.** It names the external documents the standard defers
  to, each marked normative or informative. A rule built on somebody else's conventions then says where the rest of it
  lives. `kac new` ships the section in the standards template, and a standard deferring to nothing deletes it.

### Changed

- **A standard's rules sit under `###` headings, and `kac validate` reports a Rules section with none.** The heading is
  what the rules beneath it hold a reader to, and it is the address a citation and an export both carry, so
  `part-none` now reaches standards as it already reached glossaries. A standard whose rules are a bare bullet list
  gains one heading. `kac new` ships the grouping in the standards template.
- **A tool's `category` is the folder it sits in under `tools/`, as a policy's and a standard's already were.** It was
  the one of the three still written by hand, and the only field in the schema carrying no `description:`. A record that
  writes the key now fails `derived-key`: delete the line and file the record under the folder you want it to name. It
  is no longer required, so a tool filed directly in `tools/` simply has no category, which is what the other two do.
  `kac new` ships the template without the key.

### Fixed

- **`kac validate` reports a schema declaring `of: object` with no `entry:` block, rather than ending in a stack
  trace.** The schema pass already names that fault as `schema-shape`. A record filling such a field reached the entry
  check first and took the run down, so the message never printed.
- **Declining `kac update --drop-type` says `update`, where it used to say `new`.** The message is the tail of the
  command that printed it, and the one it named was a command the reader had not run.
- **A rule whose `expr:` names a number too large for a whole number is reported, rather than ending in a stack trace.**
  `words() < 99999999999` reached the parser's integer literal and overflowed past the exception the schema load
  catches. `kac validate` now names the number, its position and the rule.
- **`kac restore` refuses a package that unpacks to more than 256MB, or that holds a single entry over 16MB.** The path
  each entry names was already held inside the import folder, and what it unpacks to was not, so a malformed package was
  read whole into memory instead. Both caps count the bytes actually read, because a zip entry's declared size is the
  package's own claim about itself.
- **A part id written against a link takes no delimiter that closes an emphasis.** `_[pol-SCRT].EMBED_` read the
  citation as `pol-SCRT.EMBED_`, because the id was measured off the source rather than off the text markdown makes of
  it. `part-ref` reported a clause nobody could write.

## 0.18.0 - 2026-08-31

### Added

- **A type's `id.width` takes a `min`/`max` span as well as an exact count.** A mnemonic drawn from a concept rather
  than cut to a length can then admit both `std-PR` and `std-SECRET` under one declaration. `kac validate` reports an id
  outside the span as `id-format` and names both ends. An exact `width: 4` behaves as it always has.
- **`filename.carries-id: false` keeps a type's id out of its filenames.** Its records are filed by topic alone, and
  nothing then reads the head of a filename as an id: `id-matches-filename` stays silent, `slug-length` measures the
  whole stem, and a link to the file is a link rather than a citation. `kac validate` refuses the three spans it cannot
  act on. One beside a filename that still carries the id, because `secret-handling.md` would otherwise bind to
  whichever id its first segment happens to spell. One on a `numbered` type, which pads to a single width so that ids
  sort. One whose `min:` sits above its `max:`, which no id can meet.
- **A field can require the part of the record it points at.** A type declares `part-required: true` beside the field's
  `ref:`, and `kac validate` reports an id there that names the record whole. The message uses the target type's own
  word for a part, so a field pointing at policies asks for a clause. `kac validate` also reports the key declared with
  no `ref:` to resolve against, and one pointing at a type that keeps no parts.

### Changed

- **Standards take mnemonic ids.** `std-0001` becomes `std-VCS`. A number records the order things were created, and a
  reader meeting one in a control's `verifies:` learns nothing. Filenames are untouched, because a standard is already
  named for its rule area. A corpus that wants its numbered standards back claims
  `.schema/standards.yaml` with a `skip:` entry in `.corpus.yaml`, which stops `kac update` replacing it.
- **`kac generate` heads a table per folder in a type's index.** A type that declares a field with `from: sub-path`
  groups its index rows on the first folder below the type, so a policy folder holding Delivery, Governance, Operations
  and Security reads as four tables instead of one long list. A record filed deeper joins the table its first folder
  heads. A type whose records all sit directly in its folder gets the single table it has always had.
- **`implements:` on a standard names clauses.** `implements: [ pol-EVER ]` becomes one entry per clause the standard
  puts into practice, as `pol-EVER.BRANCH`. The bare id claimed the whole policy, so a standard discharging six of eight
  clauses read to a coverage report as full cover and the other two disappeared. The shorthand is refused rather than
  admitted beside the list, because it is a keystroke shorter than the honest form. A corpus that wants the old reading
  back claims `.schema/standards.yaml` with a `skip:` entry in `.corpus.yaml`, which stops
  `kac update` replacing it.
- **The derived column is dropped from a table that repeats it.** Every row under a heading of Security carries
  `security`, so the Category column says nothing there and is left out. A record filed deeper keeps it, because
  `platform/node` under a heading of Platform is the one place `node` is written down. A corpus using no folders at all
  loses a column that was empty in every row.

### Fixed

- **A schema key spelled `no` or `off` now switches its behaviour off.** `on-type-page: no` in `_checks.yaml` read as
  `on-type-page: true`, because only the exact word `false` was taken, and the check was then written onto a type page
  the schema had excused it from. Both spellings of each answer are read.
- **`kac update` finds a seeded record the corpus filed in a sub-folder.** A record's folder sets its category, so a
  corpus files a seeded policy under `policies/governance/` and still holds it. Compared by path, the corpus read as
  holding none, and `update` offered a second copy at the seeded path. Accepting it left two records carrying one id,
  which `kac validate` then failed on `id-unique`. A seed absent from its path is now looked for by the id it carries,
  anywhere under its type's folder, and a match is left alone under `--policy full` as well as under `cautious`. A
  record's relative links are written for the depth it was seeded at, so there is no copy `full` could write a folder
  down that would resolve.

## 0.17.0 - 2026-08-29

### Added

- **A field can be derived from the folder a record sits in.** A type declares `from: sub-path` on a field, and `kac`
  reads its value from the folders between the type's own folder and the file.
  `policies/security/accs-access-by-identity.md`
  carries `category: security` without a line of frontmatter saying so, and `standards/platform/node/testing.md` carries
  `platform/node`. A record saved straight into its type folder gets an empty value, so you start using categories by
  making a folder. The value reaches the generated index, its sort, and `kac export`.
- **`derived-key` reports a derived field written by hand.** The key is declared, so `unknown-key` admits it and cannot
  say that the value comes from the path. Delete the line, and file the record in the folder you want it to name.
- **`schema-shape` reports a type whose `folder:` is not the name of the file declaring it.** A document's type is read
  from the folder it sits in, and that lookup uses the schema file's name, so the two disagreeing left every record of
  the type unread while `generate` wrote into the folder nobody was reading. The two names now have to agree.

### Changed

- **A standard's `axis` field is gone, and `category` replaces it.** Nothing read `axis` but one index column, which
  repeated the folder already shown in each row's link. The composition model stays: the rule-set binding a piece of
  work is the union of the folders that apply to it. Delete `axis:` from every standard.
- **A policy's `category` is read from its folder rather than from its frontmatter.** It is no longer a required enum of
  `security`, `delivery`, `operations` and `governance`. Move each policy into the folder its category named, delete the
  `category:` line, and the exported value is unchanged. The set of folders is now the corpus's own.

### Fixed

- **`kac validate` judges a `#fragment` against the headings alone.** It read a record's frontmatter block as a heading,
  so `fragment-resolves` accepted a link naming an anchor no renderer offers. A link into a record is now held to the
  headings that record carries.

## 0.16.0 - 2026-08-28

### Changed

- **`kac export` carries a field its type declares as a list.** Every such field was written as `null` on every record,
  which reads exactly as a record that holds nothing. A list now travels as a JSON array, and an entry the type declares
  as an object carries the keys that declaration names. A list a record left empty stays `null`, beside the field it
  never wrote. `docs/design/export.md` states the shape.

- **The glossary exports `tags`.** It is the first field to travel as a list. A consumer holding a vendored glossary can
  filter its records by subject without reading each `Scope`. The key lands on `glossary/<record>.json`, and a term line
  in `terms.jsonl` carries no `tags`.

- **`kac new` no longer sends a corpus a link to a type it declined.** A type's root page and its `_template.md` name
  the other types and link to them, which is what makes a full corpus navigable and what left a corpus adopting a subset
  holding dead links. Each page is now unlinked as it is written: a reference to a declined type keeps its own wording
  and loses its link, so `That is a [service](services.md).` arrives as `That is a service.`

  The same happens on `kac update --add-type`, for the page that arrives, and on `kac update --policy full`, which now
  holds a seed to the template as this corpus would have received it rather than as it was authored. Without that a full
  update wrote the links back.

  This reaches the pages a corpus receives once and then owns. A framework document is shared word for word, and
  `framework-names-types` goes on holding it to naming a type rather than linking to one.

  A link into another type's folder has no such repair, because its text names a record. Two seed pages defined one as a
  reference link, which reached a corpus whole, so `glossary.md` and `frameworks.md` now name the record without linking
  it.

- **`kac update --add-type` says what the arriving page does not get.** The new page links to the types the corpus
  holds. The pages already there name it without linking, because each was written while the type was still declined,
  and changing them is the corpus's own call.

- **`kac validate` no longer refuses a schema for naming a type the corpus declined.** A field's `ref:` and a type's
  `versus:` each name a type, and a corpus adopts as many types as it has use for. `.schema/standards.yaml` alone
  reaches four other types, through `ref:` on four fields and a `versus:` naming one of them again, so a corpus adopting
  standards and nothing else met five `schema-dispatch` errors on a schema it had just been sent. Both declarations are
  now left alone where no schema covers the type. Nothing is rendered, and `kac update --add-type` starts the reference
  without an edit to `.schema/`.

  What a record is held to does not soften with it. `ref-resolves` goes on asking that a cited id exists, and a field
  whose every declared type this corpus turned down now admits nothing rather than everything. It names the types the
  declaration wanted, since a type nothing covers has no label to read:
  `'derived-from' points at 'std-0002', which is a Standard. The field points at 'adrs', which this corpus did not
  adopt.`

  With this and the unlinking above, `kac new` adopting any single type writes a corpus that validates and exits 0.

- **`kac update --drop-type` asks before it deletes.** Giving up a type deletes its page and leaves every page still
  naming it holding a dead link. The run says so, says that `kac validate` reports the ones it can reach, and waits for
  an answer. The question takes no by default. `--yes` answers it in advance, and a run with no terminal and no
  `--yes` refuses rather than guessing.

## 0.15.0 - 2026-08-28

### Added

- **`kac validate` says when an import has fallen behind what its source publishes.** `kac restore` keeps the version a
  `consumes:` entry locked for as long as the range still admits it, which is what makes a restore reproducible and is
  also how a corpus sits on a version nobody meant it to sit on. So `validate` asks each source what it holds now, once
  per run, and reports three new checks against `.corpus.yaml`.

  `import-behind` is a **warning**: a newer version sits inside the declared range, and `kac restore` takes it.
  `import-capped` is **information**: a newer version is published and the range holds it back, which is a decision the
  corpus already made. `import-unreachable` is **information** too, for a source this run could not ask, so a lock reads
  as unchecked rather than as current. None of the three fails the build, because failing on somebody else's release
  would turn every downstream red the day a governance corpus ships.

  A source answering with no versions at all reports as unreachable rather than as current, because a registry answers a
  private feed's anonymous reader exactly as it answers a package nobody has published. A corpus with no
  `consumes:` block reads no source and builds no client, and every other check still reads the working tree alone.

- **A third severity, `info`.** `kac validate` counts it in its summary line and in `--json`, where
  `summary.infos` is new, and `kac checks` tallies it apart from the warnings. Neither a warning nor an info changes the
  exit code. A check declares `severity: info` in `.schema/_checks.yaml`. `docs/design/checks.md` covers it.

- **A corpus says who it is, and `pack` and `bundle` stop inventing it.** Four new keys in `.corpus.yaml`:
  `display-name`, `description`, `license` and `author`. `kac export` carries them in a new `about` block, `kac pack`
  writes them into the package a registry lists, and `kac bundle` writes them into the plugin manifest somebody
  installs.

  **A plugin's identity is now generated rather than copied.** `name`, `version`, `displayName`, `description`,
  `author`, `homepage`, `repository`, `license` and `keywords` are all written from the corpus, and a key the corpus
  declared nothing for is removed rather than left standing. `author` is the exception, filed under the corpus's own
  name where it named nobody, because the format asks for one and `claude plugin validate --strict` fails a manifest
  carrying none. `.plugin/.claude-plugin/plugin.json` keeps only what the corpus declares: `metadata.corpusRoot` and
  `metadata.components`, plus any key this tool has never heard of. A manifest copied from a template no longer
  publishes under the template author's name, licence and repository.

  `keywords` are the types the export carried, so a plugin never advertises a type its corpus declined. `kac new`
  writes the four keys bare, because a value supplied there would be inherited rather than chosen.

- **`kac export` names the two keys that address a part.** Each type's manifest entry gains `recordKey` and
  `partKey`, naming which key of a part line says which record it belongs to and which part of that record it is. A type
  names its own keys, so a consumer holding a corpus with a type it never adopted had no way to read them and had to
  assume a spelling. Both are absent where the type keeps no parts, as `partsFile` is. `docs/design/export.md`
  covers it.

- **`kac validate` resolves a reference across a corpus boundary.** A citation carrying a producer's shortcode, as
  `eng:pol-VURM.TIMEBOX`, resolves against the export `kac restore` unpacked under `.imports/`. It is read in prose and
  in a field declaring a `ref:`, so `implements: eng:pol-VURM.TIMEBOX` names one clause rather than a whole policy, and
  both halves are held to existing. Local records and imported ones go through one lookup, so a corpus is not judged
  more loosely for having imported the record it cites.

  Each side keeps its own spelling. A record the reading corpus holds is cited bare, one it imported carries the
  shortcode, and writing either the other way is refused naming the spelling to write.

  A new `import-restored` check fails a corpus declaring an import that is not on disk, and names `kac restore`. Every
  citation into that shortcode then stays quiet, so a run that has not restored reports one line rather than one per
  reference. `docs/cli/validate.md` documents both.

- **`kac restore` fetches the corpora a corpus declares it consumes.** A new `consumes:` block in `.corpus.yaml` names
  each producing corpus, the shortcode it is cited by, the version range it is wanted at and the source it comes from.
  `restore` resolves each range, fetches the package `kac pack` sealed, and unpacks it under `.imports/<shortcode>/`,
  which the template now gitignores. The version each range resolved to is written back onto its own entry, so
  `.corpus.yaml` stays the one description of what a corpus is.

  A `source:` names a registry's service index or a folder of packages. A folder holds the same sealed package a
  registry serves, so a corpus consuming a sibling in its own repository needs no registry, no token and no release. A
  path is relative to the corpus declaring it, as `upstream.url` is.

  A range says `1.2.0` or `^1.2.0` and nothing else, and a caret never takes a prerelease. A lock the range still admits
  is taken without asking the registry, so two restores of an unchanged descriptor write the same bytes. A run says what
  it fetched, at which version, and which corpora were already current.

  A shortcode two entries both claim is refused naming both, as is a corpus two entries both consume, as is a package
  whose own manifest is cited by a different shortcode from the one declared. `KAC_REGISTRY_TOKEN` in the environment
  carries a bearer token for a private feed. `docs/cli/restore.md` documents the verb, and `docs/corpus-descriptor.md`
  the block.

## 0.14.0 - 2026-08-27

### Added

- **`azure-devops-wiki` and a new `azure-devops` target build links.** `kac export` addressed `github` alone. A corpus
  publishing to an Azure DevOps wiki now gets a `?pagePath=` link per record, and one publishing to Azure Repos without
  a wiki gets a `?path=&version=GC<sha>` link. `kac new --publishing azure-devops` accepts the new target and fills its
  base in from a `dev.azure.com` remote, in either the SSH or the HTTPS spelling. A wiki base has to be typed in,
  because a repository's remote says nothing about which wiki publishes it.

  A wiki link is not pinned to a commit, because no `?pagePath=` URL takes one. An agent still reads the version the
  export was built from. `docs/corpus-descriptor.md` sets out both targets.

  The `azure-devops` link form and the anchor an Azure DevOps wiki resolves for a heading carrying punctuation are both
  unconfirmed against a live organisation. The wiki's page path, its anchor parameter and its rejection of a base
  carrying a page id are confirmed.

- **`kac pack` seals an export into a versioned package.** It reads `.dist/export/` and writes one file to
  `.dist/package/`, named for the corpus and its `content-version`. The file is a `.nupkg`, which is a zip carrying a
  small XML manifest a registry reads to name and version it, and both GitHub Packages and Azure DevOps Artifacts store
  one. Everything under `corpus/` inside it is the export, byte for byte, so nothing reading the result needs a NuGet
  client. Two runs over one export produce identical bytes.

  The command refuses a corpus that has not declared `corpus:`, `content-version:` and `shortcode:` in `.corpus.yaml`,
  naming the one that is missing. It publishes nothing: pushing the file is your pipeline's step, and `docs/cli/pack.md`
  carries the command for it.

- **`kac pack --repository <URL>` names where the corpus's source lives.** Some registries read that URL to decide which
  repository a package belongs to, and GitHub Packages refuses a package naming none when the token pushing it is scoped
  to a repository. The element is left out where the flag is not given, because the export states where a record is
  published and that is a different address.

- **A `policy-lookup` skill travels in the plugin.** `kac bundle` ships it beside `glossary-lookup`, and a corpus
  carrying no policies has it trimmed. It reads `policies/clauses.jsonl`, answers from a clause's `level` rather than
  from the modal in its wording, and says which of the four levels it found. What an external framework obliges stayed
  behind with the register that explains it, so the skill names that gap rather than filling it.

- **A component says whether the breadcrumb names it.** `"announce": true` on a manifest entry puts that skill in the
  breadcrumb's last line, and the default leaves it out. The line exists to create a question a session would not think
  to put, so a skill somebody asks for by name does not earn it. A corpus adding a second skill sets `announce`
  on the one worth introducing.

- **`plugin.from` in `.corpus.yaml` reads the plugin tree from one shared folder.** Several corpora in a repository keep
  one copy of the skills and hooks between them instead of a copy each. `kac bundle` merges that tree with the corpus's
  own `.plugin/`, where a file the corpus holds wins, and `kac update` withholds the shared half rather than writing it
  back. The manifest is never taken from the shared tree: it names the plugin, so it stays at
  `.plugin/.claude-plugin/plugin.json` in each corpus. Omit the key and nothing changes. A corpus adopting the key with
  the old copies still on disk has each one reported as a file the template sends nothing to, because a corpus's own
  file wins the merge and a leftover would go on shipping after every upstream change.

- **A corpus created before this declares no component for the new skill.** `kac update` writes the skill, and leaves
  `.plugin/.claude-plugin/plugin.json` alone because the manifest is the corpus's own. A path no component owns ships
  unconditionally, so add the component yourself to have it trimmed where the type is not adopted:

  ```json
  {
    "path": "skills/policy-lookup",
    "requires": [ "policies@2" ],
    "note": "Reads a clause from corpus/policies/clauses.jsonl and the owning policy beside it."
  }
  ```

## 0.13.0 - 2026-08-26

### Added

- **A list field's entries can be objects.** A field declaring `of: object` names its entry's keys in an `entry:` block,
  written with the vocabulary a field is written with. Each key is held to its own `type:`, `pattern:` and `required:`.
  `entry-shape` reports an entry that is not a mapping, and `entry-key` an entry carrying a key the field does not
  declare or missing one it requires.

- **`alignment-rollup` holds a policy's `aligns-with` to its clause table.** Both directions: a binding framework
  reference in an `Alignment` cell and not in the roll-up, and one in the roll-up that no clause cites. The message
  names the reference and the side it is missing from.

- **The roll-up carries the frameworks that bind.** A rule declares `postures:`, naming the standings that oblige a
  summary as the corpus's framework register heads them. A clause may cite a framework filed under any other standing,
  for provenance, and the roll-up leaves it behind. `framework-posture` reports a clause citing a framework the register
  does not place at all, once per framework rather than once per clause.

- **A corpus rule can read the corpus's files.** `CorpusRuleContext` carries the tree, for the rule whose question is
  answered by a page no record links into the graph. A framework register is that case: it holds no frontmatter, so it
  is no record, and it is the only place a standing is written down.

- **`part-ref` reads a part id written beside a link.** `[pol-EVER].BRANCH` cites `pol-EVER.BRANCH`, so a document
  citing six clauses of one policy carries one link definition rather than six. The part id has to sit against the
  closing bracket, so a full stop closing a sentence after a link is still a full stop. A corpus already writing this
  form may see errors it did not before.

### Changed

- **`.corpus.yaml` takes one `base` where it took `human-base` and `raw-base`.** Write the URL a person opens to browse
  the corpus: the GitHub repository with no `/blob` on the end, the Azure Repos `_git` URL, or the wiki's own URL. A
  raw-content host was a GitHub idea that no other target has, and it never served the human case. Edit the
  `publishing:` block by hand: nothing migrates it, and a descriptor still carrying the old keys exports without links.

- **An export's `publishing` block drops `rawTemplate` and carries `base` and `pathPrefix`.** `humanTemplate` stays. An
  agent reading a record's source joins `pathPrefix` ahead of the record's `path` and asks a client that authenticates
  to the target, rather than fetching a bare URL. Only GitHub ever served raw source anonymously, and only for a public
  repository. `formatVersion` moves from 2 to 3, so `kac bundle` and `kac pack` refuse every export built before this.
  Rebuild with `kac export`.

  A record's `links` loses its `raw` half for the same reason. No type's `shapeVersion` moves: that object is written
  for every type by the exporter rather than declared by any one type's `export:` block.

  The `glossary-lookup` and `policy-lookup` skills both tell an agent to fetch the file rather than substitute into a
  template, and to say so plainly where it holds no client for the target.

- **An index column holding a list renders its entries.** A column naming a list field read the value as a scalar and
  wrote an empty cell, so `aligns-with` on a policy index had been blank since the column was added. A column naming a
  list of objects renders what names each entry, which for `aligns-with` is the framework.

- **A policy's `aligns-with` is grouped by framework.** It was a flat list of strings held to an ISO 27001 pattern,
  which is why no other framework could appear in it. Each entry now carries a `framework:` and the `clauses:` reached
  inside it, and any framework may. A corpus holding policies rewrites the field.

- **A clause line carries no `alignment`.** `policies` moves to `export.version: 2`. A framework reference resolves
  through the corpus's own `frameworks.md`, which no consumer receives, so the mapping reached one without what says
  what it is worth.

## 0.12.0 - 2026-08-26

### Fixed

- **`part-ref` reads a citation written as a link.** A corpus cites a part as a code span, and as a link carrying the
  citation as its text or as its label. Only the code span was resolved. A link naming a clause or a term that does not
  exist passed `kac validate`, because a link resolves against a page and the page carries whichever part the citation
  claimed. Every form now reports under `part-ref`, and a link spelling the separator as a colon is reported as one, so
  a corpus using the link form may see errors it did not before.

- **A type index links a record through the category folder holding it.** `kac generate` wrote the filename alone, so a
  record filed under a category below the type's folder was linked as though it sat beside the index. Standards are
  filed that way by declaration, and every link to one was dead.

## 0.11.0 - 2026-08-25

### Added

- **A policy's clauses travel in an export.** `.schema/policies.yaml` declares an `export:` block, so `kac export`
  writes `policies/clauses.jsonl` and one file per policy beside the glossary's. Each clause line carries `level`,
  holding the modal the clause opens with, so a consumer tells a `MUST` from a `COULD` without parsing the words.
  `Purpose` travels as its opening paragraph, and `Scope` and `Exceptions` travel whole.

### Fixed

- **A part's `anchor` is read from where its type takes its parts.** A heading-sourced type carries the part id, which
  is a heading's slug and its anchor alike. A table-sourced type carried that id too, and no fragment resolves to an
  authored clause id. It now carries the slug of the section holding the table, so a link built from a clause line lands
  on the table.

- **A carried section leaves the link reference definitions behind.** They sit in a block at the foot of a record, which
  puts them inside whichever section is written last, so `kac export` joined them onto the end of that section's prose.
  A consumer read a run of paths that nobody sees on the page. A glossary's export is unchanged, because `Scope`
  is never a glossary's last section.

- **`kac export` leaves a clause table in the order its author wrote it.** Every type's parts were sorted on their text,
  which is right for a glossary and wrong for a table grouped by binding level: an advisory clause could reach a
  consumer ahead of the obligations. A heading-sourced type's parts still sort alphabetically.

## 0.10.0 - 2026-08-25

### Added

- **A section can travel cut down.** `export.sections:` in `.schema/<type>.yaml` takes `summary` and `reference`
  alongside `full`. `summary` carries the section's opening paragraph. `reference` carries the key with no words under
  it, leaving a consumer the record's own `path` and `links` to follow. `kac validate` no longer refuses either.

- **The export manifest states the fidelity each section travelled at.** Every entry under `types` carries a
  `sections` object naming its sections and how much of each one travels, so a consumer can tell a cut section from a
  whole one.

### Changed

- **`kac validate` reports a reduced fidelity against `export.parts:` alone.** A part line carries `full`, because
  `line:` already names key by key what of a part travels. A type declaring `summary` or `reference` there is still
  reported as declaring a fidelity nothing carries.

## 0.9.0 - 2026-08-25

### Added

- **A type declares the keys of its own export line.** `export.parts.line:` in `.schema/<type>.yaml` names the keys one
  part writes and the source filling each, drawn from a closed vocabulary covering a part's text, its body, its modal, a
  frontmatter field and a table column. `kac export` reads that declaration and names no key itself, so a second type
  exporting parts costs no code. Glossary is the type that declares one, and its `terms.jsonl` is byte for byte what it
  was.

- **Each type states its own shape version in the export manifest.** `export.version:` in the schema reaches the
  manifest as `shapeVersion` on that type's entry. `formatVersion` covers the envelope alone, so a key added to one
  type's line cannot refuse a consumer reading another.

- **`kac bundle` refuses a component reading a type at a shape the export does not carry.** A `requires` entry may name
  the shape, as `glossary@1`. A bare `glossary` asks for the type and opens none of its files. Either is trimmed, as
  before, where the export carries no such type at all.

- **`kac validate` reports a `line:` that would export nothing.** A key with no source, a source nothing fills, a
  `front.` naming a field no record carries, a `column.` naming a header the type does not declare, and a `part.lead`
  or `part.aside` against a table row are each an error. So is an `export:` block with no `version:`.

### Changed

- **`export.parts:` in a type's schema is a block, and the fidelity moves inside it.** `export.parts: full` becomes
  `export.parts.fidelity: full` with `line:` beside it. `kac validate` reports a type file still carrying the older
  form, naming the fidelity, the `line:` and the `version:` it lacks.

## 0.8.0 - 2026-08-25

### Added

- **A corpus declares the shorthand another corpus cites it by.** `.corpus.yaml` carries a top-level
  `shortcode:`, which is the `eng` in `eng:pol-VURM.TIMEBOX`. `kac validate` refuses a spelling a citation cannot carry,
  and one a type has already taken as its id prefix. `kac export` states the declared shortcode in its manifest, so a
  consumer holding several exports knows which one answers a scoped citation. `kac new` writes the key with no value: a
  shortcode cannot be changed once another corpus has cited it, so it is filled in when one is about to.

### Fixed

- **`kac validate` reads a record whose frontmatter carries a complex key.** A key written as a sequence or a mapping is
  legal YAML and names no field. It was reported as frontmatter that would not parse, which named the wrong fault. It
  now arrives as an empty key, which `unknown-key` reports against the document that wrote it.

## 0.7.0 - 2026-08-24

### Added

- **`kac update` takes a newer framework into a corpus that already has one.** It fetches the template `.corpus.yaml`
  points at, decides file by file what the corpus receives, writes it, and records what it took. Everything it writes
  stays in the working tree and nothing is committed, so `git diff` is the review step.
  [`update`](https://paul80nd.github.io/knowledge-as-code/cli/update/) covers the layers, the flags and what each
  refuses.
- **`kac update --check` reports what would change and writes nothing**, exiting non-zero where anything would. It
  answers in both directions: a framework file the corpus holds differently, and a file the corpus keeps where the
  framework's rules apply that the template sends nothing to.
- **`kac update --add-type` adopts a type, and `--drop-type` gives one up.** Adopting writes the type's schema, root
  page and template, and adds the name to `types:`. Giving one up refuses where the folder still holds records, naming
  the count.
- **`kac update --policy cautious|full` overrides `update-policy:` for one run.** `cautious` writes a seed only where
  the corpus has none. `full` holds every seed to the template and hands the reconciliation to the diff.
- **`update` stamps `upstream.commit` alongside the template version and the date.** A template read from a folder
  resolves no commit, and the key is then left as it stands.

### Removed

- **`kac mechanism` is gone**, and `kac update` replaces both its halves. It compared two corpora on identical paths and
  read a manifest at `tooling/manifest.yaml` that no corpus held, so no corpus could run it against the framework it
  actually took.
- **`role:` in `.corpus.yaml` is no longer written or read.** It said whether a corpus carried the tests that prove the
  tool, and no corpus does. `new` stops writing it, and an `update` over a descriptor still carrying it names the key
  and stops, as it does for any retired key.

### Changed

- **A continuous integration starter is refreshed and never introduced.** `new` writes the starter for the system
  `--ci` named, and an update leaves a starter the corpus does not hold where it is. Which system builds a repository is
  that repository's own answer.

## 0.6.0 - 2026-08-24

### Added

- **`kac new` turns the folder you are standing in into a corpus.** It takes the framework from a template repository at
  a ref, writes what the manifest says a corpus receives, and writes the two files no template can supply:
  `.corpus.yaml` and `README.md`. It then runs `generate`, `validate` and `git add -A`, and stops short of committing.
  [`new`](https://paul80nd.github.io/knowledge-as-code/cli/new/) covers the flags, the defaults and the order it asks
  in.
- **`--from` defaults to the framework's own repository**, and accepts a local path as well as a URL. The template is
  cloned rather than fetched over HTTP, so a repository needing authentication uses the credential helper you already
  have. A local path is the offline escape hatch.
- **`--yes` takes the default for every answer not given.** A run with no terminal and a missing answer exits with an
  error rather than waiting, because a hung pipeline is worse than a failed one.

### Changed

- **A manifest rule may declare `ci:`**, naming the continuous integration system its files serve. `kac new --ci`
  writes the matching starter and no other, so a corpus built by Azure DevOps no longer receives a GitHub Actions
  workflow that would run uninvited.
- **`minimum-tool` in the template manifest moves to `0.6.0`.** A 0.5.0 tool reads that manifest, ignores every `ci:`
  in it, and takes both starters.

## 0.5.0 - 2026-08-24

### Changed

- **`.corpus.yaml` takes a new shape.** `upstream:` now says `path`, `ref`, `commit`, `template-version` and
  `taken-on`, where it said `mechanism-version`, `synced-from` and `synced-on`. `accepted-divergences:` becomes
  `skip:`, and drops `since` and `revisit`. `update-policy:` arrives, defaulting to `cautious`. Every renamed key is
  reported by name, with what to write instead, so nothing is misread in silence; `upstream.synced-from` was dropped
  rather than renamed, and the message says to delete it.
  [The corpus descriptor](https://paul80nd.github.io/knowledge-as-code/corpus-descriptor/) covers the whole file.
- **`mechanism --sync` stamps `upstream.template-version` and `upstream.taken-on`**, where it stamped three keys. It
  leaves `upstream.commit` alone, because a sync reads a directory rather than a git ref and has no commit to record.
- **`mechanism --check` reports a template version**, where it reported a mechanism version. The number has not moved.

### Added

- **A template manifest reads `to:` on a rule**, naming where that rule's files land in a corpus. It replaces the
  pattern's directory prefix, so a template authored in a subdirectory of the repository serving it reaches a corpus's
  own root.
- **A template manifest reads `layer: removed`**, a tombstone naming a file a corpus should delete when it takes a newer
  framework. Nothing acts on it yet: `kac update` is what will.
- **A template manifest reads `minimum-tool`**, the oldest tool that can read it. The template is fetched rather than
  shipped inside the package, so the two version independently.

## 0.4.0 - 2026-08-24

### Changed

- **`kac` finds a corpus by its `.corpus.yaml`**, where it looked for a `.schema/`. It then walks up again from the
  corpus root for the schema to judge that corpus against, so one schema can serve several corpora in one repository. A
  standalone corpus holds both files at its own root and both walks stop there, which is the ordinary case and is
  unchanged. A corpus with no descriptor is no longer found: write one, and
  [the corpus descriptor](https://paul80nd.github.io/knowledge-as-code/corpus-descriptor/) says what goes in it.
- **`kac` names `.corpus.yaml` when it cannot find a corpus**, and reports separately on a corpus with no schema above
  it. The second exits 1 rather than crashing on the first schema file it tries to open.
- **`kac mechanism --help` reads its two option descriptions as sentences.** `--check` closed on a semicolon, and
  `--against` opened on a bare noun phrase. What either flag does has not moved.

## 0.3.0 - 2026-08-23

### Added

- **`--no-color` on every verb.** `NO_COLOR` in the environment asks for the same thing, and the tool already read it.
  Colour goes either way, and bold stays.

### Changed

- **`generate` writes a relative link naming the file**, where it wrote a root-relative link naming the folder. A block
  in `README.md` links `[ADR](adrs.md)`, and one in `knowledge-as-code/taxonomy.md` links `[ADRs](../adrs.md)`. The link
  resolves wherever the corpus sits, rather than only where a renderer maps a folder to the page inside it. Run
  `kac generate` after upgrading: `--check` reports every block carrying the old form until you do.
- **`validate` and `checks` list in aligned columns**, with the severity coloured. Only the message column wraps, so a
  narrow terminal breaks a sentence and never a check id. `checks` splits its count by severity.
- **`generate` marks a file it created**, and counts what it wrote against the size of the whole plan.
- **`export` and `bundle` dim the directory in each path they write**, and colour a remark by whether it is advice or an
  account of the run. Neither changes a word it prints.
- **A failure is red on stderr.** That covers every verb's hard stop, and the heading over a list of what stopped it.
  What the heading names stays plain beneath it.
- **`--json` and every exit code answer as before.** `--json` goes straight to the stream and never carries colour,
  whatever the terminal.
- **Two messages lose a semicolon the house style does not keep.** The `filename / slug-length` row in every generated
  checks table, and the meta-test reporting an over-long description. Run `kac generate` after upgrading: `--check`
  reports every type page carrying the old wording until you do.

## 0.2.1 - 2026-08-21

### Changed

- **The command line is parsed by `Spectre.Console.Cli` rather than `System.CommandLine`.** Every verb, option and exit
  code answers as it did. `--help` reflows into Spectre's layout, `-v` joins `--version`, and `-?` no longer stands for
  `--help`. The tool carries one library for reading a command line and asking a question, rather than two.

## 0.2.0 - 2026-08-20

### Changed

- **`kac index` is now `kac generate`.** The command writes each type's `_index.md` and rewrites the generated blocks in
  every type page, and only the first of those is an index. `--check` is unchanged, and so is everything either half
  writes. There is no alias: a pipeline or script still naming `index` fails until it names `generate`.

## 0.1.1 - 2026-08-20

### Added

- An icon on the nuget.org package page.
- A link from the package page to the release notes for the version being installed.

The tool answers exactly as 0.1.0 does. Only what nuget.org shows about it changed.

## 0.1.0 - 2026-08-20

The first published version.

### Added

- `kac validate` holds a corpus to the schema it carries: frontmatter, identity, structure, clauses, links, the graph
  and the type setup.
- `kac index` regenerates `_index.md` and the generated blocks in each type page. `--check` reports what is stale rather
  than writing it.
- `kac checks` lists every check the validator implements, read from the schema rather than from a list in the tool.
- `kac export` writes the corpus to `.dist/export/` as data a consumer reads instead of cloning.
- `kac bundle` assembles that export and `.plugin/` into an installable plugin.
- `kac mechanism` compares the shared layers against a reference corpus, or takes them from one.

[OKF v0.2]: https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md
