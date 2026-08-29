Feature: Document structure checks
kac checks the filename, id, H1, identity line and sections of each document. Driven
in-process against the broken-structure fixture: the same corpus its JSON golden pins.

Background:
  Given the broken-structure fixture corpus

Scenario: A document with no H1 is an error, and its missing Y-statement is a warning
  When I validate the corpus
  Then the findings for "adrs/0005-no-h1.md" are exactly:
    | severity | line | check       | message                                    |
    | error    |    1 | h1          | document has no H1.                        |
    | warning  |    0 | y-statement | no Y-statement block-quote follows the H1. |

Scenario: An id disagreeing with its filename is flagged on the id alone
  When I validate the corpus
  Then the findings for "adrs/0004-missing-consequences.md" are exactly:
    | line | check               | message                                                     |
    |      | required-section    | missing required section '## Consequences'.                 |
    | 1    | id-matches-filename | id 'adr-0009' number does not match filename number '0004'. |

Scenario: An empty section is reported, and the wording follows what the author can do about it
  When I validate the corpus
  Then the findings for "adrs/0011-headings-with-no-body.md" are exactly:
    | line | check         | message                                                                    |
    |   32 | empty-section | required section '## Consequences' has nothing under it.                   |
    |   34 | empty-section | section '## Related' has nothing under it. Write it or delete the heading. |

Scenario: A mnemonic id is checked for shape, case and agreement with the filename
  When I validate the corpus
  Then the findings for "policies/vurm-bad-id-width.md" are exactly:
    | line | check     | message                                                                                              |
    |    1 | id-format | id 'pol-VU' must be 'pol-' followed by 4 upper-case alphanumeric characters beginning with a letter. |
  And the findings for "policies/scrt-lower-case-id.md" are exactly:
    | line | check     | message                                                                                                |
    |    1 | id-format | id 'pol-scrt' must be 'pol-' followed by 4 upper-case alphanumeric characters beginning with a letter. |
  And the findings for "policies/pipe-id-disagrees.md" are exactly:
    | line | check               | message                                                         |
    |    1 | id-matches-filename | id 'pol-DEVI' mnemonic does not match filename mnemonic 'pipe'. |

Scenario: A slug id is checked for its alphabet and for agreement with the whole filename
  When I validate the corpus
  Then the findings for "tools/site-server.md" are exactly:
    | line | check     | message                                                                                 |
    |    1 | id-format | id 'tol-Site_Server' must be 'tol-' followed by lower-case letters, digits and hyphens. |
  And the findings for "tools/id-disagrees.md" are exactly:
    | line | check               | message                                                                       |
    |    1 | id-matches-filename | id 'tol-names-another-tool' slug does not match filename slug 'id-disagrees'. |

Scenario: An identity line is required beneath the H1, and is reported once when malformed
  When I validate the corpus
  Then the findings for "policies/obsv-no-identity-line.md" are exactly:
    | line | check    | message                                                          |
    |   9 | identity | no identity line follows the H1. Add `Policy: pol-OBSV` `DRAFT`. |
  And the findings for "policies/agnt-identity-malformed.md" are exactly:
    | line | check    | message                                                             |
    |   11 | identity | identity line is malformed. Write it as `Policy: pol-AGNT` `DRAFT`. |

Scenario: Each half of the identity line is held to the frontmatter separately
  When I validate the corpus
  Then the findings for "policies/trus-identity-type.md" are exactly:
    | line | check         | message                                              |
    |   11 | identity-type | identity line says 'Standard', but this is a Policy. |
  And the findings for "policies/recv-identity-id.md" are exactly:
    | line | check       | message                                                                  |
    |   11 | identity-id | identity line id 'pol-OBSV' does not match the document's id 'pol-RECV'. |

Scenario: A status is checked for agreement first, then for being upper-case
  When I validate the corpus
  Then the findings for "policies/envs-identity-status.md" are exactly:
    | line | check           | message                                                                     |
    |   11 | identity-status | identity line status 'ACTIVE' does not match the document's status 'draft'. |
  And the findings for "policies/know-identity-case.md" are exactly:
    | line | check           | message                                                   |
    |   11 | identity-status | identity line status 'Draft' must be upper-case: `DRAFT`. |

Scenario: The mnemonic prefix is excluded from the slug-length measurement
  When I validate the corpus
  Then the findings for "policies/mexp-slug-that-is-definitely-way-too-long.md" are exactly:
    | line | check       | message                                                                        |
    |      | slug-length | slug 'slug-that-is-definitely-way-too-long' is 36 characters; the limit is 30. |

Scenario: A mis-cased id label is flagged where it is read and where it is defined
  When I validate the corpus
  Then the findings for "policies/intc-label-case.md" are exactly:
    | line | check           | message                                                              |
    |      | label-canonical | link definition '[ADR-0004]' should be written as the id 'adr-0004'. |
    |      | label-canonical | link definition '[pol-vurm]' should be written as the id 'pol-VURM'. |
    | 16   | label-canonical | reference '[pol-vurm]' should be written as the id 'pol-VURM'.       |
    | 17   | label-canonical | reference '[ADR-0004]' should be written as the id 'adr-0004'.       |

