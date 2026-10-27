# The surface measurement

This measurement counts each surface `kac` and this repository present, and names what reaches each one. A surface
is something a reader can meet: a command, a flag, a check, a type, a schema field, a plugin component, a page of the
documentation site, or a process, playbook or skill. Whoever decides what to cut before v1 reads the result beside the
trial's readings, in [#1165](https://github.com/paul80nd/knowledge-as-code/issues/1165).

It changes nothing. It reads the files git tracks at one commit, so two runs over the same commit print the same
reading.

## Take a reading

1. Build `kac`: `dotnet build tooling/kac`.
2. From the repository root, run `python3 trials/surfaces/measure.py > trials/surfaces/runs/<date>.md`.

The script needs Python 3 with PyYAML. Installing `.github/requirements.txt` brings PyYAML in with `yamllint`.

### Measure one corpus

Run `python3 trials/surfaces/measure.py --corpus <path>` to measure a corpus outside this repository, such as the
adopting corpus. The reading has the types, fields and rules tables alone, limited to the types that corpus adopted.
It contains counts, ids and field names, and no record text, so it can leave the organisation.

## What each column counts

A count of files is a count of mentions, not of runs. A file mentioning `kac export --type` twice counts once. Read a
blank cell as "no file in that group mentions it".

| Column        | The files it counts                                                                            |
|---------------|------------------------------------------------------------------------------------------------|
| CI            | the workflows under `.github/workflows/`, and the template's GitHub and Azure DevOps pipelines |
| plugin        | the skills and hooks under `template/.plugin/`                                                 |
| docs          | the site under `docs/`, the root `README.md`, and `tooling/kac/PACKAGE.md`                     |
| guidance      | every `CLAUDE.md`, the skills under `.claude/skills/`, and the process records                 |
| tests         | the unit tests, the feature specs, the golden suite and its fixtures                           |
| trials        | every other trial under `trials/`, with its runs                                               |
| tool          | the C# under `tooling/kac/` and `tooling/kac.core/`                                            |
| schema        | `.schema/`                                                                                     |
| corpora       | the corpora under `examples/` and `template/`                                                  |
| dog, eng, ... | the records of one corpus: `dog-fooding`, `engineering`, `library`, `payments` and `template`  |

Each table reads its surface this way:

* **Commands and flags.** The script reads them from `kac --help` and each command's `--help`. A line counts where it
  names the command after `kac `, after `-- `, inside quotes, or after "I " as a feature step does. A flag counts
  only on the same line as its command. A feature step that describes a flag in words, such as `When I export the
  "glossary" type`, does not count.
* **Types.** The records of each type in each corpus, and the corpora whose descriptor adopts the type.
* **Plugin components.** Each component in `template/.plugin/.claude-plugin/plugin.json`, the types it reads, and
  the words of its `SKILL.md`. **Trial runs** counts the files under a trial's `runs/` naming the component.
* **Processes, playbooks and repository skills.** **Cited by** counts every other tracked file naming the process id,
  the playbook's file name or the skill's name. A generated `_index.md` does not count.
* **Documentation pages.** Each page in the `mkdocs.yml` nav, its words, and the files linking to it. A link counts
  either as a relative link to the page or as its URL on the published site.
* **Schema fields.** **Records setting it** counts the records of that type with a value in the field. A field
  declaring `from:` takes its value from the record's folders, so the table writes `derived`.
* **Rules a type declares.** Each rule under a type's `rules:`, once for each type declaring it, with the records it
  runs over in each corpus. A rule with no severity is `none`: no record is reported against it, because it is not
  yet enforced or a report reads it. The built-in checks run over every record, so this table leaves them out.

## Reading the result

A run has no pass mark. A surface with no mention in any group, or a field no record sets, is a question for #1165:
who is it for? A blank row is not a verdict. The template's corpora are seeds, and the example corpora stand in for
adopters, so a field the trial needs can have no record here yet.

`changelog.md` has 2 words because the page includes `tooling/kac/CHANGELOG.md` at build time.

## Runs

| Run                                      | Commit      | What it is                                                    |
|------------------------------------------|-------------|---------------------------------------------------------------|
| [2026-10-27](runs/2026-10-27.md)         | `375411d43` | the measurement                                               |
| [2026-10-27 fit](runs/2026-10-27-fit.md) | `76bc68189` | whether an example corpus has a place for each unused surface |
