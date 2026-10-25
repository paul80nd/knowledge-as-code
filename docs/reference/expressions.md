# Rule expressions

A **rule** is a check a type declares about its own records, and most of them are one line of YAML. The line is an
expression, `kac` evaluates it against every record of that type, and the fault it reports carries the rule's own id.

An expression asks a fixed set of questions about one record. It has no variables, no loops and no way to read another
record.

## How a rule is declared

A rule fires when its expression is false. Write the expression as the condition that ought to hold, not as the fault:

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

`description:` is what the rule means, and the generated checks table on the type's page renders it. `message:` is what
an author is told when the rule fires. Do not write the same sentence in both.

A rule with an `expr:` must also declare a **severity**, meaning the level it reports at, and a message. A rule with no
`expr:` keeps its `id` and `description`, declares no severity, and renders as an **intention**.

## The grammar

It is frozen.

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
* **A comparison is not chainable.** The parser refuses `1 < words() < 40`.
* **Dates compare as ISO strings**, so `field('a') >= field('b')` works without a date type.
* **Division by zero gives zero.**
* **No variables, user-defined functions, quantifiers or collections.**

## An absent field

`field(...)` returns either a string or nothing. A comparison where either side is absent is **false**, and `!=` is the
negation of `==`, so `!=` is true. That is one rule for every operator.

So `field('detected-at') >= field('occurred-at')` fires on a record missing a value, which `required-field` already
reports. Guard a rule about a field that may be absent:

```yaml
expr: "present('detected-at') and present('occurred-at') implies field('detected-at') >= field('occurred-at')"
```

## A key inside an object field

Every fact that reads a field takes a dotted path to one key of an object field, as `'last-rehearsed.at'`:

```yaml
expr: "field('last-rehearsed') != 'never' and days_since('last-rehearsed.at') <= 92"
```

A path through a value that is not an object reaches nothing. So on `last-rehearsed: never`,
`present('last-rehearsed.at')` is false and `field('last-rehearsed')` is `'never'`. On an object,
`field('last-rehearsed')` is absent, because the object is not a scalar.

## When an expression is checked

`kac` parses and type-checks each expression before it opens a single record, and anything wrong stops the load and
names the rule. That covers a syntax error, an unknown fact, the wrong number of arguments and a number too large to
hold. It also covers a comparison between a number and text, arithmetic on text, and a whole expression that is not a
yes/no question, such as `words() == 'three'`.

## The facts an expression can ask for

These are the whole callable surface.

| Function                         | Returns | Reads                                                                                                                |
|----------------------------------|---------|----------------------------------------------------------------------------------------------------------------------|
| `field('name')`                  | string? | a frontmatter scalar, or one key of an object field                                                                  |
| `present('name')`                | bool    | whether that field has anything, scalar, list or object. False for a bare key and an empty list as for a missing one |
| `entries('name')`                | int     | how many entries that field carries. `present()` asks whether, this asks how many. Zero where the field is absent    |
| `field_matches('name', 're')`    | bool    | that scalar against a pattern. False where absent, and the one pattern fact that sees frontmatter                    |
| `entries_match('n', 'k', 're')`  | bool    | key `k` of every object under field `n` against a pattern. True where the field is absent or has no such key         |
| `entries_unique('n', 'k')`       | bool    | whether key `k` is written once across the objects under field `n`. True where the field is absent                   |
| `section('Title')`               | bool    | whether an H2 of that name exists (case-insensitive)                                                                 |
| `section_count('Title')`         | int     | how many times it appears. `section()` asks whether, this asks how many                                              |
| `first_section()`                | string  | the first H2, or empty where there is none                                                                           |
| `links()`                        | int     | how many links the body has                                                                                          |
| `words()`                        | int     | every heading and paragraph the record **renders**. Frontmatter and fenced code carry no inline content and fall out |
| `matches('re')`                  | bool    | the body **as written**, code fences, link targets and markdown syntax included. Frontmatter is not read             |
| `section_matches('Title', 're')` | bool    | the same, bounded to one section, and false where the record has no such section                                     |
| `span('a', 'b')`                 | string  | the time between two timestamp fields, as an ISO 8601 duration in hours, minutes and seconds                         |
| `days('a', 'b')`                 | int     | the whole days between two date fields, so a rule can compare a gap against a threshold                              |
| `days_since('a')`                | int     | the whole days from a date field to the day of the run, so a rule can compare a record's age                         |
| `today()`                        | string  | the day the run happens, as an ISO date, so a rule compares it against a date field                                  |

**`span()` returns hours, never days**, so `P1DT2H` is written `PT26H`. It returns the empty string where
either field is absent, is not a moment, or where the second is before the first. Guard on those, which another check
reports.

**`days()` reads date fields, where `span()` reads timestamps.** It returns zero where a field is absent, where a value
is not a date, where the second date is the earlier, and where both are the same day.

**`days_since()` measures one date field against the day of the run.** It answers zero on everything `days()` answers
zero on, and a date in the future is one of them. So a rule comparing it against a threshold passes such a record.

**`today()` returns the day the run happens.** `kac validate` reads that day once, so every record in one run gets
the same day.

**`words()` and `matches()` read different documents.** `words()` counts the rendered text. `matches()` reads the
source, so it finds a credential pasted into a fenced block, and `**MUST**` with its markup.

A question no fact answers yet needs a new fact in `kac`.
[Checks in C#](../maintaining/checks.md#adding-a-fact) says how one is added.

## Where to go next

[What the schema is held to](held-to.md) says what `kac` refuses in the pass that compiles these expressions.
