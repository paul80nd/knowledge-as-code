# Rule expressions

A **rule** is a check a type declares about its own records, and most of them are one line of YAML. The line is an
expression, `kac` evaluates it against every record of that type, and the fault it reports carries the rule's own id.
Adding a check is adding YAML rather than editing the tool.

An expression asks a small, fixed set of questions about one record. It has no variables, no loops and no way to reach
another record. That boundary is the point: a rule that stays this small can be read by whoever trips it.

## A rule fires when its expression is false

Write the expression as the condition that ought to hold, not as the fault:

```yaml
rules:
  - id: symptoms-first
    description: Symptoms is the first section after the H1. That is how the reader finds the document.
    severity: error
    expr: "first_section() == 'Symptoms'"
    message: >
      Symptoms must be the first section. Someone reaching for this at 2am matches on what they are
      seeing, not on what the document is called.
```

`description:` is what the rule means, and it is rendered into the generated checks table on the type's page. `message:`
is what an author is told when the rule fires. One is a definition and the other is a diagnosis, so do not write the
same sentence twice.

A rule carrying an `expr:` must also declare a **severity**, meaning the level it reports at, and a message. A rule that
claims to be finished is held to being able to report.

A rule with no `expr:` keeps its `id` and `description`, declares no severity, and renders as an **intention**: a
behaviour written down that no code answers to yet.

## The grammar is frozen

Extend it only on a deliberate decision, and never for convenience.

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

* **Three types:** string, int and bool. There are no boolean literals, so every condition starts from something the
  record says.
* **Strings are single-quoted**, and a doubled quote is one quote, as in YAML and SQL. There are no backslash escapes.
  The strings that most need a quote in them are regular expressions, and a second layer of escaping over those is how
  they stop being readable.
* **A comparison is not chainable.** `1 < words() < 40` is a sentence rather than a condition, and the parser declines
  it instead of choosing an associativity nobody asked for.
* **Dates compare as ISO strings**, so `field('a') >= field('b')` works without a date type.
* **Division by zero yields zero.** Nothing in the default types divides, and the operator exists because the grammar is
  frozen. A rule that trips over it reads as a threshold nobody meets.
* **No variables, user-defined functions, quantifiers or collections.**

An expression is written as a string. Several rules are conditionals or ratios, and those read cleanly on one line and
badly as a tree of nested YAML objects.

## An absent field makes a comparison false

`field(...)` returns either a string or nothing. A comparison where either side is absent is **false**, and `!=` is the
negation of `==`, so `!=` is true. That is one rule for every operator.

The consequence is an idiom. `field('detected-on') >= field('occurred-on')` fires on a record missing a date, where
`required-field` has already said so in better words. So a rule about a field that may be absent guards it:

```yaml
expr: "present('detected-on') and present('occurred-on') implies field('detected-on') >= field('occurred-on')"
```

This is why a written rule runs longer than a first sketch of it. The alternative is each operator guessing which way
silence should fall, which trades one visible guard for a table of special cases nobody remembers.

## Every expression is checked when the schema loads

`kac` parses and type-checks each expression before it opens a single record, and anything wrong stops the load naming
the rule. That covers a syntax error, an unknown fact and the wrong number of arguments. It also covers a comparison
between a number and text, arithmetic on text, and a whole expression that is not a yes/no question.

This matters more than it looks. Without it, `words() == 'three'` compiles and then evaluates false for the life of the
schema. That is a check which appears wired up and never fires, the exact failure this layer exists to end.

## The facts an expression can ask for

These are the whole callable surface. Each reads what the parse pass already produced, so the evaluator never re-parses
markdown.

| Function                         | Returns | Reads                                                                                                                |
|----------------------------------|---------|----------------------------------------------------------------------------------------------------------------------|
| `field('name')`                  | string? | a frontmatter scalar                                                                                                 |
| `present('name')`                | bool    | whether that field carries anything, scalar or list. False for a bare key and an empty list as for a missing one     |
| `field_matches('name', 're')`    | bool    | that scalar against a pattern. False where absent, and the one pattern fact that sees frontmatter                    |
| `section('Title')`               | bool    | whether an H2 of that name exists (case-insensitive)                                                                 |
| `section_count('Title')`         | int     | how many times it appears. `section()` asks whether, this asks how many                                              |
| `first_section()`                | string  | the first H2, or empty where there is none                                                                           |
| `links()`                        | int     | how many links the body carries                                                                                      |
| `words()`                        | int     | every heading and paragraph the record **renders**. Frontmatter and fenced code carry no inline content and fall out |
| `matches('re')`                  | bool    | the body **as written**, code fences, link targets and markdown syntax included. Frontmatter is not read             |
| `section_matches('Title', 're')` | bool    | the same, bounded to one section, and false where the record holds no such section                                   |

**`words()` and `matches()` deliberately see different documents.** One walks the rendered text and the other the
source. That is what lets `matches()` find a credential pasted into a fenced block, the case those rules exist for. It
also finds `**MUST**`, an obligation the rendered text would have flattened into an ordinary word.

## Reach for a fact, not for the grammar

Adding a fact means one method on the tool's `Facts` class, one row in its function table, and one row in the table
above. A test holds this page's table against that function table, so the page and the tool cannot come apart quietly.
The grammar never changes.

`section_count()` and `field_matches()` each serve a single rule today, and each answers a question the next corpus will
ask again. That is the usual shape of a new fact, and the reason reaching for the grammar instead is almost always the
wrong move.

A question that genuinely needs loops, joins or quantifiers is a rule written in C#, and
[`tooling/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/README.md) is where that is done.

## Where to go next

[What the schema is held to](held-to.md) says what `kac` refuses in the pass that compiles these expressions.
