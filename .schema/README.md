# Schema

Machine-readable definitions of the frontmatter every knowledge type carries. These files are the **single source** for
three things:

1. **Validation** — what CI checks a document's frontmatter against.
2. **The `## Metadata` block** generated into each `<type>.md`.
3. **The `## What CI checks` block** generated into each `<type>.md`.

Plus the columns and sort order for `<type>/_index.md`.

Edit the schema, regenerate, review the diff. Never hand-edit anything inside a `BEGIN GENERATED` marker.

**`<type>/_template.md` is not generated.** A template is written and kept in step by hand, and it is also excluded from
validation, so nothing ties one to its schema in either direction. Changing a type's fields or required sections means
opening its template and making the same change there — assume that, rather than that a regeneration will catch it.

## Files

| File              | Contents                                      |
|-------------------|-----------------------------------------------|
| `_universal.yaml` | Fields every document in the taxonomy carries |
| `_enums.yaml`     | Enums shared by more than one type            |
| `_tiers.yaml`     | What each tier is called, and how it behaves  |
| `<folder>.yaml`   | One per knowledge type, named for its folder  |

Type files are named for the **folder**, not the type — `adrs.yaml`, `services.yaml`, `data.yaml`. CI infers a
document's type from its folder, so folder → schema is an identity lookup with no singularisation step.

A tier is declared twice, deliberately. `_universal.yaml` gives the `tier` field its range, and every record is
validated against it. `_tiers.yaml` says what each of those values is called, how a document of it behaves, and — where
there is one — the thing worth saying before the types beneath it are listed. Neither is derivable from the other, so
the two are reconciled when the schema loads: a value one knows and the other does not is a record that can carry a tier
no page can name, or a heading no document will ever sit under. Order is load-bearing in `_tiers.yaml` — it is the order
every generated list of types is grouped by.

