# SPEC — a small expression layer for `rules:`

Status: **proposal / not started.** Captured for a later session.

## Motivation

Every type schema carries a `rules:` block. Most entries are **prose intent, not executable
checks** — a backlog of ~25 rules described in `description:` fields, of which only a handful are
wired to C#. Today wiring one means adding a hard-coded arm to the `switch (ruleId)` in
`Validator.CheckWarnings` (`kac.core/Validator.cs`). Twelve near-identical conditionals and threshold checks are
waiting behind that switch.

This spec proposes turning the **field-predicate and simple-structural** rules into data: a
one-line `expr:` string per rule, evaluated by a tiny in-house evaluator against a fixed *fact*
context. It deliberately does **not** turn `kac` into a general policy engine (OPA/Rego). The
grammar is frozen small on purpose; rules that need real algorithms stay in C#.

See the conversation that produced this for the full reasoning. The one-line version: OPA/Rego
would replace only the evaluation *tail* of the pipeline while leaving all the markdown/frontmatter
extraction untouched, add a language + runtime dependency, and break the single-file,
no-build-step design. A ~150-line evaluator buys the one property worth having — *new rules as
data* — at a fraction of the cost.

## Scope — what becomes data, what stays code

The decision hinges on bucketing the **actual** rules in `.schema/*.yaml`.

### Bucket A — becomes a one-line `expr:` (do this)

Pure field predicates and simple structural facts. ~half the backlog, one uniform shape.

| Rule | Type | Sketch `expr` |
|------|------|---------------|
| `detected-not-before-occurred` | postmortems | `field('detected-on') >= field('occurred-on')` |
| `deprecated-has-successor` | tools | `field('status') == 'deprecated' implies present('successor')` |
| `personal-data-has-retention` | data | `field('classification') == 'personal' implies present('retention')` |
| `mechanism-has-evidence` (field part) | controls | `field('mechanism') == 'not-enforced' or present('evidence')` |
| `target-is-measurable` (present part) | nfrs | `present('measured-by')` |
| `what-went-well-required` | postmortems | `section('What Went Well')` |
| `symptoms-first` | runbooks | `first_section() == 'Symptoms'` |
| `escalation-required` | runbooks | `section('Escalation')` |
| `hub-not-specification` | capabilities | `words() <= links() * 40` |
| `low-ceremony` | discoveries | `words() <= 200` |
| `carried-in-full-by-digest` (length) | glossary | `words() <= 120` |
| `trial-has-criteria` | tools | `field('status') == 'trial' implies section('Trial Criteria')` |

Thresholds above (40, 200, 120) are placeholders — set them from the prose descriptions when
implementing.

### Bucket B — needs one new *fact*, then it's an `expr:`

Add a derived measurement to `Doc` (extraction pass) and expose it as a `Facts` method; the rule
itself is then a normal expression.

| Rule | New fact(s) needed |
|------|--------------------|
| `y-statement-present` | `has_ystatement()`, `ystatement_words()` (word count of the block-quote after H1) |
| `fallback-required` | a body/section content probe |
| `no-credentials` | `body_matches('<regex>')` — borderline; keep the regex in YAML, the scan in the fact |

Do these lazily — add the fact when you implement the rule, not up front.

### Bucket C — stays C# (never a grammar feature)

Git history, graph analysis, external data, cross-document joins, corpus-wide reporting. If you
ever feel tempted to add loops/joins/quantifiers to the grammar to express one of these, **stop** —
that is the signal you are rebuilding OPA. Write a dedicated arm instead.

- `immutable-after-accepted`, `immutable-after-published` — git diff of committed content
- `no-dependency-cycles` — graph over `depends-on`
- `drift-against-repos` — external repo state
- `policy-has-implementer`, `constraint-consistency` — cross-document
- `coverage-report`, `expiry-sweep`, `undefined-terms` — corpus-wide / reporting
- `reciprocal-supersession`, `related-matches-section` — already core checks; leave as-is

## YAML shape

Expression *strings*, not nested predicate objects — several rules are conditionals
(`A implies B`) and ratios that read cleanly inline but become ugly YAML trees. A rule **fires a
finding when `expr` evaluates false**.

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

- **Types:** string, int, bool. `field(...)` returns string-or-null; comparisons treat null as
  "absent" (any comparison with null is false except `!= ` — decide precise null semantics in the
  parser and lock it with a fixture).
- **Dates** compare correctly as ISO strings lexicographically, so `field('a') >= field('b')`
  works without a date type. Keep it that way; do not add a date type.
- **No** variables, no user-defined functions, no quantifiers, no collections. That boundary is the
  whole point.

### Fact functions (the only callable surface)

Everything an expression can see. Each reads data the extraction pass already produced — **the
evaluator never re-parses markdown.**

