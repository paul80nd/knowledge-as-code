Feature: The parts of a record
A record's parts are the children something else may cite, and each type says where it keeps them. A
policy keeps its in a clause table, so kac checks the table's shape, each row's id and each row's modal.
A glossary keeps its as the headings its terms are written as, where the address is derived and only the
address can be wrong. Both are held to the same two things: no two parts of a record share an address,
and every citation reaches the part it names, written with the separator the corpus uses.

Driven in-process against the broken-parts fixture: the same corpus its JSON golden pins.

Background:
  Given the broken-parts fixture corpus

Scenario: The section must hold a table, headed as the columns are read, with rows in it
  When I validate the corpus
  Then the findings for "policies/nota-clauses-as-bullets.md" are exactly:
    | line | check        | message                                                                                                    |
    |   23 | clause-table | the '## Clauses' section holds no table. Write one row per obligation, headed 'Id \| Clause \| Alignment'. |
  And the findings for "policies/head-wrong-headers.md" are exactly:
    | line | check        | message                                                                                        |
    |   25 | clause-table | the clause table is headed 'Ref \| Obligation'. It must be headed 'Id \| Clause \| Alignment'. |
  And the findings for "policies/empt-no-rows.md" are exactly:
    | line | check        | message                                                                 |
    |   24 | clause-table | the clause table has no rows: a record that binds nothing binds nobody. |

Scenario: An empty clause section is reported by the check that can say what belongs there
  When I validate the corpus
  Then the findings for "policies/blnk-empty-clause-section.md" are exactly:
    | line | check        | message                                                                                                    |
    |   23 | clause-table | the '## Clauses' section holds no table. Write one row per obligation, headed 'Id \| Clause \| Alignment'. |

Scenario: A clause id is a code span, matches the type's pattern, and is used once
  When I validate the corpus
  Then the findings for "policies/span-id-not-code.md" are exactly:
    | line | check            | message                                                    |
    |   27 | clause-id-format | clause id 'CLEAN' is not a code span. Write it as `CLEAN`. |
  And the findings for "policies/case-lower-clause-id.md" are exactly:
    | line | check            | message                                                |
    |   26 | clause-id-format | clause id 'clean' does not match ^[A-Z][A-Z0-9]{1,6}$. |
  And the findings for "policies/dupe-repeated-id.md" are exactly:
    | line | check          | message                                                                              |
    |   27 | part-id-unique | two clauses here address as 'SAME': a citation of it names both and reaches neither. |

Scenario: A clause opens with a modal, and the binding levels are the bold ones
  When I validate the corpus
  Then the findings for "policies/moda-no-modal.md" are exactly:
    | line | check        | message                                                                                                                        |
    |   27 | clause-modal | clause 'We will trigger clause-modal and nothing else' does not open with a modal. Write one of MUST, MUST NOT, SHOULD, COULD. |
  And the findings for "policies/bold-binding-not-bold.md" are exactly:
    | line | check        | message                                  |
    |   26 | clause-modal | 'MUST' binds. Write it bold, `**MUST**`. |

Scenario: Disorder is reported once, against the row that breaks the grouping
  When I validate the corpus
  Then the findings for "policies/ordr-out-of-order.md" are exactly:
    | severity | line | check        | message                                                                                             |
    | warning  |   27 | clause-order | clause 'SECND' is a 'MUST' but follows a 'MUST NOT'. Group the table MUST, MUST NOT, SHOULD, COULD. |

Scenario: A second modal in one row is two obligations sharing an id
  When I validate the corpus
  Then the findings for "policies/cmpd-two-obligations.md" are exactly:
    | severity | line | check           | message                                                                                          |
    | warning  |   27 | clause-compound | clause 'CLEAN' carries a second 'MUST': one obligation per clause, or the citation is ambiguous. |

Scenario: A citation is held to the clause it names, and to the document that carries it
  When I validate the corpus
  Then the findings for "policies/cref-unknown-clause.md" are exactly:
    | line | check    | message                                                                                          |
    |   15 | part-ref | 'pol-CREF.MISSING' cites a clause 'MISSING' that policies/cref-unknown-clause.md does not carry. |
  And the findings for "policies/refs-unknown-document.md" are exactly:
    | line | check    | message                                                |
    |   15 | part-ref | 'pol-ZZZZ.ANY' cites 'pol-ZZZZ', which does not exist. |

Scenario: A part id written beside a link is resolved as a citation
  When I validate the corpus
  Then the findings for "policies/link-clause-after-link.md" are exactly:
    | line | check    | message                                                                                          |
    |   16 | part-ref | 'pol-CREF.MISSING' cites a clause 'MISSING' that policies/cref-unknown-clause.md does not carry. |

Scenario: A citation separated by a colon is told the form to write
  When I validate the corpus
  Then the findings for "policies/coln-colon-separator.md" are exactly:
    | line | check    | message                                                                         |
    |   15 | part-ref | 'pol-COLN:CLEAN' separates the two halves with a colon. Write 'pol-COLN.CLEAN'. |

