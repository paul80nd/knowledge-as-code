# Skills

A skill is a document an agent loads when it recognises the work in front of it. `kac` ships sixteen, and which of
three trees each one lives in decides who can read it and what moving it costs.

The rule is one sentence. **A writing skill travels into a corpus (one repository of knowledge records kept in git)
where it governs a surface that corpus holds.** A skill about C# or about this documentation site governs nothing a
corpus has, so it stays here.

Every table below is written from the file that decides it, so none of them is a second answer that can drift.

## What travels inside a plugin

These ship in the plugin [`bundle`](cli/bundle.md) assembles, beside a frozen copy of the corpus export. They answer
from that export rather than from a network call.

**An install carries fewer than ten.** A skill reading a record type the corpus did not export is trimmed, so what
somebody receives follows from what that corpus adopted. `example-libraries` ships five of these and
`example-engineering` seven.

`Requires` names the record types a skill opens a file of. A skill requiring none is one of two things.
**`standalone`** reads no export and serves whoever is holding the plugin, so it ships whatever the corpus adopted.
**`supporting`** exists for the lookup skills and follows the last of them out. [The plugin
bundle](design/plugin.md#a-component-naming-no-type) says what the trim does to each.

A change to one of these moves the `content-version` of every corpus whose bundle ships it.

<!-- BEGIN GENERATED: skills-in-the-plugin -->

| Skill               | What it answers                                                                                                                              | Requires      |
|---------------------|----------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| `corpus-retrieval`  | Reach the published source of a record this corpus export summarises, and build a link somebody can follow                                   | supporting    |
| `glossary-lookup`   | Look a term up in the knowledge corpus glossary that travels with this plugin                                                                | `glossary@1`  |
| `raise-finding`     | File something you noticed about this corpus as an issue on the repository that publishes it                                                 | standalone    |
| `harvest-findings`  | Triage the findings filed against this corpus, and draft the record one of them asks for                                                     | standalone    |
| `request-deviation` | Ask the owner of a clause to accept a knowing departure from it, as an issue on the repository that publishes the corpus holding that clause | supporting    |
| `controls-lookup`   | Find out what proves a rule here, in the controls that travel with this plugin                                                               | `controls@1`  |
| `fix-lookup`        | Find out whether somebody here has already solved this problem, in the fixes that travel with this plugin                                    | `fixes@2`     |
| `policy-lookup`     | Find what this estate is committed to, in the policy clauses that travel with this plugin                                                    | `policies@2`  |
| `process-lookup`    | Find the procedure for a planned task, in the processes that travel with this plugin                                                         | `processes@1` |
| `standards-lookup`  | Find the rules you have to build to, in the standards that travel with this plugin                                                           | `standards@1` |

<!-- END GENERATED: skills-in-the-plugin -->

## What travels into a corpus

These arrive in a corpus's own working tree, under `.claude/skills/`. [`new`](cli/new.md) writes them when the corpus
is created and [`update`](cli/update.md) keeps them current, so a corpus owns a copy rather than reading this
repository.

They are for whoever is writing records, which is a different reader from the one who installed a plugin. Nothing
here reads an export.

Adding, removing or renaming one of these moves `version:` in `manifest.yaml`, and `upstream.template-version` in
every corpus that took it, because a corpus has to respond. Rewording one moves neither: `update` sends the new words
and nothing about the corpus changes.

<!-- BEGIN GENERATED: skills-in-a-corpus -->

| Skill               | What it answers                                                                               |
|---------------------|-----------------------------------------------------------------------------------------------|
| `technical-writing` | The writing floor for every word in this repository                                           |
| `writing-a-record`  | The shape of what a corpus holds                                                              |
| `writing-a-report`  | Turn `kac report <name>` into a published report record, and bring an existing one up to date |

<!-- END GENERATED: skills-in-a-corpus -->

## What stays in this repository

These never leave. Each governs a surface no corpus has: the C# behind `kac`, the pages of this site, or the order of
work in the repository that publishes the framework.

They are listed so that their absence from a corpus is a stated answer rather than something you go looking for. A
change to one moves no version stamp at all.

<!-- BEGIN GENERATED: skills-here -->

| Skill                 | What it answers                                                                               |
|-----------------------|-----------------------------------------------------------------------------------------------|
| `i-want-to`           | Route a piece of work in this repository to the process record or playbook carrying its steps |
| `writing-in-the-tool` | The shape of prose inside `tooling/`                                                          |
| `writing-the-docs`    | The shape of the public documentation                                                         |

<!-- END GENERATED: skills-here -->

## Reading one

A skill is a Markdown file, and nothing stops you reading it. Whichever of the bundled ones survived the trim sit
under `skills/` inside an installed plugin, and `bundle.json` beside them names the ones that did not. The rest are in
`.claude/skills/` of whichever repository holds them.

A skill states its own rules in full rather than citing a standard, because it has to work in a checkout that holds no
corpus. Where a corpus states the same rule, both say it, and the record is what a reviewer reads.
