# Validation

`kac validate` checks every record in a corpus against the schema its type declares, and `kac checks` lists every
check that run can report. A corpus is one repository of knowledge records kept in git, and a record is one Markdown
document in it. This page lists what validation is built to do, and the scenario proving each behaviour.
[`validate`](../use/cli/validate.md), [`checks`](../use/cli/checks.md) and [Checks](../reference/checks.md) own the
full statement of each one.

## What it does

* It checks the schema before any record, and reports each finding against the schema file that declares it.
  [What the schema is held to](../reference/held-to.md#what-the-pass-reports)
* It passes a corpus with no fault, a record in a sub-folder of its type folder included.
  [`from: sub-path`](../use/discovery.md#from-sub-path)
* It checks each type is both a page and a folder, or neither.
  [The shape on disk](../framework/taxonomy.md#the-shape-on-disk)
* It checks a type's template without counting it as a record.
  [What discovery excludes](../use/discovery.md#what-discovery-excludes)
* It checks every file `generate` writes into still has both markers of each block, and checks its links.
  [Generated block markers](../use/discovery.md#generated-block-markers)
* It checks a record's filename, id, H1, identity line and sections against its type.
  [The three id styles](../framework/metadata.md#the-three-id-styles)
* It refuses a field the type derives from the folder when an author writes it, in a record or a template.
  [A derived field](../reference/held-to.md#a-derived-field)
* It checks each frontmatter value against its field: enum, list, pattern, floor, date and timestamp.
  [A type's own rules](../reference/checks.md#a-types-own-rules)
* It reports frontmatter that does not parse as one finding, and checks nothing else in that record.
  [Which check reports a fault](../reference/checks.md#which-check-reports-a-fault)
* It checks each id is used once, each reference resolves to a record of the type its field names, and each
  reciprocal field agrees. [Faults nothing else would catch](../reference/checks.md#faults-nothing-else-would-catch)
* It checks a field mirroring a section, or the lines carrying a label, agrees with that section or those lines.
  [Referring to an id](../framework/metadata.md#referring-to-an-id)
* It checks the parts a type keeps: a clause table's shape, ids and modals, a term's heading and the words under it,
  and a rule's bullets. [Referring to an id](../framework/metadata.md#referring-to-an-id)
* It holds each citation of a part to the part it names, in a code span, a link, or beside a link.
  [Referring to an id](../framework/metadata.md#referring-to-an-id)
* It reports one fault once, under the check that owns it.
  [Which check reports a fault](../reference/checks.md#which-check-reports-a-fault)
* It warns, without failing, on an unused link definition, a list out of order, a Y-statement outside its bounds and
  a value below its floor. [A run that finds faults](../use/cli/validate.md#a-run-that-finds-faults)
* `kac checks` lists each check with its severity and what it proves, read from the corpus's own `.schema/`, a type
  the corpus declares itself included. [`checks`](../use/cli/checks.md#what-it-does)
* `kac checks` lists a C# rule under the check it emits, and leaves out a rule with no severity.
  [`checks`](../use/cli/checks.md#known-limits)
* `kac checks` opens no record. [`checks`](../use/cli/checks.md#what-it-does)

## What it refuses

* `kac checks` exits `1` where the checks table on a type page disagrees with the catalogue: a check with no row, a
  row for a check kept off the page, or a row for a check the schema does not declare.
  [`checks`](../use/cli/checks.md#known-limits)

## Specified by

[`Schema.feature`][Schema.feature]:

* A schema that promises what the tool cannot deliver is reported against the file that says so

[`Clean.feature`][Clean.feature]:

* The clean baseline validates with no findings
* A record filed in a sub-folder validates, at one level and at two

[`Shape.feature`][Shape.feature]:

* A type is stood up as both a page and a folder, or as neither
* Every file carrying a generated block is checked for its markers, and for its links
* A type's template is checked without being counted as a record

[`Structure.feature`][Structure.feature]:

* A document with no H1 is an error, and its missing Y-statement is a warning
* An id disagreeing with its filename is flagged on the id alone
* An empty section is reported, and the wording follows what the author can do about it
* A mnemonic id is checked for shape, case and agreement with the filename
* A slug id is checked for its alphabet and for agreement with the whole filename
* An identity line is required beneath the H1, and is reported once when malformed
* Each half of the identity line is held to the frontmatter separately
* A status is checked for agreement first, then for being upper-case
* The mnemonic prefix is excluded from the slug-length measurement
* A mis-cased id label is flagged wherever a reader meets it
* A label the style admits is held against the document it leads to
* A field the type derives from the folder is not the author's to write
* A template carrying a derived field is reported, not only a record
* The whole corpus produces exactly these findings and nothing else

[`ValueChecks.feature`][ValueChecks.feature]:

* An out-of-range, capitalised enum trips both enum checks
* A scalar where a list is declared trips the list check
* Unparseable frontmatter short-circuits to a single finding
* A field pattern applies to each list entry, not to the list as a whole
* A list shorter than its declared floor is reported against the field
* A well-formed date that names no day is reported
* A field pattern on a scalar field applies to its value
* An actor field without its prefix is refused
* A timestamp is held to its shape and then to the calendar
* The corpus as a whole produces exactly these findings and nothing else

[`Graph.feature`][Graph.feature]:

* A duplicated id is reported against the later document
* A reference to an id nothing carries is reported once, whether or not the field reciprocates
* A reference that resolves to a document of the wrong type is reported, and reported once
* A reciprocated supersede chain reports nothing at either end
* A field reconciles against the section it names, whichever section that is
* A field reconciles against the lines carrying its label, whichever label that is
* A field naming a part refuses the record named whole
* The whole graph produces exactly these findings and nothing else

[`Parts.feature`][Parts.feature]:

* The section must hold a table, headed as the columns are read, with rows in it
* An empty clause section is reported by the check that can say what belongs there
* A clause id is a code span, matches the type's pattern, and is used once
* A clause opens with a modal, and the binding levels are the bold ones
* Disorder is reported once, against the row that breaks the grouping
* A second modal in one row is two obligations sharing an id
* A citation is held to the clause it names, and to the document that carries it
* A part id written beside a link is resolved as a citation
* A citation separated by a colon is told the form to write
* Terms are addressed by the anchor their heading slugs to
* A term declared by a heading is held to carrying something under it
* Every bullet under a rule heading is held to a modal in bold capitals
* A list written inside a rule's bullet is refused
* A parts section holding no headings is told what belongs there
* The whole corpus produces exactly these findings and nothing else

[`Warnings.feature`][Warnings.feature]:

* An unused link definition and a verdict-less alternative both warn
* A list out of alphabetical order warns once, naming the entry that belongs earlier
* Numbers in a list entry compare as numbers, not as text
* A Y-statement past the ceiling its type declares warns, naming the count and the limit
* A Y-statement short of its moves warns, naming each move that is absent
* A value below its field's floor is reported against each record carrying it
* The corpus as a whole warns eight times and errors not at all

[`Checks.feature`][Checks.feature]:

* The catalogue lists each check with the severity it reports at and what it proves
* A type the corpus declares itself has its checks in the catalogue
* A rule implemented in C# is listed under the check it emits
* A rule with no severity is not in the catalogue
* The catalogue opens no record and reports no fault in one
* A check declared for the type page with no row in the checks table fails the run
* A check kept off the type page that has a row in the checks table fails the run
* A row in the checks table for a check the schema does not declare fails the run

## Unspecified

* Eight `validate` fixtures with no scenario: frontmatter that breaks, `values: $corpus.x` in its three forms, the
  descriptor's targets, expression rules, shortcodes, and type adoption. [Imports](imports.md) specifies `imports`
  and `import-versions`.
  [#247](https://github.com/paul80nd/knowledge-as-code/issues/247)
* Most rules a type declares for itself, and core checks such as `key-order`, are tripped by a golden fixture and named
  in no scenario. `kac checks` lists them, and the golden suite's coverage gate fails a check no fixture trips.
* `validate` exiting `1` exactly when it finds an error. The scenarios call the validator and not the command, so the
  golden suite is where the exit code is asserted.
* How `validate` groups its findings, and the shape of `--json`. Both are printed only.
* How `kac checks` prints the catalogue, as a grid or as `--json`. Both are printed only.

## Where to go next

[Checks](../reference/checks.md) says where each check comes from, and how to add one.

[Checks.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Checks.feature
[Clean.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Clean.feature
[Graph.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Graph.feature
[Parts.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Parts.feature
[Schema.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Schema.feature
[Shape.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Shape.feature
[Structure.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Structure.feature
[ValueChecks.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/ValueChecks.feature
[Warnings.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Warnings.feature
