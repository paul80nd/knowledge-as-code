Feature: What `report` writes, and what makes it refuse
`report` builds a report over the corpus and what it imports, and `--out` writes it to a file. docs/use/cli/report.md
states each behaviour below. Driven against the report, export-policies and export-inherited fixtures, through
`Commands.Report` with the time and the tool version fixed. A run without `--out` only prints, so every scenario that
reads a report writes it first.

Scenario: A coverage report marks each clause covered or uncovered and leaves the judgement blank
  Given the report fixture corpus
  When I write the "coverage" report to "reports/clause-coverage.md"
  Then it succeeds
  And only "reports/clause-coverage.md" changed
  And "reports/clause-coverage.md" contains "| `pol-COVR` | 2 | 1 | 1 |"
  And "reports/clause-coverage.md" contains "| `WRITTEN` | MUST | `std-COVER` |  |  |  | covered | |"
  And "reports/clause-coverage.md" contains "| `OPEN` | MUST NOT |  |  |  |  | uncovered | |"

Scenario: A frameworks report counts the clauses citing each reference
  Given the export-policies fixture corpus
  When I write the "frameworks" report to "frameworks-report.md"
  Then it succeeds
  And "frameworks-report.md" contains "| ISO 27001:2022 | Obliged | 1 | 0 |"
  And "frameworks-report.md" contains "| A.8.13 | 2 | `pol-BKUP.COPY`, `pol-BKUP.SHARED` | `pol-BKUP` | |"

Scenario: A report's frontmatter states the run that wrote it and leaves the rest to a person
  Given the report fixture corpus
  When I write the "coverage" report to "reports/clause-coverage.md"
  Then the report at "reports/clause-coverage.md" states "generated.at" as "2026-06-15T09:00:00Z"
  And the report at "reports/clause-coverage.md" states "generated.by" as "kac/9.9.9"
  And the report at "reports/clause-coverage.md" states "generated.report" as "coverage"
  And the report at "reports/clause-coverage.md" states "generated.tool" as "kac/9.9.9"
  And the report at "reports/clause-coverage.md" states "status" as "draft"
  And the report at "reports/clause-coverage.md" leaves "id" empty
  And the report at "reports/clause-coverage.md" leaves "owner" empty
  And the report at "reports/clause-coverage.md" leaves "verified" empty
  And the report at "reports/clause-coverage.md" names these sources:
    | resource       | version |
    | report-fixture | 1.2.3   |

Scenario: A corpus that imports policies reports on them, and names this corpus first among its sources
  Given the export-inherited fixture corpus
  When I write the "coverage" report to "coverage-report.md"
  Then it succeeds
  And "coverage-report.md" contains "| `eng:pol-KEEP` | 1 | 0 | 1 |"
  And the report at "coverage-report.md" names these sources:
    | resource            | version |
    | test-inherited      |         |
    | example-engineering | 0.4.1   |

Scenario: Two runs over an unchanged corpus write the same report
  Given the report fixture corpus
  When I write the "coverage" report to "first.md"
  And I write the "coverage" report to "second.md"
  Then the reports at "first.md" and "second.md" are the same

Scenario: A report kept as a record has its id and owner reported until a person fills them in
  Given the report fixture corpus
  When I write the "coverage" report to "reports/clause-coverage.md"
  Then validating the corpus reports the report at "reports/clause-coverage.md" missing "id"
  And validating the corpus reports the report at "reports/clause-coverage.md" missing "owner"

Scenario: A report refuses a path a file already occupies
  Given the report fixture corpus
  And "reports/clause-coverage.md" is replaced with "Verdicts a person wrote."
  When I write the "coverage" report to "reports/clause-coverage.md"
  Then it refuses
  And nothing is written

Scenario: A report name that is not built in is refused
  Given the report fixture corpus
  When I write the "risks" report to "risks-report.md"
  Then it refuses
  And nothing is written

Scenario: A report refuses a consumed corpus that has not been restored
  Given the export-inherited fixture corpus
  And ".imports/eng" is deleted
  When I ask for the "coverage" report
  Then it refuses
  And nothing is written
