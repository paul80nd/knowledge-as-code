# Checks: where a check comes from

## What it is for

`kac` declares every check it can report in the schema, and prints that declaration back with `kac checks`. The schema
decides what runs and against which type, so nothing is hard-coded per type. The corpus itself answers "what will CI
check this corpus for", and nobody keeps a second catalogue by hand.

Only an error fails the build. `info` is for a check declared in `_checks.yaml`. A type's own rules report at `error`
or `warning`, which is what
[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json) limits
a `severity:` there to.

Read this page when you are adding a check, or deciding whether the check you want already exists.
[`validate`](../use/cli/validate.md) says which files a check runs against.

## What it is not

**It is not `validate`.** `kac checks` reads the schema and prints what could fire. It opens no record and reports no
fault in one. A check absent from a validate run has either not been declared or not been tripped, and this is where the
two are told apart.

**It is not the table on a type page.** That table is grouped and worded by hand for whoever writes a record of the
type, and several catalogue ids fold into one row of it. The catalogue is flat and keyed by id. `kac checks` exits
non-zero once the two have drifted, and that exit is what keeps the table honest.

**It is not a prose reviewer.** A check that reads prose matches a closed set of words: the hedges
`no-hedged-ordering` warns on, the modals a type declares, a heading title, a literal link target. An open-class
rule, such as "a step opens with an imperative verb", needs a word list nobody can complete. A rule like that
belongs in a standard and in the skill an agent loads, and an agent sweeps for it.

## How it works

**A check is defined once, in the schema.**
[`.schema/_checks.yaml`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/_checks.yaml) declares the
checks that run against every document, and the ones that read the schema or the corpus rather than a record. An entry
gives its severity, what it proves correct, the reasoning behind it, and whether it belongs on a type page. The banner
comments grouping them are for whoever reads that file, and the loader passes over them like any other comment. A type's
own rules are declared beside the type, in `.schema/<type>.yaml`. Between them they are every check the validator can
report.

### The schema itself

`kac` reads the schema before it reads any document, and checks it for what the tool can act on: a rule nothing
dispatches, a key the loader never reads, a value no code branches on. That pass goes first, because the schema decides
how every document below it is read. A finding there names the schema file and the key it found.

[What the schema is held to](held-to.md) is the account of what that pass reports, and why an inert declaration counts
as a defect. It is written for whoever authors a type file. In a corpus that took this framework, that is somebody who
cannot ask what a key was meant to do, which is the reason the pass exists at all.

### A type's own rules

A rule fires against the documents of the type whose schema declares it. A rule written as an `expr:`, a one-line
condition the schema states and the tool evaluates, reports under its own id. Adding one is adding YAML rather than
editing this tool, and [Rule expressions](expressions.md) is the reference for what one may say.

A question the grammar cannot ask needs a rule written in C# instead, and that rule names the check id it reports under.
`no-dependency-cycles` reports as `dependency-cycle`. Many of the rules declared in the schema run nothing yet, and
`kac checks` prints the ones that do report. [Checks in C#](../maintaining/checks.md) says how such a rule is written.

A list the schema asks an author to fill takes a floor, set with `min-items:`.

The schema also declares rules that do not run. Each is an intention, with a `description:` and no `severity:`, and the
type page renders it under **Declared, not yet enforced**. Naming a severity without running is the one arrangement this
forbids, and `schema-dispatch` is what forbids it.

A rule that counts words, links or days sets a ratio, a ceiling or a window, and the number in it is a judgement. Three
of those numbers are calibrated:

| Rule                      | Window                                                                                  |
|---------------------------|-----------------------------------------------------------------------------------------|
| `high-risk-review-window` | 184 days from `accepted-on` to `review-by`, on a deviation whose `risk` is `high`       |
| `staleness`               | 92 days for `quarterly` and 366 for `annual`, the longest quarter and the longest year  |
| `staleness-loud`          | 92 days for a runbook, whatever its `rehearsal-frequency`                               |

`staleness` measures `quarterly` and `annual` alone. `on-change` and `per-release` state an event, so a process using
either is reported only where `last-rehearsed` is `never`. `staleness-loud` does not read `rehearsal-method`, so a
tabletop walk earns the same 92 days as a live one.

A rule that matches text keeps its pattern in `.schema/`, so tuning one is a schema edit. Most such rules read the
document *as written*, so a credential pasted into a fenced block is still found. `target-is-measurable` is the
exception. It reads a frontmatter value, which the body patterns do not see.

Code is excluded from every link and marker check. Those checks read the parsed document, and a fenced or indented block
parses to no link and no marker for them to find.

### The key-order rule

Two files declare field order, and both name `status`. `_universal.yaml` orders
`id, type, tier, status, owner, sources, tags`. The type file orders the rest, which for an ADR is
`status, decided-on, supersedes, superseded-by, deciders, related`. Neither file states one order for the merged set.

So `kac` checks a document against both chains at once, rather than against a single invented order. Every pair the
schema does order must hold. A pair it leaves alone, such as `owner` against `decided-on`, is free. Genuine disorder
still fails: `tags` before `id`, or `related` before `status`.

### Which check reports a fault

One fault is reported once, by whichever check owns it. `required-field` reports an absent field, so a `required-when:`
naming that field asks for nothing further. `reciprocal` leaves a dangling or wrong-type reference to `ref-resolves`.
`clause-table` reports a broken table and stops the pass, because nothing under it can be judged, and a missing section
belongs to `required-section`. `date-format` reports a day the calendar does not have under the same id as a malformed
one, because the author rewrites the value either way.

### Faults nothing else would catch

Three checks exist because what they refuse looks correct to everything around it.

A part declared by a heading with nothing under it has a title, a working anchor, and a citation that resolves.
`kac export` then ships it as a part of no words. `part-empty` is what refuses it.

A reference label spelled `[ADR-0013]` resolves. Markdown matches a label to its definition case-insensitively, so the
use site may spell it differently from the definition. `link-resolves` passes as soon as the path is real.
`label-canonical` compares the label the page shows with the id instead.

An id pointing at a record of the wrong type lands on a real page, so it reads as intentional, and whatever walks the
edge afterwards takes it at its word. `ref-resolves` asks the type as well as the path.

## Known limits

**`immutable-after-accepted` is declared and does not run.** It asks that an accepted document's decision changes only
by supersession. Telling that from a correction needs git history, which the static validator has none of. It belongs in
a CI step that can read a diff.
