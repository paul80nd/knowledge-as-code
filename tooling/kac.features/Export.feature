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

Scenario: A draft or a lapsed record travels with its own state
  Given the export fixture corpus
  And the review date of "glossary/search.md" passed on "2026-01-01"
  When I export
  Then it succeeds
  And ".dist/export/glossary/gls-estate.json" states "fields.status" as "draft"
  And ".dist/export/glossary/gls-search.json" states "fields.review-by" as "2026-01-01"

Scenario Outline: export.exclude withholds the records it names
  Given the export fixture corpus
  And the review date of "glossary/search.md" passed on "2026-01-01"
  And the corpus excludes "<value>" from its export
  When I export
  Then it succeeds
  And "<withheld>" does not exist
  And "<kept>" exists

Examples:
  | value   | withheld                                | kept                                          |
  | draft   | .dist/export/glossary/gls-estate.json   | .dist/export/processes/prc-add-a-title.json   |
  | overdue | .dist/export/glossary/gls-search.json   | .dist/export/glossary/gls-estate.json         |

Scenario: A cross-reference adds to seeAlso only where it names a part
  Given the export fixture corpus
  When I export
  Then it succeeds
  And the "gls-estate.record" line of ".dist/export/glossary/terms.jsonl" states "seeAlso" as ["gls-framework.record"]
  And the "gls-framework.record" line of ".dist/export/glossary/terms.jsonl" states "seeAlso" as null

Scenario: A part line carries the link to its part, spelled for the target
  Given the export fixture corpus
  And the corpus publishes to "azure-devops-wiki" at "https://dev.azure.com/acme/Standards/_wiki/wikis/KaC"
  When I export
  Then it succeeds
  And the "gls-search.query" line of ".dist/export/glossary/terms.jsonl" states "links" as {"human":"https://dev.azure.com/acme/Standards/_wiki/wikis/KaC?pagePath=%2Fglossary%2Fsearch&anchor=query"}

Scenario: Each of the three versions is read from its own source
  Given the export fixture corpus
  And the corpus states content-version "2.3.4"
  And the schema declares "glossary" at export version 7
  When I export
  Then it succeeds
  And ".dist/export/manifest.json" states "contentVersion" as "2.3.4"
  And the manifest states the export format this build writes
  And the export states "glossary" at shape version 7
  And the export states "policies" at the shape version its schema declares

Scenario: What the corpus consumes travels under the shortcode of the corpus that wrote it
  Given the export-inherited fixture corpus
  When I export
  Then it succeeds
  And ".dist/export/glossary/eng/gls-shared.json" exists
  And ".dist/export/policies/eng/pol-KEEP.json" exists
  And ".dist/export/glossary/gp/gls-old.json" exists
  And ".dist/export/glossary/terms.jsonl" contains ""record":"eng:gls-shared""
  And nothing outside .dist/ changed

Scenario: export.exclude withholds this corpus's own records alone
  Given the export-inherited fixture corpus
  And ".imports/eng/glossary/gls-shared.json" states "fields.status" as "draft"
  And the corpus excludes "draft" from its export
  When I export
  Then it succeeds
  And ".dist/export/glossary/gls-local.json" does not exist
  And ".dist/export/glossary/eng/gls-shared.json" exists
  And ".dist/export/glossary/gp/gls-old.json" exists

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

Scenario: An export refuses a consumed corpus exporting a section at another fidelity
  Given the export-inherited fixture corpus
  And the restored "eng" export states "glossary" section "Scope" at "summary"
  When I export
  Then it refuses
  And nothing is written

Scenario: An export refuses a consumed corpus exporting a type at another shape
  Given the export-inherited fixture corpus
  And the restored "eng" export states "glossary" at shape version 99
  When I export
  Then it refuses
  And nothing is written

Scenario: An export refuses one corpus arriving twice, at two versions
  Given the export-inherited fixture corpus
  And the corpus also consumes "gp" directly, restored at "0.3.0"
  When I export
  Then it refuses
  And nothing is written
