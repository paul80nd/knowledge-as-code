Feature: A clean corpus passes

Scenario: The clean baseline validates with no findings
  Given the clean fixture corpus
  When I validate the corpus
  Then validation reports 5 documents and 0 skipped
  And no findings are reported

Scenario: A record filed in a sub-folder validates, at one level and at two
  Given the clean fixture corpus
  When I validate the corpus
  Then the findings for "policies/security/scrt-nested-policy.md" are exactly:
    | severity | line | check | message |
  And the findings for "standards/common/testing.md" are exactly:
    | severity | line | check | message |
  And the findings for "standards/platform/node/testing.md" are exactly:
    | severity | line | check | message |