None of them carries a version stamp. Answering "which version of the schema is this corpus on" takes something that
reconciles the answer against an upstream, and a number nothing compares is a number a corpus can be wrong about
silently — so the stamp and its reader arrive together, or not at all. Tracked in
[knowledge-as-code#16](https://github.com/paul80nd/knowledge-as-code/issues/16).

## Field specification

```yaml
fields:
  <name>:
    required: true|false        # default false
    required-when: '<other> == <value>' | '<other> != <value>' | '<other> in [a, b]'
    type: string|date|enum|id|list|bool|int
    of: id|string               # element type, when type is list
    values: [ ... ]             # when type is enum, or an $enums.<name> reference
    ref: <folder> | [ ... ]     # when type is id or list-of-id: which type(s) the id may belong to
    reciprocal: <field>         # the field on the target that must point back
    mirrors-section: <H2>       # a section of the record whose ids this field must agree with
    pattern: '<regex>'          # additional constraint
    allow-literal: [ ... ]      # words admitted in place of a value of the declared type
    min-items: <n>              # when type is list: the floor on its length
    min-records: <n>            # when type is list: the floor on how many records carry each entry
    description: >              # one line, rendered into the generated Metadata table
    notes: >                    # the longer why; schema-only, and the fallback when there is no description
```

**Every key in these files is one the loader reads, and `notes:` is the one exception.** A key nothing reads is a
declaration in a file documented as the contract the tool enforces, so it is reported at load like any other — see
[What the schema is held to](#what-the-schema-is-held-to). `notes:` is admitted at every level and parsed only here on a
field, which is how something worth saying and not worth acting on gets said: these files are read by people who cannot
ask what a key was for, and a schema that could not explain itself would be worse than one with a loose vocabulary.
Everything else is implemented, dropped, or rewritten as a note.

`required-when` takes those three forms and no others — a condition the loader cannot read is reported against the field
that declares it rather than reading as one that never holds. It tests one other field of the same document; a condition
needing more than that is a rule with an `expr:`, not a field declaration. Where the field it names is absent the
condition does not hold, `!=` included: `required-field` is already reporting that absence, and requiring a second field
on top would report one omission as two.

`values:` is read from an `enum` and nowhere else, and `ref:` names one folder or several. Both are checked when the
schema loads — a `values:` list on a `type: list` field and a `ref:` at a folder no schema covers are each reported
naming the file and the key, because a vocabulary or a target nothing applies is a promise to whoever takes a copy of
these files. See [What the schema is held to](#what-the-schema-is-held-to). Every id the field then carries is resolved
against the corpus as `ref-resolves`, whether or not the field also declares a `reciprocal:` — the one-directional edges
are the ones no counterpart holds in step, so they are the ones a check has to hold. Both halves of the declaration are
asked: that the id names a document, and that the document is of a type the `ref:` names. An id of the wrong type lands
on a real page and so reads as deliberate to whoever follows it, and this line is the only place the type it should have
named is written down.

Between them the `ref:` declarations *are* the graph, and the taxonomy renders them as one: a diagram of how the types
relate, and a table of the field behind each edge. Nothing else declares an edge, so a relationship written only as
prose is one nothing can check, and it appears in neither.

`mirrors-section` names an H2 the type declares — any of them, and a type may mirror two fields against two sections —
and holds the ids in the field against the ids the section links to, in a bullet or in prose alike, in both directions
and case-insensitively. It is for a field carried in frontmatter and repeated in the body, where the two drift apart
quietly: `related` against `## Related` on an ADR is the case in this corpus. A name the type's `sections:` block does
not offer is reported when the schema loads, since it would reconcile against a heading no record may carry.

Both directions is the part to weigh before declaring one. A section that mentions an id the field does not carry is a
finding as much as the reverse, so the field has to be the whole truth about what the section names — a prose aside
about something deliberately *not* in the field will fail. `services` is the case in point and is why `depends-on` does
not mirror `## Dependencies`: `svc-search` names the two services whose events it consumes, and the whole point of that
paragraph is that neither is an edge.

`allow-literal` admits a word beside the field's declared type — `applies-to: [all]` on a list of service ids,
`last-rehearsed: "never"` on a date. A listed value is taken as written and nothing further is asked of it; on a list it
exempts the entry rather than the field, so the ids beside it are still ids. It exists so that a field with one honest
answer outside its type does not have to widen into a string and give up every check on the values it usually carries.
`min-items` is the floor on a list's length, read only from a `type: list` field, for the field whose value is its
breadth: a FAQ's `symptom-keywords` is the one the schema tells authors to over-fill, and nothing else holds it to more
than a single entry.

`min-records` is the other floor, and counts the opposite way: how many records of the type carry each entry, rather
than how many entries one record carries. It is what a field says when its values are there to divide the type into
groups — `internal` earns its place by naming several services, where a value carried by one record divides nothing and
belongs in a field that is free to be unique. The count is per type, case-insensitive, and once per record however often
one record repeats a value; the finding is a **warning** reported against each record carrying the short value; and the
floor is a number rather than a flag because an estate large enough will want more than two. Membership is never
declared: the corpus decides what its vocabulary is, and the schema says only that a value in this field is meant to be
shared.

`description` and `notes` answer different questions. `description` is what a reader of the type page needs at a glance
and is what the Metadata table renders; `notes` is the reasoning, which belongs here in the schema where there is room
for it. A field declaring only `notes` still renders them, so the two can be adopted a schema at a time — but where a
note has grown past a line, that is the signal it wants a `description` beside it rather than a trim.

**Keep a `description` under ~100 characters.** The generated table pads every column to its widest cell, so one long
description widens every row on the page — a 153-character cell once made all ten ADR rows 190 wide. Enum `values` are
not part of that budget: they render in a small table of their own beneath it rather than inside the cell, so declaring
a sixth value costs nothing in the width of the main table.

**Conventions the validator enforces globally**

* Dates are quoted strings in `YYYY-MM-DD` form, naming a day the calendar has.
* An absent value is a **bare key** (`decided-on:`) — never `null`, `~`, `""`, `—` or `TBD`.
* Enum values are lowercase and hyphenated.
* Unknown keys fail, except the Azure DevOps reserved keys listed in `_universal.yaml` under `reserved`.

## Type specification

Beyond `fields`, each type file declares:

| Key                        | Purpose                                                                                                          |
|----------------------------|------------------------------------------------------------------------------------------------------------------|
| `type` / `folder` / `page` | Identity, and where the type lives — see the note below                                                          |
| `label` / `label-plural`   | The display names — "Policy" heads the generated index, "Policies" names the collection in a link                |
| `tier` / `lifecycle`       | Fixed for the type; `tier` is written into frontmatter as a reader-facing trust signal, and CI checks it matches |
| `summary` / `goes-here`    | What the type is, and what a contributor has in hand when it is the answer — see the note below                  |
| `detail`                   | The paragraph beneath the one-liner, rendered into the taxonomy's own list of types                              |
| `versus`                   | How this type differs from another that is easily confused with it — see the note below                          |
| `lineage`                  | The type's prior art, what the framework took from it, and where it parts company                                |
| `collision`                | Where the type's name already means something else, and what a reader will get wrong                             |
| `id`                       | Prefix, style and width — see the note below on which styles the validator acts on                               |
| `filename`                 | Pattern and slug length limit                                                                                    |
| `sections`                 | Required and optional H2s — the required ones for presence, and either kind for holding something                |
| `clauses`                  | The clause table's section, id pattern and modals, where a type states its obligations as addressable rows       |
| `index`                    | Columns, sort columns and direction for the generated index — see the note below                                 |
| `rules`                    | Type-level behaviours — see the note below on which of them run                                                  |

**`folder`.** A type is a folder of records, a page describing them, and a template to copy. `folder:` names the first
of those and is required. The check reads the value rather than the key, because an absent `folder:` and a deliberate
`folder: null` are the same string once parsed. A type that lost the key reads exactly like one that never had it.

**`summary` and `goes-here`.** The two lines a type says about itself, and the reason a corpus's pages can describe the
corpus rather than the framework's full range. `summary` is what the type holds — "the rulebook, imperative, RFC 2119"
— and heads the type's row in the repository's own index. `goes-here` is the same type from the other side, phrased as
what the contributor is holding — "a rule people must follow when building" — and is the row in the taxonomy's decision
table. Both are required, both are rendered as table cells, and both are held to the same length limit as a rule's
`description`. The fuller account of a type, with its examples and its edges, stays on `<type>.md`.

**`detail`** is the paragraph the other two are too short to be: what the type carries beyond its first sentence, and
the edge a reader is most likely to walk over. It is rendered as prose rather than into a table, so it is not held to
the cell bound — but it is held to being *the framework's* account of the type. Anything local, any example from the
estate, belongs on `<type>.md`, which is the corpus's to write and never reconciled.

**`versus`** is the one thing a type says about another type rather than about itself: a mapping from another type's
folder to the paragraph separating the two. It becomes the taxonomy's disambiguation list.

```yaml
versus:
  standards: >
    The ADR is the decision and its reasoning, frozen. The standard is the rule that results, kept current.
```

A pair is written **once**, by the type its heading is titled from — `versus: standards` on `adrs.yaml` renders as
"ADR vs Standard". Which side that is is a judgement rather than something the tool could derive, so the tool holds the
two sides against each other instead: a pair both sides declare is two accounts of one distinction with nothing keeping
them in step, and fails. So does a pair against a folder no schema covers, or against the declaring type itself.

**`lineage`** records where the type's name came from — `prior-art`, and the `alignment` and `divergence` beside it. It
is the framework's own intellectual debt, identical wherever this schema is taken. A corpus's *standing* against a
framework is the other thing entirely: that belongs wholly to the corpus, and `frameworks.md` records it alone.

Only `prior-art` is required, and "none" is one of its answers. Some types have no useful ancestor, and claiming one is
worse than admitting none. What was taken and where it diverges are questions such a type cannot answer, so leaving both
empty is a settled state and renders as an em dash.

**`collision`** is for a type whose name a reader arrives already holding — `control` means the safeguard itself in
every governance framework, `capability` sits below an epic in SAFe and above one here. Say what the word means
elsewhere and what the reader will therefore get wrong. Most types collide with nothing and leave it out; inventing a
collision to fill the key spends a warning a reader would otherwise trust.

Paragraphs are separated by a blank line, and the generator wraps each on its own.

Write the links in `lineage` and `collision` **inline**. The block either renders into cannot see the reference
definitions at the foot of the page it lands on, and a label whose definition is deleted renders as literal brackets
rather than as a failure. A URL is never broken across lines whatever the margin, here or anywhere: folding one puts a
space in the middle of it.

Only the types a corpus has adopted are rendered, so a decision table never offers a route to a type whose page is not
there to open. A disambiguation needs both of its types by the same rule: a corpus with no controls is not helped by
being told how a standard differs from one.

**`label-plural` is required where `label` is not**, because only one of the two can be derived. A missing `label`
falls back to the type name capitalised; nothing turns `nfr` into "NFRs" or `glossary` into "Glossary", and appending an
`s` is right for some types and wrong for the rest. The plural is what a generated line uses when it points at the
type's page rather than at one of its records — "it goes in **Policies**".

**`tier`** must be one `_tiers.yaml` declares. It is what every validation rule, review expectation and language rule
keys off, and it is written into the frontmatter of every record of the type, so a tier neither file knows is a word the
corpus carries and nothing means anything by.

**`index`.** `sort:` is one column or several — `sort: [severity, id]` sorts on the first and breaks ties with the
next — and a type declaring none is sorted by `id`, the one column every document carries. `order:` is `ascending`
(the default) or `descending`, and applies to the sort as a whole rather than to one column of it; a type wanting one
column each way is asking two questions with one key. A postmortem index is the case for `descending`: the incident
someone is looking for is almost always the most recent.

**`id.style`.** Three styles are dispatched: `numbered`, `slug` and `mnemonic`, and a fourth name fails when the schema
loads. Each is a prefix and a discriminator — four digits, a lower-case slug, a fixed-width upper-case mnemonic —
checked for shape and then for agreement with the same discriminator in the filename, which is what keeps a record's id
and its path naming the same document.

**`rules`.** A rule declaring an `expr:` runs. It is evaluated against every document of its type, reports under its own
id, is listed by `kac checks`, and renders its own row into the generated `## What CI checks` block from its
`description:` — so adding one is adding YAML rather than editing the tool. See [Rule expressions](#rule-expressions)
below for what one may say.

**A rule's `description` is capped at 120 characters, and the cap is enforced.** It says what is checked, in a table a
reader scans to find the row they tripped; the reasoning belongs in the rule's `message:`, which is what the author who
trips it actually reads, or in a `#` comment for an intention that has no message. A description doing both jobs is how
every one of these grew to two or three sentences, and `schema-shape` now says so when the schema loads. An intention's
description is bound too — it renders in *Declared, not yet enforced* on the same page.

Four ids keep a hand-written class instead, because what they ask needs more than the grammar can say:
`y-statement-present` and `alternatives-have-verdicts` on the decision-record type, `terms-are-alphabetical` on the
glossary, and `no-dependency-cycles` on the service type, whose question is about the records together rather than about
any one of them. Every remaining id is a statement of intent: a behaviour someone wants, written down, that no code
answers to yet. **An intention declares no `severity:`**, which is what tells the tool it is one; the type page renders
them beneath the checks table under *Declared, not yet enforced*, so a reader can see both what a build will say about
their document and what has been written down and not built. A rule naming a severity that nothing dispatches is the one
arrangement that reads as enforced from every angle and is not, and it fails when the schema loads.

Not every statement of intent is waiting for an expression, and counting them as though they were makes the ruleset look
less finished than it is. Ten will never be an expression. Eight are not validator work in any form: seven say
**Scheduled** in their own descriptions — periodic reports over a whole corpus, several needing external state — and
`kac` has no execution model for them, while `coverage-report` is a generator and belongs with `kac index`. The other
two are not rules about a document at all: `blameless` needs a list of personal names, since no regular expression tells
`Alex Doe` from `Root Cause`, and `human-confirmed` is a `pattern:` on a field.

Reciprocity and section mirroring are declared on the **field**, not here: `reciprocal:` and `mirrors-section:` drive
them. So does a conditional requirement, through `required-when:`. A `rules:` entry restating any of those has no
effect — an entry that duplicates a declaration reads as a second, weaker source for the same obligation.

## Rule expressions

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
  able to report, and one that cannot is a schema error rather than a check that never fires.
* A rule without an `expr:` keeps `id` + `description`, declares no severity, and is rendered as an intention. Where a
  rule class answers to its id, the rule declares a severity like any other that runs.

Expression *strings*, not nested predicate objects: several rules are conditionals (`A implies B`) or ratios, and those
read cleanly inline but become ugly YAML trees.

### Grammar

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
  the strings that most need a quote in them are regular expressions, and a second escaping layer over those is how they
  stop being readable.
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

The consequence is an idiom. `field('detected-on') >= field('occurred-on')` fires on a postmortem missing a date — where
`required-field` has already said so, in better words. A rule about a field that may be absent guards it:

```yaml
expr: "present('detected-on') and present('occurred-on') implies field('detected-on') >= field('occurred-on')"
```

This is why written rules read longer than a naive sketch of them. The alternative — each operator guessing which way
silence should fall — trades one explicit guard for a table of special cases nobody remembers.

### Compile-time checking

An expression is parsed **and type-checked** at load, and anything wrong stops the load naming the rule. That covers a
syntax error, an unknown fact, the wrong number of arguments, a comparison between a number and text, arithmetic on
text, and a whole expression that is not a yes/no question.

This matters more than it looks. Without it, `words() == 'three'` compiles and then evaluates false for the life of the
schema — a check that appears wired up and never fires, which is the exact failure this layer exists to end.

### Fact functions — the only callable surface

Everything an expression can see. Each reads what the parse pass already produced, so the evaluator never re-parses
markdown.

| Function                         | Returns | Reads                                                                                                                   |
|----------------------------------|---------|-------------------------------------------------------------------------------------------------------------------------|
| `field('name')`                  | string? | a frontmatter scalar                                                                                                    |
| `present('name')`                | bool    | that scalar, non-empty — false for a bare key as well as a missing one                                                  |
| `field_matches('name', 're')`    | bool    | that scalar against a pattern — false where absent; the one pattern fact that sees frontmatter                          |
| `section('Title')`               | bool    | whether an H2 of that name exists (case-insensitive)                                                                    |
| `section_count('Title')`         | int     | how many times it appears — `section()` asks whether, this asks how many                                                |
| `first_section()`                | string  | the first H2, or empty where there is none                                                                              |
| `links()`                        | int     | how many links the body carries                                                                                         |
| `words()`                        | int     | every heading and paragraph the document **renders** — frontmatter and fenced code carry no inline content and fall out |
| `matches('re')`                  | bool    | the body **as written** — code fences, link targets and markdown syntax included; frontmatter is not                    |
| `section_matches('Title', 're')` | bool    | the same, bounded to one section; false where the document holds no such section                                        |

**`words()` and `matches()` deliberately see different documents.** One walks the rendered text, the other the source.
That is what lets `matches` find a credential pasted into a fenced block — the case those rules exist for — and find
`**MUST**`, an obligation the rendered text would have flattened into an ordinary word. Do not simplify one onto the
other; a unit test pins the difference.

Adding a fact is adding one method to `Facts` and one row to `RuleExpr.Functions`, which is what the type checker reads.
The grammar never changes. `section_count()` and `field_matches()` were each written for a single rule and each turned
out to answer a question the next corpus will ask again, which is the usual shape of a new fact — and the reason
reaching for the grammar instead is almost always the wrong move.

## What the schema is held to

These files are read by people who cannot ask what a key was meant to do — a corpus that took the framework rather than
wrote it holds every one of them. So a declaration the tool does nothing with is not harmlessly inert: `rules:` reads as
behaviour the validator applies, and a `ref:` reads as a target being checked. Before any document is validated, the
schema is held against what the tool can act on, and each finding names the file and the key.

| Reported                                                                               | Check                |
|----------------------------------------------------------------------------------------|----------------------|
| A key at any level the loader never reads, `notes:` excepted                           | `schema-unknown-key` |
| An `expr:` that will not compile, or that names no `severity:` or `message:`           | `schema-unreadable`  |
| A `required-when:` outside its three forms                                             | `schema-unreadable`  |
| `values: $enums.x` where `_enums.yaml` declares no `x`                                 | `schema-unreadable`  |
| A rule claiming a `severity:` that neither an `expr:` nor a rule class answers         | `schema-dispatch`    |
| A `ref:` entry naming a folder no schema covers                                        | `schema-dispatch`    |
| A `versus:` entry naming a folder no schema covers                                     | `schema-dispatch`    |
| `values:` on any field that is not an `enum`                                           | `schema-dispatch`    |
| `min-items:` or `min-records:` on any field that is not a `list`                       | `schema-dispatch`    |
| An `index.order:` that is neither `ascending` nor `descending`                         | `schema-dispatch`    |
| A `tier:` no `_tiers.yaml` declares, or a tier only one of the two files knows         | `schema-shape`       |
| An `id.style` with no code behind the value                                            | `schema-dispatch`    |
| A type declaring no `folder:`                                                          | `schema-shape`       |
| A `mirrors-section:` at a section the type's `sections:` block does not declare        | `schema-shape`       |
| A missing `label-plural:`, `summary:`, `goes-here:`, `detail:` or `lineage.prior-art:` | `schema-shape`       |
| A `versus:` against the declaring type itself, or one both sides declare               | `schema-shape`       |

**The question is whether code acts on the value, not whether the key is spelled correctly.** `style: mnemonic` is a
real style and would pass a spelling test; what makes it sound is the branch that reads it. Each vocabulary above is
therefore read from the code that dispatches it, so adding a name without a branch beneath is the mistake this pass
exists to prevent.

The `schema-shape` rows ask something else. There the tool acts on whatever the value says — any section is reconciled,
any folder is read, any sentence is rendered — and what makes it sound is a second declaration in the same file, or the
shape of the page the value lands on: the `sections:` block beside a `mirrors-section:`, the width of the table cell a
`summary:` becomes.

The key vocabulary is read the same way, and there is no list of permitted keys anywhere: the loader records what it
asks each mapping for, and whatever is left over is reported. So a key gains its meaning and its admission in the same
edit, and a key that stops being read stops being admitted without anyone having to remember.

A `ref:` at a type the corpus never adopted is reported for the same reason as one that is misspelled: whether the
folder was deleted here or never existed upstream, the field claims a target nothing can resolve. Re-adopt the type file
or drop the ref — the two are the same decision about what this corpus holds.

Aspiration is not the thing being removed; silence is. An intention keeps its `description:`, drops its `severity:`, and
is rendered on the type page as declared-but-not-enforced.

## Open questions

* **`standards.yaml` `axis` values are unresolved** — four different formulations exist across the corpus. The schema
  currently carries the `standards.md` version with a `TODO` note. Settle it before generating.
* **ID styles** are assigned per type. Numbered where documents accrete in sequence and the number is useful in
  navigation; slug where the thing has a natural stable name; mnemonic where a small, heavily-cited set benefits from an
  id that says something. Worth a review pass — the split is a convention, not a derivation.
