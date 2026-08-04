Feature: Frontmatter value checks
  kac flags documents whose frontmatter values break the schema. Each scenario drives kac.core's
  Validator in-process over a fixture corpus assembled from the real schema — the same corpus the
  broken-values JSON golden pins, expressed here as behaviour.

  Background:
    Given the broken-values fixture corpus

  Scenario: An out-of-range, capitalised enum trips both enum checks
    When I validate the corpus
    Then the findings for "adrs/0001-bad-enum-value.md" are exactly:
      | line | check          | message                                                                           |
      | 3    | enum           | 'status' value 'Draft' is not one of: proposed, accepted, deprecated, superseded. |
      | 3    | enum-lowercase | 'status' enum value 'Draft' must be lowercase.                                    |

  Scenario: A scalar where a list is declared trips the list check
    When I validate the corpus
    Then the findings for "adrs/0002-scalar-list.md" are exactly:
      | line | check | message                         |
      | 6    | list  | 'tags' must be a YAML sequence. |

  Scenario: Unparseable frontmatter short-circuits to a single finding
    When I validate the corpus
    Then the findings for "adrs/0003-unparseable-frontmatter.md" are exactly:
      | line | check              | message                                  |
      |      | frontmatter-parses | frontmatter is not a valid YAML mapping. |

  Scenario: The corpus as a whole produces exactly these findings and nothing else
    When I validate the corpus
    Then validation reports 3 documents and 0 skipped
    And no warnings are reported
    And the findings are exactly:
      | file                                 | line | check              | message                                                                           |
      | adrs/0001-bad-enum-value.md          | 3    | enum               | 'status' value 'Draft' is not one of: proposed, accepted, deprecated, superseded. |
      | adrs/0001-bad-enum-value.md          | 3    | enum-lowercase     | 'status' enum value 'Draft' must be lowercase.                                    |
      | adrs/0002-scalar-list.md             | 6    | list               | 'tags' must be a YAML sequence.                                                   |
      | adrs/0003-unparseable-frontmatter.md |      | frontmatter-parses | frontmatter is not a valid YAML mapping.                                          |