Scenario: Terms are addressed by the anchor their heading slugs to
  When I validate the corpus
  Then the findings for "glossary/dupe-two-terms-alike.md" are exactly:
    | line | check          | message                                                                                     |
    |   26 | part-id-unique | two terms here address as 'identity-line': a citation of it names both and reaches neither. |
  And the findings for "glossary/tref-unknown-term.md" are exactly:
    | line | check    | message                                                                                                                   |
    |   24 | part-ref | 'gls-dupe-two-terms-alike.no-such-term' cites a term 'no-such-term' that glossary/dupe-two-terms-alike.md does not carry. |

Scenario: A term declared by a heading is held to carrying something under it
  When I validate the corpus
  Then the findings for "glossary/holl-empty-term.md" are exactly:
    | line | check      | message                                                                  |
    |   27 | part-empty | term 'Hollow' has nothing under it. Write it or delete the heading.      |
    |   31 | part-empty | term 'Placeholder' has nothing under it. Write it or delete the heading. |

Scenario: A parts section holding no headings is told what belongs there
  When I validate the corpus
  Then the findings for "glossary/none-terms-section-empty.md" are exactly:
    | line | check     | message                                                                 |
    |   20 | part-none | the '## Terms' section holds no terms. Write each one as an H3 heading. |

Scenario: The whole corpus produces exactly these findings and nothing else
  When I validate the corpus
  Then validation reports 19 documents and 0 skipped
  And the findings are exactly:
    | file                                  | severity | line | check            | message                                                                                                                        |
    | glossary/dupe-two-terms-alike.md      | error    |   26 | part-id-unique   | two terms here address as 'identity-line': a citation of it names both and reaches neither.                                    |
    | glossary/holl-empty-term.md           | error    |   27 | part-empty       | term 'Hollow' has nothing under it. Write it or delete the heading.                                                            |
    | glossary/holl-empty-term.md           | error    |   31 | part-empty       | term 'Placeholder' has nothing under it. Write it or delete the heading.                                                       |
    | glossary/none-terms-section-empty.md  | error    |   20 | part-none        | the '## Terms' section holds no terms. Write each one as an H3 heading.                                                        |
    | glossary/tref-unknown-term.md         | error    |   24 | part-ref         | 'gls-dupe-two-terms-alike.no-such-term' cites a term 'no-such-term' that glossary/dupe-two-terms-alike.md does not carry.      |
    | policies/blnk-empty-clause-section.md | error    |   23 | clause-table     | the '## Clauses' section holds no table. Write one row per obligation, headed 'Id \| Clause \| Alignment'.                     |
    | policies/bold-binding-not-bold.md     | error    |   26 | clause-modal     | 'MUST' binds. Write it bold, `**MUST**`.                                                                                       |
    | policies/case-lower-clause-id.md      | error    |   26 | clause-id-format | clause id 'clean' does not match ^[A-Z][A-Z0-9]{1,6}$.                                                                         |
    | policies/cmpd-two-obligations.md      | warning  |   27 | clause-compound  | clause 'CLEAN' carries a second 'MUST': one obligation per clause, or the citation is ambiguous.                               |
    | policies/coln-colon-separator.md      | error    |   15 | part-ref         | 'pol-COLN:CLEAN' separates the two halves with a colon. Write 'pol-COLN.CLEAN'.                                                |
    | policies/cref-unknown-clause.md       | error    |   15 | part-ref         | 'pol-CREF.MISSING' cites a clause 'MISSING' that policies/cref-unknown-clause.md does not carry.                               |
    | policies/dupe-repeated-id.md          | error    |   27 | part-id-unique   | two clauses here address as 'SAME': a citation of it names both and reaches neither.                                           |
    | policies/empt-no-rows.md              | error    |   24 | clause-table     | the clause table has no rows: a record that binds nothing binds nobody.                                                        |
    | policies/head-wrong-headers.md        | error    |   25 | clause-table     | the clause table is headed 'Ref \| Obligation'. It must be headed 'Id \| Clause \| Alignment'.                                 |
    | policies/link-clause-after-link.md    | error    |   16 | part-ref         | 'pol-CREF.MISSING' cites a clause 'MISSING' that policies/cref-unknown-clause.md does not carry.                               |
    | policies/moda-no-modal.md             | error    |   27 | clause-modal     | clause 'We will trigger clause-modal and nothing else' does not open with a modal. Write one of MUST, MUST NOT, SHOULD, COULD. |
    | policies/nota-clauses-as-bullets.md   | error    |   23 | clause-table     | the '## Clauses' section holds no table. Write one row per obligation, headed 'Id \| Clause \| Alignment'.                     |
    | policies/ordr-out-of-order.md         | warning  |   27 | clause-order     | clause 'SECND' is a 'MUST' but follows a 'MUST NOT'. Group the table MUST, MUST NOT, SHOULD, COULD.                            |
    | policies/refs-unknown-document.md     | error    |   15 | part-ref         | 'pol-ZZZZ.ANY' cites 'pol-ZZZZ', which does not exist.                                                                         |
    | policies/span-id-not-code.md          | error    |   27 | clause-id-format | clause id 'CLEAN' is not a code span. Write it as `CLEAN`.                                                                     |
