Feature: Warning-level checks
  Some rules warn rather than fail. Driven in-process against the warnings fixture.

  Scenario: An unused link definition and a verdict-less alternative both warn
    Given the warnings fixture corpus
    When I validate the corpus
    Then validation reports 1 document and 0 skipped
    And the findings are exactly:
      | file                  | severity | line | check                | message                                                                                                       |
      | adrs/0001-warnings.md | warning  |      | unused-definition    | link definition '[unused-ref]' is never referenced.                                                           |
      | adrs/0001-warnings.md | warning  | 26   | alternatives-verdict | Alternatives Considered bullet has no verdict: "A message queue  — we might explore this in a future revi…". |
