Feature: Document structure checks
  kac checks the filename, id, H1 and required sections of each document. Driven in-process against
  the broken-structure fixture — the same corpus its JSON golden pins.

  Background:
    Given the broken-structure fixture corpus

  Scenario: A document with no H1 is an error, and its missing Y-statement is a warning
    When I validate the corpus
    Then the findings for "adrs/0005-no-h1.md" are exactly:
      | severity | line | check       | message                                    |
      | error    | 1    | h1          | document has no H1.                        |
      | warning  | 0    | y-statement | no Y-statement block-quote follows the H1. |

  Scenario: An id whose number disagrees with the filename is flagged on both id and H1
    When I validate the corpus
    Then the findings for "adrs/0004-missing-consequences.md" are exactly:
      | line | check               | message                                                     |
      |      | required-section    | missing required section '## Consequences'.                 |
      | 1    | id-matches-filename | id 'adr-0009' number does not match filename number '0004'. |
      | 9    | h1-matches-id       | H1 number '0009' does not match filename number '0004'.     |

  Scenario: The whole corpus produces exactly these findings and nothing else
    When I validate the corpus
    Then validation reports 6 documents and 0 skipped
    And the findings are exactly:
      | file                                                        | severity | line | check               | message                                                                                 |
      | adrs/0003-slug-that-is-definitely-way-too-long-for-limit.md | error    |      | slug-length         | slug 'slug-that-is-definitely-way-too-long-for-limit' is 46 characters; the limit is 30. |
      | adrs/0003-slug-that-is-definitely-way-too-long-for-limit.md | error    | 9    | h1-pattern          | H1 'A title with no ADR prefix' does not match ^ADR-(\d{4}): (.+)$.                       |
      | adrs/0004-missing-consequences.md                           | error    |      | required-section    | missing required section '## Consequences'.                                              |
      | adrs/0004-missing-consequences.md                           | error    | 1    | id-matches-filename | id 'adr-0009' number does not match filename number '0004'.                              |
      | adrs/0004-missing-consequences.md                           | error    | 9    | h1-matches-id       | H1 number '0009' does not match filename number '0004'.                                  |
      | adrs/0005-no-h1.md                                          | warning  | 0    | y-statement         | no Y-statement block-quote follows the H1.                                               |
      | adrs/0005-no-h1.md                                          | error    | 1    | h1                  | document has no H1.                                                                       |
      | adrs/0006-bad-id-prefix.md                                  | error    | 1    | id-prefix           | id 'xyz-0006' must start with 'adr-'.                                                     |
      | adrs/0007-bad-id-width.md                                   | error    | 1    | id-format           | id 'adr-7' must be 'adr-' followed by 4 digits.                                          |
      | adrs/0008-Bad_Name.md                                       | error    |      | filename-pattern    | filename '0008-Bad_Name.md' does not match ^\d{4}-[a-z0-9-]+\.md$.                        |
