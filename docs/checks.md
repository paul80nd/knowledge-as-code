# Checks: where a check comes from

## Intent

Every check `kac` can emit is read from the schema and dispatched by id. Nothing is hard-coded per type. `kac checks`
prints that catalogue for the corpus it is run in, so the corpus itself answers "what will CI hold this corpus to". No
document has to be remembered and maintained beside it.

Read this page when you are adding a check, or deciding whether the check you want already exists. Which files a check
runs against is [`validate`](cli/validate.md).

## What it is not

**It is not `validate`.** `kac checks` reads the schema and prints what could fire. It opens no record and reports no
fault in one. A check absent from a validate run has either not been declared or not been tripped, and this is where
the two are told apart.

**It is not the table on a type page.** That table is grouped and hand-worded for whoever writes a record of the type,
and several catalogue ids fold into one row of it. The catalogue is flat and keyed by id. `kac checks` exits non-zero
once the two have drifted, and that exit is what keeps the table honest.

## Approach

A check marked **warning** does not fail the build.

**A check is defined once, in the schema.**
[`.schema/_checks.yaml`](https://github.com/paul80nd/knowledge-as-code/blob/main/example/.schema/_checks.yaml) declares
the checks that run against every document, one entry each. An entry carries its severity, the group it belongs to,
what it proves correct, and the reasoning behind it. A type's own rules are declared beside the type, in
`.schema/<type>.yaml`. Between them they are every check the validator can emit.

**`kac checks` prints what runs**, read from the schema of the corpus it is run in.

### The schema itself

Before any document is read, the schema is held against what the tool can act on: a rule nothing dispatches, a key the
loader never reads, a value no code branches on. That pass goes first, because the schema decides how every document
below it is read. A finding there names the schema file and the key it found.

[`.schema/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/example/.schema/README.md) is the account
of what that pass reports, and why an inert declaration counts as a defect. It is written for whoever authors a type
file. In a corpus that took this framework, that is somebody who cannot ask what a key was meant to do, which is the
reason the pass exists at all.

### A type's own rules

A rule fires against the documents of the type whose schema declares it, and reports under its own id. Most are
answered by an `expr:`, a one-line condition the schema states and the tool evaluates. Adding one is adding YAML rather
than editing this tool, and
[`.schema/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/example/.schema/README.md) is the
reference for what one may say. The rest are a class each in `kac.core/Rules/`, with unit tests beside them, for the
questions the grammar cannot ask.

`dependency-cycle` is the one rule that reads every record at once. It reports a loop once, against the lowest id on
that loop.

The schema also declares rules that do not run. Each is an intention, carrying a `description:` and no `severity:`, and
rendered on its type page under *Declared, not yet enforced*. Naming a severity without running is the one arrangement
this forbids, and `schema-dispatch` is what forbids it.

A rule that counts words or links sets a ratio or a ceiling, and the number in it is a judgement. No corpus has yet
held enough records of those types to calibrate one. Each number is pinned by a fixture, so moving it is visible.

A rule that matches text is a heuristic, and a heuristic gets tuned. Its pattern lives in `.schema/` for that reason.
Tuning a regex there costs a schema edit. Moving it in the tool would cost a release every corpus has to take.

Most such rules read the document **as written**. A credential pasted into a fenced block is the case they exist for,
and the flattened text a word count walks would never see it. `target-is-measurable` is the exception. It reads a
frontmatter value, which the body patterns deliberately cannot see, because a field is judged against what its own
declaration says.

Code is excluded from every link and marker check. Those checks read the parsed document, and a fenced or indented
block parses to no link and no marker for them to find.

### The key-order rule

Two files declare field order, and both name `status`. `_universal.yaml` orders `id, tier, status, owner, tags`. The
type file orders the rest, which for an ADR is `status, decided-on, supersedes, superseded-by, deciders, related`.
Neither file states one order for the merged set.

So `kac` holds a document to both chains at once rather than to a single invented order. Every pair the schema does
order must hold. A pair it leaves alone, such as `owner` against `decided-on`, is free. Genuine disorder still fails:
`tags` before `id`, or `related` before `status`.

## Decisions

**A rule is data wherever it can be.** Wiring a rule as C# means a class, a registry line, unit tests, an entry in
`_checks.yaml`, a row in `Generator.DocRows`, and a fixture. Wiring it as an expression means a line of YAML and a
fixture. That difference is the whole argument, and it compounds. A corpus downstream of this one may add a whole type
file of its own. Without this layer every rule in that file would be inert, and enforcing one would need an upstream
code change and a release.

OPA/Rego was the obvious alternative and is the wrong shape. It would replace only the evaluation *tail* of the
pipeline, leaving all the markdown and frontmatter extraction untouched. It would add a language to learn and a runtime
dependency to the tool. The one property worth having is new rules as data, and a small hand-rolled evaluator buys it
at a fraction of that cost. `RuleExpr.cs` says when that judgement expires.

| File                         | Holds                                                                                  |
|------------------------------|----------------------------------------------------------------------------------------|
| `kac.core/Facts.cs`          | the fact functions, and nothing else an expression can reach                           |
| `kac.core/RuleExpr.cs`       | lexer, recursive-descent parser, type checker and evaluator, with no dependencies      |
| `RuleSpec` in `Schema.cs`    | `Expr`, `Compiled`, `Severity` and `Message`. `ParseRule` compiles at load             |
| `kac.core/Rules/`            | one class per rule that needs C#, and the registry each dispatcher looks them up in    |
| `Validator.CheckRules`       | evaluates every compiled rule, and looks up by id the ones that are not                |
| `Validator.CheckCorpusRules` | runs the rules that read every record at once, over the index the corpus checks build  |

`CheckRules` emits at the rule's own severity, which is why it is not `CheckWarnings`. `Facts` is built per document
and discarded once its rules have run. That is what makes `words()` safe to memoise there, where the immutable `Doc`
could not hold it.

## Known limits

**`immutable-after-accepted`** asks that the content of an accepted document does not change. It needs git history,
which the static validator has none of. It belongs in a CI step that can read a diff.
