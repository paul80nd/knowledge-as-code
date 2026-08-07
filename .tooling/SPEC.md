# SPEC — a small expression layer for `rules:`

Status: **built, and being filled in.** The evaluator, the fact context and the dispatcher are in place;
`detected-not-before-occurred` is converted. The rest of Bucket A is outstanding.

## Motivation

Every type schema carries a `rules:` block. Most entries are **prose intent, not executable checks** — over fifty rule
entries across the type schemas, of which two are answered by a hard-coded arm in `Validator.CheckRules`
(`kac.core/Validator.cs`) and one by an expression. Wiring a rule the first way means another arm; wiring it the second
way means a line of YAML.

Fewer than half the entries declare a `severity:`, and a rule without one is declared but not enforced. So the grammar
is only half the work: the other half is deciding, rule by rule, whether it fails a build or advises an author.

The **field-predicate and simple-structural** rules are therefore data: a one-line `expr:` string per rule, evaluated by
a tiny in-house evaluator against a fixed *fact* context. This deliberately does **not** turn `kac` into a general
policy engine (OPA/Rego). The grammar is frozen small on purpose; rules that need real algorithms stay in C#.

See the conversation that produced this for the full reasoning. The one-line version: OPA/Rego would replace only the
evaluation *tail* of the pipeline while leaving all the markdown/frontmatter extraction untouched, add a language +
runtime dependency, and break the single-file, no-build-step design. A ~150-line evaluator buys the one property worth
having — *new rules as data* — at a fraction of the cost.

## Scope — what becomes data, what stays code

The decision hinges on bucketing the **actual** rules in `.schema/*.yaml`.

### Bucket A — a one-line `expr:`

Pure field predicates and simple structural facts, in one uniform shape. **Converted:**

| Rule                           | Type         | `expr`                                             |
|--------------------------------|--------------|----------------------------------------------------|
| `detected-not-before-occurred` | postmortems  | guarded date comparison                            |
| `symptoms-first`               | runbooks     | `first_section() == 'Symptoms'`                    |
| `provenance-required`          | standards    | `present('derived-from') or present('implements')` |
| `hub-not-specification`        | capabilities | `words() <= links() * 40`                          |
| `links-rather-than-restates`   | explanations | `words() <= links() * 40`                          |
| `low-ceremony`                 | discoveries  | `words() <= 200`                                   |

The three thresholds are judgements, not measurements — no corpus has yet held enough of these types to calibrate them.
Each is pinned by a fixture, so moving one is a visible change rather than a silent one. Note that a ratio means a
document linking to nothing fails at any length: for a capability or an explanation, whose whole job is to point
elsewhere, that is the intended reading.

**Two rules need a schema change before they need an expression.** `trial-has-criteria` names a section `tools.yaml`
does not list, and `deprecated-has-successor` names a `successor` field where `tools.yaml` has `replaces`, which points
the other way.

**Six rules that look like Bucket A are not:**

* `what-went-well-required`, `escalation-required` and `verification-required` name sections their types already declare
  **required**. Converting them would report one absence twice, in two vocabularies.
* `unused-integrations` and `target-is-measurable` (the `present` half) name fields already declared `required: true`.
* `carried-in-full-by-digest` bounds a glossary *entry*. The glossary is a single-document type, so `words()`
  measures the whole page — the rule needs a per-entry fact, which puts it in Bucket B.

**Two more belong to `required-when`, not here.** `personal-data-has-retention` and `mechanism-has-evidence` restate
conditions the schema already declares — `classification in [personal, special-category]` and
`mechanism != not-enforced`. Both are dead, because `Validator.RequiredWhenHolds` splits on `==` and understands nothing
else, so neither condition has ever fired. Teaching it `!=` and `in [...]` fixes the declaration a schema author already
wrote; routing them through `expr:` instead would leave the misleading `required-when` in place.

**A rule about a field that may be absent must guard it.** A comparison where either side is absent is false, so
`field('detected-on') >= field('occurred-on')` fires on a postmortem missing a date — where `required-field` has already
said so. Write
`present('a') and present('b') implies field('a') >= field('b')`. The guard is why the converted rule reads longer than
the sketch it replaced.

### Bucket B — needs one new *fact*, then it's an `expr:`

Add a derived measurement to `Doc` (extraction pass) and expose it as a `Facts` method; the rule itself is then a normal
expression.