| Function | Returns | Backed by |
|----------|---------|-----------|
| `field('name')` | string? | `Doc.FrontScalar` |
| `present('name')` | bool | frontmatter scalar non-empty |
| `section('Title')` | bool | `Doc.H2` contains (case-insensitive) |
| `first_section()` | string | first `Doc.H2` |
| `links()` | int | `Doc.Links.Count` |
| `words()` | int | body word count (cache on `Doc`) |
| `has_ystatement()` | bool | `Doc.YStatement is not null` (Bucket B) |
| `ystatement_words()` | int | word count of `Doc.YStatement` (Bucket B) |

Adding a fact = adding one method to `Facts`. The grammar itself never changes.

## C# design (in `kac.core`)

Two new pieces plus small edits to three existing spots.

1. **`Facts` context** — a class built per doc from `Doc`, exposing exactly the functions above.
   ```csharp
   internal sealed class Facts(Doc d)
   {
       public string? Field(string name) => d.FrontScalar(name);
       public bool Present(string name) => d.FrontScalar(name) is { Length: > 0 };
       public bool Section(string title) =>
           d.H2.Any(h => string.Equals(h, title, StringComparison.OrdinalIgnoreCase));
       public string FirstSection() => d.H2.FirstOrDefault() ?? "";
       public int Links() => d.Links.Count;
       public int Words() => /* cached body word count */ 0;
       // Bucket B facts added lazily…
   }
   ```

2. **Evaluator** — recursive-descent parser + tree walker, ~150 lines, zero new dependencies.
   Parse once at schema load (a bad `expr` is a **build break, fail loudly** — not a silent pass);
   evaluate per doc.
   ```csharp
   internal static class Rule
   {
       public static Expr Compile(string expr);   // throws on syntax error
       public static bool Eval(Expr compiled, Facts facts);
   }
   ```

3. **`FieldSpec`/rule model** — extend the parsed rule with `Expr?`, `Severity`, `Message`, and the
   compiled `Expr`. `Schema.ParseType` already reads `rules:` into `TypeSchema.Rules`; compile the
   `expr` there so syntax errors surface at load.

4. **Dispatcher** — `Validator.CheckWarnings` loop replaces the growing switch for `expr` rules;
   Bucket-C arms remain:
   ```csharp
   foreach (var rule in t.Rules)
   {
       if (rule.Expr is null) continue;              // Bucket C handled below
       if (!Rule.Eval(rule.Compiled, facts))
           emit(rule.Severity, rule.Id, rule.Message, d.FrontStartLine);
   }
   // existing switch arms remain ONLY for git/graph/cross-doc rules
   ```
   Note this loop can emit `error` severity too, so it is no longer "CheckWarnings" only —
   rename to `CheckRules` or fold into `CheckDocument`.

## Integration wrinkle — the coverage gate

`kac-tests.cs` reads `kac checks --json` (the runtime catalogue from `CheckCatalogue.All`) and
asserts every emitted id has a fixture, and that no golden references an id `kac` no longer emits.
Static data-rule ids would break this.

**Fix:** derive the catalogue partly from the schema. At load, append each `expr` rule's
`(id, severity, description-as-summary)` to the catalogue so `kac checks` lists them and the
coverage gate requires a fixture per rule automatically. This *strengthens* the existing "a rule
cannot ship without a fixture" discipline rather than weakening it. It is the one change that
reaches beyond `CheckWarnings`.

Because the catalogue currently is a `static readonly` list, this means making the catalogue
schema-aware (load-time construction). Verify `Commands.Checks` and the `checks-<folder>` generated
table (`Generator.ChecksTable`) still render sensibly once rule ids are included — or keep the
generated table to core checks and list rule ids only in `kac checks`.

## When to abandon the in-house evaluator

Keep the grammar at the surface above. The moment a real need appears for variables, function
definitions, or quantifiers, swap the hand-rolled evaluator for **CEL** (Common Expression
Language; a .NET port exists) — the `expr:` strings largely carry over, the engine drops in. Not
before: the dependency is not worth it for ~12 predicates.

## Implementation checklist

1. [ ] Add `Facts` with Bucket-A functions; cache `Words()` on `Doc`.
2. [ ] Write the evaluator (`Rule.Compile` / `Rule.Eval`) + parser unit fixtures, including null
       semantics for `field()`.
3. [ ] Extend the rule model (`expr`/`severity`/`message`/compiled `Expr`); compile in
       `Schema.ParseType`, failing loudly on syntax errors.
4. [ ] Make `CheckCatalogue` schema-aware so `expr` rule ids appear in `kac checks --json`.
5. [ ] Convert **one** Bucket-A rule (`detected-not-before-occurred` is a clean first) end to end,
       with a golden fixture, and confirm the coverage gate passes.
6. [ ] Convert the rest of Bucket A.
7. [ ] Add Bucket-B facts and convert those rules as needed.
8. [ ] Leave Bucket C in C#. Do not extend the grammar to reach them.

## Non-goals

- A general policy engine. No arbitrary rules-as-data beyond the frozen grammar.
- A date/collection type system.
- Runtime/tenant-specific or externally-contributed rule sets.
- Replacing any Bucket-C check.
