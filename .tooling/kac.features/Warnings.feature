Feature: Warning-level checks
  Some rules warn rather than fail. Driven in-process against the warnings fixture.

  Background:
    Given the warnings fixture corpus

  Scenario: An unused link definition and a verdict-less alternative both warn
    When I validate the corpus
    Then the findings for "adrs/0001-warnings.md" are exactly:
      | severity | line | check                | message                                                                                                       |
      | warning  |      | unused-definition    | link definition '[unused-ref]' is never referenced.                                                           |
      | warning  | 28   | alternatives-verdict | Alternatives Considered bullet has no verdict: "A message queue  — we might explore this in a future revi…". |

  Scenario: A list out of alphabetical order warns once, naming the entry that belongs earlier
    When I validate the corpus
    Then the findings for "adrs/0002-list-order.md" are exactly:
      | severity | line | check      | message                                                                               |
      | warning  | 8    | list-order | 'tags' is not in alphabetical order — 'access-control' should come before 'identity'. |

  Scenario: Numbers in a list entry compare as numbers, not as text
    When I validate the corpus
    Then the findings for "policies/natr-natural-order.md" are exactly:
      | severity | line | check | message |
    And the findings for "policies/ordn-ordinal-not-natural.md" are exactly:
      | severity | line | check      | message                                                                                                       |
      | warning  | 7    | list-order | 'aligns-with' is not in alphabetical order — 'ISO27001:2022 A.8.7' should come before 'ISO27001:2022 A.8.29'. |

  Scenario: A Y-statement past the ceiling its type declares warns, naming the count and the limit
    When I validate the corpus
    Then the findings for "adrs/0003-long-y-statement.md" are exactly:
      | severity | line | check       | message                                    |
      | warning  | 13   | y-statement | Y-statement is 95 words; keep it under 60. |

  Scenario: The corpus as a whole warns five times and errors not at all
    When I validate the corpus
    Then validation reports 5 documents and 0 skipped
    And the findings are exactly:
      | file                                 | severity | line | check                | message                                                                                                       |
      | adrs/0001-warnings.md                | warning  |      | unused-definition    | link definition '[unused-ref]' is never referenced.                                                           |
      | adrs/0001-warnings.md                | warning  | 28   | alternatives-verdict | Alternatives Considered bullet has no verdict: "A message queue  — we might explore this in a future revi…". |
      | adrs/0002-list-order.md              | warning  | 8    | list-order           | 'tags' is not in alphabetical order — 'access-control' should come before 'identity'.                         |
      | adrs/0003-long-y-statement.md        | warning  | 13   | y-statement          | Y-statement is 95 words; keep it under 60.                                                                    |
      | policies/ordn-ordinal-not-natural.md | warning  | 7    | list-order           | 'aligns-with' is not in alphabetical order — 'ISO27001:2022 A.8.7' should come before 'ISO27001:2022 A.8.29'. |