Scenario: A field the type derives from the folder is not the author's to write
  When I validate the corpus
  Then the findings for "policies/cats-category-written.md" are exactly:
    | line | check       | message                                                                                                                                                     |
    | 1    | derived-key | 'category' is derived from the record's sub-path and is not written in frontmatter. Delete the line, and file the record in the folder you want it to name. |

Scenario: The whole corpus produces exactly these findings and nothing else
  When I validate the corpus
  Then validation reports 23 documents and 0 skipped
  And the findings are exactly:
    | file                                                        | severity | line | check               | message                                                                                                |
    | policies/cats-category-written.md                            | error    | 1    | derived-key         | 'category' is derived from the record's sub-path and is not written in frontmatter. Delete the line, and file the record in the folder you want it to name. |
    | adrs/0003-slug-that-is-definitely-way-too-long-for-limit.md | error    |      | slug-length         | slug 'slug-that-is-definitely-way-too-long-for-limit' is 46 characters; the limit is 30.               |
    | adrs/0004-missing-consequences.md                           | error    |      | required-section    | missing required section '## Consequences'.                                                            |
    | adrs/0004-missing-consequences.md                           | error    | 1    | id-matches-filename | id 'adr-0009' number does not match filename number '0004'.                                            |
    | adrs/0005-no-h1.md                                          | warning  | 0    | y-statement         | no Y-statement block-quote follows the H1.                                                             |
    | adrs/0005-no-h1.md                                          | error    | 1    | h1                  | document has no H1.                                                                                    |
    | adrs/0006-bad-id-prefix.md                                  | error    | 1    | id-prefix           | id 'xyz-0006' must start with 'adr-'.                                                                  |
    | adrs/0007-bad-id-width.md                                   | error    | 1    | id-format           | id 'adr-7' must be 'adr-' followed by 4 digits.                                                        |
    | adrs/0008-Bad_Name.md                                       | error    |      | filename-pattern    | filename '0008-Bad_Name.md' does not match ^\d{4}-[a-z0-9-]+\.md$.                                     |
    | adrs/0010-half-filled-copy.md                               | error    | 13   | placeholder-left    | '{{the pressure to get something committed}}' is a placeholder the template left for you to fill in.   |
    | adrs/0011-headings-with-no-body.md                          | error    | 32   | empty-section       | required section '## Consequences' has nothing under it.                                               |
    | adrs/0011-headings-with-no-body.md                          | error    | 34   | empty-section       | section '## Related' has nothing under it. Write it or delete the heading.                             |
    | policies/agnt-identity-malformed.md                         | error    | 11   | identity            | identity line is malformed. Write it as `Policy: pol-AGNT` `DRAFT`.                                    |
    | policies/dirs-directory-link.md                             | error    | 16   | link-resolves       | link target '/media' does not resolve.                                                                 |
    | policies/envs-identity-status.md                            | error    | 11   | identity-status     | identity line status 'ACTIVE' does not match the document's status 'draft'.                            |
    | policies/intc-label-case.md                                 | error    |      | label-canonical     | link definition '[ADR-0004]' should be written as the id 'adr-0004'.                                   |
    | policies/intc-label-case.md                                 | error    |      | label-canonical     | link definition '[pol-vurm]' should be written as the id 'pol-VURM'.                                   |
    | policies/intc-label-case.md                                 | error    | 16   | label-canonical     | reference '[pol-vurm]' should be written as the id 'pol-VURM'.                                         |
    | policies/intc-label-case.md                                 | error    | 17   | label-canonical     | reference '[ADR-0004]' should be written as the id 'adr-0004'.                                         |
    | policies/know-identity-case.md                              | error    | 11   | identity-status     | identity line status 'Draft' must be upper-case: `DRAFT`.                                              |
    | policies/mexp-slug-that-is-definitely-way-too-long.md       | error    |      | slug-length         | slug 'slug-that-is-definitely-way-too-long' is 36 characters; the limit is 30.                         |
    | policies/obsv-no-identity-line.md                           | error    | 9   | identity            | no identity line follows the H1. Add `Policy: pol-OBSV` `DRAFT`.                                       |
    | policies/pipe-id-disagrees.md                               | error    | 1    | id-matches-filename | id 'pol-DEVI' mnemonic does not match filename mnemonic 'pipe'.                                        |
    | policies/recv-identity-id.md                                | error    | 11   | identity-id         | identity line id 'pol-OBSV' does not match the document's id 'pol-RECV'.                               |
    | policies/scrt-lower-case-id.md                              | error    | 1    | id-format           | id 'pol-scrt' must be 'pol-' followed by 4 upper-case alphanumeric characters beginning with a letter. |
    | policies/trus-identity-type.md                              | error    | 11   | identity-type       | identity line says 'Standard', but this is a Policy.                                                   |
    | policies/vurm-bad-id-width.md                               | error    | 1    | id-format           | id 'pol-VU' must be 'pol-' followed by 4 upper-case alphanumeric characters beginning with a letter.   |
    | tools/id-disagrees.md                                       | error    | 1    | id-matches-filename | id 'tol-names-another-tool' slug does not match filename slug 'id-disagrees'.                          |
    | tools/site-server.md                                        | error    | 1    | id-format           | id 'tol-Site_Server' must be 'tol-' followed by lower-case letters, digits and hyphens.                |
