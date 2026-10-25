# Skills

A skill is a document an agent loads when it recognises the work in front of it. A skill `kac` ships either travels
inside the plugin (the corpus packaged for Claude Code), or arrives in the working tree of a corpus (one repository of
knowledge records kept in git). A few stay in the framework's own repository. Each table below is generated from the
file that decides it.

## What travels inside a plugin

These ship in the plugin [`bundle`](cli/bundle.md) assembles, beside a frozen copy of the corpus export, and read
that export. An install gets the ones whose record types the corpus exported.

`Requires` names the record types a skill opens a file of. A skill requiring none is one of two things.
**`standalone`** reads no export and serves whoever is holding the plugin, so it ships whatever the corpus adopted.
**`supporting`** exists for the lookup skills and follows the last of them out.
[What decides whether a component ships](cli/bundle.md#what-decides-whether-a-component-ships) gives the trim rules.

<!-- BEGIN GENERATED: skills-in-the-plugin -->

| Skill               | What it answers                                                                                                                      | Requires      |
|---------------------|--------------------------------------------------------------------------------------------------------------------------------------|---------------|
| `corpus-retrieval`  | Fetch the published source of a record this corpus export summarises                                                                 | supporting    |
| `glossary-lookup`   | Look a term up in the knowledge corpus glossary that travels with this plugin                                                        | `glossary@1`  |
| `raise-finding`     | File something you noticed about this corpus as an issue on the tracker that corpus states                                           | standalone    |
| `request-deviation` | Ask the owner of a clause to accept a knowing departure from it, as an issue on the tracker stated by the corpus holding that clause | supporting    |
| `ask-the-corpus`    | Send a question to the part of this corpus that answers it, when you do not know which record holds the answer                       | supporting    |
| `controls-lookup`   | Find out what proves a rule here, in the controls that travel with this plugin                                                       | `controls@1`  |
| `fix-lookup`        | Find out whether somebody here has already solved this problem, in the fixes that travel with this plugin                            | `fixes@2`     |
| `policy-lookup`     | Find what this estate is committed to, in the policy clauses that travel with this plugin                                            | `policies@2`  |
| `process-lookup`    | Find the procedure for a planned task, in the processes that travel with this plugin                                                 | `processes@1` |
| `standards-lookup`  | Find the rules you have to build to, in the standards that travel with this plugin                                                   | `standards@3` |
| `standards-review`  | Review a change against the standards that travel with this plugin, citing the rule behind every finding                             | `standards@3` |

<!-- END GENERATED: skills-in-the-plugin -->

### Copying the review view to `CODING_STANDARDS.md`

`standards-review` checks a change against the review view: the file [`bundle`](cli/bundle.md#views) writes at
`views/review.md` inside the plugin, listing every rule's obligations and the controls checking each standard. A review
tool that reads standards from your repository can read the same rules once you copy that file in:

```bash
cp ~/.claude/plugins/cache/<marketplace>/<plugin>/<version>/views/review.md CODING_STANDARDS.md
```

That is where Claude Code keeps an installed plugin, unless you set `CLAUDE_CODE_PLUGIN_CACHE_DIR`.
`~/.claude/plugins/installed_plugins.json` gives each install's `installPath`, which is the directory to copy from.

If you review with [`code-review`](https://github.com/mattpocock/skills/tree/main/skills/engineering/code-review):

* It reads `CODING_STANDARDS.md` by name, and skips whatever tooling already enforces, which is what the view's
  control lines state.
* The copy stays at the version you took it from. Copy it again each time the plugin's version moves.

## What travels into a corpus

These arrive in a corpus's own working tree, under `.claude/skills/`. [`new`](cli/new.md) writes them when the corpus
is created and [`update`](cli/update.md) keeps them current. The writing skills are for whoever writes records.
`harvest-findings` is for the corpus's maintainer, and writes to the tracker and to the records.

<!-- BEGIN GENERATED: skills-in-a-corpus -->

| Skill               | What it answers                                                                               |
|---------------------|-----------------------------------------------------------------------------------------------|
| `technical-writing` | The writing floor for every word in this repository                                           |
| `writing-a-record`  | The shape of what a corpus holds                                                              |
| `writing-a-report`  | Turn `kac report <name>` into a published report record, and bring an existing one up to date |
| `harvest-findings`  | Triage the findings filed against this corpus, and draft the record one of them asks for      |

<!-- END GENERATED: skills-in-a-corpus -->

## What stays in this repository

These never go into a corpus. Each governs a surface no corpus has: the C# behind `kac`, the pages of this site, or the
order of work in the framework's own repository.

<!-- BEGIN GENERATED: skills-here -->

| Skill                 | What it answers                                                                                                                     |
|-----------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| `i-want-to`           | Route a piece of work in this repository to the process record or playbook with its steps, and to the writing skill for its surface |
| `writing-in-the-tool` | The shape of prose inside `tooling/`                                                                                                |
| `writing-the-docs`    | The shape of the public documentation                                                                                               |

<!-- END GENERATED: skills-here -->

## Reading one

A skill is a Markdown file. A bundled skill sits under `skills/` inside an installed plugin, and `bundle.json` beside
them lists the ones the trim dropped. The rest are in `.claude/skills/` of whichever repository has them.

A skill states its own rules in full, because it has to work in a checkout that has no corpus.
