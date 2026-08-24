Feature: A clean corpus passes

Scenario: The clean baseline validates with no findings
  Given the clean fixture corpus
  When I validate the corpus
  Then validation reports 2 documents and 0 skipped
  And no findings are reported
