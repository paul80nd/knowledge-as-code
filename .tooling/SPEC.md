# The expression layer for `rules:`

Every type schema carries a `rules:` block. A rule that declares an `expr:` is a working check: the tool compiles it at
load, evaluates it against every document of that type, and reports under the rule's own id. A rule without one is a
statement of intent — a behaviour someone wanted, written down, that nothing answers to yet.

This file is the reference for that layer: what an expression may say, why the boundaries are where they are, and what
is still to be converted.

**Where things stand.** 46 rule entries across the type schemas: **18 expressions**, **2 rule classes**, and
**26 statements of intent**. Every rule an expression could answer has been converted; what is left will not be one —
see [Stays C#](#stays-c) and [Not validator work at all](#not-validator-work-at-all).

## Why an expression and not a policy engine

Wiring a rule as C# means a class, a registry line, unit tests, a row in `Generator.DocRows`, a row in two READMEs,
and a fixture. Wiring it as an expression means a line of YAML and a fixture.
That difference is the whole argument, and it compounds: a corpus that has *taken* this framework rather than authored
it may add a whole type file of its own, and before this layer existed every rule in one was inert — enforcing it
needed an upstream code change and a release.

OPA/Rego was the obvious alternative and is the wrong shape. It would replace only the evaluation *tail* of the
pipeline, leaving all the markdown and frontmatter extraction untouched, while adding a language and a runtime
dependency and breaking the single-file, no-build-step design. A small hand-rolled evaluator buys the one property
worth having — new rules as data — at a fraction of that.

## The YAML shape

A rule **fires a finding when its `expr` evaluates false**, so the expression reads as the condition that ought to hold
rather than as the fault.

```yaml
rules:
  - id: symptoms-first
    description: Symptoms is the first section after the H1 — that is how the reader finds the document.
    severity: error
    expr: "first_section() == 'Symptoms'"
    message: >
      Symptoms must be the first section. Someone reaching for this at 2am matches on what they are
      seeing, not on what the document is called.
```

* `description:` is what the rule *means*, and is rendered into the generated `## What CI checks` table on the type
  page. `message:` is what an author is told when it fires. One is a definition, the other a diagnosis; do not make them
  the same sentence.
* A rule carrying an `expr:` **must** declare a severity and a message. A rule claiming to be finished is held to being
  able to report, and the load fails otherwise.
* A rule without an `expr:` keeps `id` + `description` and is skipped by the dispatcher.

Expression *strings*, not nested predicate objects: several rules are conditionals (`A implies B`) or ratios, and those
read cleanly inline but become ugly YAML trees.

## Grammar

Frozen. Extending it is a deliberate decision, not a convenience.

```
expr    := implies
implies := or ( "implies" or )*          // A implies B  ≡  (not A) or B
or      := and ( "or" and )*
and     := cmp ( "and" cmp )*
cmp     := add ( ("=="|"!="|"<"|"<="|">"|">=") add )?
add     := mul ( ("+"|"-") mul )*        // needed only for `links() * 40` style ratios
mul     := unary ( ("*"|"/") unary )*
unary   := "not" unary | primary
primary := STRING | INT | call | "(" expr ")"
call    := IDENT "(" ( expr ("," expr)* )? ")"
```

* **Types:** string, int, bool. No boolean literals — every condition starts from something the document says.
* **Strings** are single-quoted, and a doubled quote is one quote, as in YAML and SQL. There are no backslash escapes:
  the strings that most need a quote in them are regular expressions, and a second escaping layer over those is how
  they stop being readable.
* **A comparison is not chainable.** `1 < words() < 40` is a sentence rather than a condition, and the parser declines
  it instead of choosing an associativity nobody asked for.
* **Dates** compare correctly as ISO strings lexicographically, so `field('a') >= field('b')` works without a date type.
  Keep it that way.
* **Division by zero yields zero.** Nothing in the taxonomy divides; the operator exists because the grammar is frozen,
  and a rule tripping over it should read as a threshold nobody meets rather than crash mid-corpus.
* **No** variables, user-defined functions, quantifiers or collections. That boundary is the point.

### Absence, and the guard that follows from it

`field(...)` returns string-or-null. **A comparison where either side is absent is false**, and `!=` is the negation of
`==`, so it is true. One rule for every operator.

The consequence is an idiom. `field('detected-on') >= field('occurred-on')` fires on a postmortem missing a date —
where `required-field` has already said so, in better words. A rule about a field that may be absent guards it:

```yaml
expr: "present('detected-on') and present('occurred-on') implies field('detected-on') >= field('occurred-on')"
```

This is why converted rules read longer than a naive sketch of them. The alternative — each operator guessing which way
silence should fall — trades one explicit guard for a table of special cases nobody remembers.

### Compile-time checking

An expression is parsed **and type-checked** at load, and anything wrong stops the load naming the rule. That covers a
syntax error, an unknown fact, the wrong number of arguments, a comparison between a number and text, arithmetic on
text, and a whole expression that is not a yes/no question.

This matters more than it looks. Without it, `words() == 'three'` compiles and then evaluates false for the life of the
schema — a check that appears wired up and never fires, which is the exact failure this layer exists to end.

### Fact functions — the only callable surface

Everything an expression can see. Each reads what the parse pass already produced: **the evaluator never re-parses
markdown.**

| Function                         | Returns | Reads                                                                                                |
|----------------------------------|---------|------------------------------------------------------------------------------------------------------|
| `field('name')`                  | string? | a frontmatter scalar                                                                                 |
| `present('name')`                | bool    | that scalar, non-empty — false for a bare key as well as a missing one                                |
| `field_matches('name', 're')`    | bool    | that scalar against a pattern — false where absent; the one pattern fact that sees frontmatter        |
| `section('Title')`               | bool    | whether an H2 of that name exists (case-insensitive)                                                 |
| `section_count('Title')`         | int     | how many times it appears — `section()` asks whether, this asks how many                              |
| `first_section()`                | string  | the first H2, or empty where there is none                                                           |
| `links()`                        | int     | how many links the body carries                                                                      |
| `words()`                        | int     | every heading and paragraph the document **renders** — frontmatter and fenced code carry no inline content and fall out |
| `matches('re')`                  | bool    | the body **as written** — code fences, link targets and markdown syntax included; frontmatter is not |
| `section_matches('Title', 're')` | bool    | the same, bounded to one section; false where the document holds no such section                     |

**`words()` and `matches()` deliberately see different documents.** One walks the rendered text, the other the source.
That is what lets `matches` find a credential pasted into a fenced block — the case those rules exist for — and find
`**MUST**`, an obligation the rendered text would have flattened into an ordinary word. Do not simplify one onto the
other; a unit test pins the difference.

Adding a fact is adding one method to `Facts` and one row to `RuleExpr.Functions`, which is what the type checker reads.
The grammar never changes.

`Facts` is built per document and discarded once its rules have run, which is what makes `words()` safe to memoise
there rather than on the immutable `Doc`.

## The C# behind it

| File                       | Holds                                                                                      |
|----------------------------|--------------------------------------------------------------------------------------------|
| `kac.core/Facts.cs`        | the fact functions, and nothing else an expression can reach                               |
| `kac.core/RuleExpr.cs`     | lexer, recursive-descent parser, type checker, evaluator — no dependencies                  |
| `RuleSpec` in `Schema.cs`  | `Expr`, `Compiled`, `Severity`, `Message`; `ParseRule` compiles at load                     |
| `kac.core/Rules/`          | one class per rule that needs C#, and the registry the dispatcher looks them up in          |
| `Validator.CheckRules`     | evaluates every compiled rule, and looks up by id the ones that are not                    |

`CheckRules` emits at the rule's own severity, which is why it is not `CheckWarnings`.

**A rule that needs C# is a class.** `IDocumentRule` gives it the document, the type, and the `RuleSpec` the schema
declared — so a threshold like `max-words` is read from the schema rather than held as a constant — and it declares
the `CheckDef`s it emits, which is where `CheckCatalogue.All` gets them. The rule id and the check id are separately
named because they differ: `y-statement-present` reports under `y-statement`.

That the dispatcher is a dictionary is the smaller half of it. The larger half is that a rule can be unit-tested on
its own, which is the only way to hold a rule with three ways to fail honest — the coverage gate reads ids, so it
would be green on any one of them.

Only the per-document shape has an interface. Cross-document, graph and git-history rules need inputs `RuleContext`
does not carry, and their interface is worth designing against the first real one rather than ahead of it.

## The coverage gate

`kac-tests.cs` reads `kac checks --json` and asserts every id it names has a fixture that exercises it, and that no
golden references an id `kac` no longer emits. `CheckCatalogue.For(schema)` appends each expression rule's
`(id, severity, description)` to the core catalogue, so **a rule cannot ship without a fixture** any more than a core
check can. `Commands.Checks` takes the repo root for that reason: the catalogue is a property of a corpus's schema, not
of the tool.

**The reader-facing table follows a different rule for each kind.** `Generator.DocRows` groups several core check ids
into one hand-worded row, and `ChecksTableProblems` fails until a new core check has one. An expression rule reports
under its own id and its `description:` is already that row written out, so `ChecksTable` renders it from the schema.
Copying it into `DocRows` would be the same sentence in two files, drifting apart at the first edit.

**The gate reads ids, not branches.** `y-statement-present` reports three faults under one id — an absent
block-quote, a missing move, and a Y-statement past `max-words` — and a fixture for any one of them satisfies the
gate. A rule with more than one way to fail needs a fixture for each, and this sentence is the only thing that says
so.

**Standing a rule up costs more in fixtures than in schema.** Converting five rules was 24 lines of YAML and ~590 of
fixture, because each needed its type stood up in the fixture corpus — a page, a template and a record. That cost is
not a cost of *this layer*: the gate demands a fixture however a check is implemented. It is mostly one-off, too; once
a type is present, the next rule on it is a record.

## What is worth converting

**The test is what the author is told.** Convert a rule when one fixed message says everything the C# would have said.
Keep the C# where it can name *which* part of the document is at fault and a single string cannot. A rule that reports
"something here is wrong" where it could have named the missing piece has been made cheaper and worse, and nothing in
the gate will notice.

Cost is the second question, and it only ever argues for converting a rule that has already passed the first. A schema
with no C# behind it was never the aim.

### Nothing, and that is the point

There is no queue. Every rule the test admits has been converted, which is what makes the two facts added last worth
knowing about: `section_count()` and `field_matches()` were each written for one rule, and each is now the obvious
answer to a question a corpus will ask again. Adding a fact is one method on `Facts` and one row in
`RuleExpr.Functions`; adding to the grammar is not, and the two are easy to confuse when a rule will not quite fit.

### Stays C#

Sixteen rules need git history, a graph walk, more than one document at once, or a message an expression cannot write.
**If you find yourself wanting loops, joins or quantifiers in the grammar to reach one of these, stop** — that is the
signal you are rebuilding OPA. Write a rule class.

They cluster, which is worth knowing before starting any of them:

* **Git history — 4.** `immutable-after-accepted`, `immutable-after-published`, `changelog-begins-at-active`,
  `changelog-on-material-change`. All four ask the same question: what changed in this commit versus the committed
  content, and was it substantive? One mechanism answers all of them, and it is the largest single piece of work left.
* **Cross-document — 6.** `store-has-service`, `not-load-bearing`, `constraint-consistency`, `rules-have-controls`,
  and the corpus-wide glossary pair `undefined-terms` and `unused-terms`. `Validator.CheckCorpus` already builds a
  `byId` index and resolves clause citations and reciprocals against it; these are more of that.
* **Graph — 1.** `no-dependency-cycles`.
* **Per-part — 4.** `alternatives-have-verdicts`, `terms-are-singular`, `carried-in-full-by-digest` and
  `escalation-required`. Each judges the parts of one document — bullets under a heading, entries in a glossary,
  branches of a diagnosis tree — and its message has to name the part that failed. The grammar has no collections by
  design, and a count would not name the entry. Only the first is written.
* **A fixed form — 1.** `y-statement-present`. A Y-statement is six moves in one block-quote, and the message worth
  reading names the move that is absent. An expression could report that the block-quote is not a Y-statement, which
  is the one thing the author already knows. `RuleSpec.MaxWords` belongs with it, for the ordinary reason a threshold
  sits in the schema: the ceiling is a judgement a corpus tunes, and tuning it should not be a release.

### Not validator work at all

Eight rules say **Scheduled** in their own descriptions: `feature-file-orphans`, `coverage-report`, `expiry-sweep`,
`recurring-root-causes`, `staleness`, `staleness-loud`, `drift-against-repos`, `drift-against-manifests`. They are
periodic reports over a whole corpus, several needing external state (repository lists, package manifests), and `kac`
has no execution model for them — there is `validate`, `index`, `checks`, `mechanism`, and nothing that runs on a
timer. `reverse-dependencies-generated` is a *generator* and belongs with `kac index`.

Counting these as unenforced rules makes the ruleset look less finished than it is. They are an unbuilt feature, not a
backlog.

### Two that are not rules about a document

* **`blameless`** flags personal names in the Timeline, Root cause and Contributing factors sections. No regular
  expression identifies a personal name: every shape that matches `Alex Doe` also matches `Root Cause`, and the
  corpus's `alex.doe` handle style also matches `example.com` and `kac.core`. It needs a name list or nothing.
* **`human-confirmed`** wants `confirmed-by` to be a person rather than an agent or a session id. That is a `pattern:`
  on the field, not a rule about the document.

## Traps

* **A rule restating something the schema already declares is worse than no rule.** A `reciprocal:`, a
  `mirrors-section:`, a `required-when:`, a scalar type, a required section — each has been written out as a rule
  here, and each read as outstanding work for as long as it survived. `personal-data-has-retention` is the one still
  standing — `data.yaml` declares `retention` as `required-when: 'classification in [personal, special-category]'`, and
  the validator already reports exactly that — and it is
  [issue #83](https://github.com/paul80nd/knowledge-as-code/issues/83), to be either deleted or given a question the
  field declaration cannot ask. Before converting anything, read the field declaration and the `sections:` block; the
  rule may already be answered.
* **`required-when` is a different language and stays one.** It reads `==`, `!=` and `in [...]`, tests one field
  against one other, and lives on the field. A condition needing more than that is a rule with an `expr:`.
  See [`../.schema/README.md`](../.schema/README.md).
* **Thresholds are judgements.** `words() <= links() * 40` and `words() <= 200` were chosen, not measured — no corpus
  has held enough of those types to calibrate them. Each is pinned by a fixture so moving one is visible. Note a ratio
  fails a document linking to nothing at any length; for a capability or an explanation that is the intended reading.
* **The text rules are heuristics** and will be tuned wrong first. That is the argument for holding their patterns in
  `.schema/`: tuning a regex there is a schema edit a corpus owner makes, where the same regex in C# is a release every
  corpus has to take.

## When to abandon the in-house evaluator

Keep the grammar at the surface above. The moment a real need appears for variables, function definitions or
quantifiers, swap the hand-rolled evaluator for **CEL** (Common Expression Language; a .NET port exists) — the `expr:`
strings largely carry over and the engine drops in. Not before: the dependency is not worth it at this size.

## Non-goals

* A general policy engine. No rules-as-data beyond the frozen grammar.
* A date or collection type system.
* Runtime, tenant-specific or externally-contributed rule sets.
* Replacing a check that stays C#.
