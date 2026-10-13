Feature: What `export` writes, and what makes it refuse
`export` writes the corpus into `.dist/export/` as data a consumer reads, and merges in what the corpus consumes.
docs/use/cli/export.md states each behaviour below. Driven against the export and export-inherited fixtures, through
`Commands.Export` with the commit and the time fixed.

Scenario: An export writes a manifest, a file per record and a flat file per type
  Given the export fixture corpus
  When I export
  Then it succeeds
  And ".dist/export/manifest.json" exists
  And ".dist/export/processes/prc-add-a-title.json" exists
  And ".dist/export/glossary/terms.jsonl" exists
  And ".dist/export/policies/clauses.jsonl" exists
  And the export lists the types "glossary, policies, processes"
  And nothing outside .dist/ changed

Scenario: An export of one type writes that type alone
  Given the export fixture corpus
  When I export the "glossary" type
  Then it succeeds
  And the export lists the types "glossary"
  And ".dist/export/policies" does not exist

Scenario: An export of a type the corpus has not adopted is refused
  Given the export fixture corpus
  When I export the "adrs" type
  Then it refuses
  And nothing is written

Scenario: An export rebuilds .dist/export/ whole
  Given the export fixture corpus
  And ".dist/export/orphan.json" is left from an earlier run
  When I export
  Then it succeeds
  And ".dist/export/orphan.json" does not exist

Scenario Outline: The manifest records whether the tree was dirty
  Given the export fixture corpus
  When I export from a tree git reports as <tree>
  Then ".dist/export/manifest.json" states "dirty" as <dirty>

Examples:
  | tree    | dirty |
  | dirty   | true  |
  | clean   | false |
  | unknown | null  |

Scenario: What the corpus consumes travels under the shortcode of the corpus that wrote it
  Given the export-inherited fixture corpus
  When I export
  Then it succeeds
  And ".dist/export/glossary/eng/gls-shared.json" exists
  And ".dist/export/policies/eng/pol-KEEP.json" exists
  And ".dist/export/glossary/gp/gls-old.json" exists
  And ".dist/export/glossary/terms.jsonl" contains ""record":"eng:gls-shared""
  And nothing outside .dist/ changed

Scenario: An export refuses a consumed corpus that has not been restored
  Given the export-inherited fixture corpus
  And ".imports/eng" is deleted
  When I export
  Then it refuses
  And nothing is written

Scenario: An export refuses a consumed corpus at a format it cannot read
  Given the export-inherited fixture corpus
  And ".imports/eng/manifest.json" states "formatVersion" as 99
  When I export
  Then it refuses
  And nothing is written

Scenario: An export refuses a consumed corpus exporting a type at another shape
  Given the export-inherited fixture corpus
  And the restored "eng" export states "glossary" at shape version 99
  When I export
  Then it refuses
  And nothing is written
