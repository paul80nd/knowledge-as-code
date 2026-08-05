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

  Scenario: A mnemonic id is checked for shape, case and agreement with the filename
    When I validate the corpus
    Then the findings for "policies/vurm-bad-id-width.md" are exactly:
      | line | check     | message                                                                                               |
      | 1    | id-format | id 'pol-VU' must be 'pol-' followed by 4 upper-case alphanumeric characters beginning with a letter.   |
    And the findings for "policies/scrt-lower-case-id.md" are exactly:
      | line | check     | message                                                                                               |
      | 1    | id-format | id 'pol-scrt' must be 'pol-' followed by 4 upper-case alphanumeric characters beginning with a letter. |
    And the findings for "policies/pipe-id-disagrees.md" are exactly:
      | line | check               | message                                                         |
      | 1    | id-matches-filename | id 'pol-DEVI' mnemonic does not match filename mnemonic 'pipe'. |

  Scenario: A mnemonic H1 is checked against the filename, not just a numbered one
    When I validate the corpus
    Then the findings for "policies/recv-h1-disagrees.md" are exactly:
      | line | check         | message                                                          |
      | 10   | h1-matches-id | H1 mnemonic 'OBSV' does not match filename mnemonic 'recv'.      |

  Scenario: The mnemonic prefix is excluded from the slug-length measurement
    When I validate the corpus
    Then the findings for "policies/mexp-slug-that-is-definitely-way-too-long.md" are exactly:
      | line | check       | message                                                                       |
      |      | slug-length | slug 'slug-that-is-definitely-way-too-long' is 36 characters; the limit is 30. |

  Scenario: A mis-cased id label is flagged where it is read and where it is defined
    When I validate the corpus
    Then the findings for "policies/intc-label-case.md" are exactly:
      | line | check           | message                                                              |
      |      | label-canonical | link definition '[ADR-0004]' should be written as the id 'adr-0004'. |
      |      | label-canonical | link definition '[pol-vurm]' should be written as the id 'pol-VURM'. |
      | 15   | label-canonical | reference '[pol-vurm]' should be written as the id 'pol-VURM'.       |
      | 16   | label-canonical | reference '[ADR-0004]' should be written as the id 'adr-0004'.       |

  Scenario: The whole corpus produces exactly these findings and nothing else
    When I validate the corpus
    Then validation reports 12 documents and 0 skipped
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
      | policies/intc-label-case.md                                 | error    |      | label-canonical     | link definition '[ADR-0004]' should be written as the id 'adr-0004'.                     |
      | policies/intc-label-case.md                                 | error    |      | label-canonical     | link definition '[pol-vurm]' should be written as the id 'pol-VURM'.                     |
      | policies/intc-label-case.md                                 | error    | 15   | label-canonical     | reference '[pol-vurm]' should be written as the id 'pol-VURM'.                           |
      | policies/intc-label-case.md                                 | error    | 16   | label-canonical     | reference '[ADR-0004]' should be written as the id 'adr-0004'.                           |
      | policies/mexp-slug-that-is-definitely-way-too-long.md       | error    |      | slug-length         | slug 'slug-that-is-definitely-way-too-long' is 36 characters; the limit is 30.            |
      | policies/pipe-id-disagrees.md                               | error    | 1    | id-matches-filename | id 'pol-DEVI' mnemonic does not match filename mnemonic 'pipe'.                          |
      | policies/recv-h1-disagrees.md                               | error    | 10   | h1-matches-id       | H1 mnemonic 'OBSV' does not match filename mnemonic 'recv'.                              |
      | policies/scrt-lower-case-id.md                              | error    | 1    | id-format           | id 'pol-scrt' must be 'pol-' followed by 4 upper-case alphanumeric characters beginning with a letter. |
      | policies/vurm-bad-id-width.md                               | error    | 1    | id-format           | id 'pol-VU' must be 'pol-' followed by 4 upper-case alphanumeric characters beginning with a letter.   |
