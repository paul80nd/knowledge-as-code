Feature: Cross-document graph checks
  kac reconciles ids across the whole wiki: uniqueness, referenced ids, reciprocal fields, and
  related/section mirroring. Driven in-process against the graph fixture.

  Background:
    Given the graph fixture corpus

  Scenario: A duplicated id is reported against the later document
    When I validate the corpus
    Then the findings for "adrs/0003-third.md" are exactly:
      | line | check               | message                                                     |
      | 1    | id-matches-filename | id 'adr-0002' number does not match filename number '0003'. |
      | 1    | id-unique           | id 'adr-0002' is also used by adrs/0002-second.md.          |

  Scenario: A reference to an id nothing carries is reported once, whether or not the field reciprocates
    When I validate the corpus
    Then the findings for "adrs/0004-dangling.md" are exactly:
      | line | check        | message                                                    |
      | 1    | ref-resolves | 'superseded-by' points at 'adr-0099', which does not exist. |

  Scenario: The whole graph produces exactly these findings and nothing else
    When I validate the corpus
    Then validation reports 4 documents and 0 skipped
    And the findings are exactly:
      | file                  | severity | line | check                   | message                                                                                             |
      | adrs/0001-first.md    | error    | 1    | related-matches-section | 'related' lists 'adr-0002' but it is not referenced in the '## Related' section.                    |
      | adrs/0001-first.md    | error    | 1    | related-matches-section | 'related' lists 'adr-0099' but it is not referenced in the '## Related' section.                    |
      | adrs/0001-first.md    | error    | 1    | ref-resolves            | 'related' points at 'adr-0099', which does not exist.                                               |
      | adrs/0001-first.md    | error    | 1    | reciprocal              | 'supersedes: adr-0002' is not reciprocated — adrs/0002-second.md must list 'superseded-by: adr-0001'. |
      | adrs/0001-first.md    | error    | 30   | link-resolves           | link target 'nonexistent-target.md' does not resolve.                                               |
      | adrs/0001-first.md    | error    | 30   | undefined-label         | reference '[ADR-0099]' has no link definition.                                                      |
      | adrs/0001-first.md    | warning  | 31   | bracket-literal         | '[an unlinked placeholder]' looks like a reference but has no definition (or use an inline link).   |
      | adrs/0003-third.md    | error    | 1    | id-matches-filename     | id 'adr-0002' number does not match filename number '0003'.                                         |
      | adrs/0003-third.md    | error    | 1    | id-unique               | id 'adr-0002' is also used by adrs/0002-second.md.                                                  |
      | adrs/0004-dangling.md | error    | 1    | ref-resolves            | 'superseded-by' points at 'adr-0099', which does not exist.                                         |