| Rule                  | New fact(s) needed                                                                   |
|-----------------------|--------------------------------------------------------------------------------------|
| `y-statement-present` | `has_ystatement()`, `ystatement_words()` (word count of the block-quote after H1)    |
| `fallback-required`   | a body/section content probe                                                         |
| `no-credentials`      | `body_matches('<regex>')` — borderline; keep the regex in YAML, the scan in the fact |

Do these lazily — add the fact when you implement the rule, not up front.

### Bucket C — stays C# (never a grammar feature)

Git history, graph analysis, external data, cross-document joins, corpus-wide reporting. If you ever feel tempted to add
loops/joins/quantifiers to the grammar to express one of these, **stop** — that is the signal you are rebuilding OPA.
Write a dedicated arm instead.

- `immutable-after-accepted`, `immutable-after-published` — git diff of committed content
- `no-dependency-cycles` — graph over `depends-on`
- `drift-against-repos` — external repo state
- `rules-have-controls`, `constraint-consistency` — cross-document
- `coverage-report`, `expiry-sweep`, `undefined-terms` — corpus-wide / reporting
- `reciprocal-supersession`, `related-matches-section` — already core checks; leave as-is

## YAML shape

Expression *strings*, not nested predicate objects — several rules are conditionals (`A implies B`) and ratios that read
cleanly inline but become ugly YAML trees. A rule **fires a finding when `expr` evaluates false**.

```yaml
rules:
  - id: detected-not-before-occurred
    severity: error
    expr: "field('detected-on') >= field('occurred-on')"
    message: "detected-on must be on or after occurred-on."

  - id: deprecated-has-successor
    severity: warning
    expr: "field('status') == 'deprecated' implies present('successor')"
    message: "a deprecated tool must name its successor."

  - id: hub-not-specification
    severity: warning
    expr: "words() <= links() * 40"
    message: "prose has outgrown the links — this capability is drifting into specification."
```

Rules that stay in Bucket C keep their current shape (`id` + `description` + bespoke fields, no
`expr`). The dispatcher skips any rule without an `expr` and lets the existing C# arm handle it.

## Grammar (frozen — do not extend without a deliberate decision)

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

- **Types:** string, int, bool. There are no boolean literals — every condition starts from something the document says.
- **Absence:** `field(...)` returns string-or-null. A comparison where either side is absent is **false**, and `!=` is
  the negation of `==`, so it is **true**. One rule for every operator, so a rule that cares writes the guard rather
  than working out which way silence falls.
- **Dates** compare correctly as ISO strings lexicographically, so `field('a') >= field('b')`
  works without a date type. Keep it that way; do not add a date type.
- **Type-checked at compile time**, over those three types. An expression that could never mean anything —
  `words() == 'three'`, `field('a') * 2`, an unknown fact, the wrong arity, a whole expression that is not a yes/no
  question — fails the load rather than evaluating false forever.
- **No** variables, no user-defined functions, no quantifiers, no collections. That boundary is the whole point.

### Fact functions (the only callable surface)

Everything an expression can see. Each reads data the extraction pass already produced — **the evaluator never re-parses
markdown.**

| Function             | Returns | Backed by                                                                                                             |
|----------------------|---------|-----------------------------------------------------------------------------------------------------------------------|
| `field('name')`      | string? | `Doc.FrontScalar`                                                                                                     |
| `present('name')`    | bool    | frontmatter scalar non-empty                                                                                          |
| `section('Title')`   | bool    | `Doc.H2` contains (case-insensitive)                                                                                  |
| `first_section()`    | string  | first `Doc.H2`                                                                                                        |
| `links()`            | int     | `Doc.Links.Count`                                                                                                     |
| `words()`            | int     | every heading and paragraph the document renders; frontmatter and fenced code carry no inline content and so fall out |
| `has_ystatement()`   | bool    | `Doc.YStatement is not null` (Bucket B, not built)                                                                    |
| `ystatement_words()` | int     | word count of `Doc.YStatement` (Bucket B, not built)                                                                  |

Adding a fact is adding one method to `Facts` and one row to `RuleExpr.Functions`, which is what the type checker reads.
The grammar itself never changes.

`Facts` is built per document and discarded once its rules have run, which is what makes `words()`
safe to memoise there rather than on the immutable `Doc`.

## C# design (in `kac.core`)

