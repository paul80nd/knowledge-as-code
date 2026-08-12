Feature: Clause table checks
  A policy binds in its clause table and nowhere else, so kac checks the table's shape, each row's id and
  each row's modal, and holds every citation of a clause to a clause that exists. Driven in-process against
  the broken-clauses fixture — the same corpus its JSON golden pins.

  Background:
    Given the broken-clauses fixture corpus

  Scenario: The section must hold a table, headed as the columns are read, with rows in it
    When I validate the corpus
    Then the findings for "policies/nota-clauses-as-bullets.md" are exactly:
      | line | check       | message                                                                                  |
      | 24   | clause-table | the '## Clauses' section holds no table — write one row per obligation, headed 'Id \| Clause \| Alignment'. |
    And the findings for "policies/head-wrong-headers.md" are exactly:
      | line | check       | message                                                                     |
      | 26   | clause-table | the clause table is headed 'Ref \| Obligation' — it must be headed 'Id \| Clause \| Alignment'. |
    And the findings for "policies/empt-no-rows.md" are exactly:
      | line | check       | message                                                            |
      | 25   | clause-table | the clause table has no rows — a policy that binds nothing binds nobody. |

  Scenario: An empty clause section is reported by the check that can say what belongs there
    When I validate the corpus
    Then the findings for "policies/blnk-empty-clause-section.md" are exactly:
      | line | check        | message                                                                                  |
      | 24   | clause-table | the '## Clauses' section holds no table — write one row per obligation, headed 'Id \| Clause \| Alignment'. |

  Scenario: A clause id is a code span, matches the type's pattern, and is used once
    When I validate the corpus
    Then the findings for "policies/span-id-not-code.md" are exactly:
      | line | check           | message                                              |
      | 28   | clause-id-format | clause id 'CLEAN' is not a code span — write it as `CLEAN`. |
    And the findings for "policies/case-lower-clause-id.md" are exactly:
      | line | check           | message                                             |
      | 27   | clause-id-format | clause id 'clean' does not match ^[A-Z][A-Z0-9]{1,6}$. |
    And the findings for "policies/dupe-repeated-id.md" are exactly:
      | line | check            | message                                                          |
      | 28   | clause-id-unique | clause id 'SAME' is used twice — a citation of it names two obligations. |

  Scenario: A clause opens with a modal, and the binding levels are the bold ones
    When I validate the corpus
    Then the findings for "policies/moda-no-modal.md" are exactly:
      | line | check        | message                                                                                            |
      | 28   | clause-modal | clause 'We will trigger clause-modal and nothing else' does not open with a modal — write one of MUST, MUST NOT, SHOULD, COULD. |
    And the findings for "policies/bold-binding-not-bold.md" are exactly:
      | line | check        | message                              |
      | 27   | clause-modal | 'MUST' binds — write it bold, `**MUST**`. |

  Scenario: Disorder is reported once, against the row that breaks the grouping
    When I validate the corpus
    Then the findings for "policies/ordr-out-of-order.md" are exactly:
      | severity | line | check        | message                                                                              |
      | warning  | 28   | clause-order | clause 'SECND' is a 'MUST' but follows a 'MUST NOT' — group the table MUST, MUST NOT, SHOULD, COULD. |

  Scenario: A second modal in one row is two obligations sharing an id
    When I validate the corpus
    Then the findings for "policies/cmpd-two-obligations.md" are exactly:
      | severity | line | check           | message                                                                             |
      | warning  | 28   | clause-compound | clause 'CLEAN' carries a second 'MUST' — one obligation per clause, or the citation is ambiguous. |

  Scenario: A citation is held to the clause it names, and to the document that carries it
    When I validate the corpus
    Then the findings for "policies/cref-unknown-clause.md" are exactly:
      | line | check      | message                                                                                                |
      | 16   | clause-ref | 'pol-CREF.MISSING' cites a clause 'MISSING' that policies/cref-unknown-clause.md does not carry.        |
    And the findings for "policies/refs-unknown-document.md" are exactly:
      | line | check      | message                                        |
      | 16   | clause-ref | 'pol-ZZZZ.ANY' cites 'pol-ZZZZ', which does not exist. |

  Scenario: The whole corpus produces exactly these findings and nothing else
    When I validate the corpus
    Then validation reports 13 documents and 0 skipped
    And the findings are exactly:
      | file                                       | severity | line | check            | message                                                                                            |
      | policies/blnk-empty-clause-section.md      | error    | 24   | clause-table     | the '## Clauses' section holds no table — write one row per obligation, headed 'Id \| Clause \| Alignment'. |
      | policies/bold-binding-not-bold.md          | error    | 27   | clause-modal     | 'MUST' binds — write it bold, `**MUST**`.                                                          |
      | policies/case-lower-clause-id.md           | error    | 27   | clause-id-format | clause id 'clean' does not match ^[A-Z][A-Z0-9]{1,6}$.                                             |
      | policies/cmpd-two-obligations.md           | warning  | 28   | clause-compound  | clause 'CLEAN' carries a second 'MUST' — one obligation per clause, or the citation is ambiguous.  |
      | policies/cref-unknown-clause.md            | error    | 16   | clause-ref       | 'pol-CREF.MISSING' cites a clause 'MISSING' that policies/cref-unknown-clause.md does not carry.   |
      | policies/dupe-repeated-id.md               | error    | 28   | clause-id-unique | clause id 'SAME' is used twice — a citation of it names two obligations.                           |
      | policies/empt-no-rows.md                   | error    | 25   | clause-table     | the clause table has no rows — a policy that binds nothing binds nobody.                           |
      | policies/head-wrong-headers.md             | error    | 26   | clause-table     | the clause table is headed 'Ref \| Obligation' — it must be headed 'Id \| Clause \| Alignment'.                 |
      | policies/moda-no-modal.md                  | error    | 28   | clause-modal     | clause 'We will trigger clause-modal and nothing else' does not open with a modal — write one of MUST, MUST NOT, SHOULD, COULD. |
      | policies/nota-clauses-as-bullets.md        | error    | 24   | clause-table     | the '## Clauses' section holds no table — write one row per obligation, headed 'Id \| Clause \| Alignment'.     |
      | policies/ordr-out-of-order.md              | warning  | 28   | clause-order     | clause 'SECND' is a 'MUST' but follows a 'MUST NOT' — group the table MUST, MUST NOT, SHOULD, COULD. |
      | policies/refs-unknown-document.md          | error    | 16   | clause-ref       | 'pol-ZZZZ.ANY' cites 'pol-ZZZZ', which does not exist.                                             |
      | policies/span-id-not-code.md               | error    | 28   | clause-id-format | clause id 'CLEAN' is not a code span — write it as `CLEAN`.                                        |
