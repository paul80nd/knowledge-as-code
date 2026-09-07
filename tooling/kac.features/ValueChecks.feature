Feature: Frontmatter value checks
kac flags documents whose frontmatter values break the schema. Each scenario drives kac.core's
Validator in-process over a fixture corpus assembled from the real schema: the same corpus the
broken-values JSON golden pins, expressed here as behaviour.

Background:
  Given the broken-values fixture corpus

Scenario: An out-of-range, capitalised enum trips both enum checks
  When I validate the corpus
  Then the findings for "adrs/0001-bad-enum-value.md" are exactly:
    | line | check          | message                                                                           |
    | 4    | enum           | 'status' value 'Draft' is not one of: proposed, accepted, deprecated, superseded. |
    | 4    | enum-lowercase | 'status' enum value 'Draft' must be lowercase.                                    |

Scenario: A scalar where a list is declared trips the list check
  When I validate the corpus
  Then the findings for "adrs/0002-scalar-list.md" are exactly:
    | line | check | message                         |
    | 7    | list  | 'tags' must be a YAML sequence. |

Scenario: Unparseable frontmatter short-circuits to a single finding
  When I validate the corpus
  Then the findings for "adrs/0003-unparseable-frontmatter.md" are exactly:
    | line | check              | message                                  |
    |      | frontmatter-parses | frontmatter is not a valid YAML mapping. |

Scenario: A field pattern applies to each list entry, not to the list as a whole
  When I validate the corpus
  Then the findings for "adrs/0004-bad-tag-pattern.md" are exactly:
    | line | check         | message                                                         |
    | 9    | field-pattern | 'tags' entry 'Not Lowercase' does not match ^[a-z0-9-]+$.       |
    | 10   | field-pattern | 'tags' entry 'trailing_underscore' does not match ^[a-z0-9-]+$. |

Scenario: A list shorter than its declared floor is reported against the field
  When I validate the corpus
  Then the findings for "faqs/too-few-keywords.md" are exactly:
    | line | check     | message                                                           |
    | 5    | min-items | 'symptom-keywords' has 2 entries: the schema asks for at least 3. |

Scenario: A well-formed date that names no day is reported
  When I validate the corpus
  Then the findings for "adrs/0005-impossible-date.md" are exactly:
    | line | check       | message                                                       |
    | 5    | date-format | 'decided-on' is not a date on the calendar, got '2026-13-40'. |

Scenario: A field pattern on a scalar field applies to its value
  When I validate the corpus
  Then the findings for "tools/bad-licence-pattern.md" are exactly:
    | line | check         | message                                                           |
    | 5    | field-pattern | 'licence' value 'GPL/2.0 †' does not match ^[A-Za-z0-9.\-+ ()]+$. |

Scenario: An actor field without its prefix is refused
  When I validate the corpus
  Then the findings for "adrs/0006-owner-without-a-prefix.md" are exactly:
    | line | check         | message                                                              |
    | 6    | field-pattern | 'owner' value 'alex.doe' does not match ^(human\|role):[a-z0-9.-]+$. |

Scenario: A role is refused where the field records who performed an act
  When I validate the corpus
  Then the findings for "faqs/role-cannot-confirm.md" are exactly:
    | line | check         | message                                                                             |
    | 6    | field-pattern | 'confirmed-by' value 'role:head-of-engineering' does not match ^human:[a-z0-9.-]+$. |

Scenario: The corpus as a whole produces exactly these findings and nothing else
  When I validate the corpus
  Then validation reports 9 documents and 0 skipped
  And no warnings are reported
  And the findings are exactly:
    | file                                 | line | check              | message                                                                             |
    | adrs/0001-bad-enum-value.md          | 4    | enum               | 'status' value 'Draft' is not one of: proposed, accepted, deprecated, superseded.   |
    | adrs/0001-bad-enum-value.md          | 4    | enum-lowercase     | 'status' enum value 'Draft' must be lowercase.                                      |
    | adrs/0002-scalar-list.md             | 7    | list               | 'tags' must be a YAML sequence.                                                     |
    | adrs/0003-unparseable-frontmatter.md |      | frontmatter-parses | frontmatter is not a valid YAML mapping.                                            |
    | adrs/0004-bad-tag-pattern.md         | 9    | field-pattern      | 'tags' entry 'Not Lowercase' does not match ^[a-z0-9-]+$.                           |
    | adrs/0004-bad-tag-pattern.md         | 10   | field-pattern      | 'tags' entry 'trailing_underscore' does not match ^[a-z0-9-]+$.                     |
    | adrs/0005-impossible-date.md         | 5    | date-format        | 'decided-on' is not a date on the calendar, got '2026-13-40'.                       |
    | adrs/0006-owner-without-a-prefix.md  | 6    | field-pattern      | 'owner' value 'alex.doe' does not match ^(human\|role):[a-z0-9.-]+$.                |
    | faqs/role-cannot-confirm.md          | 6    | field-pattern      | 'confirmed-by' value 'role:head-of-engineering' does not match ^human:[a-z0-9.-]+$. |
    | faqs/too-few-keywords.md             | 5    | min-items          | 'symptom-keywords' has 2 entries: the schema asks for at least 3.                   |
    | tools/bad-licence-pattern.md         | 5    | field-pattern      | 'licence' value 'GPL/2.0 †' does not match ^[A-Za-z0-9.\-+ ()]+$.                   |