* **`Facts.cs`** — built per document, exposing exactly the fact functions above and nothing else.
* **`RuleExpr.cs`** — lexer, recursive-descent parser, type checker and evaluator, no dependencies.
  `RuleExpr.Compile(string)` returns an `Expr` or throws `RuleExprException`; `RuleExpr.Eval(expr,
  facts)` answers it for one document.
* **`RuleSpec`** (in `Schema.cs`) carries `Expr`, `Compiled`, `Severity` and `Message`.
  `Schema.ParseRule` compiles at load, so a defective rule stops the load rather than becoming a check that never fires.
  A rule with an `expr:` must also carry a severity and a message — a rule claiming to be finished is held to being able
  to report.
* **`Validator.CheckRules`** evaluates every compiled rule, then falls through to the bespoke arms for the rules whose
  questions need a real algorithm. It emits at the rule's own severity, which is why it is no longer `CheckWarnings`.

## The coverage gate

`kac-tests.cs` reads `kac checks --json` and asserts every id it names has a fixture that exercises it, and that no
golden references an id `kac` no longer emits. `CheckCatalogue.For(schema)` appends each expression rule's
`(id, severity, description)` to the core catalogue, so a rule cannot ship without a fixture any more than a core check
can. `Commands.Checks` takes the repo root for this reason: the catalogue is a property of a corpus's schema, not of the
tool.

**The reader-facing table follows a different rule for each kind.** `Generator.DocRows` groups several core check ids
into one hand-worded row, and `ChecksTableProblems` fails until a new core check has one. An expression rule reports
under its own id and its `description:` is already that row written out, so `ChecksTable` renders it from the schema —
copying it into `DocRows` would be the same sentence in two files, drifting apart at the first edit.

The gate reads ids, not branches: `y-statement-present` was covered by a fixture for its absent block-quote while the
`max-words` arm had none. A rule with more than one way to fail needs a fixture for each.

## When to abandon the in-house evaluator

Keep the grammar at the surface above. The moment a real need appears for variables, function definitions, or
quantifiers, swap the hand-rolled evaluator for **CEL** (Common Expression Language; a .NET port exists) — the `expr:`
strings largely carry over, the engine drops in. Not before: the dependency is not worth it for ~12 predicates.

## What is left

Bucket A is converted. What remains is the fact that unlocks the rest.

1. [ ] **Add a text probe** — `matches(pattern)` over the body, and `section_matches('Title', pattern)` over one
   section. Six rules want exactly this and nothing else: `no-credentials` and `no-actual-data` (a token or a record in
   the prose), `not-normative` (bold RFC 2119 keywords), `blameless` (personal names in named sections),
   `no-hedged-ordering` ("typically" inside Steps) and `posture-belongs-to-frameworks`. It is the best return left — one
   fact for six rules — and it does not touch the grammar, because the pattern is an argument to a fact rather than a
   feature of the language.

       These are heuristics and will be tuned wrong first. That is the argument for holding them as data: a regex in
       `.schema/` is a schema edit, where the same regex in C# is a release every consumer corpus has to take.

2. [x] **`required-when` reads `!=` and `in [...]`.** It split on `==` and understood nothing else, so two of the
   schema's six conditional requirements had never fired: a control naming a mechanism it enforces was not asked for its
   frequency, and personal data was not asked for its retention. `mechanism-has-evidence` and
   `personal-data-has-retention` restate exactly those conditions and stay restatements.

3. [ ] **Add the Bucket-B facts for `y-statement-present`** (`has_ystatement`, `ystatement_words`), which retires the
   largest remaining C# arm along with `RuleSpec.MaxWords`.

4. [x] **What was already enforced is gone.** Nine entries restated a `reciprocal:`, a `mirrors-section:`, a
   `required-when:`, a scalar field type or a required section — and every one of their reasons was already written
   on the type page a reader actually reads. Three more were half-enforced and now describe only the half that is
   not: hedging in `measured-by`, an empty *What went well*, a diagnosis branch ending in neither a resolution nor
   an escalation.

5. [ ] Leave Bucket C in C#. Roughly fifteen rules need git history, a graph, or more than one document; a further eight
   are marked **Scheduled** and are not per-PR validation in any form. Together that is 42% of the declared rules, and
   no expression layer reaches them. Do not extend the grammar to try.

## Non-goals

- A general policy engine. No arbitrary rules-as-data beyond the frozen grammar.
- A date/collection type system.
- Runtime/tenant-specific or externally-contributed rule sets.
- Replacing any Bucket-C check.
