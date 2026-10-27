# Checks: where a check comes from

## What it is for

`kac` declares every check it can report against a corpus (one repository of knowledge records kept in git) in the
schema, and prints that declaration back with `kac checks`. The schema
decides what runs and against which type, so nothing is hard-coded per type.

Only an error fails the build. `info` is for a check declared in `_checks.yaml`. A type's own rules report at `error`
or `warning`, which is what
[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json) limits
a `severity:` there to.

Read this page when you are adding a check, or deciding whether the check you want already exists.
[`validate`](../use/cli/validate.md) says which files a check runs against.

## What it is not

**It is not `validate`.** `kac checks` reads the schema and prints what could fire. It opens no record.

**It is not the table on a type page.** That table is grouped and worded by hand, and several catalogue ids fold into
one row of it. `kac checks` exits non-zero once the two have drifted.

**It is not a prose reviewer.** A check that reads prose matches a closed set of words: the hedges `no-hedged-ordering`
warns on, the modals a type declares, a heading title, a literal link target. An open-class rule, such as "a step opens
with an imperative verb", belongs in a standard and in the skill an agent loads.

## How it works

**A check is defined once, in the schema.**
[`.schema/_checks.yaml`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/_checks.yaml) declares the
checks that run against every document, and the ones that read the schema or the corpus. An entry gives its severity,
what it proves correct, and whether it belongs on a type page. A type's own rules are declared beside the type, in
`.schema/<type>.yaml`. Between them they are every check the validator can report.

### The schema itself

`kac` checks the schema before it reads any document, and a finding there names the schema file and the key.
[What the schema is held to](held-to.md) lists what that pass reports.

### A type's own rules

A rule fires against the documents of the type whose schema declares it. A rule written as an `expr:`, a one-line
condition the schema states and the tool evaluates, reports under its own id. Adding one is adding YAML rather than
editing this tool, and [Rule expressions](expressions.md) is the reference for what one may say.

A rule written in C# names the check id it reports under: `no-dependency-cycles` reports as `dependency-cycle`.
[Checks in C#](../maintaining/checks.md#when-a-rule-needs-c) says when a rule needs one, and how it is written.

A list the schema asks an author to fill takes a floor, set with `min-items:`. A rule with no `severity:` runs nothing,
as [A rule with no `severity:`](held-to.md#a-rule-with-no-severity) says.

A rule that counts words, links or days sets a ratio, a ceiling or a window. Three windows are fixed:

| Rule                      | Window                                                                                  |
|---------------------------|-----------------------------------------------------------------------------------------|
| `high-risk-review-window` | 184 days from `accepted-on` to `review-by`, on a deviation whose `risk` is `high`       |
| `staleness`               | 92 days for `quarterly` and 366 for `annual`, the longest quarter and the longest year  |
| `staleness-loud`          | 92 days for a runbook, whatever its `rehearsal-frequency`                               |

`staleness` measures `quarterly` and `annual` alone. `on-change` and `per-release` state an event, so a process using
either is reported only where `last-rehearsed` is `never`. `staleness-loud` reads `last-rehearsed.at` alone, so a
tabletop walk earns the same 92 days as a live one.

A rule that matches text keeps its pattern in `.schema/`. Most such rules read the document *as written*, so a
credential pasted into a fenced block is still found. `target-is-measurable` reads a frontmatter value instead.

Code is excluded from every link and marker check, because a fenced or indented block parses to no link and no marker.

### The key-order rule

Two files declare field order, and both name `status`. `_universal.yaml` orders
`id, type, tier, status, owner, sources, tags`. The type file orders the rest, which for an ADR is
`status, decided-on, supersedes, superseded-by, deciders, related`. Neither file states one order for the merged set.

`kac` checks a document against both chains at once. Every pair the schema orders must hold, and a pair it leaves
alone, such as `owner` against `decided-on`, is free. `tags` before `id`, or `related` before `status`, fails.

### Which check reports a fault

One fault is reported once, by whichever check owns it:

* `required-field` reports an absent field, so a `required-when:` naming that field asks for nothing further.
* `reciprocal` leaves a dangling or wrong-type reference to `ref-resolves`.
* `clause-table` reports a broken table and stops the pass, and a missing section belongs to `required-section`.
* `date-format` reports a day the calendar does not have under the same id as a malformed one.

### Faults nothing else would catch

| Check             | Refuses                                                                                |
|-------------------|----------------------------------------------------------------------------------------|
| `part-empty`      | a part declared by a heading with nothing under it, which would export with no words   |
| `label-canonical` | a reference label spelled differently from the id it names, as `[ADR-0013]`            |
| `ref-resolves`    | an id pointing at a record of the wrong type, as well as one pointing at nothing       |

## Known limits

**Some checks run only under `validate --since`.** `immutable-after-accepted`, `immutable-after-published` and
`retire-not-delete` compare a record with its state at an earlier revision, so a run that names none skips them.
[Comparing with a base revision](../use/cli/validate.md#comparing-with-a-base-revision) says what each reports.
