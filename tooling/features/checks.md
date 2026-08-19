# `checks` — where a check comes from

## Intent

Every check `kac` can emit is read from the schema and dispatched by id; nothing here is hard-coded per type. `kac
checks` prints that catalogue for the corpus it is run in, so "what will CI hold this corpus to" is answered by the
corpus rather than by a document somebody remembers to maintain. Its readers are whoever is adding a check and whoever
is deciding whether the check they want already exists. Which files a check runs against is
[`validate.md`](validate.md).

## What it is not

**It is not `validate`.** `kac checks` reads the schema and prints what could fire. It opens no record and reports no
fault in one. A check absent from a validate run has either not been declared or not been tripped, and this is where
the two are told apart.

**It is not the table on a type page.** That table is grouped and hand-worded for whoever writes a record of the type,
and several catalogue ids fold into one row of it. The catalogue is flat and by id. `kac checks` exits non-zero where
the two have drifted, which makes it the thing that holds the table honest rather than a second copy of it.

## Approach

A check marked **warning** does not fail the build.

**A check is defined once, in the schema.** [`../../example/.schema/_checks.yaml`](../../example/.schema/_checks.yaml)
declares the checks that run against every document, one entry each: its severity, the group it belongs to, what it
proves correct, and the reasoning behind it. A type's own rules are declared beside the type, in `.schema/<type>.yaml`.
Between them they are every check the validator can emit.

**`kac checks` prints what runs**, read from the schema of the corpus it is run in, and exits non-zero where the
reader-facing table on a type page has drifted from it.

### The schema itself

Before any document is read, the schema is held against what the tool can act on: a rule nothing dispatches, a key the
loader never reads, a value no code branches on. It goes first in the run, because the schema decides how every
document below it is read, and a finding there names the schema file and the key rather than a record.

[`../../example/.schema/README.md`](../../example/.schema/README.md) is the account of what that pass reports and why
an inert declaration is treated as a defect. It is written for whoever authors a type file, which in a corpus that took
this framework is somebody who cannot ask what a key was meant to do — and that is the reason the pass exists at all.

### A type's own rules

A rule fires against the documents of the type whose schema declares it, and reports under its own id. Most are answered
by an `expr:` — a one-line condition the schema states and the tool evaluates, so adding one is adding YAML rather than
editing this tool; [`../../example/.schema/README.md`](../../example/.schema/README.md) is the reference for what one
may say. The rest are a class each in `kac.core/Rules/`, with unit tests beside them, for the questions the grammar
cannot ask.

`dependency-cycle` is the one that asks about the records together rather than about each one. It is reported once per
loop against the lowest id on it.

The schema declares roughly as many rules again that do not run — intentions, carrying a `description:` and no
`severity:`, rendered on their type page under *Declared, not yet enforced*. Naming a severity without running is the
one arrangement this forbids, and `schema-dispatch` is what forbids it.

A rule that counts words or links is a ratio or a ceiling whose number is a judgement rather than a measurement — no
corpus has yet held enough of those types to calibrate one. Each is pinned by a fixture, so changing it is visible.

A rule that matches text is a heuristic, and its pattern lives in `.schema/` for that reason: a heuristic gets tuned,
and tuning a regex there is a schema edit rather than a release every corpus has to take. Most read the document **as
written** — a credential pasted into a fenced block is the case they exist for, and the flattened text a word count
walks would never see it. `target-is-measurable` is the exception: it reads a frontmatter value, which the body patterns
deliberately cannot see, because a field is judged against what its own declaration says.

Code is excluded from every link and marker check: they walk the Markdig AST (inline links, literal runs), and fenced or
indented code carries none of those nodes.

### The key-order rule

The schema specifies field order across two files that share the `status` key: `_universal.yaml`
(`id, tier, status, owner, tags`) and the type file (for ADRs: `status, decided-on, supersedes,
superseded-by, deciders, related`). Neither states a single total order for the merged set.

Rather than invent one arbitrary total order, `kac` enforces that a document's key order is a **topological extension**
of both declared chains: every pair the schema *does* order must hold, and pairs it leaves unconstrained (e.g. `owner`
versus `decided-on`) are free. This is fully derived from the schema, matches both the `metadata.md` example and the ADR
corpus, and still catches genuine disorder (`tags` before `id`, `related` before `status`, and so on).

## Decisions

**A rule is data wherever it can be.** Wiring a rule as C# means a class, a registry line, unit tests, a row in
`Generator.DocRows`, a row in two reference pages, and a fixture. Wiring it as an expression means a line of YAML and a
fixture. That difference is the whole argument, and it compounds: a corpus that has *taken* this framework rather than
authored it may add a whole type file of its own, and before this layer existed every rule in one was inert — enforcing
it needed an upstream code change and a release.

OPA/Rego was the obvious alternative and is the wrong shape. It would replace only the evaluation *tail* of the
pipeline, leaving all the markdown and frontmatter extraction untouched, while adding a language and a runtime
dependency and breaking the single-file, no-build-step design. A small hand-rolled evaluator buys the one property worth
having — new rules as data — at a fraction of that. `RuleExpr.cs` says when that judgement expires.

| File                         | Holds                                                                                 |
|------------------------------|---------------------------------------------------------------------------------------|
| `kac.core/Facts.cs`          | the fact functions, and nothing else an expression can reach                          |
| `kac.core/RuleExpr.cs`       | lexer, recursive-descent parser, type checker, evaluator — no dependencies            |
| `RuleSpec` in `Schema.cs`    | `Expr`, `Compiled`, `Severity`, `Message`; `ParseRule` compiles at load               |
| `kac.core/Rules/`            | one class per rule that needs C#, and the registry each dispatcher looks them up in   |
| `Validator.CheckRules`       | evaluates every compiled rule, and looks up by id the ones that are not               |
| `Validator.CheckCorpusRules` | runs the rules that read every record at once, over the index the corpus checks build |

`CheckRules` emits at the rule's own severity, which is why it is not `CheckWarnings`. `Facts` is built per document and
discarded once its rules have run, which is what makes `words()` safe to memoise there rather than on the immutable
`Doc`.

## Known limits

- **`immutable-after-accepted`** (content of an accepted document must not change) needs git history and is not
  implemented in the static validator; it belongs in a diff-aware CI step.
